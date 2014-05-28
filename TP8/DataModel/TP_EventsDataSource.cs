using System;
//using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
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
using System.Xml.Serialization;
using System.Collections;
using Windows.UI.Popups;
using LPF_SOAP;
using Newtonsoft.Json;

// This is the underlying TP data model, which is mapped to the SampleDataItem of the SampleDataSource model with its data bindings in the standard item templates.
namespace TP8.Data
{
    // Code here based on style of http jesseliberty.com/2012/08/21/windows-8-data-binding-part5binding-to-lists/
    public class TP_EventsDataItem : INotifyPropertyChanged
    {
    // WORKS_WITH_PNG_BUT_NOT_BMP:
        private string _typeIconUri;
        public string TypeIconUri
        {
            get { return _typeIconUri; }
            set
            {
                _typeIconUri = value;
                RaisePropertyChanged();
            }
        }

#if TRIED_TO_MAKE_BMP_WORK_BUT_GAVE_UP_USED_PNG
        private BitmapImage _typeIconUri;
        public BitmapImage TypeIconUri
        {
            get { return _typeIconUri; }
            set
            {
                _typeIconUri = value;
                RaisePropertyChanged();
            }
        }
#endif
        // Suppress RaisePropertyChanged, because it causes problems in Checklist page Event combo box.  But will suppression cause it's own problems?  Dunno
        private string _eventName;
        public string EventName
        {
            get { return _eventName; }
            set
            {
                _eventName = value;
                //RaisePropertyChanged();
            }
        }

        private string _eventType;
        public string EventType
        {
            get { return _eventType; }
            set
            {
                _eventType = value;
                //RaisePropertyChanged();
            }
        }

        private string _eventNumericID;
        public string EventNumericID
        {
            get { return _eventNumericID; }
            set
            {
                _eventNumericID = value;
                RaisePropertyChanged();
            }
        }

        private string _parentEventID; // can be null
        public string ParentEventID
        {
            get { return _parentEventID; }
            set
            {
                _parentEventID = value;
                RaisePropertyChanged();
            }
        }

        private string _eventShortName;
        public string EventShortName
        {
            get { return _eventShortName; }
            set
            {
                _eventShortName = value;
                RaisePropertyChanged();
            }
        }

        private string _eventDate; // 2001-01-01
        public string EventDate
        {
            get { return _eventDate; }
            set
            {
                _eventDate = value;
                RaisePropertyChanged();
            }
        }

        private string _eventLatitude;
        public string EventLatitude
        {
            get { return _eventLatitude; }
            set
            {
                _eventLatitude = value;
                RaisePropertyChanged();
            }
        }

        private string _eventLongitude;
        public string EventLongitude
        {
            get { return _eventLongitude; }
            set
            {
                _eventLongitude = value;
                RaisePropertyChanged();
            }
        }

        private string _eventStreet;
        public string EventStreet
        {
            get { return _eventStreet; }
            set
            {
                _eventStreet = value;
                RaisePropertyChanged();
            }
        }

        // Next items derives from .group, comma-separated list of interger group_id's with access to this event.
        // Null if event is public.  1 = for Admins, 5 = for Hospital Users, 7 = for Researchers.
        private bool _eventForPublic;  
        public bool EventForPublic
        {
            get { return _eventForPublic; }
            set
            {
                _eventForPublic = value;
                RaisePropertyChanged();
            }
        }

        private bool _eventForAdmins;
        public bool EventForAdmins
        {
            get { return _eventForAdmins; }
            set
            {
                _eventForAdmins = value;
                RaisePropertyChanged();
            }
        }

        private bool _eventForHospitalUsers;
        public bool EventForHospitalUsers
        {
            get { return _eventForHospitalUsers; }
            set
            {
                _eventForHospitalUsers = value;
                RaisePropertyChanged();
            }
        }

        private bool _eventForResearchers;
        public bool EventForResearchers
        {
            get { return _eventForResearchers; }
            set
            {
                _eventForResearchers = value;
                RaisePropertyChanged();
            }
        }

