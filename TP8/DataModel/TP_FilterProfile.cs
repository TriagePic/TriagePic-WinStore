using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TP8.Common;
using TP8.Data;

namespace TP8
{
    public class TP_FilterProfile : BindableBase
    {
        private string filterProfileName;
        public string FilterProfileName // Of form "Default - <Short Org Name>"
        { 
            get { return filterProfileName; }
            set { this.SetProperty(ref filterProfileName, value); }
        }

        private bool searchAgainstName;
        public bool SearchAgainstName
        { 
            get { return searchAgainstName; }
            set { this.SetProperty(ref searchAgainstName, value); }
        }

        private bool searchAgainstID;
        public bool SearchAgainstID
        {
            get { return searchAgainstID; }
            set { this.SetProperty(ref searchAgainstID, value); }
        }

        private bool disasterEventIncludeTest;
        public bool DisasterEventIncludeTest
        {
            get { return disasterEventIncludeTest; }
            set { this.SetProperty(ref disasterEventIncludeTest, value); }
        }

        private bool disasterEventIncludeReal;
        public bool DisasterEventIncludeReal
        {
            get { return disasterEventIncludeReal; }
            set { this.SetProperty(ref disasterEventIncludeReal, value); }
        }

        private bool disasterEventIncludePrivate;
        public bool DisasterEventIncludePrivate
        {
            get { return disasterEventIncludePrivate; }
            set { this.SetProperty(ref disasterEventIncludePrivate, value); }
        }

        private bool disasterEventIncludePublic;
        public bool DisasterEventIncludePublic
        {
            get { return disasterEventIncludePublic; }
            set { this.SetProperty(ref disasterEventIncludePublic, value); }
        }

        private string flyoutEventComboBoxChoice;
        public string FlyoutEventComboBoxChoice // could be specific event, or group description.
        {
            get { return flyoutEventComboBoxChoice; }
            set { this.SetProperty(ref flyoutEventComboBoxChoice, value); }
        }
        // Hard to support with current state of PL, choice of events based on "Hospital/Organization":
        //"Current (if known, otherwise Default)"
        //"All Sharing These Reports"
        //"Specified:" ComboBox

        // Reported Attributes:
        private bool includeMale;
        public bool IncludeMale
        {
            get { return includeMale; }
            set { this.SetProperty(ref includeMale, value); }
        }

        private bool includeFemale;
        public bool IncludeFemale
        {
            get { return includeFemale; }
            set { this.SetProperty(ref includeFemale, value); }
        }

        private bool includeUnknownOrComplexGender;
        public bool IncludeUnknownOrComplexGender
        {
            get { return includeUnknownOrComplexGender; }
            set { this.SetProperty(ref includeUnknownOrComplexGender, value); }
        }

        private bool includeAdult;
        public bool IncludeAdult
        {
            get { return includeAdult; }
            set { this.SetProperty(ref includeAdult, value); }
        }

        private bool includePeds;
        public bool IncludePeds
        {
            get { return includePeds; }
            set { this.SetProperty(ref includePeds, value); }
        }

        private bool includeUnknownAgeGroup;
        public bool IncludeUnknownAgeGroup
        {
            get { return includeUnknownAgeGroup; }
            set { this.SetProperty(ref includeUnknownAgeGroup, value); }
        }
/* WAS:
        private bool includeGreenZone;
        public bool IncludeGreenZone
        {
            get { return includeGreenZone; }
            set { this.SetProperty(ref includeGreenZone, value); }
        }

        private bool includeBHGreenZone;
        public bool IncludeBHGreenZone
        {
            get { return includeBHGreenZone; }
            set { this.SetProperty(ref includeBHGreenZone, value); }
        }

        private bool includeYellowZone;
        public bool IncludeYellowZone
        {
            get { return includeYellowZone; }
            set { this.SetProperty(ref includeYellowZone, value); }
        }

        private bool includeRedZone;
        public bool IncludeRedZone
        {
            get { return includeRedZone; }
            set { this.SetProperty(ref includeRedZone, value); }
        }

        private bool includeGrayZone;
        public bool IncludeGrayZone
        {
            get { return includeGrayZone; }
            set { this.SetProperty(ref includeGrayZone, value); }
        }

        private bool includeBlackZone;
        public bool IncludeBlackZone
        {
            get { return includeBlackZone; }
            set { this.SetProperty(ref includeBlackZone, value); }
        }
*/
        private TP_ZoneFilters includeWhichZones;
        public TP_ZoneFilters IncludeWhichZones
        {
            get { return includeWhichZones; }
            set { this.SetProperty(ref includeWhichZones, value); }
        }

        private bool includeHasPatientName;
        public bool IncludeHasPatientName
        {
            get { return includeHasPatientName; }
            set { this.SetProperty(ref includeHasPatientName, value); }
        }

