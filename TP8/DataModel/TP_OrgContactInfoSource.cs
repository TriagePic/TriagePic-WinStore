//#define GENERATE_CONTACT_INFO
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.Popups;
//using TP8.Data;

namespace TP8.Data // nah: .DataModel
{
    // Code here based on style of http jesseliberty.com/2012/08/21/windows-8-data-binding-part5binding-to-lists/
    public class TP_OrgContactInfo : INotifyPropertyChanged // aka Hospital Contact Info
    {

        private string _orgName;
        public string OrgName
        {
            get { return _orgName; }
            set
            {
                _orgName = value;
                RaisePropertyChanged();
            }
        }

        private string _orgAbbrOrShortName;
        public string OrgAbbrOrShortName
        {
            get { return _orgAbbrOrShortName; }
            set
            {
                _orgAbbrOrShortName = value;
                RaisePropertyChanged();
            }
        }

        private string _orgStreetAddress1;
        public string OrgStreetAddress1
        {
            get { return _orgStreetAddress1; }
            set
            {
                _orgStreetAddress1 = value;
                RaisePropertyChanged();
            }
        }

        private string _orgStreetAddress2;
        public string OrgStreetAddress2
        {
            get { return _orgStreetAddress2; }
            set
            {
                _orgStreetAddress2 = value;
                RaisePropertyChanged();
            }
        }

        private string _orgTownOrCity;
        public string OrgTownOrCity
        {
            get { return _orgTownOrCity; }
            set
            {
                _orgTownOrCity = value;
                RaisePropertyChanged();
            }
        }

        private string _orgCounty;
        public string OrgCounty
        {
            get { return _orgCounty; }
            set
            {
                _orgCounty = value;
                RaisePropertyChanged();
            }
        }

        private string _org2LetterState;
        public string Org2LetterState
        {
            get { return _org2LetterState; }
            set
            {
                _org2LetterState = value;
                RaisePropertyChanged();
            }
        }

        private string _orgZipcode;
        public string OrgZipcode
        {
            get { return _orgZipcode; }
            set
            {
                _orgZipcode = value;
                RaisePropertyChanged();
            }
        }

        private string _orgPhone;
        public string OrgPhone
        {
            get { return _orgPhone; }
            set
            {
                _orgPhone = value;
                RaisePropertyChanged();
            }
        }

        private string _orgFax;
        public string OrgFax
        {
            get { return _orgFax; }
            set
            {
                _orgFax = value;
                RaisePropertyChanged();
            }
        }

        private string _orgEmail;
        public string OrgEmail
        {
            get { return _orgEmail; }
            set
            {
                _orgEmail = value;
                RaisePropertyChanged();
            }
        }

        private string _orgWebSite;
        public string OrgWebSite
        {
            get { return _orgWebSite; }
            set
            {
                _orgWebSite = value;
                RaisePropertyChanged();
            }
        }


        private string _orgNPI; // US 10-digit National Provider Identifier, assigned by CMS
        public string OrgNPI
        {
            get { return _orgNPI; }
            set
            {
                _orgNPI = value;
                RaisePropertyChanged();
            }
        }

        private string _orgLatitude; // e.g. 38.995523
        public string OrgLatitude
        {
            get { return _orgLatitude; }
            set
            {
                _orgLatitude = value;
                RaisePropertyChanged();
            }
        }

        private string _orgLongitude;
        public string OrgLongitude
        {
            get { return _orgLongitude; }
            set
            {
                _orgLongitude = value;
                RaisePropertyChanged();
            }
        }

