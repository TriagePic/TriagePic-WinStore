using TP8;
using TP8.PLWS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using System.Data;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.IO;

//using Newtonsoft.Json;
using TP = TP8;
//using PLWS = TriagePic.PLWS;
//using CodeProject.Dialog;
using System.Diagnostics;
using System.Threading.Tasks;
using TP8.Data;
using Windows.UI.Popups;

namespace LPF_SOAP
{
    /// <summary>
    /// Class LPF_JSON (in namespace LPF_SOAP and file LPF_SOAP.cs) is intended to be the interface to PLUS SOAP+JSON web-services.
    /// </summary>

    public partial class LPF_JSON
    {
        // Win 7: private TP.FormTriagePic parent;
//        private string event_list_path;
//        private string hospital_list_path;
        private bool endPointAddressReplacementAlreadyChecked = false;
        private const string PHONY_COMMUNICATIONS_EXCEPTION = "Phony exception to test communications failure";
        //private int pingLatencyInTenthsOfSeconds = -1;
        //Stopwatch stopwatch;

        public LPF_JSON() {}

#if SETASIDE
        public LPF_JSON() //(TP.FormTriagePic p)
        {
            SetAttributes(); // (p);
        }

        public void SetAttributes() //(TP.FormTriagePic p) // broken out separatedly 1.63
        {
            //parent = p;
            //event_list_path = parent.sharedAppDir + "/eventList.xml"; // Will vary depending on CLICKONCE define in parent
            //hospital_list_path = parent.sharedAppDir + "/HospitalList.xml"; // ditto
        }
#endif
 
        /// <summary>
        /// Returns the incident list 
        /// </summary>
        public async Task<string> GetIncidentList() 
        {
            //Win 7: string errorCode = ""; string errorMessage = ""; string responseData = "";
            getEventListRequest elin = null;
            getEventListResponse elout = null;
            try
            {

                SetPLEndpointAddress(App.pl); //(App.pl); //read the configured endpoint address
                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                //Win 7: responseData = App.pl.getEventList(out errorCode, out errorMessage);
                elout = await App.pl.getEventListAsync(elin);
            }

            catch (Exception e)
            {
                return "ERROR: " + e.Message;  // Win 7: responseData = "ERROR: " + e.Message;
            }

            return ChangeToErrorIfNull(elout.eventList); // Win 7: return ChangeToErrorIfNull(responseData);
        }

        /// <summary>
        /// Returns the incident list for a given user
        /// </summary>
        public async Task<string> GetIncidentList(string userPL, string passwordPL)
        {
            // Win 7: string errorCode = ""; string errorMessage = ""; string responseData = "";
            //getEventListUserRequest elin = new getEventListUserRequest();
            getEventListRequest elin = new getEventListRequest();
            //elin.username = userPL;
            //elin.password = passwordPL;
            elin.token = App.TokenPL;
            //getEventListUserResponse elout = new getEventListUserResponse();
            getEventListResponse elout = new getEventListResponse();
            try
            {

                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                // Win 7: responseData = App.pl.getEventListUser(userPL, passwordPL, out errorCode, out errorMessage);
                elout = await App.pl.getEventListAsync(elin); // App.pl.getEventListUserAsync(elin);
            }
            catch (Exception e)
            {
                elout.eventList = "ERROR: " + e.Message; // Win 7: responseData = "ERROR: " + e.Message;
            }

            return ChangeToErrorIfNull(elout.eventList); // (responseData);
        }

        public class PLUS_URL_Parts
        {
            // This is not a general-purpose URL parser... It's to help apply a particular interpretation to the
            // web service endpoint address, and its possible partial or complete substitution by OtherSettings.xml's <PLEndpointAddress> value.
            public bool hasURL { get; set; }
            public string URL { get; set; }
            public bool hasQuesWSDL { get; set; }
            public bool hasAPI { get; set; }
            public bool hasVersion { get; set; }
            public int version { get; set; }

            public PLUS_URL_Parts() {}

            public void Parse(string s)
            {
                hasURL = s.StartsWith("http://") || s.StartsWith("https://"); // We are requiring leading https:// or http:// in URL
                int locQuesWsdl = s.IndexOf("?wsdl");
                hasQuesWSDL = (locQuesWsdl > -1);
                if(hasURL)
                {
                    if(locQuesWsdl > -1)
                        URL = s.Substring(0,locQuesWsdl);
                    else
                        URL = s;
                    // For consistency of treatment, remove trailing "/"
                    if (URL[URL.Length-1] == '/')
                        URL = URL.Remove(URL.Length-1);
                    App.MyAssert(URL.Length > 0);
                }
                hasAPI = s.Contains("api=");
                version = -1;
                hasVersion = false;
                if (hasAPI)
                {
                    int startOfVersionNum = s.IndexOf("api=") + 4;
                    try
                    {
                        version = Convert.ToInt32(s.Substring(startOfVersionNum));
                        hasVersion = true;
                    }
                    catch (Exception /*e*/)
                    {
                        version = -1; // It's OK to specify "api=" without a number, one way to say "latest version"
                    }
                }
            }
        }

        /// <summary>
        /// Sets the PLWS end point address if a new value was given, and in any case displays it.
        /// An optional value in OtherSettings.xml can override build-time service reference.  See separate Word doc for override syntax.
        /// </summary>
        private void SetPLEndpointAddress(TP8.PLWS.plusWebServicesPortTypeClient pl)
        {
            if (endPointAddressReplacementAlreadyChecked)
                return;
            endPointAddressReplacementAlreadyChecked = true;

            // NOW WORKS WITH NEW WSDL: pl.Endpoint.Address = new System.ServiceModel.EndpointAddress("https://plstage.nlm.nih.gov/?wsdl&api=28");
            if (String.IsNullOrEmpty(App.CurrentOtherSettings.PLEndPointAddress))
            {
/* Moved to SettingsCredentials
                // Ampersand in "app.config" [really TriagePic.exe.config] represented as &amp;, but it's "&" when we get here.
                // Need to double it so that "&: is not treated as "underline next char" in link next
                parent.endPointURI_LinkLabel.Text = App.pl.Endpoint.Address.Uri.AbsoluteUri.Replace("&", "&&");
                parent.AddressCustomizedCheckBox.Checked = false;
 */
                return; // Normal: Use default URL & same version of API that TriagePic was built for
            }

            // Otherwise, possibly override defaults - given generated by Service Reference into app.config, and
            // propagated to target machine as
            // C:\Documents and Settings\<my account>\Local Settings\Apps\2.0\<random string 1>\
            // <random string 2>\tria..tion_<identifier suffix with hash of name and version>
            string substitute = App.CurrentOtherSettings.PLEndPointAddress;
            PLUS_URL_Parts sub = new PLUS_URL_Parts();
            sub.Parse(substitute);
            string newEndPoint;
            // Replace whole string?
            if (sub.hasURL && sub.hasQuesWSDL && sub.hasAPI && sub.hasVersion)
            {
                newEndPoint = substitute;  // yes
            }
            else
            {
                // Otherwise, start with existing endpoint and replace certain parts
                string oldEndPoint = pl.Endpoint.Address.ToString();
                // We can assume this starts with "http://" or "https://" & contains "?wsdl"
                // it will also routinely contains the api version from the build, but maybe that's not safe to assume.
                PLUS_URL_Parts old = new PLUS_URL_Parts();
                old.Parse(oldEndPoint);

                // Now we have the parts, assemble them:
                if (sub.hasURL)
                    newEndPoint = sub.URL;
                else
                    newEndPoint = old.URL;
                newEndPoint += "/?wsdl";  // always need this, whether or not its included in substitution string
                // Now differentiate 3 cases:
                // 1) Only the URL was substituted. Continue with specific version (or "/?wsdl"-only string, meaning use latest), that TriagePic was built with?
                if (!sub.hasQuesWSDL && !sub.hasAPI)
                {
                    if (old.hasAPI)
                    {
                        App.MyAssert(old.hasVersion);
                        newEndPoint += "&api=" + old.version;
                    }
                }
                else
                {
                    if (sub.hasVersion) // 2) Substitute specific version
                        newEndPoint += "&api=" + sub.version;
                    // else (3) Substitute "latest official version", not specific api
                        // We could make this more complicated here to rule out some formats, but good enough.
                        // Already have what we need, namely just .../?wsdl
                }
            }
            pl.Endpoint.Address = new System.ServiceModel.EndpointAddress(newEndPoint);
/* Moved to SettingsCredentials
            // Show to user
            // Ampersand in app.config represented as &amp;, but it's "&" when we get here.
            // Need to double it so that "&: is not treated as "underline next char" in link next
            parent.endPointURI_LinkLabel.Text = newEndPoint.Replace("&", "&&"); //App.pl.Endpoint.Address.Uri.AbsoluteUri.Replace("&", "&&"); // From app.config
            parent.AddressCustomizedCheckBox.Checked = true;
 */
        }

#if OLD_v32
        // Sorta converted to v33, but can no longer return server time stamp... don't bother.
        /// <summary>
        /// Returns the ping with server time stamp
        /// </summary>
        public async Task<string> GetPing()
        {
            string serverTime = ""; // Format:  2012:0209 18:02:31.000000 America/New_York

            //pingRequest pin = new pingRequest();
            //pingResponse pout = new pingResponse();
            pingEchoResponse pout = new pingEchoResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                pout = await App.pl.pingEchoAsync(pin);//App.pl.pingAsync(); // No error messages from server.  From framework?
                serverTime = pout.time;
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }

