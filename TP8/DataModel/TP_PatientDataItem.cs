//#define V2
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.UI.Popups;
using System.Xml.Serialization;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;
using Windows.UI; // needed for WriteableBitmap.PixelBuffer.ToStream()

#if SETASIDE
namespace TP8_1.Data
{
    /// <summary>
    /// Underlying TriagePic item data model.  Compare with TriagePic Win 7 PatientReportData and Outbox
    /// </summary>
    // This .Net class is no longer available in Win 8[Serializable]
    public class TP_PatientDataItem
    {
        // Serializer will ignore private fields without having to be told.
        private String _whenLocalTime = string.Empty;
        private String _timezone = string.Empty;
#if V2
        private String _dateEDXL = string.Empty;
        private String _distributionID_EDXL = string.Empty;
        private String _senderID_EDXL = string.Empty;
        private String _deviceName = string.Empty;
        private String _userNameForDevice = string.Empty;
        private String _userNameForWebService = string.Empty;
        private String _orgName = string.Empty;
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
#endif
        private String _sent = string.Empty; // Sent code
        private String _patientID = string.Empty; // includes prefix.  If we implement practice mode, may begin with "Prac"
        private String _zone = string.Empty;
        private String _gender = string.Empty;
        private String _ageGroup = string.Empty;
        private String _firstName = string.Empty;
        private String _lastName = string.Empty;
        private String _picCount = string.Empty;
        private String _eventShortName = string.Empty; // event short name at PL
        private String _eventName = string.Empty; // w/o suffix
        private String _eventType = string.Empty; // suffix
        private String _imageName = string.Empty; // name when decoded, w/o path
        private String _imageEncoded = string.Empty;
        private String _imageCaption = string.Empty;
        private String _comments = string.Empty;
#if V2
        // TO DO: station staff
        private String _whyNoPhotoReason = string.Empty;
#endif
        //private BitmapImage _imageBitmap = null; // Ignored for serialization
        private WriteableBitmap _imageWriteableBitmap = null;
        //etc.
        [XmlAttribute]
        public String WhenLocalTime
        {
            get { return this._whenLocalTime; }
            set { this._whenLocalTime = value; }
        }
        //public String dateEDXL,
        public String Timezone
        {
            get { return this._timezone; }
            set { this._timezone = value; }
        }

#if V2
        public String DateEDXL
        {
            get { return this._dateEDXL; }
            set { this._dateEDXL = value; }
        }
        public String DistributionID_EDXL
        {
            get { return this._distributionID_EDXL; }
            set { this._distributionID_EDXL = value; }
        }
        public String SenderID_EDXL
        {
            get { return this._senderID_EDXL; }
            set { this._senderID_EDXL = value; }
        }
        public String DeviceName // Win 7: MachineName
        {
            get { return this._deviceName; }
            set { this._deviceName = value; }
        }
        public String UserNameForDevice  // Win 7: UserName
        {
            get { return this._userName; }
            set { this._userName = value; }
        }
        public String UserNameForWebService // Win 7: PL_Name
        {
            get { return this._userNameForWebService; }
            set { this._userNameForWebService = value; }
        }
        public String OrgName
        {
            get { return this._orgName; }
            set { this._orgName = value; }
        }
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
#endif
        public String Sent
        {
            get { return this._sent; }
            set { this._sent = value; }
        }
        public String PatientID // includes prefix
        {
            get { return this._patientID; }
            set { this._patientID = value; }
        }
        public String Zone
        {
            get { return this._zone; }
            set { this._zone = value; }
        }
        public String Gender
        {
            get { return this._gender; }
            set { this._gender = value; }
        }
        public String AgeGroup
        {
            get { return this._ageGroup; }
            set { this._ageGroup = value; }
        }
        public String FirstName
        {
            get { return this._firstName; }
            set { this._firstName = value; }
        }
        public String LastName
        {
            get { return this._lastName; }
            set { this._lastName = value; }
        }
        public String PicCount
        {
            get { return this._picCount; }
            set { this._picCount = value; }
        }
        public String EventShortName // Event short name at PL,
        {
            get { return this._eventShortName; }
            set { this._eventShortName = value; }
        }
        public String EventName // w/o suffix
        {
            get { return this._eventName; }
            set { this._eventName = value; }
        }
        public String EventType // suffix
        {
            get { return this._eventType; }
            set { this._eventType = value; }
        }

        public String ImageName // Suggested image name when decoded, w/o path
        {
            get { return this._imageName; }
            set { this._imageName = value; }
        }

        public String ImageEncoded
        {
            get { return this._imageEncoded; }
            set { this._imageEncoded = value; }
        }
        public String ImageCaption
        {
            get { return this._imageCaption; }
            set { this._imageCaption = value; }
        }
        //        [XmlIgnore]
        //        public BitmapImage ImageBitmap
        //        {
        //            get { return this._imageBitmap; }
        //            set { this._imageBitmap = value; }
        //        }

