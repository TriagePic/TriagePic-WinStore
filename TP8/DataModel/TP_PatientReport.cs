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
using Windows.UI;
using System.Globalization;
//using Windows.Networking.Connectivity;
//using Windows.System.UserProfile;
using Windows.Security.Cryptography.Core;
using Windows.Security.Cryptography;
using LPF_SOAP; // needed for WriteableBitmap.PixelBuffer.ToStream()

namespace TP8.Data
{
    /// <summary>
    /// Underlying TriagePic item data model.  Compare with TriagePic Win 7 PatientReportData and Outbox
    /// </summary>
    // This .Net class is no longer available in Win 8[Serializable]
    public class TP_PatientReport
    {
        // Serializer will ignore private fields without having to be told.
        private String _whenLocalTime = string.Empty;
        private String _timezone = string.Empty;
        private String _whenLocalTimeMsg1 = string.Empty; // new Release 6 (v 3.6) Aug 2015
        private String _timezoneMsg1 = string.Empty; // new Release 6 (v 3.6) Aug 2015
        private String _dateEDXL = string.Empty;
        private String _distributionID_EDXL = string.Empty;
        private String _senderID_EDXL = string.Empty;
        private String _deviceName = string.Empty;
        private String _userNameForDevice = string.Empty;
        private String _userNameForWebService = string.Empty;
        private String _orgName = string.Empty;
        private String _orgID = string.Empty; // In US, typically NPI
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
//        private String _sentCode = string.Empty; // Sent code
        private SentCodeObj _objSentCode = new SentCodeObj(""); // excluded from xml file.  Jan 2015: changed constructor call from () to (""), so resume after termination doesn't assert on null sentcode
        private bool _superceded = false;
        private String _patientID = string.Empty; // includes prefix.  If we implement practice mode, may begin with "Prac"
        private String _zone = string.Empty;
        private String _gender = string.Empty;
        private String _ageGroup = string.Empty; // "Y" = pediatics ("peds") or youth or 0-17, "N" = adult or 18+
        private String _firstName = string.Empty;
        private String _lastName = string.Empty;
        private Int32 _nPicCount = 0;
        private String _eventShortName = string.Empty; // event short name at PL
        private String _eventName = string.Empty; // w/o suffix
        private String _eventType = string.Empty; // suffix
        private String _imageName = string.Empty; // name when decoded, w/o path
        private String _imageEncoded = string.Empty;
        private String _imageCaption = string.Empty;
        private bool _deleted = false; // Added Dec 2014.  Not stored in XML.
        private String _comments = string.Empty;
        // TO DO: station staff
        private String _whyNoPhotoReason = string.Empty;
        private String _fullNameEDXL_and_LP2 = string.Empty; // temp file for debug
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

        public String WhenLocalTimeMsg1
        {
            get { return this._whenLocalTimeMsg1; }
            set { this._whenLocalTimeMsg1 = value; }
        }
        //public String dateEDXL,
        public String TimezoneMsg1
        {
            get { return this._timezoneMsg1; }
            set { this._timezoneMsg1 = value; }
        }

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
            get { return this._userNameForDevice; }
            set { this._userNameForDevice = value; }
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
        public String OrgID
        {
            get { return this._orgID; }
            set { this._orgID = value; }
        }
        // These are used to fabricate orgID, distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail

        public String SentCode
        {
            get { return _objSentCode.ToString(); /*this._sentCode;*/ }
            set { /*this._sentCode = value;*/ _objSentCode.SetFromString(value);  }
        }
        [XmlIgnore]
        public SentCodeObj ObjSentCode
        {
            get { return this._objSentCode; }
            set { this._objSentCode = value; /*_sentCode = value.ToString();*/ }
        }
        public bool Superceded
        {
            get { return this._superceded; }
            set { this._superceded = value; }
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
        public Int32 nPicCount
        {
            get { return this._nPicCount; }
            set { this._nPicCount = value; }
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
        
        [XmlIgnore]
        public bool Deleted
        {
            get { return this._deleted; }
            set { this._deleted = value;  }
        }

        public String Comments
        {
            get { return this._comments; }
            set { this._comments = value; }
        }

        // TO DO: station staff
        public String WhyNoPhotoReason
        {
            get { return this._whyNoPhotoReason; }
            set { this._whyNoPhotoReason = value; }
        }
        public String FullNameEDXL_and_LP2 // temp file for debug
        {
            get { return this._fullNameEDXL_and_LP2; }
            set { this._fullNameEDXL_and_LP2 = value; }
        }

        public TP_PatientReport() { } // Parameterless constructor required for serializer

        public TP_PatientReport(TP_PatientReport pr)
        {
            CopyFrom(pr);
        }

        /* DUMMY DATA GENERATION PROBABLY NO LONGER NEEDED. SETASIDE Aug 2015
                // Constructor used for dummy generated data.  Additional fields filled in later.
                public TP_PatientReport(
                    String whenLocalTime,
                    String timezone,
                    String whenLocalTimeMsg1,
                    String timezoneMsg1,
                    String dateEDXL,
                    String patientID, // includes prefix
                    String zone,
                    String gender,
                    String ageGroup,
                    String firstName,
                    String lastName,
                    String eventShortName,
                    String eventName, // w/o suffix
                    String eventType, // suffix
                    String comments
                    )
                {
                    _whenLocalTime = whenLocalTime;
                    _timezone = timezone;
                    _whenLocalTimeMsg1 = whenLocalTimeMsg1;
                    _timezoneMsg1 = timezoneMsg1;
                    _dateEDXL = dateEDXL;
                    _patientID = patientID; // includes prefix
                    _zone = zone;
                    _gender = gender;
                    _ageGroup = ageGroup;
                    _firstName = firstName;
                    _lastName = lastName;
                    _eventShortName = eventShortName;
                    _eventName = eventName; // w/o suffix
                    _eventType = eventType; // suffix
                    _comments = comments;
                }

                public void CompleteGeneratedRecord()
                {
                    // Dummy data, mostly unchanging and hard-coded
                    SentCode = "Y"; // also fills in OrgSentCode
                    Superceded = false;
                    OrgID = "1234567890";
                    DistributionID_EDXL = OrgID + " " + DateEDXL;
                    SenderID_EDXL = "disaster@nlm.nih.gov";
                    DeviceName = App.DeviceName;
                    UserNameForDevice = App.UserWin8Account; //"myMSLiveAccount";
                    UserNameForWebService = App.pd.plUserName; //"hs";
                    OrgName = "NLM (testing)";
                    nPicCount = 0;
                    ImageName ="";
                    ImageEncoded = "";
                    ImageCaption = "";
                    ImageWriteableBitmap = null;
                    WhyNoPhotoReason = "";
                    FullNameEDXL_and_LP2 = "";
                }
        */

        public TP_PatientReport(
            String whenLocalTime,
            String timezone,
            String whenLocalTimeMsg1, // added Release 6
            String timezoneMsg1, // added Release 6
            String dateEDXL,
            String distributionID_EDXL,
            String senderID_EDXL,
            String deviceName,
            String userNameForDevice,
            String userNameForWebService,
            String orgName,
            String orgID,
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
            String sentCode,
            bool superceded,
            String patientID, // includes prefix
            String zone,
            String gender,
            String ageGroup,
            String firstName,
            String lastName,
            Int32 nPicCount,
            String eventShortName,
            String eventName, // w/o suffix
            String eventType, // suffix
            String imageName,
            String imageEncoded,
            String imageCaption,
            String comments,
            String whyNoPhotoReason,
            String fullNameEDXL_and_LP2
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
            _whenLocalTimeMsg1 = whenLocalTimeMsg1;
            _timezoneMsg1 = timezoneMsg1;
            _dateEDXL = dateEDXL;
            _distributionID_EDXL = distributionID_EDXL;
            _senderID_EDXL = senderID_EDXL;
            _deviceName = deviceName;
            _userNameForDevice = userNameForDevice;
            _userNameForWebService = userNameForWebService;
            _orgName = orgName;
            _orgID = OrgID;
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
            SentCode = sentCode; // fills in _sentCode, _orgSentCode
            Superceded = superceded;
            _patientID = patientID; // includes prefix
            _zone = zone;
            _gender = gender;
            _ageGroup = ageGroup;
            _firstName = firstName;
            _lastName = lastName;
            _nPicCount = nPicCount;
            _eventShortName = eventShortName;
            _eventName = eventName; // w/o suffix
            _eventType = eventType; // suffix
            _imageName = imageName;
            _imageEncoded = imageEncoded;
            _imageCaption = imageCaption;
            _imageWriteableBitmap = null;
            _comments = comments;
            _whyNoPhotoReason = whyNoPhotoReason;
            _fullNameEDXL_and_LP2 = fullNameEDXL_and_LP2;
        }

        public void Clear()
        {
            _whenLocalTime = _whenLocalTimeMsg1 = "";
            _timezone = _timezoneMsg1 = "";
            _dateEDXL = "";
            _distributionID_EDXL = "";
            _senderID_EDXL = "";
            _deviceName = "";
            _userNameForDevice = "";
            _userNameForWebService = "";
            _orgName = "";
            _orgID = "";
        // These are used to fabricate distribution ID, but do we really need them separately?
        //private String _orgNPI = string.Empty;
        //ditto orgPhone, orgEmail
            SentCode = ""; // fills in _sentCode, _orgSentCode
            Superceded = false;
            _patientID = ""; // includes prefix
            _zone = "";
            _gender = "";
            _ageGroup = "";
            _firstName = "";
            _lastName = "";
            _nPicCount = 0;
            _eventShortName = "";
            _eventName = ""; // w/o suffix
            _eventType = ""; // suffix
            _imageName = "";
            _imageEncoded = "";
            _imageCaption = "";
            _imageWriteableBitmap = null;
            _comments = "";
            _whyNoPhotoReason = "";
            _fullNameEDXL_and_LP2 = "";
        }

        public void CopyFrom(TP_PatientReport pdi)
        {
            WhenLocalTime = pdi.WhenLocalTime;
            Timezone = pdi.Timezone;
            WhenLocalTimeMsg1 = pdi.WhenLocalTimeMsg1;
            TimezoneMsg1 = pdi.TimezoneMsg1;
            DateEDXL = pdi.DateEDXL;
            DistributionID_EDXL = pdi.DistributionID_EDXL;
            SenderID_EDXL = pdi.SenderID_EDXL;
            DeviceName = pdi.DeviceName;
            UserNameForDevice = pdi.UserNameForDevice;
            UserNameForWebService = pdi.UserNameForWebService;
            OrgName = pdi.OrgName;
            OrgID = pdi.OrgID;
        // These are used to fabricate distribution ID, but do we really need them separately?
        // private String _orgNPI = string.Empty;
        // ditto orgPhone, orgEmail
            SentCode = pdi.SentCode; // also fills in OrgSentCode
            Superceded = pdi.Superceded;
            PatientID = pdi.PatientID;
            Zone = pdi.Zone;
            Gender = pdi.Gender;
            AgeGroup = pdi.AgeGroup;
            FirstName = pdi.FirstName;
            LastName = pdi.LastName;
            nPicCount = pdi.nPicCount;
            EventShortName = pdi.EventShortName;
            EventName = pdi.EventName; // w/o suffix
            EventType = pdi.EventType; // suffix
            ImageName = pdi.ImageName;
            ImageEncoded = pdi.ImageEncoded;
            ImageCaption = pdi.ImageCaption;
            ImageWriteableBitmap = pdi.ImageWriteableBitmap;
            Comments = pdi.Comments;
            WhyNoPhotoReason = pdi.WhyNoPhotoReason;
            FullNameEDXL_and_LP2 = pdi.FullNameEDXL_and_LP2;
        }


        public async Task DoSendEnqueue(bool newPatientReportObject)
        {
            if (ObjSentCode.GetCodeWithoutVersion() == "QD")
                Deleted = true; // will affect generation of "content" below.  New Dec 2014

            if (!newPatientReportObject)
            {
                await App.PatientDataGroups.UpdateSendHistory(this.PatientID, this.ObjSentCode.GetVersionCount(), this.SentCode, true /*superceded*/);
            }
            await GetCurrentOrgAndDeviceData(); // and time
            if (newPatientReportObject)
            {
                // copy values just retrieved from GetCurrentOrgAndDeviceData:
                WhenLocalTimeMsg1 = WhenLocalTime;
                TimezoneMsg1 = Timezone;
            }
            else
            {
                // retain inherited values
                App.MyAssert(!String.IsNullOrEmpty(WhenLocalTimeMsg1));
                // Hmm, maybe TT can't support this yet: App.MyAssert(!String.IsNullOrEmpty(TimezoneMsg1));
            }
            if (String.IsNullOrEmpty(ImageEncoded) || ImageEncoded.StartsWith("Assets/"))
            {
                ImageEncoded = ""; // remove "Assets..."
                ImageName = "";
                nPicCount = 0;
            }
            else
            {
                // Leave pdi.ImageName & ImageEncoded alone, as set earlier
                nPicCount = 1; // Only support 1 image so far
            }
            string content = FormatEDXL_and_Payload(GetPictureInContentObjectEDXL(), ""); // TO DO: distribution refs
            // Since we're not supporting email sends, it's not necessary to stuff caption into image filename.
            FullNameEDXL_and_LP2 = PatientID + " " + Zone;
            App.MyAssert(this.ObjSentCode.IsValid());
            FullNameEDXL_and_LP2 += ObjSentCode.GetVersionSuffixFilenameForm() + ".lp2"; // version suffix may be "" or "Msg # n"
            await WriteReportPersonContentXML(FullNameEDXL_and_LP2, content); // persist content, for debugging... but not so good for HIPPA, so delete later.
            App.sendQueue.Add(this);
        }


        // This is called by GUI thread, but by sendqueue with dequeued pr object instance.
        public async Task DequeueAndProcessPatientDataRecord(bool firstSend, string contentEDXL_and_LP2)
        {
            string results = "";
            bool deleteNow = false;
            //SentCodeObj objSentCodeAsDequeued = new SentCodeObj(SentCode); // helpers
            //SentCodeObj objSentCodeAsRevised = new SentCodeObj(SentCode); // initially the same
            //SentCode = objSentCodeAsRevised.ReplaceCodeKeepSuffix("N");
            if (ObjSentCode.GetCodeWithoutVersion() == "QD")
                deleteNow = true;
            ObjSentCode.ReplaceCodeKeepSuffix("N");
            await App.PatientDataGroups.AddSendHistory(this); // This adds a REFERENCE to this report into the _outbox and makes a clone for _allstations
            // So any change to the record below automagically is reflected in _outbox, but needs to be copied to _allstations
            try
            {
                //for (int retries = 1; retries >= 0; retries-- ) // loop to allow retry for credentials
                //{
                if (firstSend)
                {
                    results = await App.service.ReportPerson(contentEDXL_and_LP2, null);
                    if (results == "") // therefore successful; ReportPerson has filled in App.CacheUuidOfMostRecentReportSent;
                        App.CacheIdOfMostRecentReportSent = PatientID;  // VERSION 3.7 ROLLBACK ROLLFORWARD
                }
                else
                {
                    string uuid = await App.service.GetUuidFromPatientID(PatientID, EventShortName);
                    if (String.IsNullOrEmpty(uuid))
                    {
                        // VERSION 3.7 ROLLBACK ROLLFORWARD
                        // Quick hack to handle case where user sends report, then immediately revises and resends it, before it is indexed at TriageTrak
                        if (PatientID == App.CacheIdOfMostRecentReportSent)
                            uuid = App.CacheUuidOfMostRecentReportSent;
                    }
                    if (String.IsNullOrEmpty(uuid))
                    {
                        string errMsg = "TriageTrak can't find a record with Patient ID " + PatientID + " associated with event " + EventName;
                        MessageDialog dlg = new MessageDialog(errMsg);
                        await dlg.ShowAsync();
                        await App.ErrorLog.ReportToErrorLog("On Prep for Resend - for event with short name " + EventShortName, errMsg, true);
                        // bail out:
                        await App.PatientDataGroups.UpdateSendHistoryAfterOutbox(PatientID, ObjSentCode); // see explanation of this below
                        return;
                    }
                    else
                        results = await App.service.ReportPerson(contentEDXL_and_LP2, uuid);
                }
            }
            catch (Exception ex)
            {
                results = "ERROR: When sending to web site\n" + ex.ToString();
            }
            if (results.StartsWith("ERROR: "))
            {
                string usrMsg = "On Send";
                if (!firstSend)
                    usrMsg = "On Resend";
                usrMsg += " - Patient report was not accepted by TriageTrak";
                //redundant, let ReportToErrorLog do it:
                //  MessageDialog dlg = new MessageDialog(usrMsg);
                //  await dlg.ShowAsync();

                await App.ErrorLog.ReportToErrorLog(results, usrMsg, true);
                // continue for now, but error recovery needs work
            }
            if (results == "")
            {
                //SentCode = objSentCodeAsRevised.ReplaceCodeKeepSuffix("Y");
                ObjSentCode.ReplaceCodeKeepSuffix("Y");
                // TO DO: delete lp2 file if not persisting for debug
            }
            if(deleteNow) // New Dec 2014          
            {
                // Compare ViewEditReportPage's DeleteLocal
                string pid = PatientID; // ClearEntryAll will clear these, so remember them for Discard
                int v = ObjSentCode.GetVersionCount();
                //ClearEntryAll();  // Will indirectly mark as altered
                //LastSentMsg.Text = "Discard from Outbox: Done.";
                for (int i = v; i > 0; i-- ) // loop added Dec 2014.  Delete all reports for this pid
                    App.PatientDataGroups.GetOutbox().Discard(pid, i); // Note that reports previously marked as "superceded" will not be in the Outbox list, and may still be present elsewhere in spite of this loop.
                await App.PatientDataGroups.ScrubOutbox(); // Discard itself doesn't seem to do it, leaves empty record behind. Await added v 3.5
                await App.PatientDataGroups.GetOutbox().WriteXML();
                App.PatientDataGroups.UpdateListsAfterReportDelete(true); // = DeletedAtTriageTrakToo
            }
            else
                // DON'T DO, BECAUSE WE DON'T NEED TO SEARCH THROUGH OUTBOX LIST:
                //   await App.PatientDataGroups.UpdateSendHistory(PatientID, ObjSentCode);
                // (First idea:  await App.PatientDataGroups.WriteSendHistory(true /*allStationsToo*/);  )
                // Actually, we do need to search through _allstations now, because we've made the pr object
                // separate from the pr object in _outbox (so keep contents sync'd with this call...)
                await App.PatientDataGroups.UpdateSendHistoryAfterOutbox(PatientID, ObjSentCode);
        }


        private async Task WriteReportPersonContentXML(string filename, string content)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await FileIO.AppendTextAsync(file, content);
        }

        public async Task<string> ReadReportPersonContentXML(string filename)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
            return await FileIO.ReadTextAsync(file);
        }

/* NOT NEEDED.  Instead use UpdateSendHistory call after dequeue
        public async Task DoSendPart2() // Update outbox and allstations in both memory and local storage.  As well as sorted/filtered version in memory
        {
            //App.PatientDataGroups.GetOutbox().Add(pdi);
            App.PatientDataGroups.GetOutbox().UpdateOrAdd(this);
            await App.PatientDataGroups.GetOutbox().WriteXML();
            // REVISIT NEXT 2 LINES WHEN WE HAVE ACTUAL WEB SERVICES:
            App.PatientDataGroups.GetAllStations().UpdateOrAdd(this);
            await App.PatientDataGroups.GetAllStations().WriteXML("PatientReportsAllStations.xml");
            App.PatientDataGroups.ReSortAndFilter();

            SampleDataSource.RefreshOutboxItems(); // For benefit of next peek at Outbox
        }
 */

