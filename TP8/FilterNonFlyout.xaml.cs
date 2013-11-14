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
using MyToolkit.Controls;
using TP8.Data;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics;
using Windows.UI;


namespace TP8
{
    public sealed partial class FilterNonFlyout : TP8.Common.LayoutAwarePage
	{
        // Moved here from app, but then not defined earlier enough so move back:
        // public static TP_FilterProfile CurrentFilterProfile = new TP_FilterProfile();
        // public static bool profileDefined = false;

        //public bool aControlChanged = false; // Compare to flyout's m_flyout.Result

        public string MyZoneCheckBoxItemWidth { get; set; }


        private ZoneCheckBox[] zcb = null;
        private int zcbCount = 0;

		public FilterNonFlyout()
		{
			this.InitializeComponent();
            //if (!profileDefined)
            //{
            //    App.CurrentFilterProfile.ResetFilterProfileToDefault();
            //    profileDefined = true;
            //}
            // App.CurrentFilterProfile.AControlChanged = false; // until we know better
            MyZoneCheckBoxItemWidth = "100";
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
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }


        public void Opened()
		{
            InitiateZoneCheckBoxes();
            eventComboBoxNF.ItemsSource = App.CurrentDisasterListFilters; //TP_EventsDataItem.GetEventFilters();
            // Don't do setting of default here, as in:
            //   FlyoutEventComboBox.SelectedIndex = 0;
            // because it will fire save to App.CurrentFilterProfile.FlyoutEventComboBoxChoice, overwriting real choice.
            int i = 0;
            bool set = false;
            foreach (var disaster in App.CurrentDisasterListFilters)
            {
                if (disaster.EventName == App.CurrentFilterProfile.FlyoutEventComboBoxChoice)
                {
                    eventComboBoxNF.SelectedIndex = i;
                    set = true;
                    break;
                }
                i++;
            }
            if (!set)
                eventComboBoxNF.SelectedIndex = 0;
            LoadControls();
		}

        private void LoadControls() // Doing this because binding isn't working for me
        {
            searchAgainstNameNF.IsChecked = App.CurrentFilterProfile.SearchAgainstName;
            searchAgainstIDNF.IsChecked = App.CurrentFilterProfile.SearchAgainstID;
            eventTestDemoExerciseNF.IsChecked = App.CurrentFilterProfile.DisasterEventIncludeTest;
            eventRealNF.IsChecked = App.CurrentFilterProfile.DisasterEventIncludeReal;
            eventPrivateNF.IsChecked = App.CurrentFilterProfile.DisasterEventIncludePrivate;
            eventPublicNF.IsChecked = App.CurrentFilterProfile.DisasterEventIncludePublic;
            // Done elsewhere FlyoutEventComboBoxChoice = "All events";
            // Hard to support with current state of PL, choice of events based on "Hospital/Organization":
            //"Current (if known, otherwise Default)"
            //"All Sharing These Reports"
            //"Specified:" ComboBox

            // Reported Attributes:
            genderMaleNF.IsChecked = App.CurrentFilterProfile.IncludeMale;
            genderFemaleNF.IsChecked = App.CurrentFilterProfile.IncludeFemale;
            genderUnknownOrComplexNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender;
            ageGroupAdultNF.IsChecked = App.CurrentFilterProfile.IncludeAdult;
            ageGroupPedsNF.IsChecked = App.CurrentFilterProfile.IncludePeds;
            ageGroupUnknownNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup;
/* WAS:
            zoneGreenNF.IsChecked = App.CurrentFilterProfile.IncludeGreenZone;
            zoneBHGreenNF.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone;
            zoneYellowNF.IsChecked = App.CurrentFilterProfile.IncludeYellowZone;
            zoneRedNF.IsChecked = App.CurrentFilterProfile.IncludeRedZone;
            zoneGrayNF.IsChecked = App.CurrentFilterProfile.IncludeGrayZone;
            zoneBlackNF.IsChecked = App.CurrentFilterProfile.IncludeBlackZone;
 */
            foreach (ZoneCheckBox z in zcb)
            {
                z.IsChecked = App.CurrentFilterProfile.IncludeWhichZones.IsIncluded(z.ContentAutoForeground);
            }
            hasNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName;
            hasNoNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName;
            hasPhotosNF.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos;
            hasNoPhotoNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto;

            /* TO DO:
            // When Reported:
            if (App.CurrentFilterProfile.AllDates) //(uses dateFilter radio group)
                datesAllNF.IsChecked = true;
            else
                datesSpecifiedNF.IsChecked = true;

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
            filterProfileNF.SelectedIndex = 0;
        } */

        private void eventComboBoxNF_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (eventComboBoxNF.SelectedItem != null)
            {
                TP8.Data.TP_EventsDataItem tpEvent = (TP_EventsDataItem)eventComboBoxNF.SelectedItem;
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

        private void SearchAgainstNameNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.SearchAgainstName = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void SearchAgainstIDNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.SearchAgainstID = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void EventTestDemoExerciseNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludeTest = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void EventRealNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludeReal = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void EventPrivateNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludePrivate = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void EventPublicNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.DisasterEventIncludePublic = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void GenderMaleNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeMale = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void GenderFemaleNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeFemale = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void GenderUnknownOrComplexNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeUnknownOrComplexGender = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void AgeGroupAdultNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeAdult = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void AgeGroupPedsNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludePeds = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void AgeGroupUnknownNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeUnknownAgeGroup = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }
/* WAS:
        private void ZoneGreenNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeGreenZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void ZoneBHGreenNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeBHGreenZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void ZoneYellowNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeYellowZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void ZoneRedNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeRedZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void ZoneGrayNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeGrayZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void ZoneBlackNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeBlackZone = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }
*/
        private void HasNameNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasPatientName = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void HasNoNameNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasNoPatientName = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void HasPhotosNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasPhotos = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

        private void HasNoPhotoNFCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            App.CurrentFilterProfile.IncludeHasNoPhoto = (bool)((CheckBox)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }

/* TO DO:
        private void DatesAllNFRadioButton_Changed(object sender, RoutedEventArgs e)
        {
            App.MyAssert(((RadioButton)sender).Name == "datesAll");
            App.CurrentFilterProfile.AllDates = (bool)((RadioButton)sender).IsChecked;
            App.CurrentFilterProfile.AControlChanged = true; // 1 or more controls changed
        }
 */

        private void attributesSelectAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Reported Attributes:
            genderMaleNF.IsChecked = App.CurrentFilterProfile.IncludeMale = true;
            genderFemaleNF.IsChecked = App.CurrentFilterProfile.IncludeFemale = true;
            genderUnknownOrComplexNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender = true;
            ageGroupAdultNF.IsChecked = App.CurrentFilterProfile.IncludeAdult = true;
            ageGroupPedsNF.IsChecked = App.CurrentFilterProfile.IncludePeds = true;
            ageGroupUnknownNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup = true;
/* WAS:
            zoneGreenNF.IsChecked = App.CurrentFilterProfile.IncludeGreenZone = true;
            zoneBHGreenNF.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone = true;
            zoneYellowNF.IsChecked = App.CurrentFilterProfile.IncludeYellowZone = true;
            zoneRedNF.IsChecked = App.CurrentFilterProfile.IncludeRedZone = true;
            zoneGrayNF.IsChecked = App.CurrentFilterProfile.IncludeGrayZone = true;
            zoneBlackNF.IsChecked = App.CurrentFilterProfile.IncludeBlackZone = true;
 */
            hasNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName = true;
            hasNoNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName = true;
            hasPhotosNF.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos = true;
            hasNoPhotoNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto = true;
            UncheckmarkAllZones();
            App.CurrentFilterProfile.AControlChanged = true; // assume 1 or more controls changed
        }