        [XmlIgnore]
        public WriteableBitmap ImageWriteableBitmap
        {
            get { return this._imageWriteableBitmap; }
            set { this._imageWriteableBitmap = value; }
        }

        public String Comments
        {
            get { return this._comments; }
            set { this._comments = value; }
        }

#if V2
        // TO DO: station staff
        public String WhyNoPhotoReason
        {
            get { return this._whyNoPhotoReason; }
            set { this._whyNoPhotoReason = value; }
        }
#endif

        public TP_PatientDataItem() { } // Parameterless constructor required for serializer

        public TP_PatientDataItem(
            String whenLocalTime,
            String timezone,
 #if V2
            String dateEDXL,
            String distributionID_EDXL,
            String senderID_EDXL,
            String deviceName,
            String userNameForDevice,
            String userNameForWebService,
            String orgName,
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
#endif
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
            String imageName,
            String imageEncoded,
            String imageCaption,
            String comments
#if V2
            , String whyNoPhotoReason
#endif
            /* LATER:

                        ,
                        // Data not specifically about patient, but about situation at time of report:
                        String sentWebServicesOK,
                        String outboxSentCode,
                        String patientTrackingOfficer,
                        String triagePhysiciansOrRNs,
                        String otherStationStaff,
                        String photographers */
            )
        {

            _whenLocalTime = whenLocalTime;
            _timezone = timezone;
 #if V2
            _dateEDXL = dateEDXL;
            _distributionID_EDXL = distributionID_EDXL;
            _senderID_EDXL = senderID_EDXL;
            _deviceName = deviceName;
            _userNameForDevice = userNameForDevice;
            _userNameForWebService = userNameForWebService;
            _orgName = orgName;
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
#endif
            _sent = sent;
            _patientID = patientID; // includes prefix
            _zone = zone;
            _gender = gender;
            _ageGroup = ageGroup;
            _firstName = firstName;
            _lastName = lastName;
            _picCount = picCount;
            _eventShortName = eventShortName;
            _eventName = eventName; // w/o suffix
            _eventType = eventType; // suffix
            _imageName = imageName;
            _imageEncoded = imageEncoded;
            _imageCaption = imageCaption;
            _imageWriteableBitmap = null;
            _comments = comments;
#if V2
            _whyNoPhotoReason = whyNoPhotoReason;
#endif
        }

        public void Clear()
        {
            _whenLocalTime = "";
            _timezone = "";
 #if V2
            _dateEDXL = "";
            _distributionID_EDXL = "";
            _senderID_EDXL = "";
            _deviceName = "";
            _userNameForDevice = "";
            _userNameForWebService = "";
            _orgName = "";
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
#endif
            _sent = "";
            _patientID = ""; // includes prefix
            _zone = "";
            _gender = "";
            _ageGroup = "";
            _firstName = "";
            _lastName = "";
            _picCount = "";
            _eventShortName = "";
            _eventName = ""; // w/o suffix
            _eventType = ""; // suffix
            _imageName = "";
            _imageEncoded = "";
            _imageCaption = "";
            _imageWriteableBitmap = null;
            _comments = "";
#if V2
            _whyNoPhotoReason = "";
#endif
        }

        public void CopyFrom(TP_PatientDataItem pdi)
        {
            WhenLocalTime = pdi.WhenLocalTime;
            Timezone = pdi.Timezone;
#if V2
            DateEDXL = pdi.DateEDXL;
            DistributionID_EDXL = pdi.DistributionID_EDXL;
            SenderID_EDXL = pdi.SenderID_EDXL;
            DeviceName = pdi.DeviceName;
            UserNameForDevice = pdi.UserNameForDevice;
            UserNameForWebService = pdi.UserNameForWebService;
            OrgName = pdi.OrgName;
        // These are used to fabricate distribution ID, but do we really need them separately?
        // private String _orgNPI = string.Empty;
        // ditto orgPhone, orgEmail
            SentCode = pdi.SentCode;
#else
            Sent = pdi.Sent;
#endif
            PatientID = pdi.PatientID;
            Zone = pdi.Zone;
            Gender = pdi.Gender;
            AgeGroup = pdi.AgeGroup;
            FirstName = pdi.FirstName;
            LastName = pdi.LastName;
            PicCount = pdi.PicCount;
            EventShortName = pdi.EventShortName;
            EventName = pdi.EventName; // w/o suffix
            EventType = pdi.EventType; // suffix
            ImageName = pdi.ImageName;
            ImageEncoded = pdi.ImageEncoded;
            ImageCaption = pdi.ImageCaption;
            ImageWriteableBitmap = pdi.ImageWriteableBitmap;
            Comments = pdi.Comments;
#if V2
            WhyNoPhotoReason = pdi.WhyNoPhotoReason;
#endif
        }


