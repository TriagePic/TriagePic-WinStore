using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TP8
{
    public sealed partial class ZoneButton : UserControl
    {

        static ZoneButton()
        {
            // Give these different names than button properties so don't have to deal with override issues
            BackgroundColorProperty =
                DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(ZoneButton), new PropertyMetadata(Colors.Cyan));

            ContentAutoForegroundProperty =
                DependencyProperty.Register("ContentAutoForeground", typeof(string), typeof(ZoneButton), new PropertyMetadata("Dummy Text"));

            CheckMarkVisibilityProperty =
                DependencyProperty.Register("CheckMarkVisibility", typeof(Visibility), typeof(ZoneButton), new PropertyMetadata(Visibility.Visible));

            ZoneButtonWidthProperty =
                DependencyProperty.Register("ZoneButtonWidth", typeof(int), typeof(ZoneButton), new PropertyMetadata(130));
        }

        public static DependencyProperty BackgroundColorProperty { private set; get; }
        public static DependencyProperty ContentAutoForegroundProperty { private set; get; }
        public static DependencyProperty CheckMarkVisibilityProperty { private set; get; }
        public static DependencyProperty ZoneButtonWidthProperty { private set; get; }

        public ZoneButton()
        {
            this.InitializeComponent();
/* NOT WORKING FOR ME
            Binding contentBinding = new Binding();
            contentBinding.Source = this;
            contentBinding.Path = new PropertyPath("Content");
            //contentBinding.Mode = BindingMode.OneWay;
            button.SetBinding(Button.ContentProperty, contentBinding);


            Binding backgroundBinding = new Binding();
            backgroundBinding.Source = this;
            backgroundBinding.Path = new PropertyPath("Background");
            //backgroundBinding.Mode = BindingMode.OneWay;
            button.SetBinding(Button.BackgroundProperty, backgroundBinding);

            Binding foregroundBinding = new Binding();
            foregroundBinding.Source = this;
            foregroundBinding.Path = new PropertyPath("Background");
            // Application.Current.Resources["..."] in global file
            foregroundBinding.Converter = (IValueConverter)Resources["foregroundFromBackground2"]; //{StaticResource foregroundFromBackground};
            //foregroundBinding.Mode = BindingMode.OneWay;
            button.SetBinding(Button.ForegroundProperty, foregroundBinding);

            Binding checkMarkBinding = new Binding();
            checkMarkBinding.Source = this.button;
            checkMarkBinding.Path = new PropertyPath("Background");
            checkMarkBinding.Converter = (IValueConverter)Resources["foregroundFromBackground2"]; //{StaticResource foregroundFromBackground};
            //checkMarkBinding.Mode = BindingMode.OneWay;
            checkMark.SetBinding(Button.ForegroundProperty, checkMarkBinding);
 */

        }

        public ZoneButton(string Name_, Color BackgroundColor_, string ContentAutoForeground_)
        {
            this.InitializeComponent();
            this.Name = Name_;
            this.BackgroundColor = BackgroundColor_;
            this.ContentAutoForeground = ContentAutoForeground_;
        }


        public event RoutedEventHandler Click
        {
            add { this.button.Click += value; }
            remove { this.button.Click -= value; }
        }

        public Color BackgroundColor // differs from Background which is SolidBrush
        {
            set {
                SetValue(BackgroundColorProperty, value);
                this.button.Background = new SolidColorBrush(value);
            }
            get { return (Color)GetValue(BackgroundColorProperty); }
        }

        public string ContentAutoForeground
        {
            set {
                SetValue(ContentAutoForegroundProperty, value);
                this.button.Content = value;
            }
            get { return (string)GetValue(ContentAutoForegroundProperty); }
        }

        public Visibility CheckMarkVisibility
        {
            set
            {
                SetValue(CheckMarkVisibilityProperty, value);
                this.checkMark.Visibility = value;
            }
            get { return (Visibility)GetValue(CheckMarkVisibilityProperty); }
        }

        public int ZoneButtonWidth
        {
            set
            {
                SetValue(ZoneButtonWidthProperty, value);
                this.button.Width = value;
                double dvalue = value;
                this.checkMark.SetValue(Canvas.LeftProperty, (int)(dvalue * 0.75));
            }
            get { return (int)GetValue(ZoneButtonWidthProperty); }
        }


        /// <summary>
        /// Estimates perceived color brightness, based on HSP algorithm, but returns 0-255 value "squared"
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Value from 0 to 255</returns>
        public static int PerceivedBrightnessSquared(Color c)
        {
            // See commentary below for more
            return (int)(
                c.R * c.R * .241 +
                c.G * c.G * .691 +
                c.B * c.B * .068);
        }

        public void SetToolTip(ToolTip t)
        {
            ToolTipService.SetToolTip(button, t);
        }
    }



 

    /// <summary>
    /// Returns a color for font to use, either black or white, based on background color.  Used for Zone buttons
    /// </summary>
    public class ForegroundFromBackgroundColorConverter : IValueConverter
    {
        // Usage from XAML:
        // <Button x:Name="GreenZone" Content="Green" Foreground="{Binding Background, ElementName=GreenZone, Converter={StaticResource foregroundFromBackground}}" ... />
        // Adapted from converter going the opposite way at
        // http://stackoverflow.com/questions/6763032/how-to-pick-a-background-color-depending-on-font-color-to-have-proper-contrast
        // Note that the original method passed a Color value, so was called with Background.Color in the XAML
        public object Convert(object value, Type targetType, object parameter, string s)
        {
            //if (!(value is Color))
            if (!(value is SolidColorBrush))
                return new SolidColorBrush(Colors.Blue);  // if a binding problem, show font in blue as alert to programmer

            //Color backColor = (Color)value;
            SolidColorBrush scb = (SolidColorBrush)value;
            Color backColor = scb.Color;
            if (ZoneButton.PerceivedBrightnessSquared(backColor) > 34225) // 185^2 = 34225.  Using 185 as cutoff instead of 130
                // to favor more white fonts (will appear with about half of possible background colors)
                return new SolidColorBrush(Colors.Black);
            return new SolidColorBrush(Colors.White);
        }

        public object ConvertBack(object value, Type targeType, object parameter, string s)
        {
            throw new NotImplementedException();
        }
    }