        private int _eventClosed; // Zero = open, 1 = closed to all reporting, 2 = reporting is only allowed to happen externally (like Google PF).
        public int EventClosed
        {
            get { return _eventClosed; }
            set
            {
                _eventClosed = value;
                RaisePropertyChanged();
            }
        }

        /* Compare with LPF_SOAP's:
                public class Pls_Incident_Response_Row
                {
                    public string incident_id { get; set; }
                    public string parent_id { get; set; } // can be null
                    public string name { get; set; }
                    public string shortname { get; set; }
                    public string date { get; set; } // 2001-01-01
                    public string type { get; set; }
                    public string latitude { get; set; }
                    public string longitude { get; set; }
                    public string street { get; set; } // Add 1.9.9
                    public string group { get; set; } // Added Oct 2013: comma-separated list of interger group_id's with access to this event.
                    // Null if event is public.  1 = for Admins, 5 = for Hospital Users, 7 = for Researchers.
                    public string closed { get; set; } // Add 1.9.9
                    // Zero means open, 1 is closed to all reporting, 2 means reporting is only allowed to happend externally (like Google PF).
                } */

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// Helpful for App.CurrentEvent.  Like clone, except receiving object already exists.
        /// </summary>
        /// <param name="tp"></param>
        public void CopyFrom(TP_EventsDataItem tp)
        {
            EventName = tp.EventName;
            TypeIconUri = tp.TypeIconUri;
            EventType = tp.EventType;
            EventShortName = tp.EventShortName;
            // rest just for completeness
            EventNumericID = tp.EventNumericID;
            ParentEventID = tp.ParentEventID;
            EventDate = tp.EventDate;
            EventLatitude = tp.EventLatitude;
            EventLongitude = tp.EventLongitude;
            EventStreet = tp.EventStreet;
            EventForPublic = tp.EventForPublic;
            EventForAdmins = tp.EventForAdmins;
            EventForHospitalUsers = tp.EventForHospitalUsers;
            EventForResearchers = tp.EventForResearchers;
            EventClosed = tp.EventClosed;
        }

        /// <summary>
        /// Helpful for App.ViewedDisaster.
        /// </summary>
        public void Clear()
        {
            EventName = "";
            TypeIconUri = "";
            EventType = "";
            EventShortName = "";
            // rest just for completeness
            EventNumericID = "";
            ParentEventID = "";
            EventDate = "";
            EventLatitude = "";
            EventLongitude = "";
            EventStreet = "";
            EventForPublic = false;
            EventForAdmins = false;
            EventForHospitalUsers = true;
            EventForResearchers = false;
            EventClosed = -1; // dummy value
        }

        public string GetIconUriFromEventType()
        {
            switch (EventType)
            {
                case "TEST/DEMO/DRILL":
                    return "ms-appx:///Assets/yellow_diamond.png";
                case "REAL":
                    return "ms-appx:///Assets/yellow_outlined_diamond.png";
                default:
                    App.MyAssert(false);
                    return "";
            }
        }

        public string GetEventTypeFromIconUri()
        {
            switch (TypeIconUri)
            {
                case "ms-appx:///Assets/yellow_diamond.png":
                    return "TEST/DEMO/DRILL";
                case "ms-appx:///Assets/yellow_outlined_diamond.png":
                    return "REAL";
                default:
                    App.MyAssert(false);
                    return "";
            }
        }
    }


    [XmlType(TypeName = "EventsDataList")]
    public class TP_EventsDataList : IEnumerable<TP_EventsDataItem>
    {
        const string EVENTS_DATA_LIST_FILENAME = "EventsDataList.xml";

        private List<TP_EventsDataItem> inner = new List<TP_EventsDataItem>();

        public void Add(object o)
        {
            inner.Add((TP_EventsDataItem)o);
        }

        public void Remove(TP_EventsDataItem o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_EventsDataItem> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_EventsDataItem> list)
        {
            inner = list;
        }

        public IEnumerator<TP_EventsDataItem> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_EventsDataItem o)
        {
            int index = inner.FindIndex(i => i.EventShortName == o.EventShortName); // C# 3.0 lambda expression
            if (index >= 0)
                inner[index] = o; // update
            else
                Add(o);
        }