        private void attributesClearAllButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Reported Attributes:
            genderMaleNF.IsChecked = App.CurrentFilterProfile.IncludeMale = false;
            genderFemaleNF.IsChecked = App.CurrentFilterProfile.IncludeFemale = false;
            genderUnknownOrComplexNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownOrComplexGender = false;
            ageGroupAdultNF.IsChecked = App.CurrentFilterProfile.IncludeAdult = false;
            ageGroupPedsNF.IsChecked = App.CurrentFilterProfile.IncludePeds = false;
            ageGroupUnknownNF.IsChecked = App.CurrentFilterProfile.IncludeUnknownAgeGroup = false;
/* WAS:
            zoneGreenNF.IsChecked = App.CurrentFilterProfile.IncludeGreenZone = false;
            zoneBHGreenNF.IsChecked = App.CurrentFilterProfile.IncludeBHGreenZone = false;
            zoneYellowNF.IsChecked = App.CurrentFilterProfile.IncludeYellowZone = false;
            zoneRedNF.IsChecked = App.CurrentFilterProfile.IncludeRedZone = false;
            zoneGrayNF.IsChecked = App.CurrentFilterProfile.IncludeGrayZone = false;
            zoneBlackNF.IsChecked = App.CurrentFilterProfile.IncludeBlackZone = false;
 */
            hasNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasPatientName = false;
            hasNoNameNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPatientName = false;
            hasPhotosNF.IsChecked = App.CurrentFilterProfile.IncludeHasPhotos = false;
            hasNoPhotoNF.IsChecked = App.CurrentFilterProfile.IncludeHasNoPhoto = false;
            UncheckmarkAllZones();
            App.CurrentFilterProfile.AControlChanged = true; // assume 1 or more controls changed
        }

        /// <summary>
        /// Propagate checkmark change to filter list
        /// </summary>
        /// <param name="zone"></param>
        /// <param name="isIncluded">true if checkbox is checked, otherwise false</param>
        private void UpdateZoneList(string zone, bool isIncluded)
        {
            foreach (var z in App.CurrentFilterProfile.IncludeWhichZones.GetAsList()) //ZoneCheckBoxes.Items)
            {
                if (z.ZoneName == zone)
                {
                    if (z.IsIncluded != isIncluded)
                    {
                        z.IsIncluded = isIncluded;
                        App.CurrentFilterProfile.AControlChanged = true;
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
