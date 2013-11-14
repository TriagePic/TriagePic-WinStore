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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime; // for byte[].AsBuffer
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.UI;
using System.Globalization;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Animation;
// nah: using TP8.DataModel;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TP8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPageNew : TP8.Common.LayoutAwarePage
    {
        private string zoneSelected = "";

        public string MyZoneButtonItemWidth { get; set; }

        private TP_PatientReport pr = null; //pdi = null;

        private const string NOTES_TEXT_HINT = "Optional Notes"; // Must match string in XAML too
        private const string CAPTION_TEXT_HINT = "Optional Caption";  // Must match string in XAML too.  Temporary limit: 1 photo, 1 caption
        private const string EMBEDDED_FILE_PREFIX = "ms-appx:///Assets/";

        DispatcherTimer dt = null;
        private bool RollUpward = true;

        private ZoneButton[] zb = null;
        private int zbCount = 0;
        private bool firstTime = true;

        public BasicPageNew()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += VisualStateChanged;
            PatientIdTextBox.AddHandler(TappedEvent, new TappedEventHandler(PatientIdTextBox_Tapped), true);
            // pr == null in constructor
            pr = new TP_PatientReport(); // though elements may be null or empty
            App.MyAssert(App.CurrentPatient != null);

            if (String.IsNullOrEmpty(App.CurrentPatient.PatientID))
                App.CurrentPatient.PatientID = App.CurrentOtherSettings.CurrentNewPatientNumber; // reload.  Probably not good enough.
            App.MyAssert(!String.IsNullOrEmpty(App.CurrentPatient.PatientID)); // Should be loaded from OtherSettings.xml, or at generation associated with that.
            /// Maybe not: PatientIdTextBox.MaxLength = Convert.ToInt32(App.OrgPolicy.GetPatientIDMaxTextBoxLength());
            if (!String.IsNullOrEmpty(App.CurrentPatient.Zone)) // This test may need refinement
            {
                pr = App.CurrentPatient;
                // Move to LoadState, needs to be after InitiateZones call: LoadReportFieldsFromObject(pr); // may not be the way to go in long term
            }
            else
            {
                PatientIdTextBox.Text = App.CurrentPatient.PatientID; // cheap initialization
            }

            //Move to LoadState: InitiateZones();

            App.ViewedDisaster.Clear(); // a little housecleaning for the benefit of ViewEditReportPage
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0,0,0,0,500); // 500 milliseconds
            dt.Tick += dt_Tick;
            incrementPatientID.AddHandler(PointerPressedEvent, new PointerEventHandler(incrementPatientID_PointerPressed), true);
            decrementPatientID.AddHandler(PointerPressedEvent, new PointerEventHandler(decrementPatientID_PointerPressed), true);
            MyZoneButtonItemWidth = "140";

            ToolTip trafficTip = new ToolTip();
            trafficTip.Content = "Connection lag to TriageTrak.  Red if 3+ seconds (or failed), green if < 1/2 second";
            ToolTipService.SetToolTip(BlinkerTopRed, trafficTip);
            ToolTipService.SetToolTip(BlinkerMiddleYellow, trafficTip);
            ToolTipService.SetToolTip(BlinkerBottomGreen, trafficTip);
            ToolTip sendQueueTip = new ToolTip();
            sendQueueTip.Content = "Count of reports queued to send";
            ToolTipService.SetToolTip(CountInSendQueue, sendQueueTip);
        }

        private void InitiateZones()
        {
            ZoneChoiceComboSnapped.ItemsSource = App.ZoneChoices.GetZoneChoices();
            // Instantiate zone buttons:
            zbCount = App.ZoneChoices.GetZoneChoiceList().Count;
            App.MyAssert(zbCount > 0);
            zb = new ZoneButton[zbCount];
            int i = 0;
            string buttonName;
            foreach(TP_ZoneChoiceItem zci in App.ZoneChoices.GetZoneChoiceList())
            {
                if (String.IsNullOrEmpty(zci.ButtonName))
                    continue;
                buttonName = "zoneButton"+zci.ButtonName.Replace(" ","");
                zb[i] = new ZoneButton(buttonName, (Color)zci.ColorObj, zci.ButtonName);
                zb[i].Click += Zone_Click;
                ToolTip tt = new ToolTip();
                tt.Content = zci.Meaning;
                zb[i].SetToolTip(tt);
                ZoneButtons.Items.Add(zb[i]);

                /* DOESNT WORK... BUT MAYBE BECAUSE IN CONSTRUCTOR?
                // Make widths narrower for filled state:
                ObjectAnimationUsingKeyFrames anim = new ObjectAnimationUsingKeyFrames();
                DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame();
                kf.KeyTime = TimeSpan.FromSeconds(0);
                //kf.Value = 100;
                kf.SetValue(ZoneButton.ZoneButtonWidthProperty, 100);
                anim.KeyFrames.Add(kf);
                Storyboard.SetTargetName(anim, buttonName);
                // fails with exception when filled mode invoked: Storyboard.SetTargetProperty(anim, "(ZoneButton.ZoneButtonWidthProperty)");
                Storyboard.SetTargetProperty(anim, "ZoneButtonWidth"); // doesn't throw exception but doesn't change width either.  Doesn't seem to call setter
                Filled.Storyboard.Children.Add(anim);
                */
                i++;
            }

            Zone_Clear(); // disables SendButton too
        }

        private void VisualStateChanged(object sender, WindowSizeChangedEventArgs e)
        {
            string visualState = DetermineVisualState(ApplicationView.Value);
            if (visualState == "Filled")
            {
                MyZoneButtonItemWidth = "110";  // Can't change button width directly in XAML, because its set by ItemWidth in template, so not allowed.
            }
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
            InitiateZones();
            if (!String.IsNullOrEmpty(App.CurrentPatient.Zone)) // This test may need refinement
                LoadReportFieldsFromObject(pr);
            pageSubtitle.Text = " " + App.CurrentDisaster.EventName; // TO DO: binding in XAML instead of here?  Add space to separate from icon
            if(App.CurrentDisaster.TypeIconUri.Length > EMBEDDED_FILE_PREFIX.Length)
            {
                eventTypeImage.Source = new BitmapImage(new Uri(App.CurrentDisaster.TypeIconUri));
#if MAYBE
                string path = App.CurrentDisasterEventTypeIcon.Substring(prefix.Length);
                path += Windows.ApplicationModel.Package.Current.InstalledLocation.ToString();
                BitmapImage img = new BitmapImage();
                var uri = new Uri(path);
                img.UriSource = uri;
                eventTypeImage.Source = img;
#endif
                // Tried XAML binding, didn't work:                 <Image x:Name="eventTypeImage" Source="{Binding CurrentDisasterEventTypeIcon}" Width="40" VerticalAlignment="Top"/>
                MyZoneButtonItemWidth= "140";
            }

            UpdateImageLoad();
            if (firstTime)
            {
                firstTime = false;
                UpdateStoryBoard();
            }
        }



        private void UpdateStoryBoard()
        {
            for(int i = 0; i < zbCount; i++)
            {
                // Make widths narrower for filled state:
                ObjectAnimationUsingKeyFrames anim = new ObjectAnimationUsingKeyFrames();
                DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame();
                kf.KeyTime = TimeSpan.FromSeconds(0);
                //kf.Value = 100;
                kf.SetValue(ZoneButton.ZoneButtonWidthProperty, 100);
                anim.KeyFrames.Add(kf);
                Storyboard.SetTargetName(anim, zb[i].Name);
                // fails with exception when filled mode invoked: Storyboard.SetTargetProperty(anim, "(ZoneButton.ZoneButtonWidthProperty)");
                Storyboard.SetTargetProperty(anim, "ZoneButtonWidth"); // doesn't throw exception but doesn't change width either.  Doesn't seem to call setter
                Filled.Storyboard.Children.Add(anim);
            }
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

        private void UpdateImageLoad()
        {
#if PROBLEM
            if (App.CurrentPatient.ImageEncoded.Length > 0)
            {
                pdi.ImageEncoded = App.CurrentPatient.ImageEncoded;
                patientImageTextOverlay.Text = "";
                //patientImage = new Image();
                BitmapImage img = new BitmapImage();
                img = pdi.FormatImageUnencoded();
                patientImage.Source = img;
                //this worked: patientImage.Source = new BitmapImage(new Uri("ms-appx:///Assets/LightGray.png", UriKind.Absolute)); // pdi.FormatImageUnencoded();
                patientImageSnapped.Source = patientImage.Source;
            }
#endif
            RefreshPatientImageTextOverlay();
            RefreshPatientImageSource();
            EnableSendButtonIfPossible();
        }

        private void RefreshPatientImageTextOverlay()
        {
            if (App.CurrentPatient.ImageWriteableBitmap == null)
                SetPatientImageTextOverlay("No photo.  Click here for webcam.");
            else
                SetPatientImageTextOverlay("");
        }

        private void RefreshPatientImageSource()
        {
            if (App.CurrentPatient.ImageWriteableBitmap == null)
                SetPatientImageSource(new BitmapImage(new Uri(EMBEDDED_FILE_PREFIX + "SplashCameraLogo(300x300).png", UriKind.Absolute)));
            else
                SetPatientImageSource(App.CurrentPatient.ImageWriteableBitmap);
        }

        #region Multiple_UI_Controls_For_Same_Logical_Value
        private void SetPatientImageTextOverlay(string s)
        {
            patientImageTextOverlayLandscape.Text =
            patientImageTextOverlaySnapped.Text =
            patientImageTextOverlayPortrait.Text = s;
        }

        private void SetPatientImageSource(ImageSource s)
        {
            patientImageLandscape.Source =
            patientImageSnapped.Source =
            patientImagePortrait.Source = s;
        }

        private void SetFirstNameTextBox(string s)
        {
            FirstNameTextBox.Text =
            FirstNameTextBoxPortrait.Text = s;
        }

        private void SetLastNameTextBox(string s)
        {
            LastNameTextBox.Text =
            LastNameTextBoxPortrait.Text = s;
        }

        private string SyncAndGetFirstNameTextBox()
        {
            // Hmm, in Win 8.1, there's "Portrait" and "DefaultLayout" and page SizeChanged event handler
            string visualState = DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": FirstNameTextBox.Text = FirstNameTextBoxPortrait.Text; break;
                //case "Filled":
                //case "FullScreenLandscape":
                //case "Snapped":
                default: FirstNameTextBoxPortrait.Text = FirstNameTextBox.Text; break;
            }
            // All sync'd
            return FirstNameTextBox.Text;
        }

        private string SyncAndGetLastNameTextBox()
        {
            string visualState = DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": LastNameTextBox.Text = LastNameTextBoxPortrait.Text; break;
                //case "Filled":
                //case "FullScreenLandscape":
                //case "Snapped":
                default: LastNameTextBoxPortrait.Text = LastNameTextBox.Text; break;
            }
            // All sync'd
            return LastNameTextBox.Text;
        }

        private void SetCaptionTextBox(string s)
        {
            Caption.Text = CaptionPortrait.Text = s;
        }

        private string SyncAndGetCaptionTextBox()
        {
            string visualState = DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": Caption.Text = CaptionPortrait.Text; break;
                //case "Filled":
                //case "FullScreenLandscape":
                //case "Snapped":
                default: CaptionPortrait.Text = Caption.Text; break;
            }
            // All sync'd
            return Caption.Text;
        }

        private void GiveCaptionFocus()
        {
            string visualState = DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": CaptionPortrait.Focus(FocusState.Programmatic); break;
                //case "Filled":
                //case "FullScreenLandscape":
                //case "Snapped":
                
                default: Caption.Focus(FocusState.Programmatic); break;
            }
        }
