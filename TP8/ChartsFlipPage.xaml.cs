﻿using System;
using System.Collections.Generic;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Windows.UI.Xaml;
using System.Linq.Expressions;
using System.Linq;
using Windows.UI.Xaml.Media;
using Windows.UI;
using TP8.Data;
using Windows.UI.Xaml.Data;

namespace TP8  //WinRTXamlToolkit.Sample.Views
{
    public class FieldsForBarChart
    {
        public string barName { get; set; }
        public int barCount { get; set; }
        public string barColor { get; set; }

        /// <summary>
        /// This helper data structure is needed when bars of bar charts need to have individual colors, rather than series color
        /// </summary>
        /// <param name="name"></param>
        /// <param name="count"></param>
        /// <param name="color"></param>
        public FieldsForBarChart(string name, int count, string color)
        {
            barName = name;
            barCount = count;
            barColor = color;
        }
    }

    public sealed partial class ChartsFlipPage : TP8.Common.BasicLayoutPage// WAS:LayoutAwarePage //WinRTXamlToolkit.Controls.AlternativePage
    {
        private DateTime minDT;
        private DateTime maxDT;

        public ChartsFlipPage()
        {
            // Win 8.1: Check for snapped or narrow visual states moved to caller
            //WAS Win 8.0: Windows.UI.ViewManagement.ApplicationView.TryUnsnap();// Glenn's quick hack, since layout for snapped view hasn't been developed

            this.InitializeComponent();
            if(App.PatientDataGroups.GetOutboxForStatistics().Count() == 0)
                App.PatientDataGroups.OutboxForStatisticsRefresh(); // new June 2015
            UpdateCharts();
        }

        // As of late June 2015, replace throughout App.PatientDataGroups.GetOutbox() with App.PatientDataGroups.GetOutboxForStatistics()
        // Later is private clone with - before Release 6 - shortened WhenLocalTime (no UTC offset) and guaranteed to be sorted from oldest to newest arrival/update time.
        // Later is private clone with - after Release 6 - shortened WhenLocalTime and WhenLocalTimeMsg1 (no UTC offset) and guaranteed to be sorted from oldest to newest arrival time.
        private Random _random = new Random();

        public int outboxCountPerZoneAllEvents(string zone)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => p.Zone == zone);
            return count;
        }

        public int outboxCountPerZoneCurrentEvent(string zone)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => (p.Zone == zone && p.EventName == App.CurrentDisaster.EventName) );
            return count;
        }

        public int outboxCountPerGenderAllEvents(string gender)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => p.Gender == gender);
            return count;
        }

        public int outboxCountPerGenderCurrentEvent(string gender)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => (p.Gender == gender && p.EventName == App.CurrentDisaster.EventName));
            return count;
        }

        public int outboxCountPerAgeGroupAllEvents(string ageGroup)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => p.AgeGroup == ageGroup);
            return count;
        }

        public int outboxCountPerAgeGroupCurrentEvent(string ageGroup)
        {
            var count = App.PatientDataGroups.GetOutboxForStatistics().Count(p => (p.AgeGroup == ageGroup && p.EventName == App.CurrentDisaster.EventName));
            return count;
        }

        public void outboxCountPerEventAllEvents(List<NameValueItem> arrivalsByEventAllEvents)
        {
            var grouped = App.PatientDataGroups.GetOutboxForStatistics()
                .GroupBy(s => s.EventName)
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByEventAllEvents.Add(new NameValueItem { Name = thing.Name + " (" + thing.Value.ToString() + ")", Value = thing.Value });
            }
        }

        public void outboxCountPerEventAllEventsForBarChart(List<FieldsForBarChart> arrivalsByEventAllEvents)
        {
            // Differences from outboxCountPerEventAllEvents -
            //   1) use of FieldsForBarChart instead of NameValueItems (we could have used latter if all bars are same color,
            //      e.g., "Blue"... but maybe we'll color code real versus test in futuer)
            //   2) Labels are shortened, don't need count, but get pseudo tickmarks
            var grouped = App.PatientDataGroups.GetOutboxForStatistics()
                .GroupBy(s => s.EventName)
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByEventAllEvents.Add(new FieldsForBarChart(thing.Name + " -", thing.Value, "Blue")); 
            }
        }
