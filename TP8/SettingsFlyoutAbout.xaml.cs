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
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;
            this.VersionInfo.Text = "Version: " + version.Major.ToString() + "." + version.Minor.ToString();
            string forDeveloperToSeeInDebugger = version.Major.ToString() + "." + version.Minor.ToString() + "." + version.Build.ToString() + "." + version.Revision.ToString();
            // Keep in mind that this is the package version number, set with Store/"Edit App Manifes"t and revision-incremented during Store/"Create App Packages"...
            // NOT the VS Build number (editable in Project/TP8 Properties/Assembly Information).
            // However, try to by hand:
            // Keep Store.major = VS.major = currently 3 (and ideally matches TP7.Major)
            // Keep Store.minor = VS.minor = (upcoming or done) Store release #
            // Don't care about Build & Revision #s all that much, and compiling and packaging will use unrelated sets of values.
            // For TP7, compiler generated them so that they represented the compile date, and that was conveyed to user.  Don't know if that makes sense for TP8.
        }
    }
}
