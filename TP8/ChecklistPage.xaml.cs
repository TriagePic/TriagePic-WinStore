using TP8.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Collections.Specialized;
using Windows.ApplicationModel.Contacts;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TP8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class BasicPageChecklist : TP8.Common.LayoutAwarePage
    {
        public BasicPageChecklist()
        {
            this.InitializeComponent();
            //crash: EventComboBox.DataContext = App.CurrentDisasterListForCombo;
            EventComboBox.ItemsSource = App.CurrentDisasterListForCombo;
            EventComboBox.DataContext = App.CurrentDisasterListForCombo;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if GIVEUP
            // With ItemsSource="{Binding}" in XAML
            Binding bind = new Binding();
            bind.Source = App.CurrentDisasterList;
            bind.Mode = BindingMode.OneTime; // You would think oneway would protect App.CurrentDisasterList from changes, but it doesn't
            EventComboBox.SetBinding(ComboBox.ItemsSourceProperty, bind);

#endif
            //EventComboBox.ItemsSource = App.CurrentDisasterList; //Binding to List is good for unchanging collection.  Bind to ObservableCollection if changing.
            /// No: EventComboBox.DataContext = App.CurrentDisasterList;  also crashes: App.CurrentDisasterListForCombo
            //foreach (TP_EventsDataItem i in App.CurrentDisasterList)
            //    EventComboBox.Items.Add(i);




            if(App.CurrentDisaster.EventName == "")
                return;
            int count = 0;
            foreach (var i in App.CurrentDisasterList) //TP_EventsDataList.GetEvents())
            {
                if (i.EventName == App.CurrentDisaster.EventName)  // Could match instead throughout on EventShortName or EventNumericID
                {
                    EventComboBox.SelectedIndex = count; //EventComboBox.SelectedItem = i;
                    break;
                }
                count++;
            }
        }

        private void RosterNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            App.RosterNames = RosterTextBox.Text;
        }

        private async void PickButton_Click(object sender, RoutedEventArgs e) // Could return "Task"
        {
            ContactPicker cp = new ContactPicker();
            cp.CommitButtonText = "Select"; // "OK" is default
            //cp.SelectionMode = ContactSelectionMode.Fields;
            //cp.DesiredFields.Add(KnownContactField.Email);
            //obsolete after 8.0: var contacts = await cp.PickMultipleContactsAsync();
            var contacts = await cp.PickContactsAsync();

            if (contacts == null || contacts.Count == 0)
            {
                // No new contacts; but don't erase any existing ones
                return;
            }
            foreach (var contact in contacts)
            {
                if (!String.IsNullOrEmpty(RosterTextBox.Text))
                    RosterTextBox.Text += "\n"; // was: "; ";
                RosterTextBox.Text += contact.Name;
            }

        }

        #region TopAppBar
        // Attempts to make nav bar global not yet successful
        private void Checklist_Click(object sender, RoutedEventArgs e) // at moment, Home icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageChecklist), "pageChecklist");
        }

        private void New_Click(object sender, RoutedEventArgs e)  // at moment, Webcam icon on nav bar
        {
            this.Frame.Navigate(typeof(BasicPageNew), "pageNewReport");
        }

        private void AllStations_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "AllStations"); // Defined in SampleDataSource.cs
        }

        private void Outbox_Click(object sender, RoutedEventArgs e) // at moment, List icon on nav bar
        {
            this.Frame.Navigate(typeof(SplitPage), "Outbox"); // Defined in SampleDataSource.cs
        }

        private void Statistics_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(SplitPage), "Statistics"); // Defined in SampleDataSource.cs
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(HomeItemsPage), "AllGroups"); //"pageRoot");
        }
        #endregion

        private async void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventComboBox.SelectedItem != null)
            {
                TP8.Data.TP_EventsDataItem tpEvent = (TP_EventsDataItem)EventComboBox.SelectedItem;
                if (App.CurrentDisaster.EventName == tpEvent.EventName)
                    return; // no real work to do.  May be the case when we begin visit to checklist page.  New May 2014

                App.CurrentDisaster.TypeIconUri = tpEvent.TypeIconUri;
                App.CurrentDisaster.EventName = tpEvent.EventName;
                App.CurrentDisaster.EventType = tpEvent.EventType; // will this work?  instead of TP_EventsDataItem.GetEventTypeFromIconUri(tpEvent.TypeIconUri);
                App.CurrentDisaster.EventShortName = tpEvent.EventShortName; // will this work?
                // Persist it:
                App.CurrentOtherSettings.CurrentEventName = tpEvent.EventName;
                App.CurrentOtherSettings.CurrentEventShortName = tpEvent.EventShortName;
                App.CurrentOtherSettingsList.UpdateOrAdd(App.CurrentOtherSettings);
                await App.CurrentOtherSettingsList.WriteXML();

                // Invalid cache data when current event changes
                // WAS, but this now included in next step: await App.PatientDataGroups.PurgeCachedAllStationsList();
                await App.PatientDataGroups.ProcessAllStationsList(App.pd.plUserName, App.pd.plPassword, false, true); // = on startup; invalidate cache first
            }
        }

    }
}
