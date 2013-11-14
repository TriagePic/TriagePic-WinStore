using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Security.Principal;
using CodeProject.Dialog;

namespace TriagePicNamespace
{
    public class ProtectData
    {
        private FormTriagePic parent;

        public ProtectData(FormTriagePic p)
        {
            parent = p;
        }

        /// <summary>
        /// Encrypts the PL password for the current windows user and returns the encrypted bytes
        /// </summary>
        public static byte[] Protect(byte[] data)
        {
            try
            {
                //Encrypt the data using DataProtectionScope.CurrentUser. 
                //The result can be decrypted only by the same current user.
                return ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException /*e*/)
            {
                // Could look at e.Message, but instead let caller report generic message
                // Console.WriteLine("Data was not encrypted. An error occurred.");
                // Console.WriteLine(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Decrypts the PL password for the current windows user and returns the decrypted bytes
        /// </summary>
        public static byte[] Unprotect(byte[] data)
        {
            try
            {
                //Decrypt the data using DataProtectionScope.CurrentUser.
                return ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser);
            }
            catch (CryptographicException /*e*/)
            {
                // Could look at e.Message, but instead let caller report generic message
                // Console.WriteLine("Data was not decrypted. An error occurred.");
                // Console.WriteLine(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Encrypts the PL username and password and returns them as base64 encoded strings
        /// </summary>
        public bool GetEncryptedPLCredentials(ref string plUsername, ref string plPassword)
        {
            string plUser = plUsername;
            string plPass = plPassword;

            //encrypt username
            byte[] bytesToEncrypt = Encoding.ASCII.GetBytes(plUser);
            byte[] encryptedBytesToEncode = Protect(bytesToEncrypt);
            if (encryptedBytesToEncode == null)
            {
                ErrBox.Show("Current Windows user could not encrypt PL user name.");
                plUsername = "";
                return false;
            }
            plUsername = Convert.ToBase64String(encryptedBytesToEncode);

            //encrypt password
            bytesToEncrypt = Encoding.ASCII.GetBytes(plPass);
            encryptedBytesToEncode = Protect(bytesToEncrypt);
            if (encryptedBytesToEncode == null)
            {
                ErrBox.Show("Current Windows user could not encrypt PL password.");
                plPassword = "";
                return false;
            }
            plPassword = Convert.ToBase64String(encryptedBytesToEncode);

            return true;
        }

        /// <summary>
        /// Decrypts the PL username blob
        /// </summary>
        public string DecryptPLUsername()
        {
            //decrypt username
            if (!String.IsNullOrEmpty(parent.plUsername)) // check for null too v 1.47
            {
                byte[] bytesToDecrypt = Convert.FromBase64String(parent.plUsername);
                byte[] usernameBytes = Unprotect(bytesToDecrypt);
                if (usernameBytes == null)
                {
                    ErrBox.Show(
                        "The saved value of your PL user name could not be decrypted.\n" +
                        "(This can happen if certain TriagePic settings were hand-edited,\n" +
                        "or copied from another Windows machine.)");
                    return "";
                }
                return Encoding.ASCII.GetString(usernameBytes);
            }
            return "";
        }

        /// <summary>
        /// Decrypts the PL password blob
        /// </summary>
        public string DecryptPLPassword()
        {
            //decrypt username
            if (!String.IsNullOrEmpty(parent.plPassword)) // check for null too v 1.47
            {
                byte[] bytesToDecrypt = Convert.FromBase64String(parent.plPassword);
                byte[] passwordBytes = Unprotect(bytesToDecrypt);
                if (passwordBytes == null)
                {
                    ErrBox.Show(
                        "The saved value of your PL password could not be decrypted.\n" +
                        "(This can happen if certain TriagePic settings were hand-edited,\n" +
                        "or copied from another Windows machine.)");
                    return "";
                }
                return Encoding.ASCII.GetString(passwordBytes);
            }
            return "";
        }

    }
}
