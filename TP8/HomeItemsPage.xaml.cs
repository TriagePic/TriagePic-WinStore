using TP8.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using TP8.Common;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace TP8
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page 
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class HomeItemsPage : TP8.Common.BasicLayoutPage //was: LayoutAwarePage
    {

        public HomeItemsPage()
        {
            this.InitializeComponent();
        }

#if WAS
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Items"] = sampleDataGroups;

            // Too simple events model:
            // 2nd try: TP_EventsDataList edl = new TP_EventsDataList();
            if (App.DelayedMessageToUserOnStartup != "") // Got content during App.OnLaunched, but can't be easily shown until now
            {
                MessageDialog dlg = new MessageDialog(App.DelayedMessageToUserOnStartup);
                await dlg.ShowAsync();
                App.DelayedMessageToUserOnStartup = "";
            }
            App.AppFinishedLaunching = true; // set to true during home page launch

        }

#endif
        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        protected override async void LoadState(LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var navigationParameter = e.NavigationParameter;
            var sampleDataGroups = SampleDataSource.GetGroups((String)navigationParameter);
            this.DefaultViewModel["Items"] = sampleDataGroups;

            // Too simple events model:
            // 2nd try: TP_EventsDataList edl = new TP_EventsDataList();
            if (App.DelayedMessageToUserOnStartup != "") // Got content during App.OnLaunched, but can't be easily shown until now
            {
                MessageDialog dlg = new MessageDialog(App.DelayedMessageToUserOnStartup);
                await dlg.ShowAsync();
                App.DelayedMessageToUserOnStartup = "";
            }
            App.AppFinishedLaunching = true; // set to true during home page launch
            // Not needed: base.LoadState(e);
        }

        /// <summary>
        /// Invoked when an item is clicked.
        /// </summary>
        /// <param name="sender">The GridView (or ListView when the application is snapped)
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var groupId = ((SampleDataGroup)e.ClickedItem).UniqueId;
            if (groupId == "New")
                New_Click(sender, e);
            else if (groupId == "Checklist")
                Checklist_Click(sender, e);
            else if (groupId == "Statistics")
                Statistics_Click(sender, e);
            else
                this.Frame.Navigate(typeof(SplitPage), groupId);
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
/* WAS
        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ChartsFlipPage), "pageCharts"); // was: (typeof(SplitPage),"Statistics"); // Defined in SampleDataSource.cs
        }
 */

        private async void Statistics_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentVisualState == "vs320Wide" || App.CurrentVisualState == "vs321To500Wide")
            // was: if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                // In 8.1, this replaces 8.0's TryToUnsnap:
                MessageDialog dlg = new MessageDialog("Please make TriagePic wider in order to show charts.");
                await dlg.ShowAsync();
                return;
            }
            this.Frame.Navigate(typeof(ChartsFlipPage),"pageCharts");
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
        }
        #endregion

        #region BottomAppBar
        private void Help_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // The URI to launch
            string uriToLaunch = @"https://github.com/TriagePic/TriagePic-WinStore/wiki/User-Guide-to-TriagePic-from-the-Windows-Store";
            var uri = new Uri(uriToLaunch);
            LaunchDefaultBrowser(uri);
        }


        private async void LaunchDefaultBrowser(Uri uri)
        {
           // Set the desired remaining view.
           var options = new Windows.System.LauncherOptions();
           options.DesiredRemainingView = Windows.UI.ViewManagement.ViewSizePreference.UseHalf;

           // Launch the URI 
           var success = await Windows.System.Launcher.LaunchUriAsync(uri, options);

           if (success)
           {
              // URI launched
           }
           else
           {
              // URI launch failed
           }
        }
        #endregion
    }
}
