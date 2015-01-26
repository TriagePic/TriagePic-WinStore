using TP8.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Documents;
using TP8.Common;

// The Item Detail Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234232

namespace TP8
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailFlipViewPage : TP8.Common.LayoutAwarePage
    {
        private string _subtitleWithCount;
        public string subtitleWithCount
        {
            get { return _subtitleWithCount; }
            set
            {
                if (value != _subtitleWithCount)
                {
                    _subtitleWithCount = value;
                    //OnPropertyChanged("SubtitleWithCount");
                }
            }
        }

        public ItemDetailFlipViewPage()
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
            // navigationParameter will be timestamp of form: 2012-08-13 18:03:26 -04:00
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // was, but didn't handle fact that same item can appear in multiple groups:
            // var group = SampleDataSource.GetGroupFromItem(navigationParameter.ToString());
            var group = SampleDataSource.GetGroup(App.CurrentSearchResultsGroupName);

            // Assigned:
            //   - a bindable group to this.DefaultViewModel["Group"]
            //   - a collection of bindable items to this.DefaultViewModel["Items"]
            //   - the selected item to this.flipView.SelectedItem
            //var group = SampleDataSource.GetGroup((String)"AllStations");
            this.DefaultViewModel["Group"] = group;
            this.DefaultViewModel["Items"] = group.Items;
            //SampleDataItem sdi = new SampleDataItem();
            //sdi.UniqueId = (String)navigationParameter;
            String id = (String)navigationParameter;
            //this.flipView.SelectedItem = id; //sdi.UniqueId;
            this.flipView.SelectedItem = SampleDataSource.GetItem(id, App.CurrentSearchResultsGroupName);

        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = this.flipView.SelectedItem;
            // TODO: Derive a serializable navigation parameter and assign it to pageState["SelectedItem"]
            pageState["SelectedItem"] = ((SampleDataItem)(this.flipView.SelectedItem)).UniqueId; // Problem on back from Edit if: = selectedItem;//Glenn just tries this
        }

        #region AppBars
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

        private void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
        }

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
            this.Frame.Navigate(typeof(SplitPage), "Statistics"); // Defined in SampleDataSource.cs
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
        }

        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MessageDialog("Discard: TO DO");
            var t = dialog.ShowAsync(); // Assign to t to suppress compiler warning
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new MessageDialog("Edit: TO DO");
            //var t = dialog.ShowAsync(); // Assign to t to suppress compiler warning
            SampleDataItem selectedItem = SampleDataSource.GetItem(this.flipView.SelectedIndex, App.CurrentSearchResultsGroupName);

            if (selectedItem != null)
            {
                // Temporary restriction, until we can do a better job of allowing edit of other items not originated at this station.
                // See also SplitPage for this restriction.
                bool foundPatient = false;
                foreach (var pr_ in App.PatientDataGroups.GetOutbox())
                {
                    if (pr_.WhenLocalTime == selectedItem.UniqueId)
                    {
                        foundPatient = true;
                        break;
                    }
                }
                if (!foundPatient)
                {
                    string msg =
                        "Sorry, can't edit this report, or any report not in the Outbox list.\n" +
                        "Only reports that were created here (and not deleted from here) can be edited here.\n" +
                        "This is a temporary restriction of this release of TriagePic for Windows Store.\n" +
                        "For now, consider editing such reports at the TriageTrak web site.";
                    var dialog1 = new MessageDialog(msg);
                    var t1 = dialog1.ShowAsync(); // Assign to t1 to suppress compiler warning
                    return;
                }

                this.Frame.Navigate(typeof(BasicPageViewEdit), selectedItem.UniqueId); // "pageViewEdit");  //UniqueId is WhenLocalTime
                return;
            }

            var dialog = new MessageDialog("Edit: No item selected.");
            var t = dialog.ShowAsync(); // Assign to t to suppress compiler warning
        }
        #endregion

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.flipView.SelectedIndex < 0)
                return; // Ignore spurious SelectionChanged events, which sometimes happen after valid call.
            // Doesn't work: subtitleWithCount = "(" + (this.flipView.SelectedIndex + 1).ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
#if WAS
            // var tb = (TextBlock)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "SubtitleWithCount");
            var tb = (TextBlock)(DependencyObject)FindVisualChild<SubtitleWithCount>(this);
            // similar to VisualTreeHelper.GetChild()
            if (tb != null)
            {
                tb.Text = "(" + (this.flipView.SelectedIndex + 1).ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
            }
#endif
            Frame f = (Frame)Window.Current.Content;
            LayoutAwarePage p = (LayoutAwarePage)f.Content;
            var tb = (TextBlock)p.FindName("SubtitleWithCount");
            if (tb != null)
            {
                tb.Text = "(" + (this.flipView.SelectedIndex + 1).ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
            }
        }

