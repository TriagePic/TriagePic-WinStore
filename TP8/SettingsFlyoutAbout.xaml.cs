using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
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
    public sealed partial class SettingsFlyoutAbout : SettingsFlyout
    {
        public SettingsFlyoutAbout()
        {
            this.InitializeComponent();
            App.MyAssert(!String.IsNullOrEmpty(App.StorePackageVersionMajorDotMinor));
            this.VersionInfo.Text = "Version: " + App.StorePackageVersionMajorDotMinor; // code that was here moved to App.GetStorePackageVersionMajorDotMinor in June 2015 v 3.4
        }
    }
}