        private bool includeHasNoPatientName;
        public bool IncludeHasNoPatientName
        {
            get { return includeHasNoPatientName; }
            set { this.SetProperty(ref includeHasNoPatientName, value); }
        }

        private bool includeHasPhotos;
        public bool IncludeHasPhotos
        {
            get { return includeHasPhotos; }
            set { this.SetProperty(ref includeHasPhotos, value); }
        }

        private bool includeHasNoPhoto;
        public bool IncludeHasNoPhoto
        {
            get { return includeHasNoPhoto; }
            set { this.SetProperty(ref includeHasNoPhoto, value); }
        }

/* TO DO:
        // When Reported:
        private bool allDates;
        public bool AllDates // else specific (uses dateFilter radio group)
        {
            get { return allDates; }
            set { this.SetProperty(ref allDates, value); }
        }

        private string fromMonth;
        public string FromMonth
        {
            get { return fromMonth; }
            set { this.SetProperty(ref fromMonth, value); }
        }

        private string fromDay;
        public string FromDay
        {
            get { return fromDay; }
            set { this.SetProperty(ref fromDay, value); }
        }

        private string fromYear;
        public string FromYear
        {
            get { return fromYear; }
            set { this.SetProperty(ref fromYear, value); }
        }

        private string toMonth;
        public string ToMonth
        {
            get { return toMonth; }
            set { this.SetProperty(ref toMonth, value); }
        }

        private string toDay;
        public string ToDay
        {
            get { return toDay; }
            set { this.SetProperty(ref toDay, value); }
        }

        private string toYear;
        public string ToYear
        {
            get { return toYear; }
            set { this.SetProperty(ref toYear, value); }
        }
*/
        // Hack until we have better state management.  Use to get FilterNonFlyoutValues back to invoking page:
        private bool aControlChanged;
        public bool AControlChanged
        {
            get { return aControlChanged; }
            set { this.SetProperty(ref aControlChanged, value); }
        }


        /// <summary>
        /// Settings here should agree with TP_EventsDataSource.InitAsFilters()
        /// </summary>
        public void ResetFilterProfileToDefault()
        {
            FilterProfileName="Default - " + App.CurrentOrgContactInfo.OrgAbbrOrShortName;
            SearchAgainstName = true;
            SearchAgainstID = true;
            DisasterEventIncludeTest = true;
            DisasterEventIncludeReal = true;
            DisasterEventIncludePrivate = true;
            DisasterEventIncludePublic = true;
            FlyoutEventComboBoxChoice = "Current event (recommended)";//"All events";
            // Hard to support with current state of PL, choice of events based on "Hospital/Organization":
            //"Current (if known, otherwise Default)"
            //"All Sharing These Reports"
            //"Specified:" ComboBox
            
            // Reported Attributes:
            IncludeMale = true;
            IncludeFemale = true;
            IncludeUnknownOrComplexGender = true;
            IncludeAdult = true;
            IncludePeds = true;
            IncludeUnknownAgeGroup = true;
/* WAS
            IncludeGreenZone = true;
            IncludeBHGreenZone = true;
            IncludeYellowZone = true;
            IncludeRedZone = true;
            IncludeGrayZone = true;
            IncludeBlackZone = true; */
            if (IncludeWhichZones == null)
                IncludeWhichZones = new TP_ZoneFilters();
            //was: IncludeWhichZones.GenerateDefaultChoices(); // first pass
            ResetFilterProfileZones(App.ZoneChoices.GetZoneChoiceList());
            IncludeHasPatientName = true;
            IncludeHasNoPatientName = true;
            IncludeHasPhotos = true;
            IncludeHasNoPhoto = true;
            
            /* TO DO:
            // When Reported:
            AllDates = true; // else specific (uses dateFilter radio group)
            FromMonth = "";
            FromDay = "";
            FromYear = "";
            ToMonth = "";
            ToDay = "";
            ToYear = "";
             */

            // initial state.
            // Maybe not here: AControlChanged = false;
        }

        public void ResetFilterProfileZones(List<TP_ZoneChoiceItem> zc)
        {
            IncludeWhichZones.GenerateDefaultChoices(zc); // second pass
        }

        /// <summary>
        /// Returns "true" if the filter flyout has any setting other than default.
        /// </summary>summary>
        /// <returns>bool</returns>
        public bool HasFlyoutFiltering()
        {
            // See ResetFilterProfileToDefault()
            var f = App.CurrentFilterProfile; // for convenience
            return (f.HasFlyoutEventFiltering() || f.HasFlyoutFilteringOtherThanEvent() );
        }

