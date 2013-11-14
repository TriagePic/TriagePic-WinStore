using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SocialEbola.Lib.PopupHelpers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using MyToolkit.Controls;
using System.Diagnostics;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8_1
{
	public sealed partial class SortFlyout : UserControl, IPopupControl
	{
		public SortFlyout()
		{
			this.InitializeComponent();
		}

		private sFlyout m_sortflyout;

        public class sFlyout : PopupHelper<SortFlyout>
        {
	        public override PopupSettings Settings
	        {
		        get
		        {
			        return PopupSettings.Flyout;
		        }
	        }
        }

		public void SetParent(PopupHelper parent)
		{
			m_sortflyout = (sFlyout)parent;
		}

		public void Closed(CloseAction action)
		{
		}

		public void Opened()
		{
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
                    Debug.Assert(false);
                    break;
            }

            Direction.IsOn = App.SortFlyoutAscending;
        }

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
                    Debug.Assert(false);
                    break;
            }

        }

        private void DirectionToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
                App.SortFlyoutAscending = ((ToggleSwitch)sender).IsOn;
        }

        private void ShowTitleAsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //to do
        }

        private void ShowSubtitleAsRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // to do
        }

	}
}