        // Map to search template:
        public string FormatUniqueID()
        {
            return String.Format("{0}", WhenLocalTime); // Maybe change further as this evolves
        }

        public string FormatTitle()
        {
            return String.Format("{0} {1}", FirstName, LastName);
        }

        public string FormatSubtitle()
        {
            string patientID = PatientID;
            string prefix = "";
            if (!String.IsNullOrEmpty(App.OrgPolicy.OrgPatientIdPrefixText))
                prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            if (!patientID.StartsWith(prefix))
                patientID = prefix + patientID; // Treatment may not be adequate

            return String.Format("Mass Casualty ID {0}", patientID); // Change further as this evolves
        }

        public string FormatImagePath()
        {
            if (String.IsNullOrEmpty(Zone)) // This can happen if we try to take a photo before specifying the zone
                return "";
            /* WAS:
                        string imageName = "Assets/Zone"; // Placeholder color swatches for now, based on zone
                        switch(Zone) {
                            case "BH Green":
                                imageName += "BHGreen"; break;
                            case "":
                                App.MyAssert(false); break;
                            default:
                                imageName += Zone; break;
                        }
             *             imageName += ".png";
             */

            string imageName = "Assets/NoPhotoBrown(C17819).png";
            return imageName;
        }

        public async Task<string> FormatImageEncoded()
        {
            if (!String.IsNullOrEmpty(ImageEncoded))
                return await Task.FromResult<string>(ImageEncoded); // usually we're here because we read the encoding during deserialization

            if (ImageWriteableBitmap == null)
            {
                //return await Task.FromResult<string>("");
                return await Task.FromResult<string>(FormatImagePath()); // if no image, decorate with color swatch
            }
            //base64image();
            return await GetImageAsBase64Encoded();
        }

        public async Task<string> GetImageAsBase64Encoded()
        {
            // Adapted from
            // http social.msdn.microsoft.com/Forums/wpapps/en-US/.../image-to-base64-encoding [Win Phone 7, 2010] and
            // <same site> /sharing-a-writeablebitmap and
            // www.charlespetzold.com/blog/2012/08/WritableBitmap-Pixel-Arrays-in-CSharp-and-CPlusPlus.html
            // example in WebcamPage.xaml.cs
            string results = "";
            try
            {
                //using (Stream stream = App.CurrentPatient.ImageWriteableBitmap.PixelBuffer.AsStream())
                using (Stream stream = ImageWriteableBitmap.PixelBuffer.AsStream()) //There are 4 bytes per pixel. 
                {
                    byte[] pixels = await ConvertStreamToByteArray(stream);

                    // Now render the object in a known format, e.g., jpeg:
                    //InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                    IRandomAccessStream ms = new InMemoryRandomAccessStream();
                    // If calling from a synchronous function, add AsTask().ConfigureAwait(false) to avoid hanging UI thread

                    // If image as optionally cropped is over 1.5Mp, then apply aggressive compression to get it to that size (approximately)
                    double quality = 1.0;  //Max quality, the default.  For jpeg, 1.0 - quality = compression ratio
                    UInt64 totalPixels = (ulong)ImageWriteableBitmap.PixelWidth * (ulong)ImageWriteableBitmap.PixelHeight;
                    if (totalPixels > 1500000L)
                        quality = Math.Round(((double)1500000.0 / (double)totalPixels), 2, MidpointRounding.AwayFromZero); // for debug purposes, round quality to 2 fractional digits
                    // For encoding options, wee msdn.microsoft.com/en-us/library/windows/apps/jj218354.aspx "How to use encoding options".
                    var encodingOptions = new BitmapPropertySet();
                    var qualityValue = new BitmapTypedValue(quality, Windows.Foundation.PropertyType.Single);
                    encodingOptions.Add("ImageQuality", qualityValue);

                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ms, encodingOptions); //.AsTask().ConfigureAwait(false);
                    /*
                                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, ms); */
                    encoder.SetPixelData(
                        BitmapPixelFormat.Bgra8, //Rgba8, //Bgra8,
                        BitmapAlphaMode.Straight, // .Ignore
                        (uint)ImageWriteableBitmap.PixelWidth,//(uint)App.CurrentPatient.ImageWriteableBitmap.PixelWidth,
                        (uint)ImageWriteableBitmap.PixelHeight,//(uint)App.CurrentPatient.ImageWriteableBitmap.PixelHeight,
                        96.0, 96.0, pixels);

                    await encoder.FlushAsync(); //.AsTask().ConfigureAwait(false);
                    byte[] jpgEncoded = await ConvertMemoryStreamToByteArray(ms);
                    results = Convert.ToBase64String(jpgEncoded);
                    await ms.FlushAsync(); // cleanup
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error when encoding image: " + ex.Message);
            }
            return results;
        }

