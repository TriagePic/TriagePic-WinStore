using LPF_SOAP;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TP8.Data // nah .DataModel
{
#if PROBABLY_NOT

    public class ZoneChoiceBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value is int))
                return null;
            int index = (int)value;

            switch (index)
            {
                case 0:
                    return Colors.Green;
                case 1:
                    return Colors.DarkGreen;
                case 2:
                    return Colors.Yellow;
                case 3:
                    return Colors.Red;
                case 4:
                    return Colors.Gray;
                case 5:
                    return Colors.Black;
                default:
                    return Colors.White;
            }
        }

        // Compiler requires this, but since we're just doing one-way binding, no need to implement it:
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
#endif


    public class TP_ZoneFilterItem
    {
        private String _zoneName; // Should be short.  May contain spaces.  Use headline styling, e.g., begin with capital
        private bool _isIncluded;

        public TP_ZoneFilterItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="zone_name">Zone prompt name, same as on zone button</param>
        /// <param name="is_included">Is it checked?</param>
        public TP_ZoneFilterItem(String zone_name, bool is_included)
        {
            ZoneName = zone_name;
            IsIncluded = is_included;
        }

        [XmlAttribute]
        public String ZoneName
        {
            get { return this._zoneName; }
            set { this._zoneName = value; }
        }
        public bool IsIncluded
        {
            get { return this._isIncluded; }
            set { this._isIncluded = value; }
        }
    }

    public class TP_ZoneFilters : IEnumerable<TP_ZoneFilterItem>
    {
        //private List<string> _zoneFilters; // differs from zoneFilterList in having an empty string first, for combo box 'no selection yet'
        private List<TP_ZoneFilterItem> _zoneFilterList;
#if PROBABLY_NOT
        private static string ZONE_FILTERS_FILENAME = "ZoneFilters.xml";
#endif
        public TP_ZoneFilters()
        {
            //_zoneFilters = new List<string>() { "" }; // First item should always be "", zone not yet chosen
            _zoneFilterList = new List<TP_ZoneFilterItem>();

        }

        public List<TP_ZoneFilterItem> GetAsList() { return _zoneFilterList; }
/*
        public void GenerateDefaultChoices()  // first pass
        {
            Clear(); // just in case

            Add(new TP_ZoneFilterItem("Green", true));
            Add(new TP_ZoneFilterItem("BH Green", true));
            Add(new TP_ZoneFilterItem("Yellow", true));
            Add(new TP_ZoneFilterItem("Red", true));
            Add(new TP_ZoneFilterItem("Gray", true));
            Add(new TP_ZoneFilterItem("Black", true));
        }
*/
        public void GenerateDefaultChoices(List<TP_ZoneChoiceItem> zc) // second pass
        {
            Clear(); // just in case
            foreach(TP_ZoneChoiceItem zci in zc) //App.ZoneChoices.GetZoneChoiceList())
            {
                if (String.IsNullOrEmpty(zci.ButtonName))
                    continue;
                Add(new TP_ZoneFilterItem(zci.ButtonName, true)); // All selected by default
            }
        }

        public void Clear()
        {
            _zoneFilterList.Clear();
        }

        public void Add(object o)
        {
            _zoneFilterList.Add((TP_ZoneFilterItem)o);
        }

        public TP_ZoneFilterItem Find(string zone_name)
        {
            int index = _zoneFilterList.FindIndex(i => i.ZoneName == zone_name); // C# 3.0 lambda expression
            if (index >= 0)
                return _zoneFilterList[index]; // update
            //else
            return null;
        }

        public bool IsIncluded(string zone_name)
        {
            TP_ZoneFilterItem zfi = Find(zone_name);
            if (zfi == null)
                return false; // More useful than return null (where function would use return type "bool?");
            return zfi.IsIncluded;
        }

        public bool AllIncluded()
        {
            int index = _zoneFilterList.FindIndex(i => i.IsIncluded == false); // C# 3.0 lambda expression
            return (bool)(index < 0);
        }

/// To we need the following functions?

        public void Remove(TP_ZoneFilterItem o)
        {
            _zoneFilterList.Remove(o);
        }


        public void ReplaceWithList(List<TP_ZoneFilterItem> list)
        {
            _zoneFilterList = list;
        }

        public IEnumerator<TP_ZoneFilterItem> GetEnumerator()
        {
            return _zoneFilterList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_ZoneFilterItem o)
        {
            int index = _zoneFilterList.FindIndex(i => i.ZoneName == o.ZoneName); // C# 3.0 lambda expression
            if (index >= 0)
                _zoneFilterList[index] = o; // update
            else
                Add(o);
        }

        public bool UpdateIfFound(string zone_name, bool is_included)
        {
            int index = _zoneFilterList.FindIndex(i => i.ZoneName == zone_name); // C# 3.0 lambda expression
            if (index >= 0)
            {
                _zoneFilterList[index].IsIncluded = is_included; // update
                return true;
            }
            return false;
        }
#if PROBABLY_NOT

        public async Task Init()
        {
            bool exists = await DoesFileExistAsync();
            if(!exists)
                await GenerateDefaultChoices(); // This will provide a default XML file with a little content if none exists.  Usually overwritten by web service data.
            // These is no need for ProcessZoneChoices here.
            // The call to the web service to get the data is done by HospitalPolicy, which also distributes the results into our data structures by a call to
            // ParseJsonList below.
        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(ZONE_FILTERS_FILENAME);
        }

        private async Task GenerateDefaultChoices()
        {
            // Clear() and Add() will initialize both _zoneChoices and _zoneChoiceList
            // Includes "", zone not yet chosen
            Clear(); // just in case
            Add(new TP_ZoneFilterItem("Green", true));
            Add(new TP_ZoneFilterItem("BH Green", true));
            Add(new TP_ZoneFilterItem("Yellow", true));
            Add(new TP_ZoneFilterItem("Red", true));
            Add(new TP_ZoneFilterItem("Gray", true));
            Add(new TP_ZoneFilterItem("Black", true));

            await WriteXML();
        }


        public void Clear()
        {
            _zoneFilterList.Clear();
            //_zoneFilters.Clear();
            //_zoneFilters.Add(""); // Always begin with "", zone not yet chosen
        }

        public void Add(object o)
        {
            _zoneFilterList.Add((TP_ZoneFilterItem)o);
            //_zoneFilters.Add(((TP_ZoneFilterItem)o).ButtonName);
        }

        //public List<string> GetZoneFilters() { return _zoneFilters; }

        //public bool ZoneVerify(string zone)
        //{
        //    foreach (string s in _zoneFilters)
        //    {
        //        if (s == zone)
        //            return true;// zone not yet chosen is also OK
        //    }
        //    return false;
        //}


        public async Task ReadXML()
        {
            await ReadXML(ZONE_FILTERS_FILENAME, true); // outbox
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_ZoneFilterItem>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    _zoneFilterList.Add(item as TP_ZoneFilterItem);
                }
            }
        }

        public async Task WriteXML()
        {
            await WriteXML(ZONE_FILTERS_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            foreach (var item in _zoneFilterList)
                LocalStorage.Add(item as TP_ZoneFilterItem);

            await LocalStorage.Save<TP_ZoneFilterItem>(filename);
        }
#endif
    }

}
