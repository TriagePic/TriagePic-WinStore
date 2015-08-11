using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Collections.Specialized;
using System.Diagnostics;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.UI;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace TP8.Data
{
    /// <summary>
    /// Base class for <see cref="SampleDataItem"/> and <see cref="SampleDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class SampleDataCommon : TP8.Common.BindableBase
    {
        private static Uri _baseUri = new Uri("ms-appx:///");

        public SampleDataCommon(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._subtitle = subtitle;
            this._description = description;
            this._imagePath = imagePath;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _subtitle = string.Empty;
        public string Subtitle
        {
            get { return this._subtitle; }
            set { this.SetProperty(ref this._subtitle, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private string _imagePath = string.Empty;
        public string ImagePath
        {
            get { return this._imagePath; }
            set { this.SetProperty(ref this._imagePath, value); }
        }

        private ImageSource _image = null;
        public ImageSource Image
        {
            get
            {
               if (this._image != null || this._imagePath == null)
                   return _image;
               if (_imagePath.StartsWith("Assets/")) // kludge transition for fixed-color tiles
                   this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, this._imagePath));
               else if (_imagePath.Contains("/plus_cache/") || String.IsNullOrEmpty(_imagePath))
                   this._image = new BitmapImage(new Uri(SampleDataCommon._baseUri, "Assets/SquareCameraLogo(150x150).png")); // TEMP KLUDGE... In ideal world wouldn't get here
               else
                   GetImageFromEncoding();
               return this._image;
            }

            set
            {
                //this._imagePath = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public async void GetImageFromEncoding()  // See also similar function in TP_PatientReportsSource
        {
 
            var buffer = Convert.FromBase64String(_imagePath);
            var bimage = new BitmapImage();
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(buffer);
                    await writer.StoreAsync();
                }

                bimage.SetSource(ms);
            }
            this._image = bimage;
            return;
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._imagePath = path;
            this.OnPropertyChanged("Image");
        }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem : SampleDataCommon
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, string /*Color*/ imageBorderColor, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
            this._imageBorderColor = imageBorderColor; // Glenn adds
        }
/* WAS:
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content, SampleDataGroup group)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            this._content = content;
            this._group = group;
        }
*/

        private string _content = string.Empty;
        public string Content
        {
            get { return this._content; }
            set { this.SetProperty(ref this._content, value); }
        }

        private SampleDataGroup _group;
        public SampleDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }

        private string /*Color*/ _imageBorderColor = Colors.Transparent.ToString(); // Glenn adds
        public string /*Color*/ ImageBorderColor
        {
            get { return this._imageBorderColor; }
            set { this.SetProperty(ref this._imageBorderColor, value); }
        }

    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup : SampleDataCommon
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, /*Color imageBorderColor,*/ String description)
            : base(uniqueId, title, subtitle, imagePath, description)
        {
            Items.CollectionChanged += ItemsCollectionChanged;
        }

        private void ItemsCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex,Items[e.NewStartingIndex]);
                        if (TopItems.Count > 12)
                        {
                            TopItems.RemoveAt(12);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldStartingIndex < 12 && e.NewStartingIndex < 12)
                    {
                        TopItems.Move(e.OldStartingIndex, e.NewStartingIndex);
                    }
                    else if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        TopItems.Add(Items[11]);
                    }
                    else if (e.NewStartingIndex < 12)
                    {
                        TopItems.Insert(e.NewStartingIndex, Items[e.NewStartingIndex]);
                        TopItems.RemoveAt(12);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems.RemoveAt(e.OldStartingIndex);
                        if (Items.Count >= 12)
                        {
                            TopItems.Add(Items[11]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldStartingIndex < 12)
                    {
                        TopItems[e.OldStartingIndex] = Items[e.OldStartingIndex];
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    TopItems.Clear();
                    while (TopItems.Count < Items.Count && TopItems.Count < 12)
                    {
                        TopItems.Add(Items[TopItems.Count]);
                    }
                    break;
            }
        }

        private ObservableCollection<SampleDataItem> _items = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> Items
        {
            get { return this._items; }
        }

        private ObservableCollection<SampleDataItem> _topItem = new ObservableCollection<SampleDataItem>();
        public ObservableCollection<SampleDataItem> TopItems
        {
            get {return this._topItem; }
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with hard-coded content.
    /// 
    /// SampleDataSource initializes with placeholder data rather than live production
    /// data so that sample data is provided at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _allGroups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<SampleDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");
            
            return _sampleDataSource.AllGroups;
        }

        public static SampleDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        private static SampleDataGroup GetGroup2(string uniqueId) // Glenn's hack, avoid crash if GetGroup is called from Define... functions below
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataGroup GetGroupFromItem(string uniqueId) // Glenn adds
        {
            // Simple linear search is acceptable for small data sets
            foreach (var group in _sampleDataSource.AllGroups)
            {
                foreach (var item in group.Items)
                {
                    if (item.UniqueId == uniqueId)
                        return group;
                }
            }
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId) // Problematic if item is in multiple groups.  Use next form of function instead.
        {
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static SampleDataItem GetItem(string uniqueId, string groupName)
        {
            var group = GetGroup(groupName);
            App.MyAssert(group != null);
            // Simple linear search is acceptable for small data sets
            foreach (var item in group.Items)
            {
                if (item.UniqueId == uniqueId)
                    return item;
            }
            return null;
        }

        // Don't know if necessary
        public static SampleDataItem GetItem(int index, string groupName)
        {
            var group = GetGroup(groupName);
            App.MyAssert(group != null);
            App.MyAssert(index >= 0);
            App.MyAssert(index < group.Items.Count);
            return group.Items[index];
        }

        public static bool PutItem(SampleDataItem sdi) // Glenn adds
        {
            if (GetItem(sdi.UniqueId) != null)
                return false; // Unique ID conflict
            var group = sdi.Group;
            group.Items.Add(sdi);
            return true;
        }

        // v 3.5, next 3 functions made async Task return type, and await call added to DefineItemsFiltered
        public static async Task RefreshOutboxItems() // Glenn adds
        {
            await _sampleDataSource.DefineItemsFiltered(GetGroup("Outbox"));
        }

        public static async Task RefreshAllStationsItems() // Glenn adds
        {
            await _sampleDataSource.DefineItemsFiltered(GetGroup("AllStations"));
        }

        public static async Task RefreshOutboxAndAllStationsItems() // Glenn adds
        {
            await _sampleDataSource.DefineItemsFiltered(GetGroup("Outbox"));
            await _sampleDataSource.DefineItemsFiltered(GetGroup("AllStations"));
        }

        public SampleDataSource()
        {
            //String ITEM_CONTENT = String.Format("Item Content: {0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}\n\n{0}",
            //            "Curabitur class aliquam vestibulum nam curae maecenas sed integer cras phasellus suspendisse quisque donec dis praesent accumsan bibendum pellentesque condimentum adipiscing etiam consequat vivamus dictumst aliquam duis convallis scelerisque est parturient ullamcorper aliquet fusce suspendisse nunc hac eleifend amet blandit facilisi condimentum commodo scelerisque faucibus aenean ullamcorper ante mauris dignissim consectetuer nullam lorem vestibulum habitant conubia elementum pellentesque morbi facilisis arcu sollicitudin diam cubilia aptent vestibulum auctor eget dapibus pellentesque inceptos leo egestas interdum nulla consectetuer suspendisse adipiscing pellentesque proin lobortis sollicitudin augue elit mus congue fermentum parturient fringilla euismod feugiat");
            //String ITEM_TITLE = "John Doe"; //If specific examples not used
            // Content can be fullsome, only shown when full record shown.  Description is a 1 liner
            //String ITEM_CONTENT = String.Format("{0} {1}\n{2}", "Male", "Adult", "Zone: Green");
            //String ITEM_SUBTITLE = "Mass Casualty ID 911-"; // Append specific numbers; or override with specific examples not used
            //String ITEM_DESCRIPTION = String.Format("{0} {1}     {2}", "Male", "Adult", "Zone: Green");

            // Need junk text?  Here's some:
            // Lorem ipsum dolor sit amet, consectetur adipiscing elit. Vivamus tempor scelerisque lorem in vehicula. Aliquam tincidunt,
            // lacus ut sagittis tristique, turpis massa volutpat augue, eu rutrum ligula ante a ante.
            // Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue.
            // Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.

            var group1 = new SampleDataGroup("Checklist",
                    "✔  Checklist", //was: "✅  Checklist", but couldn't see with 8.1 multicolor fonts, couldn't see black outline on charcoal background
                    "Specify Disaster Event, Station Roster",//"Group Subtitle: 1",
                    "Assets/Red.png",
                    "Group Description: not used");
            this.AllGroups.Add(group1);

            var group2 = new SampleDataGroup("New",
                    "🚑  New Report",
                    "Report a New Arrival to this Station",
                    "Assets/LightGray.png",
                    "Group Description: not used");
// Evidently we have to have at least 1 item in group to show up:

            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Item Title: 1",
                    "Item Subtitle: 1",
                    "Assets/DarkGray.png",
                    Colors.White.ToString(), // image border color, probably not used
                    "Item Description: not used",
                    "Item Content: not used",
                    group2));
            this.AllGroups.Add(group2);
/* WAS:
            group2.Items.Add(new SampleDataItem("Group-2-Item-1",
                    "Item Title: 1",
                    "Item Subtitle: 1",
                    "Assets/DarkGray.png",
                    "Item Description: not used",
                    "Item Content: not used",
                    group2));
            this.AllGroups.Add(group2);
*/

            var groupOutbox = new SampleDataGroup("Outbox",
                    "📮  Outbox",
                    "View and Edit this Station's Reports", //Group Subtitle: 3",
                    "Assets/MediumGray.png",
                    "Group Description: not used");

            DefineItemsFiltered(groupOutbox).Wait();  // Wait added for v 3.5. Since we're in constructor, can't use "await"

            this.AllGroups.Add(groupOutbox);

            // Use the timestamp as UniqueID.  In Win7 TriagePic, this would be either:
            // EDXL format ("dateEDXL"), e.g., 2012-08-13T22:24:26Z
            // WhenLocalTime, e.g., 2012-08-13 18:24:26 -04:00  (with time zone "TZ" also given, e.g., EDT)
            // Latter is more informative

            var groupAllStations = new SampleDataGroup("AllStations",
                    "👪  All Stations",
                    "With More Filters, of All Reports", //"Group Subtitle: 4",
                    "Assets/LightGray.png",
                    "Group Description: not used");

            DefineItemsFiltered(groupAllStations).Wait();  //Wait added for v 3.5.  Since we're in constructor, can't use "await"

            this.AllGroups.Add(groupAllStations);

            var group5 = new SampleDataGroup("Statistics",
                    "📊  Statistics",
                    "Tables & Charts of Arrivals",
                    "Assets/MediumGray.png",
                    "Group Description: [TO DO]");
            this.AllGroups.Add(group5);
        }

        // was before 3.5: public void DefineItemsFiltered(SampleDataGroup g)
        public async Task DefineItemsFiltered(SampleDataGroup g)
        {
            // if (App.PatientDataGroups == null) {
            //   App.PatientDataGroups = new TP_PatientDataGroups();
            //   await App.PatientDataGroups.Init(); }
            App.MyAssert(App.PatientDataGroups != null);
            TP_PatientReports lpfi;
            if (g.UniqueId == "Outbox")
            {
                //lpfi = App.PatientDataGroups.GetOutbox();
                //LoadSampleDataGroupFromPatientDataList(g, lpfi);
                //lpfi = App.PatientDataGroups.GetOutboxSorted();
                //LoadSampleDataGroupFromPatientDataList(g, lpfi);
                lpfi = App.PatientDataGroups.GetOutboxSortedAndFiltered();
                await LoadSampleDataGroupFromPatientDataList(g, lpfi); // v 3.5 add await
            }
            else if (g.UniqueId == "AllStations")
            {
                //lpfi = App.PatientDataGroups.GetAllStations();
                //LoadSampleDataGroupFromPatientDataList(g, lpfi);
                //lpfi = App.PatientDataGroups.GetAllStationsSorted();
                //LoadSampleDataGroupFromPatientDataList(g, lpfi);
                lpfi = App.PatientDataGroups.GetAllStationsSortedAndFiltered();
                await LoadSampleDataGroupFromPatientDataList(g, lpfi); // v 3.5 add await
            }
            else
            {
                App.MyAssert(false);
            }
        }

        // was before v 3.5: private async void LoadSampleDataGroupFromPatientDataList(SampleDataGroup g, TP_PatientReports p)
        private async Task LoadSampleDataGroupFromPatientDataList(SampleDataGroup g, TP_PatientReports p)
        {
            g.Items.Clear();
            //string encodedTemp;
            foreach (TP_PatientReport pdi in p)
            {
                pdi.ImageEncoded = await pdi.FormatImageEncoded();
                g.Items.Add(new SampleDataItem(
                    pdi.FormatUniqueID(),
                    pdi.FormatTitle(),
                    pdi.FormatSubtitle(),
                    pdi.ImageEncoded, // freshened above
                    pdi.FormatImageBorderColor(),
                    pdi.FormatDescription(),
                    pdi.FormatContent(),
                    g));
            }
        }

    }
}
