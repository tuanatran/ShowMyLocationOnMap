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
using System.IO.IsolatedStorage;
using Microsoft.Phone.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json;




namespace ShowMyLocationOnMap
{
    public partial class MainPage : PhoneApplicationPage
    {

        private MobileServiceCollection<DeliveryRoute, DeliveryRoute> deliveries;
        private IMobileServiceTable<DeliveryRoute> deliveryTable = App.MobileService.GetTable<DeliveryRoute>();

        public class DeliveryRoute
        {
            public int Id { get; set; }

            [JsonProperty(PropertyName = "destinationPoints")]
            public DoubleCollection[] DestinationPoints { get; set; }

            [JsonProperty(PropertyName = "driver")]
            public int Driver { get; set; }
        }

        public class Driver
        {
            public int Id { get; set; }

            [JsonProperty(PropertyName = "firstName")]
            public string FirstName { get; set; }

            [JsonProperty(PropertyName = "lastName")]
            public string LastName { get; set; }
        }

        Geolocator geolocator = null;
        bool tracking = false;
        const int MIN_ZOOM_LEVEL = 1;
        const int MAX_ZOOM_LEVEL = 20;
        const int MIN_ZOOMLEVEL_FOR_LANDMARKS = 16;

        ToggleStatus locationToggleStatus = ToggleStatus.ToggledOff;
        ToggleStatus landmarksToggleStatus = ToggleStatus.ToggledOff;
        GeoCoordinate currentLocation = null;
        MapLayer locationLayer = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            //this.Loaded += MainPage_Loaded;
            //ShowMyLocationOnTheMap();

            // Create the localized ApplicationBar.
            BuildLocalizedApplicationBar();
            //ShowMyLocationOnTheMap();

            // Get current location.
            GetLocation();
        }

       

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
        

       

        private void Route_Click(object sender, RoutedEventArgs e)
        {
            string endLocation = End.Text;
            MapsDirectionsTask mapsDirectionsTask = new MapsDirectionsTask();

            // If you set the geocoordinate parameter to null, the label parameter is used as a search term. 
            LabeledMapLocation endLocationLML = new LabeledMapLocation(endLocation, null);
            // If mapsDirectionsTask.Start is not set, the user's current location // is used as start point. 
            mapsDirectionsTask.End = endLocationLML;
            mapsDirectionsTask.Show(); 
        }

        void geolocator_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            string status = "";

            switch (args.Status)
            {
                case PositionStatus.Disabled:
                    // the application does not have the right capability or the location master switch is off
                    status = "location is disabled in phone settings";
                    break;
                case PositionStatus.Initializing:
                    // the geolocator started the tracking operation
                    status = "initializing";
                    break;
                case PositionStatus.NoData:
                    // the location service was not able to acquire the location
                    status = "no data";
                    break;
                case PositionStatus.Ready:
                    // the location service is generating geopositions as specified by the tracking parameters
                    status = "ready";
                    break;
                case PositionStatus.NotAvailable:
                    status = "not available";
                    // not used in WindowsPhone, Windows desktop uses this value to signal that there is no hardware capable to acquire location information
                    break;
                case PositionStatus.NotInitialized:
                    // the initial state of the geolocator, once the tracking operation is stopped by the user the geolocator moves back to this state

                    break;
            }

