using Callisto.Controls;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace DOVICOTimerForWindowsStore
{
    /// <summary>
    /// Our main view that displays DOVICO Timer within a WebView control
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();

            // The following gives some info on the ScriptNotify event handler: http://msdn.microsoft.com/en-us/library/windows/apps/windows.ui.xaml.controls.webview.scriptnotify
            // In the Remarks section, it talks about Navigate (requires URI opt-in) and NavigateToString (does not require URI
            // opt-in) but it fails to mention the Source property that we use to set the WebView's content location. In testing,
            // it appears that the Source property behaves similar to the Navigate method because the ScriptNotify event does not
            // trigger unless the URI opt-in list is set.
            //
            // Be careful with this because we are using RoamingSettings which means a URI that is set will be propagated to all
            // devices that the user has this app installed on. If you allow just any site to call the event handler then it is
            // possible that a malicious site could set a URI that could potentially exploit vulnerabilities on their other devices.
            // 
            // Only allow the following URI to call our ScriptNotify event handler
            List<Uri> lstAllowedUris = new List<Uri>();
#if DEBUG
            // For development only. I change the following URI to a local Intranet address during development if needed. By using
            // conditional compilation symbols and only modifying the contents of the debug section, I avoid the possibility of 
            // forgetting to put things back before pushing out a release.
            lstAllowedUris.Add(new Uri("http://apps.dovico.net"));
#else // We're doing a Release build
            lstAllowedUris.Add(new Uri("http://apps.dovico.net"));
#endif // DEBUG
            wvBrowser.AllowedScriptNotifyUris = lstAllowedUris;

            // Attach our ScriptNotify event handler so that we can receive information from the JavaScript
            wvBrowser.ScriptNotify += wvBrowser_ScriptNotify;

            // Cause the web browser to load in its content
            UpdateURI();


            // Hook up a DataChanged event so that we're informed when the RoamingSettings are changed
            ApplicationData.Current.DataChanged += new TypedEventHandler<ApplicationData, object>(Current_DataChanged);
        }


        /// <summary>
        /// Event triggered when the JavaScript, of a page that we've allowed as part of the AllowedScriptNotifyUris list, calls
        /// window.event.notify and passes in a string of text. The string of text that we're looking for is a URI that the user
        /// will use from here on out to track his/her time (we need to switch the setting from the welcome page to the timer
        /// page)
        /// </summary>
        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // Pass the URI to our settings class to have the URI saved (doing so calls the SignalDataChanged method which causes
            // the Current_DataChanged event handler to be triggered which will cause the WebView to update with the latest URI)
            CSettings.SetUri(e.Value);
        }


        /// <summary>
        /// Event triggered when an ApplicationData's RoamingSettings are changed (gets changed in the SettingsOptionsContext page
        /// which is part of the Settings flyout)
        /// </summary>
        async void Current_DataChanged(ApplicationData sender, object args) 
        { 
             // DataChangeHandler may be invoked on a background thread, so use the Dispatcher to invoke the UI-related code on the
            // UI thread. 
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Cause the web browser to reload in its content
                UpdateURI();
            });
        }


        /// <summary>
        /// Helper that gives the WebView control the proper URI to display
        /// </summary>
        public void UpdateURI()
        {
            // Grab our saved URI and check to see if there is a query string on it (the '?' character)
            string sURI = CSettings.GetUri();
            bool bHasQueryString = sURI.Contains("?");

            // Add on a query string that will tell DOVICO Timer that it is being used in the Windows Store wrapper (Windows 8 - a 
            // way for us to get feedback to find out what tools are being used and how often...helps determine which tools should
            // get our focus)
            sURI += ((bHasQueryString ? "&" : "?") + "prod=WindowsStoreApp");

            // Try to create a URI from the string. If we have a valid URI then...
            Uri uUriResult;
            if (Uri.TryCreate(sURI, UriKind.Absolute, out uUriResult)) { wvBrowser.Source = uUriResult; }
        }


        /// <summary>
        /// Invoked when this page is about to be displayed
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            SettingsPane.GetForCurrentView().CommandsRequested -= MainPage_CommandsRequested;
            base.OnNavigatingFrom(e);
        }


        /// <summary>
        /// Called by the Settings charm to find out what Settings links to display and the code to call when clicked.
        /// </summary>
        private void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            // Set up the Options link on the Settings charm 
            ResourceLoader rlResource = new ResourceLoader();
            SettingsCommand cmdSettingsOptions = new SettingsCommand("cmdSettingsOptionsLabel", rlResource.GetString("SettingsOptionsLabel"), (x) =>
            {
                // Show the settings flyout (pass in the text for the title, as well as the UserControl object for the flyout's
                // contents)
                ShowFlyout(rlResource.GetString("SettingsOptionsContentTitle"), new SettingsOptionsContent());
            });

            // Set up the Privacy Statement link on the Settings charm 
            string sPrivacySettingsText = rlResource.GetString("SettingsPrivacyPolicyLabel");
            SettingsCommand cmdSettingsPrivacyStatement = new SettingsCommand("cmdSettingsPrivacyStatementLabel", sPrivacySettingsText, (x) =>
            {
                // Show the settings flyout (pass in the text for the title, as well as the UserControl object for the flyout's
                // contents)
                ShowFlyout(rlResource.GetString("SettingsPrivacyPolicyTitle"), new PrivacyPolicyContents());
            });


            // Add our Options and Privacy Statement links to the Settings charm
            args.Request.ApplicationCommands.Add(cmdSettingsOptions);
            args.Request.ApplicationCommands.Add(cmdSettingsPrivacyStatement);
        }


        /// <summary>
        /// Creates the WebViewBrush and fills our rectangle control with it
        /// </summary>
        private void CreateAndSetWebViewBrush()
        {
            // Get a brush from the WebView's content (a screen shot basically)
            WebViewBrush wvbBrush = new WebViewBrush();
            wvbBrush.SourceName = "wvBrowser";
            wvbBrush.Redraw();

            // Fill the Rectangle object with the brush
            rectWebViewBrush.Fill = wvbBrush;
        }


        /// <summary>
        /// Creates and displays the Settings flyout
        /// </summary>
        private async void ShowFlyout(string sTitle, object objContent)
        {
            // Create the WebViewBrush and fill our rectangle control with it
            CreateAndSetWebViewBrush();

            // Cause this function to wait 100 milliseconds before proceeding to get around the flicker caused by the Rectangle's
            // Fill property not being instant (give it a bit of time to redraw)
            await Task.Delay(100);


            // Create the Settings flyout (using the open source Calliso project: https://nuget.org/packages/Callisto)
            SettingsFlyout settings = new SettingsFlyout();
            settings.FlyoutWidth = Callisto.Controls.SettingsFlyout.SettingsFlyoutWidth.Wide;

            // Set the Header's brush to be the same dark blue as we use in DOVICO Timer
            settings.HeaderBrush = new SolidColorBrush(Color.FromArgb(Convert.ToByte("ff", 16), Convert.ToByte("06", 16), Convert.ToByte("6d", 16), Convert.ToByte("ab", 16)));
            
            // NOTE:    I would prefer a white background for the body of the flyout but when I set the settings.Background to a
            //          white brush, the URI label doesn't show up and it's not obvious that the URI is in a text box (no border)

                        
            // Set the text in the header of the flyout to the title provided
            settings.HeaderText = sTitle;

            // Set the small logo of our app in the header section of the settings flyout
            BitmapImage bmp = new BitmapImage(new Uri("ms-appx:///Assets/TransparentSmallLogo.png"));
            settings.SmallLogoImageSource = bmp;

            // Intercept the setting's closed event so that we can switch back to the WebView control
            settings.Closed += settings_Closed;

            // Add our content for the flyout, switch from the WebView control to the Rectangle with a WebViewBrush, and then show
            // the flyout.
            settings.Content = objContent;
            SwitchToWebViewScreenShot(true);
            settings.IsOpen = true;  
        }


        /// <summary>
        /// Helper to toggle between the WebView and a WebViewBrusht (needed because the WebView displays in front of everything 
        /// including flyouts preventing the flyouts from being seen which is a huge problem! Brand new Windows 8 and we already
        /// have IE6-like hacks!)
        /// </summary>
        private void SwitchToWebViewScreenShot(bool bSwitchToScreenShot)
        {
            // If we're to show the screen shot then...
            if (bSwitchToScreenShot)
            {
                // Hide the WebView control (MainPage_CommandsRequested has already set the rectangle's fill with a screen shot of
                // the contents of the WebView control) 
                wvBrowser.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else // We're to show the web view again...
            {
                // Show the WebView control and remove the WebViewBrush from the Rectangle
                wvBrowser.Visibility = Windows.UI.Xaml.Visibility.Visible;
                rectWebViewBrush.Fill = new SolidColorBrush(Colors.Transparent);
            } // End if (bSwitchToScreenShot)
        }


        /// <summary>
        /// Event handler when the settings flyout is closed
        /// 
        /// Switch back to the WebView control rather than the screen shot
        /// </summary>
        void settings_Closed(object sender, object e) { SwitchToWebViewScreenShot(false); }
    }
}
