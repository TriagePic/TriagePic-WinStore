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
//using MyToolkit.Controls;
using TP8.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8
{
    public sealed partial class FilterFlyout : UserControl, IPopupControl //, INotifyPropertyChanged
	{
        // Moved here from app, but then not defined earlier enough so move back:
        // public static TP_FilterProfile CurrentFilterProfile = new TP_FilterProfile();
        // public static bool profileDefined = false;

        public string MyZoneCheckBoxItemWidth { get; set; }

        private ZoneCheckBox[] zcb = null;
        private int zcbCount = 0;

		public FilterFlyout()
		{
			this.InitializeComponent();
            //if (!profileDefined)
            //{
            //    App.CurrentFilterProfile.ResetFilterProfileToDefault();
            //    profileDefined = true;
            //}
		}

        private void InitiateZoneCheckBoxes()// Compare New Report's InitiateZones()
        {
            //ZoneChoiceComboSnapped.ItemsSource = App.ZoneChoices.GetZoneChoices();
            // Instantiate zone buttons:
            zcbCount = App.ZoneChoices.GetZoneChoiceList().Count;
            App.MyAssert(zcbCount > 0);
            zcb = new ZoneCheckBox[zcbCount];
            int i = 0;
            string checkBoxName;
            foreach (TP_ZoneChoiceItem zci in App.ZoneChoices.GetZoneChoiceList())
            {
                if (String.IsNullOrEmpty(zci.ButtonName))
                    continue;
                checkBoxName = "zoneCheckBox" + zci.ButtonName.Replace(" ", "");
                zcb[i] = new ZoneCheckBox(checkBoxName, (Color)zci.ColorObj, zci.ButtonName);
                zcb[i].Click += ZoneCheckBox_Click;
                ToolTip tt = new ToolTip();
                tt.Content = zci.Meaning;
                zcb[i].SetToolTip(tt);
                ZoneCheckBoxes.Items.Add(zcb[i]);

                i++;
            }

            //Zone_Clear();
        }

		private Flyout m_flyout;

        public class Flyout : PopupHelperWithResult<bool, FilterFlyout>
        {
            public override PopupSettings Settings
            {
                get
                {
                    return PopupSettings.Flyout;
                }
            }
        }
#if ORIGINAL
        public class Flyout : PopupHelper<FilterFlyout>
        {
	        public override PopupSettings Settings
	        {
		        get
		        {
			        return PopupSettings.Flyout;
		        }
	        }
        }
#endif

		public void SetParent(PopupHelper parent)
		{
			m_flyout = (Flyout)parent;
            m_flyout.Result = false; // controls not changed
		}

		public void Closed(CloseAction action)
		{
		}

		public void Opened()
		{
            InitiateZoneCheckBoxes();
            FlyoutEventComboBox.ItemsSource = App.CurrentDisasterListFilters; //TP_EventsDataList.GetEventFilters();
            // Don't do setting of default here, as in:
            //   FlyoutEventComboBox.SelectedIndex = 0;
            // because it will fire save to App.CurrentFilterProfile.FlyoutEventComboBoxChoice, overwriting real choice.
            int i = 0;
            bool set = false;
            foreach (var disaster in App.CurrentDisasterListFilters)
            {
                if (disaster.EventName == App.CurrentFilterProfile.FlyoutEventComboBoxChoice)
                {
                    FlyoutEventComboBox.SelectedIndex = i;
                    set = true;
                    break;
                }
                i++;
            }
            if(!set)
                FlyoutEventComboBox.SelectedIndex = 0;
            LoadControls();
		}

        private void LoadControls() // Doing this because binding isn't working for me
        {
            searchAgainstName.IsChecked = App.CurrentFilterProfile.SearchAgainstName;
            searchAgainstID.IsChecked = App.CurrentFilterProfile.SearchAgainstID;
            eventTestDemoExercise.IsChecked = App.CurrentFilterProfile.DisasterEventIncludeTest;
            eventReal.IsChecked = App.CurrentFilterProfile.DisasterEventIncludeReal;
            eventPrivate.IsChecked = App.CurrentFilterProfile.DisasterEventIncludePrivate;
            eventPublic.IsChecked = App.CurrentFilterProfile.DisasterEventIncludePublic;
            // Done elsewhere FlyoutEventComboBoxChoice = "All events";
            // Hard to support with current state of PL, choice of events based on "Hospital/Organization":
            //"Current (if known, otherwise Default)"
            //"All Sharing These Reports"
            //"Specified:" ComboBox

            // Reported Attributes:
            genderMale.IsChecked = App.CurrentFilterProfile.IncludeMale;
            genderFemale.IsChecked = App.CurrentFilterProfile.IncludeFemale;
            genderUnknownOrComplex.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender;
            ageGroupAdult.IsChecked = App.CurrentFilterProfile.IncludeAdult;
            ageGroupPeds.IsChecked = App.CurrentFilterProfile.IncludePeds;
            ageGroupUnknown.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup;
/* WAS:
            zoneGreen.IsChecked = App.CurrentFilterProfile.IncludeGreenZone;
            zoneBHGreen.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone;
            zoneYellow.IsChecked = App.CurrentFilterProfile.IncludeYellowZone;
            zoneRed.IsChecked = App.CurrentFilterProfile.IncludeRedZone;
            zoneGray.IsChecked = App.CurrentFilterProfile.IncludeGrayZone;
            zoneBlack.IsChecked = App.CurrentFilterProfile.IncludeBlackZone;
*/
            foreach (ZoneCheckBox z in zcb)
            {
                z.IsChecked = App.CurrentFilterProfile.IncludeWhichZones.IsIncluded(z.ContentAutoForeground);
            }

            hasName.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName;
            hasNoName.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName;
            hasPhotos.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos;
            hasNoPhoto.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto;

            /* TO DO:
            // When Reported:
            if (App.CurrentFilterProfile.AllDates) //(uses dateFilter radio group)
                datesAll.IsChecked = true;
            else
                datesSpecified.IsChecked = true;

            // TO DO:
            // fromDate. FromMonth = "";
            // FromDay = "";
            // FromYear = "";
            // ToMonth = "";
            // ToDay = "";
            // ToYear = "";
             */
        }

/* Doesn't help... and should be private if in sealed class:
        protected void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if( PropertyChanged != null )
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
        }

        public event PropertyChangedEventHandler PropertyChanged; //= delegate { };
*/

/* TO DO - Support user saving filter profile as name (with hospital quietly appended), then retrieving same.
        private void filterProfile_Loaded(object sender, RoutedEventArgs e)
        {
            filterProfile.SelectedIndex = 0;
        } */

        private void FlyoutEventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FlyoutEventComboBox.SelectedItem != null)
            {
                TP8.Data.TP_EventsDataItem tpEvent = (TP_EventsDataItem)FlyoutEventComboBox.SelectedItem;
                //App.FilterFlyoutEventType = tpEvent.TypeIconUri;
                App.CurrentFilterProfile.FlyoutEventComboBoxChoice = tpEvent.EventName; // could be specific event, or group name
            }
        }

        // The "..._Changed" handlers that follow, were introduced when trying to do binding against CurrentFilterProfile
        // (both when it was defined in App and when it was defined in FilterFlyout).
        // You wouldn't think you would need such handlers with 2-way binding, but evidently you do.
        // There was some indication in forums (2011 vintage) that buttons/checkboxes/radios are problematic vis a vis binding.
        // Penzold's Win 8 book has a rather elaborate way of doing it.  Me, I just gave up on binding, use the event handler alone below.
        // Each changed handler is a target of both "Checked" and "Unchecked" event.  I suppose I could have just used click handlers, maybe simplify the xaml.

        private void SearchAgainstNameCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.SearchAgainstName = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void SearchAgainstIDCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.SearchAgainstID = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void EventTestDemoExerciseCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludeTest = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void EventRealCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludeReal = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void EventPrivateCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludePrivate = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void EventPublicCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludePublic = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void GenderMaleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeMale = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void GenderFemaleCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeFemale = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void GenderUnknownOrComplexCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeUnknownOrComplexGender = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void AgeGroupAdultCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeAdult = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void AgeGroupPedsCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludePeds = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void AgeGroupUnknownCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeUnknownAgeGroup = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }
