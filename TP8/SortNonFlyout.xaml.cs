using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using SocialEbola.Lib.PopupHelpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
//using MyToolkit.Controls;
using System.Diagnostics;
using TP8.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using TP8.Common;


namespace TP8
{
	public sealed partial class SortNonFlyout : TP8.Common.BasicLayoutPage// WAS LayoutAwarePage
	{
		public SortNonFlyout()
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
        protected override void LoadState(LoadStateEventArgs e) // WAS: LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            Opened();
#if TODOish
            pageSubtitle.Text = " " + App.CurrentDisasterEventName; // TO DO: binding in XAML instead of here?  Add space to separate from icon
            string prefix = "ms-appx:///";
            if (App.CurrentDisasterEventTypeIcon.Length > prefix.Length)
            {
                eventTypeImage.Source = new BitmapImage(new Uri(App.CurrentDisasterEventTypeIcon));
#if MAYBE
                string path = App.CurrentDisasterEventTypeIcon.Substring(prefix.Length);
                path += Windows.ApplicationModel.Package.Current.InstalledLocation.ToString();
                BitmapImage img = new BitmapImage();
                var uri = new Uri(path);
                img.UriSource = uri;
                eventTypeImage.Source = img;
#endif
                // Tried XAML binding, didn't work:                 <Image x:Name="eventTypeImage" Source="{Binding CurrentDisasterEventTypeIcon}" Width="40" VerticalAlignment="Top"/>
                MyZoneButtonItemWidth = "140";
            }
#endif
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(SaveStateEventArgs e) // WAS: SaveState(Dictionary<String, Object> pageState)
        {
        }


		public void Opened()
		{
            // Next line assumes enum order and control order are the same:
            SortByCarouselContainer.SelectedIndex = (Int32)App.SortFlyoutItem;
/*
            switch (App.SortFlyoutItem)
            {
                case App.SortByItem.ArrivalTime: ArrivalTime.IsChecked = true; break;
                case App.SortByItem.PatientID: PatientID.IsChecked = true; break;
                case App.SortByItem.FirstName: FirstName.IsChecked = true; break;
                case App.SortByItem.LastName: LastName.IsChecked = true; break;
                case App.SortByItem.Gender: Gender.IsChecked = true; break;
                case App.SortByItem.AgeGroup: AgeGroup.IsChecked = true; break;
                case App.SortByItem.TriageZone: TriageZone.IsChecked = true; break;
                case App.SortByItem.PLStatus: PLStatus.IsChecked = true; break;
                case App.SortByItem.DisasterEvent: DisasterEvent.IsChecked = true; break;
                case App.SortByItem.ReportingStation: ReportingStation.IsChecked = true; break;

                default:
                    App.MyAssert(false);
                    break;
            } */

            Direction.IsOn = App.SortFlyoutAscending;
        }

        // For how to create Carousel control, see http edulorenzo.wordpress.com/2012/09/25/how-to-make-a-carousel-control-for-windows-8-using-c

        /*
        private void SortByRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            switch(((RadioButton)sender).Name.ToString())
            {
                case "ArrivalTime": App.SortFlyoutItem = App.SortByItem.ArrivalTime; break;
                case "PatientID": App.SortFlyoutItem = App.SortByItem.PatientID; break;
                case "FirstName": App.SortFlyoutItem = App.SortByItem.FirstName; break;
                case "LastName": App.SortFlyoutItem = App.SortByItem.LastName; break;
                case "Gender": App.SortFlyoutItem = App.SortByItem.Gender; break;
                case "AgeGroup": App.SortFlyoutItem = App.SortByItem.AgeGroup; break;
                case "TriageZone": App.SortFlyoutItem = App.SortByItem.TriageZone; break;
                case "PLStatus": App.SortFlyoutItem = App.SortByItem.PLStatus; break;
                case "DisasterEvent": App.SortFlyoutItem = App.SortByItem.DisasterEvent; break;
                case "ReportingStation": App.SortFlyoutItem = App.SortByItem.ReportingStation; break;
                default:
                    App.MyAssert(false);
                    break;
            }

        }*/

        private void SortByCarouselContainer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Next line assumes enum order and control order are the same:
            App.SortFlyoutItem = (App.SortByItem)SortByCarouselContainer.SelectedIndex;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void DirectionToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            App.SortFlyoutAscending = ((ToggleSwitch)sender).IsOn;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        //private void GoBack(object sender, RoutedEventArgs e)
        //{

//        }

        /* TO DO:
        private void ShowTitleAsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //to do
        }

        private void ShowSubtitleAsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // to do
        }
        */


	}
}
