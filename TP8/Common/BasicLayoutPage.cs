using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace TP8.Common
{

[Windows.Foundation.Metadata.WebHostHidden]
public class BasicLayoutPage : Page
{
    // BasicLayoutPage provides centralization of navigation and visual manager functions,
    // like win8.0 LayoutAwarePage
    // Adapted from Marco Minerva's blog (where it was called just "BasicPage"):
    //   http://marcominerva.wordpress.com/2013/10/10/a-base-page-class-for-windows-8-1-store-apps-with-c-and-xaml/
    //   http://marcominerva.wordpress.com/2013/10/16/handling-visualstate-in-windows-8-1-store-apps/
    private NavigationHelper navigationHelper;
    private ObservableDictionary defaultViewModel = new ObservableDictionary();

    public ObservableDictionary DefaultViewModel
    {
        get { return this.defaultViewModel; }
    }

    public NavigationHelper NavigationHelper
    {
        get { return this.navigationHelper; }
    }

    private List<Control> _layoutAwareControls; // Glenn copies from LayoutAwarePage

    public BasicLayoutPage()
    {
        this.navigationHelper = new NavigationHelper(this);
        this.navigationHelper.LoadState += navigationHelper_LoadState;
        this.navigationHelper.SaveState += navigationHelper_SaveState;
        this.Loaded += page_Loaded; // for visual state
        this.Unloaded += page_Unloaded; // for visual state
    }

    private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
    {
        LoadState(e);
    }

    private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
    {
        SaveState(e);
    }

    protected virtual void LoadState(LoadStateEventArgs e) { }
    protected virtual void SaveState(SaveStateEventArgs e) { }

    #region NavigationHelper registration

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        navigationHelper.OnNavigatedTo(e);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        navigationHelper.OnNavigatedFrom(e);
    }

    #endregion
    #region VisualState

    private void page_Loaded(object sender, RoutedEventArgs e)
    {
        StartLayoutUpdates(sender, e);
        Window.Current.SizeChanged += Window_SizeChanged;
        DetermineVisualState();
    }

    private void page_Unloaded(object sender, RoutedEventArgs e)
    {
        StopLayoutUpdates(sender, e);
        Window.Current.SizeChanged -= Window_SizeChanged;
    }

#if WAS_IN_LAYOUT_AWARE_PAGE
    /// <summary>
    /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded"/>
    /// event of a <see cref="Control"/> within the page, to indicate that the sender should
    /// start receiving visual state management changes that correspond to application view
    /// state changes.
    /// </summary>
    /// <param name="sender">Instance of <see cref="Control"/> that supports visual state
    /// management corresponding to view states.</param>
    /// <param name="e">Event data that describes how the request was made.</param>
    /// <remarks>The current view state will immediately be used to set the corresponding
    /// visual state when layout updates are requested.  A corresponding
    /// <see cref="FrameworkElement.Unloaded"/> event handler connected to
    /// <see cref="StopLayoutUpdates"/> is strongly encouraged.  Instances of
    /// <see cref="LayoutAwarePage"/> automatically invoke these handlers in their Loaded and
    /// Unloaded events.</remarks>
    /// <seealso cref="DetermineVisualState"/>
    /// <seealso cref="InvalidateVisualState"/>
    public void StartLayoutUpdates(object sender, RoutedEventArgs e)
    {
        var control = sender as Control;
        if (control == null) return;
        if (this._layoutAwareControls == null)
        {
            // Start listening to view state changes when there are controls interested in updates
            Window.Current.SizeChanged += this.WindowSizeChanged;
            this._layoutAwareControls = new List<Control>();
        }
        this._layoutAwareControls.Add(control);

        // Set the initial visual state of the control
        VisualStateManager.GoToState(control, DetermineVisualState(ApplicationView.Value), false);
    }
#endif

    /// <summary>
    /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Loaded"/>
    /// event of a <see cref="Control"/> within the page, to indicate that the sender should
    /// start receiving visual state management changes that correspond to application view
    /// state changes.
    /// </summary>
    /// <param name="sender">Instance of <see cref="Control"/> that supports visual state
    /// management corresponding to view states.</param>
    /// <param name="e">Event data that describes how the request was made.</param>
    /// <remarks>The current view state will immediately be used to set the corresponding
    /// visual state when layout updates are requested.  A corresponding
    /// <see cref="FrameworkElement.Unloaded"/> event handler connected to
    /// <see cref="StopLayoutUpdates"/> is strongly encouraged.  Instances of
    /// <see cref="LayoutAwarePage"/> automatically invoke these handlers in their Loaded and
    /// Unloaded events.</remarks>
    /// <seealso cref="DetermineVisualState"/>
    /// <seealso cref="InvalidateVisualState"/>
    public void StartLayoutUpdates(object sender, RoutedEventArgs e)
    {
        var control = sender as Control;
        if (control == null) return;
        if (this._layoutAwareControls == null)
        {
            // Start listening to view state changes when there are controls interested in updates
            // Window.Current.SizeChanged += this.Window_SizeChanged;
            this._layoutAwareControls = new List<Control>();
        }
        this._layoutAwareControls.Add(control);

        // Set the initial visual state of the control
        VisualStateManager.GoToState(control, DetermineMappedVisualState(), false);
    }

    private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs e)
    {
        DetermineVisualState(); // check for physical state
        InvalidateVisualState(); // check for mapped state
    }