            return ChangeToErrorIfNull(serverTime);
        }
#endif

        /// <summary>
        /// Returns the ping with server time stamp, passes in elapsed time of previous ping.
        /// </summary>
        public async Task<string> GetPing(int pingLatencyInTenthsOfSeconds)
        {
            // string errorCode = "";
            // string errorMessage = "";
            string serverTime = ""; // Format:  2012:0209 18:02:31.000000 America/New_York

            // pingWithEchoRequest pin = new pingWithEchoRequest();
            pingEchoRequest pin = new pingEchoRequest();
            // pin.pingString = App.DeviceName;
            // pin.latency = pingLatencyInTenthsOfSeconds.ToString();
            pin.token = App.TokenPL;
            pin.latency = pingLatencyInTenthsOfSeconds.ToString();
            pin.pingString = App.DeviceName + ";TriagePic-Win8.1";
                // Ideal format: "machinename;device id;app name;app version;operating system;device username;pl username"
            //pingWithEchoResponse pout = new pingWithEchoResponse();
            pingEchoResponse pout = new pingEchoResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if (App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                //pout = await App.pl.pingWithEchoAsync(pin);
                pout = await App.pl.pingEchoAsync(pin); //App.pl.pingWithEchoAsync(App.DeviceName, pingLatencyInTenthsOfSeconds.ToString());
                serverTime = pout.time;
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
//            if (errorCode != "0")
//                return PackageErrorString(errorCode, errorMessage);

            return ChangeToErrorIfNull(serverTime);
        }


        /// <summary>
        /// Returns the hospital list, or an error message
        /// </summary>
        public async Task<string> GetHospitalList()
        {
            // Win 7: string errorCode = ""; string errorMessage = ""; string hospitalList = "";
            getHospitalListRequest hlin = new getHospitalListRequest();
            getHospitalListResponse hlout = new getHospitalListResponse();
            
            try
            {
                //Moved to TriagePic variable:  PLWS.plusWebServicesPortTypeClient pl = new PLWS.plusWebServicesPortTypeClient();
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                // Win 7: hospitalList = App.pl.getHospitalList(out errorCode, out errorMessage);
                hlout = await App.pl.getHospitalListAsync(hlin);

            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }

            if (hlout.errorCode != "0")
                return PackageErrorString(hlout.errorCode, hlout.errorMessage);

            return ChangeToErrorIfNull(hlout.hospitalList);
        }

        /// <summary>
        /// Gets the organization's contact info, e.g., hospital data for hospital, and puts it into App.CurrentOrgContactInfo (and as first and only item of App.CurrentOrgContactList)
        /// Returns an empty string if no error.
        /// </summary>
        public async Task<string> GetHospitalData(string hospital_uuid, string hospitalName)
        {
            /* Win 7:
            string errorCode;
            string errorMessage = "";
            string retVal = "";

            string shortname = ""; 
            string street1 = ""; 
            string street2 = ""; 
            string city = "";
            string county = "";
            string state = ""; 
            string country = ""; 
            string zip = ""; 
            string phone = ""; 
            string fax = ""; 
            string email = ""; 
            string www = ""; 
            string npi = ""; 
            string latitude = ""; 
            string longitude = ""; */
            getHospitalDataRequest hdin = new getHospitalDataRequest();
            hdin.hospital_uuid = hospital_uuid;
            getHospitalDataResponse hdout = new getHospitalDataResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                /* Win 7: retVal = App.pl.getHospitalData(
                    hospital_uuid,
                    out shortname, 
                    out street1, 
                    out street2, 
                    out city,
                    out county,
                    out state, 
                    out country, 
                    out zip, 
                    out phone, 
                    out fax, 
                    out email, 
                    out www, 
                    out npi, 
                    out latitude, 
                    out longitude, 
                    out errorCode, 
                    out errorMessage); */
                hdout = await App.pl.getHospitalDataAsync(hdin);

/* MAYBE:
                    if (retVal == null)
                        return ChangeToErrorIfNull(null);
 */
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }

            /* Win 7: 
            if (parent != null)
            {
                parent.orgNameTextBox.Text = parent.hs.oci.OrgNameText = hospitalName;
                parent.orgAbbrOrShortNameTextBox.Text = parent.hs.oci.OrgAbbrOrShortNameText = shortname;
             etc */
            App.CurrentOrgContactInfo.OrgName = hdout.name;
            App.CurrentOrgContactInfo.OrgAbbrOrShortName = hdout.shortname;
            App.CurrentOrgContactInfo.OrgStreetAddress1 = hdout.street1;
            App.CurrentOrgContactInfo.OrgStreetAddress2 = hdout.street2;
            App.CurrentOrgContactInfo.OrgTownOrCity = hdout.city;
            App.CurrentOrgContactInfo.OrgCounty = hdout.county;
            App.CurrentOrgContactInfo.Org2LetterState = hdout.state;
            App.CurrentOrgContactInfo.OrgZipcode = hdout.zip;
            App.CurrentOrgContactInfo.OrgPhone = hdout.phone;
            App.CurrentOrgContactInfo.OrgFax = hdout.fax;
            App.CurrentOrgContactInfo.OrgEmail = hdout.email;
            App.CurrentOrgContactInfo.OrgWebSite = hdout.www;
            App.CurrentOrgContactInfo.OrgNPI = hdout.npi;
            App.CurrentOrgContactInfo.OrgLatitude = hdout.latitude;
            App.CurrentOrgContactInfo.OrgLongitude = hdout.longitude;
            App.CurrentOrgContactInfo.OrgCountry = hdout.country;

            App.OrgContactInfoList.Clear();
            App.OrgContactInfoList.Add(App.CurrentOrgContactInfo); // kludgery

            if (hdout.errorCode != "0")
                return PackageErrorString(hdout.errorCode, hdout.errorMessage);

            return "";
        }