        private string _orgCountry; // e.g., "USA"
        public string OrgCountry
        {
            get { return _orgCountry; }
            set
            {
                _orgCountry = value;
                RaisePropertyChanged();
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;


        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        public async Task<string> GetCurrentOrgContactInfo(string orgUuid, string orgName)  // This body introduced v34
        {
            int u = -1; 
            try
            {
                u = Convert.ToInt32(orgUuid);
            }
            catch (Exception)
            {
                App.MyAssert(false);
                u = -1;
            }
            string status;
            if (u == -1)
                return "ERROR: HOSPITAL ID WTIH BAD FORMAT.";
            status = await GetCurrentOrgContactInfo(u, orgName);
            return status;
        }

        public async Task<string> GetCurrentOrgContactInfo(int orgUuid, string orgName) // was v33: string orgUuid, ...
        {
            App.MyAssert(App.pd.plToken.Length == 128); // token is 128 char long SHA-512.  

            string status = await App.service.GetHospitalData(orgUuid, orgName); // Function call puts results directly into App.CurrentOrgContactInfo
            // could check status for "ERROR:" or "COMMUNICATIONS ERROR:"
            // If ERROR, there may or may not be anything put into App.CurrentOrgContactInfo
            return status;
        }

    }

    // We're introducing a list ONLY so we can use LocalStorage class.  It's a kludge.

    [XmlType(TypeName = "OrgContactInfoList")]
    public class TP_OrgContactInfoList : IEnumerable<TP_OrgContactInfo>
    {
        const string ORG_CONTACT_INFO_FILENAME = "OrgContactInfo.xml";

        private List<TP_OrgContactInfo> inner = new List<TP_OrgContactInfo>();

        public void Add(object o)
        {
            inner.Add((TP_OrgContactInfo)o);
        }

        public void Remove(TP_OrgContactInfo o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_OrgContactInfo> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_OrgContactInfo> list)
        {
            inner = list;
        }

        public IEnumerator<TP_OrgContactInfo> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_OrgContactInfo o)
        {
            //int index = inner.FindIndex(i => i.EventID == o.EventID); // C# 3.0 lambda expression
            //if (index >= 0)
            //    inner[index] = o; // update
            if (inner.Count > 0)
            {
                App.MyAssert(inner.Count == 1);
                inner[0] = o;
            }
            else
                Add(o);
        }

        public async Task Init()
        {
            // This is called AFTER App.OrgDataList.Init()
            bool exists = await DoesFileExistAsync();
            //if (!exists)
            //    GenerateDefaultOrgContactInfo(); // This will provide a default XML file with a little content if none exists

            await ProcessOrgContactInfo(exists);

            // That is clear the list in memory, then call the web service.  If successful, memory list is available and also gets written to XML file.
            // If failed, will try to read the XML file instead.

        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(ORG_CONTACT_INFO_FILENAME);
        }

        private async Task GenerateDefaultOrgContactInfo()
        {
            /* Default data dropped July 7, 2014.  More likely to cause problems than be helpful.  Just initialize file, empty except for root XML, instead.
            inner.Add(new TP_OrgContactInfo()
            {
                OrgName = "NLM (testing)",
                OrgAbbrOrShortName = "NLM",
                OrgStreetAddress1 = "9000 Rockvile Pike",
                OrgStreetAddress2 = "",
                OrgTownOrCity = "Bethesda",
                OrgCounty = "Montgomery",
                Org2LetterState = "MD",
                OrgZipcode = "20892",
                OrgPhone = "",
                OrgFax = "",
                OrgEmail = "",
                OrgWebSite = "www.nlm.nih.gov",
                OrgNPI = "1234567890",
                OrgLatitude = "38.99523",
                OrgLongitude = "-77.096597",
                OrgCountry = "USA"
            }); */
            await WriteXML();
        }

        private async Task ProcessOrgContactInfo(bool exists)
        {
            App.MyAssert(App.OrgDataList.Count() != 0);

            if (App.OrgContactInfoList.Count() != 0)
                return; // anticipating fresh install wizard

            await ReadXML();
            string Uuid = "";
            string Name = "";
            TP_OrgContactInfo oci = this.FirstOrDefault(); // let's not stuff this into App.CurrentOrgContactInfo quite yet.
            if (oci == null) // file read failed
            {
                Uuid = App.OrgDataList.First().OrgUuid;
                Name = App.OrgDataList.First().OrgName;
            }
            else
            {
                // Find corresponding list item:
                Name = oci.OrgName;
                Uuid = App.OrgDataList.GetOrgUuidFromOrgName(Name);
                if (Uuid == "") // couldn't find match, assume OrgContactInfo.xml is stale (or dummy generated) and to be replaced.
                {
                    Uuid = App.OrgDataList.First().OrgUuid;
                    Name = App.OrgDataList.First().OrgName;
                }
            }
            // Freshen the data if possible:
            string s = "";
            int u = -1; // introduced v34
            try
            {
                u = Convert.ToInt32(Uuid);
            }
            catch (Exception)
            {
                App.MyAssert(false);
                u = -1;
            }
            if(u != -1)
                s = await App.CurrentOrgContactInfo.GetCurrentOrgContactInfo(u, Name); // puts results into CurrentOrgContactInfo
            if (s.StartsWith("ERROR:") || u == -1)
            {
                await ReadXML(); // in case call to web service wiped out App.CurrentOrgContactInfo
                // For the user, this is not an error
                App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + "  - My organization's contact info\n"; 
                // MessageDialog dlg = new MessageDialog("Could not connect to web service to get my organization's contact info.  Using local cached version instead.");
                // await dlg.ShowAsync();
            }
            else
                await WriteXML();

        }

        public async Task ReadXML()
        {
            await ReadXML(ORG_CONTACT_INFO_FILENAME); 
        }

        public async Task ReadXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_OrgContactInfo>(filename);
            if (LocalStorage.Data != null)
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_OrgContactInfo); // if there's more than 1 we're going to ignore them.
                }
            App.LocalStorageDataSemaphore.Release();
        }

        public async Task WriteXML()
        {
            await WriteXML(ORG_CONTACT_INFO_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_OrgContactInfo);

            await LocalStorage.Save<TP_OrgContactInfo>(filename);
            App.LocalStorageDataSemaphore.Release();
        }


    }
}
