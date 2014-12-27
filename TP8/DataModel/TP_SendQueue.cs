using TP8;
using TP8.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.Core; // for Stopwatch

//#if READY
namespace TP8.Data
{
    public class TP_SendQueue
    {
        // Win 7: We're going to use a single data queue, for both email and web service sends.
        // This is because we want to present a single queue to user, and to make it easier to determine when to move image files
        // Win 8: Only concerned with web service sends.
        static public BlockingCollection<TP_PatientReport> reportsToSend = new BlockingCollection<TP_PatientReport>();
        //bool goodConnectivity = false; // Until we know... variable moved to App
        const Int32 pingInterval = 3000; // In milliseconds
        // The UI thread is the initial producer.
        // It will do reportsToSend.Add(newReport); or resentReport
#if IF_ALLOW_CANCEL
        static CancellationToken ct;  // Placeholder, Need cancellationTokenSource.Token
#endif
        // Caused problems when declaration is locat to TestConnectivity:
        //SolidColorBrush blueBrush = new SolidColorBrush(Colors.Blue);
        SolidColorBrush redBrush = new SolidColorBrush(Colors.OrangeRed);
        SolidColorBrush yellowBrush = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush greenBrush = new SolidColorBrush(Colors.GreenYellow);
        SolidColorBrush darkGreenBrush = new SolidColorBrush(Colors.DarkGreen);
        SolidColorBrush darkRedBrush = new SolidColorBrush(Colors.DarkRed);
        SolidColorBrush darkYellowBrush = new SolidColorBrush(Colors.DarkGoldenrod); // close enuf

        public TP_SendQueue() {}

        public void Add(TP_PatientReport prd1) // Assume caller will clear & reuse prd1, so make a copy of data for queue.
        {
            // Call this after StartWork()
            TP_PatientReport prd2 = new TP_PatientReport(prd1); // deep copy
            TP_SendQueue.reportsToSend.Add(prd2);
            UpdateCountOnReportPage();
            // Once the orginal is copied & the copy queued, the original is treated as read-only
            // NO, CAUSES PROBLEMS:  prd1.Reinitialize(); // moved here to prevent race condition between queueing and clearing.
        }
#pragma warning disable 4014
        // warning CS4014: Because this call is not awaited, execution of the current method continues before the call is completed.
        // Consider applying the 'await' operator to the result of the call.
        public void StartWork()
        {
            // Now on UI thread here, unlike Win 7 TriagePic
            TestConnectivity();  // DON'T await.  This runs "forever"
            LoopProcessReport(); // Likewise
        }
#pragma warning restore 4014

        public async Task LoopProcessReport()
        {
            while(true)
            {
                await Task.Delay(pingInterval); //was in Win 7: Thread.Sleep(pingInterval); // loose alignment with pinger
                if (!App.goodWebServiceConnectivity)
                    continue;
                await ProcessReport(); // just single at a time here, not up to 4 as in Win 7
                UpdateCountOnReportPage();
            }
        }

#if GIVEUP
        public void StartWork()
        {

            Task blinker = Task.Factory.StartNew(async () => {
                await TestConnectivity();
            });
#if SETASIDE
            Task sender = Task.Factory.StartNew(() =>
            {
                while (true) // always look for more work
                {
                    Task.Delay(pingInterval); //was in Win 7: Thread.Sleep(pingInterval); // loose alignment with pinger
                    // Alternative to using Sleep here might be, in ProcessReport, using TryTake with TimeSpan
                    // See http://stackoverflow.com/questions/3502902/dequeueing-objects-from-a-concurrentqueue-in-c-sharp
                    if (!goodConnectivity)
                        continue;
/* TO DO
                    if (parent.checkCredentialsWhenConnectivityRestored) // new 1.52
                    {
                        //Debug.Assert(parent.authorized); // but only based on stored credentials
                        //Debug.Assert(parent.newReport.PL_Name == parent.pd.DecryptPLUsername());
                        //Debug.Assert(!String.IsNullOrEmpty(parent.newReport.PL_Name));
                        //string plpass = parent.pd.DecryptPLPassword();
                        Debug.Assert(!String.IsNullOrEmpty(App.pd.plPassword); //(parent.pd.DecryptPLPassword()));
                        string errorMessage = parent.service.VerifyPLCredentials(parent.newReport.PL_Name, plpass, true); // hospital staff or hsa only
                        if (!String.IsNullOrEmpty(errorMessage))
                        {
                            if (errorMessage.Contains("COMMUNICATIONS ERROR:"))
                            {
                                goodConnectivity = false;
                                continue;
                            }
                            else
                            {
                                parent.authorized = false;
                                // TO DO? AppBox.Show(errorMessage);
                                // Bailing will cause problems, possible loss of data queue?.  TO DO!!! REVISIT
                                parent.authorized = parent.AskForCredentials3TimesThenBail();
                                if (!parent.authorized)
                                    continue; // BECAUSE OF BAIL, WON'T REALLY GET HERE

                                // Debug.Assert(parent.authorized);
                                // Decrypt new values:
                                parent.newReport.PL_Name = parent.pd.DecryptPLUsername();
                                plpass = parent.pd.DecryptPLPassword();

                                Debug.Assert(!String.IsNullOrEmpty(parent.newReport.PL_Name));
                                Debug.Assert(!String.IsNullOrEmpty(plpass));

                                parent.PL_NameFieldLabel.Text = parent.newReport.PL_Name;
                            }
                        }
                        parent.checkCredentialsWhenConnectivityRestored = false; // Done.
                    }
*/
                    Parallel.For(0, reportsToSend.Count,
                         new ParallelOptions() { MaxDegreeOfParallelism = 4 },
                         async (dummy) =>
                         {
                             await ProcessReport(dummy);
                         });
                };
            });


#if IF_ALLOW_CANCEL
        Task sender = Task.Factory.StartNew(() => 
            Parallel.ForEach(
                //TP_PatientReport report in reportsToSend.GetConsumingEnumerable(ct),
                reportsToSend,
                new ParallelOptions() { CancellationToken = ct },
                report => { this.ProcessReport(report);})
                ,ct);

        Task.WaitAll(sender);
#endif
#endif
        }
#endif