/* HOW TO USE?
        private childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject 
        { 
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) 
            { 
                DependencyObject child = VisualTreeHelper.GetChild(obj, i); 
                if (child != null && child is childItem) 
                    return (childItem)child; 
                else 
                { 
                    childItem childOfChild = FindVisualChild<childItem>(child); 
                    if (childOfChild != null) 
                        return childOfChild; 
                } 
            } 
            return null; 
        } 
*/

#if SETASIDE
        private void flipView_Loaded(object sender, RoutedEventArgs e)
        {
/*
            //var scroller = MyToolkit.UI.FrameworkElementExtensions.FindVisualChild<ScrollViewer>(FlipViewItem); // ; FindFirstElementInVisualTree<ScrollViewer>(FlipViewItem);
            // WORK BUT DONT NEED:
            // var rtb = MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "richTextBlock");
            // if (rtb == null)
            //    return;
            // Could use rtb instead of this in FindVisualChild
#if COULDNT_FIND_PP_BY_NAME
            var paragraph = (Paragraph)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(rtb, "RubricWithCount"); //(this, "RubricWithCount");
            // similar to VisualTreeHelper.GetChild()
            if (paragraph != null)
            {
                var run = new Run { Text = "test" }
                paragraph.Inlines.Add(run);
            }
#endif
            var tb = (TextBlock)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "RubricWithCount"); //(this, "RubricWithCount");
            // similar to VisualTreeHelper.GetChild()
            if (tb != null)
            {
                tb.Text = "test updating";
                tb.Visibility = Visibility.Collapsed; // toggle this to force it to be shown with new content
                tb.Visibility = Visibility.Visible;
            }
            //this.flipView.SelectedIndex
 */
        }

        private void flipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
/* Unreliable, but let's see if we turn off scrollviewer zoom mode
            var tb = (TextBlock)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "RubricWithCount"); //(this, "RubricWithCount");
            // similar to VisualTreeHelper.GetChild()
            if (tb != null)
            {
                tb.Text = "test updating";
                tb.Visibility = Visibility.Collapsed; // toggle this to force it to be shown with new content
                tb.Visibility = Visibility.Visible;
            }
 */
            var tb = (TextBlock)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "RubricWithCount"); //(this, "RubricWithCount");
            // similar to VisualTreeHelper.GetChild()
            if (tb != null)
            {
                tb.Text = "(" + this.flipView.SelectedIndex.ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
                tb.Visibility = Visibility.Collapsed; // toggle this to force it to be shown with new content
                tb.Visibility = Visibility.Visible;
            }
            tb = (TextBlock)(DependencyObject)MyToolkit.UI.FrameworkElementExtensions.FindVisualChild(this, "RubricWithCount2"); //(this, "RubricWithCount");
            // similar to VisualTreeHelper.GetChild()
            if (tb != null)
            {
                tb.Text = "(" + (this.flipView.SelectedIndex.ToString() + 1) + " of " + this.flipView.Items.Count.ToString() + ")";
                tb.Visibility = Visibility.Collapsed; // toggle this to force it to be shown with new content
                tb.Visibility = Visibility.Visible;
            }
        }

        /* Tried to use the functions below with this extra paragraph within richTextBlock of flipView:
                                            <Paragraph>
                                        <InlineUIContainer>
                                            <TextBlock x:Name="RubricWithCount" FontSize="20" FontWeight="Light" SelectionChanged="RubricWithCount_SelectionChanged"/>
                                            <!-- Text="{Binding Index, Source={StaticResource itemsViewSource}}"/> -->
                                            <!-- Loaded="RubricWithCount_Loaded" /> -->
                                        </InlineUIContainer>
                                    </Paragraph>
        */

        private void RubricWithCount_Loaded(object sender, RoutedEventArgs e)
        {
           // ((TextBlock)sender).Text = "(" + this.flipView.SelectedIndex.ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
        }

        private void RubricWithCount_SelectionChanged(object sender, RoutedEventArgs e)
        {
/*
            TextBlock tb = ((TextBlock)sender);
            tb.Text = "(" + this.flipView.SelectedIndex.ToString() + " of " + this.flipView.Items.Count.ToString() + ")";
            tb.Visibility = Visibility.Collapsed; // toggle this to force it to be shown with new content
            tb.Visibility = Visibility.Visible;
*/ 
        }

#if COULDNT_FIGURE_HOW_TO_BIND
        public string CountRubric
        {
            get
            {
                return "(" + this.flipView.Items.Count.ToString() + ")";
            }
        }
#endif
#endif
    }
}
