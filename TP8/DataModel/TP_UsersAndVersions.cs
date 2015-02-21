#define GENERATE_CHOICES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TP8.Common;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TP8.Data // nah .DataModel
{


    public class TP_UserNameAndVersion
    {
        private String _user; // Win domain\name
        private String _version; // Win8 TriagePic major.minor build
        private String _plUsername; // base64 encoded and encrypted.
        private String _plPassword; // base64 encoded and encrypted.
        private String _plToken; // encrypted.  Added Dec 2014

        public TP_UserNameAndVersion()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="user_name"></param>
        /// <param name="version"></param>
        /// <param name="pl_user_name"></param>
        /// <param name="pl_password"></param>
        /// <param name="pl_token"></param>
        public TP_UserNameAndVersion(String user_name, String version, String pl_user_name, String pl_password, String pl_token) // pl_token added Dec 2014
        {
            User = user_name;
            Version = version;
            plUsername = pl_user_name;
            plPassword = pl_password;
            plToken = pl_token;
        }

        [XmlAttribute]
        public String User
        {
            get { return this._user; }
            set { this._user = value; }
        }
        public String Version
        {
            get { return this._version; }
            set { this._version = value; }
        }
        public String plUsername
        {
            get { return this._plUsername; }
            set { this._plUsername = value; }
        }
        public String plPassword
        {
            get { return this._plPassword; }
            set { this._plPassword = value; }
        }
        public String plToken
        {
            get { return this._plToken; }
            set { this._plToken = value; }
        }
    }


    [XmlType(TypeName = "UsersAndVersions")] // Compare Win 7 HospitalList.xml
    public class TP_UsersAndVersions : IEnumerable<TP_UserNameAndVersion>
    {
        const string USERS_AND_VERSIONS_FILENAME = "UsersAndVersions.xml";

        private List<TP_UserNameAndVersion> inner = new List<TP_UserNameAndVersion>();

        public void Add(object o)
        {
            inner.Add((TP_UserNameAndVersion)o);
        }

        public void Remove(TP_UserNameAndVersion o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_UserNameAndVersion> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_UserNameAndVersion> list)
        {
            inner = list;
        }

        public IEnumerator<TP_UserNameAndVersion> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void UpdateOrAdd(TP_UserNameAndVersion o)
        {
            int index = inner.FindIndex(i => i.User == o.User); // C# 3.0 lambda expression
            if (index >= 0)
                inner[index] = o; // update
            else
                Add(o);
        }

        public async Task Init()
        {
            if (!await DoesFileExistAsync())
                await GenerateDefaultUserNameAndVersion();

            await ProcessUsersAndVersions();
        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(USERS_AND_VERSIONS_FILENAME);
        }

        private async Task GenerateDefaultUserNameAndVersion()
        {
            inner.Add(new TP_UserNameAndVersion("LastCompatibleVersion","1.00","","","")); // Like Win 7 version.  Don't know yet if this makes any sense.) {
            await WriteXML();
        }

        public async Task ProcessUsersAndVersions()
        {
            // Rewritten Dec 2014
            await ReadXML();
            bool altered = PurgeBadOrUselessRecords();
            if (altered)
                await WriteXML();

            TP_UserNameAndVersion o = new TP_UserNameAndVersion();
            o.User = App.UserWin8Account;
            int index = inner.FindIndex(i => i.User == o.User); // C# 3.0 lambda expression
            if (index < 0)
            {
                // No pre-existing credentials.
                await ShowStartupWiz();  // This should set App.pd.plToken, call OrgDataList.Init()
                await AddEncryptedDataRowInXML(o);
                return;
            }

            // If we already have all the credentials, don't need to process what's in file.  IS THIS A GOOD IDEA?
            if (App.pd.plUserNameEncryptedAndBase64Encoded.Length > 0 && App.pd.plUserName.Length > 0 &&
                App.pd.plPasswordEncryptedAndBase64Encoded.Length > 0 && App.pd.plPassword.Length > 0 &&
                App.pd.plTokenEncryptedAndBase64Encoded.Length > 0 && App.pd.plToken.Length == 128) // New Dec 2014.
                return;

            // We already have credentials from local file, just need to decrypt them
            // TO DO: multiple PL accounts for same Win login?
            // Probably needs more error handling refinement
            App.pd.plUserNameEncryptedAndBase64Encoded = inner[index].plUsername;
            App.pd.plPasswordEncryptedAndBase64Encoded = inner[index].plPassword;
            // If decryption fails, user gets a message.  otherwise pd.plUserName, pd.plPassword are filled in
            await App.pd.DecryptPL_Username();
            await App.pd.DecryptPL_Password();
            if (inner[index].plToken.Length > 0)
            {
                App.pd.plTokenEncryptedAndBase64Encoded = inner[index].plToken;
                // If decryption fails, user gets a message.  Otherwise, pd.plToken is filled in.
                await App.pd.DecryptPL_Token();
                await App.OrgDataList.Init(); // uses plToken
                return;
            }

            string results = await App.service.GetUserToken();
            if (!results.Contains("ERROR") && results.Length == 128) // token is 128 char long SHA-512
            {
                App.pd.plToken = results;
                await App.OrgDataList.Init(); // uses plToken
                return;
            }

            // TO DO - BETTER ERROR RECOVERY. FIRST PASS: 
            App.pd.plToken = "";
            await ShowStartupWiz();  // This should set App.pd.plUserName, App.pd.plPassword, App.pd.plToken, call OrgDataList.Init()
            App.MyAssert(App.pd.plToken.Length == 128); // token is 128 char long SHA-512
            App.MyAssert(App.pd.plUserName.Length > 0);
            App.MyAssert(App.pd.plPassword.Length > 0);
            // TO DO: handling case if user cancels startup wizard
            await AddOrUpdateEncryptedDataRowInXML(o);
        }

        /// <summary>
        /// Purge from memory any bad or useless UsersAndVersions.XML records.  Caller is response for saving purge to file.
        /// </summary>
        /// <returns>whether any records were purged</returns>
        private bool PurgeBadOrUselessRecords() // Broken out as separate function Dec 2014
        {
            bool altered = false;
            bool innerbreak = false;
            // A little trickiness of get around IEnumerable restrictions:
            while (true)
            {
                foreach (var x in inner)
                {
                    if (x.User == null ||
                        // was: String.IsNullOrEmpty(x.User) ||   ... but maybe empty string if user doesn't give apps permission to get Win 8 user name.  Changed May, 2014.  
                        // String.IsNullOrEmpty(x.Version) ||
                        (x.User != "LastCompatibleVersion" && (String.IsNullOrEmpty(x.plUsername) || String.IsNullOrEmpty(x.plPassword))) ||
                        (x.User != null && (String.IsNullOrEmpty(x.plUsername) || String.IsNullOrEmpty(x.plPassword))) // Pathological case if user toggles permission for apps to use user name
                        )
                    {
                        inner.Remove(x);
                        altered = true;
                        innerbreak = true;
                        break;
                    }
                }
                if (innerbreak)
                    innerbreak = false; // then do another pass through loop.  Inefficient, but this is a short list
                else
                    break;
            }
            return altered;
        }

        private async Task ShowStartupWiz()
        {
            var sw = new StartupWizUserControl.StartupWiz();
            var results = await sw.ShowAsync();
            if (results == 0)
                return; // user cancelled.
        }

        public async Task AddEncryptedDataRowInXML(TP_UserNameAndVersion u)
        {
            await App.pd.EncryptAndBase64EncodePLCredentialsAsync(); // assigns App.pd.plUserNamePasswordEncryptedAndBase64Encoded, App.pd.plPasswordEncryptedAndBase64Encoded
            u.plUsername = App.pd.plUserNameEncryptedAndBase64Encoded;
            u.plPassword = App.pd.plPasswordEncryptedAndBase64Encoded;
            u.plToken = App.pd.plTokenEncryptedAndBase64Encoded; // New Dec 2014
            u.Version = "3.00"; // TO DO - GET VERSION FROM COMPILER
            Add(u);
            await WriteXML();
        }

        public async Task AddOrUpdateEncryptedDataRowInXML(TP_UserNameAndVersion u)
        {
            // A little trickiness of get around IEnumerable restrictions.  Delete then add back:
            foreach (var x in inner)
            {
                if (x.User == u.User)
                {
                    inner.Remove(x);
                    break;
                }
            }
            await AddEncryptedDataRowInXML(u);
        }

        //private void Wiz_Click(object sender, RoutedEventArgs e)
        //{
        //    wizPopUp.IsOpen = false;
        //}

        public async Task ReadXML()
        {
            await ReadXML(USERS_AND_VERSIONS_FILENAME, true);
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_UserNameAndVersion>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_UserNameAndVersion);
                }
            }
            App.LocalStorageDataSemaphore.Release();
        }

        public async Task WriteXML()
        {
            await WriteXML(USERS_AND_VERSIONS_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_UserNameAndVersion);

            await LocalStorage.Save<TP_UserNameAndVersion>(filename);
            App.LocalStorageDataSemaphore.Release();
        }
    }

}
