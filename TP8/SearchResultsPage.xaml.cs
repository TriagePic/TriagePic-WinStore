using TP8.Data;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Popups;

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace TP8
{
#if POSSIBLY_BUT_TRY_APP_GLOBAL_FIRST
    /// <summary>
    /// Helper class to transmit current selection between this page and flip view
    /// </summary>
    public class navigationParamGroupAndUniqueID
    {
        public string GroupName;
        public string UniqueId; // mass casualty ID
    }
#endif
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage : TP8.Common.LayoutAwarePage
    {
        //private string _query;  Use App.CurrentSerachQuery instead
        private Filter _selectedGroupFilter = null; // Glenn adds, to help flyout

        public SearchResultsPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            //Ignore App.CurrentDisaster.EventName;
            App.SearchResultsEventTitleTextBasedOnCurrentFilterProfile = eventText.Text = GetEventTextBasedOnCurrentFilterProfile();
            // exclude Org from App.SearchResultsEventTitleTextBasedOnCurrentFilterProfile, to ease interrogating string elsewhere
            if (App.CurrentFilterProfile.ReportedAtMyOrgOnly)
                eventText.Text += ", " + App.CurrentOrgContactInfo.OrgAbbrOrShortName;
            else
                eventText.Text += ", Any Org";
            App.CurrentSearchQuery = navigationParameter as String;
            //App.PatientDataGroups.ReFilter(); // filter or query has changed.  This will affect contents of groups fetched below.
            App.PatientDataGroups.ReSortAndFilter();
            SampleDataSource.RefreshOutboxAndAllStationsItems(); // Propagate here
            var gOut = SampleDataSource.GetGroup("Outbox");
            var gAll = SampleDataSource.GetGroup("AllStations");

            var query = App.CurrentSearchQuery == null ? string.Empty : App.CurrentSearchQuery.ToLower();

            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            if (String.IsNullOrEmpty(App.CurrentSearchResultsGroupName)) // Glenn adds this, maybe can add to data model at some point
                App.CurrentSearchResultsGroupName = "Outbox";

            var filterList = new List<Filter>();
            if (App.CurrentSearchResultsGroupName == "Outbox")
            {
                filterList.Add(new Filter("📮  Outbox", GetSearchResultList(gOut, query).Count, true));
                filterList.Add(new Filter("👪  All Stations", GetSearchResultList(gAll, query).Count, false)); //AllStations was FullSearch earlier
            }
            else
            {
                // Keep the filter order unchanged, so UI doesn't swap them.
                filterList.Add(new Filter("📮  Outbox", GetSearchResultList(gOut, query).Count, false));
                filterList.Add(new Filter("👪  All Stations", GetSearchResultList(gAll, query).Count, true));
            }

            /* Compare with SplitPage:
            var group = SampleDataSource.GetGroup((String)navigationParameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items; */
 
            var group = SampleDataSource.GetGroup(App.CurrentSearchResultsGroupName);
            this.DefaultViewModel["Group"] = group;

            /* Maybe not:
            this.DefaultViewModel["Items"] = group.Items; */

            string filteron = "";
            if(App.CurrentFilterProfile.HasFlyoutFilteringOtherThanEvent()) //was: if(App.PatientDataGroups.HasFlyoutFiltering())
                filteron = "Filtered";
            otherFilterText.Text = filteron;

            // Communicate results through the view model
            string sortdesc = App.PatientDataGroups.GetShortSortDescription();
            string visualState = DetermineVisualState(ApplicationView.Value);
            if (visualState == "FullScreenPortrait" || visualState == "Snapped") // might not be enough, may need VisualStateChanged handler
            {
                sortdesc = App.PatientDataGroups.GetVeryShortSortDescription();
            }
            this.DefaultViewModel["QueryText"] = '\u201c' + App.CurrentSearchQuery + '\u201d' + sortdesc;
            this.DefaultViewModel["Filters"] = filterList;  // These are the group names
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1;
/* orig template:
            var queryText = navigationParameter as String;

            // TODO: Application-specific searching logic.  The search process is responsible for
            //       creating a list of user-selectable result categories:
            //
            //       filterList.Add(new Filter("<filter name>", <result count>));
            //
            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("All", 0, true));

            // Communicate results through the view model
            this.DefaultViewModel["QueryText"] = '\u201c' + queryText + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
            this.DefaultViewModel["ShowFilters"] = filterList.Count > 1; */
        }

        private string GetEventTextBasedOnCurrentFilterProfile()
        {
            if (App.CurrentFilterProfile.FlyoutEventComboBoxChoice == "Current event (recommended)")
                return App.CurrentDisaster.EventName;

            if (!App.CurrentFilterProfile.HasFlyoutFiltering())
                return "All Events";

            if(!String.IsNullOrEmpty(App.CurrentFilterProfile.FlyoutEventComboBoxChoice) && App.CurrentFilterProfile.FlyoutEventComboBoxChoice != "All events")
                return App.CurrentFilterProfile.FlyoutEventComboBoxChoice; // Specific event

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Events";

            if( !App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic)
                    return "No Events"; // Pathological cases

            if( !App.CurrentFilterProfile.DisasterEventIncludeReal &&
                !App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "No Events"; // Pathological cases

            if( !App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Public Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Private Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                App.CurrentFilterProfile.DisasterEventIncludePublic &&
                !App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Test Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                !App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Real Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic &&
                !App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Private Test Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                !App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Private Real Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic &&
                !App.CurrentFilterProfile.DisasterEventIncludeReal &&
                App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Private Test Events";

            if( !App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                !App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Public Real Events";

            if( App.CurrentFilterProfile.DisasterEventIncludePrivate &&
                !App.CurrentFilterProfile.DisasterEventIncludePublic &&
                App.CurrentFilterProfile.DisasterEventIncludeReal &&
                !App.CurrentFilterProfile.DisasterEventIncludeTest)
                    return "All Public Test Events";

            return "";
        }

        /// <summary>
        /// Invoked when a group filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected group filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Determine what filter was selected
            _selectedGroupFilter = e.AddedItems.FirstOrDefault() as Filter;
            GroupOrFlyoutFilterChanged();
        }

        public void GroupOrFlyoutFilterChanged()
        {
            var query = App.CurrentSearchQuery.ToLower();
            var selectedFilter = _selectedGroupFilter; // convenience alias for template sample code.
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding group Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                // Below, we respond to the change in active group filter by setting this.DefaultViewModel["Results"]
                // to a collection of items with bindable Image, Title, Subtitle, and Description properties
                SampleDataGroup g = null;
                switch(selectedFilter.Name)
                {
                    case "👪  All Stations":
                        g = SampleDataSource.GetGroup("AllStations");
                        App.CurrentSearchResultsGroupName = "AllStations";
                        break;
                    case "📮  Outbox":
                        g = SampleDataSource.GetGroup("Outbox");
                        App.CurrentSearchResultsGroupName = "Outbox";
                        break;
                    default: break;
                }
                if(g != null)
                {
                    this.DefaultViewModel["Results"] = GetSearchResultList(g, query);
                }
#if PROBLEM
//...namely, code here will show "No results found" and no group subtitles if *either* group has 0 entries, but only makes sense if *both* have 0 entries.
//so disable for now
                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
#endif
                {
                    VisualStateManager.GoToState(this, "ResultsFound", true);
                    return;
                }
            }

            // Display informational text when there are no search results.
            VisualStateManager.GoToState(this, "NoResultsFound", true);

        }

        /// <summary>
        /// Matches search query against patient name and ID.  Search is exact, but can do substrings.
        /// If query is empty, return all
        /// </summary>
        /// <param name="sdg">Data source (outbox or PL)</param>
        /// <param name="query">user search string</param>
        /// <returns>List of SearchResult</returns>
        List<SearchResult> GetSearchResultList(SampleDataGroup sdg, string query)
        {
            App.MyAssert(sdg != null); // Group exists, even if no items in it.
            // Evidently not in RT: Uri memoryResourceUri = System.IO.Packaging.PackUriHelper.Create();

            return (
             from i in sdg.Items
             //where InSearchResults(i, query)  Filtering now down in construction of sdg
             select new SearchResult
             {
                 Image = i.Image, //Uri Image = SourceToUri(i.Image) was good enough when just color swatches for images.  But Image is now an ImageSource
                 ImageBorderColor = i.ImageBorderColor, 
                 Title = i.Title,
                 Subtitle = i.Subtitle,
                 UniqueId = i.UniqueId,
                 Description = i.Description
             }).ToList();
        }
#if WAS
        List<SearchResult> GetSearchResultList(SampleDataGroup sdg, string query)
        {
            App.MyAssert(sdg != null); // Group exists, even if no items in it.

            if(query == string.Empty)
                return (
                 from i in sdg.Items
                 select new SearchResult
                 {
                     Image = SourceToUri(i.Image),
                     Title = i.Title,
                     Subtitle = i.Subtitle,
                     UniqueId = i.UniqueId,
                     Description = i.Description
                 }).ToList();
            //otherwise
            return (
             from i in sdg.Items
             let patientName = i.Title.ToLower()
             let patientID = i.Subtitle.ToLower()
             where patientName.StartsWith(query) || patientName.Contains(" " + query) || patientID.Contains(query)
             where InSearchResults(i, query)
             select new SearchResult
             {
                 Image = SourceToUri(i.Image),
                 Title = i.Title,
                 Subtitle = i.Subtitle,
                 UniqueId = i.UniqueId,
                 Description = i.Description
             }).ToList();
        }
#endif


        /// <summary>
        /// Invoked when a filter is selected using a RadioButton when not snapped.
        /// </summary>
        /// <param name="sender">The selected RadioButton instance.</param>
        /// <param name="e">Event data describing how the RadioButton was selected.</param>
        void Filter_Checked(object sender, RoutedEventArgs e)
        {
            // Mirror the change into the CollectionViewSource used by the corresponding ComboBox
            // to ensure that the change is reflected when snapped
            if (filtersViewSource.View != null)
            {
                var filter = (sender as FrameworkElement).DataContext;
                filtersViewSource.View.MoveCurrentTo(filter);
            }
        }

        /// <summary>
        /// View model describing one of the filters available for viewing search results.
        /// </summary>
        private sealed class Filter : TP8.Common.BindableBase
        {
            private String _name;
            private int _count;
            private bool _active;

            public Filter(String name, int count, bool active = false)
            {
                this.Name = name;
                this.Count = count;
                this.Active = active;
            }

            public override String ToString()
            {
                return Description;
            }

            public String Name
            {
                get { return _name; }
                set { if (this.SetProperty(ref _name, value)) this.OnPropertyChanged("Description"); }
            }

            public int Count
            {
                get { return _count; }
                set { if (this.SetProperty(ref _count, value)) this.OnPropertyChanged("Description"); }
            }

            public bool Active
            {
                get { return _active; }
                set { this.SetProperty(ref _active, value); }
            }

            public String Description
            {
                get { return String.Format("{0} ({1})", _name, _count); }
            }
        }

        private void resultsGridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Frame.Navigate(typeof(ItemDetailFlipViewPage), ((SearchResult)e.ClickedItem).UniqueId);
#if MAYBE_BUT_TRY_APP_GLOBAL_FIRST
            navigationParamGroupAndUniqueID nav = new navigationParamGroupAndUniqueID();
            nav.GroupName = _selectedGroupFilter.Name;
            nav.UniqueId = ((SearchResult)e.ClickedItem).UniqueId;
            Frame.Navigate(typeof(ItemDetailFlipViewPage), nav);
#endif
        }

        public static Uri SourceToUri(ImageSource ImgSrc)
        {
            // Glenn hacks this up
            BitmapImage result = (BitmapImage)ImgSrc;
            return result.UriSource;
        }

        private class SearchResult
        {
            public string UniqueId { get; set; }
            public string Title { get; set; }
            public string Subtitle { get; set; }
            public ImageSource Image { get; set; }// Glenn breaks compatability: Uri Image { get; set; }
            public string Description { get; set; }
            public string ImageBorderColor { get; set; } // Glenn adds
        }

        // Glenn adds (pg 265, Likeness Win 8 C# XAML book)
        public static void SearchResultsPage_SuggestionsRequested(
            Windows.ApplicationModel.Search.SearchPane sender,
            Windows.ApplicationModel.Search.SearchPaneSuggestionsRequestedEventArgs args)
        {
            var query = args.QueryText.ToLower();
            if (query.Length < 2)
                return;
            var g = SampleDataSource.GetGroup("AllStations");
            /*
            var suggestions = (
                         from i in g.Items
                         from keywords in i.Title.Split(' ')
                         let keyword = Regex.Replace(keywords.ToLower(), @"[^\w\.@-]", "")
                         where i.Title.ToLower().Contains(query) && keyword.StartsWith(query)
                         orderby keyword
                         select keyword).Distinct(); */
#if FIRST_TRY
            var suggestions = (
                          from i in g.Items
                          let title = i.Title.ToLower()
                          where title.StartsWith(query) || title.Contains(" "+query)
                          orderby title
                          select i.Title).Distinct();

            args.Request.SearchSuggestionCollection.AppendQuerySuggestions(suggestions);
#endif
            // Search Pane can show at most 5 suggestions.  Need smarter algorithm
            //string strcol;
            int count = 0;
            foreach (var i in g.Items)
            {
                string title = i.Title.ToLower();
                string patientID = i.Subtitle.ToLower(); // In theory patient ID prefix could contain alphas
                App.MyAssert(title.Length > 0);
                App.MyAssert(patientID.Length > 0);
                if (!title.StartsWith(query) && !title.Contains(" " + query) && !patientID.Contains(query))
                    continue;
                // Nah, ToString isn't useful: strcol = args.Request.SearchSuggestionCollection.ToString(); // With initial selection from windows, and any changes we make in loop
                //if (strcol.Contains(i.Title)) // Avoid dups
                //    continue;
                args.Request.SearchSuggestionCollection.AppendQuerySuggestion(i.Title);
                if (++count == 5)
                    break; // Could look at .SearchSuggestionCollection.Size parameter, but maybe our approach push our choices and flush the system ones
            }


        }

        private async void filterFlyout_Click(object sender, RoutedEventArgs e)
        {
            //bool changed;
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                this.Frame.Navigate(typeof(FilterNonFlyout), "pageFilterNonFlyout");
                // may set to true: App.CurrentFilterProfile.AControlChanged
            }
            else
            {
                //Windows.UI.ViewManagement.ApplicationView.TryUnsnap();// Glenn's quick hack, since this doesn't work well while snapped
                // Instead, each click on button creates an instance, but they just pile up until the user unsnaps, then they appear atop each other
                var flyout = new FilterFlyout.Flyout();
                App.CurrentFilterProfile.AControlChanged = await flyout.ShowAsync();
            }
            if (App.CurrentFilterProfile.AControlChanged)
            {
                //GroupOrFlyoutFilterChanged(); // Search is not refreshed until flyout is dismissed.  Simple but maybe not ideal.
                LoadState((Object)App.CurrentSearchQuery, null); // As if navigating anew to this page... to get lists & their counts right
                App.CurrentFilterProfile.AControlChanged = false;
                // Persist the change...
                App.FilterProfileList.UpdateOrAdd(App.CurrentFilterProfile);
                await App.FilterProfileList.WriteXML();
            }
        }

