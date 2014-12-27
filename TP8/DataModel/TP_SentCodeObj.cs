using System;
using System.Diagnostics;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

namespace TP8.Data
{
    public class SentCodeObj
    {
        private string stringForm;

        public SentCodeObj(string s) { stringForm = s; Debug.Assert(IsValid()); }

        public SentCodeObj() { } // stringForm not yet valid

        public SentCodeObj(SentCodeObj o) { stringForm = o.stringForm; }

        public override string ToString() { return stringForm; }

        public void SetFromString(string s) { stringForm = s; Debug.Assert(IsValid()); }

        // Read-only methods, with signatures to either look at class variable stringForm or passed parameter:

        public bool IsValid() { return IsValid(stringForm); }

        public bool IsValid(string s)
        {
            if (IsValidCodeWithoutVersion(s) && IsValidVersionSuffix(s))
                return true;
            return false;
        }

        public bool IsValidCodeWithoutVersion() { return IsValidCodeWithoutVersion(stringForm); }

        public bool IsValidCodeWithoutVersion(string s)
        {
            string code = GetCodeWithoutVersion(s);
            switch(code)
            {
                case "Q": case "QN":
                case "QD": // New Dec 2014 queued for delete
                case "Y": case "N":
                case "X1": case "X2": case "X3": case "X4": // abnormals
                case "": // allowed before enqueued
                    return true;
                default:
                    return false; // In theory, includes null
/* Win 7:
                case "Q": case "QE": case "QN":
                case "Y": case "Y+": case "Y-":
                case "y": case "y+": case "y-":
                case "N":
                case "X1": case "X2": case "X3": case "X4": // abnormals
                    return true;
                default:
                return false; // In theory, includes null or ""
 */
            }
        }

        public bool IsDoneOK() { return IsDoneOK(stringForm); }

        public bool IsDoneOK(string s)
        {
            string code = GetCodeWithoutVersion(s);
            switch (code)
            {
                case "Y":
                // Used in Win7, probably not pertinent to Win8:
                case "Y+":
                case "y":
                case "y+":
                    return true;
                default:
                    return false; // In theory, includes null or ""
            }
        }

        public bool IsQueued() { return IsQueued(stringForm); }

        public bool IsQueued(string s)
        {
            if (String.IsNullOrEmpty(s))
                return false;
            return (bool)(s[0] == 'Q');  // Covers "QE", "QN", "QD", and updates too
        }

        public bool IsQueuedEmailOnly() { return IsQueuedEmailOnly(stringForm); }

        public bool IsQueuedEmailOnly(string s)
        {
            string code = GetCodeWithoutVersion();
            if (String.IsNullOrEmpty(s))
                return false;
            return (bool)(code == "QE");  // Covers updates too
        }

        public bool IsQueuedRetry() { return IsQueuedRetry(stringForm); }

        public bool IsQueuedRetry(string s)
        {
            string code = GetCodeWithoutVersion();
            if (String.IsNullOrEmpty(s))
                return false;
            return (bool)(code == "QE" || code == "QN");  // Covers updates too
        }

        public bool IsValidVersionSuffix() { return IsValidVersionSuffix(stringForm); }

        public bool IsValidVersionSuffix(string s)
        {
            string suffix = GetVersionSuffix(s); // either "", or has '#' as first character
            if (suffix == "")
                return true; // OK to not have a suffix
            if (suffix == "#") // '#' by itself is invalid
                return false;
            int count;
            try
            {
                count = Convert.ToInt32(suffix.Substring(1));// exclude '#'.  This should catch any stray non-numeric chars.
            }
            catch (Exception)
            {
                return false;
            }
            if (count < 2) // 1 should be implicit, not explicit
                return false;
            return true;
        }

        public string GetVersionSuffix() { return GetVersionSuffix(stringForm); }

        public string GetVersionSuffix(string s)
        {
            Debug.Assert(s.Length > 0);
            int updateSymbol = s.IndexOf('#');
            string suffix = ""; // first send is #1, but that's implicit, not explicit
            if (updateSymbol > 0)
                suffix = s.Substring(updateSymbol);
            return suffix; // include '#'
        }

        public string GetVersionSuffixFilenameForm() { return GetVersionSuffixFilenameForm(stringForm); }

        public string GetVersionSuffixFilenameForm(string s)
        {
            Debug.Assert(s.Length > 0);
            int updateSymbol = s.IndexOf('#');
            string suffix = ""; // first send is #1, but that's implicit, not explicit
            if (updateSymbol > 0)
                suffix = " Msg " + s.Substring(updateSymbol);
            return suffix; // include ' Msg #' if not empty
        }



        public Int32 GetVersionCount() { return GetVersionCount(stringForm); }