/* TO DO:
        private void SetCheckBoxPracticeMode(bool o)
        {
            CheckBoxPracticeModeLandscape.IsChecked =
            CheckBoxPracticeModePortrait.IsChecked =
            CheckBoxPracticeModeSnapped.IsChecked = o;
        }
*/
        #endregion

#if DIDNT_WORK_CUZ_DIALOG_LIMITED_TO_3_BUTTONS
        private async Task ConfirmOrReviseGender()
        {
            if ((bool)CheckBoxMale.IsChecked && !(bool)CheckBoxFemale.IsChecked)
                return;
            if (!(bool)CheckBoxMale.IsChecked && (bool)CheckBoxFemale.IsChecked)
                return;
            // Only alert about uncommon cases
            string message =
                "Usually one gender checkbox is checked.  Please confirm or revise your choice:\n";

            var md = new MessageDialog(message);
            md.Commands.Add(new UICommand("Male",                           (UICommandInvokedHandler) => { CheckBoxMale.IsChecked = true;  CheckBoxFemale.IsChecked = false; }));
            md.Commands.Add(new UICommand("Female",                         (UICommandInvokedHandler) => { CheckBoxMale.IsChecked = false; CheckBoxFemale.IsChecked = true;  }));
            md.Commands.Add(new UICommand("Unknown (neither box checked)",  (UICommandInvokedHandler) => { CheckBoxMale.IsChecked = false; CheckBoxFemale.IsChecked = false; }));
            md.Commands.Add(new UICommand("Complex Gender (both checked)",  (UICommandInvokedHandler) => { CheckBoxMale.IsChecked = true;  CheckBoxFemale.IsChecked = true;  }));
            if ((bool)CheckBoxMale.IsChecked && (bool)CheckBoxFemale.IsChecked)
                md.DefaultCommandIndex = 3; // Complex
            else
                md.DefaultCommandIndex = 2; // Unknown
            await md.ShowAsync();
        }

        private async Task ConfirmOrReviseAgeGroup()
        {
            if ((bool)CheckBoxAdult.IsChecked && !(bool)CheckBoxPeds.IsChecked)
                return;
            if (!(bool)CheckBoxAdult.IsChecked && (bool)CheckBoxPeds.IsChecked)
                return;
            // Only alert about uncommon cases
            string message =
                "Usually one age group checkbox is checked.  Please confirm or revise your choice:\n";

            var md = new MessageDialog(message);
            md.Commands.Add(new UICommand("Adult",                                  (UICommandInvokedHandler) => { CheckBoxAdult.IsChecked = true;  CheckBoxPeds.IsChecked = false; }));
            md.Commands.Add(new UICommand("Pediatric / Youth",                      (UICommandInvokedHandler) => { CheckBoxAdult.IsChecked = false; CheckBoxPeds.IsChecked = true;  }));
            md.Commands.Add(new UICommand("Unknown (neither box checked)",          (UICommandInvokedHandler) => { CheckBoxAdult.IsChecked = false; CheckBoxPeds.IsChecked = false; }));
            md.Commands.Add(new UICommand("Other (e.g., Expectant) (both checked)", (UICommandInvokedHandler) => { CheckBoxAdult.IsChecked = true;  CheckBoxPeds.IsChecked = true;  }));
            if ((bool)CheckBoxAdult.IsChecked && (bool)CheckBoxPeds.IsChecked)
                md.DefaultCommandIndex = 3; // Other
            else
                md.DefaultCommandIndex = 2; // Unknown
            await md.ShowAsync();
        }