#if WAS_IN_LAYOUT_AWARE_PAGE
    /// <summary>
    /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded"/>
    /// event of a <see cref="Control"/>, to indicate that the sender should start receiving
    /// visual state management changes that correspond to application view state changes.
    /// </summary>
    /// <param name="sender">Instance of <see cref="Control"/> that supports visual state
    /// management corresponding to view states.</param>
    /// <param name="e">Event data that describes how the request was made.</param>
    /// <remarks>The current view state will immediately be used to set the corresponding
    /// visual state when layout updates are requested.</remarks>
    /// <seealso cref="StartLayoutUpdates"/>
    public void StopLayoutUpdates(object sender, RoutedEventArgs e)
    {
        var control = sender as Control;
        if (control == null || this._layoutAwareControls == null) return;
        this._layoutAwareControls.Remove(control);
        if (this._layoutAwareControls.Count == 0)
        {
            // Stop listening to view state changes when no controls are interested in updates
            this._layoutAwareControls = null;
            Window.Current.SizeChanged -= this.WindowSizeChanged;
        }
    }
#endif

    /// <summary>
    /// Invoked as an event handler, typically on the <see cref="FrameworkElement.Unloaded"/>
    /// event of a <see cref="Control"/>, to indicate that the sender should start receiving
    /// visual state management changes that correspond to application view state changes.
    /// </summary>
    /// <param name="sender">Instance of <see cref="Control"/> that supports visual state
    /// management corresponding to view states.</param>
    /// <param name="e">Event data that describes how the request was made.</param>
    /// <remarks>The current view state will immediately be used to set the corresponding
    /// visual state when layout updates are requested.</remarks>
    /// <seealso cref="StartLayoutUpdates"/>
    public void StopLayoutUpdates(object sender, RoutedEventArgs e)
    {
        var control = sender as Control;
        if (control == null || this._layoutAwareControls == null)
            return;
        this._layoutAwareControls.Remove(control);
        if (this._layoutAwareControls.Count == 0)
        {
            // Stop listening to view state changes when no controls are interested in updates
            this._layoutAwareControls = null;
            //Window.Current.SizeChanged -= this.Window_SizeChanged;
        }
    }


    public void DetermineVisualState()
    {
        var state = string.Empty;
        var applicationView = ApplicationView.GetForCurrentView();
        var appSize = Window.Current.Bounds; // current size of application window, presumably in scaled pixels

        if (applicationView.IsFullScreen)
        {
            if (applicationView.Orientation == ApplicationViewOrientation.Landscape)
                state = "FullScreenLandscape";
            else
                state = "FullScreenPortrait";
        }
        else
        {

            if (appSize.Width == 320)
                state = "vs320Wide";// was before June 2015: "Snapped";
            else if (appSize.Width <= 500)
                state = "vs321To500Wide"; // was: "Narrow";
            // Simplified decision making follows, as opposed to "MAYBE_TOO_MUCH" that uses C++ dll "MetroAssist"
            else if (appSize.Width <= 672) // 672 is screen split in half on 1366 wide (x 768) display.  Common widths: 1024, 1366, 1920, 2560.  Corresponding Half widths: 512, 672, 960, 1280
                state = "vs501To672Wide"; // was: "Half";
            else if (appSize.Width <= 1025)  // 1024.5 is 3/4 on 1366 wide display. Common 3/4 widths: 768, 1024.5, 1440, 1920 
                state = "vs673To1025Wide"; // was: "Filled";
            else if (appSize.Width <= 1365) // for benefit of split screen
                state = "vs1026To1365Wide"; // was: "Wide"; // added June 2015
            else
                state = "vsOver1365Wide"; // added June 2015
            // Silverlight would have used LayoutRoot.ActualWidth; // Will be zero until first call of LayoutUpdated

#if MAYBE_TOO_MUCH
            else // instead of simplified setting of Half and Filled above
            {
                //Deprecated: DisplayProperties.LogicalDpi // Logical pixel width x (logical dpi / 96) gives device pixel width
                // Need to get physical resolution of device, which is made surprisely hard to get
                // System.Windows.SystemParameters. // I guess not allowed in Store app
                // Reference MetroHelpers.dll, project included in TP8 solution
                // Has to be included in project... see http://stackoverflow.com/questions/13500391/how-to-add-a-lib-file-reference-to-visual-studio-2012
                // Also, when adding this Reference in VS, be sure to select "Solution" in left menu; more general browsing and selecting dll will complain and fail. 
                // This is a win 8.1 conversion of a 3rd party win 8 app that uses directx to get device resolution.
                var assist = new MetroHelpers.MetroAssist();
                var res = assist.ScreenResolution;
                // I don't know if MetroAssist call is orientation-aware.  So, make sure its always as if landscape orientation
                if (res.X < res.Y)
                {
                    // interchange
                    var X = res.X;
                    res.X = res.Y;
                    res.Y = X;
                }
                //txtScreenRes.Text = string.Format("Your screen resolution when in landscape mode is {0}x{1}.", res.X, res.Y);

                // So we have the physical resolution of the full screen.  Now let's calculate the physical resolution of the app,
                // using variant (adapted to 8.1) of http://programmerpayback.com/2013/08/31/detecting-screen-resolution-in-windows-8-and-windows-phone-apps/
                // August 31, 2013 by Tim Greenfield 

                // Initialize if no scaling
                double w = appSize.Width;
                double h = appSize.Height;

                // DisplayProperties is deprecated in 8.1
                // switch (DisplayProperties.ResolutionScale)

                DisplayInformation di = DisplayInformation.GetForCurrentView();
                // I'm going to guess that ResolutionScale ratio is the same as the ratio of RawDpiY / LogicalDpi
                //float pli = di.LogicalDpi; // pixels per logical inch of current environment
                //float rawDpiY = di.RawDpiY; // gets the raw dots per inch along the display monitor
                switch (DisplayInformation.GetForCurrentView().ResolutionScale)
                {
                    case ResolutionScale.Scale140Percent:

                        w = Math.Ceiling(appSize.Width * 1.4);
                        h = Math.Ceiling(appSize.Height * 1.4); // don't really care about y, but for completeness
                        break;

                    case ResolutionScale.Scale180Percent:

                        w = Math.Ceiling(appSize.Width * 1.8);
                        h = Math.Ceiling(appSize.Height * 1.8); // don't really care about y, but for completeness
                        break;
                }

                Size appSizePhysical = new Size(w,h); // could use Windows.Foundation.Rect as well
                double percentwidth = appSizePhysical.Width * 100.0 / res.X;
                App.MyAssert(percentwidth <= 100.0);
                if (percentwidth > 60.0) // cut some slack around half
                    state = "Filled";
                else
                    state = "Half"; // lower side of range will be bounded by "Narrow"
            }
#endif
        }

        System.Diagnostics.Debug.WriteLine("Width: {0}, New VisualState: {1}", 
            appSize.Width, state);

        App.CurrentVisualState = state; // Glenn adds

        VisualStateManager.GoToState(this, state, true);
    }

    // Next 2 functions copied from Win8.0 LayoutAwarePage.cs:
#if WAS_IN_LAYOUT_AWARE_PAGES
    /// <summary>
    /// Translates <see cref="ApplicationViewState"/> values into strings for visual state
    /// management within the page.  The default implementation uses the names of enum values.
    /// Subclasses may override this method to control the mapping scheme used.
    /// </summary>
    /// <param name="viewState">View state for which a visual state is desired.</param>
    /// <returns>Visual state name used to drive the
    /// <see cref="VisualStateManager"/></returns>
    /// <seealso cref="InvalidateVisualState"/>
    protected virtual string DetermineVisualState(ApplicationViewState viewState)
    {
        return viewState.ToString();
    }
#endif

    /// <summary>
    /// Returns the visual state within the page.  The default implementation returns the global state.
    /// Subclasses may override this method to control the mapping scheme used.
    /// </summary>
    /// <returns>Visual state name used to drive the
    /// <see cref="VisualStateManager"/></returns>
    /// <seealso cref="InvalidateVisualState"/>
    protected virtual string DetermineMappedVisualState()
    {
        if (String.IsNullOrEmpty(App.CurrentVisualState))
            DetermineVisualState();
        App.MyAssert(!String.IsNullOrEmpty(App.CurrentVisualState));
        return App.CurrentVisualState;
    }
#if WAS_IN_LAYOUT_AWARE_PAGES
    /// <summary>
    /// Updates all controls that are listening for visual state changes with the correct
    /// visual state.
    /// </summary>
    /// <remarks>
    /// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to
    /// signal that a different value may be returned even though the view state has not
    /// changed.
    /// </remarks>
    
    public void InvalidateVisualState()
    {
        if (this._layoutAwareControls != null)
        {
            string visualState = DetermineVisualState(ApplicationView.Value);
            foreach (var layoutAwareControl in this._layoutAwareControls)
            {
                VisualStateManager.GoToState(layoutAwareControl, visualState, false);
            }
        }
    }
#endif

    /// <summary>
    /// Updates all controls that are listening for visual state changes with the correct visual state.
    /// </summary>
    /// <remarks>
    /// Typically used in conjunction with overriding <see cref="DetermineVisualState"/> to
    /// signal that a different value may be returned even though the view state has not changed.
    /// </remarks>

    public void InvalidateVisualState()
    {
        if (this._layoutAwareControls != null)
        {
            string visualState = DetermineMappedVisualState();
            foreach (var layoutAwareControl in this._layoutAwareControls)
            {
                VisualStateManager.GoToState(layoutAwareControl, visualState, false);
            }
        }
    }
    #endregion

#if FOR_REF_FROM_LAYOUT_AWARE_PAGE
        /// <summary>
        /// Invoked as an event handler to navigate backward in the navigation stack
        /// associated with this page's <see cref="Frame"/>.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the
        /// event.</param>
        protected virtual void GoBack(object sender, RoutedEventArgs e)
        {
            // Use the navigation frame to return to the previous page
            if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
        }
#endif

    protected virtual void GoBack(object sender, RoutedEventArgs e)
    {
        GoBack();
    }

    protected virtual void GoBack()
    {
        if(this.navigationHelper.CanGoBack())
            this.navigationHelper.GoBack();
    }

}

}