//#define GENERATE_OUTBOX_XML
//#define GENERATE_ALLSTATIONS_XML
using System;
//using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.UI.Popups;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using Windows.ApplicationModel.Resources.Core;
//using Windows.Foundation;
//using Windows.Foundation.Collections;
//using Windows.UI.Xaml.Data;
//using Windows.UI.Xaml.Media;
//using Windows.UI.Xaml.Media.Imaging;
//using System.Collections.Specialized;
using System.Linq;
using System.Collections;
using System.Xml.Serialization;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging; // needed for WriteableBitmap.PixelBuffer.ToStream()

#if SETASIDE
// This is the underlying TP data model, which is mapped to the SampleDataItem of the SampleDataSource model with its data bindings in the standard item templates.
namespace TP8_1.Data
{

    /// <summary>
    /// Like Win 7 TriagePic's PatientReports class
    /// </summary>
    [XmlType(TypeName = "PatientDataList")]
    public class TP_PatientDataList : IEnumerable<TP_PatientDataItem>
    {
        private List<TP_PatientDataItem> inner = new List<TP_PatientDataItem>();

        public void Add(object o)
        {
            inner.Add((TP_PatientDataItem)o);
        }

        public void Remove(TP_PatientDataItem o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_PatientDataItem> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_PatientDataItem> list)
        {
            inner = list;
        }

        public async Task ReadXML()
        {
            await ReadXML("PatientDataList.xml"); // outbox
        }

        public async Task ReadXML(string filename)
        {
            Clear();
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_PatientDataItem>(filename);
            if (LocalStorage.Data != null)
            {
                TP_PatientDataItem pdi__;
                foreach (var item in LocalStorage.Data)
                {
                    // Fixup:
                    pdi__ = item as TP_PatientDataItem;
                    pdi__.AdjustAfterReadXML();
                    inner.Add(pdi__);
                }
            }
        }

        public async Task WriteXML()
        {
            await WriteXML("PatientDataList.xml");
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            TP_PatientDataItem pdi__;
            foreach (var patient in inner)
            {
                // Fixup:
                pdi__ = patient as TP_PatientDataItem;
                pdi__.AdjustBeforeWriteXML();
                LocalStorage.Add(pdi__);
            }

            await LocalStorage.Save<TP_PatientDataItem>(filename);
        }

        // Not right at compile time...
//        public TP_PatientDataList OrderBy<T>(Func<TP_PatientDataItem, TKey> keySelector)
//        {
//            return inner.OrderBy(keySelector).ToList();
//        }

