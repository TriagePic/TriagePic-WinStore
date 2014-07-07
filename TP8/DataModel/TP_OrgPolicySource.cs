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
    public class TP_OrgPolicy  : INotifyPropertyChanged // aka Hospital Policy
    {

        private string _orgPatientIdPrefixText;
        public string OrgPatientIdPrefixText
        {
            get { return _orgPatientIdPrefixText; }
            set
            {
                _orgPatientIdPrefixText = value;
                RaisePropertyChanged();
            }
        }

        private Int32 _orgPatientIdFixedDigits; // -1 means variable # of digits
        public Int32 OrgPatientIdFixedDigits
        {
            get { return _orgPatientIdFixedDigits; }
            set
            {
                _orgPatientIdFixedDigits = value;
                RaisePropertyChanged();
            }
        }

        private string _triageZoneListJSON;  // This is parsed into elements of App.ZoneChoices. See TP_ZoneChoice
        public string TriageZoneListJSON
        {
            get { return _triageZoneListJSON; }
            set
            {
                _triageZoneListJSON = value;
                RaisePropertyChanged();
            }
        }

        private bool _photoRequired;
        public bool PhotoRequired
        {
            get { return _photoRequired; }
            set
            {
                _photoRequired = value;
                RaisePropertyChanged();
            }
        }

        private bool _honorNoPhotoRequest;
        public bool HonorNoPhotoRequest
        {
            get { return _honorNoPhotoRequest; }
            set
            {
                _honorNoPhotoRequest = value;
                RaisePropertyChanged();
            }
        }

        private bool _photographerNameRequired;
        public bool PhotographerNameRequired
        {
            get { return _photographerNameRequired; }
            set
            {
                _photographerNameRequired = value;
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

        public string ForceValidFormatID(string pID)
        {
            UInt32 val = GetValidSuffix(pID);
            string suffix = PadPatientIdSuffixWithLeadingZerosIfReqd(val.ToString());
            return OrgPatientIdPrefixText + suffix;
        }

        /// <summary>
        /// Will format an incremented or decremented patient ID, based on current ID, organization's ID format policy, and RollUpward boolean.
        /// Caller must save results to PatientIdTextBox
        /// </summary>
        /// <returns></returns>
        public string BumpPatientID(string pID, bool upward)
        {
            UInt32 val = GetValidSuffix(pID); // but not yet zero padded if appropriate
            if (upward)
            {
                if (val < GetMaxPatientIdSuffixValue())
                    val++;
            }
            else
            {
                if (val > 0)
                    val--;
            }
            string suffix = PadPatientIdSuffixWithLeadingZerosIfReqd(val.ToString());
            return OrgPatientIdPrefixText + suffix;
        }

        public UInt32 GetValidSuffix(string pID)
        {
            UInt32 val = 0; // default
            string prefix = OrgPatientIdPrefixText;
            if (pID == "" || pID == prefix)
                return 0;

            string suffix;
            if(pID.Substring(0, prefix.Length) == prefix)
                suffix = pID.Substring(prefix.Length);
            else if (pID.Length < prefix.Length)
                suffix = pID; // might be wrong guess
            else
            {
                suffix = pID.Substring(prefix.Length);
                if (suffix.Length > 9)
                    suffix = suffix.Remove(0, suffix.Length - 9); // too many digits? There's no "right" answer, but throw away highest ones will get rid of too many leading zeroes
                // Don't bother checking against shorter suffix length bounds here... we'll do that check after conversion to Uint32
            }

            try
            {
                val = Convert.ToUInt32(suffix);
            }
            catch (Exception)
            {
                return 0; // if non-numerics, just bail
            }

            if (val > GetMaxPatientIdSuffixValue())
                val = GetMaxPatientIdSuffixValue();
            return val;
        }


        public string PadPatientIdSuffixWithLeadingZerosIfReqd(string suffix)
        {
            if (OrgPatientIdFixedDigits == -1) // means variably-length string
                return suffix; // don't pad
            App.MyAssert(OrgPatientIdFixedDigits > 2 && OrgPatientIdFixedDigits < 10); // upper bound to fit in int.  Lower bound just for real-world practicality
            while (suffix.Length < OrgPatientIdFixedDigits)
                suffix = "0" + suffix;
            return suffix; // If suffix is already too long, this leaves it unchanged.  Caller needs to verify that case.
        }

        public UInt32 GetMaxPatientIdSuffixValue()
        {
            if (OrgPatientIdFixedDigits == -1)
                return 999999999; // max in a uint32 has 10 digits, but we restrict to 9 digits in which all values are allowed.
            App.MyAssert(OrgPatientIdFixedDigits > 2 && OrgPatientIdFixedDigits < 10); // upper bound to fit in int.  Lower bound just for real-world practicality
            UInt32 maxVal = 9;
            for (int i = 1; i < OrgPatientIdFixedDigits; i++)
            {
                maxVal = (maxVal * 10) + 9;
            }
            return maxVal; // returns 999 to 999999999
        }

#if MAYBE_NOT_USEFUL
        public UInt32 GetPatientIDMaxTextBoxLength()
        {
            UInt32 suffixLength;
            if (OrgPatientIdFixedDigits == -1)
                suffixLength = 9; // see comments in GetMaxPatientIdSuffixLength
            else
                suffixLength = Convert.ToUInt32(OrgPatientIdFixedDigits);
            return Convert.ToUInt32(OrgPatientIdPrefixText.Length) + suffixLength;
        }
#endif
        public async Task<string> GetCurrentOrgPolicy(string orgUuid)
        {
            string status = await App.service.GetHospitalPolicy(orgUuid); // Function call puts results directly into App.OrgPolicy and first and only item of App.OrgPolicyList
            // could check status for "ERROR:" or "COMMUNICATIONS ERROR:"
            // If ERROR, there may or may not be anything put into App.CurrentOrgContactInfo
            return status;
        }

    }

    // We're introducing a list ONLY so we can use LocalStorage class.  It's a kludge.

    [XmlType(TypeName = "OrgPolicyList")]
    public class TP_OrgPolicyList : IEnumerable<TP_OrgPolicy>
    {
        const string ORG_POLICY_FILENAME = "OrgPolicy.xml";

        private List<TP_OrgPolicy> inner = new List<TP_OrgPolicy>();

        public void Add(object o)
        {
            inner.Add((TP_OrgPolicy)o);
        }

        public void Remove(TP_OrgPolicy o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_OrgPolicy> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_OrgPolicy> list)
        {
            inner = list;
        }

        public IEnumerator<TP_OrgPolicy> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_OrgPolicy o)
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
        }

        public async Task Init()
        {
            // This is called AFTER App.OrgDataList.Init()
            bool exists = await DoesFileExistAsync();
            if (!exists)
                await GenerateDefaultOrgPolicy(); // This will provide a default XML file with a little content if none exists

            await ProcessOrgPolicy(exists);

        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(ORG_POLICY_FILENAME);
        }

        private async Task GenerateDefaultOrgPolicy()
        {
            /* Default data dropped July 7, 2014.  More likely to cause problems than be helpful.  Just initialize file, empty except for root XML, instead.
            inner.Add(new TP_OrgPolicy()
            {
                OrgPatientIdFixedDigits = 4,
                OrgPatientIdPrefixText = "911-"
            }); */
            await WriteXML();
        }

        private async Task ProcessOrgPolicy(bool exists)
        {
            if (App.OrgPolicyList.Count() != 0)
                return; // anticipating fresh install wizard


            string Uuid  = App.OrgDataList.GetOrgUuidFromOrgName(App.CurrentOrgContactInfo.OrgName);
            if (Uuid == "") // couldn't find match, assume OrgContactInfo.xml is stale (or dummy generated) and to be replaced.
            {
                // TO DO: Report error to log here
                 Uuid = App.OrgDataList.First().OrgUuid;
            }

            string s;
            s = await App.OrgPolicy.GetCurrentOrgPolicy(Uuid); // puts results into App.OrgPolicy and first and only item of App.OrgPolicyList
            if (s.StartsWith("ERROR:"))
            {
                await ReadXML(); // in case call to web service wiped out App.OrgPolicy and App.OrgPolicyList
                App.OrgPolicy = this.First();
                // For the user, this is not an error
                App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + "  - My organization's policies for TriagePic settings\n";
                // MessageDialog dlg = new MessageDialog("Could not connect to web service to get my organization's policies for TriagePic settings.  Using local cached version instead.");
                // await dlg.ShowAsync();
            }
            else
                await WriteXML();
        }

        public async Task ReadXML()
        {
            await ReadXML(ORG_POLICY_FILENAME); 
        }

        public async Task ReadXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_OrgPolicy>(filename);
            if (LocalStorage.Data != null)
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_OrgPolicy); // if there's more than 1 we're going to ignore them.
                }
            App.LocalStorageDataSemaphore.Release();
        }

        public async Task WriteXML()
        {
            await WriteXML(ORG_POLICY_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_OrgPolicy);

            await LocalStorage.Save<TP_OrgPolicy>(filename);
            App.LocalStorageDataSemaphore.Release();
        }


    }
}