        public static async Task<byte[]> ConvertStreamToByteArray(Stream s)
        {
            byte[] p = new byte[s.Length]; //new byte[4 * ImageWriteableBitmap.PixelWidth * ImageWriteableBitmap.PixelHeight];
            s.Seek(0, SeekOrigin.Begin);
            await s.ReadAsync(p, 0, p.Length); // copy from s to p
            return p;
        }

        public static async Task<byte[]> ConvertMemoryStreamToByteArray(IRandomAccessStream s)
        {
            // see stackoverflow.com/questions/14017900/conversion-between-bitmapimage-and-byte-array-in-windows8
            using (DataReader reader = new DataReader(s.GetInputStreamAt(0)))
            {
                await reader.LoadAsync((uint)s.Size);
                byte[] p = new byte[s.Size];
                reader.ReadBytes(p);
                return p;
            }
        }

        // Name to store image under when decoded
        public string FormatImageName()
        {
            string imageName = PatientID + " ";
            if(String.IsNullOrEmpty(Zone))
                imageName += "ZoneNotSetYet"; // we'll have to clean up after zone is set
            else
                imageName += Zone.Replace(" ", ""); // remove spaces to simplify any subsequent machine processing of image name
/* WAS:
            switch (Zone)
            {
                //case "BH Green":
                //    imageName += "BHGreen"; break;
                case "":
                    imageName += "ZoneNotSetYet"; break; // we'll have to clean up after zone is set
                default:
                    imageName += Zone; break;
            }
 */
            imageName += ".jpg";
            return imageName;
        }

        public string /*Color*/ FormatImageBorderColor()
        {
            return App.ZoneChoices.GetColorNameFromZoneName(Zone);
/* WAS:
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
            }
 */
        }

        public string FormatDescription()
        {
            string gender = Gender;
            if (gender == "Unknown")
                gender = "Gender?"; // shorter than "Unknown Gender"
            string ageGroup = AgeGroup;
            if (ageGroup == "Unknown Age Group")
                ageGroup = "Age Group?";
            return String.Format("{0}, {1}     {2}", gender, ageGroup, Zone);
            // Original: "Item Description: Pellentesque porta, mauris quis interdum vehicula, urna sapien ultrices velit, nec venenatis dui odio in augue. Cras posuere, enim a cursus convallis, neque turpis malesuada erat, ut adipiscing neque tortor ac erat.",
        }

        public string FormatContent() // Only shown close-up view
        {
            string gender = Gender;
            if (gender == "Unknown")
                gender += " Gender";
            //if (ageGroup == "Unknown Age Group")
            //    ageGroup = "Age Group?";
            DateTimeOffset dto = ParseWhenLocalTime(WhenLocalTime);
            return String.Format("{0}, {1}\nZone: {2}\n\n{3} {4}\n{5}\n\n{6}",
                gender, AgeGroup, Zone, dto.ToString(), Timezone, EventName, Comments);
            //dto.ToString uses the current culture's default format for short date and long time.
            //ToString can take other pattern parameters too
        }

        private DateTimeOffset ParseWhenLocalTime(string localtime)
        {
            DateTimeOffset dto = new DateTimeOffset();
            try
            {
                dto = DateTimeOffset.Parse(localtime);
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog("When trying to parse time:\n" + localtime + "\n" + e.Message);
                var t = dialog.ShowAsync(); // assign to t to suppress compiler warning
            };
            return dto;
        }

        public void AdjustBeforeWriteXML()
        {
            if (String.IsNullOrEmpty(ImageEncoded) || ImageEncoded.StartsWith("Assets/"))
            {
                ImageEncoded = ""; // remove Asset/...png from file version of pdi
                ImageName = "";
                PicCount = "0";
            }
            else
            {
                PicCount = "1"; // Only 1 image supported so far.
                ImageName = FormatImageName(); // based on pdi__.Zone
                // ImageEncoded has encoded string
            }
        }


