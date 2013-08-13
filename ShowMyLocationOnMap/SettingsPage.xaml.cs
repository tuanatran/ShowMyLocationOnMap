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
        string authType = SettingsContainer.AuthType.Value.ToString();
        private LiveConnectClient client;
        private LiveConnectSession _session;
        public SettingsPage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            await AttemptLogin(authType);
        }

        private async void btnSignin_SessionChanged(object sender, LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                client = new LiveConnectClient(e.Session);
                LiveOperationResult operationResult = await client.GetAsync("me");
                try
                {
                    dynamic meResult = operationResult.Result;
                    if (meResult.first_name != null &&
                        meResult.last_name != null)
                    {
                        infoTextBlock.Text = "Hello " +
                            meResult.first_name + " " +
                            meResult.last_name + "!";
                    }
                    else
                    {
                        infoTextBlock.Text = "Hello, signed-in user!";
                    }
                }
                catch (LiveConnectException exception)
                {
                    this.infoTextBlock.Text = "Error calling API: " +
                        exception.Message;
                }
            }
            else
            {
                infoTextBlock.Text = "Not signed in.";
            }
        }

        public async Task<bool> AttemptLogin(string authType)
        {
            try
            {
                if (authType == typeof(MobileServiceAuthenticationProvider).ToString())
                {
                    if (!String.IsNullOrEmpty(SettingsContainer.LiveConnectToken.Value))
                    {
                        App.CurrentUser = await App.MobileService.LoginAsync(MobileServiceAuthenticationProvider.MicrosoftAccount);
                    }
                    else
                    {
                        var liveIdClient = new LiveAuthClient(App.APP_AUTHKEY_LIVECONNECT);

                        while (_session == null)
                        {
                            var result = await liveIdClient.LoginAsync(new[] { "wl.signin" });

                            if (result.Status != LiveConnectSessionStatus.Connected)
                            {
                                continue;
                            }

                            _session = result.Session;

                            App.CurrentUser = await App.MobileService.LoginWithMicrosoftAccountAsync(result.Session.AuthenticationToken);

                            SettingsContainer.LiveConnectToken.Value = result.Session.AuthenticationToken;
                        }
                    }
                    SettingsContainer.FirstRunTime.Value = DateTime.Now;
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


    }
}