            Dispatcher.BeginInvoke(() =>
            {
                StatusTextBlock.Text = status;
            });
        }

        void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            Dispatcher.BeginInvoke(() =>
            {
                LatitudeTextBlock.Text = args.Position.Coordinate.Latitude.ToString("0.00");
                LongitudeTextBlock.Text = args.Position.Coordinate.Longitude.ToString("0.00");
            });
        }

        private void TrackLocationButton_Click(object sender, RoutedEventArgs e)
        {
            /*if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }*/

            if (!tracking)
            {
                geolocator = new Geolocator();
                geolocator.DesiredAccuracy = PositionAccuracy.High;
                geolocator.MovementThreshold = 100; // The units are meters.

                geolocator.StatusChanged += geolocator_StatusChanged;
                geolocator.PositionChanged += geolocator_PositionChanged;

                tracking = true;
                TrackLocationButton.Content = "stop tracking";
            }
            else
            {
                geolocator.PositionChanged -= geolocator_PositionChanged;
                geolocator.StatusChanged -= geolocator_StatusChanged;
                geolocator = null;

                tracking = false;
                TrackLocationButton.Content = "track location";
                StatusTextBlock.Text = "stopped";
            }
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
        }

        #region Event handlers for App Bar buttons and menu items

        void ToggleLocation(object sender, EventArgs e)
        {
            switch (locationToggleStatus)
            {
                case ToggleStatus.ToggledOff:
                    ShowLocation();
                    CenterMapOnLocation();
                    locationToggleStatus = ToggleStatus.ToggledOn;
                    break;
                case ToggleStatus.ToggledOn:
                    this.mapWithMyLocation.Layers.Remove(locationLayer);
                    locationLayer = null;
                    locationToggleStatus = ToggleStatus.ToggledOff;
                    break;
            }
        }

        void ToggleLandmarks(object sender, EventArgs e)
        {
            switch (landmarksToggleStatus)
            {
                case ToggleStatus.ToggledOff:
                    this.mapWithMyLocation.LandmarksEnabled = true;
                    if (this.mapWithMyLocation.ZoomLevel < MIN_ZOOMLEVEL_FOR_LANDMARKS)
                    {
                        this.mapWithMyLocation.ZoomLevel = MIN_ZOOMLEVEL_FOR_LANDMARKS;
                    }
                    landmarksToggleStatus = ToggleStatus.ToggledOn;
                    break;
                case ToggleStatus.ToggledOn:
                    this.mapWithMyLocation.LandmarksEnabled = false;
                    landmarksToggleStatus = ToggleStatus.ToggledOff;
                    break;
            }

        }

        void ZoomIn(object sender, EventArgs e)
        {
            if (this.mapWithMyLocation.ZoomLevel < MAX_ZOOM_LEVEL)
            {
                this.mapWithMyLocation.ZoomLevel++;
            }
        }

        void ZoomOut(object sender, EventArgs e)
        {
            if (this.mapWithMyLocation.ZoomLevel > MIN_ZOOM_LEVEL)
            {
                this.mapWithMyLocation.ZoomLevel--;
            }
        }

        #endregion

        #region Helper functions for App Bar button and menu item event handlers

        private void ShowLocation()
        {
            // Cr*eate a small circle to mark the current location.
            Ellipse myCircle = new Ellipse();
            myCircle.Fill = new SolidColorBrush(Colors.Blue);
            myCircle.Height = 20;
            myCircle.Width = 20;
            myCircle.Opacity = 50;

            // Create a MapOverlay to contain the circle.
            MapOverlay myLocationOverlay = new MapOverlay();
            myLocationOverlay.Content = myCircle;
            myLocationOverlay.PositionOrigin = new Point(0.5, 0.5);
            myLocationOverlay.GeoCoordinate = currentLocation;

            // Create a MapLayer to contain the MapOverlay.
            locationLayer = new MapLayer();
            locationLayer.Add(myLocationOverlay);

            // Add the MapLayer to the Map.
            this.mapWithMyLocation.Layers.Add(locationLayer);

        }

        private async void GetLocation()
        {
            // Get current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
            currentLocation = CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);
        }

        private void CenterMapOnLocation()
        {
            this.mapWithMyLocation.Center = currentLocation;
        }

        #endregion

        // Create the localized ApplicationBar.
        private void BuildLocalizedApplicationBar()
        {
            // Set the page's ApplicationBar to a new instance of ApplicationBar.
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Opacity = 0.5;

            // Create buttons with localized strings from AppResources.
            // Toggle Location button.
            ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/location.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarToggleLocationButtonText;
            appBarButton.Click += ToggleLocation;
            ApplicationBar.Buttons.Add(appBarButton);
            // Toggle Landmarks button.
            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/landmarks.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarToggleLandmarksButtonText;
            appBarButton.Click += ToggleLandmarks;
            ApplicationBar.Buttons.Add(appBarButton);
            // Zoom In button.
            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/zoomin.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarZoomInButtonText;
            appBarButton.Click += ZoomIn;
            ApplicationBar.Buttons.Add(appBarButton);
            // Zoom Out button.
            appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/zoomout.png", UriKind.Relative));
            appBarButton.Text = AppResources.AppBarZoomOutButtonText;
            appBarButton.Click += ZoomOut;
            ApplicationBar.Buttons.Add(appBarButton);

            // Create menu items with localized strings from AppResources.
            // Toggle Location menu item.
            ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarToggleLocationMenuItemText);
            appBarMenuItem.Click += ToggleLocation;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            // Toggle Landmarks menu item.
            appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarToggleLandmarksMenuItemText);
            appBarMenuItem.Click += ToggleLandmarks;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            // Zoom In menu item.
            appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarZoomInMenuItemText);
            appBarMenuItem.Click += ZoomIn;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
            // Zoom Out menu item.
            appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarZoomOutMenuItemText);
            appBarMenuItem.Click += ZoomOut;
            ApplicationBar.MenuItems.Add(appBarMenuItem);
        }

        private enum ToggleStatus
        {
            ToggledOff,
            ToggledOn
        }
    }
}