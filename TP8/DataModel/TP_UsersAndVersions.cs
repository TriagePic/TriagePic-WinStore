﻿#define GENERATE_CHOICES
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


    public class TP_UserNameAndVersion
    {
        private String _user; // Win domain\name
        private String _version; // Win8 TriagePic major.minor build
        private String _plUsername; // base64 encoded and encrypted.
        private String _plPassword; // base64 encoded and encrypted.

        public TP_UserNameAndVersion()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="button_name"></param>
        /// <param name="meaning"></param>
        /// <param name="color_name"></param>
        /// <param name="color_obj"></param>
        public TP_UserNameAndVersion(String user_name, String version, String pl_user_name, String pl_password)
        {
            User = user_name;
            Version = version;
            plUsername = pl_user_name;
            plPassword = pl_password;
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
            inner.Add(new TP_UserNameAndVersion("LastCompatibleVersion","3.00","","")); // Like Win 7 version.  Don't know yet if this makes any sense.) {
            await WriteXML();
        }

        public async Task ProcessUsersAndVersions()
        {
            await ReadXML();
            // Purge any bad/useless records:
            bool altered = false;
            bool innerbreak = false;
            // A little trickiness of get around IEnumerable restrictions:
            while (true)
            {
                foreach (var x in inner)
                {
                    if (String.IsNullOrEmpty(x.User) ||
                        // String.IsNullOrEmpty(x.Version) ||
                        (x.User != "LastCompatibleVersion" && (String.IsNullOrEmpty(x.plUsername) || String.IsNullOrEmpty(x.plPassword))))
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
            if (altered)
            {
                await WriteXML();
            }
            TP_UserNameAndVersion o = new TP_UserNameAndVersion();
            o.User = App.UserWin8Account;
            int index = inner.FindIndex(i => i.User == o.User); // C# 3.0 lambda expression
            if (index >= 0) // TO DO: multiple PL accounts for same Win login?
            {
                // We already have credentials from local file, just need to decrypt them
                if (String.IsNullOrEmpty(App.pd.plUserNameEncryptedAndBase64Encoded) || String.IsNullOrEmpty(App.pd.plPasswordEncryptedAndBase64Encoded))
                {
                    App.pd.plUserNameEncryptedAndBase64Encoded = inner[index].plUsername;
                    App.pd.plPasswordEncryptedAndBase64Encoded = inner[index].plPassword;
                    // If decryption fails, user gets a message.  otherwise pd.plUserName, pd.plPassword are filled in
                    await App.pd.DecryptPL_Username();
                    await App.pd.DecryptPL_Password();
                    // TO DO - BETTER ERROR RECOVERY
                }
            }
            else
            {
                // HARD-CODED - TO DO - GET FROM USER
                App.pd.plUserName = "hs";
                App.pd.plPassword = "hStaff2011";
                App.pd.EncryptAndBase64EncodePLCredentials(); // assigns App.pd.plUserNamePasswordEncryptedAndBase64Encoded, App.pd.plPasswordEncryptedAndBase64Encoded
                o.plUsername = App.pd.plUserNameEncryptedAndBase64Encoded;
                o.plPassword = App.pd.plPasswordEncryptedAndBase64Encoded;
                o.Version = "3.00"; // TO DO - GET VERSION FROM COMPILER
                Add(o);
                await WriteXML();
            }

        }

        public async Task ReadXML()
        {
            await ReadXML(USERS_AND_VERSIONS_FILENAME, true);
        }

        public async Task ReadXML(string filename, bool clearFirst)
        {
            if (clearFirst)
                Clear();
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_UserNameAndVersion>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_UserNameAndVersion);
                }
            }
        }

        public async Task WriteXML()
        {
            await WriteXML(USERS_AND_VERSIONS_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_UserNameAndVersion);

            await LocalStorage.Save<TP_UserNameAndVersion>(filename);
        }
    }

}