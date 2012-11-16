using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace DOVICOTimerForWindowsStore
{
    /// <summary>
    /// Our app's Options (displayed by the Windows Settings charm)
    /// </summary>
    public sealed partial class SettingsOptionsContent : UserControl
    {
        // Will hold the URI that was available when this dialog is first constructed so that we can compare URIs when the URI 
        // textbox loses focus and only save if it was changed.
        private string m_sOriginalURI = "";


        // Default constructor
        public SettingsOptionsContent()
        {
            this.InitializeComponent();

            // We're using a string resource for the URI label to make allowing for different languages a bit easier later
            ResourceLoader rlResource = new ResourceLoader();
            lblURILabel.Text = rlResource.GetString("SettingsOptionsContentURILabel");

            // Grab the current URI and put it into the URI textbox
            m_sOriginalURI = CSettings.GetUri();
            txtURI.Text = m_sOriginalURI;

            // Setup a LostFocus event so that we can save the URI that was entered
            txtURI.LostFocus += txtURI_LostFocus;
        }


        // Event triggered when the URI textbox looses focus
        void txtURI_LostFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Grab the current URI from the text box. If it does not match what we have from when we were first displayed (or the
            // last time the field lost focus) then...
            string sCurrentURI = txtURI.Text;
            if (sCurrentURI != m_sOriginalURI)
            {
                // Remember the new URI (just in case the lost focus is because the user clicked out of the textbox and not because
                // the view is closing) and then have the URI setting saved.
                m_sOriginalURI = sCurrentURI;
                CSettings.SetUri(m_sOriginalURI);
            } // End if (sCurrentURI != m_sOriginalURI)
        }
    }
}