        /// <summary>
        /// Returns "true" if the filter flyout has any setting other than default.
        /// </summary>summary>
        /// <returns>bool</returns>
        public bool HasFlyoutFilteringOtherThanEvent()
        {
            // See TP_FilterProfile.cs: ResetFilterProfileToDefault()
            var f = App.CurrentFilterProfile; // for convenience
            if (!f.IncludeHasNoPatientName ||
                !f.IncludeHasPatientName ||
                !f.IncludeMale ||
                !f.IncludeFemale ||
                !f.IncludeUnknownOrComplexGender ||
                !f.IncludeAdult ||
                !f.IncludePeds ||
                !f.IncludeUnknownAgeGroup ||
                !f.IncludeWhichZones.AllIncluded() ||
/* WAS:
                !f.IncludeGreenZone ||
                !f.IncludeBHGreenZone ||
                !f.IncludeYellowZone ||
                !f.IncludeRedZone ||
                !f.IncludeGrayZone ||
                !f.IncludeBlackZone || */
                !f.IncludeHasNoPhoto ||
                !f.IncludeHasPhotos )
                //!f.SearchAgainstName ||
                //!f.SearchAgainstID
                return true;
            return false;
        }

        /// <summary>
        /// Returns "true" if the filter flyout has any setting other than "All events".  Note default: is "Current event (recommended)".
        /// </summary>summary>
        /// <returns>bool</returns>
        public bool HasFlyoutEventFiltering()
        {
            // See TP_FilterProfile.cs: ResetFilterProfileToDefault()
            var f = App.CurrentFilterProfile; // for convenience
            if (
                !f.DisasterEventIncludeReal ||
                !f.DisasterEventIncludeTest ||
                !f.DisasterEventIncludePrivate ||
                !f.DisasterEventIncludePublic ||
                f.FlyoutEventComboBoxChoice != "All events")
                return true;
            return false;
        }

        // TO DO: Copy From, Clear
    }



    [XmlType(TypeName = "FilterProfileList")]
    public class TP_FilterProfileList : IEnumerable<TP_FilterProfile>
    {
        const string FILTER_PROFILE_LIST_FILENAME = "FilterProfiles.xml";

        private List<TP_FilterProfile> inner = new List<TP_FilterProfile>();

        public void Add(object o)
        {
            inner.Add((TP_FilterProfile)o);
        }

        public void Remove(TP_FilterProfile o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_FilterProfile> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_FilterProfile> list)
        {
            inner = list;
        }

        public IEnumerator<TP_FilterProfile> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_FilterProfile o)
        {
            int index = inner.FindIndex(i => i.FilterProfileName == o.FilterProfileName); // C# 3.0 lambda expression
            if (index >= 0)
                inner[index] = o; // update
            else
                Add(o);
        }

        public async Task Init()
        {
            bool exists = await DoesFileExistAsync();
            if(!exists)
                await GenerateDefaultFilterProfileList(); // This will provide a default XML file with a little content if none exists

            // ProcessEventList will clear the list in memory, then read the XML file instead.
            // The filter profiles are entirely local; no web service call needed
            //await ProcessFilterProfileList();
        }

        //public async Task ProcessFilterProfileList()
        //{
        //    //App.CurrentFilterProfile = 
        //}

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(FILTER_PROFILE_LIST_FILENAME);
        }

        private async Task GenerateDefaultFilterProfileList()
        {
            inner.Clear();
            TP_FilterProfile defaultprofile = new TP_FilterProfile();
            defaultprofile.ResetFilterProfileToDefault();
            inner.Add(defaultprofile);

            await WriteXML();
        }

        public async Task ReadXML()
        {
            await ReadXML(FILTER_PROFILE_LIST_FILENAME, true);
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_FilterProfile>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_FilterProfile);
                }
            }
        }

        public async Task WriteXML()
        {
            await WriteXML(FILTER_PROFILE_LIST_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_FilterProfile);

            await LocalStorage.Save<TP_FilterProfile>(filename);
        }

        public async Task<TP_FilterProfile> GetDefaultForCurrentOrg()
        {
            App.MyAssert(App.CurrentOrgContactInfo.OrgAbbrOrShortName.Length > 0);
            int index = inner.FindIndex(i => i.FilterProfileName == "Default - " + App.CurrentOrgContactInfo.OrgAbbrOrShortName); // C# 3.0 lambda expression
            if (index >= 0)
            {
                inner[index].AControlChanged = false;
                return inner[index];
            }
            //else
            TP_FilterProfile o = new TP_FilterProfile();
            o.ResetFilterProfileToDefault();
            o.AControlChanged = false;
            App.MyAssert(o.FilterProfileName == "Default - " + App.CurrentOrgContactInfo.OrgAbbrOrShortName);
            Add(o);
            await WriteXML();
            return o;
        }

    }
}
