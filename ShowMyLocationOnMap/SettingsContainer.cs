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
            FirstRunTime,
            LiveConnectToken,
            AuthType,
            LocalPassword,
            EnableLocation,
            DisableApplicationIdleDetection,
            DisableUserIdleDetection
        }

        /// <summary>
        // set the default settings
        // the IsolatedStorageProperty will set properties if they do not exist, otherwise will get
        /// </summary>

        // Keeps the first login time
        public static readonly IsolatedStorageProperty<DateTime> FirstRunTime =
            new IsolatedStorageProperty<DateTime>(SETTINGS_OPTIONS.FirstRunTime.ToString(), DateTime.Now);
        public static readonly IsolatedStorageProperty<bool> EnableLocation =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.EnableLocation.ToString(), false);
        public static readonly IsolatedStorageProperty<bool> DisableApplicationIdleDetection =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.DisableApplicationIdleDetection.ToString(), false);
        public static readonly IsolatedStorageProperty<bool> DisableUserIdleDetection =
            new IsolatedStorageProperty<bool>(SETTINGS_OPTIONS.DisableUserIdleDetection.ToString(), false);
        public static readonly IsolatedStorageProperty<string> LocalPassword =
            new IsolatedStorageProperty<string>(SETTINGS_OPTIONS.LocalPassword.ToString(), String.Empty);
        public static readonly IsolatedStorageProperty<string> LiveConnectToken =
            new IsolatedStorageProperty<string>(SETTINGS_OPTIONS.LiveConnectToken.ToString(), String.Empty);
        public static readonly IsolatedStorageProperty<MobileServiceAuthenticationProvider> AuthType =
            new IsolatedStorageProperty<MobileServiceAuthenticationProvider>(SETTINGS_OPTIONS.AuthType.ToString(), MobileServiceAuthenticationProvider.MicrosoftAccount);
    }
}
