using System;
using System.ServiceModel;
using Windows.Storage;

namespace DOVICOTimerForWindowsStore
{
    public class CSettings
    {
        // The default URI if the URI hasn't been set yet or if a bad URI was entered.
        protected const string AUTHORITY = "apps.dovico.net";
        protected const string DEFAULT_URI = ("http://" + AUTHORITY + "/timer/welcome.aspx");


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
            // We don't want someone going in and changing the URI to something we don't control (e.g. google or whatever), getting into trouble and then blaming us. If the URI
            // specified is not the 'apps.dovico.net' DOMAIN (can't just do an indexof type thing because it might be a trick by placing that in a query string or something)
            // then default to the welcome page instead.
            //
            // By default we assume a bad URI was passed in
            bool bValidURI = false;

            // Convert the URI string received into a Uri object. If successful then...
            Uri uUriResult;
            if (Uri.TryCreate(sURI, UriKind.Absolute, out uUriResult)) 
            {
                // We now know that the URI is in a valid format but is it pointing to apps.dovico.net? If it is then...
                if (uUriResult.Authority == AUTHORITY) 
                {
                    // Coming from the right domain is important but we also need to make sure that only the Timer service is being used in this extension (I don't want this
                    // extension used by any other service that might be hosted on apps.dovico.net which might end up being 3rd party apps at some point!)
                    //
                    // I would prefer using UriTemplate but it doesn't appear to be in the current version of the Windows Store version of .net so I'm forced to use strings. If
                    // the URI provided matches 'http://apps.dovico.net/timer' (test first without the forward slash) then....
                    string sLowerCaseExpectedUri = (uUriResult.Scheme + "://" + AUTHORITY + "/timer").ToLowerInvariant();
                    if (sURI.ToLowerInvariant() == sLowerCaseExpectedUri) 
                    { 
                        bValidURI = true; 
                    }
                    else // The URI didn't match our expected URI without the froward slash (might be something totally different or it might be our expected URI with a forward slash and additional data)...
                    {
                        // Tag on a forward slash ('http://apps.dovico.net/timer/') to our expected URI. If the provided URI is at least as long as our expected URI and the
                        // left portion of the provided URI matches the one we're looking for then we have a match.
                        sLowerCaseExpectedUri += "/";
                        int iExpectedLen = sLowerCaseExpectedUri.Length;
                        if ((sURI.Length >= iExpectedLen) && (sURI.Substring(0, iExpectedLen).ToLowerInvariant() == sLowerCaseExpectedUri))
                        {
                            bValidURI = true;
                        } // End if
                    } // End if (sURI.ToLowerInvariant() == sLowerCaseExpectedUri)
                } // End if (uUriResult.Authority == AUTHORITY)
            } // End if (Uri.TryCreate(sURI, UriKind.Absolute, out uUriResult))
            
            
            // Set the new URI value and then cause the DataChanged event to be sent to all registered handlers (use the URI 
            // set if its valid. Otherwise, use the default URI)
            ApplicationData.Current.RoamingSettings.Values["URI"] = (bValidURI ? sURI : DEFAULT_URI);
            ApplicationData.Current.SignalDataChanged();
        }
    }
}
