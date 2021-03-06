﻿using SocialEbola.Lib.PopupHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TP8.Data;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8
{
    public sealed partial class StartupWizUserControl : UserControl, IPopupControl
    {
        public StartupWizUserControl()
        {
            this.InitializeComponent();

            // Oct 2014/v33 redo: Turned on property to password status to allow user to ask to reveal password (an "eye" button in right of field)
            StartupPasswordStatus.Text = "";
            StartupOrgChoice.Visibility = Visibility.Collapsed; // enclose in grid of min height to reserve space
            StartupOrgComboBox.IsEnabled = false; // new v33
            StartupContinue.IsEnabled = false; // new v33
            // Move to on_loaded to get this to work: StartupTextBoxUserNamePLUS.Focus(FocusState.Programmatic);
#if BEFORE_PLUS_v33
            StartupOrgComboBox.ItemsSource = App.OrgDataList;
            // Compare with similar code in SettingsMyOrg
            // No, don't trigger handler here: StartupOrgComboBox.SelectedIndex = 0; // Until we know otherwise
            int choice = 0;
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgName))
            {
                int count = 0;
                foreach (var i in App.OrgDataList) //TP_EventsDataList.GetEvents())
                {
                    if (i.OrgName == App.CurrentOrgContactInfo.OrgName)  // Could match instead throughout on EventShortName
                    {
                        choice = count;// StartupOrgComboBox.SelectedIndex = count;
                        break;
                    }
                    count++;
                }
                // if no match, then remain with first item selected
            }
            StartupOrgComboBox.SelectedIndex = choice; // trigger handler
#endif
        }

		private StartupWiz m_startupWiz;

        public class StartupWiz : PopupHelperWithResult<Int32, StartupWizUserControl>
        {
            // Provisionally use Int32, startup step, as return value
        }


		public void SetParent(PopupHelper parent)
		{
			m_startupWiz = (StartupWiz)parent;
            m_startupWiz.Result = 0; // controls not changed
		}

        public void Opened() {}
        
        public void Closed(CloseAction action) {}

        // Adding combo box to startup wizard hangs system on startup (unless break all/continue in debugger.
        // This problem seen before.  Probably need to rethink flow in App.cs, maybe add new first-time home page?

        private async void StartupOrgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.MyAssert(App.CurrentOrgContactInfo != null); // but all fields may be empty

            if (StartupOrgComboBox.SelectedItem == null)
                return;

            TP_OrgData d = (TP_OrgData)StartupOrgComboBox.SelectedValue;
            if (d.OrgName == App.CurrentOrgContactInfo.OrgName)
                return;

            /*string s = */ await App.CurrentOrgContactInfo.GetCurrentOrgContactInfo(d.OrgUuid, d.OrgName);
            // Could check s for "ERROR:"
            // ALREADY DONE BY GetCurrentOrgContactInfo:
            // App.OrgContactInfoList.Clear();
            // App.OrgContactInfoList.Add(App.CurrentOrgContactInfo);
            await App.OrgContactInfoList.WriteXML();
        }

        /// <summary>
        /// Allow user press of keyboard Enter in password box to in effect tap "Validate and Continue" button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void StartupPasswordBoxPLUS_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // Tried this with KeyDown instead of KeyUp at first but known problem with double-event-firing with Enter, see:
            // https://social.msdn.microsoft.com/Forums/windowsapps/en-US/734d6c7a-8da2-48c6-9b3d-fa868b4dfb1d/c-textbox-keydown-triggered-twice-in-metro-applications?forum=winappswithcsharp
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                await StartupValidateAndContinueButtonImpl();
            }
        }

        private async void StartupValidateAndContinueButton_Click(object sender, RoutedEventArgs e)
        {
            await StartupValidateAndContinueButtonImpl();
        }

        private async Task StartupValidateAndContinueButtonImpl()
        {
            await Validate();
            if (StartupPasswordStatus.Text != "User name and password are valid") // may instead be INVALID, or some other message if Enter hit
                return;

            App.MyAssert(App.pd.plToken != null && App.pd.plToken.Length == 128); // App.pd.plToken was set by Validate() above
            StartupCancel.IsEnabled = false; // new PLUS v33
            StartupValidateAndContinue.IsEnabled = false; // new PLUS v33
            App.MyAssert(App.pd.plUserName == StartupTextBoxUserNamePLUS.Text); // already assigned in UpdateCredentialsAndCheckSyntax()
            App.MyAssert(App.pd.plPassword == StartupPasswordBoxPLUS.Password); // ditto
            // Before PLUS v33, then moved:
            //m_startupWiz.Result = 1;  
            //m_startupWiz.CloseAsync();
            // Rest of function is new with PLUS v33:
            await App.OrgDataList.Init(); // uses App.pd.plToken
            if(App.OrgDataList.Count() == 0) // New Release 6
            {
                // Fatal error... no web connectivity and no org cache file
                if (App.DelayedMessageToUserOnStartup != "") // Got content during App.OnLaunched, but can't be easily shown until now
                {
                    // Message will be:
                        // Could not connect to TriageTrak web service.  Using previous information, cached locally, instead for:
                        // - List of organizations (e.g., hospitals)
                        // Nor was this list available from a cache file on this machine!
                        //
                        // PLEASE EXIT, establish an internet connection, then retry TriagePic.
                    // Let's rewrite so not so clunky:
                    App.DelayedMessageToUserOnStartup =
                        "Could not connect to TriageTrak web service, nor find previous information (remembered on this device) that would let us continue.\n" +
                        "PLEASE EXIT, establish an internet connection, then retry TriagePic.";
                    StartupCancel.IsEnabled = true;
                    StartupCancel.Content = "EXIT";
                    StartupPasswordStatus.Text = App.DelayedMessageToUserOnStartup; // User will be told to EXIT
                    //MessageDialog dlg = new MessageDialog(App.DelayedMessageToUserOnStartup);
                    //await dlg.ShowAsync();
                    App.DelayedMessageToUserOnStartup = "";
                }
                return;
            }
            StartupOrgChoice.Visibility = Visibility.Visible;
            StartupOrgComboBox.IsEnabled = true;
            StartupContinue.IsEnabled = true;
            // code below was moved from .Init, where it lived prior to v33:
            StartupOrgComboBox.ItemsSource = App.OrgDataList;
            // Compare with similar code in SettingsMyOrg
            // No, don't trigger handler here: StartupOrgComboBox.SelectedIndex = 0; // Until we know otherwise
            int choice = 0;
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgName))
            {
                int count = 0;
                foreach (var i in App.OrgDataList) //TP_EventsDataList.GetEvents())
                {
                    if (i.OrgName == App.CurrentOrgContactInfo.OrgName)  // Could match instead throughout on EventShortName
                    {
                        choice = count;// StartupOrgComboBox.SelectedIndex = count;
                        break;
                    }
                    count++;
                }
                // if no match, then remain with first item selected
            }
            StartupOrgComboBox.SelectedIndex = choice; // trigger handler

        }

        private async Task Validate()
        {
            string results = await App.service.VerifyPLCredentials(StartupTextBoxUserNamePLUS.Text, StartupPasswordBoxPLUS.Password, true /*hospitalStaffOrAdminOnly*/);
            if (results == "")
            {
                StartupPasswordStatus.Text = "User name and password are valid";
                // Caller saves validated change to App.pd and UserAndVersions XML.  See TP_UserAndVersions
            }
            else
                StartupPasswordStatus.Text = "User name and/or password INVALID";
        }

        /// <summary>
        /// Set focus to user name box initially
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupTextBoxUserNamePLUS_Loaded(object sender, RoutedEventArgs e)
        {
            StartupTextBoxUserNamePLUS.Focus(FocusState.Programmatic);
        }
        
        private void StartupTextBoxUserNamePLUS_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            UpdateCredentialsAndCheckSyntax();
        }

        private void StartupPasswordBoxPLUS_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdateCredentialsAndCheckSyntax();
        }

        private bool UpdateCredentialsAndCheckSyntax()
        {
            // Would be better to have a settingsFlyout onClose handler, but how to do that not so clear
            // Password:
            // Min length: 8 chars
            // Max length: 16 chars (enforced by entry field)
            // Must have:
            //  at least 1 uppercase
            //  at least 1 lowercase
            //  at least 1 numeral
            // Cannot contain username

            // User name: no spaces

            string u = App.pd.plUserName = StartupTextBoxUserNamePLUS.Text;
            string s = App.pd.plPassword = StartupPasswordBoxPLUS.Password;
            // Caller does: App.pd.EncryptAndBase64EncodePLCredentials(); // might be slow
            if (u.Length == 0 && s.Length == 0)
            {
                StartupPasswordStatus.Text = "Please enter user name & password"; return false;
            }
            if (u.Length == 0)
            {
                StartupPasswordStatus.Text = "Please enter user name"; return false;
            }
            if (s.Length == 0)
            {
                StartupPasswordStatus.Text = "Please enter password"; return false;
            }
            if (s.Contains(StartupTextBoxUserNamePLUS.Text)) // already checked that length not zero
            {
                StartupPasswordStatus.Text = "Password must not contain user name"; return false;
            }
            if (s.Length < 8)
            {
                StartupPasswordStatus.Text = "Password too short"; return false;
            }
            bool test = false;
            for (int i = 0; i < s.Length; i++)
                if (char.IsLower(s[i]))
                {
                    test = true;
                    break;
                }
            if (!test)
            {
                StartupPasswordStatus.Text = "Password needs at least 1 lowercase letter"; return false;
            }
            test = false;
            for (int i = 0; i < s.Length; i++)
                if (char.IsUpper(s[i]))
                {
                    test = true;
                    break;
                }
            if (!test)
            {
                StartupPasswordStatus.Text = "Password needs at least 1 uppercase letter"; return false;
            }
            test = false;
            for (int i = 0; i < s.Length; i++)
                if (char.IsDigit(s[i]))
                {
                    test = true;
                    break;
                }
            if (!test)
            {
                StartupPasswordStatus.Text = "Password needs at least 1 digit"; return false;
            }
            StartupPasswordStatus.Text = "User name and password syntax OK"; return true;
        }

        private void StartupCancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.pd.plUserName = ""; // cheap hack
            App.pd.plPassword = "";
            App.pd.plToken = "";
 
            m_startupWiz.Result = 0; // means cancel
            m_startupWiz.CloseAsync();
            // Let's try this:
            //try
            //{
            //    worked in 8.0 without try/catch, but not 8.1: Window.Current.Close(); // Will suspend the app.  Since only 1 current window, will go back to start screen
                // 8.1: generated InvalidOperation exception (closing main Window in Store app) if not caught
            //}
            //catch (Exception)
            //{ }
            // The big gun would be, but this might fail Store certification:
            Application.Current.Exit();
        }

        // New button with PLUS v33, organization choice moved to AFTER user login:
        private void StartupContinue_Click(object sender, RoutedEventArgs e)
        {
            m_startupWiz.Result = 1;
            m_startupWiz.CloseAsync();
        }

        /// <summary>
        /// Allow user press of keyboard Enter in username box to signal move to password box.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartupTextBoxUserNamePLUS_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            // See comment in StartupPasswordBoxPLUS_KeyUp as to why we're using key up, not key down.
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                StartupPasswordBoxPLUS.Focus(FocusState.Programmatic);
            }
        }

    }
}
