using LPF_SOAP;
using Newtonsoft.Json;
using System;
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
#if TODO
    class TP_ZoneChoice : IOrderedEnumerable<string>
    {
        TP_ZoneChoice(){

    }
#endif

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



    public class TP_ZoneChoiceItem
    {
        private String _buttonName; // Should be short.  May contain spaces.  Use headline styling, e.g., begin with capital
        // buttonName, with spaces removed, will also be used as part of suggested filename for image.
        //private bool _buttonNameInBlack;
        private String _meaning; // a few words to indicate medical category
        private String _colorName; // May be one of defined Colors name, or MS defined color syntax for defining abgr object, e.g., #aarrggbb
        private Color _colorObj;

        public TP_ZoneChoiceItem()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="button_name"></param>
        /// <param name="meaning"></param>
        /// <param name="color_name"></param>
        /// <param name="color_obj"></param>
        public TP_ZoneChoiceItem(String button_name, /*bool button_name_in_black,*/ String meaning, String color_name, Color color_obj)
        {
            ButtonName = button_name;
            //ButtonNameInBlack = button_name_in_black;
            Meaning = meaning;
            ColorName = color_name;
            ColorObj = color_obj;
        }

        [XmlAttribute]
        public String ButtonName
        {
            get { return this._buttonName; }
            set { this._buttonName = value; }
        }
        //public bool ButtonNameInBlack
        //{
        //    get { return this._buttonNameInBlack; }
        //    set { this._buttonNameInBlack = value; }
        //}
        public String Meaning
        {
            get { return this._meaning; }
            set { this._meaning = value; }
        }
        public String ColorName
        {
            get { return this._colorName; }
            set { this._colorName = value; }
        }
        public Color ColorObj
        {
            get { return this._colorObj; }
            set { this._colorObj = value; }
        }
    }

    public class TP_ZoneChoices
    {
        private List<string> _zoneChoices; // differs from zoneChoiceList in having an empty string first, for combo box 'no selection yet'
        private List<TP_ZoneChoiceItem> _zoneChoiceList;

        private static string ZONE_CHOICES_FILENAME = "ZoneChoices.xml";

        public TP_ZoneChoices()
        {
            _zoneChoices = new List<string>() { "" }; // First item should always be "", zone not yet chosen
            _zoneChoiceList = new List<TP_ZoneChoiceItem>();

#if WAS_GENERATE_CHOICES
            // Includes "", zone not yet chosen
            _zoneChoices = new List<string>() { "", "Green", "BH Green", "Yellow", "Red", "Gray", "Black" };
            _zoneChoiceList = new List<TP_ZoneChoiceItem>();
            TP_ZoneChoiceItem z = new TP_ZoneChoiceItem();
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("Green", /*false,*/ "Treat eventually if needed"/*"Minor or no injury"*/, "Lime", Colors.Lime));
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("BH Green", /* false,*/ "Treat for behavioral health"/*"Behavioral health, needs counseling"*/, "Green", Colors.Green));
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("Yellow", /*true,*/ "Treat soon" /*"Urgent but not immediate"*/, "Yellow", Colors.Yellow));
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("Red", /*false,*/ "Treat immediately"/*"Immediate"*/, "Red", Colors.Red));
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("Gray", /*false,*/ "Cannot be saved", "Gray", Colors.Gray));
            _zoneChoiceList.Add(new TP_ZoneChoiceItem("Black", /*false,*/ "Deceased", "Black", Colors.Black));