        public IEnumerator<TP_PatientDataItem> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_PatientDataItem o)
        {
            int index = inner.FindIndex(i => i.PatientID == o.PatientID); // C# 3.0 lambda expression
            if (index > 0)
                inner[index] = o; // update
            else
                Add(o);
        }

    }


    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public class TP_PatientDataGroups
    {

        private TP_PatientDataList _outbox = new TP_PatientDataList();
        private TP_PatientDataList _allstations = new TP_PatientDataList();
//        private TP_PatientDataList _outboxfiltered = new TP_PatientDataList();
//        private TP_PatientDataList _allstationsfiltered = new TP_PatientDataList();
        private TP_PatientDataList _outboxsorted = new TP_PatientDataList();
        private TP_PatientDataList _allstationssorted = new TP_PatientDataList();
        private TP_PatientDataList _outboxsortedandfiltered = new TP_PatientDataList();
        private TP_PatientDataList _allstationssortedandfiltered = new TP_PatientDataList();

        public TP_PatientDataList GetOutbox()
        {
            return _outbox;
        }

        public TP_PatientDataList GetAllStations()
        {
            return _allstations;
        }
/*
        public TP_PatientDataList GetOutboxFiltered()
        {
            return _outboxfiltered;
        }

        public TP_PatientDataList GetAllStationsFiltered()
        {
            return _allstationsfiltered;
        }
*/
        public TP_PatientDataList GetOutboxSorted()
        {
            return _outboxsorted;
        }

        public TP_PatientDataList GetAllStationsSorted()
        {
            return _allstationssorted;
        }

        public TP_PatientDataList GetOutboxSortedAndFiltered()
        {
            return _outboxsortedandfiltered;
        }

        public TP_PatientDataList GetAllStationsSortedAndFiltered()
        {
            return _allstationssortedandfiltered;
        }

        public TP_PatientDataGroups()
        {
            // can't await here for Init(), so call separately after this class constructed
        }

        public async Task Init()
        {
#if GENERATE_OUTBOX_XML
            AddItemsSet1(_outbox);
            await _outbox.WriteXML();
#else
            await _outbox.ReadXML();
#endif
#if GENERATE_ALLSTATIONS_XML
            AddItemsSet1(_allstations);
            AddItemsSet2(_allstations);
            await _allstations.WriteXML("PatientReportsAllStations.xml");
#else
            await _allstations.ReadXML("PatientReportsAllStations.xml");
#endif
            Scrub(); // Remove bad data
            //ReFilter();
            ReSortAndFilter();
            // must follow ReSortAndFilter:
            SampleDataSource.RefreshOutboxItems(); // For benefit of next peek at Outbox
        }

        /*
        public void ReFilter()
        {
            _outboxfiltered.Clear();
            foreach(TP_PatientDataItem i in _outbox)
            {
                if(InFilteredResults(i, App.CurrentSearchQuery))
                    _outboxfiltered.Add(i);
            }
            _allstationsfiltered.Clear();
            foreach (TP_PatientDataItem i in _allstations)
            {
                if (InFilteredResults(i, App.CurrentSearchQuery))
                    _allstationsfiltered.Add(i);
            }
        }*/

        private void Scrub()
        {
            ScrubImpl(_outbox);
            ScrubImpl(_allstations);
        }

        private void ScrubImpl(TP_PatientDataList l)
        {
            // Remove any records without patient ID
            List<TP_PatientDataItem> scrubL = new List<TP_PatientDataItem>();
            foreach (TP_PatientDataItem i in l)
            {
                if (!String.IsNullOrEmpty(i.PatientID))
                    scrubL.Add(i);
            }
            l.ReplaceWithList(scrubL);
        }

        public void ReSortAndFilter()
        {
            ReSortAndFilterImpl(_outbox, _outboxsorted, _outboxsortedandfiltered, true);
            ReSortAndFilterImpl(_allstations, _allstationssorted, _allstationssortedandfiltered, true);
        }

        public void ReSortAndMinimallyFilter() // used by SplitPage, filter is only "current event only" checkbox
        {
            ReSortAndFilterImpl(_outbox, _outboxsorted, _outboxsortedandfiltered, false);
            ReSortAndFilterImpl(_allstations, _allstationssorted, _allstationssortedandfiltered, false);
        }
        /// <summary>
        /// When called, converts TP_PatientDataList to List<TP_PatientDataItem>
        /// </summary>
        /// <param name="orig"></param>
        /// <param name="sort"></param>
        /// <param name="sortfilt"></param>
        private void ReSortAndFilterImpl(TP_PatientDataList origL, TP_PatientDataList sortL, TP_PatientDataList sortfiltL, bool useFullFilter)
        {
            // If I could figure out how to define TP_PatientDataList.OrderBy, wouldn't need these conversions here and at end:
            List<TP_PatientDataItem> orig = origL.GetAsList();
            List<TP_PatientDataItem> sort = new List<TP_PatientDataItem>(); // NOT NEEDED: = sortL.GetAsList();
            List<TP_PatientDataItem> sortfilt = new List<TP_PatientDataItem>(); // NOT NEEDED: = sortfiltL.GetAsList();

            // NOT NEEDED: sort.Clear();
            if (App.SortFlyoutAscending)
            {
                switch (App.SortFlyoutItem)
                {
                    case App.SortByItem.Gender:
                        sort = orig.OrderBy(o => o.Gender).ToList(); break;
                    case App.SortByItem.FirstName:
                        sort = orig.OrderBy(o => o.FirstName).ToList(); break;
                    case App.SortByItem.LastName:
                        sort = orig.OrderBy(o => o.LastName).ToList(); break;
                    case App.SortByItem.AgeGroup:
                        sort = orig.OrderBy(o => o.AgeGroup).ToList(); break;
                    case App.SortByItem.PatientID:
                        sort = orig.OrderBy(o => o.PatientID).ToList(); break;
                    case App.SortByItem.TriageZone:
                        sort = orig.OrderBy(o => o.Zone).ToList(); break;
                    case App.SortByItem.ArrivalTime:
                        sort = orig.OrderBy(o => o.WhenLocalTime).ToList(); break; // Or maybe sent.  probably needs more work, use of timezone or UTC?
                    case App.SortByItem.DisasterEvent:
                        sort = orig.OrderBy(o => o.EventName).ToList(); break; // or maybe Event ID
                    default:
                        sort = orig.OrderByDescending(o => o.FirstName).ToList(); break;
                }
#if NEEDSWORK
                    case App.SortByItem.PLStatus:
                        sort = orig.OrderBy(o => o.PLStatus).ToList(); break;
                    case App.SortByItem.ReportingStation:
                        sort = orig.OrderBy(o => o.).ToList(); break;
                    maybe also PicCount, whether there is comment

#endif
            }
            else
            {
                switch (App.SortFlyoutItem)
                {
                    case App.SortByItem.Gender:
                        sort = orig.OrderByDescending(o => o.Gender).ToList(); break;
                    case App.SortByItem.FirstName:
                        sort = orig.OrderByDescending(o => o.FirstName).ToList(); break;
                    case App.SortByItem.LastName:
                        sort = orig.OrderByDescending(o => o.LastName).ToList(); break;
                    case App.SortByItem.AgeGroup:
                        sort = orig.OrderByDescending(o => o.AgeGroup).ToList(); break;
                    case App.SortByItem.PatientID:
                        sort = orig.OrderByDescending(o => o.PatientID).ToList(); break;
                    case App.SortByItem.TriageZone:
                        sort = orig.OrderByDescending(o => o.Zone).ToList(); break;
                    case App.SortByItem.ArrivalTime:
                        sort = orig.OrderByDescending(o => o.WhenLocalTime).ToList(); break; // Or maybe sent.  probably needs more work, use of timezone or UTC?
                    case App.SortByItem.DisasterEvent:
                        sort = orig.OrderByDescending(o => o.EventName).ToList(); break; // or maybe Event ID
                    default:
                        sort = orig.OrderByDescending(o => o.FirstName).ToList(); break;
                }
#if NEEDSWORK
                    case App.SortByItem.PLStatus:
                        sort = orig.OrderByDescending(o => o.PLStatus).ToList(); break;
                    case App.SortByItem.ReportingStation:
                        sort = orig.OrderByDescending(o => o.).ToList(); break;
                    maybe also PicCount, whether there is comment

#endif
            }

            if (useFullFilter)
            {
                // NOT NEEDED: sortfilt.Clear();
                foreach (TP_PatientDataItem i in sort)
                {
                    if (InFilteredResults(i, App.CurrentSearchQuery))
                        sortfilt.Add(i);
                }
            }
            else
            {
                foreach (TP_PatientDataItem i in sort)
                {
                    if (!App.OutboxCheckBoxCurrentEventOnly || i.EventName == App.CurrentDisaster.EventName)
                        sortfilt.Add(i);
                }
            }

            origL.ReplaceWithList(orig);
            sortL.ReplaceWithList(sort);
            sortfiltL.ReplaceWithList(sortfilt);

        }

        public string GetShortSortDescription()
        {
            string desc = " by ";

            switch (App.SortFlyoutItem)
            {
                case App.SortByItem.Gender:
                    desc += "gender"; break;
                case App.SortByItem.FirstName:
                    desc += "first name"; break;
                case App.SortByItem.LastName:
                    desc += "last name"; break;
                case App.SortByItem.AgeGroup:
                    desc += "age group"; break;
                case App.SortByItem.PatientID:
                    desc += "patient ID"; break;
                case App.SortByItem.TriageZone:
                    desc += "triage zone (alphabetic)"; break;
                case App.SortByItem.ArrivalTime:
                    desc += "time & date"; break; // Or maybe sent.  probably needs more work, use of timezone or UTC?
                case App.SortByItem.DisasterEvent:
                    desc += "disaster event"; break; // or maybe Event ID
                default:
                    desc += "unknown sort method"; break;
            }
            if (!App.SortFlyoutAscending)
                desc += " descending";
            return desc;
        }


        /// <summary>
        /// Dummy data for _outbox and _allstations
        /// </summary>
        /// <param name="ltp"></param>
        private void AddItemsSet1(TP_PatientDataList ltp)
        {
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:01:26 -04:00",
                "EDT",
                "Y",
                "0001",
                "Yellow",
                "Male",
                "Adult",
                "John",
                "Doe",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0001",
                "Assets/ZoneYellow.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:02:26 -04:00",
                "EDT",
                "Y",
                "0002",
                "Red",
                "Female",
                "Adult",
                "Jane",
                "Doe",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0002",
                "Assets/ZoneRed.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:03:26 -04:00",
                "EDT",
                "Y",
                "0003",
                "Yellow",
                "Female",
                "Pediatric",
                "Janet",
                "Doe",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0003",
                "Assets/ZoneYellow.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:04:26 -04:00",
                "EDT",
                "Y",
                "0004",
                "BH Green",
                "Female",
                "Adult",
                "Karen",
                "Deer",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0004",
                "Assets/ZoneBHGreen.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:05:26 -04:00",
                "EDT",
                "Y",
                "0005",
                "Red",
                "Male",
                "Adult",
                "Alfred",
                "Alcott",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0005",
                "Assets/ZoneRed.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:06:26 -04:00",
                "EDT",
                "Y",
                "0006",
                "Green",
                "Male",
                "Pediatric",
                "Basil",
                "McDowell",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0006",
                "Assets/ZoneGreen.png",
                "",
                "first ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:07:26 -04:00",
                "EDT",
                "Y",
                "0007",
                "Black",
                "Male",
                "Adult",
                "", // [No name]
                "",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0007",
                "Assets/ZoneBlack.png",
                "",
                "first ambulance"
                ));

        }
            // Use the timestamp as UniqueID.  In Win7 TriagePic, this would be either:
            // EDXL format ("dateEDXL"), e.g., 2012-08-13T22:24:26Z
            // WhenLocalTime, e.g., 2012-08-13 18:24:26 -04:00  (with time zone "TZ" also given, e.g., EDT)
            // Latter is more informative

        /// <summary>
        /// Dummy data for _outbox and _allstations
        /// </summary>
        /// <param name="ltp"></param>
        private void AddItemsSet2(TP_PatientDataList ltp)
        {

            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:08:26 -04:00",
                "EDT",
                "Y",
                "0008",
                "Yellow",
                "Male",
                "Adult",
                "Thomas \"Tommy\"",
                "Belamy",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0008",
                "Assets/ZoneYellow.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:09:26 -04:00",
                "EDT",
                "Y",
                "0009",
                "Green",
                "Female",
                "Adult",
                "Mary Jane",
                "Bolt",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0009",
                "Assets/ZoneGreen.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:10:26 -04:00",
                "EDT",
                "Y",
                "0010",
                "Yellow",
                "Female",
                "Pediatric",
                "Bella",
                "Zuber",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0010",
                "Assets/ZoneYellow.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:11:26 -04:00",
                "EDT",
                "Y",
                "0011",
                "Gray",
                "Female",
                "Adult",
                "", //[No Name]
                "",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0011",
                "Assets/ZoneGray.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:12:26 -04:00",
                "EDT",
                "Y",
                "0012",
                "Red",
                "Male",
                "Adult",
                "Maximillian",
                "Glum",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0012",
                "Assets/ZoneRed.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:13:26 -04:00",
                "EDT",
                "Y",
                "0008",
                "Green",
                "Male",
                "Pediatric",
                "Toc",
                "Flint",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0013",
                "Assets/ZoneGreen.png",
                "",
                "second ambulance"
                ));
            ltp.Add(new TP_PatientDataItem(
                "2012-08-13 18:14:26 -04:00",
                "EDT",
                "Y",
                "0014",
                "Black",
                "Male",
                "Adult",
                "Michael",
                "Redcliff",
                "1",
                "rockville",
                "Rockville earthquake",
                "TEST/DEMO/DRILL",
                "911-0014",
                "Assets/ZoneBlack.png",
                "",
                "second ambulance"
                ));

        }

        /// <summary>
        /// Applies flyout filters to candidate items for listing
        /// </summary>
        /// <param name="i"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        private bool InFilteredResults(TP_PatientDataItem i, string query) // compare InSearchResults(SampleDataItem i, string query);
        {
            string patientName = (i.FirstName + " " + i.LastName).ToLower();
            if (patientName == " ")
                patientName = "";
            string patientID = i.PatientID.ToLower();
            if (String.IsNullOrEmpty(patientName) && !App.CurrentFilterProfile.IncludeHasNoPatientName)
                return false;
            if (!String.IsNullOrEmpty(patientName) && !App.CurrentFilterProfile.IncludeHasPatientName)
                return false;
            App.MyAssert(!String.IsNullOrEmpty(i.Gender));
            switch (i.Gender)
            {
                case "Male":
                    if (!App.CurrentFilterProfile.IncludeMale)
                        return false; break;
                case "Female":
                    if (!App.CurrentFilterProfile.IncludeFemale)
                        return false; break;
                case "Unknown": case "Complex":
                    if (!App.CurrentFilterProfile.IncludeUnknownOrComplexGender)
                        return false; break;
                default:
                    App.MyAssert(false);
                    return false;
            }
            App.MyAssert(!String.IsNullOrEmpty(i.AgeGroup));
            switch (i.AgeGroup)
            {
                case "Adult":
                    if (!App.CurrentFilterProfile.IncludeAdult)
                        return false; break;
                case "Pediatric":
                    if (!App.CurrentFilterProfile.IncludePeds)
                        return false; break;
                case "Unknown Age Group":
                    if (!App.CurrentFilterProfile.IncludeUnknownAgeGroup)
                        return false; break;
                default:
                    App.MyAssert(false);
                    return false;
            }
            App.MyAssert(!String.IsNullOrEmpty(i.Zone));
            switch (i.Zone)
            {
                case "Green":
                    if (!App.CurrentFilterProfile.IncludeGreenZone)
                        return false; break;
                case "BH Green":
                    if (!App.CurrentFilterProfile.IncludeBHGreenZone)
                        return false; break;
                case "Yellow":
                    if (!App.CurrentFilterProfile.IncludeYellowZone)
                        return false; break;
                case "Red":
                    if (!App.CurrentFilterProfile.IncludeRedZone)
                        return false; break;
                case "Gray":
                    if (!App.CurrentFilterProfile.IncludeGrayZone)
                        return false; break;
                case "Black":
                    if (!App.CurrentFilterProfile.IncludeBlackZone)
                        return false; break;
                default:
                    App.MyAssert(false);
                    return false;
            }
            if (!String.IsNullOrEmpty(i.PicCount)) // Until picCount is fully implemented, just skip if empty
            {
                if(i.PicCount == "0" && !App.CurrentFilterProfile.IncludeHasNoPhoto)
                    return false;
                if(i.PicCount != "0" && !App.CurrentFilterProfile.IncludeHasPhotos)
                    return false;
            }
            App.MyAssert(!String.IsNullOrEmpty(i.EventType));
            if (i.EventType == "REAL" && !App.CurrentFilterProfile.DisasterEventIncludeReal)
                return false;
            if (i.EventType == "TEST/DEMO/DRILL" && !App.CurrentFilterProfile.DisasterEventIncludeTest)
                return false;

            if (query == string.Empty)
                return true;
            if (App.CurrentFilterProfile.SearchAgainstName && (patientName.StartsWith(query) || patientName.Contains(" " + query)))
                return true;
            if (App.CurrentFilterProfile.SearchAgainstID && patientID.Contains(query))
                return true;
            return false;
#if TODO
            String whenLocalTime,
            //String dateEDXL,
            String timezone,
            String sent,
            String patientID, // includes prefix
            String zone,
            String gender,
            String ageGroup,
            String firstName,
            String lastName,
            String picCount,
            String eventShortName,
            String eventName, // w/o suffix
            String eventType, // suffix
            String imagePrefix,
            String imagePath,
            String comments

            App.CurrentFilterProfile.DisasterEventIncludePrivate;
            App.CurrentFilterProfile.DisasterEventIncludePublic;
            // Done elsewhere FlyoutEventComboBoxChoice = "All events";
            // Hard to support with current state of PL, choice of events based on "Hospital/Organization":
            //"Current (if known, otherwise Default)"
            //"All Sharing These Reports"
            //"Specified:" ComboBox

#endif
        }



    }
}
#endif