        /// <summary>
        /// Returns the hospital policy for hospital 
        /// </summary>
        public async Task<string> GetHospitalPolicy(string hospital_uuid)
        {
            /* Win 7:
            string errorCode;
            string errorMessage = "";
            string retValPatientIdPrefix = "";
            bool patientIdSuffixVariable = true;
            string patientIdSuffixFixedLength = "";
            bool photoRequired = true;
            bool honorNoPhotoRequest = false;
            bool photographerNameRequired = false; */
            getHospitalPolicyRequest hpin = new getHospitalPolicyRequest();
            hpin.hospital_uuid = hospital_uuid;
            getHospitalPolicyResponse hpout = new getHospitalPolicyResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                /* Win 7:
                retValPatientIdPrefix = App.pl.getHospitalPolicy(
                    hospital_uuid,
                    out patientIdSuffixVariable, // boolean
                    out patientIdSuffixFixedLength, //number of digits if fixed, from 2 to 9.  Gets leading zeroes.
                    out photoRequired,
                    out honorNoPhotoRequest,
                    out photographerNameRequired,
                    out errorCode, 
                    out errorMessage); */
                hpout = await App.pl.getHospitalPolicyAsync(hpin);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            if (hpout.errorCode != "0")
                return PackageErrorString(hpout.errorCode, hpout.errorMessage);
/* MAYBE:
            if (retValPatientIdPrefix == null)
                return ChangeToErrorIfNull(null);
*/
            // Win 7:DistributeHospitalPolicy(retValPatientIdPrefix, patientIdSuffixVariable, patientIdSuffixFixedLength,
            //   photoRequired, honorNoPhotoRequest, photographerNameRequired);
            if (hpout.patientIdSuffixVariable)
                App.OrgPolicy.OrgPatientIdFixedDigits = -1;
            else // TO DO error detection around convert to int32
                App.OrgPolicy.OrgPatientIdFixedDigits = Convert.ToInt32(hpout.patientIdSuffixFixedLength);
            App.OrgPolicy.OrgPatientIdPrefixText = hpout.patientIdPrefix;
// KLUDGE DUE TO DESIGN MESS UP IN TRIAGETRACK AND OTHER APPS:
            int len = hpout.patientIdPrefix.Length;
            if (hpout.patientIdPrefix[len-1] != '-') // Force hyphen
                App.OrgPolicy.OrgPatientIdPrefixText += "-";
// END KLUDGE
            App.MyAssert(App.OrgPolicy.OrgPatientIdPrefixText != null);
            App.OrgPolicy.TriageZoneListJSON = hpout.triageZoneList;
            await App.ZoneChoices.ParseJsonList(hpout.triageZoneList);
            App.OrgPolicy.PhotoRequired = hpout.photoRequired;
            App.OrgPolicy.HonorNoPhotoRequest = hpout.honorNoPhotoRequest;
            App.OrgPolicy.PhotographerNameRequired = hpout.photographerNameRequired;
            App.OrgPolicyList.Clear();
            App.OrgPolicyList.Add(App.OrgPolicy); // kludgery
            return "";
        }
#if SETASIDE
        /// <summary>
        /// Returns the text of a hospital's legal statement to accompany email distributions of non-anonymized patient data. 
        /// </summary>
        public string GetHospitalLegalese(string hospital_uuid)
        {
            string errorCode;
            string errorMessage = "";
            string retValLegalese = "";

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                retValLegalese = App.pl.getHospitalLegalese(
                    hospital_uuid,
                    out errorCode,
                    out errorMessage);
            }
            catch (Exception e)
            {
                errorMessage = "ERROR: " + e.Message;
                return errorMessage;
            }
            if (errorCode != "0")
                return PackageErrorString(errorCode, errorMessage);

/* MAYBE:
            if (retValLegalese == null)
                return ChangeToErrorIfNull(null);
INSTEAD: */
            if (retValLegalese == null)
                retValLegalese = "";

            // Update Legalese file:  legaleseFileInfo = new FileInfo(pathTextAttachDir + "/restrictions.txt");
            if(parent.legaleseFileInfo.Exists)
                parent.legaleseFileInfo.Delete(); // overwrite with new info
            using (StreamWriter sw = parent.legaleseFileInfo.CreateText())
            {
                // Turn line separators into CRLF from LF (or any existing CRLF):
                retValLegalese = retValLegalese.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                sw.Write(retValLegalese);
                sw.Flush();
                sw.Close();
            }
            return "";
        }

        /// <summary>
        /// Returns the text of a hospital's legal statement to accompany email distributions of anonymized patient data. 
        /// </summary>
        public string GetHospitalLegaleseAnon(string hospital_uuid)
        {
            string errorCode;
            string errorMessage = "";
            string retValLegaleseAnon = "";

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                retValLegaleseAnon = App.pl.getHospitalLegaleseAnon(
                    hospital_uuid,
                    out errorCode,
                    out errorMessage);
            }
            catch (Exception e)
            {
                errorMessage = "ERROR: " + e.Message;
                return errorMessage;
            }
            if (errorCode != "0")
                return PackageErrorString(errorCode, errorMessage);
/* MAYBE:
            if (retValLegaleseAnon == null)
                return ChangeToErrorIfNull(null);
*/
            if (retValLegaleseAnon == null)
                retValLegaleseAnon = "";

            // Update Anonymized-Data Legalese file: anonymizedLegaleseFileInfo = new FileInfo(pathTextAttachDir + "/anonymized.txt");

            if (parent.anonymizedLegaleseFileInfo.Exists)
                parent.anonymizedLegaleseFileInfo.Delete(); // overwrite with new info
            using (StreamWriter sw = parent.anonymizedLegaleseFileInfo.CreateText())
            {
                // Turn line separators into CRLF from LF (or any existing CRLF):
                retValLegaleseAnon = retValLegaleseAnon.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
                sw.Write(retValLegaleseAnon);
                sw.Flush();
                sw.Close();
            }
            return "";
        }

        /// <summary>
        /// Returns the text of a hospital's legal statement to accompany email distributions of anonymized patient data. 
        /// </summary>
        public string GetHospitalLegaleseTimestamps(string hospital_uuid)
        {
            string errorCode;
            string errorMessage = "";
            string retValLegaleseTimestamp = "";
            string legaleseAnonTimestamp = "";

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                retValLegaleseTimestamp = App.pl.getHospitalLegaleseTimestamps(
                    hospital_uuid,
                    out legaleseAnonTimestamp,
                    out errorCode,
                    out errorMessage);
            }
            catch (Exception e)
            {
                errorMessage = "ERROR: " + e.Message;
                return errorMessage;
            }
            if (errorCode != "0")
                return PackageErrorString(errorCode, errorMessage);
