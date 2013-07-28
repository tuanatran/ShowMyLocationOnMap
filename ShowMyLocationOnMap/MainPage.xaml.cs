using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ShowMyLocationOnMap.Resources;
using Microsoft.Phone.Maps.Controls;
using System.Device.Location; // Provides the GeoCoordinate class.
using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;

namespace ShowMyLocationOnMap
{
    public partial class MainPage : PhoneApplicationPage
    {
       // Geolocator myGeolocator = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            ShowMyLocationOnTheMap();
            GetDirection();
            //TrackLocation();
           

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private void TrackLocation()
        {
            //myGeolocator = new Geolocator();
            //myGeolocator.DesiredAccuracy = PositionAccuracy.High;
            //myGeolocator.MovementThreshold = 100; // The units are meters. 
            //myGeolocator.StatusChanged += geolocator_StatusChanged;
            //myGeolocator.PositionChanged += geolocator_PositionChanged;
        }

        private async void GetDirection()
        {
            // Get Directions 
            MapsDirectionsTask mapsDirectionsTask = new MapsDirectionsTask();
            // You can specify a label and a geocoordinate for the end point. //
            // GeoCoordinate spaceNeedleLocation = new GeoCoordinate(47.6204,-122.3493); 
            // LabeledMapLocation s
            // paceNdleLML = new LabeledMapLocation("Space Needle",  spaceNeedleLocation);  

            // If you set the geocoordinate parameter to null, the label parameter // is used as a search term. 
            LabeledMapLocation spaceNdleLML = new LabeledMapLocation("Space Needle", null);
            // If mapsDirectionsTask.Start is not set, the user's current location // is used as start point. 
            mapsDirectionsTask.End = spaceNdleLML;
            mapsDirectionsTask.Show(); 
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains("LocationConsent"))
            {
                // User has opted in or out of Location
                return;
            }
            else
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = false;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private async void ShowMyLocationOnTheMap()
        {
            // Get my current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate = 
            CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

            // Make my current location the center of the Map.
            this.mapWithMyLocation.Center = myGeoCoordinate;
            this.mapWithMyLocation.ZoomLevel = 13;

            // Create a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = myGeoCoordinate;

            // Create a MapLayer to contain the MapOverlay.
            MapLayer myLocationLayer = new MapLayer();
            myLocationLayer.Add(myLocationOverlay);

            // Add the MapLayer to the Map.
            mapWithMyLocation.Layers.Add(myLocationLayer);
        }
    }
}