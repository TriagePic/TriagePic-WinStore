using System;
//using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
//using System.Collections.ObjectModel;
//using System.ComponentModel;
//using System.Runtime.CompilerServices;
//using Windows.ApplicationModel.Resources.Core;
//using Windows.Foundation;
//using Windows.Foundation.Collections;
//using Windows.UI.Xaml.Data;
//using Windows.UI.Xaml.Media;
//using Windows.UI.Xaml.Media.Imaging;
//using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Collections;
using Windows.UI.Popups;
using LPF_SOAP;
using Newtonsoft.Json;
using System.Globalization;

// This is the underlying TP data model, which is mapped to the SampleDataItem of the SampleDataSource model with its data bindings in the standard item templates.
namespace TP8.Data
{
    // Code here based on style of http jesseliberty.com/2012/08/21/windows-8-data-binding-part5binding-to-lists/
    public class TP_ErrorLogItem : INotifyPropertyChanged
    {
        [XmlAttribute]
        private string _errorDate; // 2001-01-01
        public string ErrorDate
        {
            get { return _errorDate; }
            set
            {
                _errorDate = value;
                RaisePropertyChanged();
            }
        }

        private string _messageToLog;
        public string MessageToLog
        {
            get { return _messageToLog; }
            set
            {
                _messageToLog = value;
                RaisePropertyChanged();
            }
        }

        private string _messageToUser;
        public string MessageToUser
        {
            get { return _messageToUser; }
            set
            {
                _messageToUser = value;
                RaisePropertyChanged();
            }
        }

        private bool _shownToUser;
        public bool ShownToUser
        {
            get { return _shownToUser; }
            set
            {
                _shownToUser = value;
                RaisePropertyChanged();
            }
        }




        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }

        /// <summary>
        /// Like clone, except receiving object already exists.
        /// </summary>
        /// <param name="tp"></param>
        public void CopyFrom(TP_ErrorLogItem tp)
        {
            ErrorDate = tp.ErrorDate;
            MessageToLog = tp.MessageToLog;
            MessageToUser = tp.MessageToUser;
            ShownToUser = tp.ShownToUser;
        }


        public void Clear()
        {
            ErrorDate = "";
            MessageToLog = "";
            MessageToUser = "";
            ShownToUser = false;
        }

    }


    [XmlType(TypeName = "ErrorLog")]
    public class TP_ErrorLog : IEnumerable<TP_ErrorLogItem>
    {
        const string ERROR_LOG_FILENAME = "ErrorLog.xml";

        private List<TP_ErrorLogItem> inner = new List<TP_ErrorLogItem>();

        public void Add(object o)
        {
            inner.Add((TP_ErrorLogItem)o);
        }

        public void Remove(TP_ErrorLogItem o)
        {
            inner.Remove(o);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public List<TP_ErrorLogItem> GetAsList()
        {
            return inner;
        }

        public void ReplaceWithList(List<TP_ErrorLogItem> list)
        {
            inner = list;
        }

        public IEnumerator<TP_ErrorLogItem> GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public async Task Init()
        {
            bool exists = await DoesFileExistAsync();
            if (!exists)
            {
                await GenerateDefaultErrorLog(); // This will provide a default XML file with a little content if none exists
            }

            Clear(); // clear the list in memory, then try to read the XML file.
            await ReadXML();
        }

        public string GetNowAsErrorDate()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }

        private async Task<bool> DoesFileExistAsync()
        {
            return await LocalStorage.DoesFileExistAsync(ERROR_LOG_FILENAME);
        }

        private async Task GenerateDefaultErrorLog()
        {
            await ReportToErrorLog("Generating Default Empty Error Log", "", false);
        }


        public async Task ReportToErrorLog(string messageToLog, string messageToUser, bool shownToUser)
        {
            TP_ErrorLogItem eli = new TP_ErrorLogItem();
            eli.ErrorDate = GetNowAsErrorDate();
            eli.MessageToLog = messageToLog;
            eli.MessageToUser = messageToUser;
            eli.ShownToUser = shownToUser;
            Add(eli);
            await WriteXML();
        }

        public async Task ReadXML()
        {
            await ReadXML(ERROR_LOG_FILENAME);
        }

        public async Task ReadXML(string filename)
        {
            await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            LocalStorage.Data.Clear();
            await LocalStorage.Restore<TP_ErrorLogItem>(filename);
            if (LocalStorage.Data != null)
            {
                foreach (var item in LocalStorage.Data)
                {
                    inner.Add(item as TP_ErrorLogItem);
                }
            }
            App.LocalStorageDataSemaphore.Release();
        }

        public async Task WriteXML()
        {
            await WriteXML(ERROR_LOG_FILENAME);
        }

        public async Task WriteXML(string filename)
        {
            // NO: await App.LocalStorageDataSemaphore.WaitAsync(); // Data buffer shared with other read/writes, so serialize access
            // If reporting error during read/write, we need to be able get access to do so.
            bool done = await App.LocalStorageDataSemaphore.WaitAsync(1000); // wait at most 1 second
            // done is for benefit of debug here.  Will be true if other read/write finished, but false if timeout, which is the case when there's a read of bad xml file

            // TO DO: Come up with error reporting code that doesn't share LocalStorage.Data to do it.
            LocalStorage.Data.Clear();
            foreach (var item in inner)
                LocalStorage.Add(item as TP_ErrorLogItem);

            await LocalStorage.Save<TP_ErrorLogItem>(filename);
            App.LocalStorageDataSemaphore.Release();
        }
    }

}