/* MAYBE:
            if (retValLegaleseTimestamp == null)
                return ChangeToErrorIfNull(null);
*/
            // Add DistributeLegaleseTimestamps function here?
            return "";
        }
#endif
        private string PackageErrorString(string errorCode, string errorMessage)
        {
            if(String.IsNullOrEmpty(errorCode))
            {
                errorMessage =
                    "COMMUNICATIONS ERROR: Could not reach PL web service.\n\n" +
                    "This may be due to a DNS change to the PL machine.\n" +
                    "Admins: try rebooting machine, or from command line using ipconfig /flushdns\n" +
                    "TriagePic developer: try updating PLWS Service Reference.";

                // Caution: code elsewhere looks for "COMMUNICATIONS ERROR:" prefix.
                return errorMessage;
            }

            return "ERROR: " + errorCode.ToString() + " - " + errorMessage;
        }

        private string ChangeToErrorIfNull(string webServiceReturnValue)
        {
            // empty string is OK, but not null string.
            //Null string was observed if mismatch between DNS name given in WSDL, and where actual machine resolves to
            //Seen we first implementation of load balancing, where WSDL originally contained physical address of 1 of the service machines, instead of logical name
            if (webServiceReturnValue != null)
                return webServiceReturnValue;
            
            string errorMessage =
                "COMMUNICATIONS ERROR: Could not reach PL web service.\n\n" +
                "This may be due to a disagreement as to where the service is located.\n\n" +
                "PL and TriagePic developers: inspect WSDL for service location,\n" +
                "try revising WSDL and updating PLWS Service Reference.";

            // Caution: code elsewhere looks for "COMMUNICATIONS ERROR:" prefix.
            return errorMessage;
        }

        /// <summary>
        /// Report a Person, or update, via authenticated PL web service
        /// </summary>
        /// <param name="uuid">uuid if an update, or empty/null string otherwise</param>
        /// <param name="content">XML string</param>
        /// <returns>Empty string if OK, otherwise error message</returns>
        public async Task<string> ReportPerson(string content, string uuid)
        {
            // Win 7 code combined bodies of following calls, but harder in Win 8:
             if (String.IsNullOrEmpty(uuid))
                return await ReportPersonFirstTime(content);
            return await ReportPersonAgain(content, uuid);
       }


        /// <summary>
        /// Report a Person via authenticated PL web service
        /// </summary>
        /// <param name="content">XML string</param>
        /// <returns>Empty string if OK, otherwise error message</returns>
        private async Task<string> ReportPersonFirstTime(string content)
        {
            /* Win 7:
            string errorCode = "";
            string errorMessage = "";
            string url = "";
            string plname = parent.pd.DecryptPLUsername();
            string plpass = parent.pd.DecryptPLPassword(); */

            reportPersonRequest rpin = new reportPersonRequest();
            reportPersonResponse rpout = new reportPersonResponse();
            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                /* Win 7: url = App.pl.reportPerson(
                    content,
                    parent.os.EventShortNameText,
                    "TRIAGEPIC1",
                    plname,
                    plpass,
                    out errorCode,
                    out errorMessage);*/
                /* v32:
                rpin.username = App.pd.plUserName; // await App.pd.DecryptPL_Username();
                rpin.password = App.pd.plPassword; // await App.pd.DecryptPL_Password();
                rpin.xmlFormat = "TRIAGEPIC1";
                rpin.eventShortName = App.CurrentDisaster.EventShortName;
                rpin.personXML = content; */
                rpin.token = App.TokenPL;
                rpin.payload = content;
                rpin.payloadFormat = "TRIAGEPIC1";
                rpin.shortname = App.CurrentDisaster.EventShortName;
                rpout = await App.pl.reportPersonAsync(rpin);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            if (rpout.errorCode != "0")
                return PackageErrorString(rpout.errorCode, rpout.errorMessage);

            return ""; //Win 7: CAUSES PROBLEMS FOR CALLER: return url;
        }

       /// <summary>
        /// Update a Person report, via authenticated PL web service
        /// </summary>
        /// <param name="uuid">uuid if an update, or empty/null string otherwise</param>
        /// <param name="content">XML string</param>
        /// <returns>Empty string if OK, otherwise error message</returns>
        private async Task<string> ReportPersonAgain(string content, string uuid)
        {
            /* Win 7:
            string errorCode = "";
            string errorMessage = "";
            string url = "";
            string plname = parent.pd.DecryptPLUsername();
            string plpass = parent.pd.DecryptPLPassword(); */
            reReportPersonRequest rrpin = new reReportPersonRequest();
            reReportPersonResponse rrpout = new reReportPersonResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                /* Win 7:
                errorCode = App.pl.reReportPerson(
                    uuid,   
                    content,
                    parent.os.EventShortNameText,
                    "TRIAGEPIC1",
                    plname,
                    plpass,
                    out errorMessage);  */
                //rrpin.username = await App.pd.DecryptPL_Username();
                //rrpin.password = await App.pd.DecryptPL_Password();
                rrpin.token = App.TokenPL;
                rrpin.uuid = uuid;
                //rrpin.xmlFormat = "TRIAGEPIC1";
                rrpin.payloadFormat = "TRIAGEPIC1";
                //rrpin.eventShortname = App.CurrentDisaster.EventShortName;
                rrpin.shortname = App.CurrentDisaster.EventShortName;
                //rrpin.personXML = content;
                rrpin.payload = content;
                rrpout = await App.pl.reReportPersonAsync(rrpin);                 
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            if (rrpout.errorCode != "0")
                return PackageErrorString(rrpout.errorCode, rrpout.errorMessage);

            return ""; //Win 7: CAUSES PROBLEMS FOR CALLER: return url;
        }

        /// <summary>
        /// GetUuidFromPatientID calls TriageTrak's getUuidByMassCasualtyId function
        /// </summary>
        /// <returns>uuid, or empty string if not found or error</returns>
        // Has a different signature than Win 7: public async Task<string> GetUuidFromPatientID(string mcid, string shortEventName, out string uuid)
        // Throws away error reason, just returns empty string if error
        public async Task<string> GetUuidFromPatientID(string mcid, string shortEventName)
        {
            getUuidByMassCasualtyIdRequest guin = new getUuidByMassCasualtyIdRequest();
            getUuidByMassCasualtyIdResponse guout = new getUuidByMassCasualtyIdResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);
                guin.mcid = mcid;
                guin.shortname = shortEventName;
                //guin.username = await App.pd.DecryptPL_Username();
                //guin.password = await App.pd.DecryptPL_Password();
                guin.token = App.TokenPL;

                guout = await App.pl.getUuidByMassCasualtyIdAsync(guin);
                 
            }
            catch (Exception e)
            {
                string errorMessage = "ERROR: " + e.Message; // This line for debug breakpoint only
                return "";
            }
            if (guout.errorCode != "0")
            {
                string errorMessage = PackageErrorString(guout.errorCode, guout.errorMessage); // 9998, 9999 also possible.  This line for debug breakpoint only
                return "";
            }

/* MAYBE:
            if (uuid == null)
                return "";
*/

            return guout.uuid;
        }