        public Int32 GetVersionCount(string s)
        {
            Debug.Assert(s.Length > 0);
            int updateSymbol = s.IndexOf('#');
            Int32 count = 1; // first send is #1, but that's implicit, not explicit
            if (updateSymbol > 0)
                try
                {
                    count = Convert.ToInt32(s.Substring(updateSymbol + 1));// exclude '#'
                }
                catch(Exception)
                {
                    count = 0; // fail quietly I guess
                }
            return count; 
        }

        public string GetCodeWithoutVersion() { return GetCodeWithoutVersion(stringForm); }

        public string GetCodeWithoutVersion(string s)
        {
            Debug.Assert(s.Length > 0);
            int updateSymbol = s.IndexOf('#');
            string code = "";
            if (updateSymbol > -1)
                code = s.Substring(0, updateSymbol);
            else
                code = s;
            return code;
        }
#if WIN7
        /// <summary>
        /// After a send, this provides a portion of the GUI status line message, based on revised Sent Code (already in this object),
        /// and auxillary information from Sent Code as originally dequeued.
        /// This function should NOT be used for "Q", "QE", or "QN" codes, or for transient messages before and during sends.
        /// </summary>
        /// <param name="emailOnlyRequest">true if "QE" when dequeued, else false (or if not dequeued)</param>
        /// <returns></returns>
        public string StatusMessageFromSentCode(bool emailOnlyRequest)
        {
            string s;
            string code = GetCodeWithoutVersion();
            switch (code)
            {
                case "Y":
                    s = "Sent to PL: "; break;
                case "Y+":
                    if (emailOnlyRequest)
                        s = "Emailed others OK: ";
                    else
                        s = "Sent to PL, emailed others: ";
                    break;
                case "Y-":
                    if (emailOnlyRequest)
                        s = "Emailed to others failed: ";
                    else
                        s = "Sent to PL OK; emails to others failed: "; break; // used to say: "...Some/all emails to others failed: "
                case "y":
                    s = "Emailed PL: "; break;
                case "y+":
                    s = "Emailed PL & others: "; break;
                case "N":
                    s = "All emails failed, including to PL: "; break;
                case "Q": case "QE": case "QN": default:
                    Debug.Assert(false);
                    s = "Internal error " + code + ":"; break;
            }
            return s;
        }
        // Earlier version was in FormTriagePic: public string StatusMessageFromSentCode(string sentCodeFinal, string sentCodeAsDequeued)
#endif

        /// <summary>
        /// After a send, this provides a portion of the GUI status line message, based on revised Sent Code (already in this object),
        /// and auxillary information from Sent Code as originally dequeued.
        /// This function should NOT be used for "Q", "QE", or "QN" codes, or for transient messages before and during sends.
        /// </summary>
        /// <param name="emailOnlyRequest">true if "QE" when dequeued, else false (or if not dequeued)</param>
        /// <returns></returns>
        public string StatusMessageFromSentCode(bool emailOnlyRequest)
        {
            string s;
            string code = GetCodeWithoutVersion();
            switch (code)
            {
                case "Y":
                    s = "Sent"; break;
                case "N":
                    s = "NOT SENT.  Please retry."; break;
                case "Q":
                case "QN":
                case "QD": // New Dec 2014.  Necessary here?
                default:
                    //Debug.Assert(false);
                    s = "Internal error " + code; break;
            }
            return s;
        }

        // Rest of methods are not read-only (they write to stringForm)

        /// <summary>
        /// Replaces the Sent Code with the new value, leaving the version suffix if any unchanged.
        /// </summary>
        /// <param name="newCode">New Sent Code, without '#' version suffix</param>
        /// <returns></returns>
        public string ReplaceCodeKeepSuffix(string newCode) // leaving update suffix unchanged
        {
            Debug.Assert(IsValidCodeWithoutVersion());
            string suffix = GetVersionSuffix();
            stringForm = newCode + suffix;
            Debug.Assert(IsValid());
            return stringForm;
        }

        /// <summary>
        /// Replaces both the base Sent Code and any edit-version suffix
        /// </summary>
        /// <param name="newCodeAndVersion">New sent code including '#' version suffix</param>
        /// <returns>Same string passed in</returns>
        public string ReplaceCodeAndVersion(string newCodeAndVersion)
        {
            stringForm = newCodeAndVersion;
            Debug.Assert(IsValid());
            return stringForm;
        }


        /// <summary>
        /// Increments the edit-version suffix, without changing base Sent Code
        /// </summary>
        /// <returns>Sent code including '#' version suffix</returns>
        public string BumpVersion()
        {
            int count = GetVersionCount();
            count++;
            stringForm = GetCodeWithoutVersion() + "#" + count.ToString();
            Debug.Assert(IsValid());
            return stringForm;
        }





    }
}
