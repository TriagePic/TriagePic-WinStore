using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.DataTransfer.ShareTarget;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace TP8 //was: ImageHelper2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebcamPage : TP8.Common.LayoutAwarePage // Glenn adds inheritence from LayoutAwarePagewas: MainPage
    {
        public WebcamPage()
        {
            InitializeComponent();
        }

        private ShareOperation _shareOperation;
        private WriteableBitmap _writeableBitmap;

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e); // Glenn adds. This will set the _pagekey variable, which otherwise would be null and later throw an exception in OnNavigatedFrom.
            var args = e.Parameter as ShareTargetActivatedEventArgs;
            
            if (args == null)
            {
                CaptureButton_Click(this, null); // Glenn adds
                return;
            }

            _shareOperation = args.ShareOperation;

            if (_shareOperation.Data.Contains(
                StandardDataFormats.Bitmap))
            {
                _bitmap = await _shareOperation.Data.GetBitmapAsync();
                await ProcessBitmap();
            }
            else if (_shareOperation.Data.Contains(
                StandardDataFormats.StorageItems))
            {
                _items = await _shareOperation.Data.GetStorageItemsAsync();
                await ProcessStorageItems();
            }
            else _shareOperation.ReportError(
                "TriagePic was unable to find a valid bitmap.");
        }

        private async Task LoadBitmap(IRandomAccessStream stream)
        {
            _writeableBitmap = new WriteableBitmap(1, 1);
            _writeableBitmap.SetSource(stream);
            _writeableBitmap.Invalidate();
            await Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => ImageTarget.Source = _writeableBitmap);
        }

        RandomAccessStreamReference _bitmap;

        private async Task ProcessBitmap()
        {
            if (_bitmap != null)
            {
                await LoadBitmap(await _bitmap.OpenReadAsync());
            }
        }

        IReadOnlyList<IStorageItem> _items;

        private async Task ProcessStorageItems()
        {
            foreach (var file in _items.Where(item => 
                item.IsOfType(StorageItemTypes.File))
                .Select(item => item as StorageFile).Where(file => file.ContentType
                    .StartsWith(
                    "image",
                    StringComparison.CurrentCultureIgnoreCase)))
            {
                await LoadBitmap(await file.OpenReadAsync());
                break;
            }
        }

        private void CheckAndClearShareOperation()
        {
            if (_shareOperation != null)
            {
                _shareOperation.ReportCompleted();
                _shareOperation = null;
            }
        }

        #region TopAppBar
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

/* Let's not do this, all the bottom controls take you back to New:
        private void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
        }
 */

        // Remaining are called ONLY from app navigation bar:

        private void AllStations_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "AllStations"); // Defined in SampleDataSource.cs
        }

        private void Outbox_Click(object sender, RoutedEventArgs e) // at moment, List icon on nav bar
        {
            this.Frame.Navigate(typeof(SplitPage), "Outbox"); // Defined in SampleDataSource.cs
        }

        private async void Statistics_Click(object sender, RoutedEventArgs e)
        {
            //SOON           if (App.CurrentVisualState == "Snapped" || App.CurrentVisualState == "Narrow")
            if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                // In 8.1, this replaces 8.0's TryToUnsnap:
                MessageDialog dlg = new MessageDialog("Please make TriagePic wider in order to show charts.");
                await dlg.ShowAsync();
                return;
            }
            this.Frame.Navigate(typeof(ChartsFlipPage), "pageCharts");
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups");// "pageRoot");
        }
        #endregion

        public async void CaptureButton_Click(object sender,
            RoutedEventArgs e)
        {
            // WAS Win 8.0: Windows.UI.ViewManagement.ApplicationView.TryUnsnap();// Glenn's quick hack, since this doesn't work well while snapped
            //SOON           if (App.CurrentVisualState == "Snapped" || App.CurrentVisualState == "Narrow")
            if (Windows.UI.ViewManagement.ApplicationView.Value == Windows.UI.ViewManagement.ApplicationViewState.Snapped)
            {
                MessageDialog dlg = new MessageDialog("Please make TriagePic wider in order to take a picture.");
                await dlg.ShowAsync();
                return;
            }
            CheckAndClearShareOperation();
            var camera = new CameraCaptureUI();
            //camera.PhotoSettings.Format = CameraCaptureUIPhotoFormat.Jpeg;
            //The user chooses the resolution (choices provided by the system), but the programmer can specify the max choice:
            // Glenn recommends 1.5 Mpx or less. There's a 0.9 Mpx choice for LifeCam, but can't really get to exactly that as max, given canned enumerated choices
            camera.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.Large3M; // 1920 x 1080, or a similar 4:3 rez.  Best we can do.  Will include 0.9, 2.1 for Lifecam
            //camera.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.HighestAvailable;  // 2.1 Mpix with MS LifeCam
            //camera.PhotoSettings.MaxResolution = CameraCaptureUIMaxPhotoResolution.MediumXga; //  1024 x 768 (or similar 16:9 rez).  For Lifecam, gives 0.5 Mpix

            // By default, format is jpg; other choices are png and jpx.
            // By default, cropping is allowed.  CameraCaptureUI has embedded cropping tool.  You can set rez or aspect ratio of crop.
            var result = await camera.CaptureFileAsync(CameraCaptureUIMode.Photo);
            if (result != null)
            {
                await LoadBitmap(await result.OpenAsync(
                    FileAccessMode.Read));
            }
        }

        public /*async*/ void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            /* TO DO (and uncomment async above):
            if (_writeableBitmap != null)
            {
                var picker = new FileSavePicker {SuggestedStartLocation = PickerLocationId.PicturesLibrary};
                picker.FileTypeChoices.Add("Image", new List<string> { ".png" });
                picker.DefaultFileExtension = ".png";
                picker.SuggestedFileName = "photo";
                var savedFile = await picker.PickSaveFileAsync();

                try
                {
                    if (savedFile != null)
                    {
                        using (var output = await
                            savedFile.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            var encoder =
                                await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, output);

                            byte[] pixels;

                            using (var stream = _writeableBitmap.PixelBuffer.AsStream())
                            {
                                pixels = new byte[stream.Length];
                                await stream.ReadAsync(pixels, 0, pixels.Length);
                            }

                            encoder.SetPixelData(BitmapPixelFormat.Rgba8,
                                                    BitmapAlphaMode.Straight,
                                                    (uint)_writeableBitmap.PixelWidth,
                                                    (uint)_writeableBitmap.PixelHeight,
                                                    96.0, 96.0,
                                                    pixels);

                            await encoder.FlushAsync();
                            await output.FlushAsync();
                        }
                    }
                }
                catch (Exception ex)
                {
                    var s = ex.ToString();
                }
                finally
                {
                    CheckAndClearShareOperation();
                }
            } */