        public void AdjustAfterReadXML()
        {
            if(String.IsNullOrEmpty(ImageEncoded))
            {
                ImageEncoded = FormatImagePath(); // stuff Asset/...png into memory version of pdi
                ImageName = "";
                PicCount = "0";
            }
            else if (ImageEncoded.StartsWith("Assets/"))
            {
                //Don't do this: ImageEncoded = "";
                ImageName = "";
                PicCount = "0";
            }
            else
            {
                PicCount = "1"; // Only 1 image supported so far.
                ImageName = FormatImageName(); // based on pdi__.Zone
                // ImageEncoded has encoded string
            }
        }

#if MAYBE_NOT
// see functions in TP_OrgPolicySource.cs
        /// <summary>
        /// Inspects and if necessary adjusts the PatientID field to conform to the current patient ID format
        /// </summary>
        /// <param name="problem">can be shown to user.  Can also be parsed for additional error recovery.</param>
        /// <returns>false to suppress send of record as is</returns>
        public bool ConformPatientID(out string problem)
        {
            // Adjusts the Patient ID to the policy form given
            // Compare: Win 7 TriagePic's FormTriagePic.PatientNumberTextBox_Changed and FormFreshInstallWiz4's StartupPatientNumberTextBox_TextChanged
            problem = "";
            string s = this.PatientID; // includes prefix (unlike Win 7 version)
            string prefix = App.OrgPolicy.OrgPatientIdPrefixText; // for convenience
            int digits = App.OrgPolicy.OrgPatientIdFixedDigits; // likewise
            if (!s.Contains(prefix))
            {
                problem = "ID field must contain prefix " + prefix;
                return false;
            }
            s = s.Remove(0, prefix.Length); // work with just digits next
            if(digits == -1)
            {
                // variable length string:
                if(String.IsNullOrEmpty(s))
                    return false; // Momentarily don't compalin about an empty string during editing

                if(s.Length > 9) // Limited to aspecific # of digits; 9 works for an unit32, with range from 0 to 4294967295
                {
                    problem = "ID field must have 9 digits or less.";
                    // to do (by caller?):  Prevent out of control rollup or down
                    // patientID_MouseDownCount = 0;
                    // timerPatientUpDown.Enabled = false;
                    return false;
                }
                int i = s.IndexOf(' ');
                if (i >= 0)
                {
                    // Ideally we should never get here during editing, because of keypress handler screening by caller
                    problem = "ID field must have only digits.";
                    return false;
                }
                if (s.Length > 0 && s[0] == '0')
                {
                    problem = "ID field must not have leading zeroes.";
                    return false;
                }
                try
                {
                    Convert.ToUInt32(s);
                }
                catch (FormatException)
                {
                    // Here if it has non-numeric char
                    // Ideally we should never get here during editing, because of keypress handler screening by caller
                    problem = "ID field must have only digits.";
                    return false;
                }
                return true;
            }
            else
            {
                // Fixed length string with leading zeroes
                App.MyAssert(digits > 2 && digits < 10);
                string sAfter = s;
                while (sAfter.Length > digits)
                {
                    if(sAfter[0] == '0')
                        sAfter = sAfter.Substring(1); // drop leading '0' char
                    else
                    {
                        // Win 7: sAfter = App.PatientNumberPrevious; break;
                        // instead, complain
                        problem = "ID field exceeds allowed length of " + App.OrgPolicy.OrgPatientIdFixedDigits + ".";
                        return false;
                    }
                    this.PatientID = sAfter;
                    return true;
                }
                // Pad with leading zeroes if too short:
                while (sAfter.Length < digits)
                    sAfter = "0" + sAfter;
                this.PatientID = sAfter;
                return true;
            }
        }
#endif


#if V2
        /// <summary>
        /// Generates EDXL-DE wrapper around text and encoded-photo payloads.
        /// </summary>
        /// <param name="picContent"></param>
        /// <param name="edxl_payload">Main XML subtree with text content</param>
        /// <param name="distrRefs">Null if no previous message to refer to, or else separated list of DistributionIDs (making this an "Update")</param>
        /// <returns></returns>
        public string FormatEDXL_and_Payload(string picContent, string distrRefs)
        {
            // Adapted from Win 7's ReportPersonFormatting.cs FormatEDXL_and_Payload.
            // However, only supports LPF formatting, not PTL or TEP
            char cLF = (char)0x0A;
            string LF = cLF.ToString();
/*
            string gender = "";
            if (patientReport.genderMaleChecked && !patientReport.genderFemaleChecked)
                gender = "M";
            else if (!patientReport.genderMaleChecked && patientReport.genderFemaleChecked)
                gender = "F";
            else if (!patientReport.genderMaleChecked && !patientReport.genderFemaleChecked)
                gender = "U";
            else // both boxes checked
                gender = "C"; // complex.
 */
            App.MyAssert(_gender == "M" || _gender == "F" || _gender == "C" || _gender == "U");

 /*
            string peds = null;
            if (patientReport.pedsChecked)
                peds = "Y";
            else
                peds = "N"; */
            App.MyAssert(this._ageGroup == "Y" || this._ageGroup == "N");

            string distribStatus = "Exercise";  // Assume named events are generally exercises.  This is default.
            /*
            Debug.Assert(!String.IsNullOrEmpty(patientReport.eventShortName));
            Debug.Assert(!String.IsNullOrEmpty(patientReport.eventSuffix));
            Debug.Assert(!String.IsNullOrEmpty(patientReport.eventNameWithSuffix)); */
            App.MyAssert(!String.IsNullOrEmpty(_eventShortName));
            App.MyAssert(!String.IsNullOrEmpty(_eventType));
            App.MyAssert(!String.IsNullOrEmpty(_eventName));  // No longer supporting unnamed events
            switch (_eventType)//(patientReport.eventSuffix)
            {
                case "TEST/DEMO/DRILL":
                    //if (String.IsNullOrEmpty(patientReport.eventName) || patientReport.eventName.Contains("Test") || patientReport.eventName.Contains("test"))
                    if(_eventName.Contains("Test") || _eventName.Contains("test"))
                        distribStatus = "Test";
                    break;
                case "REAL - NOT A DRILL":
                    distribStatus = "Actual";
                    break;
                default:
                    break;
            }

            /* patientReport */ _dateEDXL = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); // UTC date.  Trailing "Z" is std abbrev for "UTC".  Embedded "T" indicates start of time
            // v 1.21 fixed bug in hour format, which needs to be 24-hr clock if not saying AM or PM, not 12-hr.

