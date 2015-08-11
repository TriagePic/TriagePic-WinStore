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
using TP8.Common;
using System.ComponentModel;
// nah: using TP8.DataModel;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TP8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPageNew : TP8.Common.BasicLayoutPage //, INotifyPropertyChanged // was: LayoutAwarePage
    {
        private string zoneSelected = "";

        public string MyZoneButtonItemWidth { get; set; }

        private TP_PatientReport pr = null; //pdi = null;
        // FEB 2015 COMMENT: Treatment of pr and App.CurrentPatient here, and updatedReport and App.CurrentPatient in ViewEditReport, not real consistent.
        // In ViewEditReport, App.CurrentPatient is treated as original version of report, but then overwritten if we navigate to webcam.
        // Need to rethink next time refactoring of these pages is done (e.g., to have a common base page)

        private const string NOTES_TEXT_HINT = "Optional Notes"; // Must match string in XAML too
        private const string CAPTION_TEXT_HINT = "Optional Caption";  // Must match string in XAML too.  Temporary limit: 1 photo, 1 caption
        private const string EMBEDDED_FILE_PREFIX = "ms-appx:///Assets/";

        DispatcherTimer dt = null;
        private bool RollUpward = true;

        private ZoneButton[] zb = null;
        private int zbCount = 0;
 //IS_THIS_NECESSARY       private bool firstTime = true;
        private NavigationEventArgs latestNavigation = null;
#if DIDNT_WORK_FOR_3_OBJECT_BIND
        //public static string LastSentMessage { get; set; } // New June 2015 bind target
        public event PropertyChangedEventHandler PropertyChanged;
        private string _LastSentMessage;
        public string LastSentMessage
        {
            get
            {
                return this._LastSentMessage;
            }
            set
            {
                this._LastSentMessage = value;
                if (this.PropertyChanged != null)
                    this.PropertyChanged(this, new PropertyChangedEventArgs("LastSentMessage"));
            }
        }
        /*
 <Rectangle x:Name="BlinkerTopRed" x:FieldModifier="public" Height="15" Width="15" Fill="DarkRed" />
                        <Rectangle x:Name="BlinkerMiddleYellow" x:FieldModifier="public" Margin="0,4" Height="15" Width="15" Fill="DarkGoldenrod" />
                        <Rectangle x:Name="BlinkerBottomGreen" x:FieldModifier="public" Height="15" Width="15" Fill="DarkGreen" />
                        <TextBlock x:Name="CountInSendQueue" x:FieldModifier="public" Height="30" Width="40" Margin="0,10,0,0" FontSize="18" TextAlignment="Center" FontWeight="Bold" /> */
#endif
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
                // This does not fire TextChanged, so we'll force it later in LoadState
            }
            // We'll check if ID is already in use later, in LoadState

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
            LastSentMsg.DataContext = this;
            patientID_ConflictStatus.Text = ""; // July 2015, v 5.  Clear design-time string "[conflict status]"
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
            AdjustZoneButtonWidth();
            /* WAS Win 8.0:
            string visualState = DetermineVisualState(ApplicationView.Value);
            if (visualState == "vs673To1025Wide")
            {
                MyZoneButtonItemWidth = "110";  // Can't change button width directly in XAML, because its set by ItemWidth in template, so not allowed.
            } */
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            latestNavigation = e;            
            base.OnNavigatedTo(e); // This calls LoadState
        }

        private void AdjustZoneButtonWidth()
        {
            // Broken out as separate function Sept 2014 in TP-8.1 project, moved here March 2015
            string visualState = App.CurrentVisualState; // WAS Win 8.0: DetermineVisualState(ApplicationView.Value);
            if (visualState == "vs673To1025Wide" || visualState == "vs501To672Wide")  // make filled and half the same, until we know better
            {
                MyZoneButtonItemWidth = "110";  // Can't change button width directly in XAML, because its set by ItemWidth in template, so not allowed.
            }
            else
                MyZoneButtonItemWidth = "140";
            // Narrow and snapped don't use buttons for zones, instead a pull-down
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        protected override async void LoadState(LoadStateEventArgs e)
        {
            InitiateZones();
            // was before Release 2:
            // if (!String.IsNullOrEmpty(App.CurrentPatient.Zone)) // This test may need refinement
            //    LoadReportFieldsFromObject(pr);
            if (latestNavigation.NavigationMode == NavigationMode.Back) // probably back from webcam
            {
                // Assume there's content to reload
                pr = App.CurrentPatient;
                LoadReportFieldsFromObject(pr);
            }
            await PatientID_TextChanged(); // added July 2015. Will check if initial ID is already used, i.e., reported for current event. If so, colorize fields
            pageSubtitle.Text = " " + App.CurrentDisaster.EventName; // TO DO: binding in XAML instead of here?  Add space to separate from icon
            if (App.CurrentDisaster.TypeIconUri.Length > EMBEDDED_FILE_PREFIX.Length)
            {
                eventTypeImage.Source = new BitmapImage(new Uri(App.CurrentDisaster.TypeIconUri));
                // Tried XAML binding, didn't work:                 <Image x:Name="eventTypeImage" Source="{Binding CurrentDisasterEventTypeIcon}" Width="40" VerticalAlignment="Top"/>
                // WAS before next call added: MyZoneButtonItemWidth = "140";
            }
            AdjustZoneButtonWidth();

            UpdateImageLoad();
#if IS_THIS_NECESSARY
            if (firstTime)
            {
                firstTime = false;
                UpdateStoryBoard();
            }
#endif
            // Not needed: base.LoadState(e);
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
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            InitiateZones();
            // was before Release 2:
            // if (!String.IsNullOrEmpty(App.CurrentPatient.Zone)) // This test may need refinement
            //    LoadReportFieldsFromObject(pr);
            if (latestNavigation.NavigationMode == NavigationMode.Back) // probably back from webcam
            {
                // Assume there's content to reload
                pr = App.CurrentPatient;
                LoadReportFieldsFromObject(pr);
            }
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
#endif

#if IS_THIS_NECESSARY
        private void UpdateStoryBoard()
        {
            for(int i = 0; i < zbCount; i++)
            {
                // Make widths narrower for filled and half state:
                ObjectAnimationUsingKeyFrames anim1 = new ObjectAnimationUsingKeyFrames();
                anim1 = UpdateStoryBoardImpl(anim1, i);
                Filled.Storyboard.Children.Add(anim1);
                ObjectAnimationUsingKeyFrames anim2 = new ObjectAnimationUsingKeyFrames();
                anim2 = UpdateStoryBoardImpl(anim2, i);
                Half.Storyboard.Children.Add(anim2); // added May 2015
            }
        }

        private ObjectAnimationUsingKeyFrames UpdateStoryBoardImpl(ObjectAnimationUsingKeyFrames anim, int i) // Broken out May 2015
        {
            // Make widths narrower for filled and half state:
            DiscreteObjectKeyFrame kf = new DiscreteObjectKeyFrame();
            kf.KeyTime = TimeSpan.FromSeconds(0);
            //kf.Value = 100;
            kf.SetValue(ZoneButton.ZoneButtonWidthProperty, 100);
            anim.KeyFrames.Add(kf);
            Storyboard.SetTargetName(anim, zb[i].Name);
            // fails with exception when filled mode invoked: Storyboard.SetTargetProperty(anim, "(ZoneButton.ZoneButtonWidthProperty)");
            Storyboard.SetTargetProperty(anim, "ZoneButtonWidth"); // doesn't throw exception but doesn't change width either.  Doesn't seem to call setter
            return anim;
        }
#endif
        // Override of SaveState not needed
#if WAS

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }
#endif

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
                SetPatientImageSource(new BitmapImage(new Uri(EMBEDDED_FILE_PREFIX + "PhotoBackgroundCameraLogo(300x300).png", UriKind.Absolute)));
            else
                SetPatientImageSource(App.CurrentPatient.ImageWriteableBitmap);
        }

        #region Multiple_UI_Controls_For_Same_Logical_Value
        private void SetPatientImageTextOverlay(string s)
        {
            patientImageTextOverlayLandscape.Text =
            patientImageTextOverlayHalf.Text =
            patientImageTextOverlaySnapped.Text =
            patientImageTextOverlayPortrait.Text = s;
        }

        private void SetPatientImageSource(ImageSource s)
        {
            patientImageLandscape.Source =
            patientImageHalf.Source =
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
            // Hmm, in standard Win 8.1, there's "Portrait" and "DefaultLayout" and page SizeChanged event handler.  We're using BasicLayoutPage categorization here instead.
            string visualState = App.CurrentVisualState; // WAS Win 8.0:  DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": FirstNameTextBox.Text = FirstNameTextBoxPortrait.Text; break;
                //case "FullScreenLandscape":
                //case "vsOver1365Wide":
                //case "vs1026To1365Wide":
                //case "vs673To1025Wide":
                //case "vs501To672Wide":
                //case "vs321To500Wide":
                //case "vs320Wide":
                default: FirstNameTextBoxPortrait.Text = FirstNameTextBox.Text; break;
            }
            // All sync'd
            return FirstNameTextBox.Text;
        }

        private string SyncAndGetLastNameTextBox()
        {
            string visualState = App.CurrentVisualState; // WAS Win 8.0:  DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": LastNameTextBox.Text = LastNameTextBoxPortrait.Text; break;
                //case "FullScreenLandscape":
                //case "vsOver1365Wide":
                //case "vs1026To1365Wide"
                //case "vs673To1025Wide":
                //case "vs501To672Wide":
                //case "vs321To500Wide":
                //case "vs320Wide":
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
            string visualState = App.CurrentVisualState; // WAS Win 8.0:  DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": Caption.Text = CaptionPortrait.Text; break;
                //case "FullScreenLandscape":
                //case "vs1026To1365Wide":
                //case "vs673To1025Wide":
                //case "vs501To672Wide":
                //case "vs321To500Wide":
                //case "vs320Wide":
                default: CaptionPortrait.Text = Caption.Text; break;
            }
            // All sync'd
            return Caption.Text;
        }

        private void GiveCaptionFocus()
        {
            string visualState = App.CurrentVisualState; // WAS Win 8.0:  DetermineVisualState(ApplicationView.Value);
            switch (visualState)
            {// current visual state is the master, other states are slaves
                case "FullScreenPortrait": CaptionPortrait.Focus(FocusState.Programmatic); break;
                //case "FullScreenLandscape":
                //case "vsOver1365Wide":
                //case "vs1026To1365Wide":
                //case "vs673To1025Wide":
                //case "vs501To672Wide":
                //case "vs321To500Wide":
                //case "vs320Wide":
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

            AdjustProgressBarVisibility(Visibility.Visible);
            if (Notes.Text == NOTES_TEXT_HINT) // Must match string in XAML too
                Notes.Text = "";
            if (App.RosterNames != "")
                Notes.Text += "\nRoster at Station:\n" + App.RosterNames; // DON'T DO THIS FOR RE-SEND... wait til we handle rostering smarter. 
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
                // Tried subsituting data-bound LastSentMessage for LastSentMsg.Text below, but didn't help with bindings that change with visual state
                if (count > 0)
                    AdjustLastSentMsgText("Reports waiting to send:  " + count);
                else
                {
                    AdjustProgressBarVisibility(Visibility.Collapsed);
                    AdjustLastSentMsgText("Sent");
                }

                await Task.Delay(2000); // see message for 2 seconds
                if (count == 0 || count <= TP_SendQueue.reportsToSend.Count()) // if count doesn't decrease after 2 seconds, then don't wait further in this loop
                    break;
            }
            AdjustProgressBarVisibility(Visibility.Collapsed);
            // Maybe not: if(!LastSentMsg.Text.StartsWith("Reports waiting to send")) // Let count of waiting reports persist
            /* was: LastSentMsg.Text */
            AdjustLastSentMsgText("");
        }

        private void AdjustProgressBarVisibility(Visibility v) // New June 2015
        {
            progressBarSendingPortrait.Visibility = progressBarSendingPortrait.Visibility = progressBarSending.Visibility = v;
            /*if (App.CurrentVisualState == "FullScreenPortrait")
                progressBarSendingPortrait.Visibility = v;
            else if (App.CurrentVisualState == "vs320Wide" || App.CurrentVisualState == "vs321To500Wide")
               progressBarSendingPortrait.Visibility = v; v;
            else
                progressBarSending.Visibility = v;*/
        }

        private void AdjustLastSentMsgText(string s)
        {
            LastSentMsgPortrait.Text = LastSentMsgSnapped.Text = LastSentMsg.Text = s;
            /*if (App.CurrentVisualState == "FullScreenPortrait")
                LastSentMsgPortrait.Text = s;
            else if (App.CurrentVisualState == "vs320Wide" || App.CurrentVisualState == "vs321To500Wide")
                LastSentMsgSnapped.Text = s;
            else
                LastSentMsgPortrait.Text = s;*/
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
                case "Other Age Group (e.g., Expectant)": // pregnant
                    CheckBoxAdult.IsChecked = CheckBoxPeds.IsChecked = true; break;
                case "Adult":
                    CheckBoxAdult.IsChecked = true; break;
                case "Youth":  // WAS: Pediatric
                    CheckBoxPeds.IsChecked = true; break;
                case "Unknown Age Group":  // WAS: "Unknown"
                    break;
                case "": // Added Feb 2015. May occur if record entry is incomplete, interrupted by webcam capture
                    break;
                default: App.MyAssert(false); break;
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
                    break;
                case "": // Added Feb 2015. May occur if record entry is incomplete, interrupted by webcam capture
                    break;
                default: App.MyAssert(false); break;
            }
            zoneSelected = pr_.Zone;
            if (zoneSelected != "") // Test for empty string added Feb 2015. May occur if record entry is incomplete, interrupted by webcam capture 
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
            EnableSendButtonIfPossible(); // added June 2015
        }

        private void EnableSendButtonIfPossible()
        {
            if (zoneSelected != "" && PatientIdTextBox.Text != "" &&
                !patientID_ConflictStatus.Text.StartsWith("Used -") && // New July 2015 Revision 5.  Note that "Used?" is OK
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
            pr = App.CurrentPatient; // Probably not necessary since pr will be going out of scope, but just in case.
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

        private async void Statistics_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentVisualState == "vs320Wide" || App.CurrentVisualState == "vs321To500Wide")
            // WAS before Release 3: if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
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
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
        }

        private async void Webcam_Click(object sender, RoutedEventArgs e)
        {
            await SaveReportFieldsToObject(App.CurrentPatient, "");
            pr = App.CurrentPatient; // Probably not necessary since pr will be coing out of scope, but just in case.
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

        private async void PatientID_TextChanged(object sender, TextChangedEventArgs e)
        {
            await PatientID_TextChanged();
        }

        private async Task PatientID_TextChanged()
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

            await ColorizeBasedOnPatientID(); // new, July 2015, version 3.5
            EnableSendButtonIfPossible();
        }

        /// <summary>
        /// Analyzes patientIdTextBox contents, and colors the backgrounds of all form entry fields
        /// </summary>
        /// <returns>Code from result of patient ID analysis</returns>
        public async Task<int> ColorizeBasedOnPatientID() // new, July 2015, version 3.5.  Similar to TP7's FormTriagePic function of the same name
        {
            int nUsed = await WasPatientIdentifierAlreadyUsed(PatientIdTextBox.Text);
            if (nUsed > 0)
                FlagProblematicPatientID(nUsed); // Colorize Patient ID, do status message [different place than TP7]
            else
                UnflagProblematicPatientID(nUsed); // If 0 or -1. 
            return nUsed;
        }

        /// <summary>
        /// Checks local and remote resources, for match with ID with or without "AUTO" prepended.
        /// </summary>
        /// <param name="s">Mass Casualty ID</param>
        /// <returns>-1 = don't know; 0 = no; 1 = yes, in outbox for this event; 2 = yes, known to TT (but not outbox) for this event</returns>
        public async Task<int> WasPatientIdentifierAlreadyUsed(string s)
        {
            App.MyAssert(!s.StartsWith("AUTO")); // This is true in Release 5, but may change in future, in which case more changes needed here.

            // TP7 has 2 additional states, which we'll hold off on:
            // 3 = yes, in outbox for DIFFERENT event (allowed by delete of previous use in Outbox recommended)
            // 4 = yes, in sent files (but not outbox) for same or different event. Would involve directory search of image file names
            foreach (var pr_ in App.PatientDataGroups.GetOutbox())
            {
                if (App.CurrentDisaster.EventShortName != pr_.EventShortName)
                    continue;
                if (s == pr_.PatientID) // Will need to change this further if TP8 can reserve IDs with AUTO prefix
                    return 1; // may or may not be from same org as current org
            }
            int i = await App.service.IsPatientIdAlreadyInUseForCurrentEvent(s); // returns -1 = don't know; 0 = no; 1 = yes
            if (i == 1)
                return 2;
            // For now:
            string s1 = "AUTO" + s;
            if (i == 0)
            {
                i = await App.service.IsPatientIdAlreadyInUseForCurrentEvent(s1); // returns -1 = don't know; 0 = no; 1 = yes
                if (i == 1)
                    return 2;
                if (i == 0)
                    return 0;
            }
            // Need another web service, to know if AUTO ID was reserved by someone else, but not yet used.
            App.MyAssert(i == -1);
            // New in TP8 (not in TP7 I believe)
            // If call to web service failed, check local All Stations cache. Data may be stale and (because of 250 size limit) incomplete, but still mostly valid.
            // CAUTION: if you delete a record from outbox & TriageTrak, stale All Stations might still have it.
            // This might be a rare case (fixable by deleting it from All Stations too)
            foreach (var pr_ in App.PatientDataGroups.GetAllStations())
            {
                if (App.CurrentDisaster.EventShortName != pr_.EventShortName)
                    continue;
                if (s == pr_.PatientID || s1 == pr_.PatientID)
                    return 2; // may or may not be from same org as current org
            }
            return -1; // we could return 0 here, but staleness issue intrudes.
            // If searching for 3, do so here
            // If searching for 4, do so here
            //return 0;
        }

        public void FlagProblematicPatientID(int nUsed)
        {
            App.MyAssert(nUsed > 0 && nUsed < 5);
            SetSpecialBackColors(nUsed);
            switch(nUsed)
            {
                // If below text msgs are changed, see also EnableSendButtonIfPossible(), which looks at this field's value
                case 1: patientID_ConflictStatus.Text = "Used - in Outbox"; break;
                case 2: patientID_ConflictStatus.Text = "Used - TriageTrak"; break;
                default: App.MyAssert(false); break; // no case 3 or 4 yet
            }
        }

        private void SetSpecialBackColors(int nUsed) // new, July 2015, version 3.5.  Similar to TP7's FormTriagePic function of same name
        {
            App.MyAssert(nUsed > 0 && nUsed < 5);

            // SOME DAY:
            // if (newReport.practiceModeChecked)
            //      ClearSpecialBackColors(); // Special case, then restore Patient ID color below.

            SolidColorBrush c;
            if (nUsed == 3) // DO WE NEED THIS IN TP8?
            {
                // Not forbidden by policy, but has operational problems
                PatientIdTextBox.Background = new SolidColorBrush(Colors.Yellow); // make this field more saturated than others colored by c
                c = new SolidColorBrush(Windows.UI.Colors.NavajoWhite);
            }
            else
            {
                // Forbidden by policy
                PatientIdTextBox.Background = new SolidColorBrush(Colors.Orange); // make this field more saturated than others colored by c
                c = new SolidColorBrush(Colors.NavajoWhite);
            }
            // SOME DAY:
            // if (newReport.practiceModeChecked)
            //      return;

            FirstNameTextBox.Background = c;
            LastNameTextBox.Background = c;
            CheckBoxMale.Background = c;
            CheckBoxFemale.Background = c;
            CheckBoxAdult.Background = c;
            CheckBoxPeds.Background = c;
            Caption.Background = c;
            Notes.Background = c;
            // When we support multiple photos, will have to do photoRole too
        }

        private void ClearSpecialBackColors()
        {
            SolidColorBrush c = new SolidColorBrush(Colors.White); // In TP7 version, checkbox bkgds were set to white,
                // textboxs' to Color.Empty (as "usual enable/disble colors"). Unclear if latter necessary here (or possible.. leave unset? or .Transparent?).
            PatientIdTextBox.Background = c;
            FirstNameTextBox.Background = c;
            LastNameTextBox.Background = c;
            Caption.Background = c;
            Notes.Background = c;
            // When we support multiple photos, will have to do photoRole too
            c = new SolidColorBrush(Colors.Transparent); // In TP7 version, checkbox bkgds were set to white,
            CheckBoxMale.Background = c;
            CheckBoxFemale.Background = c;
            CheckBoxAdult.Background = c;
            CheckBoxPeds.Background = c;
        }

        private void UnflagProblematicPatientID(int nUsed)
        {
            App.MyAssert(nUsed == -1 || nUsed == 0);
            ClearSpecialBackColors();
            switch (nUsed)
            {
                // If below text msgs are changed, see also EnableSendButtonIfPossible(), which looks at this field's value
                case -1: patientID_ConflictStatus.Text = "Used? Assume not"; break;
                case 0: patientID_ConflictStatus.Text = ""; break;
                default: App.MyAssert(false); break;
            }
        }

        public bool IsProblematicPatientID()
        {
            SolidColorBrush b = (SolidColorBrush)PatientIdTextBox.Background; // hope this works
            return (bool)((b.Color == Colors.Orange) || (b.Color == Colors.Yellow));
        }

        private bool VerifyNotProblematicPatientID()
        {
            if (!IsProblematicPatientID())
                return true;
/* not yet
            SolidColorBrush b = (SolidColorBrush)PatientIdTextBox.Background; // hope this works
            if(b.Color == Colors.Yellow)
            {
                // TO DO MAYBE
            }
            else
            {
                // TO DO
            } */
            return false;
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
