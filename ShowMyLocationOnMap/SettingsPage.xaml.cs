using System;
using System.Windows;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Microsoft.Live;
using Microsoft.Live.Controls;
using System.Text;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;

namespace ShowMyLocationOnMap
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        private MobileServiceAuthenticationProvider authType = SettingsContainer.AuthType.Value;
        private LiveAuthClient _authClient = App.AuthClient;
        private LiveConnectClient _client;
        private LiveConnectSession _session;
        public static MyData settings;
        public static bool consent = false;

        public SettingsPage()
        {
            InitializeComponent();
            LoadData();
            Loaded += SettingsPage_Loaded;
        }

        public class MyData : INotifyPropertyChanged
        {
            private string myDataProperty;
            private bool userIdleDetectionCheckBoxEnabled;
            private bool appIdleDetectionCheckBoxEnabled;

            public MyData() { }

            public MyData(DateTime dateTime)
            {
                myDataProperty = "Last bound time was " + dateTime.ToLongTimeString();
            }

            public String UserName
            {
                get { return SettingsContainer.UserName.Value; }
                set
                {
                    SettingsContainer.UserName.Value = value;
                    OnPropertyChanged("UserName");
                }
            }

            public DateTime SessionExpires
            {
                get { return SettingsContainer.SessionExpires.Value; }
                set
                {
                    SettingsContainer.SessionExpires.Value = value;
                    OnPropertyChanged("SessionExpires");
                }
            }

            public bool LocationConsent
            {
                get { return SettingsContainer.LocationConsent.Value; }
                set
                {
                    SettingsContainer.LocationConsent.Value = value;
                    OnPropertyChanged("LocationConsent");
                    SetLocationConsent(value);
                }
            }

            public bool UserIdleDetectionDisabled
            {
                get { return SettingsContainer.DisableUserIdleDetection.Value; }
                set
                {
                    SettingsContainer.DisableUserIdleDetection.Value = value;
                    OnPropertyChanged("UserIdleDetectionDisabled");
                    SetUserIdleDetection(value);
                }
            }

            public bool UserIdleDetectionDisabledCheckBoxEnabled
            {
                get { return PhoneApplicationService.Current.UserIdleDetectionMode != IdleDetectionMode.Disabled; }
                set
                {
                    userIdleDetectionCheckBoxEnabled = value;
                    OnPropertyChanged("UserIdleDetectionDisabledCheckBoxEnabled");
                }
            }

            public bool AppIdleDetectionDisabled
            {
                get
                {
                    return SettingsContainer.DisableApplicationIdleDetection.Value;
                }
                set
                {
                    SettingsContainer.DisableApplicationIdleDetection.Value = value;
                    OnPropertyChanged("AppIdleDetectionDisabled");
                    SetApplicationIdleDetection(value);
                }
            }

            public bool AppIdleDetectionCheckBoxEnabled
            {
                get { return PhoneApplicationService.Current.ApplicationIdleDetectionMode != IdleDetectionMode.Disabled; }
                set
                {
                    appIdleDetectionCheckBoxEnabled = value;
                    OnPropertyChanged("AppIdleDetectionCheckBoxEnabled");
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }


        private static void SetUserIdleDetection(bool option)
        {
            if (!option)
            {
                StringBuilder msgText = new StringBuilder();
                msgText.Append("This app is intended to be run while being charged. Disabling User ");
                msgText.Append("Idle detection will ensure that the screen does not turn off while navigating. ");
                msgText.Append("Detection will be disabled only while this app is running, but you can manually enable ");
                msgText.Append("it from the Settings page.");
                MessageBoxResult result = MessageBox.Show(msgText.ToString(), "Disable User Idle Detection", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    consent = true;
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
                    settings.UserIdleDetectionDisabled = true;
                }
            }
            else
            {
                if (!consent)
                {
                    PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Enabled;
                    settings.UserIdleDetectionDisabled = false;
                }
            }
        }

        private static void SetApplicationIdleDetection(bool option)
        {
            if (!option)
            {
                StringBuilder msgText = new StringBuilder();
                msgText.Append("It is recommended that you also disable Application Idle detection. If this is not done, the application ");
                msgText.Append("will not run when the lock screen is engaged. When this is disabled, it will remain disabled until ");
                msgText.Append("your current session ends. Would you like to disable application idle detection now?");
                MessageBox.Show(msgText.ToString(), "Disable Application Idle Detection", MessageBoxButton.OK);
                PhoneApplicationService.Current.ApplicationIdleDetectionMode = IdleDetectionMode.Disabled;
                settings.AppIdleDetectionDisabled = true;
            }
        }

        private static void SetLocationConsent(bool option)
        {
            consent = false;
            // User must give location service consent for app to function
            // User has not opted in for Location
            if (!option)
            {
                MessageBoxResult result = MessageBox.Show("This app accesses your phone's location. Is that ok?", "Location", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    consent = true;
                }
                else
                {
                    result = MessageBox.
                        Show("This app will not function without your location. Press 'ok' to provide consent or 'cancel' to close.",
                        "Location", MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        consent = true;
                        settings.LocationConsent = consent;
                    }
                    else
                    {
                        MessageBox.Show("This app will now close.");
                        settings.LocationConsent = consent;
                        Application.Current.Terminate();
                    }
                }
            }
        }

        private async void SettingsPage_Loaded (object sender, RoutedEventArgs routedEventArgs)
        {
            await Authenticate(authType);
        }

        private void LoadData()
        {
            settings = new MyData();
            settings.LocationConsent = SettingsContainer.LocationConsent.Value;
            settings.UserIdleDetectionDisabled = (PhoneApplicationService.Current.UserIdleDetectionMode == IdleDetectionMode.Disabled);
            settings.AppIdleDetectionDisabled = (PhoneApplicationService.Current.ApplicationIdleDetectionMode == IdleDetectionMode.Disabled);
            settings.SessionExpires = SettingsContainer.SessionExpires.Value;
            Binding locationConsentBinding = new Binding("LocationConsent");
            locationConsentBinding.Mode = BindingMode.TwoWay;
            locationConsentBinding.Source = settings;
            CheckBox_LocationService.SetBinding(CheckBox.IsCheckedProperty, locationConsentBinding);
            Binding userIdleDetectionDisabledBinding = new Binding("UserIdleDetectionDisabled");
            userIdleDetectionDisabledBinding.Mode = BindingMode.TwoWay;
            userIdleDetectionDisabledBinding.Source = settings;
            CheckBox_UserIdleDetectionDisabled.SetBinding(CheckBox.IsCheckedProperty, userIdleDetectionDisabledBinding);
            Binding appIdleDetectionDisabledBinding = new Binding("AppIdleDetectionDisabled");
            appIdleDetectionDisabledBinding.Mode = BindingMode.TwoWay;
            appIdleDetectionDisabledBinding.Source = settings;
            CheckBox_AppIdleDetectionDisabled.SetBinding(CheckBox.IsCheckedProperty, appIdleDetectionDisabledBinding);
            Binding sessionExpiresBinding = new Binding("SessionExpires");
            sessionExpiresBinding.Mode = BindingMode.OneWay;
            sessionExpiresBinding.Source = settings;
            TextBlock_Session_Expires.SetBinding(TextBlock.TextProperty, sessionExpiresBinding);
            LockControls();
        }

        private void LockControls()
        {
            Binding userIdleDetectionCheckBoxEnabledBinding = new Binding("UserIdleDetectionCheckBoxEnabled");
            userIdleDetectionCheckBoxEnabledBinding.Mode = BindingMode.TwoWay;
            userIdleDetectionCheckBoxEnabledBinding.Source = settings;
            CheckBox_UserIdleDetectionDisabled.SetBinding(CheckBox.IsEnabledProperty, userIdleDetectionCheckBoxEnabledBinding);
            Binding appIdleDetectionCheckBoxEnabledBinding = new Binding("AppIdleDetectionCheckBoxEnabled");
            appIdleDetectionCheckBoxEnabledBinding.Mode = BindingMode.TwoWay;
            appIdleDetectionCheckBoxEnabledBinding.Source = settings;
            CheckBox_AppIdleDetectionDisabled.SetBinding(CheckBox.IsEnabledProperty, appIdleDetectionCheckBoxEnabledBinding);
            this.DataContext = settings;
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

        private async Task Authenticate (MobileServiceAuthenticationProvider authType)
        {
            while (App.CurrentUser == null)
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
                                settings.SessionExpires = DateTime.UtcNow.Add(_session.Expires.Offset);
                            }
                            else
                            {
                                LiveLoginResult loginResult = await _authClient.LoginAsync(
                                    new[] { "wl.signin", "wl.basic", "wl.offline_access" });
                                if (loginResult.Status == LiveConnectSessionStatus.Connected)
                                {
                                    _session = _authClient.Session;
                                    settings.SessionExpires = DateTime.UtcNow.Add(_session.Expires.Offset);
                                }
                            }
                            try
                            {
                                _client = new LiveConnectClient(_session);
                                LiveOperationResult operationResult = await _client.GetAsync("me");
                                dynamic result = operationResult.Result;
                                if (result != null)
                                {
                                    settings.UserName = result.name;
                                    this.infoTextBlock.Text = string.Join(" ", "Hello", result.name, "!");
                                }
                                else
                                {
                                    this.infoTextBlock.Text = "Error getting name.";
                                }
                                App.CurrentUser = await App.MobileService.LoginWithMicrosoftAccountAsync(_session.AuthenticationToken);
                                SettingsContainer.LiveConnectToken.Value = _session.AuthenticationToken;
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
                }
                catch (Exception ex)
                {
                    SettingsContainer.LiveConnectToken.Value = String.Empty;
                    StringBuilder message = new StringBuilder("An exception of type ");
                    message.Append(ex.GetType()).Append(" from source ").Append(ex.Source).Append(" occurred: ").Append(ex.Message);
                    MessageBox.Show(message.ToString());
                }
            }
        }
    }
}