#endif
        /// <summary>
        /// Complains if user made unusual choice with sex checkboxes.
        /// </summary>
        /// <returns>true to continue, false to wait for adjustment</returns>
        private async Task<bool> ConfirmOrReviseGender()
        {
            if ((bool)CheckBoxMale.IsChecked && !(bool)CheckBoxFemale.IsChecked)
                return true;
            if (!(bool)CheckBoxMale.IsChecked && (bool)CheckBoxFemale.IsChecked)
                return true;
            // Only alert about uncommon cases
            string message =
                "Usually one gender checkbox is checked.  Please confirm or revise your choice.\n\n" +
                "Choices are Male, Female, Unknown (neither box checked), or Complex Gender (both boxes checked).";

            bool results = true;
            var md = new MessageDialog(message);
            md.Commands.Add(new UICommand("Confirm", (UICommandInvokedHandler) => { results = true; }));
            md.Commands.Add(new UICommand("Let Me Revise", (UICommandInvokedHandler) => { results = false; }));
            await md.ShowAsync();
            return results;
        }

        /// <summary>
        /// Complains if user made unusual choice with age group checkboxes.
        /// </summary>
        /// <returns>true to continue, false to wait for adjustment</returns>
        private async Task<bool> ConfirmOrReviseAgeGroup()
        {
            if ((bool)CheckBoxAdult.IsChecked && !(bool)CheckBoxPeds.IsChecked)
                return true;
            if (!(bool)CheckBoxAdult.IsChecked && (bool)CheckBoxPeds.IsChecked)
                return true;
            // Only alert about uncommon cases

            string message =
                "Usually one age group checkbox is checked.  Please confirm or revise your choice.\n\n" +
                "Choices are Adult, Pediatric/Youth, Unknown (neither box checked), or Other (e.g., Expectant) (both boxes checked).";

            bool results = true;
            var md = new MessageDialog(message);
            md.Commands.Add(new UICommand("Confirm", (UICommandInvokedHandler) => { results = true; }));
            md.Commands.Add(new UICommand("Let Me Revise", (UICommandInvokedHandler) => { results = false; }));
            await md.ShowAsync();
            return results;
        }

        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            App.MyAssert(App.PatientDataGroups != null);
            if (!await ConfirmOrReviseGender())
                return;
            if (!await ConfirmOrReviseAgeGroup())
                return;

            progressBarSending.Visibility = Visibility.Visible;

            if (Notes.Text == NOTES_TEXT_HINT) // Must match string in XAML too
                Notes.Text = "";
            if (SyncAndGetCaptionTextBox() == CAPTION_TEXT_HINT) // CAPTION_TEXT_HINT must match string in XAML too
                SetCaptionTextBox("");

            await SaveReportFieldsToObject(pr, "Q"); // IntendToSend
            TP_PatientReport pr2 = new TP_PatientReport(pr); // create a new object for each report, that will be passed through the queue
            // and be reference from the _outbox and _allstations lists.
            await pr2.DoSendEnqueue(true); // cleanup image fields, generate lp2 content, save it as file, then call SendQueue.Add, 

            // Move to later: await pr.DoSendPart2(); // Update outbox and allstations in both memory and local storage.  As well as sorted/filtered version in memory

            Clear_Entry_Click(sender, e); // Also increments patient ID
            // Save incremented value, with prefix removed.  Remembers it to OtherSettings.xml, and clears App.CurrentPatient:
            await pr2.DoSendPart3(PatientIdTextBox.Text.Remove(0, App.OrgPolicy.OrgPatientIdPrefixText.Length));