        public bool IsPublic(string eventName)
        {
            int index = inner.FindIndex(i => i.EventName == eventName); // C# 3.0 lambda expression
            if (index >= 0)
                return inner[index].EventForPublic;
            return false; // if this is old or phony generated data, the associated event may be missing.  Default to private (ForResearchers) only
        }

        public bool IsPrivate(string eventName)
        {
            // For now, just treat Admins, Researchers, Hospital Users as in the same supergroup
            return !IsPublic(eventName);
        }

        public bool IsForAdmins(string eventName)
        {
            int index = inner.FindIndex(i => i.EventName == eventName); // C# 3.0 lambda expression
            if (index >= 0)
                return inner[index].EventForAdmins;
            return false;  // if this is old or phony generated data, the associated event may be missing.  Default to private (ForResearchers) only
        }

        public bool IsForHospitalUsers(string eventName)
        {
            int index = inner.FindIndex(i => i.EventName == eventName); // C# 3.0 lambda expression
            if (index >= 0)
                return inner[index].EventForHospitalUsers;
            return false;   // if this is old or phony generated data, the associated event may be missing.  Default to private (ForResearchers) only
        }

        public bool IsForResearchers(string eventName)
        {
            int index = inner.FindIndex(i => i.EventName == eventName); // C# 3.0 lambda expression
            if (index >= 0)
                return inner[index].EventForResearchers;
            App.MyAssert(false);
            return true;  // if this is old or phony generated data, the associated event may be missing.  Default to private (ForResearchers) only
        }

        public TP_EventsDataItem FindByIncidentID(string eventNumericID)
        {
            int index = inner.FindIndex(i => i.EventNumericID == eventNumericID); // C# 3.0 lambda expression
            if (index >= 0)
                return inner[index];
            return null;
        }


        public async Task Init()
        {
            bool exists = await DoesFileExistAsync();
            if(!exists)
                await GenerateDefaultEventsList(); // This will provide a default XML file with a little content if none exists

            // ProcessEventList will clear the list in memory, then call the web service.  If successful, memory list is available and also gets written to XML file.
            // If failed, will try to read the XML file instead.
            await ProcessEventList(App.pd.plUserName, App.pd.plPassword, true);
        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(EVENTS_DATA_LIST_FILENAME);
        }

        private async Task GenerateDefaultEventsList()
        {
            inner.Add(new TP_EventsDataItem() { EventName = "Test with TriagePic", EventType = "TEST/DEMO/DRILL", EventShortName = "testtp" });
            inner.Add(new TP_EventsDataItem() { EventName = "Test event #2", EventType = "TEST/DEMO/DRILL", EventShortName = "t2" });
            inner.Add(new TP_EventsDataItem() { EventName = "Rockville earthquake", EventType = "TEST/DEMO/DRILL", EventShortName = "rockville" });
            inner.Add(new TP_EventsDataItem() { EventName = "Real event #1", EventType = "REAL", EventShortName = "r1" });
            inner.Add(new TP_EventsDataItem() { EventName = "Real event #2", EventType = "REAL", EventShortName = "r1" });

            foreach (var item in inner)
            {
                if (item.EventType != "TEST/DEMO/DRILL" && item.EventType != "REAL")
                {
                    App.MyAssert(false);
                    item.EventType = "TEST/DEMO/DRILL"; // Force it
                }
                item.TypeIconUri = item.GetIconUriFromEventType();
                item.EventForPublic = false;
                item.EventForAdmins = true;
                item.EventForHospitalUsers = true;
                item.EventForResearchers = false;
            }
            await WriteXML();
        }

        public async Task InitAsFilters() // If CurrentDisasterListFilters, call this instead of Init, and after Init of CurrentDisasterList
        {

            // WORKS_WITH_PNG_BUT_NOT_BMP:
            inner.Add(new TP_EventsDataItem() { TypeIconUri = "ms-appx:///Assets/blank_for_filter.png", EventName = "Current event (recommended)" });
            inner.Add(new TP_EventsDataItem() { TypeIconUri = "ms-appx:///Assets/blank_for_filter.png", EventName = "All events" });
            await ReadXML(EVENTS_DATA_LIST_FILENAME, false); // could do adds from CurrentDisasterList instead...
        }