/* SAVE FOR COMMENTARY:
        Color foreColor = (PerceivedBrightness(backColor) > 130 ? Color.Black : Color.White);
        // 130 is arbitrary cutoff, favors more black foreColors (about 3/4 black out of .Net 3 colors shown in nbdtech table at www.nbdtech.com/images/blog/20080427/AllColors.png)
        // In eyeballing that table (which includes calculated value), 185 would give about 50%, and probably as high as you'd want to go.
        // Higher cutoff gives more white foreColors

        /// <summary>
        /// Estimates perceived color brightness, based on HSP algorithm
        /// </summary>
        /// <param name="c"></param>
        /// <returns>Value from 0 to 255</returns>
        private int PerceivedBrightness(Color c)
        {
            // from http://stackoverflow.com/questions/2241447/make-foregroundcolor-black-or-white-depending-on-background 
            // See also www.nbdtech.com/Blog/archive/2008/04/27/Calculating-the-Perceived-Brightness-of-a-Color.aspx for reasoning
            // Also has a table of example swatches with cutoff at 130 .
            // In the HSP algorithm, the weighting are the standard ones for luma (aka Y), but applied as weighted distance in 3D RGB space,
            // reportedly to give more weight to extremes.
            // Note that the weightings given here are for the original version of the HSP color model, and that the author
            // subsequently said that "I have been informed that 0.299, 0.587, and 0.114 are more accurate" at
            // http://alienryderflex.com/hsp.html.  However, the test images for HSP 241 and HSP 299 show differences are subtle.
            // Glenn says: I'm guessing another way of thinking about the squaring is that it is applying a gamma-expansion of 2.0, in the context of
            // a luma calculation [SMPTE Rec 709 version] of Y' = 0.2126 * R' + 0.7152 * G' + 0.0722 B', where R', G', B' are gamma-expanded, e.g., R' = R^(gamma factor)
            // For display on monitors, gamma factors of 1.8 or 2.2 are common.  [Hmm, am I confusing gamma-compression and gamma-expansion in the above equation?  aaargg]
            // See en.wikipeida.org/wiki/Luma_(video) and .../Gamma_correction for more
            // Note: if we need more performance, can drop the square root, have the caller use a cutoff squared
            return (int)Math.Sqrt(
                c.R * c.R * .241 +
                c.G * c.G * .691 +
                c.B * c.B * .068);
        }
 */

}