            // These 2 moved here in v 1.49 from caller; distributionID depends on date.EDXL:
            /* patientReport.*/ _distributionID_EDXL = GetDistributionID();
            /* patientReport.*/ _senderID_EDXL = GetSenderID();

            /* For REF:
            <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:cap="urn:oasis:names:tc:emergency:cap:1.1" attributeFormDefault="unqualified" elementFormDefault="qualified" targetNamespace="urn:oasis:names:tc:emergency:cap:1.1" xmlns:xs="urn:oasis:names:tc:emergency:cap:1.1http://www.w3.org/2001/XMLSchema">
            <element name = "category" maxOccurs = "unbounded">
                            <simpleType>
                              <restriction base = "string">
                                <enumeration value = "Geo"/>
                                <enumeration value = "Met"/>
                                <enumeration value = "Safety"/>
                                <enumeration value = "Security"/>
                                <enumeration value = "Rescue"/>
                                <enumeration value = "Fire"/>
                                <enumeration value = "Health"/>
                                <enumeration value = "Env"/>
                                <enumeration value = "Transport"/>
                                <enumeration value = "Infra"/>
                                <enumeration value = "CBRNE"/>
                                <enumeration value = "Other"/>
                              </restriction>
                            </simpleType>
                          </element>
            (1) Code Values:
“Geo” - Geophysical (inc. landslide)
“Met” - Meteorological (inc. flood)
“Safety” - General emergency and public safety
“Security” - Law enforcement, military, homeland and local/private security
“Rescue” - Rescue and recovery
“Fire” - Fire suppression and rescue
“Health” - Medical and public health
“Env” - Pollution and other environmental
“Transport” - Public and private transportation
“Infra” - Utility, telecommunication, other non-transport infrastructure
“CBRNE” – Chemical, Biological, Radiological, Nuclear or High-Yield Explosive threat or attack
“Other” - Other events
(2) Multiple instances allowed

            */
            /*
            Debug.Assert(!String.IsNullOrEmpty(patientReport.distributionID_EDXL));  // Because GetDistributionID was called already.
            Debug.Assert(!String.IsNullOrEmpty(patientReport.senderID_EDXL));  // Because GetSenderID was called already.
            Debug.Assert(!String.IsNullOrEmpty(patientReport.dateEDXL)); */
            string results;
            // Begin EDXL wrapper
            results = "<EDXLDistribution xmlns=\"urn:oasis:names:tc:emergency:EDXL:DE:1.0\">" + LF +
            "<distributionID>NPI " + MakeValueSafeForXML(/*patientReport.*/_distributionID_EDXL) + "</distributionID>" + LF + // This can be any unique ID.  We'll make one up.
            "<senderID>" + MakeValueSafeForXML(/*patientReport.*/_senderID_EDXL) + "</senderID>" + LF + // must be of form actor@domainname
            "<dateTimeSent>" + /*patientReport.*/_dateEDXL + "</dateTimeSent>" + LF + // TO DO format: 2007-02-15T16:53:00-05:00
            "<distributionStatus>" + distribStatus + "</distributionStatus>" + LF;
            if(String.IsNullOrEmpty(distrRefs))
                results += "<distributionType>Report</distributionType>" + LF;
            else
                results += "<distributionType>Update</distributionType>" + LF;
                // Below value is default.  Alternatives to default not easily known.  Maybe use cap 1.1's <scope> of "Public", "Restricted", "Private"
                // latter could use explicitAddress blocks analogously to CAP 1.1's <addresses>
                // EDXL Spec says "combinded...", but probably a typo, spec examples say "combined..."
                // Spec example also contains "Unclassified" as value
            results += "<combinedConfidentiality>UNCLASSIFIED AND NOT SENSITIVE</combinedConfidentiality>" + LF + // [sic]
                // could have <senderRole> and/or recipientRole here, each with valueListUrn and value.
                // EDXL doc says examples of things <keyword> might be used to describe include event type, event cause, incident ID, and response type
                // Glenn says: for now, use CAP 1.1 as enumeration here:
            "<keyword>" + LF +
            "  <valueListUrn>urn:oasis:names:tc:emergency:cap:1.1</valueListUrn>" + LF +
            "  <value>Health</value>" + LF +
            "  <value>Rescue</value>" + LF + // Multiple values from same list OK
            "</keyword>" + LF;
                // Unclear if adding other keywords from list is good or bad idea.
                // Multiple keyword lists would be OK
                // more valueListUrn examples: urn:sandia:gov:sensors:keywords, http://www.niem.gov/EventTypeList
            if (!String.IsNullOrEmpty(distrRefs))
                results += "<distributionReference>" + distrRefs + "</distributionReference>" + LF;