#if SETASIDE
        /// <summary>
        /// IsPatientID_KnownToPL calls PL's getUuidByMassCasualtyId function.
        /// If we get back a uuid, return true;  Otherwise, return false (including from the expected error 407)
        /// </summary>
        /// <returns></returns>
        public bool IsPatientID_KnownToPL(string mcid, string shortEventName)
        {
            string errorCode = "";
            string errorMessage = "";
            string plname = parent.pd.DecryptPLUsername();
            string plpass = parent.pd.DecryptPLPassword();

            string uuid = "";
            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                uuid = App.pl.getUuidByMassCasualtyId( // Introduced PLUS 2.0
                    mcid,
                    shortEventName,
                    plname,
                    plpass,
                    out errorCode,  // expected: 0, 1, 2, 407 = No record associated with this mass casualty ID exists.
                    out errorMessage);

            }
            catch (Exception)
            {
                return false;
            }
            if (errorCode != "0" || uuid == "")
                return false; // 9998, 9999 also possible

/* MAYBE:
            if (uuid == null)
                return ChangeToErrorIfNull(null);
*/
            return true;
        }
#endif


        /// <summary>
        /// ExpirePearson calls PL's expirePerson function
        /// </summary>
        /// <returns></returns>
        public async Task<string> ExpirePerson(string uuid, string explanation)
        {
            // Owners, admins, and hospital staff admins can immediately expire a record.
            // Everyone else can submit an expiration request to a moderated queue.
            // A flag is set on a record once it is manually expired, stipulating that it
            // was requested to be expired vs. naturally expired (as requested by the original record author)
            expirePersonRequest epin = new expirePersonRequest();
            expirePersonResponse epout = new expirePersonResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                epin.uuid = uuid;
                epin.explanation = explanation;
                //epin.username = await App.pd.DecryptPL_Username();
                //epin.password = await App.pd.DecryptPL_Password();
                epin.token = App.TokenPL;

                epout = await App.pl.expirePersonAsync(epin);
            }
            catch (Exception e)
            {
                return "ERROR: " + e.Message;
            }
            bool queued = epout.queued; // Don't really care if its queued or not here.  This line just for debugger.
            if (epout.errorCode != "0")
                return PackageErrorString(epout.errorCode, epout.errorMessage); // 9998, 9999 also possible.

/* MAYBE:
            if (queued == null)
                return ChangeToErrorIfNull(null);
*/
            return "";
        }
#if SETASIDE

        
        public void DistributeHospitalPolicy(string patientIdPrefix, bool patientIdSuffixVariable, string patientIdSuffixFixedLength,
            bool photoRequired, bool honorNoPhotoRequest, bool photographerNameRequired)
        {
            //parent.PatientNumberPrefix.Text = 
            parent.hs.hp.OrgPatientID_PrefixText = parent.orgPatientID_Prefix.Text = patientIdPrefix;

            if (patientIdSuffixVariable == true)
            {
                parent.formatRadioButton2.Checked = true; // This may or may not fire CheckChanged
                // if it doesn't, do the essentials here:
                parent.hs.hp.OrgPatientID_FixedDigits = parent.patientID_FixedDigits = -1;  // Ignore patientIdSuffixFixedLength
                // parent.PatientNumberMaskedTextBox.Mask = ""; // We'll do our own character validation
            }
            else
            {
                // set prompt from "_" to "0", set MaskFormat to AllowPrompt
                parent.formatRadioButton1.Checked = true;
                parent.patientID_DigitsNumericUpDown1.Value = parent.patientID_FixedDigits
                    = parent.hs.hp.OrgPatientID_FixedDigits = Convert.ToInt32(patientIdSuffixFixedLength);

                // but we know the radio is now where we want it.
            }
 

            // TO DO:  Cache these...
            parent.photoRequiredCheckBox.Checked = photoRequired;
            parent.honorNoPhotoRequestCheckBox.Checked = honorNoPhotoRequest;
            parent.photographerNameRequiredCheckBox.Checked = photographerNameRequired;
        }
#endif
        /// <summary>
        /// Verifies PL credentials
        /// </summary>
        /// <returns>Error message or empty string</returns>
        public async Task<string> VerifyPLCredentials(string username, string password, bool hospitalStaffOrAdminOnly)
        {
            bool valid = false;
            string errorCode = "";
            string errorMessage = "";
            string exceptionMessage = "";
            //checkUserAuthHospitalRequest cuahin = new checkUserAuthHospitalRequest();
            //checkUserAuthHospitalResponse cuahout = new checkUserAuthHospitalResponse();
            //checkUserAuthRequest cuain = new checkUserAuthRequest();
            //checkUserAuthResponse cuaout = new checkUserAuthResponse();
            requestUserTokenRequest rutin = new requestUserTokenRequest();
            requestUserTokenResponse rutout = new requestUserTokenResponse();

            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

#if v32_OR_EARLIER
                if (hospitalStaffOrAdminOnly)
                {
                    cuahin.username = username;
                    cuahin.password = password;
                    cuahout = await App.pl.checkUserAuthHospitalAsync(cuahin);
                    valid = cuahout.valid;
                    errorCode = cuahout.errorCode;
                    errorMessage = cuahout.errorMessage;
                }
                else
                {
                    cuain.username = username;
                    cuain.password = password;
                    cuaout = await App.pl.checkUserAuthAsync(cuain);
                    valid = cuaout.valid;
                    errorCode = cuaout.errorCode;
                    errorMessage = cuaout.errorMessage;
                }
#endif
                rutin.username = username;
                rutin.password = password;
                rutout = await App.pl.requestUserTokenAsync(rutin);
                App.TokenPL = rutout.token;
                //MAYBE TO DO AS NEEDED: groupIdPL = rutout.groupIdPL;
                errorCode = rutout.errorCode;
                errorMessage = rutout.errorMessage;
            }

            catch (Exception e)
            {
                // Can't await (e.g., for MessageDialog) in catch, so just format string here.
                exceptionMessage =
                    "ERROR - Technical details:\n" +
                    e.Message + "\n";
                // Typical if web services are down, or value in OtherSettings.xml is wrong:
                // "There was no endpoint listening at https://plbreak.nlm.nih.gov/?wsdl&api=1.9.6 that could accept the message.
                // This is often caused by an incorrect address or SOAP action. See InnerException, if present, for more details."
                if (e.InnerException != null && e.InnerException.Message != null && e.InnerException.Message.Length > 0)
                    exceptionMessage +=
                        "More Technical Details (from Inner Exception):\n" +
                        e.InnerException.Message + "\n";
            }
            if (exceptionMessage.Length > 0)
            {
                // Caution: code elsewhere looks for "COMMUNICATIONS ERROR:" prefix.
                errorMessage =
                    "COMMUNICATIONS ERROR: When trying to authenticate PL credentials.\n\n" +
                    "Possible general causes:\n" +
                    "No wireless/wired network connection, PL web site is down, PL web services are disabled, or\n" +
                    "there is a wrong value in the OtherSetting.xml file.\n\n" +
                    "Want to see additional technical details?\n";

                bool showMore = false;
                var md = new MessageDialog(errorMessage);
                md.Commands.Add(new UICommand("Yes", (UICommandInvokedHandler) => { showMore = true; }));
                md.Commands.Add(new UICommand("No"));
                md.DefaultCommandIndex = 1; // No
                await md.ShowAsync();

                if (showMore)
                {
                    var dlg = new MessageDialog(exceptionMessage);
                    await dlg.ShowAsync();
                }
                return errorMessage + exceptionMessage;
            }

            if (errorCode != "0" || !valid)
                return PackageErrorString(errorCode, errorMessage);