#if PROBLEM
            if(_writeableBitmap != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {

                    // Convert image to byte[]
                    byte[] imgB = ConvertBitmapToByteArray(_writeableBitmap);
                    string base64image = Convert.ToBase64String(imgB);
                    App.CurrentPatient.ImageEncoded = base64image;
                }
                App.CurrentPatient.ImageName = App.CurrentPatient.FormatImageName();
            }
#endif
#if SECONDTRY
            if(_writeableBitmap != null)
            {
                //App.CurrentPatient.ImageBitmap.SetSource(_bitmap);
                MemoryStream ms = _writeableBitmap.PixelBuffer.AsStream().AsOutputStream();
                App.CurrentPatient.ImageBitmap.SetSource(s);
                App.CurrentPatient.ImageName = App.CurrentPatient.FormatImageName();
            }
#endif
            if (_writeableBitmap != null)
            {
                //App.CurrentPatient.ImageBitmap.SetSource(_bitmap);
                App.CurrentPatient.ImageWriteableBitmap = _writeableBitmap;
                App.CurrentPatient.ImageName = App.CurrentPatient.FormatImageName();
                //done later: App.CurrentPatient.ImageEncoded = await App.CurrentPatient.FormatImageEncoded();
                App.CurrentPatient.ImageEncoded = ""; // clear out any color swatch beginning with Assets, we have a real image
                if (App.CurrentPatient.SentCode != "" && !App.CurrentPatient.ObjSentCode.IsQueued()) // could look at WhenLocalTime instead
                    App.ReportAltered = true; // tell caller the image changed (though App.ReportAltered might already be true due to other field changes)
            }
            base.GoBack(sender, e);
#if WAS_BUT_DOESNT_LEAVE_NAV_TRAIL_RIGHT
/* WAS:
            if (App.CurrentPatient.SentCode == "Y") // could look at WhenLocalTime instead
            {
                if (_writeableBitmap != null)
                    App.ReportAltered = true; // tell caller the image changed (though App.ReportAltered might already be true due to other field changes)
                this.Frame.Navigate(typeof(BasicPageViewEdit), "pageViewEditReport");
            }
            else
                this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport"); */
            if (App.CurrentPatient.SentCode == "" || App.CurrentPatient.ObjSentCode.IsQueued()) // could look at WhenLocalTime instead
               this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
            else 
            {
                if (_writeableBitmap != null)
                    App.ReportAltered = true; // tell caller the image changed (though App.ReportAltered might already be true due to other field changes) //<<<Moved to above
                this.Frame.Navigate(typeof(BasicPageViewEdit), "pageViewEditReport");
            }
#endif
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            App.CurrentPatient.ImageWriteableBitmap = _writeableBitmap = null; // TO DO: Handle case where there was already an image in NewReport
            base.GoBack(sender, e);
#if WAS_BUT_DOESNT_LEAVE_NAV_TRAIL_RIGHT
/* WAS:
            if(App.CurrentPatient.SentCode == "Y") // could look at WhenLocalTime instead
                this.Frame.Navigate(typeof(BasicPageViewEdit), "pageViewEditReport");  // leave App.ReportAltered as is
            else
                this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport"); */
            if(App.CurrentPatient.SentCode == "" || App.CurrentPatient.ObjSentCode.IsQueued()) // could look at WhenLocalTime instead
                this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
            else
                this.Frame.Navigate(typeof(BasicPageViewEdit), "pageViewEditReport");  // leave App.ReportAltered as is
#endif
        }

/* Likness debug...
        private void Grid_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            ShowPointerPressed("Grid");
        }

        private void StackPanel_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            ShowPointerPressed("StackPanel");
            // uncomment the following line to prevent the event from bubbling further
            //e.Handled = true;
        }

        private void Rectangle_PointerPressed_1(object sender, PointerRoutedEventArgs e)
        {
            ShowPointerPressed("Rectangle");
        }

        private void ShowPointerPressed(string source)
        {
            var text = string.Format("Pointer pressed from {0}", source);
            Events.Text = string.Format("{0} // {1}", Events.Text, text);
            Debug.WriteLine(text);
        }
 */
    }
}
