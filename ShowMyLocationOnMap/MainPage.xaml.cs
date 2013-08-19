namespace ShowMyLocationOnMap
{
    using System;
    using System.Windows;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Maps.Controls;
    using System.Device.Location; // Provides the GeoCoordinate class.
    using Windows.Devices.Geolocation; //Provides the Geocoordinate class.
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.IO.IsolatedStorage;
    using Microsoft.Phone.Tasks;
    using Microsoft.WindowsAzure.MobileServices;
    using Newtonsoft.Json;
    using ShowMyLocationOnMap.DataModel;
    using System.Threading.Tasks;
    using Microsoft.Phone.Maps.Toolkit;
    using System.Collections.ObjectModel;
    using System.Linq;

    public sealed partial class MainPage : PhoneApplicationPage
    {
        Geolocator geolocator = null;
        GeoCoordinate myGeoCoordinate;
        bool tracking = false;

        public class Location
        {
            public String Latitude { get; set; }
            public String Longitude { get; set; }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
        }

        private async Task GetMyLocation()
        {
            // Get my current location.
            Geolocator myGeolocator = new Geolocator();
            Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
            Geocoordinate geocoordinate = myGeoposition.Coordinate;
            GeoCoordinate myGeoCoordinate =
            CoordinateConverter.ConvertGeocoordinate(geocoordinate);
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

        private void Pushpin_OnTap(object sender, GestureEventArgs e)
        {
            Pushpin pushpin = sender as Pushpin;
            MessageBox.Show(pushpin.Content.ToString());
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
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                // The user has opted out of Location.
                return;
            }

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

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await GetMyLocation();
            ObservableCollection<DependencyObject> children = MapExtensions.GetChildren(mapWithMyLocation);
            var obj = children.FirstOrDefault(x => x.GetType() == typeof(MapItemsControl)) as MapItemsControl;
            UserLocationMarker marker = (UserLocationMarker)this.FindName("UserLocationMarker");
            marker.GeoCoordinate = myGeoCoordinate;

            Pushpin pushpin = (Pushpin)this.FindName("MyPushpin");
            pushpin.GeoCoordinate = new GeoCoordinate(30.712474, -132.32691);

        }
    }
}