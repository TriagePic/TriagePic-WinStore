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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using System.Threading.Tasks;
using Windows.UI;
using System.Globalization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Animation;
using Win8.Controls;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TP8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPageViewEdit : TP8.Common.LayoutAwarePage
    {
        private string zoneSelected = "";

        public string MyZoneButtonItemWidth { get; set; }

        //App.CurrentPatient serves as the previousReport 
        private TP_PatientReport updatedReport = null;
        //TRIED, USELESS:  private bool suppressMarkingAsAltered = false;

        private const string NOTES_TEXT_HINT = "Optional Notes"; // Must match string in XAML too
        private const string CAPTION_TEXT_HINT = "Optional Caption";  // Must match string in XAML too.  Temporary limit: 1 photo, 1 caption
        private const string EMBEDDED_FILE_PREFIX = "ms-appx:///Assets/";

        DispatcherTimer dt = null;
        private bool RollUpward = true;

        // These NOT in NewReport too:
        private string ViewedDisasterEventTypeIcon;
        // MAYBE TO DO IN NEXT VERSION: Assist App.ReportAltered:
        // private bool PatientIdAltered;

        // Used with bottom app bar:
        private Popup discardMenuPopUp = null;
        private Popup whyDiscardedPopUp = null;
        private string uuid = "";

        public BasicPageViewEdit()
        {
            this.InitializeComponent();
            Window.Current.SizeChanged += VisualStateChanged;
            PatientIdTextBox.AddHandler(TappedEvent, new TappedEventHandler(PatientIdTextBox_Tapped), true);
            //previousPdi = new TP_PatientReport();
            updatedReport = new TP_PatientReport();
            App.MyAssert(App.CurrentPatient != null); // though elements may be null or empty
            // Some items in NewReport's constructor have been moved to LoadState() here.
            InitiateZones();
            dt = new DispatcherTimer();
            dt.Interval = new TimeSpan(0, 0, 0, 0, 500); // 500 milliseconds
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

            discardMenuPopUp = new Popup();
            //discardMenuPopUp.Closed += (o, e) => this.GetParentOfType<AppBar>().IsOpen = false; // When popup closes, close app bar too.
            //discardMenuPopUp.Closed += (o, e) => TopAppBar.IsOpen = false;
            //discardMenuPopUp.Closed += (o, e) => BottomAppBar.IsOpen = false;
        }

        private void InitiateZones()
        {
            ZoneChoiceComboSnapped.ItemsSource = App.ZoneChoices.GetZoneChoices();
            // Instantiate zone buttons:
            int count = App.ZoneChoices.GetZoneChoiceList().Count;
            App.MyAssert(count > 0);
            ZoneButton[] zb = new ZoneButton[count];
            int i = 0;
            string buttonName;
            foreach (TP_ZoneChoiceItem zci in App.ZoneChoices.GetZoneChoiceList())
            {
                if (String.IsNullOrEmpty(zci.ButtonName))
                    continue;
                buttonName = "zoneButton" + zci.ButtonName.Replace(" ", "");
                zb[i] = new ZoneButton(buttonName, (Color)zci.ColorObj, zci.ButtonName);
                zb[i].Click += Zone_Click;
                ToolTip tt = new ToolTip();
                tt.Content = zci.Meaning;
                zb[i].SetToolTip(tt);
                ZoneButtons.Items.Add(zb[i]);
                // Make widths narrower for filled state:  NOT WORKING YET
                ObjectAnimationUsingKeyFrames anim = new ObjectAnimationUsingKeyFrames();
                DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame();
                kf.KeyTime = TimeSpan.FromSeconds(0);
                kf.Value = 100;
                anim.KeyFrames.Add(kf);
                Storyboard.SetTargetName(anim, buttonName);
                Storyboard.SetTargetProperty(anim, "ZoneButtonWidth");
                Filled.Storyboard.Children.Add(anim);
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
        protected override async void LoadState(Object navigationParameter, Dictionary<String, Object> pageState) // Glenn added async here, might not turn out well
        {
            // Differs from NewReport:
            if ((string)navigationParameter == "pageViewEditReport")
            {
                // Coming from web cam.  We already have what we need in App.CurrentPatient
                //suppressMarkingAsAltered = false;
                updatedReport = App.CurrentPatient;
                await Load_Entry(updatedReport);
            }
            else
            {   // navigation parameter is the unique Id associated with search results in the the flip view
                //suppressMarkingAsAltered = true;
                ShowTitleAndSentTimeAsUnaltered(); // need this?
                ClearEntryAll(); //ClearEntryExceptPatientID(); // Will need more work when we're reloading state from suspend
                string UniqueID = (string)navigationParameter;

                bool foundPatient = false;
                foreach (var pr_ in App.PatientDataGroups.GetOutbox())
                {
                    if (pr_.WhenLocalTime == UniqueID)
                    {
                        foundPatient = true;
                        App.CurrentPatient = pr_;
                        updatedReport = pr_;
                        await Load_Entry(updatedReport);
                        break;
                    }
                }
                if (!foundPatient)
                    App.MyAssert(false);
            }
            if (App.ReportAltered)
                ShowTitleAndSentTimeAsAltered();

            pageSubtitle.Text = " " + App.ViewedDisaster.EventName; // App.CurrentDisasterEventName; // TO DO: binding in XAML instead of here?  Add space to separate from icon
            ViewedDisasterEventTypeIcon = App.ViewedDisaster.GetIconUriFromEventType();
            // Momentarily shift focus to notes and caption to force text font to correct color for contents
            Notes.Focus(FocusState.Programmatic);
            GiveCaptionFocus();
            PatientIdTextBox.Focus(FocusState.Programmatic);
            // In common with NewReport:
            if(ViewedDisasterEventTypeIcon.Length > EMBEDDED_FILE_PREFIX.Length)
            {
                eventTypeImage.Source = new BitmapImage(new Uri(ViewedDisasterEventTypeIcon));
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
            //suppressMarkingAsAltered = false;
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

        /// <summary>
        /// Specific to ViewEditReport, not in NewReport
        /// </summary>
        /// <param name="pr_"></param>
        private async Task/*void*/ Load_Entry(TP_PatientReport pr_)
        {
            Zone_Clear(); // disables SendButton too.  Unlike NewReport, which starts with no zone selected, we'll indicate a selected zone in a moment.
            // First do fields common to NewReport and ViewEditReport:
            LoadReportFieldsFromObject(pr_);
            // Then difference:
            string sentMsg = "";
            if (pr_.ObjSentCode.GetVersionCount() > 1)
                sentMsg = pr_.ObjSentCode.GetVersionSuffixFilenameForm().Substring(1);  // of form " Msg#2" with leading space, which is then removed

            if (pr_.ObjSentCode.IsDoneOK()) // most equivalent to: if (pr_.SentCode.StartsWith("Y"))
            {
                if (sentMsg == "")
                    LastSentMsg.Text = "When";
                else
                    LastSentMsg.Text = sentMsg;
                LastSentMsg.Text += " sent: " + pr_.WhenLocalTime + " " + pr_.Timezone;
            }
            else
            {
                if (sentMsg == "")
                    LastSentMsg.Text = "Not yet sent.";
                else
                    LastSentMsg.Text = sentMsg + " not yet sent.";
            }
            // TO DO: Make this smarter:
            //if (App.CurrentDisasterEventShortName != pr_.EventShortName)
            //    LastSentMsg.Text += "   Current Event Changed!";
            //App.CurrentDisasterEventShortName = pr_.EventShortName;
            //App.CurrentDisasterEventName = pr_.EventName; // w/o suffix
            //App.CurrentDisasterEventType = pr_.EventType; // can be used to create suffix
            App.ViewedDisaster.EventShortName   = pr_.EventShortName;
            App.ViewedDisaster.EventName = pr_.EventName;
            App.ViewedDisaster.EventType = pr_.EventType;
            var Event = new TP_EventsDataItem();
            Event.EventType = pr_.EventType;
            ViewedDisasterEventTypeIcon = Event.GetIconUriFromEventType();
            if (pr_.ImageWriteableBitmap == null)
            {
                if (pr_.ImageEncoded.StartsWith("Assets/")) // Might be Assets/NoPhotoBrown(C17819)(300x225).png
                    pr_.ImageEncoded = "";
                pr_.ImageWriteableBitmap = await CreateImageAsync(pr_.ImageEncoded);
            }
            App.CurrentPatient.ImageWriteableBitmap = pr_.ImageWriteableBitmap; // fake as if just came from web cam
        }

        public async static Task<WriteableBitmap> CreateImageAsync(string base64image)
        {
            // from social.msdn.microsoft.com ... /decoding-then-reencoding-an-image-from-server
            if(String.IsNullOrEmpty(base64image))
                return null;

            byte[] bytes = Convert.FromBase64String(base64image);
            var stream = new InMemoryRandomAccessStream();
            DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0));
            writer.WriteBytes(bytes);
            await writer.StoreAsync();
            stream.Seek(0);
            BitmapImage bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);
            WriteableBitmap bmp = new WriteableBitmap(bitmap.PixelHeight, bitmap.PixelWidth);
            stream.Seek(0);
            bmp.SetSource(stream);
            return bmp;
        }

        private void UpdateImageLoad()
        {
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
                SetPatientImageSource(new BitmapImage(new Uri(EMBEDDED_FILE_PREFIX + "PhotoBackgroundCameraLogo(300x300).png", UriKind.Absolute)));
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

        private void SetPatientImageIsTapEnabled(bool b)
        {
            patientImageLandscape.IsTapEnabled =
            patientImageSnapped.IsTapEnabled =
            patientImagePortrait.IsTapEnabled = b;
        }

        private void SetFirstNameTextBox(string s)
        {
            FirstNameTextBox.Text =
            FirstNameTextBoxPortrait.Text = s;
        }

        private void SetFirstNameTextBoxIsEnabled(bool b)
        {
            FirstNameTextBox.IsEnabled =
            FirstNameTextBoxPortrait.IsEnabled = b;
        }

        private void SetLastNameTextBox(string s)
        {
            LastNameTextBox.Text =
            LastNameTextBoxPortrait.Text = s;
        }

        private void SetLastNameTextBoxIsEnabled(bool b)
        {
            LastNameTextBox.IsEnabled =
            LastNameTextBoxPortrait.IsEnabled = b;
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
            Caption.Text =
            CaptionPortrait.Text = s;
        }

        private void SetCaptionTextBoxIsEnabled(bool b)
        {
            Caption.IsEnabled =
            CaptionPortrait.IsEnabled = b;
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
            // In first release, user cannot edit or change patient ID.
            // Send button should be disabled if no change was made, unless not yet sent [to do?]
            // Unlike with Win7, the PatientReport lists here can contain the same patientID multiple times, because the list will contain all messages, not just the latest.
            // New flag "superceded" keep track of that.
            // However, if there's an error, redo will use the same object, not a new one.
            App.MyAssert(App.PatientDataGroups != null);
            if (!await ConfirmOrReviseGender())
                return;
            if (!await ConfirmOrReviseAgeGroup())
                return;

            LastSentMsg.Text = "";
            //LastSentMsg.Visibility = Visibility.Collapsed;
            progressBarSending.Visibility = Visibility.Visible;

            if (Notes.Text == NOTES_TEXT_HINT) // Must match string in XAML too
                Notes.Text = "";
            if (SyncAndGetCaptionTextBox() == CAPTION_TEXT_HINT) // Must match string in XAML too
                SetCaptionTextBox("");

            App.MyAssert(updatedReport.ObjSentCode.IsValid());
            if (updatedReport.ObjSentCode.GetCodeWithoutVersion() == "Y")
            {
                TP_PatientReport updatedReportNewInstance = new TP_PatientReport(updatedReport); // create a new object for each report,
                // that will be passed through the queue and be reference from the _outbox and _allstations lists.
                await SendClickImpl(updatedReportNewInstance);
            }
            else
            {
                // "N" or error.  to do: better treatment for error cases "Xn".
                await SendClickImpl(updatedReport); // recycle existing object
            }

            // Replaced by UpdateSendHistory on dequeue: await updatedReport.DoSendPart2(); // Update outbox and allstations in both memory and local storage.  As well as sorted/filtered version in memory

            ClearEntryAll(); //Clear_Entry_Click(sender, e); // Unlike New Report, DOES NOT increment patient ID
            // Save incremented value, with prefix removed.  Remembers it to OtherSettings.xml, and clears App.CurrentPatient:
            // NO: await updatedReport.DoSendPart3(PatientIdTextBox.Text.Remove(0, App.OrgPolicy.OrgPatientIdPrefixText.Length));
            //Bad idea, the object is being enqueued: updatedReport.Clear(); // forget about this patient, start anew
            App.CurrentPatient.Clear(); // not a good idea to null pdi or CurrentPatient, since we're not reinstantiating it elsewhere.
            App.ReportAltered = false; // reset

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
            ShowTitleAndSentTimeAsUnaltered(); // reset fields from red to white, restore original title
            EntryEachIsEnabled(false); // disable entry fields
        }

        private async Task SendClickImpl(TP_PatientReport pr)
        {
            App.MyAssert(pr.ObjSentCode.IsValid());
            bool newPrObject = false;
            if (pr.ObjSentCode.GetCodeWithoutVersion() == "Y")
            {
                newPrObject = true;
                // Mark existing record as superceded (we can use copied fields in new record to get the parameter values we need)
                await App.PatientDataGroups.UpdateSendHistory(pr.PatientID, pr.ObjSentCode.GetVersionCount(), pr.SentCode, true /*superceded*/);
                pr.ObjSentCode.BumpVersion();
            }
            // else "N" or error.  to do: better treatment for error cases "Xn".

            pr.ObjSentCode.ReplaceCodeKeepSuffix("Q");
            await SaveReportFieldsToObject(pr);
            await pr.DoSendEnqueue(newPrObject); // cleanup image fields, generate lp2 content, save it as file, then call SendQueue.Add, 
        }


        /// <summary>
        /// Identical to code in NewReportPage.  When called, the PatientReport.SendCode has already been updated with the desired value.
        /// </summary>
        /// <param name="pr_"></param>
        private async Task SaveReportFieldsToObject(TP_PatientReport pr_) //, string IntendToSend)
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
            // Other fields:
            /* Now done by separate GetOrgAndDeviceData() call:
                        CultureInfo provider = CultureInfo.InvariantCulture;
                        pr_.WhenLocalTime = (DateTimeOffset.Now).ToString("yyyy-MM-dd HH:mm:ss K", provider);
                        pr_.Timezone = GetTimeZoneAbbreviation(); */
            //pr_.SentCode = IntendToSend; //"Q", eventually "Y".  Will have suffix if edit+resend
            pr_.PatientID = PatientIdTextBox.Text; // includes prefix
            pr_.Zone = zoneSelected;
            pr_.nPicCount = 0;  // Actual 0 or 1 determination will be done at beginning of WriteXML
            /* Now done by separate GetOrgAndDeviceData() call:
                        pr_.EventShortName = App.CurrentDisaster.EventShortName;
                        pr_.EventName = App.CurrentDisaster.EventName; // w/o suffix
                        pr_.EventType = App.CurrentDisaster.EventType; // can be used to create suffix */
            pr_.ImageName = pr_.FormatImageName();
            pr_.ImageWriteableBitmap = App.CurrentPatient.ImageWriteableBitmap;
            pr_.ImageEncoded = await pr_.FormatImageEncoded(); // derive base64 from updatedReport.ImageWriteableBitmap
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
        /// Identical to code in NewReport
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
            App.MyAssert(pr_.AgeGroup == "Youth" || pr_.AgeGroup == "Adult" || pr_.AgeGroup == "Unknown Age Group" || pr_.AgeGroup == "Other Age Group (e.g., Expectant)");
            switch (pr_.AgeGroup)
            {
                case "Adult":
                    CheckBoxAdult.IsChecked = true; break;
                case "Youth": //case "Pediatric":
                    CheckBoxPeds.IsChecked = true; break;
                case "Other Age Group (e.g., Expectant)":
                    CheckBoxAdult.IsChecked = CheckBoxPeds.IsChecked = true; break;
                case "Unknown Age Group":
                default: break;
            }
            App.MyAssert(CheckBoxMale.IsChecked == false);
            App.MyAssert(CheckBoxFemale.IsChecked == false);
            App.MyAssert(pr_.Gender == "Male" || pr_.Gender == "Female" || pr_.Gender == "Complex Gender" || pr_.Gender == "Unknown");
            switch (pr_.Gender)
            {
                case "Male":
                    CheckBoxMale.IsChecked = true; break;
                case "Female":
                    CheckBoxFemale.IsChecked = true; break;
                case "Complex Gender":
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

        /// <summary>
        /// Differs from NewReport by MarkAsAltered call
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
            MarkAsAltered(); // includes EnableSendButtonIfPossible();
        }

        /// <summary>
        /// Clears the checkmarks off the zone buttons, and disables the SendButton too
        /// </summary>
        private void Zone_Clear()
        {
            // Clear checkmark from all buttons:
            if (ZoneButtons.Items != null)
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

        private void ZonesEachIsEnabled(bool b)
        {
            if (ZoneButtons.Items != null)
                foreach (ZoneButton zb in ZoneButtons.Items)
                    zb.IsEnabled = b;
            ZoneChoiceComboSnapped.IsEnabled = b;
        }


        /// <summary>
        /// Differs from NewReport in that it also checks if there was an alteration
        /// </summary>
        private void EnableSendButtonIfPossible()
        {
            if (zoneSelected != "" && PatientIdTextBox.Text != "" // NO, prefix may vary over time or over org: && PatientIdTextBox.Text != "911-" // not prefix alone
                && App.ReportAltered)
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
            await GetFreshImage();
        }

        private async Task GetFreshImage()
        {
            // Maybe we should introduce App.EditingPatient, rather than reusing CurrentPatient
            await SaveReportFieldsToObject(App.CurrentPatient); //, ""); // Leave IntendToSend empty until "Send" button pressed.
            App.MyAssert(App.CurrentPatient.SentCode != "" && !App.CurrentPatient.ObjSentCode.IsQueued()); // This MUST be true because WebcamPage will be checking it to decide to come back here, instead of to NewReport
            this.Frame.Navigate(typeof(WebcamPage));
            // if fresh image is gotten, App.ReportAltered will be true for benefit of MarkAsAltered
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

            if (App.CurrentPatient.ImageWriteableBitmap == null)
                App.CurrentPatient.ImageWriteableBitmap = new WriteableBitmap(1, 1); // initial size doesn't matter

            // Set the source of the WriteableBitmap to the image stream
            using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                try
                {
                    await App.CurrentPatient.ImageWriteableBitmap.SetSourceAsync(fileStream);
                }
                catch (TaskCanceledException)
                {
                    // The async action to set the WriteableBitmap's source may be canceled if the user clicks the button repeatedly
                    return;
                }
            }
            App.CurrentPatient.ImageName = App.CurrentPatient.FormatImageName();
            //done later: App.CurrentPatient.ImageEncoded = await App.CurrentPatient.FormatImageEncoded();
            App.CurrentPatient.ImageEncoded = ""; // clear out any color swatch beginning with Assets, we have a real image
            //this.Frame.Navigate(this.Frame.Content.GetType()); // Kludge to refresh page, call loadstate
            //this.Frame.GoBack(); // Remove extra call from back stack
            // Instead, repeat code from LoadState:
            UpdateImageLoad();
        }
        

        /// <summary>
        /// In ViewEditReport only, not NewReport
        /// </summary>
        private void MarkAsAltered()
        {
            // TRIED, USELESS: (check focus in textbox textchanged handlers instead)
            // if (suppressMarkingAsAltered)  
            //    return;
            if (!App.ReportAltered) // avoid calling ShowTitle... multiple times
            {
                App.ReportAltered = true;
                ShowTitleAndSentTimeAsAltered();
            }
            EnableSendButtonIfPossible();
        }
/* NEED?
        /// <summary>
        /// In ViewEditReport only, not NewReport
        /// </summary>
        private void MarkAsDiscarded()
        {
            if (!App.ReportAltered) // avoid calling ShowTitle... multiple times
            {
                App.ReportAltered = true;
                ShowTitleAndSentTimeAsAltered("");
            }
            EnableSendButtonIfPossible();
        }
*/
        private void ShowTitleAndSentTimeAsAltered()
        {
            ShowTitleAndSentTimeAsAltered("  Awaiting  your re-send.");
        }

        private void ShowTitleAndSentTimeAsAltered(string appendedToLastSentMsg)
        {
            App.MyAssert(App.ReportAltered);
            if(pageTitle.Text.Contains("*")) // another check to avoid multiple calls
                return; // Already marked
            pageTitle.Text += "*"; // Mark visually as altered
            SolidColorBrush Brush3 = new SolidColorBrush();
            Brush3.Color = Windows.UI.Colors.Red;
            pageTitle.Foreground = Brush3;
            LastSentMsg.Text += appendedToLastSentMsg;
            LastSentMsg.Foreground = Brush3;
        }

        private void ShowTitleAndSentTimeAsUnaltered()
        {
            App.ReportAltered = false; // reset
            if (!pageTitle.Text.Contains("*")) // another check to avoid multiple calls
                return; // Already unmarked
            pageTitle.Text = "🚑  Edit Report"; // Mark visually as unaltered... though more dramatic visual change will be disabling all the entry fields
            SolidColorBrush Brush4 = new SolidColorBrush();
            Brush4.Color = Windows.UI.Colors.White;
            pageTitle.Foreground = Brush4;

            LastSentMsg.Text = "Done with this page.  Please navigate to Outbox, New Report, etc.";
            LastSentMsg.Foreground = Brush4;
        }



        /// <summary>
        /// Similar but not identical to NewReport's ClearEntryExceptPatientID
        /// </summary>
        private void ClearEntryAll() // NOT same as NewReport's ClearEntryExceptPatientID()
        {
            PatientIdTextBox.Text = "";
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
            updatedReport.Clear();
            App.ReportAltered = false;
            App.CurrentPatient.Clear();
            App.MyAssert(App.CurrentPatient.ImageWriteableBitmap == null);
            RefreshPatientImageTextOverlay();
            RefreshPatientImageSource();
            LastSentMsg.Text = "";
        }

        /// <summary>
        /// Enables or disables all entry fields (but Send button is done as part of Clear)
        /// </summary>
        private void EntryEachIsEnabled(bool b)
        {
            PatientIdTextBox.IsEnabled = b; // Even if enabled, still marked as IsReadOnly
            SetFirstNameTextBoxIsEnabled(b);
            SetLastNameTextBoxIsEnabled(b);
            ZonesEachIsEnabled(b);
            CheckBoxAdult.IsEnabled = b;
            CheckBoxPeds.IsEnabled = b;
            CheckBoxMale.IsEnabled = b;
            CheckBoxFemale.IsEnabled = b;
            Notes.IsEnabled = b;
            SetCaptionTextBoxIsEnabled(b);
            SetPatientImageIsTapEnabled(b);
        }

        #region TopAppBar
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            // TO DO - How to change event type
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

        private async void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
        {
            if (await AbandoningPageOK())
            {
                App.CurrentPatient.Clear();
                App.ViewedDisaster.Clear();
                this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
            }
        }

        private async void AllStations_Click(object sender, RoutedEventArgs e)
        {
            if (await AbandoningPageOK())
            {
                App.CurrentPatient.Clear();
                App.ViewedDisaster.Clear();
                this.Frame.Navigate(typeof(SplitPage), "AllStations"); // Defined in SampleDataSource.cs
            }
        }

        private async void Outbox_Click(object sender, RoutedEventArgs e) // at moment, List icon on nav bar
        {
            if (await AbandoningPageOK())
            {
                App.CurrentPatient.Clear();
                App.ViewedDisaster.Clear();
                this.Frame.Navigate(typeof(SplitPage), "Outbox"); // Defined in SampleDataSource.cs
            }
        }

        private async void Statistics_Click(object sender, RoutedEventArgs e)
        {
            if (await AbandoningPageOK())
            {
                App.CurrentPatient.Clear();
                App.ViewedDisaster.Clear();
                this.Frame.Navigate(typeof(SplitPage), "Statistics"); // Defined in SampleDataSource.cs
            }
        }

        private async void Home_Click(object sender, RoutedEventArgs e)
        {
            if (await AbandoningPageOK())
            {
                App.CurrentPatient.Clear();
                App.ViewedDisaster.Clear();
                this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
            }
        }
        #endregion

        #region BottomAppBar
        /* Probably more confusing the helpful:
        private void Clear_Entry_Click(object sender, RoutedEventArgs e)
        {
            // Invoked by what is now called the "Delete" app bar icon, originally called "Clear"
            // Unlike New Report's ClearEntryExceptPatientID();
            ClearEntryAll();
            MarkAsAltered(); // Manually cleared is an alteration
        } */

        // Code in this region is also common to SplitPage
        private void Discard_Click(object sender, RoutedEventArgs e)
        {
            // This Win 8.0 method is based on sample from http://www.csharpteacher.com/2013/04/how-to-add-menu-to-app-bar-windows-8.html
            // We'd do it a different way, with a XAML CommandBar, if restricting to 8.1
            // More complex version:  http://weblogs.asp.net/broux/archive/2012/07/03/windows-8-application-bar-popup-button.aspx
            /*Popup*/ discardMenuPopUp = new Popup();
            discardMenuPopUp.IsLightDismissEnabled = true;  // Dismiss popup automatically when user hits other part of app
            StackPanel panel = new StackPanel(); // create a panel as the root of the menu
            panel.Background = BottomAppBar.Background;
            panel.Height = 140;
            panel.Width = 180;
            Button DeleteLocalButton = new Button();
            DeleteLocalButton.Content = "From outbox only";
            DeleteLocalButton.Style = (Style)App.Current.Resources["TextButtonStyle"];
            DeleteLocalButton.Margin = new Thickness(20, 10, 20, 10);
            DeleteLocalButton.Click += DeleteLocal_Click;
            panel.Children.Add(DeleteLocalButton);
            Button DeleteRemoteTooButton = new Button();
            DeleteRemoteTooButton.Content = "From TriageTrak too";
            DeleteRemoteTooButton.Style = (Style)App.Current.Resources["TextButtonStyle"];
            DeleteRemoteTooButton.Margin = new Thickness(20, 10, 20, 10);
            DeleteRemoteTooButton.Click += DeleteRemoteToo_Click;
            panel.Children.Add(DeleteRemoteTooButton);  
            // Add the root menu as the popup contents:
            discardMenuPopUp.Child = panel;  
            // Calculate the location, here in the bottom righthand corner with padding of 4:
            discardMenuPopUp.HorizontalOffset = Window.Current.CoreWindow.Bounds.Right - panel.Width - 4;
            discardMenuPopUp.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - BottomAppBar.ActualHeight - panel.Height - 4;
            discardMenuPopUp.IsOpen = true;
        }


        private async void DeleteLocal_Click(object sender, RoutedEventArgs e)
        {
            string pid = App.CurrentPatient.PatientID; // ClearEntryAll will clear these, so remember them for Discard
            int v = App.CurrentPatient.ObjSentCode.GetVersionCount();
            ClearEntryAll();  // Will indirectly mark as altered
            LastSentMsg.Text = "Discard from Outbox: Done.";
            App.PatientDataGroups.GetOutbox().Discard(pid, v);
            App.PatientDataGroups.ScrubOutbox(); // Discard itself doesn't seem to do it, leaves empty record behind
            await App.PatientDataGroups.GetOutbox().WriteXML();
            App.PatientDataGroups.Init2(); // resort, refilter, refresh
            discardMenuPopUp.IsOpen = false;
            TopAppBar.IsOpen = false;
            BottomAppBar.IsOpen = false;
        }

        private async void DeleteRemoteToo_Click(object sender, RoutedEventArgs e)
        {
            if (!App.goodWebServiceConnectivity)
            {
                MessageDialog dlg = new MessageDialog(
                    "Sorry, better communications with TriageTrak is needed to do this.  Discarding was cancelled.  Try again later when the 'traffic light' squares are flashing green or yellow instead of red.");
                await dlg.ShowAsync();
                // We don't want to encourage doing a local delete, because that will make it harder to later do a remote delete.  So close popup, app bar.
                discardMenuPopUp.IsOpen = false;
                TopAppBar.IsOpen = false;
                BottomAppBar.IsOpen = false;
                return;
            }

            // Ask if discard locally or both places?

            uuid = await App.service.GetUuidFromPatientID(App.CurrentPatient.PatientID, App.CurrentDisaster.EventShortName);
            if (String.IsNullOrEmpty(uuid))
            {
                string errMsg = "TriageTrak can't find a record with Patient ID " + App.CurrentPatient.PatientID + " associated with event '" + App.CurrentDisaster.EventName +
                    "'.  Discarding was cancelled.  You could try again but choose 'From Outbox Only', if you think that appropriate.";
                MessageDialog dlg = new MessageDialog(errMsg);
                await dlg.ShowAsync();
                await App.ErrorLog.ReportToErrorLog("On ViewEdit Prep for Discard - for event with short name " + App.CurrentDisaster.EventShortName, errMsg, true);
                // bail out:
                // Maybe not:
                // await App.PatientDataGroups.UpdateSendHistoryAfterOutbox(App.CurrentPatient.PatientID, ObjSentCode); // see explanation of this below
                discardMenuPopUp.IsOpen = false;
                TopAppBar.IsOpen = false;
                BottomAppBar.IsOpen = false;
                return;
            }

            discardMenuPopUp.IsOpen = false; // take down 1 popup, then put up another
            whyDiscardedPopUp = new Popup();
            whyDiscardedPopUp.IsLightDismissEnabled = true;  // Dismiss popup automatically when user hits other part of app
            StackPanel panel2 = new StackPanel(); // create a panel as the root of the menu
            panel2.Background = BottomAppBar.Background;
            panel2.Height = 140;
            panel2.Width = 180;
            TextBlock label = new TextBlock();
            label.Text = "Why discard? (optional explanation)";
            panel2.Children.Add(label);
            TextBox explanation = new TextBox();
            explanation.Name = "Explanation";
            panel2.Children.Add(explanation);
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Style = (Style)App.Current.Resources["TextButtonStyle"];
            OKButton.Margin = new Thickness(20, 10, 20, 10);
            OKButton.Click += FinishRemoteDiscard_Click;
            panel2.Children.Add(OKButton);

            // Add the root menu as the popup contents:
            whyDiscardedPopUp.Child = panel2;
            // Calculate the location, here in the bottom righthand corner with padding of 4:
            whyDiscardedPopUp.HorizontalOffset = Window.Current.CoreWindow.Bounds.Right - panel2.Width - 4;
            whyDiscardedPopUp.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - BottomAppBar.ActualHeight - panel2.Height - 4;
            whyDiscardedPopUp.IsOpen = true;
        }

        private async void FinishRemoteDiscard_Click(object sender, RoutedEventArgs e) // From OK button in whyDiscardedPopUp
        {
            // Quick hack... call service directly, instead of putting request on send queue
            TextBox explanation = (TextBox)whyDiscardedPopUp.FindName("Explanation");
            App.MyAssert(explanation != null);
            await App.service.ExpirePerson(uuid, explanation.Text); 
            // On to local discard....
            DeleteLocal_Click(sender, e);
            TopAppBar.IsOpen = false;
            BottomAppBar.IsOpen = false;
            whyDiscardedPopUp.IsOpen = false;
        }

        private async void Webcam_Click(object sender, RoutedEventArgs e)
        {
            // We are NOT abandoning page here, will come back to it.
            await GetFreshImage();
        }
        #endregion

        private async Task<bool> AbandoningPageOK()
        {
            if (!App.ReportAltered)
                return true;
            bool results = false;
            var md = new MessageDialog("Leaving this page (except to get new image) will cause edit changes to be discarded.  Are you sure?");
            md.Commands.Add(new UICommand("Yes", (UICommandInvokedHandler) => { results = true; } ));
            md.Commands.Add(new UICommand("No"));
            await md.ShowAsync();
            return results;
        }

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

        private void Caption_SetTextColor(SolidColorBrush b)
        {
            Caption.Foreground =
            CaptionPortrait.Foreground = b;
        }

        private void Caption_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SyncAndGetCaptionTextBox() == CAPTION_TEXT_HINT)
                SetCaptionTextBox("");
            SolidColorBrush Brush1 = new SolidColorBrush();
            Brush1.Color = Windows.UI.Colors.Black;
            Caption_SetTextColor(Brush1);
        }

        private void Caption_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SyncAndGetCaptionTextBox() == String.Empty)
            {
                SetCaptionTextBox(CAPTION_TEXT_HINT);
                SolidColorBrush Brush2 = new SolidColorBrush();
                Brush2.Color = Windows.UI.Colors.Gray;  // Must match color in XAML
                Caption_SetTextColor(Brush2);
            }
        }

        private void Caption_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).FocusState == FocusState.Unfocused) // prevent spurius post-LoadState calls
                return;
            MarkAsAltered();
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).FocusState == FocusState.Unfocused) // prevent spurius post-LoadState calls
                return;
            MarkAsAltered();
        }

        private void FirstNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).FocusState == FocusState.Unfocused) // prevent spurius post-LoadState calls
                return;
            MarkAsAltered();
        }

        private void LastNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (((TextBox)sender).FocusState == FocusState.Unfocused) // prevent spurius post-LoadState calls
                return;
            MarkAsAltered();
        }

        private void PatientIdTextBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // NOT YET SUPPORTING EDITING PATIENT ID IN VIEW EDIT
            // Maybe this will fire, if user goes to field in order to copy out patient ID
            if (App.OrgPolicy.OrgPatientIdFixedDigits != -1)
            {
                PatientIdTextBox.Select(PatientIdTextBox.Text.Length, 0); // Cursor to far right
            }
        }

        private void PatientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            // NOT YET SUPPORTING EDITING PATIENT ID IN VIEW EDIT
            // IsReadOnly= true, so shouldn't get here at this time.
            EnableSendButtonIfPossible();
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

        // In first release, changing of ViewEdit's patient ID is disabled in XAML, so next bunch of functions won't be called
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
            // NOT YET SUPPORTING EDITING PATIENT ID IN VIEW EDIT
            // IsEnabled = false, so shouldn't get here at this time.
            RollUpward = true;
            return BumpAndStuffPatientID();
        }

        private string DecrementPatientID() // first draft  Assumes suffix length is 4.  Caller must save results to PatientIdTextBox
        {
            // NOT YET SUPPORTING EDITING PATIENT ID IN VIEW EDIT
            // IsEnabled = false, so shouldn't get here at this time.
            RollUpward = false;
            return BumpAndStuffPatientID();
        }

        private string BumpAndStuffPatientID()
        {
            // NOT YET SUPPORTING EDITING PATIENT ID IN VIEW EDIT
            // IsEnabled = false, so shouldn't get here at this time.
            MarkAsAltered(); // first draft
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

        private void AnyCheckBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // We are not concerned with the value, yet.  Only that something has changed.
            MarkAsAltered();
        }

    }
}
