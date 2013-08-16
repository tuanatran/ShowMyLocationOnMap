﻿using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.Live.Controls;
using System.Text;
using Microsoft.Phone.Shell;

namespace ShowMyLocationOnMap
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private MobileServiceAuthenticationProvider authType = SettingsContainer.AuthType.Value;
        private LiveAuthClient _authClient = new LiveAuthClient(App.APP_AUTHKEY_LIVECONNECT);
        private LiveConnectClient _client;
        private LiveConnectSession _session;
        private bool retry = false;

        public SettingsPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            CheckBox_LocationService.IsChecked = SettingsContainer.LocationConsent.Value;
            CheckBox_DisableUserIdle.IsChecked = SettingsContainer.DisableUserIdleDetection.Value;
            CheckBox_DisableAppIdle.IsChecked = SettingsContainer.DisableApplicationIdleDetection.Value;
            if (CheckBox_DisableAppIdle.IsChecked.Value)
            {
                CheckBox_DisableAppIdle.IsEnabled = false;
            }
            SetLocationConsent();
            // only force set this when user first signs into the app
            // it is optional so leave user setting until user signs out
            if (String.IsNullOrEmpty(SettingsContainer.LiveConnectToken.Value))
            {
                SetUserIdleDetection();
            }
        }

        private void Refresh()
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml?" + DateTime.Now.Ticks, UriKind.Relative));
        }

        private void SetApplicationIdleDetection()
        {
            StringBuilder msgText = new StringBuilder();
            msgText.Append("It is recommended that you disable Application Idle detection. If this is not done, the application ");
            msgText.Append("will not run when the lock screen is engaged. When this is disabled, it will remain disabled until ");
            msgText.Append("your current session ends. Would you like to diable application idle detection now?");
            MessageBoxResult result = MessageBox.Show(msgText.ToString(), "ApplicationIdleDetectionMode", MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
                SettingsContainer.DisableApplicationIdleDetection.Value = true;
                CheckBox_DisableAppIdle.IsChecked = true;
                CheckBox_DisableAppIdle.IsEnabled = false;
            }
        }

        private void SetUserIdleDetection()
        {
            if (!SettingsContainer.DisableUserIdleDetection.Value)
            {
                StringBuilder msgText = new StringBuilder();
                msgText.Append("This app is intended to be run while being charged. Disabling application ");
                msgText.Append("idle detection will ensure that the screen does not turn off while navigating. ");
                msgText.Append("Detection will be disabled only while this app is running, but you can manually enable ");
                msgText.Append("it from the Settings page.");
                MessageBox.Show(msgText.ToString());
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                SettingsContainer.DisableUserIdleDetection.Value = true;
                CheckBox_DisableUserIdle.IsChecked = true;
                Refresh();
            }
            else
            {
                SettingsContainer.DisableUserIdleDetection.Value = false;
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                CheckBox_DisableUserIdle.IsChecked = false;
            }
        }

        private void SetLocationConsent()
        {
            // User must give location service consent for app to function
            while (!SettingsContainer.LocationConsent.Value)
            {
                // User has not opted in for Location
                GetLocationConsent(retry);
            }
        }

        private void GetLocationConsent(bool retry)
        {
            MessageBoxResult result = MessageBox.Show("This app accesses your phone's location. Is that ok?", 
                "Location", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                SettingsContainer.LocationConsent.Value = true;                
            }
            else
            {
                if (!retry)
                {
                    result = MessageBox.
                        Show("This app will not function without your location. Press 'ok' to provide consent or 'cancel' to close.", 
                        "Location", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        SettingsContainer.LocationConsent.Value = true;
                        Refresh();
                    }
                    else
                    {
                        MessageBox.Show("This app will now close.");
                        Application.Current.Terminate();
                    }
                }
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            bool loginTask = await AttemptLogin(authType);
            if (!loginTask)
            {
                MessageBox.Show("Login failed. ");
            }
        }

        private void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                _session = e.Session;
                _client = new LiveConnectClient(_session);
                infoTextBlock.Text = "Signed in.";
            }
            else
            {
                infoTextBlock.Text = "Not signed in.";
                _client = null;
            }
        }


        public async Task<bool> AttemptLogin(MobileServiceAuthenticationProvider authType)
        {
            try
            {
                if (authType.GetType() == typeof(MobileServiceAuthenticationProvider))
                {
                    if (String.IsNullOrEmpty(SettingsContainer.LiveConnectToken.Value) && App.CurrentUser == null)
                    {
                        App.CurrentUser = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                    }
                    try
                    {
                        LiveLoginResult initializeResult = await _authClient.InitializeAsync();
                        if (initializeResult.Status == LiveConnectSessionStatus.Connected)
                        {
                            _session = _authClient.Session;
                        }
                        else
                        {
                            LiveLoginResult loginResult = await _authClient.LoginAsync(
                                new[] { "wl.signin", "wl.basic", "wl.offline_access" });
                            if (loginResult.Status == LiveConnectSessionStatus.Connected)
                            {
                                _session = _authClient.Session;
                            }
                        }
                        try
                        {
                            _client = new LiveConnectClient(_session);
                            LiveOperationResult operationResult = await _client.GetAsync("me");
                            dynamic result = operationResult.Result;
                            if (result != null)
                            {
                                this.infoTextBlock.Text = string.Join(" ", "Hello", result.name, "!");
                            }
                            else
                            {
                                this.infoTextBlock.Text = "Error getting name.";
                            }
                            App.CurrentUser = await App.MobileService.LoginWithMicrosoftAccountAsync(_session.AuthenticationToken);
                            SettingsContainer.LiveConnectToken.Value = _session.AuthenticationToken;
                            SettingsContainer.SessionExpires.Value = DateTime.UtcNow.Add(_session.Expires.Offset);
                        }
                        catch (LiveAuthException exception)
                        {
                            this.infoTextBlock.Text = "Error signing in: " + exception.Message;
                        }
                        catch (LiveConnectException exception)
                        {
                            this.infoTextBlock.Text = "Error calling API: " + exception.Message;
                        }
                    }
                    catch (LiveAuthException exception)
                    {
                        this.infoTextBlock.Text = "Error initializing: " + exception.Message;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                SettingsContainer.LiveConnectToken.Value = String.Empty;
                StringBuilder message = new StringBuilder("An exception of type ");
                message.Append(ex.GetType()).Append(" from source ").Append(ex.Source).Append(" occurred: ").Append(ex.Message);
                MessageBox.Show(message.ToString());
                return false;
            }
        }

        private void CheckBox_LocationService_Unchecked(object sender, RoutedEventArgs e)
        {
            SettingsContainer.LocationConsent.Value = false;
            retry = false;
            SetLocationConsent();
        }

        private void CheckBox_LocationService_Checked(object sender, RoutedEventArgs e)
        {
            SettingsContainer.LocationConsent.Value = true;
            Refresh();
        }

        private void CheckBox_DisableAppIdle_Checked(object sender, RoutedEventArgs e)
        {
            SetApplicationIdleDetection();
        }

        private void CheckBox_DisableUserIdle_Checked(object sender, RoutedEventArgs e)
        {
            SetUserIdleDetection();
        }

        private void CheckBox_DisableUserIdle_Unchecked(object sender, RoutedEventArgs e)
        {
            SetUserIdleDetection();
        }
    }
}