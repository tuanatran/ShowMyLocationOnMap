using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;

namespace ShowMyLocationOnMap
{
    public static class SettingsContainer
    {
        public enum SETTINGS_OPTIONS
        {
            UserName,
            SessionExpires,
            LiveConnectToken,
            AuthType,
            LocationConsent,
            DisableApplicationIdleDetection,
            DisableUserIdleDetection
        }

        /// <summary>
        // set the default settings
        // the IsolatedStorageProperty will set properties if they do not exist
        /// </summary>

        public static readonly IsolatedStorageProperty<String> UserName =
            new IsolatedStorageProperty<String>(SETTINGS_OPTIONS.UserName.ToString(), String.Empty);
        public static readonly IsolatedStorageProperty<DateTime> SessionExpires =
            new IsolatedStorageProperty<DateTime>(SETTINGS_OPTIONS.SessionExpires.ToString(), DateTime.Now);
        public static readonly IsolatedStorageProperty<bool> LocationConsent =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.LocationConsent.ToString(), false);
        public static readonly IsolatedStorageProperty<bool> DisableApplicationIdleDetection =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.DisableApplicationIdleDetection.ToString(), false);
        public static readonly IsolatedStorageProperty<bool> DisableUserIdleDetection =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.DisableUserIdleDetection.ToString(), false);
        public static readonly IsolatedStorageProperty<string> LiveConnectToken =
            new IsolatedStorageProperty<string>(SETTINGS_OPTIONS.LiveConnectToken.ToString(), String.Empty);
        public static readonly IsolatedStorageProperty<MobileServiceAuthenticationProvider> AuthType =
            new IsolatedStorageProperty<MobileServiceAuthenticationProvider>(SETTINGS_OPTIONS.AuthType.ToString(), 
                MobileServiceAuthenticationProvider.MicrosoftAccount);
    }
}