        public async Task DoSendPart3(string updatedPatientNumber)
        {
            App.CurrentOtherSettings.CurrentNewPatientNumber = updatedPatientNumber;
            App.CurrentOtherSettingsList.UpdateOrAdd(App.CurrentOtherSettings);
            await App.CurrentOtherSettingsList.WriteXML();
            Clear(); // forget about this patient, start anew
            App.CurrentPatient.Clear(); // not a good idea to null pdi or CurrentPatient, since we're not reinstantiating it elsewhere.
            //await Task.Delay(5000);  // <<< DEBUG PROGRESS BAR
        }

        /// <summary>
        /// This is typically called when a new empty patient record is created, and
        /// again (in case of change to org choice or credentials) just as almost-completed record is queued for sending.
        /// </summary>
        public async Task GetCurrentOrgAndDeviceData()
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            WhenLocalTime = (DateTimeOffset.Now).ToString("yyyy-MM-dd HH:mm:ss K", provider);
            Timezone = GetTimeZoneAbbreviation();
            DateEDXL = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); // UTC date.  Trailng "Z" is std abbrev for "UTC".  Embedded "T" indicates start of time.
            OrgName = App.CurrentOrgContactInfo.OrgName;
            OrgID = App.CurrentOrgContactInfo.OrgNPI;
            EventShortName = App.CurrentDisaster.EventShortName;
            EventName = App.CurrentDisaster.EventName; // w/o suffix
            EventType = App.CurrentDisaster.EventType; // can be used to create suffix
            UserNameForDevice = App.UserWin8Account;
            UserNameForWebService = await App.pd.DecryptPL_Username();
            DeviceName = App.DeviceName;
            SenderID_EDXL = GetSenderID();
            DistributionID_EDXL = GetDistributionID();
        }

        /// <summary>
        /// Return an abbreviation for the current system timezone, reflecting whether daylight-savings and standard time.
        /// </summary>
        /// <returns></returns>
        public string GetTimeZoneAbbreviation()
        {
            // There are no international standards for timezone abbreviations.
            // The usual recommended hack (at least for US timezones) is to take the first letter of each word as done below.
            string tzname;
            // Note that Win8 does not support Win7's System.TimeZones.
            // However, let's try it with TimeZoneInfo instead.  MSDN says TimeZone.CurrentTimeZone in Win 7 = TimeZoneInfo.Local
            // (Not used here but also of interest:  The WinRTTimeZones code, available from NuGet, has a TimeZones objects that wraps Win32 time features supported in Win 8)
            // See also for other timezone issues: "Programming Windows, Version 6", Petzold, Chapter 15.
            if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                tzname = TimeZoneInfo.Local.DaylightName;
            else
                tzname = TimeZoneInfo.Local.StandardName;
            return AbbreviateTimeZone(tzname);
        }

        private string AbbreviateTimeZone(string tzname)
        {
            // Hack to convert time zone name to abbreviation by just taking the first letter of each word.
            // This will not work 100% for time zones worldwide.  Results are typically 3 letter (e.g., always if US) or 4 letter.
            // For a somewhat full list, see www.timeanddate.com/lbirary/abbreviations/timezones/
            string[] words = tzname.Split(" ".ToCharArray());
            string tzabbr = "";
            foreach (string word in words)
                tzabbr += word[0];
            return tzabbr;
        }

        // For more about timezone data see
        //https://en.wikipedia.org/wiki/Tz_database
        //http://web.cs.ucla.edu/~eggert/tz/tz-link.htm
        //http://home.kpn.nl/vanadovv/time/TZworld.html
        // 
        public string InferTimeZoneFromOffset(string offset) // offset is of form "+00:00" or "00:00" or "-04:00"
        {
            // This assumes that the current location of the device is in the same timezone as the reporting location of the device.
            // In other words, this is a hack because TT isn't retaining timezone info and we have to infer it instead.
            string tzDT = AbbreviateTimeZone(TimeZoneInfo.Local.DaylightName);
            string tzST = AbbreviateTimeZone(TimeZoneInfo.Local.StandardName);
            if (TimeZoneInfo.Local.BaseUtcOffset.ToString() == offset) // difference between UTC and locale standard time
                return tzST;

            TimeSpan ts;
            offset = offset.Replace("+",""); // TryParse doesn't want to see leading +
            if (!TimeSpan.TryParse(offset, out ts))
                return "";

            TimeSpan ts1 = new TimeSpan(1,0,0); // 1 hours, 0 minutes, 0 seconds
            ts = ts.Subtract(ts1); //ts.Add(ts1);
            if (TimeZoneInfo.Local.SupportsDaylightSavingTime && TimeZoneInfo.Local.BaseUtcOffset == ts)
                return tzDT;
            return "";
        }

        public string InferAndFormatTimeZone(string offset)
        {
            // Formatting for FormatContent
            string itz = InferTimeZoneFromOffset(offset);
            if (String.IsNullOrEmpty(itz))
                return offset;
            return offset + " " + itz + "*";
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
            // WAS BEFORE JULY 2015, BUT NOT GOOD ENUF
            //string prefix = "";
            //if (!String.IsNullOrEmpty(App.OrgPolicy.OrgPatientIdPrefixText))
            //    prefix = App.OrgPolicy.OrgPatientIdPrefixText;
            //if (!patientID.StartsWith(prefix))
            //    patientID = prefix + patientID; // Treatment may not be adequate

            // NEW verion 3.5: If prefix is from a different org than current one, or has the stupid AUTO preamble, don't mess with it:
            if (OrgName == App.CurrentOrgContactInfo.OrgName)
            {
                // We can do a little more fixup, since we know current org's prefix (and its non-empty)
                string prefix = App.OrgPolicy.OrgPatientIdPrefixText;
                if(
                    !String.IsNullOrEmpty(prefix) &&
                    !(patientID.StartsWith(prefix) || patientID.StartsWith("AUTO"+prefix)) &&
                     Char.IsDigit(patientID,0) // belt & suspenders here
                )
                patientID = prefix + patientID; // add missing prefix
            }

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

            string imageName = "Assets/NoPhotoBrown(C17819)(300x225).png";
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

    /* Not quite what we want, converts bitmap, not jpeg, to base64
        public async Task<string> GetImageAsBase64Encoded()
        {
            using (Stream stream = ImageWriteableBitmap.PixelBuffer.AsStream()) //There are 4 bytes per pixel. 
            {
                byte[] pixels = await ConvertStreamToByteArray(stream);
                return Convert.ToBase64String(pixels);
            }
        } */

/* Didn't work
        public string GetImageAsBase64Encoded()
        {
            byte[] pixels = ImageWriteableBitmap.PixelBuffer.ToArray();
            return Convert.ToBase64String(pixels);
        } */

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

        /// <summary>
        /// Await loading an image from an external web site
        /// </summary>
        /// <param name="uri"></param>
        public async Task<WriteableBitmap> LoadImageWriteableBitmapFromWeb(Uri uri)
        {
            // From Petzold, Programming Windows, 6th edition pg. 692-3
            /* This simple method will return before the file read is complete.  As expected, this is a problem:  
                RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(uri);
                IRandomAccessStreamWithContentType fileStream = await streamRef.OpenReadAsync();
                WriteableBitmap wb = new WriteableBitmap(1, 1); // dummy values
                wb.SetSource(fileStream);
                return wb; */
            try
            {
                RandomAccessStreamReference streamRef = RandomAccessStreamReference.CreateFromUri(uri);

                // Create a buffer for reading the stream:
                Windows.Storage.Streams.Buffer buffer = null;
                // Read the entire file:
                using (IRandomAccessStreamWithContentType fileStream = await streamRef.OpenReadAsync())
                {
                    buffer = new Windows.Storage.Streams.Buffer((uint)fileStream.Size);
                    await fileStream.ReadAsync(buffer, (uint)fileStream.Size, InputStreamOptions.None);
                }
                WriteableBitmap wb = new WriteableBitmap(1, 1); // dummy values
                // Create a memory stream for transferring the data
                using (InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream())
                {
                    await memoryStream.WriteAsync(buffer);
                    memoryStream.Seek(0);
                    // Use the memory stream as the Bitmap Source
                    wb.SetSource(memoryStream);
                }
                return wb;
            }
            catch(Exception)
            {
                return null; // new Dec 2014.  Probably bad, dangling uri.  Let caller report & handle error.
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

            imageName += ".jpg";
            return imageName;
        }

        public string /*Color*/ FormatImageBorderColor()
        {
            return App.ZoneChoices.GetColorNameFromZoneName(Zone);
        }

        public string FormatDescription()
        {
            string gender = Gender;
            if (gender == "Unknown")
                gender = "Gender?"; // shorter than "Unknown Gender"
            string ageGroup = AgeGroup;
            if (ageGroup == "Unknown Age Group")
                ageGroup = "Age Group?";
            DateTimeOffset dto = ParseWhenLocalTime(WhenLocalTimeMsg1); // was before Release 6: (WhenLocalTime)
            string date = dto.ToString("f") + " " + TimezoneMsg1; // was before Release 6: (Timezone)
            string status = "";
            if (!ObjSentCode.IsDoneOK())
                status = " - Not Sent"; // shorter than StatusMessageFromSentCode
            if (ObjSentCode.GetVersionCount() > 1)
                status += " -" +ObjSentCode.GetVersionSuffixFilenameForm(); // latter has leading space
            return String.Format("{0}, {1}       {2}\n{3}{4}\n{5}",
                gender, AgeGroup, Zone, date, status, EventName);
            //return String.Format("{0}, {1}     {2}", gender, ageGroup, Zone);
         }

        public string FormatContent() // Only shown close-up view
        {
            string gender = Gender;
            if (gender == "Unknown")
                gender += " Gender";
            //if (ageGroup == "Unknown Age Group")
            //    ageGroup = "Age Group?";
            string dateArrived = "Arrived:\n"; // Optional in some sense, if about the same as dateUpdated
            string dateUpdated = ""; // Only add "Updated:\n" below if optional Arrived was added
            string updated = "";
            if(String.IsNullOrEmpty(WhenLocalTime))
                dateUpdated = "No date or time for send given.";
            else
            {
                if(!String.IsNullOrEmpty(WhenLocalTimeMsg1) && !AboutTheSameTime(WhenLocalTime, WhenLocalTimeMsg1)) // Could check timezones too, but this is complicated enough
                {
                    // This block new Release 6 (v 3.6) Aug 2015
                    dateArrived += FormatLocalAndUtcDates(WhenLocalTimeMsg1, TimezoneMsg1); // in local and UTC timezones
                    updated = "\nUpdated:\n"; // Only see this in next section if there's also Arrival info
                }
                dateUpdated = updated + FormatLocalAndUtcDates(WhenLocalTime, Timezone);
            }
            string status = "";
            if (ObjSentCode.GetVersionCount() > 1)
                status += " - " + ObjSentCode.GetVersionSuffixFilenameForm(); // Normal resend
            if (!ObjSentCode.IsDoneOK())
                status += " - Sent code: " + ObjSentCode.StatusMessageFromSentCode(false /*email only request*/); // Unusual
            string timezoneNote = "";
            if (dateArrived.Contains("*") || dateUpdated.Contains("*"))
                timezoneNote = "  * Timezone when reported is inferred.";
            return String.Format("{0}, {1}\n{2} Zone at {3}\n{4}{5}{6}\n{7}\nEvent: {8}\n{9}",
                gender, AgeGroup, Zone, OrgName, dateArrived, dateUpdated, status, timezoneNote, EventName, Comments); // dateArrived may be empty
            //dto.ToString uses the current culture's default format for short date and long time.
            //ToString can take other pattern parameters too
        }

        private string FormatLocalAndUtcDates(string WhenLocalTime_, string TimezoneMsg_) // New Release 6
        {
            // Ordinarily returns a 2-line result. Can be 1 line if problem.
            // If timezone inferred, it will have "*" after it.
            string date = "  ";
            DateTimeOffset dto = ParseWhenLocalTime(WhenLocalTime_);
            if ((dto.DateTime == DateTimeOffset.MinValue) && (dto.Offset == TimeSpan.Zero))
            {
                date += "Unreadable datetime of " + WhenLocalTime_ + " " + TimezoneMsg_; // latter may be empty
            }
            else
            {
                date += dto.DateTime.ToString() + " "; // Extract DateTime to drop offset (last 6 characters like " +00:00" or " -04:00")
                //elaboration of: date = "Arrived: " + dto1.ToString() + " " + TimezoneMsg_;
                if (!String.IsNullOrEmpty(TimezoneMsg_))
                    date += TimezoneMsg_; // We have an actual timezone in the data
                else
                {
                    // Try to infer a timezone instead
                    string offsetFormat = (dto.Offset < TimeSpan.Zero ?  "\\-" : "") + "hh\\:mm"; // Unclear whether we should add "\\+" here in positive case. Don't need seconds, timezone offsets don't use.
                    string offset = dto.Offset.ToString(offsetFormat);
                    string inferredTimeZoneAbbr = offset;
                    string itz = InferTimeZoneFromOffset(offset); // If inferred, will be of form "offset timezone*". Otherwise just offset
                    if (!String.IsNullOrEmpty(itz))
                        inferredTimeZoneAbbr += " " + itz + "*";
                    date += inferredTimeZoneAbbr; // If can't infer, back to original form of dto1.ToString()
                }
                date += "\n  " + dto.UtcDateTime.ToString() + " UTC";
            }
            return date;
        }

        private DateTimeOffset ParseWhenLocalTime(string localtime)
        {
            App.MyAssert(localtime != null);
            DateTimeOffset dto = new DateTimeOffset();
            bool errCaught = false; // can't bring up MessageDialog directly from within exception handler
            string excMsg = "";
            try
            {
                dto = DateTimeOffset.Parse(localtime);
            }
            catch (Exception e)
            {
                errCaught = true;
                excMsg = e.Message;
            };
            if (errCaught)
            {
                var dialog = new MessageDialog("When trying to parse time:\n" + localtime + "\n" + excMsg);
                var t = dialog.ShowAsync(); // assign to t to suppress compiler warning
                // dto will return with .DateTime == DateTimeOffset.MinValue and .Offset == TimeSpan.Zero
            }
            return dto;
        }

        private bool AboutTheSameTime(string LocalTime1, string LocalTime2) // New Release 6
        {
            int MaxSecondsApart = 60;
            // Could check timezones too, but this is complicated enough
            App.MyAssert(!String.IsNullOrEmpty(LocalTime1));
            App.MyAssert(!String.IsNullOrEmpty(LocalTime2));
            if (LocalTime1 == LocalTime2)
                return true;

            DateTimeOffset dto1 = ParseWhenLocalTime(LocalTime1);
            if ((dto1.DateTime == DateTimeOffset.MinValue) && (dto1.Offset == TimeSpan.Zero))
                return false; // unreadable datetime

            DateTimeOffset dto2 = ParseWhenLocalTime(LocalTime2);
            if ((dto2.DateTime == DateTimeOffset.MinValue) && (dto2.Offset == TimeSpan.Zero))
                return false; // unreadable datetime

            var seconds = (dto1.DateTime < dto2.DateTime)? (dto2.DateTime- dto1.DateTime).TotalSeconds: (dto1.DateTime - dto2.DateTime).TotalSeconds;
            App.MyAssert(seconds >= 0);
            if(seconds <= MaxSecondsApart)
                return true;
            return false;
        }

        public void AdjustBeforeWriteXML()
        {
            if (String.IsNullOrEmpty(ImageEncoded) || ImageEncoded.StartsWith("Assets/"))
            {
                ImageEncoded = ""; // remove Asset/...png from file version of pdi
                ImageName = "";
                nPicCount = 0;
            }
            else
            {
                nPicCount = 1; // Only 1 image supported so far.
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
                nPicCount = 0;
            }
            else if (ImageEncoded.StartsWith("Assets/"))
            {
                //Don't do this: ImageEncoded = "";
                ImageName = "";
                nPicCount = 0;
            }
            else
            {
                nPicCount = 1; // Only 1 image supported so far.
                ImageName = FormatImageName(); // based on pdi__.Zone
                // ImageEncoded has encoded string
            }
        }


        /// <summary>
        /// Generates EDXL-DE wrapper around text and encoded-photo payloads.
        /// Caller should call GetCurrentOrgAndDeviceData() right before this.
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

            // Move all asserts to beginning of function.  These would cause problems later in function if asserts fail here.
            // See SaveReportFieldsToPDI for valid values for gender, age group
            App.MyAssert(_gender == "Male" || _gender == "Female" || _gender == "Complex Gender" || _gender == "Unknown");
            App.MyAssert(_ageGroup == "Youth" || _ageGroup == "Adult" || _ageGroup == "Unknown Age Group" || _ageGroup == "Other Age Group (e.g., Expectant)");
            App.MyAssert(!String.IsNullOrEmpty(_eventShortName));
            App.MyAssert(!String.IsNullOrEmpty(_eventType));
            App.MyAssert(!String.IsNullOrEmpty(_eventName));  // No longer supporting unnamed events
//// SOME PROBLEM            App.MyAssert(!String.IsNullOrEmpty(_userNameForDevice));
            App.MyAssert(!String.IsNullOrEmpty(_dateEDXL));
            App.MyAssert(!String.IsNullOrEmpty(_distributionID_EDXL));
            App.MyAssert(!String.IsNullOrEmpty(_senderID_EDXL));

            string distribStatus = "Exercise";  // Assume named events are generally exercises.  This is default.
            switch (_eventType)//(patientReport.eventSuffix)
            {
                case "TEST/DEMO/DRILL":
                    //if (String.IsNullOrEmpty(patientReport.eventName) || patientReport.eventName.Contains("Test") || patientReport.eventName.Contains("test"))
                    if(_eventName.Contains("Test") || _eventName.Contains("test"))
                        distribStatus = "Test";
                    break;
                case "REAL - NOT A DRILL": case "REAL":
                    distribStatus = "Actual";
                    break;
                default:
                    break;
            }
 /* Assume already done by call to GetCurrentOrgAndDeviceData():
            _dateEDXL = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"); // UTC date.  Trailing "Z" is std abbrev for "UTC".  Embedded "T" indicates start of time
            _distributionID_EDXL = GetDistributionID();
            _senderID_EDXL = GetSenderID(); */

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
            string results;
            // Begin EDXL wrapper
            results = "<EDXLDistribution xmlns=\"urn:oasis:names:tc:emergency:EDXL:DE:1.0\">" + LF +
            "<distributionID>NPI " + MakeValueSafeForXML(_distributionID_EDXL) + "</distributionID>" + LF + // This can be any unique ID.  We'll make one up.
            "<senderID>" + MakeValueSafeForXML(_senderID_EDXL) + "</senderID>" + LF + // must be of form actor@domainname
            "<dateTimeSent>" + _dateEDXL + "</dateTimeSent>" + LF + // TO DO format: 2007-02-15T16:53:00-05:00
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
            // Transition from EDXL header to XML payload (LPF content)
            string formatAbbreviation  = "LPF";
            
            //girish : moved <incidentID> outside content object
            //results += "<incidentID>" + GetIncidentID() + "</incidentID>" + LF;
            // Question here of whether to use TriageTrak numeric value, or short event name.  Former seems more internal.
            // API will start checking the shortEventName passed here is same as one passed as parameter in (re)report PLUS web service.
            results += "<incidentID>"+ App.CurrentDisaster.EventShortName + "</incidentID>" + LF;

            results +=
            "<contentObject>" + LF +
            "  <contentDescription>" + formatAbbreviation +
                    " notification - disaster victim arrives at hospital triage station</contentDescription>" + LF +
            "  <xmlContent>" + LF;
            
            results += generateLPF();

            results +=
            "  </xmlContent>" + LF +
            "</contentObject>" + LF +
            picContent + // May be empty if no pics.
            // NO LF here
            "</EDXLDistribution>" + LF;

            return results;
       }

        /// <summary>
        /// Encodes chars that are trouble for XML when part of content: ampersand, angle brackets, single and double quotes.
        /// Compare with LPF_SOAP.RemoveEncodingFromXML, which goes in opposite direction
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string MakeValueSafeForXML(string s)
        {
            // From Win 7 ReportPersonFormatting.cs
            if (String.IsNullOrEmpty(s))
                return s;
            // Do XML encoding:
            s = s.Replace("&", "&amp;");
            s = s.Replace("<", "&lt;");
            s = s.Replace(">", "&gt;");
            s = s.Replace("\"", "&quot;");
            s = s.Replace("'", "&apos;");
            return s;
        }

        /// <summary>
        /// For EDXL-DE wrapper.  Returns the sender's email address if available.
        /// Otherwise, the organization's full name and (if available) phone number.  In that case, commas are replaced by hyphens,
        /// for the benefit of inclusion in a distribution reference, which has comma-separated components.
        /// </summary>
        /// <returns></returns>
        private string GetSenderID()
        {
            return GetSenderID(App.CurrentOrgContactInfo);
        }

        private string GetSenderID(TP_OrgContactInfo oci)
        {
            // Compare Win 7 ReportPersonFormatting.cs GetSenderID()
            string senderID = oci.OrgEmail;  // email address shouldn't contain commas.  But if it does, retain it... more important than distr ID
            if (senderID == "")
            {
                App.MyAssert(!String.IsNullOrEmpty(oci.OrgName));
                senderID = oci.OrgName;
                if (!String.IsNullOrEmpty(oci.OrgPhone))
                    senderID += " " + oci.OrgPhone;
                // In first version of EDXL-DE, distribution reference may contain multiple sender IDs separated by commas.
                // EDXL-DE spec didn't define way to escape commas within a senderID when used in distribution reference.
                // So have to screen out commas.  No perfect substitution character... hyphen arguably no worse than others (space or ; / _).
                senderID = senderID.Replace(",", "-");
            }
            return senderID;
        }

        /// <summary>
        /// For EDXL-DE wrapper.  Returns the sender'ss org ID (typically 10 digit NPI) + " " + datetime stampe.
        /// Commas (not expected) are replaced by hyphens, for the benefit of inclusion in a distribution reference, which has comma-separated components.
        /// </summary>
        /// <returns></returns>
        private string GetDistributionID()
        {
            return GetDistributionID(App.CurrentOrgContactInfo);
        }

        private string GetDistributionID(TP_OrgContactInfo oci)
        {
            // Compare Win 7 ReportPersonFormatting.cs GetDistributionID()
            string distributionID = oci.OrgNPI + " " + _dateEDXL;
            return distributionID.Replace(",", "-");
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
           if (!String.IsNullOrEmpty(_comments))
               comments += LF + "Comments: " + MakeValueSafeForXML(_comments);
           return comments;
       }

       protected string generateLPF()
       {
            // Adapted from Win 7 ReportPersonFormatting generateLPF(...)
           // In Win 7 code, patientReport object was explicitly referenced
           // We are doing string rather than XElement here to get nice indentation... makes debug so much easier.
            char cLF = (char)0x0A;
            string LF = cLF.ToString();
            string results;

            string genderCode = _gender.Substring(0, 1); // See SaveReportFieldsToPDI for valid values

            string pedsCode = ""; // if "Unknown..." or "Other..."
            if (_ageGroup == "Youth")
                pedsCode = "Y";
            else if (_ageGroup == "Adult")
                pedsCode = "N";

            results =
            "    <lpfContent>" + LF +
            "      <version>1.3</version>" + LF + // LPF versioning was begun at 1.2, from where PTL left off
            "      <login>" + LF +
            "        <userName>" + MakeValueSafeForXML(_userNameForDevice) + "</userName>" + LF + // or we could fabricate hospitalAuthor as in FormatPFIF
            "        <machineName>" + _deviceName + "</machineName>" + LF + // finally added v 1.60
            "      </login>" + LF +
            "      <person>" + LF +
            "        <personId>" + MakeValueSafeForXML(_patientID /*_PrefixAndNumber*/) + "</personId>" + LF +
            "        <eventName>" + _eventShortName + "</eventName>" + LF + // redefined as short name v 1.20
            "        <eventLongName>" + MakeValueSafeForXML(/*patientReport.eventNameWithSuffix*/ _eventName + " - " +  _eventType) + "</eventLongName>" + LF +
            "        <organization>" + LF + // organization assigning personID, i.e., hospital
            "          <orgName>" + MakeValueSafeForXML(_orgName) + "</orgName>" + LF +
            "          <orgId>" + MakeValueSafeForXML(_orgID/*_orgNPI*/) + "</orgId>" + LF +
            "        </organization>" + LF +
            "        <lastName>" + MakeValueSafeForXML(this._lastName) /*patientReport.lastNameSafeForXML*/ + "</lastName>" + LF +
            "        <firstName>" + MakeValueSafeForXML(this._firstName) /*patientReport.firstNameSafeForXML*/ + "</firstName>" + LF +
            "        <gender>" + genderCode + "</gender>" + LF +
            "        <genderEnum>M, F, U, C</genderEnum>" + LF +
            "        <genderEnumDesc>Male; Female; Unknown; Complex(M/F)</genderEnumDesc>" + LF +
            "        <peds>" + pedsCode + "</peds>" + LF +  // PTL format doesn't support Peds, LPF does.
            "        <pedsEnum>Y,N</pedsEnum>" + LF +
            "        <pedsEnumDesc>Pediatric patient? Yes, No</pedsEnumDesc>" + LF +
            "        <triageCategory>" + /*patientReport.zone*/ this._zone + "</triageCategory>" + LF +
            "        <triageCategoryEnum>Green, BH Green, Yellow, Red, Gray, Black</triageCategoryEnum>" + LF +   //// TO DO <<<
            "        <triageCategoryEnumDesc>Treat eventually if needed; Treat for behavioral health; Treat soon; Treat immediately; Cannot be saved; Deceased</triageCategoryEnumDesc>" + LF;
            if (_deleted)
            {
                results +=
            "        <expiryDate>" + FormatDeleteNowDate() + "</expiryDate>" + LF; // New Dec 2014.  If not included, expiration defaults to whatever policy TT has at the moment, e.g., 1 year.
            }
           results +=
            "        <comments>" + mergeComments() + "</comments>" + LF +
            // Not shown: Patient's home address, personal phone numbers
            // Not shown: Location to which patient is released, transferred, or sent to when deceased
            // Begin transition back to EDXL wrapper:
            "      </person>" + LF +
            "    </lpfContent>" + LF;
            return results;
       }


        /// <summary>
        /// Formats the content of <expiryDate> to be same as WhenLocalTime, except UTC.
        /// </summary>
        /// <returns>If problem with WhenLocalTime, uses UtcNow instead as source</returns>
        private string FormatDeleteNowDate() // New Dec 2014
        {
            // Desired format for TriageTrak parsing is 2022-01-01 16:20:00 UTC
            // The "u" format gets us mostly there, but then replace trailing "Z" with " UTC".
            string dt;
            DateTimeOffset dtoIfProblem = DateTimeOffset.UtcNow;
            if (String.IsNullOrEmpty(WhenLocalTime))
            {
                dt = dtoIfProblem.UtcDateTime.ToString("u");
                dt.Replace("Z"," UTC");
                return dt;
            }

            DateTimeOffset dto = ParseWhenLocalTime(WhenLocalTime);
            if ((dto.DateTime == DateTimeOffset.MinValue) && (dto.Offset == TimeSpan.Zero))
                dt = dtoIfProblem.UtcDateTime.ToString("u"); // unreadable datetime
            else
                dt = dto.UtcDateTime.ToString("u");
                
            dt = dt.Replace("Z"," UTC");
            return dt;
        }

       /// <summary>
       /// EncodePictures takes the synchronized lists of short and full filenames in the passed Patient Report Data,
       /// gets those image files, and creates an XML fragment with base64 encoding of all images, suitable for inclusion in EDXL-DE.
       /// </summary>
       /// <param name="prd"></param>
       /// <param name="previousHashes"></param>
       /// <returns></returns>
       public string GetPictureInContentObjectEDXL(/* TO DO: string[] previousHashes*/)
       {
           // Based somewhat on Win 7's ReportPersonByWebService.cs EncodePictures, but only dealing with a single photo.
           // When called, there should be only .jpg files in lists... but we'll check for ".jpg" anyway.
           // Assumes short and long name arrays are synchronized.
           if (nPicCount == 0 || ImageEncoded.Length == 0 || String.IsNullOrEmpty(ImageName))
               return "";

           string results = "";
           char cLF = (char)0x0A;
           string LF = cLF.ToString();

           UInt32 lengthUnencodedInBytes;
           string hash = GetImageSha1Hash(out lengthUnencodedInBytes);
            //Compare Win 7: ReportPersonByWebService: string encodedFile = EncodeFromFile(fullName, out lengthInBytes, out hash);  // 2 out parameters added in 1.45
           bool suppressResendOfPhotoBytes = false;

#if TODO
            // CAUTIION: If original send failed, then we edit it from in the queue (which will become possible at some point)
            // then code that follows might suppress sending bits that PL doesn't have yet.  Worry about it in the future.
            if (previousHashes != null)  // If this is a new patient, caller sets previousHashes to null
            {
                foreach (string prevHash in previousHashes)
                {
                    if (prevHash == hash)
                    {
                        suppressResendOfPhotoBytes = true; // Not sure how well this mechanism works with multiple instances of default photo.
                        countSuppressedPhotos++;
                        break;
                    }
                }
            }
#endif
            results += "<contentObject>" + LF;  // += here to accumulate contentObjects
            results += "  <contentDescription>" + ImageName.Replace(".jpg", "") + "</contentDescription>" + LF;
            // Other stuff that could go here, but not now:
            //<contentKeyword>
            //    <valueListUrn>
            //    <value>
            //</contentKeyword>
            //<incidentID />
            //<incidentDescription />
            //<originatorRole />
            //<consumerRole />
            //<confidentiality />
            results += "  <nonXMLContent>" + LF;
            results += "    <mimeType>image/jpeg</mimeType>" + LF;
            // Size and SHA-1 added v 1.45
            results += "    <size>" + lengthUnencodedInBytes.ToString() + "</size>" + LF; // image size in bytes before base64 encoding (and any encryption).
            results += "    <digest>" + hash + "</digest>" + LF; // calculated sha-1 value.
            /* FIRST ATTEMPT AT SHA-1:
            results += "    <size>" + encodedFileWithoutLineBreaks.Length * 2 + "</size>" + LF; // filesize in bytes when encrypted.  2 bytes per char
            //If we wanted to be fancy: int len = System.Text.Encoding.ASCII.GetByteCount(str);
            results += "    <digest>" + CalculateSHA1(encodedFileWithoutLineBreaks, Encoding.ASCII) + "</digest>" + LF;
            // <digest /> calculated sha-1 value
            */
            results += "    <uri>" + ImageName + "</uri>" + LF;
            if (suppressResendOfPhotoBytes) // new 1.45
                results += "    <contentData />" + LF;
            else
            {
                results += "    <contentData>" + LF; // don't know if LF will cause any interpretation problems for encoded data.
                results += ImageEncoded + LF; // don't know if LF and spaces before </contentData> will cause any problems
                results += "    </contentData>" + LF;
            }
            results += "  </nonXMLContent>" + LF;
            // In theory one could add other non-standard XML nodes here.  Sometimes used for signing with digital signature.
            results += "</contentObject>" + LF;

#if MAYBE
           // The wording here reflects the fact that we might have multiple copies of the default photo.
           if (countSuppressedPhotos == 1)
               AppBox.Show(
                   "To save bandwidth, repressing resend of a copy of a previously sent photo\n" +
                   "(though will resend its associated text).  PL will use the copy it already has.");
           else if (countSuppressedPhotos > 1)
               AppBox.Show(
                   "To save bandwidth, repressing resend of " + countSuppressedPhotos + " copies of previously sent photos\n" +
                   "(though will resend the associated text).  PL will use the copy it already has.");
#endif
           return results;
       }

       private string GetImageSha1Hash(out UInt32 lengthUnencodedInBytes)
       {
           // Adapted from http cyanbyfuchisa.wordpress.com/2013/01/15/hash-with-c-in-winnt
           App.MyAssert(!String.IsNullOrEmpty(ImageEncoded));
           byte[]imageBytes = Convert.FromBase64String(this.ImageEncoded);
           lengthUnencodedInBytes = Convert.ToUInt32(imageBytes.Length);
           IBuffer buffer = imageBytes.AsBuffer();
           // NO, not from ImageWriteableBitmap: IBuffer buffer = this.ImageWriteableBitmap.PixelBuffer;
           //lengthUnencodedInBytes = buffer.Length;

           // Same result as: UInt32 size = Convert.ToUInt32(ImageWriteableBitmap.PixelHeight * (ImageWriteableBitmap.PixelWidth * 4)); // Assume 32 bits since that is what Metro supports

           HashAlgorithmProvider hashAlgorithm = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
           IBuffer hashBuffer = hashAlgorithm.HashData(buffer);
           // Don't need to do this: return CryptographicBuffer.EncodeToBase64String(hashBuffer);
           // Instead report as hex string
           string hex = BitConverter.ToString(hashBuffer.ToArray());
           hex = hex.Replace("-","");
           return hex;
       }

       public void LoadFromJsonSearchResponse(Search_Response_Toplevel_Row item)
       {
           WhenLocalTime = MapWhenLocalTime(item);
           Timezone = MapTimeZone(item); // As of Release 6, this is a no-op, just gets empty string. See function comments for why.
           WhenLocalTimeMsg1 = MapWhenLocalTimeMsg1(item);
           TimezoneMsg1 = Timezone; // So lame, but best we can do for now. //MapTimeZoneMsg1(item);
           DateEDXL = MapDateEDXL(item);
           DistributionID_EDXL = MapDistributionID_EDXL(item);
           SenderID_EDXL = MapSenderID_EDXL(item);
           DeviceName = MapDeviceName(item);
           UserNameForDevice = MapUserNameForDevice(item);
           UserNameForWebService = MapUserNameForWebService(item);
           OrgName = MapOrgName(item);
           OrgID = MapOrgID(item); // In US, typically NPI
           // These are used to fabricate distribution ID, but do we really need them separately?
           //orgNPI = ;
           //ditto orgPhone, orgEmail
           //        sentCode = ; // Sent code
           SentCode = MapSentCode(item); // TO DO: Msg#n
           Superceded = MapSuperceded(item);
           PatientID = MapPatientID(item); // includes prefix.  If we implement practice mode, may begin with "Prac"
           Zone = MapZone(item);
           Gender = MapGender(item);
           AgeGroup = MapAgeGroup(item); // "Y" = pediatics ("peds") or youth or 0-17, "N" = adult or 18+
           // Usual problem, first name is not necessarily given_name (culture specific), but force for now
           FirstName = MapFirstName(item);
           LastName = MapLastName(item);
           nPicCount = MapPickCount(item); // Int32
           App.MyAssert(MapEventData(item)); // event short name at PL, event long name (w/o suffix), and event type (like Win 7 suffix)
           MapImageInfo(item); // Does ImageName (= name when decoded, w/o path), ImageEncoded, ImageCaption.  Only 1 image [considered primary] supported so far
           Comments = MapComments(item);
           // TO DO: station staff
           WhyNoPhotoReason = MapWhyNoPhotoReason(item); // TO DO
           FullNameEDXL_and_LP2 = ""; // temp file for debug
           //private BitmapImage _imageBitmap = null; // Ignored for serialization
           ImageWriteableBitmap = MapImageWriteableBitmap(item);
       }
#if FOR_REF_INCIDENT_LIST
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
#endif

/* FOR REF:
        public string alternate_names { get; set; } // can be <null>
        public string birth_date { get; set; } // can be <null>
        public string creation_time { get; set; } // "2013-08-12T18:29:54-04:00"
        public string expiry_date { get; set; }  // can be <null>
        public string family_name { get; set; }  // can be <null>
        public string full_name { get; set; } //can be <null>
        public string given_name { get; set; } // can be <null>
        public int hospital_uuid { get; set; }
        public List<Search_Response_Image> images { get; set; }
        public int incident_id { get; set; }
        public string last_clothing { get; set; } // can be <null>
        public string last_seen { get; set; } // e.g., "NLM (testing)";
        public string last_updated { get; set; } // "2013-08-12T18:31:50-04:00"
        public Search_Response_Location location { get; set; }  // can be <null>
        public int max_age { get; set; }  // e.g., 150
        public int min_age { get; set; } // e.g., 18
        public string opt_gender { get; set; } // Sahana code, e.g., "mal"
        public string opt_race { get; set; } // can be <null>
        public string opt_religion { get; set; } // Sahana code, can be <null>
        public string opt_status { get; set; } // Sahana code, e.g., "ali"
        public string other_comments { get; set; } // e.g., "LPF notification - disaster victim arrves at hospital triage station"
        public string p_uuid { get; set; } // e.g., "triagetrak.nlm.nih.gov/person.3099294"
        public List<Search_Response_Person_Notes> person_notes { get; set; }
        public string profile_urls { get; set; } // can be <null>
        public string rep_uuid { get; set; } // e.g., "triagetrak.nlm.nih.gov/person.10480
        public string years_old { get; set; } // can be <null> */

        
        // Essential mapping was described in Glenn's google doc "Proposed Search Results Revision to PLUS (People Locator User Services)"
        // Returned format is YYYY-MM-DD HH:MM:SS [-]KK:KK
        private string MapWhenLocalTime(Search_Response_Toplevel_Row item)
        {
/* Before June 2015:
            // Maybe need to chew off -04:00:
            if(item.edxl != null && item.edxl.Count() > 0 && !String.IsNullOrEmpty(item.edxl[0].last_posted))
                return item.edxl[0].last_posted; // Assume same format and timezone as last_updated
            if (!String.IsNullOrEmpty(item.last_updated))
                return item.last_updated;
            return item.creation_time; */
            // Let us retain -04:00:
            if (!String.IsNullOrEmpty(item.last_updated))
                return MapWhenLocalTimeImpl(item.last_updated); // input example: 2015-06-21T22:11:11-04:00 .  Server time I guess

            // It would be better to do client time, but we don't really know UTC offset for that (yet).  We could make some assumptions to infer UTC offset of last_posted and add it, by
            // comparing it with last_updated, looking for differences that were (appx) intervals of 1 hr.
            if (item.edxl != null && item.edxl.Count() > 0 && !String.IsNullOrEmpty(item.edxl[0].last_posted))
                return item.edxl[0].last_posted; // example: 2015-06-21 22:11:11   Same(ish) value as last_updated, but with " " instead of "T" and no UTC offset. Client time I guess
            // Really need to fabricate an original record too in some cases, which would be better for stat charts... to do
            return MapWhenLocalTimeImpl(item.creation_time); // example input: 2015-06-21T22:11:11-04:00  Time casualty record first created (before any edits). Client time?
        }

        /// <summary>
        /// Makes a slightly more readable format, used as the TP8 standard in the WhenLocalTime attribute of the XML files
        /// </summary>
        /// <param name="s">Assumes input format of YYYY-MM-DDTHH:MM:SS-KK:KK or YYYY-MM-DDTHH:MM:SS+KK:KK</param>
        /// <returns>YYYY-MM-DD HH:MM:SS -KK:KK or YYYY-MM-DDTHH:MM:SS +KK:KK</returns>
        private string MapWhenLocalTimeImpl(string s)
        {
            s = s.Replace("T", " ");
            if(s.Length != 25)
                return s;
            string s1 = s.Substring(0, 19);
            string s2 = s.Substring(19);
            s2 = s2.Replace("-", " -");
            s2 = s2.Replace("+", " +");
            return s1+s2;
        }

        private string MapTimeZone(Search_Response_Toplevel_Row item)   // TO DO
        {
/* Made a no-op for Release 6 (v 3.6). Will leave it blank, to indicate that there's no time-zone info available from TT.
 * Instead, we'll do an inference of time zone at display time. See FormatContent, particularly FormatLocalAndUtcDates, above.
            // There is not a unique mapping from UTC offset to time zone name or abbreviation
            // So can only be done in context of location and date (for daylight savings vs standard)
            // Really, too hard... it would be way better if TriageTrak retained the reported value
            string dt = MapDateEDXL(item);
            // Pathetic, will only work for east coast US:
            if(dt.Contains("-04:00"))
                return "EDT";
            if (dt.Contains("-05:00"))
                return "EST";
 */
            return "";
        }

        // Essential mapping was described in Glenn's google doc "Proposed Search Results Revision to PLUS (People Locator User Services)"
        // Returned format is YYYY-MM-DD HH:MM:SS [-]KK:KK
        private string MapWhenLocalTimeMsg1(Search_Response_Toplevel_Row item)
        {
            /* Before June 2015:
                        // Maybe need to chew off -04:00:
                        if(item.edxl != null && item.edxl.Count() > 0 && !String.IsNullOrEmpty(item.edxl[0].last_posted))
                            return item.edxl[0].last_posted; // Assume same format and timezone as last_updated
                        if (!String.IsNullOrEmpty(item.last_updated))
                            return item.last_updated;
                        return item.creation_time; */
            // Let us retain -04:00:
            if (!String.IsNullOrEmpty(item.creation_time))
                return MapWhenLocalTimeImpl(item.creation_time); // input example: 2015-06-21T22:11:11-04:00 .  Server time I guess
            // A case could be made for leaving this null, as that would be accurate. But it will cause problems later
            return MapWhenLocalTimeImpl(item.last_updated); // example input: 2015-06-21T22:11:11-04:00  Time casualty record first created (before any edits). Client time?
        }


        private string MapDateEDXL(Search_Response_Toplevel_Row item)
        {
            if (item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].distr_id))
                return MapWhenLocalTime(item);
            string s = item.edxl[0].distr_id;
            int i = s.LastIndexOf(" ");
            if (i < 0)
                return MapWhenLocalTime(item);
            if (s.Length == i + 1)
            {
                // There's a trailing space... could be benign, or indicates a missing datetime
                // For now, bail...
                return MapWhenLocalTime(item);
            }
            string t = s.Substring(i + 1);
            // Maybe need to chew off -04:00:
            return t; // TO DO: Actual parse this to confirm its in time format
        }

        private string MapDistributionID_EDXL(Search_Response_Toplevel_Row item)
        {
            if(item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].distr_id))
                return "";

            return item.edxl[0].distr_id;
        }

        private string MapSenderID_EDXL(Search_Response_Toplevel_Row item)
        {
            if (item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].sender_id))
                return "";

            return item.edxl[0].sender_id;
        }

        private string MapDeviceName(Search_Response_Toplevel_Row item)
        {
            if (item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].login_machine))
                return "";

            return item.edxl[0].login_machine;
        }

        private string MapUserNameForDevice(Search_Response_Toplevel_Row item) 
        {
            if (item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].login_account))
                return "";

            return item.edxl[0].login_account; // aka users.user_name
        }

        private string MapUserNameForWebService(Search_Response_Toplevel_Row item)
        {
            return item.reporter_username;
        }

        private string MapOrgName(Search_Response_Toplevel_Row item)
        {
            if (item.hospital_uuid == null)
                return "Public/Other";
            TP_OrgData od = App.OrgDataList.GetOrgDataFromOrgUuid(item.hospital_uuid.ToString());
            if (od != null && !String.IsNullOrEmpty(od.OrgName))
                return od.OrgName;
            if (!String.IsNullOrEmpty(item.last_seen)) // has organization long name if reported by TriagePic
                return item.last_seen;
            return "TODO"; // Could try search by OrgName too...
        }

        private string MapOrgID(Search_Response_Toplevel_Row item)
        {
            if (item.hospital_uuid == null)
                return "0"; // Public/Other
            TP_OrgData od = App.OrgDataList.GetOrgDataFromOrgUuid(item.hospital_uuid.ToString());
            if(od == null || String.IsNullOrEmpty(od.OrgNPI))
                return "TODO";
            return od.OrgNPI; // In US, can use NPI for hospitals and other facilities accepting Medicare
        } 

         private string MapSentCode(Search_Response_Toplevel_Row item) {return "Y";} // TO DO: Msg#n... only makes sense for sent from here

         private bool   MapSuperceded(Search_Response_Toplevel_Row item) {return false;}

         private string MapPatientID(Search_Response_Toplevel_Row item)
         {
             // includes prefix.  If from Win 7, or we implement practice mode in Win 8, may begin with "Prac"
             if (item.edxl == null || item.edxl.Count() == 0 || String.IsNullOrEmpty(item.edxl[0].mass_casualty_id))
                 return "";

             return item.edxl[0].mass_casualty_id;
         } 

         private string MapZone(Search_Response_Toplevel_Row item)
         {
             if (item.edxl != null && item.edxl.Count() > 0 && !String.IsNullOrEmpty(item.edxl[0].triage_category))
                 return item.edxl[0].triage_category;

             // Estimate from Sahana status
             if(String.IsNullOrEmpty(item.opt_status))
                 return "Green"; // TO DO: Org psecific
             switch (item.opt_status)
             {
                 case "ali": return "Green";
                 case "inj": return "Yellow";
                 case "dec": return "Black"; // I think this is the 3 letter code for deceased.
                 default: break;
             }
             return "Green"; // can't really express unknown for this.  Probably should change that.
         }

         /// <summary>
        /// Based on opt_gender, returns "Male","Female","Unknown", or "Complex Gender".  Unknown if any problem.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
         private string MapGender(Search_Response_Toplevel_Row item)
         {
             switch (item.opt_gender)
             {
                 case "mal":
                     return "Male";  // reported as M
                 case "fml":
                     return "Female";  // reported as F
                 case "cpx":
                     return "Complex Gender";  // reported as C
                 default:  // include unk, null    // reported as U
                     break;
             }
             return "Unknown";
         }


        /// <summary>
         /// Returns "Youth" = pediatics ("peds") = Y or youth or 0-17, "Adult" = adult or 18+, "Unknown Age Group" if unknown or problem.
         /// Special case: "Other Age Group (e.g., Expectant)", which may or may not be handled right here & at TriageTrak
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
         private string MapAgeGroup(Search_Response_Toplevel_Row item)
         {
             if(!String.IsNullOrEmpty(item.years_old))
             {
                 try
                 {
                     UInt32 age = Convert.ToUInt32(item.years_old);
                     if(age >= 0 && age <= 17)
                         return "Youth";  // reported as Y for Peds
                     if (age >= 18 && age <= 150)
                         return "Adult";  // reported as N for Peds
                     // If problem, ignore age, look at just min and max
                 }
                 catch(Exception) {}// If problem, ignore age, look at just min and max
             }
             if (String.IsNullOrEmpty(item.min_age) || String.IsNullOrEmpty(item.max_age))
                 return "Unknown Age Group";
             UInt32 minAge;
             UInt32 maxAge;
             try
             {
                 minAge = Convert.ToUInt32(item.min_age);
                 maxAge = Convert.ToUInt32(item.max_age);
             }
             catch (Exception)
             {
                 return "Unknown Age Group";
             }
             if (minAge > 150 || maxAge > 150 || minAge > maxAge)
                 return "Unknown Age Group";
             //if (String.IsNullOrEmpty(item.min_age) || String.IsNullOrEmpty(item.max_age))
             //    return "";
             if (minAge <= 17 && maxAge >= 18) // straddles adult & youth.  Indeterminate
                 return "Other Age Group (e.g., Expectant)";  // MAYBE this is how mapping should work
             if (maxAge <= 17)
                 return "Youth";
             return "Adult";
         } 
         
        /// <summary>
        /// Return the first name, nick name, and middle initial if given
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
         private string MapFirstName(Search_Response_Toplevel_Row item)
         {
             // Usual problem, first name is not necessarily given_name (culture specific), but force for now.
             // Could make smarter if take location of event into account
             // Before Release 3, calls herein to String.IsNullOrWhiteSpace were String.IsNullOrEmpty.  Not uncommon to see strings of " ".
             string ret = "";
             if (!String.IsNullOrWhiteSpace(item.given_name))
             {
                ret = item.given_name;
                if (!String.IsNullOrWhiteSpace(item.alternate_names))
                    ret += " \"" + item.alternate_names + "\""; // treat as nick
             }
             else if (!String.IsNullOrWhiteSpace(item.full_name))
             {
                 ret = GetAllButLastWord(item.full_name);
             }
             else if (!String.IsNullOrWhiteSpace(item.alternate_names))
             {
                 ret = GetAllButLastWord(item.alternate_names);
             }
             return ret;
         }

        private string GetAllButLastWord(string name)
        {
            char[] c = new char[1] { ' ' };
            name = name.Trim(c);  // Added for Release 3
            if (!name.Contains(' '))
                return ""; // Added for Release 3
            string[] s = name.Split(c);
            return name.Substring(0, name.Length - (s.Last().Length + 1)); // +1 for last space separator
        }

        private string GetLastWord(string name)
        {
            // TO DO: Make smarter to handle suffixes like Jr.  Also prefixes like "von"
            char[] c = new char[1] { ' ' };
            name = name.Trim(c);  // Added for Release 3
            if (!name.Contains(' '))
                return name; // Added for Release 3
            string[] s = name.Split(c);
            return s.Last(); // Before Release 3, was buggy: s[0]
        }

         private string MapLastName(Search_Response_Toplevel_Row item)
         {
             // Usual problem, last name is not necessarily family_name (culture specific), but force for now.
             // Could make smarter if take location of event into account.
             // Before Release 3, calls herein to String.IsNullOrWhiteSpace were String.IsNullOrEmpty.  Not uncommon to see strings of " ".
             string ret = "";
             if (!String.IsNullOrWhiteSpace(item.family_name))
             {
                 ret = item.family_name;
             }
             else if (!String.IsNullOrWhiteSpace(item.full_name))
             {
                 ret = GetLastWord(item.full_name);
             }
             else if (!String.IsNullOrWhiteSpace(item.alternate_names))
             {
                 ret = GetLastWord(item.alternate_names);
             }
             return ret;
         }

         private Int32  MapPickCount(Search_Response_Toplevel_Row item)
         {
             return item.images.Count();
         }

         /// <summary>
         /// Returns fills in short event name (as known at TriageTrak), long name, and event type.
         /// </summary>
         /// <param name="item"></param>
         /// <returns>false if problem</returns>
         private bool MapEventData(Search_Response_Toplevel_Row item)
         {
             string evID = item.incident_id.ToString();
             if (String.IsNullOrEmpty(evID))
                 return false;
             TP_EventsDataItem edi = App.CurrentDisasterList.FindByIncidentID(evID);
             if (edi == null)
                return false; // TO DO - Do Web Service lookup
             if(String.IsNullOrEmpty(edi.EventShortName) || String.IsNullOrEmpty(edi.EventName) || String.IsNullOrEmpty(edi.EventType))
                 return false;  
             EventShortName = edi.EventShortName;
             EventName = edi.EventName;
             EventType = edi.EventType;
             return true;
         }


         // Only supporting 1 image so far.  Look for primary:
         private void MapImageInfo(Search_Response_Toplevel_Row item)
         {
             /* Example:
                    "images":[
                    {
                    "note_record_id":null,
                    "image_id":"86576",
                    "image_type":"jpeg",
                    "image_height":"598",
                    "image_width":"440",
                    "created":"2013-11-07 13:58:29",
                    "url":"tmp\/plus_cache\/pl.nlm.nih.govSLASHperson.2961741__86576_full.jpg",
                    "url_thumb":"tmp\/plus_cache\/pl.nlm.nih.govSLASHperson.2961741__86576_thumb.jpg",
                    "original_filename":"911-00003 Gray.jpg",
                    "principal":"1",
                    "sha1original":"b0b0e7f818effd77e81f413bf7b4cde8436119d3",
                    "color_channels":"3",
                    "note_id":0,
                    "tags":[]
                    }], */
             foreach (Search_Response_Image i in item.images)
             {
                 if (i.principal == "1" || item.images.Count() == 1)
                 {
                     string endpoint = App.pl.Endpoint.Address.Uri.AbsoluteUri; // find path to TriageTrak, chew off trailing "/?wsdl&api=<num>"
                     int lastslash = endpoint.LastIndexOf('/');
                     App.MyAssert(lastslash > 0);
                     endpoint = endpoint.Substring(0, lastslash);
                     this.ImageName = i.original_filename; // MAYBE
                     // Note: don't replace ...SLASH... with .../...
                     this.ImageEncoded = endpoint + "/" + i.url_thumb; // MAYBE.  For full size, see i.url.  Dump this here, then we'll replace it in TP_PatientReportsSource after image fetch
                     //this.GetImageSha1Hash
                     this.ImageCaption = "";
                     foreach (Search_Response_Tag t in i.tags)
                     {
                         if (t.tag_x == "0" && t.tag_y == "0" && t.tag_w == "0" && t.tag_h == "0")
                         {
                             this.ImageCaption = t.tag_text;
                             break;
                         }
                     }
                     break;
                 }
             }
         }
         //private string MapImageName(Search_Response_Toplevel_Row item) {return "TODO";} // name when decoded, w/o path
         //private string MapImageEncoded(Search_Response_Toplevel_Row item) {return "TODO";}
         //private string MapImageCaption(Search_Response_Toplevel_Row item) {return "TODO";}
         private string MapComments(Search_Response_Toplevel_Row item)
         {
             //"last_clothing":null,"other_comments":"LPF notification - disaster victim arrives at hospital triage station&amp;#10;Pictures: 1"
             string oc = item.other_comments;
             if (String.IsNullOrEmpty(oc) && String.IsNullOrEmpty(item.last_clothing)  && String.IsNullOrEmpty(item.last_seen))
                 return "";
             string results = "";
             if (!String.IsNullOrEmpty(oc))
             {
                 oc = oc.Replace("&amp;#10;", "\n"); // kludge until TriageTrak is fixed
                 oc = oc.Replace("&#10;", "\n");
                 string[] oca = oc.Split('\n');
                 foreach (string ss in oca)
                 {
                     if (ss == "")
                         continue;
                     if (ss.Contains("LPF notification - disaster victim arrives at hospital triage station")) // Uniformative here
                         continue;
                     if (ss.Contains("Pictures: "))
                         continue; // We'll handle this elsewhere
                     if (ss.StartsWith("Comments: "))
                     {
                         results += ss.Substring("Comments: ".Length); // chew of Comments
                         continue;
                     }
                     results += ss + "n";
                 }
             }
             if (!String.IsNullOrEmpty(item.last_clothing))
                 results += "Last clothing: " + item.last_clothing + "\n";
             if ((item.hospital_uuid == null) && !String.IsNullOrEmpty(item.last_seen))
                 results += "Last seen: " + item.last_seen + "\n";
             return results;
         }
                        // TO DO: station staff
         private string MapWhyNoPhotoReason(Search_Response_Toplevel_Row item) {return "TODO";} // TO DO
         //private string FullNameEDXL_and_LP2 = "" {} // temp file for debug
                        //private BitmapImage _imageBitmap = null {} // Ignored for serialization
         private WriteableBitmap MapImageWriteableBitmap(Search_Response_Toplevel_Row item) {return null;} // TO DO

    }
}