/* WAS:
        private void ZoneGreenCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeGreenZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void ZoneBHGreenCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeBHGreenZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void ZoneYellowCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeYellowZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void ZoneRedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeRedZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void ZoneGrayCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeGrayZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void ZoneBlackCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeBlackZone = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }
*/
        private void HasNameCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasPatientName = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void HasNoNameCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasNoPatientName = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void HasPhotosCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasPhotos = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

        private void HasNoPhotoCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasNoPhoto = (bool)((CheckBox)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }

/* TO DO:
        private void DatesAllRadioButton_Changed(object sender, RoutedEventArgs e)
        {
            App.MyAssert(((RadioButton)sender).Name == "datesAll");
            App.CurrentFilterProfile.AllDates = (bool)((RadioButton)sender).IsChecked;
            m_flyout.Result = true; // 1 or more controls changed
        }
 */

        private void attributesSelectAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Reported Attributes:
            genderMale.IsChecked = App.CurrentFilterProfile.IncludeMale = true;
            genderFemale.IsChecked = App.CurrentFilterProfile.IncludeFemale = true;
            genderUnknownOrComplex.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender = true;
            ageGroupAdult.IsChecked = App.CurrentFilterProfile.IncludeAdult = true;
            ageGroupPeds.IsChecked = App.CurrentFilterProfile.IncludePeds = true;
            ageGroupUnknown.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup = true;