#endif
        }

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
            return await LocalStorage.DoesFileExistAsync(ZONE_CHOICES_FILENAME);
        }

        private async Task GenerateDefaultChoices()
        {
#if DROPPED
// Default data dropped July 7, 2014.  More likely to cause problems than be helpful.  Just initialize file, empty except for root XML, instead.
            // Clear() and Add() will initialize both _zoneChoices and _zoneChoiceList
            // Includes "", zone not yet chosen
            Clear(); // just in case
            Add(new TP_ZoneChoiceItem("Green", "Treat eventually if needed"/*"Minor or no injury"*/, "Lime", Colors.Lime));
            Add(new TP_ZoneChoiceItem("BH Green", "Treat for behavioral health"/*"Behavioral health, needs counseling"*/, "Green", Colors.Green));
            Add(new TP_ZoneChoiceItem("Yellow", "Treat soon" /*"Urgent but not immediate"*/, "Yellow", Colors.Yellow));
            Add(new TP_ZoneChoiceItem("Red", "Treat immediately"/*"Immediate"*/, "Red", Colors.Red));
            Add(new TP_ZoneChoiceItem("Gray", "Cannot be saved", "Gray", Colors.Gray));
            Add(new TP_ZoneChoiceItem("Black", "Deceased", "Black", Colors.Black));
#endif
            await WriteXML();
        }


        public void Clear()
        {
            _zoneChoiceList.Clear();
            _zoneChoices.Clear();
            _zoneChoices.Add(""); // Always begin with "", zone not yet chosen
        }

        public void Add(object o)
        {
            _zoneChoiceList.Add((TP_ZoneChoiceItem)o);
            _zoneChoices.Add(((TP_ZoneChoiceItem)o).ButtonName);
        }

        public List<string> GetZoneChoices() { return _zoneChoices; }
        public List<TP_ZoneChoiceItem> GetZoneChoiceList() { return _zoneChoiceList; }

        public bool ZoneVerify(string zone)
        {
            foreach (string s in _zoneChoices)
            {
                if (s == zone)
                    return true;// zone not yet chosen is also OK
            }
            return false;
        }

        public string GetColorNameFromZoneName(string zone) // used by FormatImageBorderColor()
        {
            foreach (var zoneChoice in _zoneChoiceList)
                if (zone == zoneChoice.ButtonName)
                    return zoneChoice.ColorName;
            return "White"; // default
            /*
                        switch (Zone)
                        {
                            // Colors are same ones used for zone .png color swatches in Assets
                            case "Green":
                                return Colors.Lime.ToString();
                            case "BH Green":
                                return Colors.Green.ToString();
                            case "Yellow":
                                return Colors.Yellow.ToString();
                            case "Red":
                                return Colors.Red.ToString();
                            case "Gray":
                                return Colors.Gray.ToString();
                            case "Black":
                                return Colors.Black.ToString();
                            default:
                                return Colors.White.ToString();
 
                        } */
        }

        public async Task ParseJsonList(string s) // from web service hpout.triageZoneList
        {
            Clear();
                // convert from json
            List<HospitalPolicyTriageZone> zones = JsonConvert.DeserializeObject<List<HospitalPolicyTriageZone>>(s);
            if (zones != null)
            {
                foreach (var item in zones)
                {
                    // Similar to Win 7 servive.FilterIncidentResponseRows:
                    TP_ZoneChoiceItem zci = new TP_ZoneChoiceItem();
                    zci.ButtonName = item.name;
                    zci.Meaning = item.description;
                    zci.ColorName = item.button_color_name; // optional for TriageTrak
                    if (zci.ColorName == "")  // Hard to find nearest named color from arbitrary rgb value; best algorithms would work in non-RGB space, e.g., HSV.
                        zci.ColorName = item.button_color_rgb; // Do we have to convert this to argb form from rgb?
                    zci.ColorObj = GetColorFromHexString(item.button_color_rgb);
                    Add(zci); // adds to _zoneChoices and _zoneChoiceList
                }
            }
            await WriteXML(); // update
        }


        // From Sara Silver post, http social.msdn.microsoft.com/Forums/.../convert-string-to-color-in-metro
        private Color GetColorFromHexString(string hexValue)
        {
            // Expected format from web service is #RRGGBB. Note no AA
            /* If hexValue had AA, we might do:
            hexValue = hexValue.Replace("#","");
            var a = Convert.ToByte(hexValue.Substring(0, 2), 16);
            var r = Convert.ToByte(hexValue.Substring(2, 2), 16);
            var g = Convert.ToByte(hexValue.Substring(4, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(6, 2), 16); */
            var a = Convert.ToByte("FF", 16); // opaque color.  "16" means base 16 interpretation, for hex
            var r = Convert.ToByte(hexValue.Substring(1, 2), 16); // skips over "#"
            var g = Convert.ToByte(hexValue.Substring(3, 2), 16);
            var b = Convert.ToByte(hexValue.Substring(5, 2), 16);
            return Color.FromArgb(a, r, g, b);
        }

        public async Task ReadXML()
        {
            await ReadXML(ZONE_CHOICES_FILENAME, true); // outbox
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_ZoneChoiceItem>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    _zoneChoiceList.Add(item as TP_ZoneChoiceItem);
                }
            }
            App.LocalStorageDataSemaphore.Release();
        }

        public async Task WriteXML()
        {
            await WriteXML(ZONE_CHOICES_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            foreach (var item in _zoneChoiceList)
                LocalStorage.Add(item as TP_ZoneChoiceItem);

            await LocalStorage.Save<TP_ZoneChoiceItem>(filename);
            App.LocalStorageDataSemaphore.Release();
        }
    }

}
