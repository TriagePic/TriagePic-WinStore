using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class SettingsFlyoutOptionsCentral : SettingsFlyout
    {
        public SettingsFlyoutOptionsCentral()
        {
            InitializeComponent();
            string prefix = "(none)";
            if (!String.IsNullOrEmpty(App.OrgPolicy.OrgPatientIdPrefixText))
                prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            string format = "Variable length (1-9 digits)";
            if (App.OrgPolicy.OrgPatientIdFixedDigits > 0)
                format = App.OrgPolicy.OrgPatientIdFixedDigits + " digits, leading zeroes";
            string zl = "";
            foreach (string s in App.ZoneChoices.GetZoneChoices())
                if (s != "") // skip empty string that means 'no choice yet'
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