            // Use of "e-mail" and "DMIS COGs" are from EDXL spec example, but no real method set at time of spec.
                //"<explicitAddress>" + LF +
                //  "<explicitAddressScheme>e-mail</explicitAddressScheme>" + LF +
                //  "<explicitAddressValue>dellis@sandia.gov</explicitAddressValue>" + LF +
                //"</explicitAddress>" + LF +
                //"<explicitAddress>" + LF +
                //  "<explicitAddressScheme>DMIS COGs</explicitAddressScheme>" + LF +
                //  "<explicitAddressValue>1734</explicitAddressValue>" + LF +
                //"</explicitAddress>" + LF +

                // Ampersand in endpoint string in app.config represented as "&amp;", but it was changed to "&&" in
                // parent.endPointURI_LinkLabel.Text to view correctly.  Reverse that:

            //"<explicitAddress>" + LF +
            //"  <scheme>PL web service</scheme>" + LF +
            //"  <value>" + parent.endPointURI_LinkLabel.Text.Replace("&&", "&amp;") + "</value>" + LF +
                // e.g., "  <value>https://pl.nlm.nih.gov/?wsdl&amp;api=1.9.2</value>" + LF +
            //"</explicitAddress>" + LF;
            //results += FormatExplicitAddresses(txtTo, "e-mail");
            //results += FormatExplicitAddresses(txtCc, "e-mail-cc");
            //results += FormatExplicitAddresses(txtBcc, "e-mail-bcc");

#if TARGET_AREA
                // This was a mockup used in early TriagePic, had no functional effect.  This functionality might re-appear but more likely in PL.
            "<targetArea>" + LF;
            // We are using the approximate center of the NIH campus, near buildings 12 & 13 and midway between Suburban Hospital & WRNMMC, as the
            // logical center of the BHEPP partnership: = 38.9992, -77.1024
            // (Actual - Suburban 38.9974, -77.1107
            // WRNMMC 39.0016, -77.0920
            // NIH CC, Patient transfer entrance 39.0022, -77.1056
            // Lister Hill 38.9937, -77.0990
            switch (parent.eventRange)
            {
                default: case 1:
                    results +=
            "  <circle>38.9992,-77.1024, 40.2</circle>" + LF; break; // latitude, longitude of Bethesda, 25 mile (40.2 km) radius
                case 2:
                    results +=
            "  <circle>38.9992,-77.1024, 80.5</circle>" + LF; break; // latitude, longitude of Bethesda, 50 mile (80.5 km) radius
                case 3:
                    results +=
            "  <circle>38.9992,-77.1024, 161</circle>" + LF; break; // latitude, longitude of Bethesda, 100 mile (161 km) radius
                case 4:
                    results +=
            "  <subdivision>US-MD</subdivision>" + LF + //ISO 3166-2 designation
            "  <subdivision>US-DC</subdivision>" + LF +
            "  <subdivision>US-VA</subdivision>" + LF +
            "  <subdivision>US-WV</subdivision>" + LF +
            "  <subdivision>US-DE</subdivision>" + LF +
            "  <subdivision>US-PA</subdivision>" + LF +
            "  <subdivision>US-NJ</subdivision>" + LF; break;
            }
            results +=
            "</targetArea>" + LF +
#endif
            // Transition from EDXL header to XML payload (PTL, LPF, or TEP content)
            string formatAbbreviation  = "LPF";
            
            //girish : moved <incidentID> outside content object
            //results += "<incidentID>" + GetIncidentID() + "</incidentID>" + LF;
            results += "<incidentID></incidentID>" + LF;

            results +=
            "<contentObject>" + LF +
            "  <contentDescription>" + formatAbbreviation +
                    " notification - disaster victim arrives at hospital triage station</contentDescription>" + LF +
            "  <xmlContent>" + LF; // Further nesting with <embeddedXMLContent> dropped as unnecessary Aug 2011, LPF 1.3

