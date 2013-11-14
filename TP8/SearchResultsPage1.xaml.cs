using TP8_1.Data;

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

// The Search Contract item template is documented at http://go.microsoft.com/fwlink/?LinkId=234240

namespace TP8_1.Common
{
    /// <summary>
    /// This page displays search results when a global search is directed to this application.
    /// </summary>
    public sealed partial class SearchResultsPage1 : TP8_1.Common.LayoutAwarePage
    {
        private string _query;

        public SearchResultsPage1()
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
            _query = navigationParameter as String;

            var query = _query == null ? string.Empty : _query.ToLower();

            //       Only the first filter, typically "All", should pass true as a third argument in
            //       order to start in an active state.  Results for the active filter are provided
            //       in Filter_SelectionChanged below.

            var filterList = new List<Filter>();
            filterList.Add(new Filter("Outbox", GetSearchResultList(SampleDataSource.GetGroup("Outbox"),query).Count, true));
            filterList.Add(new Filter("From All Stations", GetSearchResultList(SampleDataSource.GetGroup("FullSearch"),query).Count, false));
            //filterList.Add(new Filter("Outbox", SampleDataSource.GetGroup("Outbox").Items.Count, true));
            //filterList.Add(new Filter("All at PL", SampleDataSource.GetGroup("FullSearch").Items.Count, false));
            //filterList.Add(new Filter("PL", SampleDataSource.GetGroup("FullSearch").Items.Count, false));
            /* Compare with SplitPage:
            var group = SampleDataSource.GetGroup((String)navigationParameter);
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items; */
            var group = SampleDataSource.GetGroup((String)"Outbox");
            this.DefaultViewModel["Group"] = group;
            /* Maybe not:
            this.DefaultViewModel["Items"] = group.Items; */

            this.DefaultViewModel["QueryText"] = '\u201c' + _query + '\u201d';
            this.DefaultViewModel["Filters"] = filterList;
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

        /// <summary>
        /// Invoked when a filter is selected using the ComboBox in snapped view state.
        /// </summary>
        /// <param name="sender">The ComboBox instance.</param>
        /// <param name="e">Event data describing how the selected filter was changed.</param>
        void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var query = _query.ToLower();
            // Determine what filter was selected
            var selectedFilter = e.AddedItems.FirstOrDefault() as Filter;
            if (selectedFilter != null)
            {
                // Mirror the results into the corresponding Filter object to allow the
                // RadioButton representation used when not snapped to reflect the change
                selectedFilter.Active = true;

                // TODO: Respond to the change in active filter by setting this.DefaultViewModel["Results"]
                //       to a collection of items with bindable Image, Title, Subtitle, and Description properties
                SampleDataGroup g = null;
                switch(selectedFilter.Name)
                {
                    case "From All Stations":
                        g = SampleDataSource.GetGroup("FullSearch");
                        break;
                    case "Outbox":
                        g = SampleDataSource.GetGroup("Outbox");
                        break;
                    default: break;
                }
                if(g != null)
                {
                    this.DefaultViewModel["Results"] = GetSearchResultList(g, query);
                }

                // Ensure results are found
                object results;
                ICollection resultsCollection;
                if (this.DefaultViewModel.TryGetValue("Results", out results) &&
                    (resultsCollection = results as ICollection) != null &&
                    resultsCollection.Count != 0)
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
            Debug.Assert(sdg != null);
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
             select new SearchResult
             {
                 Image = SourceToUri(i.Image),
                 Title = i.Title,
                 Subtitle = i.Subtitle,
                 UniqueId = i.UniqueId,
                 Description = i.Description
             }).ToList();
        }

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
        private sealed class Filter : TP8_1.Common.BindableBase
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
            public Uri Image { get; set; }
            public string Description { get; set; }
        }

        // Glenn adds (pg 265, Likeness Win 8 C# XAML book)
        public static void SearchResultsPage1_SuggestionsRequested(
            Windows.ApplicationModel.Search.SearchPane sender,
            Windows.ApplicationModel.Search.SearchPaneSuggestionsRequestedEventArgs args)
        {
            var query = args.QueryText.ToLower();
            if (query.Length < 2)
                return;
            var g = SampleDataSource.GetGroup("FullSearch");
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
                Debug.Assert(title.Length > 0);
                Debug.Assert(patientID.Length > 0);
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

        private void filterFlyout_Click(object sender, RoutedEventArgs e)
        {
            var flyout = new FilterFlyout.Flyout();
            flyout.ShowAsync();
        }
    }

}
