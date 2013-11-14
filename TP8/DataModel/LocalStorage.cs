using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using System.IO;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Core;

// Adapted from www.irisclasson.com/2012/07/11/
//  example-metro-app-winrt-serializing-and-deseralizing[sic]-objects-using-xmlserializer-to-storagefile-and-localfolder-using-generics-and-asyncawait-threading/...
// Found by search for "windows 8 xmlserializer"

// File(s) created with be in :
// C:\Users\<USER_NAME>\AppData\Local\Packages\<PACKAGE_FAMILY_NAME>\LocalState

// where <PACKAGE_FAMILY_NAME> will be seen under the “Packaging” tab of the “Package.appxmanifest” file, e.g., b56191b4-c587-4cef-bec1-222c0ed2775c

// Original's namespace: SerializeListWinRT.DataModel
namespace TP8.Data
{
    class LocalStorage
    {
        private static List<object> _data = new List<object>();

        public static List<object> Data
        {
            get { return _data; }
            set { _data = value; }
        }

        public static StorageFile file { get; set; }

        // in sample code: private const string filename = "animals.xml";

        private static string errMsg = "";

        static public void Add(object item)
        {
            _data.Add(item);
        }

        static async public Task Save<T>(string filename)
        {
//            if (await DoesFileExistAsync(filename))
//            {
                  await Windows.System.Threading.ThreadPool.RunAsync((sender) => LocalStorage.SaveAsync<T>(filename).Wait(), Windows.System.Threading.WorkItemPriority.Normal); 
//            }
//            else
//            {
//                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename,CreationCollisionOption.ReplaceExisting);
//            }
        }
/* Commentary on above function... Mohamad on January 27, 2013 at 1:38 am said: 
 
...you will get better and more reliable performance under stress if you do not await your saves. There is actually no need to await the saves.
Otherwise you get Access Denied if you run two saves in a short time; the second save will usually error.

So I changed this line
  await Windows.System.Threading.ThreadPool.RunAsync((sender) => LocalStorage.SaveAsync().Wait(), Windows.System.Threading.WorkItemPriority.Normal); 
to 
  Windows.System.Threading.ThreadPool.RunAsync((sender) => LocalStorage.SaveAsync().Start(), Windows.System.Threading.WorkItemPriority.Normal); 

Note that .Wait() was changed to .Start()

Additionally the call to Save was changed to no longer await it:
 LocalStorage.Save();
*/

// Note a version of all these could be made for roaming instead of local folder, which has pluses/minuses: ApplicationData.Current.RoamingFolder....
// Namely would survive (for a while) uninstall... but has data size limits

        static async public Task Restore<T>(string filename)
        {
            if (await DoesFileExistAsync(filename))
            {
                bool OK = true;
                try
                {
                    await Windows.System.Threading.ThreadPool.RunAsync((sender) => LocalStorage.RestoreAsync<T>(filename).Wait(), Windows.System.Threading.WorkItemPriority.Normal);
                }
                catch (Exception /*e*/) // Glenn adds
                {
                    OK = false; // can't await in catch clause, so do this
                }
                if (!OK)
                {
                    errMsg = "Error, couldn't read expected contents of cache file\n" + filename + "\n";
                    await ReportError("During LocalStorage.Restore", errMsg);
                    return;
                }
            }
            else
            {
                file = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename);
            }
        }

        static private async Task ReportError(string msg2Log, string msg2User)
        {
            /* Problematic on startup:
            //await App.dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            //{
                MessageDialog dlg = new MessageDialog(errMsg); await dlg.ShowAsync();
            //});
             */
            TP_ErrorLogItem eli = new TP_ErrorLogItem()
            {
                ErrorDate = App.ErrorLog.GetNowAsErrorDate(),
                MessageToLog = msg2Log,
                MessageToUser = msg2User,
                ShownToUser = false
            };
            App.ErrorLog.Add(eli);
            await App.ErrorLog.WriteXML();
        }

        static async public Task<bool> DoesFileExistAsync(string fileName) 
        {
	        try
	        {
                await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
		        return true;
            } 
            catch 
            {
		        return false;
	        }
        }

        static async private Task SaveAsync<T>(string filename)
        {
                StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                IRandomAccessStream sessionRandomAccess = await sessionFile.OpenAsync(FileAccessMode.ReadWrite);
                IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
                string err = "";
                try
                {
                    var serializer = new XmlSerializer(typeof(List<object>), new Type[] {typeof (T)});

                    //Using DataContractSerializer , look at the cat-class
                    //var sessionSerializer = new DataContractSerializer(typeof(List<object>), new Type[] { typeof(T) });
                    //sessionSerializer.WriteObject(sessionOutputStream.AsStreamForWrite(), _data);

                    //Using XmlSerializer
                    serializer.Serialize(sessionOutputStream.AsStreamForWrite(), _data);
                }
                catch (Exception ex)
                {
                    Exception debugex = ex.InnerException;
                    err = "Inner exception message: " + ex.InnerException.Message;
                    //throw ex;  // breakpoint here
                }
                if(err != "")
                    await ReportError("During LocalStorage.SaveAsync serialization", err);
                sessionRandomAccess.Dispose();
                await sessionOutputStream.FlushAsync();
                sessionOutputStream.Dispose();
        }

        static async private Task RestoreAsync<T>(string filename)
        {
            errMsg = "";
            StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            if (sessionFile == null)
            {
                errMsg = "Error, couldn't find file\n" + filename + "\n";
                await ReportError("During LocalStorage.RestoreAsync", errMsg);
                return;
            }
            IInputStream sessionInputStream = await sessionFile.OpenReadAsync();

            //Using DataContractSerializer , look at the cat-class (in sample code)
            // var sessionSerializer = new DataContractSerializer(typeof(List<object>), new Type[] { typeof(T) });
            //_data = (List<object>)sessionSerializer.ReadObject(sessionInputStream.AsStreamForRead());

            //Using XmlSerializer , look at the Dog-class (in sample code)
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<object>), new Type[] { typeof(T) });
                _data = (List<object>)serializer.Deserialize(sessionInputStream.AsStreamForRead());
            }
            catch (Exception ex)
            {
                errMsg = "Error when trying to read file\n" + sessionFile.Path + "\n";
                errMsg += ex.Message + "\n\nDetails:\n";
                errMsg += ex.InnerException;
            }
            if (errMsg != "")
            {
                await ReportError("During LocalStorage.RestoreAsync", errMsg);
            }
            sessionInputStream.Dispose();
        }
    }
}