        public async Task ProcessEventList(string plname, string plpass, bool clearFirst)
        {
            // Compare Win 7 FormTriagePic.ProcessEventList(...)
            if (clearFirst)
                Clear();
            bool useCachedEventList = false; // This was global in Win 7
            List<Pls_Incident_Response_Row> incidentResponseRows = null; // likewise
            string s;
            s = await App.service.GetIncidentList(plname, plpass);
            if (s.StartsWith("ERROR:"))
            {
                // For the user, this is not an error
                App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + "  - Disaster event list\n"; 
                //MessageDialog dlg = new MessageDialog("Could not connect to web service to get disaster event list.  Using local cached version instead.");
                //await dlg.ShowAsync();
                useCachedEventList = true;
            }
            else
            {
                s = App.service.GetCleanedContentFromRawResults(s);
                if (s.Length == 0)
                {
                    await App.ErrorLog.ReportToErrorLog("During TP_EventsDataSource.ProcessEventList", "Invalid empty disaster event list", false);
                    // For the user, this is not an error
                    App.DelayedMessageToUserOnStartup = App.NO_OR_BAD_WEB_SERVICE_PREFIX + "  - Disaster event list (actually, got list from web service but it was empty)\n"; 
                    //MessageDialog dlg = new MessageDialog("Could not get valid disaster event list from web service.  Using local cached version instead.");
                    //await dlg.ShowAsync();
                    useCachedEventList = true;
                }
            }
            if (useCachedEventList)
            {
                await ReadXML(); // These results were previously filtered, so don't have to do that again.
            }
            else
            {
                incidentResponseRows = JsonConvert.DeserializeObject<List<Pls_Incident_Response_Row>>(s);
                // Remove rows closed to reporting:
                incidentResponseRows = App.service.FilterIncidentResponseRows(incidentResponseRows);
                if (incidentResponseRows != null)
                {
                    foreach (var item in incidentResponseRows)
                    {
                        // Similar to Win 7 servive.FilterIncidentResponseRows:
                        TP_EventsDataItem edi = new TP_EventsDataItem();
                        edi.EventName = item.name;
                        edi.EventShortName = item.shortname;
                        edi.EventType = item.type;
                        if (edi.EventType == "TEST")
                            edi.EventType = "TEST/DEMO/DRILL";
                        edi.TypeIconUri = edi.GetIconUriFromEventType();
                        // Rest of these included for completeness
                        edi.EventNumericID = item.incident_id;
                        edi.ParentEventID = item.parent_id;
                        edi.EventDate = item.date;
                        edi.EventLatitude = item.latitude;
                        edi.EventLongitude = item.longitude;
                        edi.EventStreet = item.street;
                        // item.group: comma-separated list of interger group_id's with access to this event.
                        // Null if event is public.  1 = for Admins, 5 = for Hospital Users, 7 = for Researchers.

                        edi.EventForPublic = (bool)String.IsNullOrEmpty(item.group);
                        if (edi.EventForPublic)
                        {
                            // These items are false in the sense that they are not JUST for the particular group
                            edi.EventForAdmins = false;
                            edi.EventForHospitalUsers = false;
                            edi.EventForResearchers = false;
                        }
                        else
                        {
                            // TO DO: Less lame parsing of item.group
                            edi.EventForAdmins = (bool)(item.group.Contains("1"));
                            edi.EventForHospitalUsers = (bool)(item.group.Contains("5"));
                            edi.EventForResearchers = (bool)(item.group.Contains("7"));
                        }
                        edi.EventClosed = Convert.ToInt32(item.closed); // 0 = open (only value that gets through filtering above)
                        //1 = closed to all reporting, 2 = reporting is only allowed to happen externally (like Google PF).
                        inner.Add(edi);
                    }
                }
                await WriteXML();
            }
        }

        public async Task ReadXML()
        {
            await ReadXML(EVENTS_DATA_LIST_FILENAME, true); // outbox
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_EventsDataItem>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_EventsDataItem);
                }
            }
        }

        public async Task WriteXML()
        {
            await WriteXML(EVENTS_DATA_LIST_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_EventsDataItem);

            await LocalStorage.Save<TP_EventsDataItem>(filename);
        }
    }

}
