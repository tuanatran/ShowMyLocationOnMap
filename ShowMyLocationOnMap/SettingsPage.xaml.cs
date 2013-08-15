using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.Live.Controls;
using System.Text;

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
            SetSettings();
        }

        private void Refresh()
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml?" + DateTime.Now.Ticks, UriKind.Relative));
        }

        private void SetSettings()
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
                        MessageBox.Show("This app will now close.", "Location", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                        {
                            Application.Current.Terminate();
                        }
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
        }

        private void CheckBox_LocationService_Checked(object sender, RoutedEventArgs e)
        {
            SettingsContainer.LocationConsent.Value = true;
        }
    }
}