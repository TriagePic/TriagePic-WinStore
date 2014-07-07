using TP8.Common;
using TP8.Data;
using LPF_SOAP;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;

using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// WAS in Win 8.0: using Callisto.Controls;
//using MyToolkit.Controls;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
using Windows.System.UserProfile;
using Windows.Storage;
using System.Threading;



// nah, using TP8.DataModel; // By Rico Suter.  For DatePicker

// The Split App template is documented at http://go.microsoft.com/fwlink/?LinkId=234228

namespace TP8
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.DebugSettings.BindingFailed += new BindingFailedEventHandler(DebugSettings_BindingFailed); //Tim Heuer suggestion
        }

        void DebugSettings_BindingFailed(object sender, BindingFailedEventArgs e)
        {

            Debug.WriteLine(e.Message);
        }

        // Glenn add's to report error to user, outside of UI thread:
        public static CoreDispatcher dispatcher { get; set; }

        // Glenn adds globals, to ease passing info through navigate parameters
        // Access globals everywhere through, e.g., App.CurrentResultsGroup
        // These are data globals.  Styles are better defined in Resources
        public static TP_EventsDataList CurrentDisasterList = new TP_EventsDataList();
        // NO LONGER NEEDED AFTER BUG FIXED, but we may bring this back in the future if we add event filtering to event-choice control:
        // public static ObservableCollection<TP_EventsDataItem> CurrentDisasterListForCombo = new ObservableCollection<TP_EventsDataItem>();
        public static TP_EventsDataList CurrentDisasterListFilters = new TP_EventsDataList();
        public static TP_EventsDataItem CurrentDisaster = new TP_EventsDataItem();
        public static string RosterNames = "";
        //public static string CurrentDisasterEventName = "";
        //public static string CurrentDisasterEventTypeIcon { get; set; }
        //public static string CurrentDisasterEventID = "";
        //public static string CurrentDisasterEventType = "";
        public static string CurrentSearchResultsGroupName = "";
        public static string CurrentSearchQuery = "";
        public static TP_PatientReport CurrentPatient = new TP_PatientReport();
        public static TP_OtherSettingsList CurrentOtherSettingsList = new TP_OtherSettingsList(); // mainly helper for other items
        public static TP_OtherSettings CurrentOtherSettings = new TP_OtherSettings(); // ditto
        public enum SortByItem
        {
            ArrivalTime,
            PatientIdWithPrefixAlphabetic,
            PatientIdSkipPrefixNumeric,
            FirstName,
            LastName,
            Gender,
            AgeGroup,
            TriageZone,
            PLStatus,
            DisasterEvent,
            ReportingStation
        }
        public static string SearchResultsEventTitleTextBasedOnCurrentFilterProfile = ""; // may be summary text like "All Events" or "All Public Events", or specific event

        // Used by ViewEditReport
        public static bool ReportAltered = false;
        public static bool ReportDiscarded = false; // only checked as special case of ReportAltered=true
        public static TP_EventsDataItem ViewedDisaster = new TP_EventsDataItem(); // may differ from CurrentDisaster


        //public static TP_FilterProfile DefaultFilterProfile = new TP_FilterProfile();

        public static TP_FilterProfileList FilterProfileList = new TP_FilterProfileList();
        // Here first, then moved to FilterFlyout, but wasn't defined early enough so move back:
        public static TP_FilterProfile CurrentFilterProfile = new TP_FilterProfile();

        public static TP_PatientDataGroups PatientDataGroups;

        public static SortByItem SortFlyoutItem = SortByItem.ArrivalTime;

        public static bool SortFlyoutAscending = true;

        public static bool OutboxCheckBoxCurrentEventOnly = true;
        public static bool OutboxCheckBoxMyOrgOnly = true;
        public static bool AllStationsCheckBoxMyOrgOnly = true;

        public static TP_ZoneChoices ZoneChoices = new TP_ZoneChoices();

        public static TP_OrgPolicyList OrgPolicyList = new TP_OrgPolicyList(); // Ignore all but first entry on list.  Includes patient ID format
        public static TP_OrgPolicy OrgPolicy = new TP_OrgPolicy(); // gets first entry on list

        public static TP_OrgDataList OrgDataList = new TP_OrgDataList(); // list of all hospitals/organizations defined at our instance of PL/Vesuvius

        public static TP_OrgContactInfoList OrgContactInfoList = new TP_OrgContactInfoList(); // Ignore all but first entry on list.
        public static TP_OrgContactInfo CurrentOrgContactInfo = new TP_OrgContactInfo(); // gets first entry on list

        //public static string FilterFlyoutEventChoice = ""; // could be specific event, or group description.
        //public static string FilterFlyoutEventType { get; set; }

        public static PLWS.plusWebServicesPortTypeClient pl = new PLWS.plusWebServicesPortTypeClient();
        public static LPF_JSON service = new LPF_JSON();
        public static bool BlockWebServices = false;

        public static TP_ErrorLog ErrorLog = new TP_ErrorLog();
        public static ProtectData pd = new ProtectData(); // credentials of current user here.  // HARD-CODED.  TO DO!!!!
        public static TP_UsersAndVersions UserAndVersions = new TP_UsersAndVersions(); // local list of all user accounts on this device
        public static string UserWin8Account = "";
        public static string DeviceName = "";
        public static TP_SendQueue sendQueue = new TP_SendQueue();
        public static bool goodWebServiceConnectivity = false; // until we know better.  Determined by pings.
        //public static List<string> SentStatusMessageAsSemaphore = new List<string>();
        public static string DelayedMessageToUserOnStartup = ""; // Occurs during App.OnLaunched, but can't be easily shown until home page launch
        public const string NO_OR_BAD_WEB_SERVICE_PREFIX =
            "Could not connect to TriageTrak web service.  Using previous information, cached locally, instead for:\n"; // Often used with delayed message
        public static bool AppFinishedLaunching = false; // set to true during home page launch
        public static bool ProcessingAllStationsList = false;
        public static SemaphoreSlim LocalStorageDataSemaphore = new SemaphoreSlim(1); // LocalStorage.Data is a static shared resource; serialize read/write access to it.

        public static App Instance
        {
            get { return ((App)Current); }
        }

        [Conditional("DEBUG")]
        public static void MyAssert(bool condition)
        {
            // Since usual old App.MyAssert isn't so useful in Windows Store apps...
            // see stackoverflow.com/questions/10528168/debug-assertfalse-does-not-trigger-in-win8-metro-apps
            if (!condition)
                System.Diagnostics.Debugger.Break();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();
                //Associate the frame with a SuspensionManager key                                
                SuspensionManager.RegisterFrame(rootFrame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await SuspensionManager.RestoreAsync();
                        await DoStartup();
                        await DoResumeFromTermination();
                    }
                    catch (SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }
            if (rootFrame.Content != null)
                Window.Current.Activate(); // app is already running, or the navigation state was restored.
            else
            {
                // Code associated with DoStartup was here, now separate function, called from ExtendedStartup
                // See multimedialion.wordpress.com/2012/12/13/adding-an-extended-splash-screen-to-a-windows-8-app-tutorial-c
                SplashScreen splashScreen = args.SplashScreen;
                ExtendedSplash eSplash = new ExtendedSplash(splashScreen);
                // Register an event handler to be exectued when the splash screen has been dismissed
                splashScreen.Dismissed += new TypedEventHandler<SplashScreen, object>(eSplash.onSplashScreenDismissed);
                //Maybe not...rootFrame.Content = eSplash; // suggested by stackoverflow.com/questions/12743355/screen-flashes-between-splash-and-extended-splash-in-windows-8-app
                Window.Current.Content = eSplash;

                // Ensure the current window is active
                Window.Current.Activate();
                // THIS HUNG: Do not repeat app initialization when already running, just ensure that
                // the window is active
                //if (args.PreviousExecutionState == ApplicationExecutionState.Running)
                //{
                //    Window.Current.Activate();
                //    return;
                //}
                dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
                await DoStartup();
                Window.Current.Content = rootFrame; // restore after splash screen
                sendQueue.StartWork(); // DON'T await.  Needs to be after Window.Current.Content = rootFrame;
 //           if (rootFrame.Content == null)
 //           {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                if (!rootFrame.Navigate(typeof(HomeItemsPage), "AllGroups"))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
        }

        public static async Task DoStartup()
        {
            await ErrorLog.Init();
            await App.ErrorLog.ReportToErrorLog("FYI: Beginning App.Launch", "", false);

            UserWin8Account = await GetUserWin8Account();  // May be empty string if PC Settings/Prvacy/"Let apps use my name and account picture" is false
            DeviceName = GetDeviceName();

            await OrgDataList.Init(); // get list of all hospitals/organizations defined at our TriageTrak.  Don't need username/password for this.
            // Do this early so startup wizard has info it needs.

            await UserAndVersions.Init(); // Need PL password for web services to work.  Also init's pd.  Startup wiz called here.
            // Initialize app model before navigating to home page, so groups will be set up
#if MAYBE_NOT_ANY_MORE
            //DefaultFilterProfile.ResetFilterProfileToDefault();
            //Was here first, then moved to flyout, then moved back: 
            CurrentFilterProfile.ResetFilterProfileToDefault(); // will change more further below
            CurrentFilterProfile.AControlChanged = false;
#endif
            //DelayedMessageToUserOnStartup += "  - TEST OF DELAYED MESSAGE";
            PatientDataGroups = new TP_PatientDataGroups(); // which will use CurrentFilterProfile
            await PatientDataGroups.Init(); // reads in data.  See Init2 further below
            App.RegisterSettings(); // Added for settings flyout
            // Initialize from events data model.  Take first disaster event in list as default for now.
            //var evcol = new ObservableCollection<TP_EventsDataItem>();
            //evcol = TP_EventsDataList.GetEvents();
            await CurrentDisasterList.Init();
            CurrentDisaster.CopyFrom(CurrentDisasterList.FirstOrDefault());
            await CurrentDisasterListFilters.InitAsFilters();
            // NO LONGER NEEDED ONCE BUG FIXED: CloneDisasterListForCombo();

            // Moved earlier: await OrgDataList.Init(); // get list of all hospitals/organizations defined at our PL/Vesuvius

            if (String.IsNullOrEmpty(CurrentOrgContactInfo.OrgAbbrOrShortName)) // usually already setup
            {
                await OrgContactInfoList.Init();
                if (OrgContactInfoList.Count() > 0)
                    CurrentOrgContactInfo = OrgContactInfoList.First(); // causes too many problems elsewhere: FirstOrDefault();
            }

            await ZoneChoices.Init(); // does minimal.  Just done in case OrgPolicyList.Init() runs into trouble.
            await OrgPolicyList.Init(); // Will also fetch data for ZoneChoices.
            if (OrgPolicyList.Count() > 0)
                OrgPolicy = OrgPolicyList.First(); // FirstOrDefault(); // will return null if nothing in list

            CurrentFilterProfile = await FilterProfileList.GetDefaultForCurrentOrg();

            await CurrentOtherSettingsList.Init(); // initializes ACTUAL value for App.CurrentDisaster
            if (CurrentOtherSettingsList.Count() > 0)
                CurrentOtherSettings = CurrentOtherSettingsList.First();

            PatientDataGroups.Init2(); // See Init further above.  Init2 handles work after CurrentFilterProfile, actual App.CurrentDisaster have been defined.

            // Moved to later in call sequence: sendQueue.StartWork(); // DON'T await
            await App.ErrorLog.ReportToErrorLog("FYI: Ending App.Launch", "", false);
        }

/* NO LONGER NEEDED ONCE BUG FIXED:
        public static void CloneDisasterListForCombo()
        {
            CurrentDisasterListForCombo.Clear(); // not needed during startup, but when called from checklist page
            foreach (TP_EventsDataItem i in CurrentDisasterList)
            {
                TP_EventsDataItem j = new TP_EventsDataItem { };
                j.CopyFrom(i);
                CurrentDisasterListForCombo.Add(j);
            }
        } */

        private static void DoSaveOnTermination()
        {
            // Save things here that aren't otherwise captured by our XML files.
            // Things captured in XML files:
            //   CurrentDisasterList
            //   CurrentDisasterListFilters
            //   CurrentOtherSettingsList
            //   FilterProfileList
            //   ZoneChoices
            //   OrgPolicyList
            //   OrgDataList - all hospitals/orgs defined
            //   OrgContactInfoList - ignore all but first entry on list
            //   UserAndVersions list

            // Much of this is handled in DoStartup
            // For this to work for non-strings, supposedly "[DataContract]" should preface struct/class declaration, and "[DataMember]" prefix each member var
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_EventsDataItem));
            SuspensionManager.KnownTypes.Add(typeof(TP_PatientReport)); // Added April 28, 2014
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_OtherSettings));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_FilterProfile));
            SuspensionManager.KnownTypes.Add(typeof(TP_PatientDataGroups));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(SortByItem));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_OrgPolicy));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_OrgContactInfo));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(PLWS.plusWebServicesPortTypeClient));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(LPF_JSON));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(TP_ErrorLog));
            // Probably not: SuspensionManager.KnownTypes.Add(typeof(ProtectData));

            // Complex:
            SuspensionManager.SessionState["CurrentPatient"] = CurrentPatient; // Added April 28, 2014
            SuspensionManager.SessionState["PatientDataGroups"] = PatientDataGroups;

            // Globals of type string:
            SuspensionManager.SessionState["RosterNames"] = RosterNames;
            SuspensionManager.SessionState["CurrentSearchQuery"] = CurrentSearchQuery;
            SuspensionManager.SessionState["CurrentSearchResultsGroupName"] = CurrentSearchResultsGroupName;
            SuspensionManager.SessionState["SearchResultsEventTitleTextBasedOnCurrentFilterProfile"] = SearchResultsEventTitleTextBasedOnCurrentFilterProfile;
            // Don't need, regenerate instead: SuspensionManager.SessionState["UserWin8Account"] = UserWin8Account;
            SuspensionManager.SessionState["DeviceName"] = DeviceName;
            SuspensionManager.SessionState["DelayedMessageToUserOnStartup"] = DelayedMessageToUserOnStartup;


            // Globals of type bool:
            SuspensionManager.SessionState["ReportAltered"] = ReportAltered; // used by ViewEditReport
            SuspensionManager.SessionState["ReportDiscared"] = ReportDiscarded; // used by ViewEditReport
            SuspensionManager.SessionState["OutboxCheckBoxCurrentEventOnly"] = OutboxCheckBoxCurrentEventOnly;
            SuspensionManager.SessionState["OutboxCheckBoxMyOrgOnly"] = OutboxCheckBoxMyOrgOnly;
            SuspensionManager.SessionState["AllStationsCheckBoxMyOrgOnly"] = AllStationsCheckBoxMyOrgOnly;
            SuspensionManager.SessionState["BlockWebServices"] = BlockWebServices;
            // Probably not: goodWebServiceConnectivity = false; // Determined by pings.  Don't bother with stale data:
            //   (bool)SuspensionManager.SessionState["goodWebServiceConnectivity"];
            SuspensionManager.SessionState["AppFinishedLaunching"] = AppFinishedLaunching;
            SuspensionManager.SessionState["ProcessingAllStationsList"] = ProcessingAllStationsList;

            // followed by SuspensionManager.SaveAsync();
        }

        private static async Task DoResumeFromTermination()
        {
            // Called after DoStartup.  Organized here by data type.
            // Globals of type string:
            RosterNames = (string)SuspensionManager.SessionState["RosterNames"];
            CurrentSearchQuery = (string)SuspensionManager.SessionState["CurrentSearchQuery"];
            CurrentSearchResultsGroupName = (string)SuspensionManager.SessionState["CurrentSearchResultsGroupName"];
            SearchResultsEventTitleTextBasedOnCurrentFilterProfile = (string)SuspensionManager.SessionState["SearchResultsEventTitleTextBasedOnCurrentFilterProfile"];
            if (String.IsNullOrEmpty(UserWin8Account))
                // May be empty string if PC Settings/Prvacy/"Let apps use my name and account picture" is false
                UserWin8Account = await GetUserWin8Account(); // Regenerate instead of: UserWin8Account = (string)SuspensionManager.SessionState["UserWin8Account"];
            if (String.IsNullOrEmpty(DeviceName))
                DeviceName = GetDeviceName(); // Regenerate instead of: DeviceName = (string)SuspensionManager.SessionState["DeviceName"];
            DelayedMessageToUserOnStartup = (string)SuspensionManager.SessionState["DelayedMessageToUserOnStartup"]; 

            // Globals of type bool:
            ReportAltered = (bool)SuspensionManager.SessionState["ReportAltered"]; // used by ViewEditReport
            ReportDiscarded = (bool)SuspensionManager.SessionState["ReportDiscared"]; // used by ViewEditReport
            OutboxCheckBoxCurrentEventOnly = (bool)SuspensionManager.SessionState["OutboxCheckBoxCurrentEventOnly"];
            OutboxCheckBoxMyOrgOnly = (bool)SuspensionManager.SessionState["OutboxCheckBoxMyOrgOnly"];
            AllStationsCheckBoxMyOrgOnly = (bool)SuspensionManager.SessionState["AllStationsCheckBoxMyOrgOnly"];
            BlockWebServices = (bool)SuspensionManager.SessionState["BlockWebServices"];
            // Probably not: goodWebServiceConnectivity = false; // Determined by pings.  Don't bother with stale data: (bool)SuspensionManager.SessionState["goodWebServiceConnectivity"];
            AppFinishedLaunching = (bool)SuspensionManager.SessionState["AppFinishedLaunching"];
            ProcessingAllStationsList = (bool)SuspensionManager.SessionState["ProcessingAllStationsList"];

            // Complex types:
            // Most of restoration of these is handled in DoStartup, but we'll see what's needed here.

            // Probably not: dispatcher = (CoreDispatcher)SuspensionManager.SessionState["dispatcher"];
            // Probably not: CurrentDisasterListForCombo = (ObservableCollection<TP_EventsDataItem>)SuspensionManager.SessionState["CurrentDisasterListForCombo"];
            // Probably not: CurrentDisaster = (TP_EventsDataItem)SuspensionManager.SessionState["CurrentDisaster"];
            CurrentPatient = (TP_PatientReport)SuspensionManager.SessionState["CurrentPatient"]; // added April 28, 2014.  Current Patient has uncompleted New Report, might be swapped out when taking photo
            // Probably not: CurrentOtherSettings = (TP_OtherSettings)SuspensionManager.SessionState["CurrentOtherSettings"];
            // Probably not: ViewedDisaster = (TP_EventsDataItem)SuspensionManager.SessionState["ViewedDisaster"];  // used by ViewEditReport
            // Probably not: CurrentFilterProfile = (TP_FilterProfile)SuspensionManager.SessionState["CurrentFilterProfile"];
            PatientDataGroups = (TP_PatientDataGroups)SuspensionManager.SessionState["PatientDataGroups"];
            // Probably not: SortFlyoutItem = (SortByItem)SuspensionManager.SessionState["SortFlyoutItem"];
            // Probably not: OrgPolicy = (TP_OrgPolicy)SuspensionManager.SessionState["OrgPolicy"]; // first entry on OrgPolicyList
            // Probably not: CurrentOrgContactInfo = (TP_OrgContactInfo)SuspensionManager.SessionState["CurrentOrgContactInfo"]; // first entry on OrgContactInfoList
            // Probably not: pl = (PLWS.plusWebServicesPortTypeClient)SuspensionManager.SessionState["pl"];
            // Probably not: service = (LPF_JSON)SuspensionManager.SessionState["service"];
            // Probably not: ErrorLog = (TP_ErrorLog)SuspensionManager.SessionState["ErrorLog"];
            // Probably not: pd = (ProtectData)SuspensionManager.SessionState["pd"];
            // Probably not: sendQueue = (TP_SendQueue)SuspensionManager.SessionState["sendQueue"];
        }

        #region Settings

        private static void RegisterSettings()
        {
            var pane = SettingsPane.GetForCurrentView();
            pane.CommandsRequested += Pane_CommandsRequested;
        }


        static void Pane_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // See http://msdn.microsoft.com/en-us/library/windows/apps/bg182878.aspx#SettingsFlyout
            // Pane_CommandsRequested() was originally implemented with CALLISTO flyout controls in Win 8.0.
            // Compared to Callisto, more content is supplied in flyout XAML rather than here in C#

            string heading = "My Credentials";
            Windows.UI.ApplicationSettings.SettingsCommand credentialsCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutCredentials sfCredentials = new SettingsFlyoutCredentials();
                    sfCredentials.Show();
                });

            args.Request.ApplicationCommands.Add(credentialsCommand);

            heading = "Policy Options, Set Centrally"; // Title set in XAML is simply "Policy Options", to avoid "...".  Section title below it is "Options Set Centrally"
            Windows.UI.ApplicationSettings.SettingsCommand optionsCentralCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutOptionsCentral sfOptionsCentral = new SettingsFlyoutOptionsCentral();
                    sfOptionsCentral.Show();
                });

            args.Request.ApplicationCommands.Add(optionsCentralCommand);

            heading = "Options Set Here";
            Windows.UI.ApplicationSettings.SettingsCommand optionsLocalCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutOptionsLocal sfOptionsLocal = new SettingsFlyoutOptionsLocal();
                    sfOptionsLocal.Show();
                });

            args.Request.ApplicationCommands.Add(optionsLocalCommand);

            heading = "My Organization";
            Windows.UI.ApplicationSettings.SettingsCommand orgCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutMyOrg sfOrg = new SettingsFlyoutMyOrg();
                    sfOrg.Show();
                });

            args.Request.ApplicationCommands.Add(orgCommand);

            heading = "Data Privacy";
            Windows.UI.ApplicationSettings.SettingsCommand privacyCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutPrivacy sfPrivacy = new SettingsFlyoutPrivacy();
                    sfPrivacy.Show();
                });

            args.Request.ApplicationCommands.Add(privacyCommand);

            heading = "About + Support"; // Use "+" insted of "&" because of width constraints on title, to avoid "..."
            Windows.UI.ApplicationSettings.SettingsCommand aboutCommand =
                new Windows.UI.ApplicationSettings.SettingsCommand(heading, heading, (handler) =>
                {
                    SettingsFlyoutAbout sfAbout = new SettingsFlyoutAbout();
                    sfAbout.Show();
                });

            args.Request.ApplicationCommands.Add(aboutCommand);
        }

        #endregion


        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            if(App.LocalStorageDataSemaphore.CurrentCount == 0) // awaiting read/write xml competion
            {
                bool done = await App.LocalStorageDataSemaphore.WaitAsync(500); // wait 1/2 second for completion.  If didn't (and timedout) will get back false
                if (done)
                    App.LocalStorageDataSemaphore.Release(); // cleanup
                // TO DO: else case, maybe put semaphore in suspension manager... but localStorage.Data is likely bunged on resume
            }

            DoSaveOnTermination();
            await SuspensionManager.SaveAsync();
            // Generally, we are saving everything as we go along, so shouldn't have to do this here...
            await PatientDataGroups.GetOutbox().WriteXML();
            await PatientDataGroups.GetAllStations().WriteXML("PatientReportsAllStations.xml"); // REVISIT WHEN WE HAVE ACTUALLY WEB SERVICES
            //   CurrentDisasterList
            //   CurrentDisasterListFilters
            //   CurrentOtherSettingsList
            //   FilterProfileList
            //   ZoneChoices
            //   OrgPolicyList
            //   OrgDataList - all hospitals/orgs defined
            //   OrgContactInfoList - ignore all but first entry on list
            //   UserAndVersions list

            // Petzold's suggested way to save stack of page navigation, but I think SuspensionManager does that for us:
            // ApplicationDataContainer appData = ApplicationData.Current.LocalSettings;
            // appData.Values["NavigationState"] = (Window.Current.Content as Frame).GetNavigationState(); // end Petzold
            deferral.Complete();
        }

