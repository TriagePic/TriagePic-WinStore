using SocialEbola.Lib.PopupHelpers;
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

            StartupPasswordStatus.Text = "";
            StartupOrgComboBox.ItemsSource = App.OrgDataList;
            StartupOrgComboBox.SelectedIndex = 0; // Until we know otherwise
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgName))
            {
                int count = 0;
                foreach (var i in App.OrgDataList) //TP_EventsDataList.GetEvents())
                {
                    if (i.OrgName == App.CurrentOrgContactInfo.OrgName)  // Could match instead throughout on EventShortName
                    {
                        StartupOrgComboBox.SelectedIndex = count;
                        break;
                    }
                    count++;
                }
                // if no match, then remain with first item selected
            }

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

            /*string s = */
            await App.CurrentOrgContactInfo.GetCurrentOrgContactInfo(d.OrgUuid, d.OrgName);
            // Could check s for "ERROR:"
            // ALREADY DONE BY GetCurrentOrgContactInfo:
            // App.OrgContactInfoList.Clear();
            // App.OrgContactInfoList.Add(App.CurrentOrgContactInfo);
            await App.OrgContactInfoList.WriteXML();
#if PROBABLYNOTHERE
            // See if patient ID format changes
            int format = App.OrgPolicy.OrgPatientIdFixedDigits;
            string prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            await App.OrgPolicyList.Init();
            if (App.OrgPolicyList.Count() > 0)
                App.OrgPolicy = App.OrgPolicyList.First(); // FirstOrDefault(); // will return null if nothing in list
            if (prefix != App.OrgPolicy.OrgPatientIdPrefixText)
                App.CurrentPatient.PatientID.Replace(prefix, App.OrgPolicy.OrgPatientIdPrefixText);
            if (format != App.OrgPolicy.OrgPatientIdFixedDigits)
                App.CurrentPatient.PatientID = App.OrgPolicy.ForceValidFormatID(App.CurrentPatient.PatientID); // maybe this will work
#endif

        }

        private void StartupDoneButton_Click(object sender, RoutedEventArgs e)
        {
            App.pd.plUserName = StartupTextBoxUserNamePLUS.Text;
            App.pd.plPassword = StartupPasswordBoxPLUS.Password;
            m_startupWiz.Result = 1;  
            m_startupWiz.CloseAsync();
        }


        private void StartupValidateButton_Click(object sender, RoutedEventArgs e)
        {
            var t = Validate(); // assign to t to suppress compiler warning
        }

        private async Task Validate()
        {
            string results = await App.service.VerifyPLCredentials(StartupTextBoxUserNamePLUS.Text, StartupPasswordBoxPLUS.Password, true /*hospitalStaffOrAdminOnly*/);
            if (results == "")
            {
                StartupPasswordStatus.Text = "User name and password are valid";
                // TO DO:  Save validated change to App.UserAndVersions.  See TP_UserAndVersions
            }
            else
                StartupPasswordStatus.Text = "User name and/or password INVALID";
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
            /// Caller does: App.pd.EncryptAndBase64EncodePLCredentials(); // might be slow
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
 
            m_startupWiz.Result = 0; // means cancel
            m_startupWiz.CloseAsync();
            // Let's try this:
            Window.Current.Close(); // Will suspend the app.  Since only 1 current window, will go back to start screen
            // The big gun would be, but this might fail Store certification:
            // Application.Current.Exit();
        }
    }
}