            /*Debug.Assert(!String.IsNullOrEmpty(patientReport.userName));*/
#if V2
            App.MyAssert(!String.IsNullOrEmpty(_userNameForDevice));
#else
            App.MyAssert(!String.IsNullOrEmpty(_userName);
#endif
            
            results += generateLPF(gender, peds);

            results +=  // </embeddedXMLContent> nesting dropped as unnecessary Aug 2011, LPF 1.3
            "  </xmlContent>" + LF +
            "</contentObject>" + LF +
            picContent + // May be empty if pics are sent as email attachments, not embedded.
            // NO LF here
            "</EDXLDistribution>" + LF;

            return results;
       }


       protected string mergeComments()
       {
            // Adapted from Win 7 ReportPersonFormatting mergeComments
           // In Win 7 code, the 'patientReport.' object was used as an explicit prefix.
           char cLF = (char)0x0A;
           string LF = cLF.ToString();
           string comments;

           // Lines within "comments" don't have leading spaces, so recipient can get content without stripping leading spaces.
           // This is at the expense of XML pretty-printing
           comments = "Pictures: " + _nPicCount.ToString();
           if (_nPicCount == 0 && !String.IsNullOrEmpty(_whyNoPhotoReason))
               comments += LF + "Why No Photo: " + _whyNoPhotoReason; // reasons are canned so known to be safe for XML
           if (!String.IsNullOrEmpty(patientReport.comments))
               comments += LF + "Comments: " + MakeValueSafeForXML(_comments);
           return comments;
       }

       protected string generateLPF(string gender, string peds)
       {
            // Adapted from Win 7 ReportPersonFormatting generateLPF(...)
            char cLF = (char)0x0A;
            string LF = cLF.ToString();
            string results;

            results =
            "    <lpfContent>" + LF +
            "      <version>1.3</version>" + LF + // LPF versioning was begun at 1.2, from where PTL left off
            "      <login>" + LF +
            "        <userName>" + MakeValueSafeForXML(/*patientReport.*/_userName) + "</userName>" + LF + // or we could fabricate hospitalAuthor as in FormatPFIF
            "        <machineName>" + /*patientReport.*/_machineName + "</machineName>" + LF + // finally added v 1.60
            "      </login>" + LF +
            "      <person>" + LF +
            "        <personId>" + MakeValueSafeForXML(/*patientReport.*/_patientID /*_PrefixAndNumber*/) + "</personId>" + LF +
            "        <eventName>" + /*patientReport.*/_eventShortName + "</eventName>" + LF + // redefined as short name v 1.20
            "        <eventLongName>" + MakeValueSafeForXML(/*patientReport.eventNameWithSuffix*/ _eventName + " - " +  _eventType) + "</eventLongName>" + LF +
            "        <organization>" + LF + // organization assigning personID, i.e., hospital
            "          <orgName>" + MakeValueSafeForXML(/*patientReport.*/_orgName) + "</orgName>" + LF +
            "          <orgId>" + MakeValueSafeForXML(/*patientReport.*/_orgNPI) + "</orgId>" + LF +
            "        </organization>" + LF +
            "        <lastName>" + MakeValueSafeForXML(this._lastName) /*patientReport.lastNameSafeForXML*/ + "</lastName>" + LF +
            "        <firstName>" + MakeValueSafeForXML(this._firstName) /*patientReport.firstNameSafeForXML*/ + "</firstName>" + LF +
            "        <gender>" + gender + "</gender>" + LF +
            "        <genderEnum>M, F, U, C</genderEnum>" + LF +
            "        <genderEnumDesc>Male; Female; Unknown; Complex(M/F)</genderEnumDesc>" + LF +
            "        <peds>" + peds + "</peds>" + LF +  // PTL format doesn't support Peds, LPF does.
            "        <pedsEnum>Y,N</pedsEnum>" + LF +
            "        <pedsEnumDesc>Pediatric patient? Yes, No</pedsEnumDesc>" + LF +
            "        <triageCategory>" + /*patientReport.zone*/ this._zone + "</triageCategory>" + LF +
            "        <triageCategoryEnum>Green, BH Green, Yellow, Red, Gray, Black</triageCategoryEnum>" + LF +   //// TO DO <<<
            "        <triageCategoryEnumDesc>Treat eventually if needed; Treat for behavioral health; Treat soon; Treat immediately; Cannot be saved; Deceased</triageCategoryEnumDesc>" + LF +
            "        <comments>" + mergeComments() + "</comments>" + LF +
            // Not shown: Patient's home address, personal phone numbers
            // Not shown: Location to which patient is released, transferred, or sent to when deceased
            // Begin transition back to EDXL wrapper:
            "      </person>" + LF +
            "    </lpfContent>" + LF;
            return results;
       }
#endif

    }
}
#endif