/* WAS:
            zoneGreen.IsChecked = App.CurrentFilterProfile.IncludeGreenZone = true;
            zoneBHGreen.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone = true;
            zoneYellow.IsChecked = App.CurrentFilterProfile.IncludeYellowZone = true;
            zoneRed.IsChecked = App.CurrentFilterProfile.IncludeRedZone = true;
            zoneGray.IsChecked = App.CurrentFilterProfile.IncludeGrayZone = true;
            zoneBlack.IsChecked = App.CurrentFilterProfile.IncludeBlackZone = true;
 */
            hasName.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName = true;
            hasNoName.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName = true;
            hasPhotos.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos = true;
            hasNoPhoto.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto = true;
            CheckmarkAllZones();
            m_flyout.Result = true; // assume 1 or more controls changed
        }

        private void attributesClearAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Reported Attributes:
            genderMale.IsChecked = App.CurrentFilterProfile.IncludeMale = false;
            genderFemale.IsChecked = App.CurrentFilterProfile.IncludeFemale = false;
            genderUnknownOrComplex.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender = false;
            ageGroupAdult.IsChecked = App.CurrentFilterProfile.IncludeAdult = false;
            ageGroupPeds.IsChecked = App.CurrentFilterProfile.IncludePeds = false;
            ageGroupUnknown.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup = false;
/* WAS:
            zoneGreen.IsChecked = App.CurrentFilterProfile.IncludeGreenZone = false;
            zoneBHGreen.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone = false;
            zoneYellow.IsChecked = App.CurrentFilterProfile.IncludeYellowZone = false;
            zoneRed.IsChecked = App.CurrentFilterProfile.IncludeRedZone = false;
            zoneGray.IsChecked = App.CurrentFilterProfile.IncludeGrayZone = false;
            zoneBlack.IsChecked = App.CurrentFilterProfile.IncludeBlackZone = false;
 */
            hasName.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName = false;
            hasNoName.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName = false;
            hasPhotos.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos = false;
            hasNoPhoto.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto = false;
            UncheckmarkAllZones();
            m_flyout.Result = true; // assume 1 or more controls changed
        }

        /// <summary>
        /// Propagate checkmark change to filter list
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="isIncluded">True if checkbox is checked, else false</param>
        private void UpdateZoneList(string zone, bool isIncluded)
        {
            foreach (var z in App.CurrentFilterProfile.IncludeWhichZones.GetAsList()) //ZoneCheckBoxes.Items)
            {
                if (z.ZoneName == zone)
                {
                    if (z.IsIncluded != isIncluded)
                    {
                        z.IsIncluded = isIncluded;
                        m_flyout.Result = true; // effect same as App.CurrentFilterProfile.AControlChanged = true;
                    }
                    return;
                }
            }
        }

        private void ZoneCheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox clickedCheckBox = (CheckBox)sender;
            string zoneTapped = clickedCheckBox.Content.ToString();
            bool isChecked = (bool)clickedCheckBox.IsChecked;
            UpdateZoneList(zoneTapped, isChecked);
        }

        /// <summary>
        /// Unchecks all zone filter checkboxes, and sets corresponding list
        /// </summary>
        private void UncheckmarkAllZones()
        {
            AdjustAllZones(false);
        }

        /// <summary>
        /// Checks all zone filter checkboxes, and sets corresponding list
        /// </summary>
        private void CheckmarkAllZones()
        {
            AdjustAllZones(true);
        }

        private void AdjustAllZones(bool commonValue)
        {
            if (ZoneCheckBoxes.Items != null)
                foreach (ZoneCheckBox zcb in ZoneCheckBoxes.Items)
                    zcb.IsChecked = commonValue;
            foreach (var z in App.CurrentFilterProfile.IncludeWhichZones.GetAsList())
                z.IsIncluded = commonValue;
        }

	}
}