/* DOESN'T QUITE WORK IF YOU CARE WHICH COLOR GETS ASSIGNED TO WHICH CATEGORY:
        public void outboxCountPerGenderCurrentEvent(List<NameValueItem> arrivalsByGenderCurrentEvent)
        {
            var grouped = App.PatientDataGroups.GetOutbox()
                .Where(s => s.EventID == App.CurrentDisasterEventID)
                .GroupBy(s => s.Gender)
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByGenderCurrentEvent.Add(new NameValueItem { Name = thing.Name, Value = thing.Value });
            }
        }

        public void outboxCountPerAgeGroupCurrentEvent(List<NameValueItem> arrivalsByAgeGroupCurrentEvent)
        {
            var grouped = App.PatientDataGroups.GetOutbox()
                .Where(s => s.EventID == App.CurrentDisasterEventID)
                .GroupBy(s => s.AgeGroup)
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByAgeGroupCurrentEvent.Add(new NameValueItem { Name = thing.Name, Value = thing.Value });
            }
        }

        public void outboxCountPerZoneCurrentEvent(List<NameValueItem> arrivalsByZoneCurrentEvent)
        {
            var grouped = App.PatientDataGroups.GetOutbox()
                .Where(s => s.EventID == App.CurrentDisasterEventID)
                .GroupBy(s => s.Zone)
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByZoneCurrentEvent.Add(new NameValueItem { Name = thing.Name, Value = thing.Value });
            }
        }
*/
        /// <summary>
        /// For pie charts.  Includes count parenthetically in name for benefit of legend.
        /// </summary>
        /// <param name="arrivalsByDateCurrentEvent"></param>
        public void outboxCountPerDateCurrentEvent(List<NameValueItem> arrivalsByDateCurrentEvent)
        {
            var outbx = App.PatientDataGroups.GetOutboxForStatistics();
            // No longer needed, now that we have _outboxforstatistics:
            //TP8.Data.TP_PatientReports outbx2 = new TP8.Data.TP_PatientReports();
            //foreach (var i in outbx)
            //{
            //    if (String.IsNullOrEmpty(i.WhenLocalTime))
            //        continue; // probably weird empty record
            //    i.WhenLocalTime = ParseDate(i.WhenLocalTime); // take first 10 characters, ignore UTC offset
            //    outbx2.Add(i); // Hmmm, are we changing outbox list itself, removing UTC offset? Maybe not intended side effect.
            //}

            var grouped = outbx //outbx2
                .Where(s => s.EventName == App.CurrentDisaster.EventName) // Maybe should be using this througout: .Where(s => s.EventID == App.CurrentDisasterEventID)
                .GroupBy(s => s.WhenLocalTimeMsg1)  // was (s => s.WhenLocalTime) before Release 6
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByDateCurrentEvent.Add(new NameValueItem { Name = thing.Name + " (" + thing.Value.ToString() + ")", Value = thing.Value });
            }

        }

        public void outboxCountPerDateCurrentEventForBarChart(List<FieldsForBarChart> arrivalsByDateCurrentEvent)
        {
            var outbx = App.PatientDataGroups.GetOutboxForStatistics();
            // No longer needed, now that we have _outboxforstatistics:
            //TP8.Data.TP_PatientReports outbx2 = new TP8.Data.TP_PatientReports();
            //foreach (var i in outbx)
            //{
            //    if (String.IsNullOrEmpty(i.WhenLocalTime))
            //        continue; // probably weird empty record
            //    i.WhenLocalTime = ParseDate(i.WhenLocalTime); // take first 10 characters, ignore UTC offset
            //    outbx2.Add(i); // Hmmm, are we changing outbox list itself, removing UTC offset? Maybe not intended side effect.
            //}

            var grouped = outbx //outbx2
                .Where(s => s.EventName == App.CurrentDisaster.EventName) // Maybe should be using this througout: .Where(s => s.EventID == App.CurrentDisasterEventID)
                .GroupBy(s => s.WhenLocalTimeMsg1) // (s => s.WhenLocalTime) before Release 6
                .Select(group => new NameValueItem { Name = group.Key, Value = group.Count() });
            // Can't figure out cast, so copy instead:
            foreach (var thing in grouped)
            {
                arrivalsByDateCurrentEvent.Add(new FieldsForBarChart ( thing.Name + " -", thing.Value, "Blue" ));
            }

        }

        /// <summary>
        /// Returns a list with the start time of each bucket (even hours) and the count within the bucket,
        /// for current event
        /// </summary>
        /// <param name="arrivalsByDateCurrentEvent"></param>
        public void outboxCountPerDayCurrentEvent(List<DateValueItem> arrivalsByDateCurrentEvent)
        {
            var outbx = App.PatientDataGroups.GetOutboxForStatistics();
            TP8.Data.TP_PatientReports outbx2 = new TP8.Data.TP_PatientReports();
            foreach (var i in outbx)
            {
                if(i.EventName == App.CurrentDisaster.EventName)
                    outbx2.Add(i);
            }
            // No longer needed, now that we have _outboxforstatistics:
            // // Sorting added June 2015:
            //List<TP_PatientReport> outbx2L = outbx2.GetAsList();
            //List<TP_PatientReport> sortL = new List<TP_PatientReport>();
            // sort oldest to newest by local datetime
            //sortL = outbx2L.OrderBy(o => o.WhenLocalTime).ToList(); // or sortL = outbx2L.OrderByDescending(o => o.WhenLocalTime).ToList(); 
            //TP_PatientReports sort = new TP_PatientReports();
            //sort.ReplaceWithList(sortL);
            //outboxCountPerDaySpecificEventSet(sort, arrivalsByDateCurrentEvent); // assumes outbox order is oldest to newest
            outboxCountPerDaySpecificEventSet(outbx2, arrivalsByDateCurrentEvent); // assumes outbox order is oldest to newest
        }
        /// <summary>
        /// Returns a list with the start time of each bucket (even hours) and the count within the bucket,
        /// without distinguishing among events. Includes open and closed events.
        /// </summary>
        /// <param name="arrivalsByDateCurrentEvent"></param>
        public void outboxCountPerDayAllEvents(List<DateValueItem> arrivalsByDateAllEvents)
        {
            // No longer needed, now that we have _outboxforstatistics:
            //             // Sorting added June 2015:
            //List<TP_PatientReport> outbx2L = App.PatientDataGroups.GetOutbox().GetAsList();
            //List<TP_PatientReport> sortL = new List<TP_PatientReport>();
            // // sort oldest to newest by local datetime
            //sortL = outbx2L.OrderBy(o => o.WhenLocalTime).ToList(); // or sortL = outbx2L.OrderByDescending(o => o.WhenLocalTime).ToList(); 
            //TP_PatientReports sort = new TP_PatientReports();
            //sort.ReplaceWithList(sortL);
            //outboxCountPerDaySpecificEventSet(sort, arrivalsByDateAllEvents); // assumes outbox order is oldest to newest
            outboxCountPerDaySpecificEventSet(App.PatientDataGroups.GetOutboxForStatistics(), arrivalsByDateAllEvents); // assumes outbox order is oldest to newest
        }

        /// <summary>
        /// Given an input list of patients, ordered from oldest first to newest last, for either a single event or a set of events that we don't care to distinguish (e.g., all events),
        /// returns a list with the date of each one-day bucket (with time zeroed) and the count within the bucket
        /// </summary>
        /// <param name="arrivalsByDateSpecificEvent"></param>
        public void outboxCountPerDaySpecificEventSet(TP_PatientReports arrivalsIn, List<DateValueItem> arrivalsOut)
        {
            if (arrivalsIn.Count<TP_PatientReport>() == 0)
                return;
            DateTimeOffset startOfBucket;
            DateTimeOffset currentItem;
            bool firstloop = true;
            int inBucket = 0;
            foreach (var i in arrivalsIn)
            {
                // For Release 6, use of i.WhenLocalTime in this loop replaced by i.WhenLocalTimeMsg1, i.e., arrival time
                if (String.IsNullOrEmpty(i.WhenLocalTimeMsg1))
                    continue; // assume weird empty record

                if (firstloop)
                {
                    firstloop = false;

                    DateTimeOffset firstInBucket = DateTimeOffset.Parse(i.WhenLocalTimeMsg1);
                    // Zero out anything under 1 day
                    startOfBucket = new DateTimeOffset(firstInBucket.Year, firstInBucket.Month, firstInBucket.Day, 0, 0, 0, firstInBucket.Offset);
                    inBucket = 1;
                    continue;
                }
                currentItem = DateTime.Parse(i.WhenLocalTimeMsg1);
                if (currentItem.Year == startOfBucket.Year && currentItem.DayOfYear == startOfBucket.DayOfYear)
                {
                    inBucket++;
                    continue;
                }
                // record count accumulated from last through loop:
                arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime, Value = inBucket });

