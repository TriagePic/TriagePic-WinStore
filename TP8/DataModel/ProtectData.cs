using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// Win 7: using System.Security.Cryptography;
using System.Security.Principal;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.Security.Cryptography.DataProtection;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Popups; // for IBuffer.ToArray()
//using CodeProject.Dialog;

namespace TP8
{
    public class ProtectData
    {
        public string plUserName = "";
        public string plPassword = "";
        public string plToken = ""; // New Dec 2014
        public string plUserNameEncryptedAndBase64Encoded = "";
        public string plPasswordEncryptedAndBase64Encoded = "";
        public string plTokenEncryptedAndBase64Encoded = ""; // New Dec 2014.

        public ProtectData(/*FormTriagePic p*/)
        {
        //    parent = p;
        }

#if WAS
        /// <summary>
        /// TEMPORARY HARD-CODED
        /// </summary>
        /// <returns></returns>
        public string DecryptPLPassword()
        {
            //return "hStaff2011";
        }
        /// <summary>
        /// TEMPORARY HARD-CODED
        /// </summary>
        /// <returns></returns>
        public string DecryptPLUsername()
        {
            //return "hs";

        }
#endif

        // Following functions adapted from
        // http://msdn.microsoft.com/en-us/library/windows/apps/windows.security.cryptography.dataprotection.dataprotectionprovider.aspx

        /// <summary>
        /// Encrypts the PL username and password and returns them as base64 encoded strings
        /// </summary>
        public void EncryptAndBase64EncodePLCredentials()
        // Compare Win 7: void GetEncryptedPLCredentials(ref string plUsername, ref string plPassword)
        {
            // Using This version so we can call it from constructor
            plUserNameEncryptedAndBase64Encoded = EncryptAndBase64EncodeString(plUserName).Result; // blocks awaiting results
            plPasswordEncryptedAndBase64Encoded  = EncryptAndBase64EncodeString(plPassword).Result; // blocks awaiting results
            plTokenEncryptedAndBase64Encoded = EncryptAndBase64EncodeString(plToken).Result; // blocks awaiting results
        }

        public async Task EncryptAndBase64EncodePLCredentialsAsync()
        {
            plUserNameEncryptedAndBase64Encoded = await EncryptAndBase64EncodeString(plUserName);
            plPasswordEncryptedAndBase64Encoded = await EncryptAndBase64EncodeString(plPassword);
            plTokenEncryptedAndBase64Encoded = await EncryptAndBase64EncodeString(plToken);
        }

        public async Task<string> EncryptAndBase64EncodeString(string s)
        // Compare Win 7: void GetEncryptedPLCredentials(ref string plUsername, ref string plPassword)
        {
            if(String.IsNullOrEmpty(s))
                return s;
            // Based loosely on part of msdn sample Protect() function.
            String strDescriptor = "LOCAL=user";  // Choices are LOCAL=user, LOCAL=machine, WEBCREDENTIALS=MyPasswordName, WEBCREDENTIALS=MyPasswordName.myweb.com
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;

            // Protect a message to the local user.
            IBuffer buffProtected = await this.SampleProtectAsync(
                s,
                strDescriptor,
                encoding);

            string results = Convert.ToBase64String(buffProtected.ToArray());
            return results;
        }

        public async Task<IBuffer> SampleProtectAsync(  // From msdn.  Compare Win 7 TriagePic's public static byte[] Protect(byte[] data)
            String strMsg,
            String strDescriptor,
            BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object for the specified descriptor.
            DataProtectionProvider Provider = new DataProtectionProvider(strDescriptor);

            // Encode the plaintext input message to a buffer.
            IBuffer buffMsg = CryptographicBuffer.ConvertStringToBinary(strMsg, encoding);

            // Encrypt the message.
            IBuffer buffProtected = await Provider.ProtectAsync(buffMsg);

            // Execution of the SampleProtectAsync function resumes here
            // after the awaited task (Provider.ProtectAsync) completes.
            return buffProtected;
        }

