using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace TP8
{
    public sealed partial class SettingsFlyoutOptionsLocal : SettingsFlyout
    {
        public SettingsFlyoutOptionsLocal()
        {
            InitializeComponent();
            string s =
                "  TriagePic Reports to:\n  " + App.pl.Endpoint.Address.Uri.AbsoluteUri; //.Replace("&", "&&") +
            TextBlockEndpoints.Text = s;
            // Ampersand in "app.config" [really TriagePic.exe.config] represented as &amp;, but it's "&" when we get here.
            // Need to double it so that "&: is not treated as "underline next char" in link [doubling not needed for string)
            s =
            "\nEndpoint Address Customized?  " + (String.IsNullOrEmpty(App.CurrentOtherSettings.PLEndPointAddress) ? "No" : "Yes") + "\n" +
            "Customizable by device admin in OtherSettings.XML";

            blockToggle.IsOn = App.BlockWebServices; // Probably could just instead do in XAML IsOn="{Binding App.BlockWebServices}"
        }

        private void blockToggle_Toggled(object sender, RoutedEventArgs e)
        {
            App.BlockWebServices = ((ToggleSwitch)sender).IsOn; // Probably could just instead do in XAML IsOn="{Binding App.BlockWebServices}"
        }
    }
}
