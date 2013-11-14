using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Popups;
using Windows.UI.Xaml;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8
{
    public sealed partial class SettingsOptionsCentral // : DependencyObject
    {
        public SettingsOptionsCentral()
        {
            InitializeComponent();
            string prefix = "(none)";
            if (!String.IsNullOrEmpty(App.OrgPolicy.OrgPatientIdPrefixText))
                prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            string format = "Variable length (1-9 digits)";
            if (App.OrgPolicy.OrgPatientIdFixedDigits > 0)
                format = App.OrgPolicy.OrgPatientIdFixedDigits + " digits, leading zeroes";
            string zl = "";
            foreach(string s in App.ZoneChoices.GetZoneChoices())
                if(s != "") // skip empty string that means 'no choice yet'
                    zl += "  " + s + "\n";

            SomeText.Text =
                "\nFor " + App.CurrentOrgContactInfo.OrgName +
                "\n\nFormat of Patient ID/Mass Casualty ID\n" +
                "  Optional Fixed Prefix: '" + prefix + "'\n" +
                "  Numeric Format: " + format + "\n" +
                "\nTriage Zone Choices\n" +
                zl;
        }


    }
}
