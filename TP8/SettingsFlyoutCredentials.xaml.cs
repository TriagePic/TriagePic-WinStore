using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace TP8
{
    public sealed partial class SettingsFlyoutCredentials : SettingsFlyout
    {
        private string plUserNameOrig;
        private string plPasswordOrig;
        private string plTokenOrig;
        public SettingsFlyoutCredentials()
        {
            InitializeComponent();
            if (String.IsNullOrEmpty(App.UserWin8Account)) // UserWin8Account may be empty string if user has PC Settings/Privacy/"Let apps use my name and account picture" as false.
            {
                TextBlockMyWin8Login.Text =
                    "  [Login name not available to apps,\n" +
                    "  due to your PC Settings/Privacy choice]\n" + 
                    "  At Device: " + App.DeviceName;
            }
            else
            {
                TextBlockMyWin8Login.Text =
                    "  " + App.UserWin8Account + "\n" + // UserWin8Account may be empty string if user has PC Settings/Privacy/"Let apps use my name and account picture" as false.
                    "  At Device: " + App.DeviceName;
            }
            plUserNameOrig = TextBoxUserNamePLUS.Text = App.pd.plUserName;
            plPasswordOrig = PasswordBoxPLUS.Password = App.pd.plPassword;
            plTokenOrig = App.pd.plToken;
            PasswordStatus.Text = "";
            // NO, Can cause lockup due to synchronous call:
            //if (UpdateCredentialsAndCheckSyntaxSync())
            //{
                var t = Validate();  // assign to t to suppress compiler warning
            //}
        }

        private void ValidateButton_Click(object sender, RoutedEventArgs e)
        {
            var t = Validate(); // assign to t to suppress compiler warning
        }

        private async Task Validate()
        {
            string results = await App.service.VerifyPLCredentials(TextBoxUserNamePLUS.Text, PasswordBoxPLUS.Password, true /*hospitalStaffOrAdminOnly*/);
            if (results == "")
            {
                PasswordStatus.Text = "User name and password are valid";
                // TO DO:  Save validated change to App.UserAndVersions.  See TP_UserAndVersions
            }
            else
                PasswordStatus.Text = "User name and/or password INVALID";
        }

        private void TextBoxUserNamePLUS_TextChanged(object sender, Windows.UI.Xaml.Controls.TextChangedEventArgs e)
        {
            /* bool t = */ UpdateCredentialsAndCheckSyntax();
        }

        private void PasswordBoxPLUS_PasswordChanged(object sender, RoutedEventArgs e)
        {
            /* bool t = */ UpdateCredentialsAndCheckSyntax();
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

            string u = App.pd.plUserName = TextBoxUserNamePLUS.Text;
            string s = App.pd.plPassword = PasswordBoxPLUS.Password;
            // caller does, after token fetch: await App.pd.EncryptAndBase64EncodePLCredentialsAsync();
            if (u.Length == 0  && s.Length == 0)
            {
                PasswordStatus.Text = "Please enter user name & password"; return false;
            }
            if (u.Length == 0)
            {
                PasswordStatus.Text = "Please enter user name"; return false;
            }
            if(s.Length == 0)
            {
                PasswordStatus.Text = "Please enter password"; return false;
            }
            if (s.Contains(TextBoxUserNamePLUS.Text)) // already checked that length not zero
            {
                PasswordStatus.Text = "Password must not contain user name"; return false;
            }
            if (s.Length < 8)
            {
                PasswordStatus.Text = "Password too short"; return false;
            }
            bool test = false;
            for (int i = 0; i <s.Length; i++)
                if (char.IsLower(s[i]))
                {
                    test = true;
                    break;
                }
            if(!test)
            {
                PasswordStatus.Text = "Password needs at least 1 lowercase letter"; return false;
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
                PasswordStatus.Text = "Password needs at least 1 uppercase letter"; return false;
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
                PasswordStatus.Text = "Password needs at least 1 digit"; return false;
            }
            PasswordStatus.Text = "User name and password syntax OK"; return true;
        }

        private async void SettingsFlyout_BackClick(object sender, BackClickEventArgs e)
        {
            if(plUserNameOrig != App.pd.plUserName || plPasswordOrig != App.pd.plPassword || plTokenOrig != App.pd.plToken)
            {
                await App.pd.EncryptAndBase64EncodePLCredentialsAsync();
                TP_UserNameAndVersion o = new TP_UserNameAndVersion();
                o.User = App.UserWin8Account;
                await App.UserAndVersions.AddOrUpdateEncryptedDataRowInXML(o);
            }
        }
    }
}