#if DIDNT_WORK
            double op = 100.0;
            while(true)
            {
                op -= 0.00001;
                LastSentMsg.Opacity = op;
                if(op <= 0.0)
                    break;
            }
            LastSentMsg.Opacity = 100.0;
#endif

            Int32 count;
            while (true)
            {
                count = TP_SendQueue.reportsToSend.Count();
                if (count > 0)
                    LastSentMsg.Text = "Reports waiting to send:  " + count;
                else
                {
                    progressBarSending.Visibility = Visibility.Collapsed;
                    LastSentMsg.Text = "Sent";
                }

                await Task.Delay(2000); // see message for 2 seconds
                if (count == 0 || count <= TP_SendQueue.reportsToSend.Count()) // if count doesn't decrease after 2 seconds, then don't wait further in this loop
                    break;
            }
            progressBarSending.Visibility = Visibility.Collapsed;
            // Maybe not: if(!LastSentMsg.Text.StartsWith("Reports waiting to send")) // Let count of waiting reports persist
            LastSentMsg.Text = "";
        }



        /// <summary>
        /// Identical to code in ViewEditReportPage
        /// </summary>
        /// <param name="pdi_"></param>
        private async Task SaveReportFieldsToObject(TP_PatientReport pr_, string IntendToSend)
        {
            if ((bool)CheckBoxAdult.IsChecked && (bool)CheckBoxPeds.IsChecked)
                pr_.AgeGroup = "Other Age Group (e.g., Expectant)"; // pregnant
            else if ((bool)CheckBoxAdult.IsChecked)
                pr_.AgeGroup = "Adult";
            else if ((bool)CheckBoxPeds.IsChecked)
                pr_.AgeGroup = "Youth";
            else
                pr_.AgeGroup = "Unknown Age Group";

            //string _gender;
            if ((bool)CheckBoxMale.IsChecked && (bool)CheckBoxFemale.IsChecked)
                pr_.Gender = "Complex Gender"; // pregnant
            else if ((bool)CheckBoxMale.IsChecked)
                pr_.Gender = "Male";
            else if ((bool)CheckBoxFemale.IsChecked)
                pr_.Gender = "Female";
            else
                pr_.Gender = "Unknown";

            // User editable fields:
            pr_.FirstName = SyncAndGetFirstNameTextBox();
            pr_.LastName = SyncAndGetLastNameTextBox();
            pr_.Comments = Notes.Text;
            // TO DO - captions
            // Other fields:
/* Now done by separate GetOrgAndDeviceData() call:
            CultureInfo provider = CultureInfo.InvariantCulture;
            pdi_.WhenLocalTime = (DateTimeOffset.Now).ToString("yyyy-MM-dd HH:mm:ss K", provider);
            pdi_.Timezone = GetTimeZoneAbbreviation(); */
            pr_.SentCode = IntendToSend; //"Y";
            pr_.PatientID = PatientIdTextBox.Text; // includes prefix
            pr_.Zone = zoneSelected;
            pr_.nPicCount = 0;  // Actual 0 or 1 determination will be done at beginning of WriteXML
/* Now done by separate GetOrgAndDeviceData() call:
            pdi_.EventShortName = App.CurrentDisaster.EventShortName;
            pdi_.EventName = App.CurrentDisaster.EventName; // w/o suffix
            pdi_.EventType = App.CurrentDisaster.EventType; // can be used to create suffix */
            pr_.ImageName = pr_.FormatImageName();
            pr_.ImageWriteableBitmap = App.CurrentPatient.ImageWriteableBitmap;
            pr_.ImageEncoded = await pr_.FormatImageEncoded(); // derive base64 from pdi.ImageWriteableBitmap
            pr_.ImageCaption = SyncAndGetCaptionTextBox();
        }

        /// <summary>
        /// Return an abbreviation for the current system timezone, reflecting whether daylight-savings and standard time.
        /// </summary>
        /// <returns></returns>
        public string GetTimeZoneAbbreviation()
        {
            // There are no international standards for timezone abbreviations.
            // The usual recommended hack (at least for US timezones) is to take the first letter of each word as done below.
            string tzname;
            // Note that Win8 does not support Win7's System.TimeZones.
            // However, let's try it with TimeZoneInfo instead.  MSDN says TimeZone.CurrentTimeZone in Win 7 = TimeZoneInfo.Local
            // (Not used here but also of interest:  The WinRTTimeZones code, available from NuGet, has a TimeZones objects that wraps Win32 time features supported in Win 8)
            // See also for other timezone issues: "Programming Windows, Version 6", Petzold, Chapter 15.
            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                tzname = TimeZoneInfo.Local.DaylightName;
            else
                tzname = TimeZoneInfo.Local.StandardName;
            // Hack to convert time zone name to abbreviation by just taking the first letter of each word.
            // This will not work 100% for time zones worldwide.  Results are typically 3 letter (e.g., always if US) or 4 letter.
            // For a somewhat full list, see www.timeanddate.com/lbirary/abbreviations/timezones/
            string[] words = tzname.Split(" ".ToCharArray());
            string tzabbr = "";
            foreach (string word in words)
                tzabbr += word[0];
            return tzabbr;
        }

        /// <summary>
        /// Identical to code in ViewEditReport
        /// </summary>
        /// <param name="pr_"></param>
        private void LoadReportFieldsFromObject(TP_PatientReport pr_)
        {
            // Already cleared controls when we get here.
            SetFirstNameTextBox(pr_.FirstName);
            SetLastNameTextBox(pr_.LastName);
            PatientIdTextBox.Text = pr_.PatientID;
            App.MyAssert(CheckBoxAdult.IsChecked == false);
            App.MyAssert(CheckBoxPeds.IsChecked == false);
            switch (pr_.AgeGroup)
            {
                case "Adult":
                    CheckBoxAdult.IsChecked = true; break;
                case "Pediatric":
                    CheckBoxPeds.IsChecked = true; break;
                case "Unknown":
                default: break;
            }
            App.MyAssert(CheckBoxMale.IsChecked == false);
            App.MyAssert(CheckBoxFemale.IsChecked == false);
            switch (pr_.Gender)
            {
                case "Male":
                    CheckBoxMale.IsChecked = true; break;
                case "Female":
                    CheckBoxFemale.IsChecked = true; break;
                case "Complex":
                    CheckBoxMale.IsChecked = CheckBoxFemale.IsChecked = true; break;
                case "Unknown":
                default: break;
            }
            zoneSelected = pr_.Zone;
            ZoneSelect(zoneSelected);

            // Empty fields with hints are already showing hints.  But should we override:
            // We have earlier, during clearing, moved focus away from Notes and Captions.
            if (pr_.Comments != "")
            {
                Notes.Text = pr_.Comments;
                Notes.Focus(FocusState.Programmatic); // Will repaint font as black
            }
            if (pr_.ImageCaption != "")
            {
                SetCaptionTextBox(pr_.ImageCaption);
                GiveCaptionFocus();
            }
            /* TO DO.  Note - consider moving practice mode checkbox to settings:
            if (pr_.PatientID.StartsWith("Prac"))
                SetCheckBoxPracticeMode(true); */
        }

        /// <summary>
        /// To do: Identical to code in ViewEditReport
        /// </summary>
        /// <param name="zone"></param>
        private void ZoneSelect(string zone)
        {
            // All checkmarks should be off when this is called
            foreach (ZoneButton zb in ZoneButtons.Items)
            {
                if (zb.ContentAutoForeground == zone)
                {
                    zb.CheckMarkVisibility = Visibility.Visible;
                    return;
                }
            }
            zoneSelected = "";
            App.MyAssert(false); // really true?
        }

        private void Zone_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            Zone_Clear();
            zoneSelected = clickedButton.Content.ToString();
            if (!App.ZoneChoices.ZoneVerify(zoneSelected))
            {
                App.MyAssert(false); // programmer screwup
                zoneSelected = "";
                return;
            }
            ZoneSelect(zoneSelected);
            // Next line differs from ViewEditReportPage:
            if (zoneSelected != "" && pr.ImageName != "" && pr.ImageName.Contains("ZoneNotSetYet"))
                pr.ImageName.Replace("ZoneNotSetYet", zoneSelected);
            EnableSendButtonIfPossible();
        }

        /// <summary>
        /// Clears the checkmarks off the zone buttons, and disables the SendButton too
        /// </summary>
        private void Zone_Clear()
        {
            // Clear checkmark from all buttons:
            if(ZoneButtons.Items != null)
                foreach (ZoneButton zb in ZoneButtons.Items)
                    zb.CheckMarkVisibility = Visibility.Collapsed;

            zoneSelected = "";
            ZoneChoiceComboSnapped.SelectedIndex = 0;
            DisableSendButton();
        }

        private void ZoneChoiceComboSnapped_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            zoneSelected = ZoneChoiceComboSnapped.SelectedItem.ToString();
        }

        private void EnableSendButtonIfPossible()
        {
            if (zoneSelected != "" && PatientIdTextBox.Text != "" &&
                PatientIdTextBox.Text != App.OrgPolicy.OrgPatientIdPrefixText) // not prefix alone
                EnableSendButton();
        }

        #region Send_Button_Common_To_NewReport_and_ViewEditReport

        private void SetSendButtonIsEnabled(bool o)
        {
            SendButtonLandscape.IsEnabled =
            SendButtonPortrait.IsEnabled =
            SendButtonSnapped.IsEnabled = o;
        }

        private void SetSendButtonColor(SolidColorBrush b)
        {
            outerCircleLandscape.Stroke = b;
            outerCirclePortrait.Stroke = b;
            outerCircleSnapped.Stroke = b;
            SendButtonCaptionLandscape.Foreground = b; // no caption in other modes I guess
        }


        private void EnableSendButton()
        {
            SetSendButtonIsEnabled(true);
            SolidColorBrush white = new SolidColorBrush();
            white.Color = Colors.White;
            SetSendButtonColor(white);
        }

        private void DisableSendButton()
        {
            SetSendButtonIsEnabled(false);
            SolidColorBrush gray = new SolidColorBrush();
            gray.Color = Colors.Gray;
            SetSendButtonColor(gray);
        }
        #endregion

        private async void patientImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await SaveReportFieldsToObject(App.CurrentPatient, ""); // Not yet sent
            this.Frame.Navigate(typeof(WebcamPage));
        }

        private async void Pick_Picture_Click(object sender, RoutedEventArgs e)
        {
            // This method loads an image into the WriteableBitmap using the SetSource method
            // Based on code from MS Win 8 sample "XAML Image Sample", function LoadImageUsingSetSource_Click

            FileOpenPicker picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".png"); // Might not work
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".bmp"); // Might not work

            StorageFile file = await picker.PickSingleFileAsync();
            if (file == null)
                return; // Ensure a file was selected

            if(!String.IsNullOrEmpty(App.CurrentPatient.ImageEncoded)) // old picture
                App.CurrentPatient.ImageEncoded = ""; // throw it away

            if (App.CurrentPatient.ImageWriteableBitmap == null)
                App.CurrentPatient.ImageWriteableBitmap = new WriteableBitmap(1, 1); // initial size doesn't matter

            // Set the source of the WriteableBitmap to the image stream.
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                if(fileStream.Size == 0)
                    return; // empty
                try
                {
                    // Glenn adds extra code to get image-encoded file stream and save it as base64
                    // Adapted from "File Manipulation in Windows 8 Store Apps" by Sumit Maitra, 10/13/2012, www.dotnetcurry.com/SowArticle.aspx?ID=838

                    byte[] array3 = new byte[fileStream.Size];
                    IBuffer content = await fileStream.ReadAsync(
                        array3.AsBuffer(0, (int)fileStream.Size), // byte[].AsBuffer needs using ...WindowsRuntime added (and VS2010 doesn't prompt you for it)
                        (uint)fileStream.Size,
                        InputStreamOptions.Partial);
                    App.CurrentPatient.ImageEncoded = Convert.ToBase64String(content.ToArray());
                    // now do viewable image
                    fileStream.Seek(0);
                    await App.CurrentPatient.ImageWriteableBitmap.SetSourceAsync(fileStream);  // I guess jpeg-decoding (or whatever) is happening here.
                }
                catch (TaskCanceledException)
                {
                    // The async action to set the WriteableBitmap's source may be canceled if the user clicks the button repeatedly
                    return;
                }
            }
            App.CurrentPatient.ImageCaption = "Original filename: " + file.DisplayName;
            this.Caption_SetTextProgrammatically(App.CurrentPatient.ImageCaption);
            App.CurrentPatient.ImageName = App.CurrentPatient.FormatImageName();
            //done later: App.CurrentPatient.ImageEncoded = await App.CurrentPatient.FormatImageEncoded();
            App.CurrentPatient.ImageEncoded = ""; // clear out any color swatch beginning with Assets, we have a real image
            //this.Frame.Navigate(this.Frame.Content.GetType()); // Kludge to refresh page, call loadstate
            //this.Frame.GoBack(); // Remove extra call from back stack
            // Instead, repeat code from LoadState:
            UpdateImageLoad();
        }
        
        // Bottom app bar:
        private void Clear_Entry_Click(object sender, RoutedEventArgs e)
        {
            ClearEntryIncrementPatientID();
        }

        /// <summary>
        /// Differs slightly between NewReport and ViewEditReport
        /// </summary>
        private void ClearEntryIncrementPatientID()
        {
            ClearEntryExceptPatientID();
            //Assume auto-increment:
            PatientIdTextBox.Text = IncrementPatientID();
        }

        /// <summary>
        /// Same in NewReport and ViewEditReport
        /// </summary>
        private void ClearEntryExceptPatientID()
        {
            // Invoked by what is now called the "Delete" app bar icon, originally called "Clear"
            SetFirstNameTextBox("");
            SetLastNameTextBox("");
            Zone_Clear();  // disables SendButton too
            CheckBoxAdult.IsChecked = false;
            CheckBoxPeds.IsChecked = false;
            CheckBoxMale.IsChecked = false;
            CheckBoxFemale.IsChecked = false;
            // Don't clear Practice checkbox, it should be sticky
            // Toggle focus states so that notes, caption lose focus, which reinserts gray hint text
            Notes.Text = "";
            Notes.Focus(FocusState.Programmatic);
            SetCaptionTextBox("");
            GiveCaptionFocus();
            PatientIdTextBox.Focus(FocusState.Programmatic);
            PatientIdTextBox.IsEnabled = false; // toggle to suppress virtual keyboard popup
            PatientIdTextBox.IsEnabled = true;
            pr.Clear();
            App.CurrentPatient.Clear();
            App.MyAssert(App.CurrentPatient.ImageWriteableBitmap == null);
            RefreshPatientImageTextOverlay();
            RefreshPatientImageSource();
       }

        #region AppBars
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