/* MAYBE:
            if (valid == null)
                return ChangeToErrorIfNull(null);
*/
            return "";
        }
#if SETASIDE

        public string PrettyPrintXML(string XML) // Takes raw string
        {
            // From http bytes.com/topic/net/answers/179850-pretty-print-xml
            XmlDocument dom = new XmlDocument();
            dom.LoadXml(XML);
            XmlNodeReader xmlReader = new XmlNodeReader(dom);
            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
            xmlWriter.Formatting = System.Xml.Formatting.Indented;
            xmlWriter.Indentation = 1; // # of chars to indent
            xmlWriter.WriteNode(xmlReader, true);
            return stringWriter.ToString();
        }
#endif
        public string GetCleanedContentFromRawResults(string s)
        {
            // Content is between square brackets.  Also converts XML special chars.
            // Returns empty string if problem
            int n = s.IndexOf("[");
            if (n < 0)
                return ""; //Response missing contents beginning with square bracket.

            s = s.Substring(n);
            n = s.IndexOf("]");
            if (n < 0)
                return ""; // Response missing contents ending with square bracket.

            s = s.Substring(0,n+1);

            return RemoveEncodingFromXML(s);
        }

        /// <summary>
        /// For single-XML-field content.
        /// Replaces HTML-style encodings for ampersand, angle brackets, double and single straight quotes with ASCII chars.
        /// It's the opposite of reportPersonFormatting.MakeValueSafeForXML(s).
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public string RemoveEncodingFromXML(string s)
        {
            // Do XML decoding:
            // &amp; --> &
            // &lt; --> <
            // &gt; --> >
            // &quot; --> "
            // &apos; --> '
            s = s.Replace("&amp;", "&");
            s = s.Replace("&lt;", "<");
            s = s.Replace("&gt;", ">");
            s = s.Replace("&quot;", "\"");
            s = s.Replace("&apos;", "'");
            return s;
        }
#if SETASIDE
        // Take s, results of GetCleanedContentFromRawResults, and deserialize to Pls_Indident_Response_Row
        // NEEDS_JSON_NET
        // Adapted from http json.codeplex.com/Thread/View.aspx?ThreadID=61407
        // "Deserialize json array to Object array", July 2009 between rroestenburg, elishnevsky
        //Pls_Incident_Response_Row[] oa = (Pls_Incident_Response_Row[])JsonConvert.DeserializeObject(jstr, Pls_Incident_Response_Row[])));
        //rows = JsonConvert.DeserializeObject<List<Pls_Incident_Response_Row>>(s);

        public string PrettyPrintIncidentResponseRows(List<Pls_Incident_Response_Row> rows)
        {
            // For message box, so uses \r\n line breaks
            string t;
            t = "incident_id\tparent_id\tname\tshortname\tdate\ttype\tlatitude\tlongitude\r\n";
            for (int i = 0; i < rows.Count; i++)
            {
                t += rows[i].incident_id + "\t" +
                    rows[i].parent_id + "\t" +
                    rows[i].name + "\t" +
                    rows[i].shortname + "\t" +
                    rows[i].date + "\t" +
                    rows[i].type + "\t" +
                    rows[i].latitude + "\t" +
                    rows[i].longitude + "\t" +
                    rows[i].street + "\t" + // added v 1.30
                    rows[i].closed + "\r\n"; // ditto
            }
            return t;
        }
#endif
        /// <summary>
        /// Remove events that are closed to reporting.
        /// </summary>
        /// <param name="inrows"></param>
        /// <returns></returns>
        public List<Pls_Incident_Response_Row> FilterIncidentResponseRows(List<Pls_Incident_Response_Row> inrows)
        {
            // When we get here, rows are already restricted to just events the current PL user credentials can see.
            List<Pls_Incident_Response_Row> rows;
            rows  = inrows;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                if (rows[i].closed != "0")
                    rows.RemoveAt(i);
            }
            return rows;
        }
#if SETASIDE

        public List<string> FormatNameForCombo(List<Pls_Incident_Response_Row> rows)
        {
           List<string> names = new List<string>();
           for (int i = 0; i < rows.Count; i++)
            {
                switch (rows[i].type)
                {
                    // DRILL phased out, now part of TEST
                    case "Drill": // Old
                    case "DRILL":
                        //names.Add(rows[i].name + " - " + rows[i].type);
                        //break;
                    case "Non-Event": // Old
                    case "TEST or DEMO": // Old
                    case "TEST":
//                        if(rows[i].name == "Test Event")
//                            names.Add("Unnamed TEST or DEMO");
//                        else
//                            names.Add(rows[i].name + " - " + rows[i].type);
                        names.Add(rows[i].name + " - TEST/DEMO/DRILL");
                        break;
                    case "REAL":
                    default: // Real events
                            names.Add(rows[i].name + " - REAL - NOT A DRILL");
                        break;
                }
            }
           return names;
        }

        public List<string> FormatShortName(List<Pls_Incident_Response_Row> rows)
        {
            List<string> names = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
               names.Add(rows[i].shortname);
            }
            return names;
        }


        public List<string> FormatSuffixForCombo(List<Pls_Incident_Response_Row> rows)
        {
            List<string> suffixes = new List<string>();
            for (int i = 0; i < rows.Count; i++)
            {
                switch (rows[i].type)
                {
                    case "Drill": // Old
                    case "DRILL":
                    // DRILL phased out, now part of TEST
                        //suffixes.Add("DRILL");
                        //break;
                    case "Non-Event": // Old
                    case "TEST or DEMO":  // Old
                    case "TEST":
                        //suffixes.Add("TEST or DEMO");
                        suffixes.Add("TEST/DEMO/DRILL");
                        break;
                    default: // Real events
                    case "REAL":
                        suffixes.Add("REAL - NOT A DRILL");
                        break;
                }
            }
            return suffixes;
        }



        // if we pull the JSON out of XML, then can deserialize with WCF by:
        // using System.Web.Script.Serialization (.Net 3.5 SP 1 System.Web.Extension.dll)
        // JavaScriptSerializer jSer = new JavaScriptSerializer();
        // abstract code: BusinessObjectType bo = jSer.Deserialize<BusinessObjectType>(XML);
        // More: http stackoverflow.com/questions/401756/parsing-json-using-json-net [but reader Marc Gravell gives non-json.net way]

        // Here, we're caching using List object:

        public List<Pls_Incident_Response_Row> ReadEventListFromFile()
		{
            parent.newEventListFileCreated = false; // Assume file exists until we know better
			if ( !File.Exists( event_list_path ))
		    {
                parent.newEventListFileCreated = true;
                WriteEventListToFile(); // Write out class elements as initialized above.
			}
            XmlSerializer ser = new XmlSerializer(typeof(List<Pls_Incident_Response_Row>));
            return (List<Pls_Incident_Response_Row>)ser.Deserialize(new XmlTextReader(event_list_path));
        }

        public void WriteEventListToFile()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<Pls_Incident_Response_Row>));
            StreamWriter wr = new StreamWriter(event_list_path);
            ser.Serialize(wr, parent.incidentResponseRows);
            wr.Flush();
            wr.Close();
        }

        public List<TriagePic.JsonType.Hospital> ReadHospitalListFromFile()
        {
            parent.newHospitalListFileCreated = false; // Assume file exists until we know better
            if (!File.Exists(hospital_list_path))
            {
                parent.newHospitalListFileCreated = true;
                WriteHospitalListToFile(); // Write out class elements as initialized above.
            }
            // was before 1.62:
            //XmlSerializer ser = new XmlSerializer(typeof(List<TriagePic.JsonType.Hospital>));
            //return (List<TriagePic.JsonType.Hospital>)ser.Deserialize(new XmlTextReader(hospital_list_path));

            // Try to prevent conflict over xml file access later in WriteHospitalListTofile:
            List<TriagePic.JsonType.Hospital> o = new List<TriagePic.JsonType.Hospital>();
            XmlSerializer ser = new XmlSerializer(typeof(List<TriagePic.JsonType.Hospital>));
            XmlTextReader xtr = new XmlTextReader(hospital_list_path);
            try
            {
                o = (List<TriagePic.JsonType.Hospital>)ser.Deserialize(xtr);
            }
            catch (Exception ex)
            {
                ErrBox.Show(
                    "When try to read HospitalSettings.xlm:\n" +
                    "Message ---\n" + ex.Message +
                    "\nHelpLink ---\n" + ex.HelpLink +
                    "\nSource ---\n" + ex.Source +
                    "\nStackTrace ---\n" + ex.StackTrace +
                    "\nTargetSite ---\n" + ex.TargetSite);
            }

            xtr.Close();
            return o;
        }

        public void WriteHospitalListToFile()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<TriagePic.JsonType.Hospital>));
            StreamWriter wr = new StreamWriter(hospital_list_path);
            ser.Serialize(wr, parent.hospitals);
            wr.Flush();
            wr.Close();
        }
