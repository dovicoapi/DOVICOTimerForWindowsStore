using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Controls;


namespace DOVICOTimerForWindowsStore
{
    /// <summary>
    /// Our app's Privacy Policy (displayed by the Windows Settings charm)
    /// </summary>
    public sealed partial class PrivacyPolicyContents : UserControl
    {
        // Default constructor
        public PrivacyPolicyContents()
        {
            this.InitializeComponent();

            // Set the text for the 'What information do we collect?' and 'What do we use your information for?' sections
            ResourceLoader rlResource = new ResourceLoader();
            txtWhatInfoDoWeCollectLabel.Text = rlResource.GetString("PrivacyPolicyWhatInfoDoWeCollectLabel");
            txtWhatInfoDoWeCollectLine1.Text = rlResource.GetString("PrivacyPolicyWhatInfoDoWeCollectLine1");
            txtWhatInfoDoWeCollectLine2.Text = rlResource.GetString("PrivacyPolicyWhatInfoDoWeCollectLine2");
            txtWhatDoWeDoWithYourInfoLabel.Text = rlResource.GetString("PrivacyPolicyWhatDoWeDoWithYourInfo");
            txtWhatDoWeDoWithYourInfoLine1.Text = rlResource.GetString("PrivacyPolicyWhatDoWeDoWithYourInfoLine1");
            txtWhatDoWeDoWithYourInfoLine2.Text = rlResource.GetString("PrivacyPolicyWhatDoWeDoWithYourInfoLine2");
        }
    }
}