#if ALSO_DIDNT_WORK
        private async void filterFlyout_Click(object sender, RoutedEventArgs e)
        {
            //Windows.UI.ViewManagement.ApplicationView.TryUnsnap();// Glenn's quick hack, since this doesn't work well while snapped
            // Instead, each click on button creates an instance, but they just pile up until the user unsnaps, then they appear atop each other
            var flyout = new FilterFlyout.Flyout();
            await flyout.ShowAsync();
        }
#endif

        private async void sortFlyout_Click(object sender, RoutedEventArgs e)
        {
            //bool changed;
            if (ApplicationView.Value == ApplicationViewState.Snapped)
            {
                this.Frame.Navigate(typeof(SortNonFlyout), "pageSortNonFlyout");
                // may set to true: App.CurrentFilterProfile.AControlChanged
            }
            else
            {
                //Windows.UI.ViewManagement.ApplicationView.TryUnsnap();// Glenn's quick hack, since this doesn't work well while snapped
                var flyout = new SortFlyout.sFlyout();
                // we could introduce our own variable here, but we'll reuse filter's

                App.CurrentFilterProfile.AControlChanged = await flyout.ShowAsync();
            }
            if (App.CurrentFilterProfile.AControlChanged)
            {
                //GroupOrFlyoutFilterChanged(); // Search is not refreshed until flyout is dismissed.  Simple but maybe not ideal.
                LoadState((Object)App.CurrentSearchQuery, null); // As if navigating anew to this page... to get lists & their counts right
                App.CurrentFilterProfile.AControlChanged = false;
            }
        }

        #region TopAppBar
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

        private void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
        }

        // Remaining are called ONLY from app navigation bar:

        private void AllStations_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "AllStations"); // Defined in SampleDataSource.cs
        }

        private void Outbox_Click(object sender, RoutedEventArgs e) // at moment, List icon on nav bar
        {
            this.Frame.Navigate(typeof(SplitPage), "Outbox"); // Defined in SampleDataSource.cs
        }

        private async void Statistics_Click(object sender, RoutedEventArgs e)
        {
            //SOON           if (App.CurrentVisualState == "Snapped" || App.CurrentVisualState == "Narrow")
            if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                // In 8.1, this replaces 8.0's TryToUnsnap:
                MessageDialog dlg = new MessageDialog("Please make TriagePic wider in order to show charts.");
                await dlg.ShowAsync();
                return;
            }
            this.Frame.Navigate(typeof(ChartsFlipPage), "pageCharts"); // was: (typeof(SplitPage),"Statistics"); // Defined in SampleDataSource.cs
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups"); //"pageRoot");
        }
        #endregion
    }

}
