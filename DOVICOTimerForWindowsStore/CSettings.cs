using System;
using Windows.Storage;

namespace DOVICOTimerForWindowsStore
{
    public class CSettings
    {
        // The default URI if the URI hasn't been set yet or if a bad URI was entered.
        protected static string DEFAULT_URI = "http://apps.dovico.net/timer/welcome.aspx";


        // Helper to return the current URI that is set
        public static string GetUri()
        {
            // Grab the URI from the roaming settings (http://blogs.msdn.com/b/windowsappdev/archive/2012/07/17/roaming-your-app-data.aspx)
            // If the value does not yet exist then set the URI to point to the DOVICO Timer welcome page
            string sURI = ApplicationData.Current.RoamingSettings.Values["URI"] as string;
            if (string.IsNullOrWhiteSpace(sURI)) { sURI = DEFAULT_URI; }

            // Return the URI to the caller
            return sURI;
        }


        // Helper to save the URI setting
        public static void SetUri(string sURI)
        {
            // We don't want someone going in and changing the URI to something we don't control (e.g. google or whatever), getting 
            // into trouble and then blaming us. If the URI specified is not the 'apps.dovico.net' DOMAIN (can't just do an indexof
            // type thing because it might be a trick by placing that in a query string or something) then default to the
            // welcome page instead.
            //
            // By default we assume a bad URI was passed in
            bool bValidURI = false;

            // Convert the URI string received into a Uri object. If successful then...
            Uri uUriResult;
            if (Uri.TryCreate(sURI, UriKind.Absolute, out uUriResult)) { 
                // We now know that the URI is in a valid format but is is pointing to apps.dovico.net? If it is then the URI is
                // OK
                if (uUriResult.Authority == "apps.dovico.net") { bValidURI = true; }
            } // End if (Uri.TryCreate(sURI, UriKind.Absolute, out uUriResult))
            
            
            // Set the new URI value and then cause the DataChanged event to be sent to all registered handlers (use the URI 
            // set if its valid. Otherwise, use the default URI)
            ApplicationData.Current.RoamingSettings.Values["URI"] = (bValidURI ? sURI : DEFAULT_URI);
            ApplicationData.Current.SignalDataChanged();
        }
    }
}