#if OK_BUT_TOO_MANY_ZERO_POINTS
                startOfBucket = startOfBucket.AddDays(1.0);
                // Generate buckets with zeros, 1 per day:
                while (currentItem.Year > startOfBucket.Year || currentItem.DayOfYear > startOfBucket.DayOfYear)
                {
                    arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime, Value = 0 });
                    startOfBucket = startOfBucket.AddDays(1.0);
                }
#endif
                startOfBucket = startOfBucket.AddDays(1.0);
                // Generate two buckets with zeros, at either end of range (or a single bucket if there's only 1 zero day between non-zero days):
                if (currentItem.Year > startOfBucket.Year || currentItem.DayOfYear > startOfBucket.DayOfYear)
                {
                    arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime, Value = 0 }); // 1st one
                    startOfBucket = startOfBucket.AddDays(1.0);
                    if (currentItem.Year > startOfBucket.Year || currentItem.DayOfYear > startOfBucket.DayOfYear)
                    {
                        while (currentItem.Year > startOfBucket.Year || currentItem.DayOfYear > startOfBucket.DayOfYear)
                            startOfBucket = startOfBucket.AddDays(1.0); // skip redundant zero points in center of their range
                        arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime.AddDays(-1.0), Value = 0 }); // 2nd one.  Note -1 day
                    }
                }

                // New bucket starting
                inBucket = 1;
            }
            // Last bucket:
            arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime, Value = inBucket });
            // Special case...
            if (arrivalsOut.Count == 1)
            {
                // Need to generate an extra zero point so that there's at least 1 line segment visible.
                // We'll do it here at end, rather than beginning... smarter algorithm would have caller give us hint to generate at beginning, at end, or both.
                startOfBucket = startOfBucket.AddDays(1.0);
                arrivalsOut.Add(new DateValueItem { DateTimeBucket = startOfBucket.DateTime, Value = 0 });
            }
        }

        // Moved to TP_PatientReportsSource:
        //private string ParseDate(string WhenLocalTime)
        //{
        //    return WhenLocalTime.Substring(0, 10);// Assume YYYY-MM-DD format
        //}

        private void UpdateCharts()
        {
            UpdateChartPage1();
            UpdateChartPage2();
            UpdateChartPage3();
            UpdateChartPage4();
            UpdateChartPage5();
            UpdateChartPage6();
            UpdateChartPage7();
#if SHOW_EXAMPLES_NOT_YET_USED

            List<NameValueItem> items = new List<NameValueItem>();
// Placeholder for charts not yet converted:
            items.Add(new NameValueItem { Name = "Green", Value = _random.Next(10, 100) });
            items.Add(new NameValueItem { Name = "BH Green", Value = _random.Next(10, 100) });
            items.Add(new NameValueItem { Name = "Yellow", Value = _random.Next(10, 100) });
            items.Add(new NameValueItem { Name = "Red", Value = _random.Next(10, 100) });
            items.Add(new NameValueItem { Name = "Gray", Value = _random.Next(10, 100) });
            items.Add(new NameValueItem { Name = "Black", Value = _random.Next(10, 100) });


            //((PieSeries)this.PieChartByTriageZoneCurrentEvent.Series[0]).ItemsSource = arrivalsByZoneCurrentEvent;

            //((ColumnSeries)this.Chart.Series[0]).ItemsSource = items;
            //((BarSeries)this.BarChart.Series[0]).ItemsSource = items;
            //((LineSeries)this.LineChart.Series[0]).ItemsSource = items;

            ((LineSeries)this.LineChart2.Series[0]).ItemsSource = items;
            ((ColumnSeries)this.MixedChart.Series[0]).ItemsSource = items;
            ((LineSeries)this.MixedChart.Series[1]).ItemsSource = items;
            ((AreaSeries)this.AreaChart.Series[0]).ItemsSource = items;
            ((BubbleSeries)this.BubbleChart.Series[0]).ItemsSource = items;
            ((ScatterSeries)this.ScatterChart.Series[0]).ItemsSource = items;
            ((StackedBarSeries)this.StackedBar.Series[0]).SeriesDefinitions[0].ItemsSource = items;
            ((StackedBarSeries)this.StackedBar.Series[0]).SeriesDefinitions[1].ItemsSource = items;
            ((StackedBarSeries)this.StackedBar.Series[0]).SeriesDefinitions[2].ItemsSource = items;
            ((Stacked100BarSeries)this.StackedBar100.Series[0]).SeriesDefinitions[0].ItemsSource = items;
            ((Stacked100BarSeries)this.StackedBar100.Series[0]).SeriesDefinitions[1].ItemsSource = items;
            ((Stacked100BarSeries)this.StackedBar100.Series[0]).SeriesDefinitions[2].ItemsSource = items;

            ((StackedColumnSeries)this.StackedColumn.Series[0]).SeriesDefinitions[0].ItemsSource = items;
            ((StackedColumnSeries)this.StackedColumn.Series[0]).SeriesDefinitions[1].ItemsSource = items;
            ((StackedColumnSeries)this.StackedColumn.Series[0]).SeriesDefinitions[2].ItemsSource = items;

            ((Stacked100ColumnSeries)this.StackedColumn100.Series[0]).SeriesDefinitions[0].ItemsSource = items;
            ((Stacked100ColumnSeries)this.StackedColumn100.Series[0]).SeriesDefinitions[1].ItemsSource = items;
            ((Stacked100ColumnSeries)this.StackedColumn100.Series[0]).SeriesDefinitions[2].ItemsSource = items;
#endif
        }

        private void UpdateChartPage1()
        {
            // For 4 piecharts on 1st page:
            UpdateChartPage1ByZone();
            UpdateChartPage1ByGender();
            UpdateChartPage1ByAgeGroup();
            UpdateChartPage1ByEvent();
        }

        private void UpdateChartPage1ByZone()
        {
            List<NameValueItem> arrivalsByZoneAllEvents = new List<NameValueItem>();
            arrivalsAdd(arrivalsByZoneAllEvents, "Green", outboxCountPerZoneAllEvents("Green"));
            arrivalsAdd(arrivalsByZoneAllEvents, "BH Green", outboxCountPerZoneAllEvents("BH Green"));
            arrivalsAdd(arrivalsByZoneAllEvents, "Yellow", outboxCountPerZoneAllEvents("Yellow"));
            arrivalsAdd(arrivalsByZoneAllEvents, "Red", outboxCountPerZoneAllEvents("Red"));
            arrivalsAdd(arrivalsByZoneAllEvents, "Gray", outboxCountPerZoneAllEvents("Gray"));
            arrivalsAdd(arrivalsByZoneAllEvents, "Black", outboxCountPerZoneAllEvents("Black"));

            ((PieSeries)this.PieChartByTriageZoneAllEvents.Series[0]).ItemsSource = arrivalsByZoneAllEvents;
        }

        /// <summary>
        /// Helper function for pie chart labels and values
        /// </summary>
        /// <param name="lnvi"></param>
        /// <param name="category"></param>
        /// <param name="count"></param>
        private void arrivalsAdd(List<NameValueItem> lnvi, string category, int count)
        {
            lnvi.Add(new NameValueItem { Name = category + " (" + count.ToString() + ")", Value = count });
        }

        /// <summary>
        /// Helper function for bar chart labels, values, colors
        /// </summary>
        /// <param name="lnvi"></param>
        /// <param name="category"></param>
        /// <param name="count"></param>
        private void barsAdd(List<FieldsForBarChart> lnvi, string category, int count)
        {
            string webColor = category;
            if (webColor == "BH Green")
                webColor = "LimeGreen";
            lnvi.Add(new FieldsForBarChart(category + " -", count, webColor )); // Add trailing space & dash for aesthestics when displayed. Don't need count in label, unlike pies
        }

        /// <summary>
        /// Helper function for bar chart labels, values, colors
        /// </summary>
        /// <param name="lnvi"></param>
        /// <param name="category"></param>
        /// <param name="count"></param>
        private void barsAdd(List<FieldsForBarChart> lnvi, string category, string webColor, int count)
        {
            lnvi.Add(new FieldsForBarChart(category + " -", count, webColor)); // Add trailing space & dash for aesthestics when displayed. Don't need count in label, unlike pies
        }

        private void UpdateChartPage1ByGender()
        {
            List<NameValueItem> arrivalsByGenderAllEvents = new List<NameValueItem>();
            arrivalsAdd(arrivalsByGenderAllEvents, "Male", outboxCountPerGenderAllEvents("Male"));
            arrivalsAdd(arrivalsByGenderAllEvents, "Female", outboxCountPerGenderAllEvents("Female"));
            arrivalsAdd(arrivalsByGenderAllEvents, "Unknown", outboxCountPerGenderAllEvents("Unknown"));
            arrivalsAdd(arrivalsByGenderAllEvents, "Complex", outboxCountPerGenderAllEvents("Complex"));

            ((PieSeries)this.PieChartByGenderAllEvents.Series[0]).ItemsSource = arrivalsByGenderAllEvents;
        }

        private void UpdateChartPage1ByAgeGroup()
        {
            List<NameValueItem> arrivalsByAgeGroupAllEvents = new List<NameValueItem>();
            arrivalsAdd(arrivalsByAgeGroupAllEvents, "Adult", outboxCountPerAgeGroupAllEvents("Adult"));
            arrivalsAdd(arrivalsByAgeGroupAllEvents, "Peds", outboxCountPerAgeGroupAllEvents("Youth")); // was: "Pediatric"
            arrivalsAdd(arrivalsByAgeGroupAllEvents, "Unknown", outboxCountPerAgeGroupAllEvents("Unknown Age Group"));

            ((PieSeries)this.PieChartByAgeGroupAllEvents.Series[0]).ItemsSource = arrivalsByAgeGroupAllEvents;
        }

        private void UpdateChartPage1ByEvent()
        {
            // Color assignment not stable, but OK for now
            List<NameValueItem> arrivalsByEventAllEvents = new List<NameValueItem>();
            outboxCountPerEventAllEvents(arrivalsByEventAllEvents);

            ((PieSeries)this.PieChartByEventAllEvents.Series[0]).ItemsSource = arrivalsByEventAllEvents;
        }

        private void UpdateChartPage2()
        {
            PiesForCurrentEvent.Text = "For Current Event \"" + App.CurrentDisaster.EventName + "\"";
            // For 4 piecharts on 2nd page:
            UpdateChartPage2ByZone();
            UpdateChartPage2ByGender();
            UpdateChartPage2ByAgeGroup();
            UpdateChartPage2ByDate();
        }

        private void UpdateChartPage2ByZone()
        {
            List<NameValueItem> arrivalsByZoneCurrentEvent = new List<NameValueItem>();
            // DOESN'T QUITE WORK IF YOU CARE ABOUT WHICH COLOR GETS ASSIGNED TO A CATEGORY:
            //   outboxCountPerZoneCurrentEvent(arrivalsByZoneCurrentEvent);
            arrivalsAdd(arrivalsByZoneCurrentEvent, "Green", outboxCountPerZoneCurrentEvent("Green"));
            arrivalsAdd(arrivalsByZoneCurrentEvent, "BH Green", outboxCountPerZoneCurrentEvent("BH Green"));
            arrivalsAdd(arrivalsByZoneCurrentEvent, "Yellow", outboxCountPerZoneCurrentEvent("Yellow"));
            arrivalsAdd(arrivalsByZoneCurrentEvent, "Red", outboxCountPerZoneCurrentEvent("Red"));
            arrivalsAdd(arrivalsByZoneCurrentEvent, "Gray", outboxCountPerZoneCurrentEvent("Gray"));
            arrivalsAdd(arrivalsByZoneCurrentEvent, "Black", outboxCountPerZoneCurrentEvent("Black"));

            // was: arrivalsByZoneCurrentEvent.Add(new NameValueItem { Name = "Green", Value = outboxCountPerZoneCurrentEvent("Green") });  etc.

            ((PieSeries)this.PieChartByTriageZoneCurrentEvent.Series[0]).ItemsSource = arrivalsByZoneCurrentEvent;
        }

        private void UpdateChartPage2ByGender()
        {
            List<NameValueItem> arrivalsByGenderCurrentEvent = new List<NameValueItem>();
            // DOESN'T QUITE WORK IF YOU CARE ABOUT WHICH COLOR GETS ASSIGNED TO A CATEGORY:
            //   outboxCountPerGenderCurrentEvent(arrivalsByGenderCurrentEvent);
            arrivalsAdd(arrivalsByGenderCurrentEvent, "Male", outboxCountPerGenderCurrentEvent("Male"));
            arrivalsAdd(arrivalsByGenderCurrentEvent, "Female", outboxCountPerGenderCurrentEvent("Female"));
            arrivalsAdd(arrivalsByGenderCurrentEvent, "Unknown", outboxCountPerGenderCurrentEvent("Unknown"));
            arrivalsAdd(arrivalsByGenderCurrentEvent, "Complex", outboxCountPerGenderCurrentEvent("Complex"));

            // was: arrivalsByGenderCurrentEvent.Add(new NameValueItem { Name = "Male", Value = outboxCountPerGenderCurrentEvent("Male") }); etc.

            ((PieSeries)this.PieChartByGenderCurrentEvent.Series[0]).ItemsSource = arrivalsByGenderCurrentEvent;
        }

        private void UpdateChartPage2ByAgeGroup()
        {
            List<NameValueItem> arrivalsByAgeGroupCurrentEvent = new List<NameValueItem>();
            // DOESN'T QUITE WORK IF YOU CARE ABOUT WHICH COLOR GETS ASSIGNED TO A CATEGORY:
            //   outboxCountPerAgeGroupCurrentEvent(arrivalsByAgeGRoupCurrentEvent);
            arrivalsAdd(arrivalsByAgeGroupCurrentEvent, "Adult", outboxCountPerAgeGroupCurrentEvent("Adult"));
            arrivalsAdd(arrivalsByAgeGroupCurrentEvent, "Peds", outboxCountPerAgeGroupCurrentEvent("Youth")); // was: "Pediatric"
            arrivalsAdd(arrivalsByAgeGroupCurrentEvent, "Unknown", outboxCountPerAgeGroupCurrentEvent("Unknown Age Group"));

            // was: arrivalsByAgeGroupCurrentEvent.Add(new NameValueItem { Name = "Adult", Value = outboxCountPerAgeGroupCurrentEvent("Adult") });  etc.

            ((PieSeries)this.PieChartByAgeGroupCurrentEvent.Series[0]).ItemsSource = arrivalsByAgeGroupCurrentEvent;
        }

        private void UpdateChartPage2ByDate()
        {
            // Color assignment not stable, but OK for now
            List<NameValueItem> arrivalsByDateCurrentEvent = new List<NameValueItem>();
            outboxCountPerDateCurrentEvent(arrivalsByDateCurrentEvent);

            ((PieSeries)this.PieChartByDateCurrentEvent.Series[0]).ItemsSource = arrivalsByDateCurrentEvent;
        }

        private void UpdateChartPage3()
        {
            UpdateChartPage3ByZone();
            UpdateChartPage3ByGender();
            UpdateChartPage3ByAgeGroup();
            UpdateChartPage3ByEvent();
        }

        private void UpdateChartPage3ByZone()
        {
            /* First try
                        List<NameValueItem> arrivalsByZoneAllEvents = new List<NameValueItem>();
                        arrivalsAdd(arrivalsByZoneAllEvents, "Green", outboxCountPerZoneAllEvents("Green"));
                        arrivalsAdd(arrivalsByZoneAllEvents, "BH Green", outboxCountPerZoneAllEvents("BH Green"));
                        arrivalsAdd(arrivalsByZoneAllEvents, "Yellow", outboxCountPerZoneAllEvents("Yellow"));
                        arrivalsAdd(arrivalsByZoneAllEvents, "Red", outboxCountPerZoneAllEvents("Red"));
                        arrivalsAdd(arrivalsByZoneAllEvents, "Gray", outboxCountPerZoneAllEvents("Gray"));
                        arrivalsAdd(arrivalsByZoneAllEvents, "Black", outboxCountPerZoneAllEvents("Black"));
             * 
                         ((BarSeries)this.BarChart.Series[0]).ItemsSource = arrivalsByZoneAllEvents;
            */
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            List<FieldsForBarChart> zoneBars = new List<FieldsForBarChart>();
            barsAdd(zoneBars, "Black", outboxCountPerZoneAllEvents("Black"));
            barsAdd(zoneBars, "Gray", outboxCountPerZoneAllEvents("Gray"));
            barsAdd(zoneBars, "Red", outboxCountPerZoneAllEvents("Red"));
            barsAdd(zoneBars, "Yellow", outboxCountPerZoneAllEvents("Yellow"));
            barsAdd(zoneBars, "BH Green", outboxCountPerZoneAllEvents("BH Green"));
            barsAdd(zoneBars, "Green", outboxCountPerZoneAllEvents("Green"));

            BarSeries bs = ((BarSeries)this.BarChartByTriageZoneAllEvents.Series[0]);
            bs.ItemsSource = zoneBars;

            // Setting the interval (and reportedly Min and Max too) causes an exception in XAML, because they are nullable types; do it in code-behind instead.
            // Also reportedly (https://winrtxmltookkit.codeplex.com/discussions/433978) orientation must be stated:
            bs.DependentRangeAxis = new LinearAxis()
                { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis()
                { FontSize = 6, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage3ByGender()
        {
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            List<FieldsForBarChart> genderBars = new List<FieldsForBarChart>();
            // For color choices, compare with pie chart GenderPallete style
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            barsAdd(genderBars, "Complex", "Purple", outboxCountPerGenderAllEvents("Complex"));
            barsAdd(genderBars, "Unknown", "Tan", outboxCountPerGenderAllEvents("Unknown"));
            barsAdd(genderBars, "Female", "Pink", outboxCountPerGenderAllEvents("Female"));
            barsAdd(genderBars, "Male", "Blue", outboxCountPerGenderAllEvents("Male"));

            BarSeries bs = ((BarSeries)this.BarChartByGenderAllEvents.Series[0]);
            bs.ItemsSource = genderBars;
            bs.DependentRangeAxis = new LinearAxis()
                { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 12 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis()
                { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage3ByAgeGroup()
        {
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            List<FieldsForBarChart> ageGroupBars = new List<FieldsForBarChart>();
            // For color choices, compare with pie chart AgeGroupPallete style
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            barsAdd(ageGroupBars, "Unknown", "Tan", outboxCountPerAgeGroupAllEvents("Unknown Age Group"));
            barsAdd(ageGroupBars, "Peds", "Teal", outboxCountPerAgeGroupAllEvents("Youth")); // was before June 2015: Pediatric
            barsAdd(ageGroupBars, "Adult", "Brown", outboxCountPerAgeGroupAllEvents("Adult"));

            BarSeries bs = ((BarSeries)this.BarChartByAgeGroupAllEvents.Series[0]);
            bs.ItemsSource = ageGroupBars;
            bs.DependentRangeAxis = new LinearAxis()
                { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 13 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis()
                { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead

        }


        private void UpdateChartPage3ByEvent()
        {
            // Color assignment not stable, but OK for now
            List<FieldsForBarChart> eventBars = new List<FieldsForBarChart>();
            outboxCountPerEventAllEventsForBarChart(eventBars);

            BarSeries bs = ((BarSeries)this.BarChartByEventAllEvents.Series[0]);
            bs.ItemsSource = eventBars;
            bs.DependentRangeAxis = new LinearAxis()
                { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 12 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis()
                { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage4()
        {
            BarsForCurrentEvent.Text = "For Current Event \"" + App.CurrentDisaster.EventName + "\"";
            UpdateChartPage4ByZone();
            UpdateChartPage4ByGender();
            UpdateChartPage4ByAgeGroup();
            UpdateChartPage4ByEvent();
        }

        private void UpdateChartPage4ByZone()
        {
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            List<FieldsForBarChart> zoneBarsCurrent = new List<FieldsForBarChart>();
            barsAdd(zoneBarsCurrent, "Black", outboxCountPerZoneCurrentEvent("Black"));
            barsAdd(zoneBarsCurrent, "Gray", outboxCountPerZoneCurrentEvent("Gray"));
            barsAdd(zoneBarsCurrent, "Red", outboxCountPerZoneCurrentEvent("Red"));
            barsAdd(zoneBarsCurrent, "Yellow", outboxCountPerZoneCurrentEvent("Yellow"));
            barsAdd(zoneBarsCurrent, "BH Green", outboxCountPerZoneCurrentEvent("BH Green"));
            barsAdd(zoneBarsCurrent, "Green", outboxCountPerZoneCurrentEvent("Green"));

            BarSeries bs = ((BarSeries)this.BarChartByTriageZoneCurrentEvent.Series[0]);
            bs.ItemsSource = zoneBarsCurrent;

            // Setting the interval (and reportedly Min and Max too) causes an exception in XAML, because they are nullable types; do it in code-behind instead.
            // Also reportedly (https://winrtxmltookkit.codeplex.com/discussions/433978) orientation must be stated:
            bs.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis() { FontSize = 6, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage4ByGender()
        {
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            List<FieldsForBarChart> genderBarsCurrent = new List<FieldsForBarChart>();
            // For color choices, compare with pie chart GenderPallete style
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            barsAdd(genderBarsCurrent, "Complex", "Purple", outboxCountPerGenderCurrentEvent("Complex"));
            barsAdd(genderBarsCurrent, "Unknown", "Tan", outboxCountPerGenderCurrentEvent("Unknown"));
            barsAdd(genderBarsCurrent, "Female", "Pink", outboxCountPerGenderCurrentEvent("Female"));
            barsAdd(genderBarsCurrent, "Male", "Blue", outboxCountPerGenderCurrentEvent("Male"));

            BarSeries bs = ((BarSeries)this.BarChartByGenderCurrentEvent.Series[0]);
            bs.ItemsSource = genderBarsCurrent;
            bs.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 12 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis() { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage4ByAgeGroup()
        {
            // Coloring of individual bars done as suggested by:
            // http://social.msdn.microsoft.com/Forums/en-US/silverlightcontrols/thread/dd46b472-5bc3-4e14-a72a-9cd1aff6149d
            // "Solution For Multi Color BarChart depends on the Value Using WCF", Kumar Sirangi
            List<FieldsForBarChart> ageGroupBarsCurrent = new List<FieldsForBarChart>();
            // For color choices, compare with pie chart AgeGroupPallete style
            // Top-to-bottom code order here will be bottom-to-top when displayed on chart:
            barsAdd(ageGroupBarsCurrent, "Unknown", "Tan", outboxCountPerAgeGroupCurrentEvent("Unknown Age Group"));
            barsAdd(ageGroupBarsCurrent, "Peds", "Teal", outboxCountPerAgeGroupCurrentEvent("Youth")); // "Pediatric";
            barsAdd(ageGroupBarsCurrent, "Adult", "Brown", outboxCountPerAgeGroupCurrentEvent("Adult"));

            BarSeries bs = ((BarSeries)this.BarChartByAgeGroupCurrentEvent.Series[0]);
            bs.ItemsSource = ageGroupBarsCurrent;
            bs.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 13 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis() { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead

        }


        private void UpdateChartPage4ByEvent()
        {
            // Color assignment not stable, but OK for now
            List<FieldsForBarChart> dateBarsCurrent = new List<FieldsForBarChart>();
            outboxCountPerDateCurrentEventForBarChart(dateBarsCurrent);

            BarSeries bs = ((BarSeries)this.BarChartByDateCurrentEvent.Series[0]);
            bs.ItemsSource = dateBarsCurrent;
            bs.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.X, ShowGridLines = true, FontSize = 12 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            bs.IndependentAxis = new CategoryAxis() { FontSize = 8, MajorTickMarkStyle = this.Resources["charting:HideTicMarks"] as Style, Orientation = AxisOrientation.Y }; // Pseudo ticmarks done with dash in labels instead
        }

        private void UpdateChartPage5()
        {
            List<DateValueItem> arrivalsLine = new List<DateValueItem>();
            outboxCountPerDayCurrentEvent(arrivalsLine);
            LineSeries ls = ((LineSeries)this.LineChartArrivals.Series[0]);
                
            ls.ItemsSource = arrivalsLine;  // this is already sorted by local datetime, so just pick off min and max values
            if (arrivalsLine.Count == 0)
                minDT = maxDT = DateTime.Today; // just to avoid crashing
            else
            {
                minDT = arrivalsLine[0].DateTimeBucket;
                maxDT = arrivalsLine[arrivalsLine.Count-1].DateTimeBucket;
            }
            minDT = minDT.AddDays(-2.0);
            maxDT = maxDT.AddDays(2.0);
            WidenRange(); // may adjust minDT, maxDT

            // Setting the interval (and reportedly Min and Max too) causes an exception in XAML, because they are nullable types; do it in code-behind instead.
            // Also reportedly (https://winrtxmltookkit.codeplex.com/discussions/433978) orientation must be stated:
           ls.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.Y, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
           ls.IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 10, Orientation = AxisOrientation.X };

        }

        private void UpdateChartPage6()
        {
            List<DateValueItem> arrivalsLine = new List<DateValueItem>();
            outboxCountPerDayAllEvents(arrivalsLine);
            LineSeries ls = ((LineSeries)this.LineChartArrivalsAllStations.Series[0]);

            ls.ItemsSource = arrivalsLine;// this is already sorted by local datetime, so just pick off min and max values
            if (arrivalsLine.Count == 0)
                minDT = maxDT = DateTime.Today; // just to avoid crashing
            else
            {
                minDT = arrivalsLine[0].DateTimeBucket;
                maxDT = arrivalsLine[arrivalsLine.Count - 1].DateTimeBucket;
            }
            minDT = minDT.AddDays(-2.0);
            maxDT = maxDT.AddDays(2.0);
            WidenRange(); // may adjust minDT, maxDT

            // Setting the interval (and reportedly Min and Max too) causes an exception in XAML, because they are nullable types; do it in code-behind instead.
            // Also reportedly (https://winrtxmltookkit.codeplex.com/discussions/433978) orientation must be stated:
            ls.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.Y, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            ls.IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 10, Orientation = AxisOrientation.X };

#if FAILED_ON_SERIES_1
            // Try another 1 with same content:
            LineSeries ls1 = ((LineSeries)this.LineChartArrivalsAllStations.Series[1]);

            ls1.ItemsSource = arrivalsLine;

            // Setting the interval (and reportedly Min and Max too) causes an exception in XAML, because they are nullable types; do it in code-behind instead.
            // Also reportedly (https://winrtxmltookkit.codeplex.com/discussions/433978) orientation must be stated:
            //ls1.DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.Y, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
            //ls1.IndependentAxis = new DateTimeAxis() { FontSize = 10, Orientation = AxisOrientation.X };
#endif

        }


        private void UpdateChartPage7()
        {
            string eventName = "";
            // No longer needed, now that we have App.PatientDataGroups.GetOutboxForStatistics():
            // // sorting added June 2015, so call to outboxCountPerDaySpecificEventSet in UpdateChartPage7Impl will work right
            // List<TP_PatientReport> orig = App.PatientDataGroups.GetOutbox().GetAsList();
            // List<TP_PatientReport> sort = new List<TP_PatientReport>();
            // sort = orig.OrderBy(o => o.WhenLocalTime).ToList();
            // //sort = orig.OrderByDescending(o => o.WhenLocalTime).ToList();
            // TP_PatientReports pdl = new TP_PatientReports();
            // pdl.ReplaceWithList(sort);
            TP_PatientReports pdl = App.PatientDataGroups.GetOutboxForStatistics();
            TP_PatientReports results = new TP_PatientReports();
            TP_EventsDataList edl = App.CurrentDisasterList;
            // Across events.  Widen range by at least 2 days to prevent stupid clipping of line segments and/or data points.
            minDT = FindMinimumDate().AddDays(-2.0);
            maxDT = FindMaximumDate().AddDays(2.0);
            WidenRange(); // May adjust minDT, maxDT
            List<DateValueItem>[] arrivalLines = new List<DateValueItem>[10]; // Handle up to 10 events first draft
            LineSeries[] ls = new LineSeries[10];
            int i = 0;
            foreach (var edi in edl)
            {
                eventName = edi.EventName;
                foreach (var pdi in pdl)
                {
                    if (pdi.EventName == eventName)
                        results.Add(pdi);
                }
                if (results.Count() > 0)
                {
                    arrivalLines[i] = new List<DateValueItem>();
                    ls[i] = new LineSeries();
                    UpdateChartPage7Impl(eventName, results, ls[i], arrivalLines[i]);
/* FIRST DRAFT, THEN MOVED
                    if (i == 0)
                    {
                        ls[i].IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 10, Orientation = AxisOrientation.X, Location = AxisLocation.Bottom }; // explicitly state bottom so won't end up with top & bottom labels
                        ls[i].DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.Y, ShowGridLines = true, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
                    }
                    else
                    {
                        ls[i].IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 0.1, Orientation = AxisOrientation.X, Location = AxisLocation.Bottom }; // explicitly state bottom so won't end up with top & bottom labels
                        // When redraws, grid lines are not perfectly atop each other, so suppress
                        ls[i].DependentRangeAxis = new LinearAxis() { Minimum = 0, Interval = 1.0, Orientation = AxisOrientation.Y, ShowGridLines = false, FontSize = 0.1 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
                    }
                    //ls[i].DataPointStyle = GetDataPointStyleWithNoPointsRandomLineColors();
                    LineChartArrivalsAllStations_Page7.Series.Add(ls[i]);
 */
                    results.Clear();
                    i++;
                }
            }
            // Can't rely on auto-scaling to work right with multilines if we're setting axis (in order to set interval)
            // So calculate max count here across events, for benefit of setting axes:
            int maxCount = 0;
            for(int j=0; j < i; j++)
            {
                foreach(var dvi in arrivalLines[j])
                {
                    if(dvi.Value > maxCount)
                        maxCount = dvi.Value;
                }
            }
            // Prettify:
            double yinterval = 1.0;
            if (maxCount <= 10)
            {
                maxCount = 10;
            }
            else if (maxCount <= 25)
            {
                maxCount = 25;
                yinterval = 5.0;
            }
            else if (maxCount <= 50)
            {
                maxCount = 50;
                yinterval = 5.0;
            }
            else if (maxCount <= 100)
            {
                maxCount = 100;
                yinterval = 10.0;
            }
            else
            {
                maxCount = (maxCount % 100) * 100;
                yinterval = 25.0;
            }

            // Set axes & finish
            for (int j = 0; j < i; j++)
            {
                if (j == 0)
                {
                    ls[j].IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 10, Orientation = AxisOrientation.X, Location = AxisLocation.Bottom }; // explicitly state bottom so won't end up with top & bottom labels
                    ls[j].DependentRangeAxis = new LinearAxis() { Minimum = 0, Maximum = maxCount, Interval = yinterval, Orientation = AxisOrientation.Y, ShowGridLines = true, Location = AxisLocation.Left, FontSize = 11 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
                }
                else
                {
                    ls[j].IndependentAxis = new DateTimeAxis() { Minimum = minDT, Maximum = maxDT, FontSize = 0.1, Orientation = AxisOrientation.X, Location = AxisLocation.Bottom }; // explicitly state bottom so won't end up with top & bottom labels
                    // When redraws, grid lines are not perfectly atop each other, so suppress
                    ls[j].DependentRangeAxis = new LinearAxis() { Minimum = 0, Maximum = maxCount, Interval = yinterval, Orientation = AxisOrientation.Y, ShowGridLines = true, Location=AxisLocation.Left, FontSize = 0.1 }; // BorderThickness = new Thickness(2.0), BorderBrush = new SolidColorBrush(Colors.White) };
                }
                //ls[i].DataPointStyle = GetDataPointStyleWithNoPointsRandomLineColors();
                LineChartArrivalsAllStations_Page7.Series.Add(ls[j]);
            }

        }

        /// <summary>
        /// Looks at all open events in the Outbox, finds earliest day (in local time)
        /// </summary>
        /// <returns></returns>
        private DateTime FindMinimumDate()
        {
            string eventName = "";
            DateTime results = DateTime.MaxValue;
            TP_PatientReports outbx = App.PatientDataGroups.GetOutboxForStatistics();
            // No longer needed, now that we have App.PatientDataGroups.GetOutboxForStatistics():
            // TP8.Data.TP_PatientReports outbx2 = new TP8.Data.TP_PatientReports();
            // foreach (var i in outbx)
            // {
            //    if (String.IsNullOrEmpty(i.WhenLocalTime))
            //        continue; // probably weird empty record
            //    i.WhenLocalTime = ParseDate(i.WhenLocalTime); // take first 10 characters, ignore UTC offset
            //    outbx2.Add(i); // Hmmm, are we changing outbox list itself, removing UTC offset? Maybe not intended side effect.
            //}
            TP_EventsDataList edl = App.CurrentDisasterList;
            foreach (var edi in edl)
            {
                eventName = edi.EventName;
                foreach (var pdi in outbx) // outbx2)
                {
                    // As of Release 6, pdi.WhenLocalTime in this loop replaced by pdi.WhenLocalTimeMsg1, i.e., arrival time
                    if (pdi.EventName == eventName)
                        if (DateTime.Parse(pdi.WhenLocalTimeMsg1) < results) // might work
                            results = DateTime.Parse(pdi.WhenLocalTimeMsg1);
                }
            }
            return results;
        }

        /// <summary>
        /// Looks at all open events in the Outbox, finds latest day (in local time)
        /// </summary>
        /// <returns></returns>
        private DateTime FindMaximumDate()
        {
            string eventName = "";
            DateTime results = DateTime.MinValue;
            TP_PatientReports outbx = App.PatientDataGroups.GetOutboxForStatistics();
            // No longer needed now that we have App.PatientDataGroups.GetOutboxForStatistics():
            // TP_PatientReports outbx = App.PatientDataGroups.GetOutboxForStatistics();
            // TP8.Data.TP_PatientReports outbx2 = new TP8.Data.TP_PatientReports();
            // foreach (var i in outbx)
            // {
            //    if (String.IsNullOrEmpty(i.WhenLocalTime))
            //        continue; // probably weird empty record
            //    i.WhenLocalTime = ParseDate(i.WhenLocalTime); // take first 10 characters, ignore UTC offset
            //    outbx2.Add(i); // Hmmm, are we changing outbox list itself, removing UTC offset? Maybe not intended side effect.
            // }
            TP_EventsDataList edl = App.CurrentDisasterList;
            foreach (var edi in edl)
            {
                eventName = edi.EventName;
                foreach (var pdi in outbx) //outbx2)
                {
                    // As of Release 6, pdi.WhenLocalTime in this loop replaced by pdi.WhenLocalTimeMsg1, i.e., arrival time
                    if (pdi.EventName == eventName)
                        if (DateTime.Parse(pdi.WhenLocalTimeMsg1) > results) // might work
                            results = DateTime.Parse(pdi.WhenLocalTimeMsg1);
                }
            }
            return results;
        }

        /// <summary>
        /// May adjust class variables maxDT, minDT
        /// </summary>
        private void WidenRange()
        {
            TimeSpan ts = maxDT - minDT;
            int days = ts.Days;
            if ((days % 2) == 1)
                ++days; // round up to even number
            int targetDays = 0;
            // Further widen range in an adhoc way as cheap hack to force x axis to be days only, instead of hours
            // TO DO: arrange recalculation if user changes app width
            switch (App.CurrentVisualState)
            {
                // use only even numbers as targets
                case "FullScreenPortrait":
                    targetDays = 10; break;
                case "FullScreenLandscape":
                case "Over1365Wide":
                    targetDays = 20; break;
                case "vs1026To1365Wide":
                case "vsvs673To1025Wide":
                    targetDays = 16; break;
                default:
                    targetDays = 8; break;
            }
            if (targetDays - days <= 0)
                return; // no adjustment needed

            int padDays = (targetDays - days) / 2; // divide by 2 for left and right pads
            minDT = minDT.AddDays(-padDays);
            maxDT = maxDT.AddDays(padDays);
        }

        private void UpdateChartPage7Impl(string eventName, TP_PatientReports arrivalsOneEvent, LineSeries series, List<DateValueItem> arrivalLine)
        {
            outboxCountPerDaySpecificEventSet(arrivalsOneEvent, arrivalLine);
            series.ItemsSource = arrivalLine;

            series.Name = "Chart 7 " + eventName;
            series.Title = eventName;
            // should these be "Name" or "Value" instead of "Path"?
            series.IndependentValueBinding = new Binding();
            series.DependentValueBinding = new Binding();
            series.IndependentValueBinding.Path = new PropertyPath("DateTimeBucket");
            series.DependentValueBinding.Path = new PropertyPath("Value");
/* MAYBENOT
            series.IndependentValuePath = "DateTimeBucket";
            series.DependentValuePath = "Value";
 */
            series.IsSelectionEnabled = true;
        }

        private static Style GetDataPointStyleWithNoPointsRandomLineColors()
        {
            // If you suppress data points, then all line colors are yellow-orange by default.  Replace with random colors.
            // Adapted from http://stackoverflow.com/questions/5956564/wpf-toolkit-line-chart-without-points-and-with-different-colors
            // By Sanguin Yin, May 17 2011
            Random ran = new Random();
            Color background = Color.FromArgb(255, (byte)ran.Next(255), (byte)ran.Next(255), (byte)ran.Next(255));
            Style style = new Style(typeof(DataPoint));
            Setter st1 = new Setter(DataPoint.BackgroundProperty, new SolidColorBrush(background));
            Setter st2 = new Setter(DataPoint.BorderBrushProperty, new SolidColorBrush(Colors.White));
            Setter st3 = new Setter(DataPoint.BorderThicknessProperty, new Thickness(0));
            Setter st4 = new Setter(DataPoint.HeightProperty, 0);
            Setter st5 = new Setter(DataPoint.WidthProperty, 0);
            //Setter st6 = new Setter(DataPoint.TemplateProperty, null); // causes exception
            style.Setters.Add(st1); style.Setters.Add(st2); style.Setters.Add(st3); style.Setters.Add(st4); style.Setters.Add(st5);
            return style;
        }
/*
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="charting:LineDataPoint">
                        <Grid x:Name="Root" Opacity="0" />
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
*/
        private new void GoBack(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        public class NameValueItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public class DateValueItem
        {
            public DateTime DateTimeBucket { get; set; }
            public int Value { get; set; }
        }

        private void OnUpdateButtonClick(object sender, RoutedEventArgs e)
        {
            UpdateCharts();
        }

        // Next set of function compensate for lousy choice of recoloring of pie segment after touch
        private void PieChartByTriageZoneAllEvents_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage1ByZone();
        }

        private void PieChartByGenderAllEvents_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage1ByGender();
        }

        private void PieChartByAgeGroupAllEvents_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage1ByAgeGroup();
        }

        private void PieChartByEventAllEvents_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage1ByEvent();
        }

        private void PieChartByTriageZoneCurrentEvent_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage2ByZone();
        }

        private void PieChartByGenderCurrentEvent_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage2ByGender();
        }

        private void PieChartByAgeGroupCurrentEvent_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage2ByAgeGroup();
        }

        private void PieChartByDateCurrentEvent_PointerReleased(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UpdateChartPage2ByDate();
        }

    }
}