        public async Task TestConnectivity()
        {
            Stopwatch sw = Stopwatch.StartNew();

            string results;
            int pingLatencyInTenthsOfSeconds = -1;

            while (true)
            {
                if (Window.Current.Content != null)
                {
                    // Throws Invalid conversion exception, but otherwise seems OK:
                    Type type = ((Frame)Window.Current.Content).CurrentSourcePageType;
                    // Throws exception: var frame = (Frame)Window.Current.Content;
                    //dynamic frame = Window.Current.Content;
                    //dynamic type = frame==null?null:frame.CurrentSourcePageType;
                    //Type type = frame.GetType();
                    //string name = Window.Current.Content.ToString();
                    //if(name.Contains("BasicPageNew") || name.Contains("BasicPageViewEdit")) // use contains because of TP8. prefix
                    if (type != null && (type.Name == "BasicPageNew" || type.Name == "BasicPageViewEdit"))
                    {
                        //if (_Frame.Content.GetType() != typeof(BasicPageNew))
                        //    return;
                        string s = type.ToString();
                        //var bpn = (BasicPageNew)_Frame.Content;
                        //parent.pictureBoxConnect2PL.BackgroundImage = TriagePic.Properties.Resources.dark_traffic;
                        DarkenTrafficLightOnReportPage(type);
                        sw.Reset();
                        sw.Start();
                        results = await App.service.GetPing(pingLatencyInTenthsOfSeconds); // Thread.Sleep(50); // total delay would be around 62 milliseconds
                        sw.Stop();
                        pingLatencyInTenthsOfSeconds = Convert.ToInt32(Math.Round(Convert.ToDouble(sw.ElapsedMilliseconds) / 100.0));
                        if (sw.Elapsed.TotalMilliseconds > (double)3000.0 || results.Contains("ERROR:")) // > 3 seconds or error
                        {
                            //parent.pictureBoxConnect2PL.BackgroundImage = TriagePic.Properties.Resources.red_traffic;
                            LightUpRed(type);
                            App.goodWebServiceConnectivity = false;
                        }
                        else if (sw.Elapsed.TotalMilliseconds > (double)500.0) // 1/2 second
                        {
                            //parent.pictureBoxConnect2PL.BackgroundImage = TriagePic.Properties.Resources.yellow_traffic;
                            LightUpYellow(type);
                            App.goodWebServiceConnectivity = true; // good but not great
                        }
                        else
                        {
                            //parent.pictureBoxConnect2PL.BackgroundImage = TriagePic.Properties.Resources.green_traffic;
                            LightUpGreen(type);
                            App.goodWebServiceConnectivity = true;
                        }
                    }
                }
                //Thread.Sleep(pingInterval);
                await Task.Delay(pingInterval);
            }
        }

#if GIVEUP
        private void DiddleUI(SolidColorBrush b)
        {
            var _Frame = Window.Current.Content as Frame;
            //if (_Frame.Content.GetType() != typeof(BasicPageNew))
            //    return;
            Type type = _Frame.CurrentSourcePageType;
            string s = type.ToString();
            var bpn = (BasicPageNew)_Frame.Content;
            return;
            /*await*/ CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
            () =>
            {
                // Your UI udate code goes here
                bpn.Blinker.Fill = b;
            }
        );
        }
#endif
        private void DarkenTrafficLightOnReportPage(Type t)
        {
            var _Frame = Window.Current.Content as Frame;
            if (t.Name == "BasicPageNew")
            {
                var p = (BasicPageNew)_Frame.Content;
                p.BlinkerTopRed.Fill = darkRedBrush;
                p.BlinkerMiddleYellow.Fill = darkYellowBrush;
                p.BlinkerBottomGreen.Fill = darkGreenBrush;
            }
            else if (t.Name == "BasicPageViewEdit")
            {
                var p = (BasicPageViewEdit)_Frame.Content;
                p.BlinkerTopRed.Fill = darkRedBrush;
                p.BlinkerMiddleYellow.Fill = darkYellowBrush;
                p.BlinkerBottomGreen.Fill = darkGreenBrush;
            }
        }