/* MAYBE TO DO
        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            // Suggestion from MSDN
            // At window creation time, access the SearchPane object and register SearchPane event handlers.
            // (like Query Submittted, SuggestionsRequested, and ResultSuggestionChosen) so that
            // can respond to the user's search queries _at any time_.

            // Get search pane
            Windows.ApplicationModel.Search.SearchPane searchPane = Windows.ApplicationModel.Search.SearchPane.GetForCurrentView();
            // Register a QuerySubmitted event handler:
            // TO DO
            // Register a suggestionsRequested if your app displays its own suggestions in the search pane
            searchPane.SuggestionsRequested += SearchResultsPage.SearchResultsPage_SuggestionsRequested;
            // Register a ResultsSuggestionChosen if you app displays result suggestion in the search pane
            // TO DO
        }
    */

        /// <summary>
        /// Invoked when the application is activated to display search results.
        /// </summary>
        /// <param name="args">Details about the activation request.</param>
        protected async override void OnSearchActivated(Windows.ApplicationModel.Activation.SearchActivatedEventArgs args)
        {
            // TODO: Register the Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().QuerySubmitted
            Windows.ApplicationModel.Search.SearchPane.GetForCurrentView().SuggestionsRequested += SearchResultsPage.SearchResultsPage_SuggestionsRequested;
            // event in OnWindowCreated to speed up searches once the application is already running

            // If the Window isn't already using Frame navigation, insert our own Frame
            var previousContent = Window.Current.Content;
            var frame = previousContent as Frame;

            // If the app does not contain a top-level frame, it is possible that this 
            // is the initial launch of the app. Typically this method and OnLaunched 
            // in App.xaml.cs can call a common method.
            if (frame == null)
            {
                // Create a Frame to act as the navigation context and associate it with
                // a SuspensionManager key
                frame = new Frame();
                TP8.Common.SuspensionManager.RegisterFrame(frame, "AppFrame");

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Restore the saved session state only when appropriate
                    try
                    {
                        await TP8.Common.SuspensionManager.RestoreAsync();
                    }
                    catch (TP8.Common.SuspensionManagerException)
                    {
                        //Something went wrong restoring state.
                        //Assume there is no state and continue
                    }
                }
            }

            frame.Navigate(typeof(SearchResultsPage), args.QueryText);
            Window.Current.Content = frame;

            // Ensure the current window is active
            Window.Current.Activate();
        }

        // Utilties:
        /// <summary>
        /// Return the name of the current logged on user.
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetUserWin8Account()
        {
            // These calls may return null or empty strings
            // Next 3 calls will be empty string if PC Settings/Prvacy/"Let apps use my name and account picture" is false
            string displayName = await UserInformation.GetDisplayNameAsync(); // Blockable by privacy settings, UserInformation.NameAccessAllowed.
            // If user is logged on with Microsoft account, this additional info is available:
            string firstName = await UserInformation.GetFirstNameAsync();
            string lastName = await UserInformation.GetLastNameAsync();
            if (String.IsNullOrEmpty(displayName) && String.IsNullOrEmpty(firstName) && String.IsNullOrEmpty(lastName))
                return "";
            if (String.IsNullOrEmpty(displayName))
                return firstName + " " + lastName;
            if (String.IsNullOrEmpty(firstName) && String.IsNullOrEmpty(lastName))
                return displayName;
            return displayName + " (" + firstName + " " + lastName + ")"; // displayName is probably MS Live email address in this case
            // It is also possible to get user's account picture
        }

        /// <summary>
        /// Returns the local win 8 host name.
        /// </summary>
        /// <returns></returns>
        private static string GetDeviceName()
        {
            // This is the local win 8 host name, not the hardware manufacturer.
            // From social.msdn.microsoft.com/Forums/windowsapps/en-US/.../retrieve-computer-name-and-the-mac-id-of-the-device
            var names = NetworkInformation.GetHostNames();

            int foundIdx;
            for (int i = 0; i < names.Count; i++)
            {
                foundIdx = names[i].DisplayName.IndexOf(".local");
                if (foundIdx > 0)
                    return names[i].DisplayName.Substring(0, foundIdx);
            }
            return ""; // failed
        }

    }
}