        /// <summary>
        /// Decodes and decrypts the base64 PL username.  Saves it in member variable and also returns it... empty if problem.
        /// </summary>
        public async Task<string> DecryptPL_Username() // Like Win 7 DecryptPLUsername
        {
            //decrypt username
            if (!String.IsNullOrEmpty(plUserNameEncryptedAndBase64Encoded))
            {
                plUserName = await DecodeAndDecryptString(plUserNameEncryptedAndBase64Encoded);
                if (String.IsNullOrEmpty(plUserName))
                {
                    string errMsg =
                        "The saved value of your PL user name could not be decrypted.\n" +
                        "(This can happen if certain TriagePic settings were hand-edited,\n" +
                        "or copied from another Windows machine.)";
                    MessageDialog dlg = new MessageDialog(errMsg);
                    await dlg.ShowAsync();
                }
            }
            return plUserName;
        }

        /// <summary>
        /// Decodes and decrypts the base64 PL username.  Saves it in member variable and also returns it... empty if problem.
        /// </summary>
        public async Task<string> DecryptPL_Password() // Like Win 7 DecryptPLPassword
        {
            //decrypt username
            if (!String.IsNullOrEmpty(plPasswordEncryptedAndBase64Encoded))
            {
                plPassword = await DecodeAndDecryptString(plPasswordEncryptedAndBase64Encoded);
                if (String.IsNullOrEmpty(plPassword))
                {
                    string errMsg =
                        "The saved value of your PL password could not be decrypted.\n" +
                        "(This can happen if certain TriagePic settings were hand-edited,\n" +
                        "or copied from another Windows machine.)";
                    MessageDialog dlg = new MessageDialog(errMsg);
                    await dlg.ShowAsync();
                }
            }
            return plPassword;
        }

        /// <summary>
        /// Decodes and decrypts the base64 PL token.  Saves it in member variable and also returns it... empty if problem.
        /// </summary>
        public async Task<string> DecryptPL_Token()
        {
            //decrypt token
            if (!String.IsNullOrEmpty(plTokenEncryptedAndBase64Encoded))
            {
                plToken = await DecodeAndDecryptString(plTokenEncryptedAndBase64Encoded);
                if (String.IsNullOrEmpty(plToken))
                {
                    string errMsg =
                        "The saved value of your PL token could not be decrypted.\n" +
                        "(This can happen if certain TriagePic settings were hand-edited,\n" +
                        "or copied from another Windows machine.)";
                    MessageDialog dlg = new MessageDialog(errMsg);
                    await dlg.ShowAsync();
                }
            }
            return plToken;
        }


        public async Task<string> DecodeAndDecryptString(string s)
        {
            if(String.IsNullOrEmpty(s))
                return null;

            byte[] bytes = Convert.FromBase64String(s);
            IBuffer buffProtected = bytes.AsBuffer();
            // Based loosely on part of msdn sample Protect() function.
            BinaryStringEncoding encoding = BinaryStringEncoding.Utf8;

            // Decrypt the previously protected message.
            string results = await this.SampleUnprotectData(
                buffProtected,
                encoding);
            return results;
        }


        public async Task<String> SampleUnprotectData(
            IBuffer buffProtected,
            BinaryStringEncoding encoding)
        {
            // Create a DataProtectionProvider object.
            DataProtectionProvider Provider = new DataProtectionProvider();

            // Decrypt the protected message specified on input.
            IBuffer buffUnprotected = await Provider.UnprotectAsync(buffProtected);

            // Execution of the SampleUnprotectData method resumes here
            // after the awaited task (Provider.UnprotectAsync) completes
            // Convert the unprotected message from an IBuffer object to a string.
            String strClearText = CryptographicBuffer.ConvertBinaryToString(encoding, buffUnprotected);

            // Return the plaintext string.
            return strClearText;
        }
    }
}