        private void UpdateCountOnReportPage()
        {
            Type type = ((Frame)Window.Current.Content).CurrentSourcePageType;
            if (type == null)
                return;
            if (type.Name != "BasicPageNew" && type.Name != "BasicPageViewEdit")
                return;
            int count = reportsToSend.Count();
            string displayedCount = ""; // If zero, show as blank
            if(count > 0)
                displayedCount = count.ToString();
            var _Frame = Window.Current.Content as Frame;
            if (type.Name == "BasicPageNew")
            {
                var p = (BasicPageNew)_Frame.Content;
                p.CountInSendQueue.Text = displayedCount;
            }
            else if (type.Name == "BasicPageViewEdit")
            {
                var p = (BasicPageViewEdit)_Frame.Content;
                p.CountInSendQueue.Text = displayedCount;
            }
        }

        private void LightUpRed(Type t)
        {
            var _Frame = Window.Current.Content as Frame;
            if (t.Name == "BasicPageNew")
            {
                var p = (BasicPageNew)_Frame.Content;
                p.BlinkerTopRed.Fill = redBrush;
            }
            else if (t.Name == "BasicPageViewEdit")
            {
                var p = (BasicPageViewEdit)_Frame.Content;
                p.BlinkerTopRed.Fill = redBrush;
            }
        }

        private void LightUpYellow(Type t)
        {
            var _Frame = Window.Current.Content as Frame;
            if (t.Name == "BasicPageNew")
            {
                var p = (BasicPageNew)_Frame.Content;
                p.BlinkerMiddleYellow.Fill = yellowBrush;
            }
            else if (t.Name == "BasicPageViewEdit")
            {
                var p = (BasicPageViewEdit)_Frame.Content;
                p.BlinkerMiddleYellow.Fill = yellowBrush;
            }
        }

