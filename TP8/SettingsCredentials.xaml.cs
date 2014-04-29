using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8
{
    public sealed partial class SettingsCredentials
    {
        public SettingsCredentials()
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
            TextBoxUserNamePLUS.Text = App.pd.plUserName;
            PasswordBoxPLUS.Password = App.pd.plPassword;
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
            var t = UpdateCredentialsAndCheckSyntax();
        }

        private void PasswordBoxPLUS_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var t = UpdateCredentialsAndCheckSyntax();
        }

// PROBLEMATIC
//        private bool UpdateCredentialsAndCheckSyntaxSync()
//        {
//            // Version to call from constructor
//            var output = UpdateCredentialsAndCheckSyntax();
//            return output.Result; // blocks on results
//        }
//
        private async Task<bool> UpdateCredentialsAndCheckSyntax()
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
            await App.pd.EncryptAndBase64EncodePLCredentialsAsync();
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

    }
}
