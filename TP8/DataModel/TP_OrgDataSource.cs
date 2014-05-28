using LPF_SOAP;
using Newtonsoft.Json;
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
    public class TP_OrgData : INotifyPropertyChanged // aka Hospital Contact Info
    {

        private string _orgUuid; // Vesuvius/PL ID aka hospital_uuid
        public string OrgUuid
        {
            get { return _orgUuid; }
            set
            {
                _orgUuid = value;
                RaisePropertyChanged();
            }
        }

        private string _orgNPI; // US National Provider ID from CMS; or other facility code
        public string OrgNPI
        {
            get { return _orgNPI; }
            set
            {
                _orgNPI = value;
                RaisePropertyChanged();
            }
        }

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

        /* NOT NEEDED YET
        // In Win 7 version of TriagePic, this is handled by OtherSettings.xml's HospitalNameSelected.
        // Not strictly needed, because can be inferred by single entry in App.CurrentOrgContactInfo and OrgContactInfo.xml
        // If we decided to cache ALL OrgContactInfo, then would need.
                private bool _orgSelected;
                public bool OrgSelected
                {
                    get { return _orgSelected; }
                    set
                    {
                        _orgSelected = value;
                        RaisePropertyChanged();
                    }
                }
        */

        public event PropertyChangedEventHandler PropertyChanged;


        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

    }


    [XmlType(TypeName = "OrgDataList")] // Compare Win 7 HospitalList.xml
    public class TP_OrgDataList : IEnumerable<TP_OrgData>
    {
        const string ORG_DATA_LIST_FILENAME = "OrgDataList.xml";

        private List<TP_OrgData> inner = new List<TP_OrgData>();

        public void Add(object o)
        {
            inner.Add((TP_OrgData)o);
        }

        public void Remove(TP_OrgData o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_OrgData> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_OrgData> list)
        {
            inner = list;
        }

        public IEnumerator<TP_OrgData> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

/* WAS BUT PROBABLY WRONG NOW
        public void UpdateOrAdd(TP_OrgData o)
        {
            //int index = inner.FindIndex(i => i.EventShortName == o.EventShortName); // C# 3.0 lambda expression
            //if (index >= 0)
            //    inner[index] = o; // update
            if (inner.Count > 0)
            {
                App.MyAssert(inner.Count == 1);
                inner[0] = o;
            }
            else
                Add(o);
        } */

        public async Task Init()
        {
            if (!await DoesFileExistAsync())
                await GenerateDefaultOrgData();

            await ProcessOrgList();
        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(ORG_DATA_LIST_FILENAME);
        }

        private async Task GenerateDefaultOrgData()
        {
            inner.Add(new TP_OrgData() {
                OrgUuid = "1",
                OrgNPI = "1234567890",
                OrgName = "NLM (testing)",
                OrgAbbrOrShortName = "NLM",
                OrgLatitude = "38.99523",
                OrgLongitude = "-77.096597"
                // NOT NEEDED YET:  , OrgSelected = true
            });
            await WriteXML();
        }

        public async Task ProcessOrgList() // Don't need credentials passed because underlying service doesn't need them
        {
            // This is similar to a section of code in Win 7 FormTriagePic
            // However, avoiding use of hospitalUuids = new Dictionary
            if (App.OrgDataList.Count() != 0)
                return; // anticipating fresh install wizard

            // equivalent jsonHospitalList is global in Win 7
            string jsonOrgList = await App.service.GetHospitalList();
            if ((jsonOrgList.StartsWith("ERROR:")) || (jsonOrgList.StartsWith("COMMUNICATION ERROR:")) || jsonOrgList.Length == 0)
            {
                await App.OrgDataList.ReadXML(); // Win 7: hospitals = service.ReadHopsitalListFromFile();
                if (/*newHospitalListFileCreated || */ inner.Count() == 0)
                {
                    App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + 
                        "  - List of organizations (e.g., hospitals)\n" +
                        "      Nor was this list available from a cache file on this machine!\n\n" +

                        "      PLEASE EXIT, establish an internet connection, then retry TriagePic.\n";
                    // MessageDialog dlg = new MessageDialog(
                    //   "Could not get list of organizations (e.g., hospitals) from either web service or the cache file on this machine.\n" +
                    //   "Please exit, establish an internet connection, then retry TriagePic.");
                    // await dlg.ShowAsync();
                    return;
                }
                else
                {
                    // For the user, this is not an error
                    App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + "  - List of organizations (e.g., hospitals)\n"; 
                    // MessageDialog dlg = new MessageDialog(
                    //    "Could not connect to web service to get list of organizations (e.g., hospitals).  Using local cached version instead.");
                    // await dlg.ShowAsync();
                }
            }
            else
            {
                // convert from json
                List<Hospital> orgs = JsonConvert.DeserializeObject<List<Hospital>>(jsonOrgList);
                if (orgs != null)
                {
                    // NOT NEEDED YET: int i = 0;
                    foreach (var item in orgs)
                    {
                        // Similar to Win 7 servive.FilterIncidentResponseRows:
                        TP_OrgData od = new TP_OrgData();
                        od.OrgUuid = item.hospital_uuid;
                        od.OrgName = item.name;
                        od.OrgAbbrOrShortName = item.shortname;
                        od.OrgNPI = item.npi;
                        od.OrgLatitude = item.latitude;
                        od.OrgLongitude = item.longitude;
                        /* NOT NEEDED YET:
                        od.OrgSelected = false;
                        if (i++ == 0)
                            od.OrgSelected = true; // tag the first one until we know better */
                        inner.Add(od);
                    }
                }
                await App.OrgDataList.WriteXML(); // update
            }

            // Because of binding, Settings/My Organization combo box should get updated list of names from App.OrgDataList automagically

        }

        /// <summary>
        /// Returns empty string if no match
        /// </summary>
        /// <param name="orgName"></param>
        /// <returns></returns>
        public string GetOrgUuidFromOrgName(string orgName)
        {
            string uuid = "";
            foreach (var i in App.OrgDataList)
            {
                if (i.OrgName == orgName)
                {
                    uuid = i.OrgUuid;
                    break;
                }
            }
            return uuid;
        }

        /// <summary>
        /// Returns empty string if no match
        /// </summary>
        /// <param name="orgName"></param>
        /// <returns></returns>
        public TP_OrgData GetOrgDataFromOrgUuid(string orgUuid)
        {
            foreach (var i in App.OrgDataList)
            {
                if (i.OrgUuid == orgUuid)
                {
                    return i;
                }
            }
            return null;
        }

        public async Task ReadXML()
        {
            await ReadXML(ORG_DATA_LIST_FILENAME); 
        }

        public async Task ReadXML(string filename)
        {
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_OrgData>(filename);
            if (LocalStorage.Data != null)
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_OrgData); // if there's more than 1 we're going to ignore them.
                }
        }

        public async Task WriteXML()
        {
            await WriteXML(ORG_DATA_LIST_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_OrgData);

            await LocalStorage.Save<TP_OrgData>(filename);
        }

    }
}
