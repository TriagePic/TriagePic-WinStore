using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class SettingsFlyoutMyOrg : SettingsFlyout
    {
        public SettingsFlyoutMyOrg()
        {
            App.MyAssert(App.CurrentOrgContactInfo != null); // but all fields may be empty

            InitializeComponent();

            OrgComboBox.ItemsSource = App.OrgDataList;
            // No, don't trigger handler here: OrgComboBox.SelectedIndex = 0; // Until we know otherwise
            int choice = 0;
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgName))
            {
                int count = 0;
                foreach (var i in App.OrgDataList) //TP_EventsDataList.GetEvents())
                {
                    if (i.OrgName == App.CurrentOrgContactInfo.OrgName)  // Could match instead throughout on EventShortName
                    {
                        choice = count;//OrgComboBox.SelectedIndex = count;
                        break;
                    }
                    count++;
                }
                // if no match, then remain with first item selected
            }
            OrgComboBox.SelectedIndex = choice; // trigger handler

            // move to handler: DisplayCurrentOrgContactInfo();
        }

        private void DisplayCurrentOrgContactInfo()
        {
            App.MyAssert(App.CurrentOrgContactInfo != null); // but all fields may be empty

            if (String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgName))
            {
                SomeTextOrgInfo.Text = "";
                return;
            }

            string s =
            App.CurrentOrgContactInfo.OrgName + "\n" +
            "  or simply " + App.CurrentOrgContactInfo.OrgAbbrOrShortName + "\n";
            // Assume everything else might be optional
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgStreetAddress1))
                s += App.CurrentOrgContactInfo.OrgStreetAddress1 + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgStreetAddress2))
                s += App.CurrentOrgContactInfo.OrgStreetAddress2 + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgTownOrCity))
                s += App.CurrentOrgContactInfo.OrgTownOrCity;
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.Org2LetterState)) // put on same line as town
                s += ", " + App.CurrentOrgContactInfo.Org2LetterState;
            s += "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgZipcode))
                s += App.CurrentOrgContactInfo.OrgZipcode + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgCountry))
                s += App.CurrentOrgContactInfo.OrgCountry + "\n";
            s += "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgCounty))
                s += "County or Region: " + App.CurrentOrgContactInfo.OrgCounty + "\n";
            s += "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgPhone))
                s += "Phone: " + App.CurrentOrgContactInfo.OrgPhone + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgFax))
                s += "Fax: " + App.CurrentOrgContactInfo.OrgFax + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgEmail))
                s += App.CurrentOrgContactInfo.OrgEmail + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgWebSite))
                s += App.CurrentOrgContactInfo.OrgWebSite + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgNPI))
                s += "NPI or other Facility ID: " + App.CurrentOrgContactInfo.OrgNPI + "\n";
            if (!String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgLatitude) &&
                !String.IsNullOrEmpty(App.CurrentOrgContactInfo.OrgLongitude))
                s += "GPS Location: " + App.CurrentOrgContactInfo.OrgLatitude + ", " + App.CurrentOrgContactInfo.OrgLongitude + "\n";

            SomeTextOrgInfo.Text = s;
        }

        private async void OrgComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.MyAssert(App.CurrentOrgContactInfo != null); // but all fields may be empty

            if (OrgComboBox.SelectedItem == null)
            {
                SomeTextOrgInfo.Text = "";
                return;
            }
         
            TP_OrgData d = (TP_OrgData)OrgComboBox.SelectedValue;
            if (d.OrgName == App.CurrentOrgContactInfo.OrgName)
            {
                DisplayCurrentOrgContactInfo();
                return;
            }

            SomeTextOrgInfo.Text = "";
            /*string s = */ await App.CurrentOrgContactInfo.GetCurrentOrgContactInfo(d.OrgUuid, d.OrgName);
            // Could check s for "ERROR:"
            DisplayCurrentOrgContactInfo();
            App.OrgContactInfoList.Clear();
            App.OrgContactInfoList.Add(App.CurrentOrgContactInfo);
            await App.OrgContactInfoList.WriteXML();
            // See if patient ID format changes (buggy before Release 7)
            string OldID = App.CurrentPatient.PatientID;
            int format = App.OrgPolicy.OrgPatientIdFixedDigits;
            string prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            App.OrgPolicyList.Clear(); // added Release 7, to coerce proper action on next line
            await App.OrgPolicyList.Init();
            if (App.OrgPolicyList.Count() > 0)
                App.OrgPolicy = App.OrgPolicyList.First(); // FirstOrDefault(); // will return null if nothing in list
            if (prefix != App.OrgPolicy.OrgPatientIdPrefixText)
                App.CurrentPatient.PatientID = App.CurrentPatient.PatientID.Replace(prefix, App.OrgPolicy.OrgPatientIdPrefixText);
            if (format != App.OrgPolicy.OrgPatientIdFixedDigits)
                App.CurrentPatient.PatientID = App.OrgPolicy.ForceValidFormatID(App.CurrentPatient.PatientID); // maybe this will work

            // If New Report page is visible, change ID field & set of zones too... new in Release 7
            if (Window.Current.Content == null)
                return;

            // This process could throw Invalid conversion exception, but otherwise seems OK:
            var _Frame = Window.Current.Content as Frame;
            Type type = _Frame.CurrentSourcePageType;
            if(type == null)
                return;

            if (type.Name != "BasicPageNew")
                return;

            var p = (BasicPageNew)_Frame.Content;
            if (OldID != App.CurrentPatient.PatientID)
            {
                // Fortunately, the same textbox is used for all layout modes:
                p.PatientIdTextBox.Text = App.CurrentPatient.PatientID;
            }

            p.ClearZoneButtons(); // Too hard to know if zone collection changed... just assume it did
            p.InitiateZones(); // TO DO: test this further when TT offers orgs with different zones
        }

    }
}