#endif

        // NEW WITH Win 8 - Calls to PL Search functions:
        /// <summary>
        /// Gets all reports for the current event, independent of submitter or submitting organization
        /// </summary>
        public async Task<string> GetReportsFromAllStationsCurrentEvent(string userPL, string passwordPL)
        {
            //searchWithAuthRequest sarin = new searchWithAuthRequest(); // Before V31: named searchCompleteWithAuth
            //searchWithAuthResponse sarout = new searchWithAuthResponse();
            searchRequest srin = new searchRequest();
            searchResponse srout = new searchResponse();
            //sarin.username = userPL;
            //sarin.password = passwordPL;
            srin.token = App.TokenPL;
            //sarin.eventShortname = App.CurrentDisaster.EventShortName;
            // v33, replace "sarin" with "srin" many times below
            srin.eventShortname = App.CurrentDisaster.EventShortName;
            srin.filterAgeAdult = srin.filterAgeChild = srin.filterAgeUnknown = true;
            srin.filterGenderMale = srin.filterGenderFemale = srin.filterGenderComplex = srin.filterGenderUnknown = true;
/* NO.  Always get all records here.  Filter later
            if(App.OutboxCheckBoxMyOrgOnly)
            {
                string Name = App.CurrentOrgContactInfo.OrgName;
                string Uuid = App.OrgDataList.GetOrgUuidFromOrgName(Name);
                if (Uuid == "") // couldn't find match
                {
                    App.MyAssert(false);
                    //Uuid = App.OrgDataList.First().OrgUuid;
                    //Name = App.OrgDataList.First().OrgName;
                }
                sarin.filterHospital = Uuid;
            }
            else
            {
                // spec says: sarin.filterHospital = ""; // empty = don't filter on orgs; otherwise, [but not implemeneted] comma-separated org shortnames
                // What Greg chose to implement instead is single hospital uuid  */
                srin.filterHospital = "all"; // temporary workaround
            //}
            // WAS before V32: sarin.filterHospitalSH = sarin.filterHospitalWRNMMC = sarin.filterHospitalOther = true;
            srin.filterStatusAlive = srin.filterStatusInjured = srin.filterStatusDeceased = srin.filterStatusMissing = srin.filterStatusUnknown = srin.filterStatusFound = true;
            srin.filterHasImage = false; // true would return ONLY reports with images
            srin.pageStart = "0";
            srin.perPage = "250"; // 1000 gave out of memory problems
            srin.sortBy = ""; // = updated desc, score desc
            srin.searchTerm = "";
            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                // Win 7: responseData = App.pl.getEventListUser(userPL, passwordPL, out errorCode, out errorMessage);
                //sarout = await App.pl.searchWithAuthAsync(sarin);
                srout = await App.pl.searchAsync(srin);
            }
            catch (Exception e)
            {
                // WAS: sarout
                srout.resultSet = "ERROR: " + e.Message; // Win 7: responseData = "ERROR: " + e.Message;
            }

            return ChangeToErrorIfNull(srout.resultSet); // WAS before v33: sarout
        }


        // NEW WITH Win 8 - Calls to PL Search functions:
        /// <summary>
        /// Gets all reports for the current event, from the current hospital.  Caller must filter more, to narrow.
        /// </summary>
        public async Task<string> GetReportsForOutbox(string userPL, string passwordPL)
        {
            //searchWithAuthRequest sarin = new searchWithAuthRequest(); // Before V31: named searchCompleteWithAuth
            //searchWithAuthResponse sarout = new searchWithAuthResponse();
            searchRequest srin = new searchRequest();
            searchResponse srout = new searchResponse();
            srin.token = App.TokenPL;
            //sarin.username = userPL;
            //sarin.password = passwordPL;
            // "sarin" replaced by "srin", "sarout" replaced by "srout" many places below
            srin.eventShortname = App.CurrentDisaster.EventShortName;
            srin.filterAgeAdult = srin.filterAgeChild = srin.filterAgeUnknown = true;
            srin.filterGenderMale = srin.filterGenderFemale = srin.filterGenderComplex = srin.filterGenderUnknown = true;
            string Name = App.CurrentOrgContactInfo.OrgName;
            string Uuid = App.OrgDataList.GetOrgUuidFromOrgName(Name);
            if (Uuid == "") // couldn't find match
            {
                App.MyAssert(false);
                //Uuid = App.OrgDataList.First().OrgUuid;
                //Name = App.OrgDataList.First().OrgName;
            }
            srin.filterHospital = Uuid; // instead of "" or "all" = don't filter on orgs.
            // WAS before v32: sarin.filterHospitalSH = sarin.filterHospitalWRNMMC = sarin.filterHospitalOther = true;
            srin.filterStatusAlive = srin.filterStatusInjured = srin.filterStatusDeceased = srin.filterStatusMissing = srin.filterStatusUnknown = srin.filterStatusFound = true;
            srin.filterHasImage = false;  // true would return ONLY reports with images
            srin.pageStart = "0";
            srin.perPage = "250"; // "1000";  If you change this, change it too in TP_PatientReportsSource.cs
            srin.sortBy = ""; // = updated desc, score desc
            srin.searchTerm = "";
            try
            {
                SetPLEndpointAddress(App.pl); //read the configured endpoint address

                if(App.BlockWebServices)
                    throw new Exception(PHONY_COMMUNICATIONS_EXCEPTION);

                // Win 7: responseData = App.pl.getEventListUser(userPL, passwordPL, out errorCode, out errorMessage);
                //sarout = await App.pl.searchWithAuthAsync(sarin);
                srout = await App.pl.searchAsync(srin);
            }
            catch (Exception e)
            {
                srout.resultSet = "ERROR: " + e.Message; // Win 7: responseData = "ERROR: " + e.Message;
            }

            return ChangeToErrorIfNull(srout.resultSet);
        }
    }



    // For JSON.NET
    // We will cache these values, and distribute an initial set for fresh installs as well

    /// <summary>
    /// Pls_Incident_Response_Row (in namespace LPF_SOAP and file LPF_SOAP.cs) is a helper data structure,
    /// representing 1 "row" of the response of shn_pls_getIncidentList web call.
    /// </summary>
    // Win 7: [Serializable]
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
            // Zero means open, 1 is closed to all reporting, 2 means reporting is only allowed to happen externally (like Google PF).
    }

    // In Win 7 TriagePic, next structure was in a separate JsonType.cs file and TiragePic.JsonType namespace

    public class Hospital
    {
        public string hospital_uuid { get; set; }
        public string npi { get; set; }
        public string name { get; set; }
        public string shortname { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
    }

    public class HospitalPolicyTriageZone
    {
        public string name { get; set; }
        public string description { get; set; }
        public string button_color_rgb { get; set; }
        public string button_color_name { get; set; }
        public string sahana_status { get; set; }
    }

// New TP8:

    public class Search_Response_Tag
    {
        public string tag_id { get; set; } // int in string, e.g. "44"
        public string tag_x { get; set; } // all are int in string, e.g., "0"
        public string tag_y { get; set; }
        public string tag_w { get; set; }
        public string tag_h { get; set; }
        public string tag_text { get; set; } // e.g., "this is a caption"
    }

    public class Search_Response_Image
    {
        public string note_record_id { get; set; } // can be null // Int or string? assume string
        public string image_id { get; set; }  // int in string, e.g., "86464"
        public string image_type { get; set; }  // e.g., "jpeg"
        public string image_height { get; set; } // int in string, e.g., "540"
        public string image_width { get; set; } // int in string, e.g., "960"
        public string created { get; set; } // e.g., "2013-08-12 18:31:53"
        public string url { get; set; } // e.g., "tmp/plus_cache/triagetrak.nlm.nih.govSLASHperson.3099294__86464_full.png"
        // TriageTrak-generated thumbnails are fixed-width:
        public string url_thumb { get; set; } // e.g., "tmp/plus_cache/triagetrak.nlm.nih.govSLASHperson.3099294__86464_thumb.png"
        // original_filename is a temporary name, not a valid URL
        public string original_filename { get; set; } // e.g., "4000 Green.jpg", "content://media/external/images/media/12144", "/data/data/com.pl.triagepic/app_Pictures/1376346499708.png"
        public string principal { get; set; } // int in string, e.g., "1"
        public string sha1original { get; set; } // e.g., "a2615ab024edac8675a908b383d5822f2ac824c7"
        public string color_channels { get; set; } // int in string, e.g., 3
        // note_id of 0 and note_record_id of <null> means the photo is part of the main record, not a ReUnite comment.
        public int note_id { get; set; } // e.g., 0
        public List<Search_Response_Tag> tags { get; set; }
    }


    public class Search_Response_Location
    {
        public string street1 { get; set; }  // can be null
        public string street2 { get; set; }  // can be null
        public string neighborhood { get; set; }  // can be null
        public string city { get; set; } // can be null
        public string region { get; set; } // can be null
        public string postal_code { get; set; } // can be null
        public string country { get; set; } // can be null
        public Search_Response_GPS gps { get; set; }
    }

    public class Search_Response_GPS
    {
        public string latitude { get; set; }  // both can be null
        public string longitude { get; set; }
    }


    public class Search_Response_Person_Notes
    {
        public string note_id { get; set; } // int in string, e.g., "1"
        public string note_about_p_uuid { get; set; } // e.g., "10.0.0.29/ceb-vesuvius/vesuvius/www/person.4189"
        public string note_written_by_p_uuid { get; set; } // int in string, e.g., "1"
        public string note_written_by_name { get; set; } // can be null
        public string note { get; set; } // e.g. "a"
        public string when { get; set; } // e.g., "2013-11-14T23:06:09-06:00
        public string suggested_status { get; set; } // e.g., "dec"
        public Search_Response_Location suggested_location { get; set; }
        public string pfif_note_id { get; set; } // can be null
    }
// In theory, not used with TriageTrak, only ReUnite/PL
    public class Search_Response_Voice_Notes
    {
    }

    public class Search_Response_EDXL
    {
        // Next 4 only available in TriageTrak response, not PL
        // Next 2 have security implications, so not desired with ReUnite (e.g., response from PL)
        public string last_posted { get; set; }  // time reported by client on either report or re-report.  e.g. "2013-11-14 21:20:16"
        // If reported from web site, next 2 might be "NIH\miernickig" and "n/a"
        public string login_account { get; set; } // From EDXL.login_account.  Also available as users.user_name
        public string login_machine { get; set; } // From EDXL.login_machine.
        public string mass_casualty_id { get; set; }
        public string triage_category { get; set; } // e.g., "Green"
        public string sender_id { get; set; }
        public string distr_id { get; set; } // Full name: distribution_id.  e.g., "NPI 1234567890 2011-08-08T16:20:00z
    }

    public class Search_Response_Toplevel_Row
    {
        // In order as generated by TriageTrak
        public string p_uuid { get; set; } // e.g., "triagetrak.nlm.nih.gov/person.3099294"
        public string full_name { get; set; } //can be null
        public string family_name { get; set; }  // can be null
        public string given_name { get; set; } // can be null
        public string alternate_names { get; set; } // can be null
        public string profile_urls { get; set; } // can be null
        public string incident_id { get; set; } // positive integer in string
        public string hospital_uuid { get; set; } // positive integer in string.  SOON: contained in response from TriageTrak only, not PL
        public string expiry_date { get; set; }  // can be null
        public string opt_status { get; set; } // Sahana code, e.g., "ali"
        public string last_updated { get; set; } // "2013-08-12T18:31:50-04:00"
        public string creation_time { get; set; } // "2013-08-12T18:29:54-04:00"
        public Search_Response_Location location { get; set; }  // can be null
        public string birth_date { get; set; } // can be null
        public string opt_race { get; set; } // can be null
        public string opt_religion { get; set; } // Sahana code, can be null
        public string opt_gender { get; set; } // Sahana code, e.g., "mal"
        public string years_old { get; set; } // can be null.  Assume int in string
        public string min_age { get; set; } // e.g., "18"
        public string max_age { get; set; }  // e.g., "150"
        public string last_seen { get; set; } // e.g., "NLM (testing)";
        public string last_clothing { get; set; } // can be null
        public string other_comments { get; set; } // e.g., "LPF notification - disaster victim arrves at hospital triage station"
        public string rep_uuid { get; set; } // positive int in string, e.g., "1"
        public string reporter_username { get; set; } // e.g., "hs"
        public List<Search_Response_Image> images { get; set; }
        public List<Search_Response_Voice_Notes> voice_notes { get; set; } // empty array
        public List<Search_Response_Person_Notes> person_notes { get; set; }
        public List<Search_Response_EDXL> edxl { get; set; }
    }

}