//        private void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
//        {
//            this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
//        }

        private void AllStations_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "AllStations"); // Defined in SampleDataSource.cs
        }

        private void Outbox_Click(object sender, RoutedEventArgs e) // at moment, List icon on nav bar
        {
            this.Frame.Navigate(typeof(SplitPage), "Outbox"); // Defined in SampleDataSource.cs
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "Statistics"); // Defined in SampleDataSource.cs
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
        }

        private async void Webcam_Click(object sender, RoutedEventArgs e)
        {
            await SaveReportFieldsToObject(App.CurrentPatient, "");
            this.Frame.Navigate(typeof(WebcamPage), e);
        }
        #endregion

        private void Notes_GotFocus(object sender, RoutedEventArgs e)
        {
            if (Notes.Text == NOTES_TEXT_HINT)
                Notes.Text = "";
            SolidColorBrush Brush1 = new SolidColorBrush();
            Brush1.Color = Windows.UI.Colors.Black;
            Notes.Foreground = Brush1;
        }

        private void Notes_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Notes.Text == String.Empty)
            {
                Notes.Text = NOTES_TEXT_HINT;
                SolidColorBrush Brush2 = new SolidColorBrush();
                Brush2.Color = Windows.UI.Colors.Gray;  // Must match color in XAML
                Notes.Foreground = Brush2;
            }
        }


        private void Caption_SetTextColor(Windows.UI.Color c)
        {
            SolidColorBrush Brush1 = new SolidColorBrush();
            Brush1.Color = Colors.Black;
            Caption_SetTextColor(Brush1);
        }

        private void Caption_SetTextColor(SolidColorBrush b)
        {
            Caption.Foreground = CaptionPortrait.Foreground = b;
        }

        private void Caption_SetTextProgrammatically(string s)
        {
            if (s == String.Empty)
            {// same as LostFocus - assuming caption doesn't have focus here... almost always true
                Caption_SetTextColor(Colors.Gray); // Must match color in XAML
                SetCaptionTextBox(CAPTION_TEXT_HINT);
            }
            else
            {
                Caption_SetTextColor(Colors.Black);
                SetCaptionTextBox(s);
            }
        }


        private void Caption_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SyncAndGetCaptionTextBox() == CAPTION_TEXT_HINT)
                SetCaptionTextBox("");
            Caption_SetTextColor(Colors.Black);
        }

        private void Caption_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SyncAndGetCaptionTextBox() == String.Empty)
            {
                SetCaptionTextBox(CAPTION_TEXT_HINT);
                Caption_SetTextColor(Colors.Gray);// Must match color in XAML
            }
        }

        private void Caption_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TO DO?  Differs on purpose from ViewEdit
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TO DO?  Differs on purpose from ViewEdit
        }

        private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Probably don't need to update on each keystroke: pdi.FirstName = FirstNameTextBox.Text;
        }

        private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // TO DO?  Differs on purpose from ViewEdit
        }

        // Note that you must explicitly add a handler to make this work with a textbox:
        private void PatientIdTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (App.OrgPolicy.OrgPatientIdFixedDigits != -1)
            {
                PatientIdTextBox.Select(PatientIdTextBox.Text.Length, 0); // Cursor to far right
            }
        }

        private void PatientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            int ss = PatientIdTextBox.SelectionStart;
            //int sa = PatientIdTextBox.SelectionLength;
            string temp = App.OrgPolicy.ForceValidFormatID(PatientIdTextBox.Text);
            if (temp != PatientIdTextBox.Text)
                PatientIdTextBox.Text = temp; // just preventing endless loop of calls here
            int length = PatientIdTextBox.Text.Length;
            // if fixed digits, generally edit calculator style, with roll-in from right
            if (App.OrgPolicy.OrgPatientIdFixedDigits != -1)
            {
                PatientIdTextBox.Select(length, 0); // Cursor to far right
                //PatientIdTextBox.Select(ss,0); // restore.
            }
            else
            {
                if (ss < length && temp != App.OrgPolicy.OrgPatientIdPrefixText + "0") // If error, number is reset to zero.  Move cursor to far right
                    PatientIdTextBox.Select(ss, 0);
                else
                    PatientIdTextBox.Select(length, 0); // Cursor to far right
            }
            EnableSendButtonIfPossible();
        }

        private void PatientIdTextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void patientImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            string errMsg = "Couldn't load image.\n" + e.ErrorMessage;
            MessageDialog dlg = new MessageDialog(errMsg);
            var t = dlg.ShowAsync();  // assign to t to suppress compiler warning
        }

        private void patientImage_ImageOpened(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Width: {0}, Height: {1}", ((BitmapImage)patientImage.Source).PixelWidth, ((BitmapImage)patientImage.Source).PixelHeight);
        }

        private void decrementPatientID_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DecrementPatientID();
            dt.Stop(); //prevent runaways
        }

        private void incrementPatientID_Tapped(object sender, TappedRoutedEventArgs e)
        {
            IncrementPatientID();
            dt.Stop(); //prevent runaways
        }

        private string IncrementPatientID() // first draft.  Assumes suffix length is 4.  Caller must save results to PatientIdTextBox
        {
            RollUpward = true;
            return BumpAndStuffPatientID();
        }

        private string DecrementPatientID() // first draft  Assumes suffix length is 4.  Caller must save results to PatientIdTextBox
        {
            RollUpward = false;
            return BumpAndStuffPatientID();
        }

        private string BumpAndStuffPatientID()
        {
            PatientIdTextBox.Text = App.OrgPolicy.BumpPatientID(PatientIdTextBox.Text, RollUpward);
            return PatientIdTextBox.Text;
        }


        // ManipulationStareted & Completed not firing by default for button.  PointerReleased does; have to add handler for PointerPressed to fire too.

        public void dt_Tick(object sender, object e) // In Win8, 2nd arg has to be of type object, not EventArgs
        {
            BumpAndStuffPatientID();
        }

        private void incrementPatientID_PointerPressed(object sender, PointerRoutedEventArgs e) // doesn't typically fire from Button object, so added handler to here.
        {
            RollUpward = true;
            dt.Start();
        }

        private void incrementPatientID_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            dt.Stop();
        }

        private void decrementPatientID_PointerPressed(object sender, PointerRoutedEventArgs e) // doesn't typically fire from Button object, so added handler to here.
        {
            RollUpward = false;
            dt.Start();
        }

        private void decrementPatientID_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            dt.Stop();
        }


    }
}