        private void LightUpGreen(Type t)
        {
            var _Frame = Window.Current.Content as Frame;
            if (t.Name == "BasicPageNew")
            {
                var p = (BasicPageNew)_Frame.Content;
                p.BlinkerBottomGreen.Fill = greenBrush;
            }
            else if (t.Name == "BasicPageViewEdit")
            {
                var p = (BasicPageViewEdit)_Frame.Content;
                p.BlinkerBottomGreen.Fill = greenBrush;
            }
        }

//        public void ProcessReport(TP_PatientReport report) // We asked for up to 4 instances of this running
        public async Task ProcessReport() // Compare win 7 ProcessReport(int dummy)
        {
            bool gotReport = false;
            TP_PatientReport dequeuedReport;


            // Nature of reportsToSend.Take is that this will block until there's something in queue.
            // But don't really want to take if connection is bad.
            // So instead we checked count above.
            //dequeuedReport = reportsToSend.Take();  // But could block too long if another thread got the report first!
            gotReport = reportsToSend.TryTake(out dequeuedReport);
            if (!gotReport)
                return;
            // We could push the record back on queue upon error, but then it would recur endlessly unless we marked it.  Dunno.
            //SentCodeObj objSentCodeDequeuedReport = new SentCodeObj(dequeuedReport.SentCode);

            string contentPath = dequeuedReport.FullNameEDXL_and_LP2;
            if (String.IsNullOrEmpty(contentPath))
            {
                dequeuedReport.ObjSentCode.ReplaceCodeKeepSuffix("X1"); // dequeuedReport.SentCode = objSentCodeDequeuedReport.ReplaceCodeKeepSuffix("X1");
                await App.PatientDataGroups.UpdateSendHistory(dequeuedReport.PatientID, dequeuedReport.ObjSentCode); //parent.outbox.UpdateSendHistory(dequeuedReport);
                await App.ErrorLog.ReportToErrorLog("From SendQueue dequeuing", "X1 as Send Status", false);
                return;
            }
            if(!await LocalStorage.DoesFileExistAsync(contentPath)) //if (!File.Exists(contentPath))
            {
                // KLUDGE
                // Win 7: contentPath = contentPath.Replace("processed", "sent");
                //if (!File.Exists(contentPath))
                //{
                dequeuedReport.ObjSentCode.ReplaceCodeKeepSuffix("X2"); // dequeuedReport.SentCode = objSentCodeDequeuedReport.ReplaceCodeKeepSuffix("X2");
                    await App.PatientDataGroups.UpdateSendHistory(dequeuedReport.PatientID, dequeuedReport.ObjSentCode); //parent.outbox.UpdateSendHistory(dequeuedReport);
                    await App.ErrorLog.ReportToErrorLog("From SendQueue dequeuing", "X2 as Send Status", false);
                    return; 
                //}
            }

            string contentEDXL_and_LP2 = ""; //read from file
            bool threwException = false;
            try
            {
                contentEDXL_and_LP2 = await dequeuedReport.ReadReportPersonContentXML(contentPath);
                //File.ReadAllText(contentPath);
            }
            catch (Exception /*e*/)
            {
                threwException = true;
            }
            if (threwException)
            {
                dequeuedReport.ObjSentCode.ReplaceCodeKeepSuffix("X3"); // dequeuedReport.SentCode = objSentCodeDequeuedReport.ReplaceCodeKeepSuffix("X3");
                await App.PatientDataGroups.UpdateSendHistory(dequeuedReport.PatientID, dequeuedReport.ObjSentCode); //parent.outbox.UpdateSendHistory(dequeuedReport);
                await App.ErrorLog.ReportToErrorLog("From SendQueue dequeuing", "X3 as Send Status", false);
                return;
            }

            if(!dequeuedReport.ObjSentCode.IsQueued()) //if(!objSentCodeDequeuedReport.IsQueued()) // Win 8 Should be "Q", possibly with updates.  Win 7: Should be "Q", "QE", or either with updates.
            {
                Debug.Assert(false);
                dequeuedReport.ObjSentCode.ReplaceCodeKeepSuffix("X4"); // dequeuedReport.SentCode = objSentCodeDequeuedReport.ReplaceCodeKeepSuffix("X4");
                await App.PatientDataGroups.UpdateSendHistory(dequeuedReport.PatientID, dequeuedReport.ObjSentCode); //parent.outbox.UpdateSendHistory(dequeuedReport); // TO DO log error somewhere
                await App.ErrorLog.ReportToErrorLog("From SendQueue dequeuing", "X4 as Send Status", false);
                return;
            }
 
            //if (objSentCodeDequeuedReport.IsQueuedRetry() || objSentCodeDequeuedReport.GetVersionCount() > 1)
            if (dequeuedReport.ObjSentCode.IsQueuedRetry() || dequeuedReport.ObjSentCode.GetVersionCount() > 1) // delete requests come here too
            {
                await dequeuedReport.DequeueAndProcessPatientDataRecord(false /*firstSend*/, contentEDXL_and_LP2);
                // Win7: parent.formRE.DequeueAndProcessPatientDataRecordAgain(dequeuedReport, contentEDXL_and_LP2);
            }
            else
            {
                Debug.Assert(dequeuedReport.SentCode == "Q"); // first time for this patient, from main tab
                await dequeuedReport.DequeueAndProcessPatientDataRecord(true /*firstSend*/, contentEDXL_and_LP2);
                // Win7: parent.DequeueAndProcessPatientDataRecord(dequeuedReport, contentEDXL_and_LP2);
            } 

        }

     }
}

//#endif