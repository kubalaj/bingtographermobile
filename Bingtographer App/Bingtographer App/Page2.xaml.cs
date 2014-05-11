/*=================Page2.xaml.cs=========================*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Bingtographer_App.Resources;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using System.IO.IsolatedStorage;

//Includes the libraries to make http requests
//to push to the DataBase.
using System.Text;
using System.Net.Http;

using System.Diagnostics;
using System.Windows.Threading;


// Provides the GeoCoordinate class.
using Microsoft.Phone.Maps.Controls;
using System.Device.Location; 


// Libraries for the Map view
using System.Windows.Media;
using System.Windows.Shapes;

using Microsoft.Phone.Net.NetworkInformation;



namespace Bingtographer_App
{
    public partial class Page2 : PhoneApplicationPage
    {

        /* ============GLOBAL VARIBLES=====================*/
        // Variables for 
        Geolocator geolocator = null;
        bool firstPoint;
        bool tracking = false;
        string name = "";
        string path = "";
        string lat = "";
        string longi = "";
        string complete = "";
        string key = "";
        string lattitudetextblock = "";
        string longitudetextblock = "";

        //Varible for a polyline for the path
        private MapPolyline pathLine;

        //Varibles for tracking time
        private DispatcherTimer _timer = new DispatcherTimer();
        private long _startTime; 

        // Variables for tracking the Distance
        string stringDistance =  " ";
        double x = 0;
        double y = 0;
        double xPrev = 0;
        double yPrev = 0;
        double dist = 0;
        List<string> lattitude = new List<string>();
        List<string> longitude = new List<string>();

        /*==========================================================*/    

        public Page2()
        {
            //Initialize the phone app, prompt the map to show and set the zoom level
            InitializeComponent();
            this.mapWithMyLocation.ZoomLevel = 13;
            firstPoint = true;
            ShowMyLocationOnTheMap();
            geolocator = new Geolocator();

            // create a line which illustrates the run
            pathLine = new MapPolyline();
            pathLine.StrokeColor = Colors.Red;
            pathLine.StrokeThickness = 5;
            mapWithMyLocation.MapElements.Add(pathLine);
        }

        /*This function is more initzliation for the Geolocaor class that is used*/
        protected override void OnRemovedFromJournal(System.Windows.Navigation.JournalEntryRemovedEventArgs e)
        {
            App.Geolocator.PositionChanged -= geolocator_PositionChanged;
            App.Geolocator = null;
        }

        <summary>
        </summary>

        /*=============================Continually Track Positon======================*/
        /*Much of the code used to continually track the loaction of the phone was used from
         a tutorial by on Microsofts development site. It can be found in the link provided
         below*/
        /*http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj247548(v=vs.105).aspx*/
        
        
        /*This will initailize many of the compoents used. It sets the Accuray and to high as well
         as sets the Movement threshold of when to collect date. It will also Set the title to the 
         path title passed my MainPage.xaml. It checks location preferences as well to make sure
         consent is on.*/
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string msg = "";
            string type = "";

            if (App.Geolocator == null)
            {
                App.Geolocator = new Geolocator();
                App.Geolocator.DesiredAccuracy = PositionAccuracy.High;
                App.Geolocator.DesiredAccuracyInMeters = 50;
                App.Geolocator.MovementThreshold = 100; // The units are meters.
                App.Geolocator.PositionChanged += geolocator_PositionChanged;
            }


            if (NavigationContext.QueryString.TryGetValue("msg", out msg))
            {
                textblock1.Text = msg;
            }
            if (NavigationContext.QueryString.TryGetValue("type", out type))
            {
                path = type;
            }
        

            name = msg;
            

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


        /*Start Click will start the inizalization and tracking of the application. If start is 
         pressed first the icon will change to the standard stop icon and the text will also change from
         start trace to stop trace. Then it will see if location consent was set to yes. If it passes this 
         check it will see the mode of transportation and adjust the collection thresolhold accordingly. It then
         everythime this threshold is passes will update the position changed. A timer is also set to start, this
         is used to calculate the average speed of the application. When the button is pressed again
         the stop button and text will change to its origanal display. The timer will also stop for the speed calculation. */
        public void Start_Click(object sender, EventArgs e)
        {

         

            ApplicationBarIconButton btn = (ApplicationBarIconButton)ApplicationBar.Buttons[0];
            if (tracking == false)
            {
                _timer.Start();
                _startTime = System.Environment.TickCount;
                btn.Text = "stop trace";
                btn.IconUri = new Uri("/Images/stop.png", UriKind.Relative);
                //startTime = moment.Second;

                if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
                {
                    // The user has opted out of Location.
                    return;
                }

                if (!tracking)
                {
                    geolocator = new Geolocator();
                    geolocator.DesiredAccuracy = PositionAccuracy.High;

                    string mode = "";
                    //Setting the Movement Threshold for the different desired modes of transportation
                    if (NavigationContext.QueryString.TryGetValue("mode", out mode))
                    {
                        if (mode == "walking")
                        {
                            // For Walking
                            geolocator.MovementThreshold = .5; // The units are meters.
                        }
                        if (mode == "bike")
                        {
                            // For Bike
                            geolocator.MovementThreshold = 3; // The units are meters.
                        }
                        else
                        {
                            // For Car
                            geolocator.MovementThreshold = 5; // The units are meters.
                        }
                    }

                    geolocator.StatusChanged += geolocator_StatusChanged;
                    geolocator.PositionChanged += geolocator_PositionChanged;

                    tracking = true;
                    //TrackLocationButton.Content = "stop and submit trace";
                }
            }
            else if (btn.Text == "stop trace")
            {
                _timer.Stop();
                geolocator.PositionChanged -= geolocator_PositionChanged;
                geolocator.StatusChanged -= geolocator_StatusChanged;
                geolocator = null;

                tracking = false;
                StatusTextBlock.Text = "stopped";
                btn.Text = "new trace";
                btn.IconUri = new Uri("/Images/play.png", UriKind.Relative);

                MessageBoxResult result =
                    MessageBox.Show("You Have Currently Stopped your trace, to resume please press start",
                    "Stopped Trace",
                    MessageBoxButton.OK);
                if (result==MessageBoxResult.OK)
                {
                    btn.Text = "start trace";
                    btn.IconUri = new Uri("/Bingtographer_Icons/transport.play.png", UriKind.Relative);
                }

            }
           
        }

        /*This will be the visual used for the user to see the status of the phone's GPS. Depending on 
         whether the phone's GPS is ready, not available, initializing, ect..... this will display
         a message on the phone accordingly*/
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
 

        /*This is the method that will actually calculate the distance. It takes 4 points,
         converts from lattitude and longitude to miles and uses the standard
         distance formule to return the distance between two points in miles*/
        public static double distance(double x1, double y1, double x2, double y2)
        {
            double lattitudeFactor = x1;
            double ans = 0;
            x1 -= x2;
            y1 -= y2;

            //Convert to positive such that there is no error with cos
            Math.Abs(x1);
            Math.Abs(y1);

            //convert point 1
            y1 = y1 * (69.172 * Math.Cos(lattitudeFactor));
            x1 = x1 * 69.172;
            y1 = Math.Abs(y1);
            x1 = Math.Abs(x1);
            ans = Math.Sqrt(Math.Pow(x1, 2) + Math.Pow(y1, 2));
            return ans;
        }


        /*This method will detirmine and add when a postion is changed to the phone. It will first initialize 
         a new Geocooridante object that will be used to track the position of the phone. It will Initialize
         this and set this positon to our latttidue and longitude string. If this is the first time it is 
         called it will push this current point to the database. This creates a key and allows the constanst sync feature
         provided by the application. If not it will calculate the distance and the speed; display that information as well
         as draw a line based on where you are from the last collected point.*/
        void geolocator_PositionChanged(Geolocator sender, PositionChangedEventArgs args)
        {
            var lineCoord = new GeoCoordinate(args.Position.Coordinate.Latitude, args.Position.Coordinate.Longitude);

            if (!App.RunningInBackground)
            {
                Dispatcher.BeginInvoke(() =>
                {
                    lattitudetextblock = args.Position.Coordinate.Latitude.ToString("0.00");
                    longitudetextblock = args.Position.Coordinate.Longitude.ToString("0.00");
                    ShowMyLocationOnTheMap();
                });
            }
            else
            {
                Microsoft.Phone.Shell.ShellToast toast = new Microsoft.Phone.Shell.ShellToast();
                toast.Content = args.Position.Coordinate.Latitude.ToString("0.00");
                toast.Title = "Location: ";
                toast.NavigationUri = new Uri("/Page2.xaml", UriKind.Relative);
                toast.Show();

            }



            Dispatcher.BeginInvoke(async() =>
            {
                lattitudetextblock = args.Position.Coordinate.Latitude.ToString("0.0000000000");
                lat = lattitudetextblock;

                longitudetextblock = args.Position.Coordinate.Longitude.ToString("0.0000000000");
                longi = longitudetextblock;
                

                int index = lattitude.Count;
                double storeThresholdx = 0.0;
                double storeThresholdy = 0.0;
                x = Convert.ToDouble(lat);
                y = Convert.ToDouble(longi);
                if (lattitude.Count == 0 || longitude.Count == 0)
                {
                    lattitude.Add(lat);
                    longitude.Add(longi);
                    string defaultURL = "http://107.170.254.134/db2/add.php?name=insertName&type=insertType";
                    HttpContent content;

                    defaultURL = defaultURL.Replace("insertName", name);
                    defaultURL = defaultURL.Replace("insertType", path);
                    HttpClient httpClient = new HttpClient();
                    String locationString = RoadToString();
                    content = new StringContent(locationString);
                    try
                    {
                        HttpResponseMessage response = await httpClient.PostAsync(defaultURL, content).ConfigureAwait(false);
                        String newString = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine(newString);
                        String keyValue = newString;
                        keyValue = keyValue.Replace("key:", "");
                        keyValue = keyValue.Replace("time:", "^");
                        keyValue = keyValue.Replace(" ", "");
                        String[] keyTime = keyValue.Split('^');
                        key = keyTime[0];

                    }
                    catch (HttpRequestException hre)
                    {
                        System.Diagnostics.Debug.WriteLine(hre);
                    }
                }
                else if (lattitude.Count != 0 || longitude.Count != 0)
                {
                    index = index - 1;
                    xPrev = Convert.ToDouble(lattitude[lattitude.Count - 1]);
                    yPrev = Convert.ToDouble(longitude[longitude.Count -1]);
                    storeThresholdx = x - xPrev;
                    storeThresholdy = y - yPrev;
                    if (storeThresholdx != 0 || storeThresholdy != 0)
                    {
                        lattitude.Add(lat);
                        longitude.Add(longi);

                        Sync(RoadToString());
                        storeThresholdx = 0;
                        storeThresholdy = 0;
                        dist += distance(x, y, xPrev, yPrev);

                        stringDistance = Convert.ToString(Math.Round(dist,5));

                        string speedText = "";
                        
                       
                        MenuDistance.Text = "Distance: " + stringDistance + " mi";
                       

                        if (args.Position.Coordinate.Speed != null)
                        {
                            //speedText = args.Position.Coordinate.Speed.ToString("0.000"); // in meters/second
                            speedText = args.Position.Coordinate.Speed.HasValue ? args.Position.Coordinate.Speed.ToString() : "UNKNOWN"; // in meters/second
                            MenuSpeed.Text = "Speed: " + speedText + " meters/second";
                        }

                        
                    }
                    
                    // to show path
                    pathLine.Path.Add(lineCoord);
                }





            });
        }

        /*==================================================================================================*/

       
        /*=======================MAP================================*/ 

        /*Followed and modified a tutorial on Microsofts development site. Tutorial may be found at 
         link provided below*/
        /*http://msdn.microsoft.com/en-us/library/windowsphone/develop/jj735578(v=vs.105).aspx */


        /*Initalize the Geocoordinate class use for the map position*/
        public static class CoordinateConverter
        {
            public static GeoCoordinate ConvertGeocoordinate(Geocoordinate geocoordinate)
            {
                return new GeoCoordinate
                    (
                    geocoordinate.Latitude,
                    geocoordinate.Longitude,
                    geocoordinate.Altitude ?? Double.NaN,
                    geocoordinate.Accuracy,
                    geocoordinate.AltitudeAccuracy ?? Double.NaN,
                    geocoordinate.Speed ?? Double.NaN,
                    geocoordinate.Heading ?? Double.NaN
                    );
            }
        }

       /*This method will initailze and show the location on the map/ It also will put an elipse
        on the current postion and will update depending on where you are and place a new point 
        accordingly*/
        private async void ShowMyLocationOnTheMap()
        {

                // Get my current location.
                Geolocator myGeolocator = new Geolocator();
                Geoposition myGeoposition = await myGeolocator.GetGeopositionAsync();
                Geocoordinate myGeocoordinate = myGeoposition.Coordinate;
                GeoCoordinate myGeoCoordinate =
                    CoordinateConverter.ConvertGeocoordinate(myGeocoordinate);

                // Make my current location the center of the Map.
                if (firstPoint == true)
                {
                    this.mapWithMyLocation.Center = myGeoCoordinate;
                    firstPoint = false;
                }

                Ellipse myCircle = new Ellipse();
                myCircle.Fill = new SolidColorBrush(Colors.Blue);
                myCircle.Height = 10;
                myCircle.Width = 10;
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

        /*=======================================================*/ 




        /*This method will continusly sync the data grabed from the phone. It will use the insert key
         function in the database to constantly append to the key associated with the current GPS trace.*/
        public async void Sync(String locationString)
        {

            //Check to see if the app is either wifi or netwrok connected

  

            string updateRoadURL = "http://107.170.254.134/db2/updateRoad.php?key=insertKey";
            //App.updating = true;
            String newURL = updateRoadURL;
            newURL = newURL.Replace("insertKey", key);
            HttpClient httpClient = new HttpClient();
            HttpContent content = new StringContent(locationString);
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(newURL, content).ConfigureAwait(false);
                String timeValue = await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException hre)
            {
                System.Diagnostics.Debug.WriteLine(hre);
            }
            //App.updating = false;

        }



        /*This is the function that will convert the list of lattitudes and longitudes to the approperiate
         String. The string will first contain the inital content for it to be read by the databse and then through 
         a loop append all latttiudes and longitudes created.*/
        private String RoadToString()
        {
            String locationString = null;
            String newComplete = "";
            locationString += "{\"type\":\"MultiLineString\",\"coordinates\":[[insertCoordinates]],\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"EPSG:4326\"}}}";
            for (int x = 0; x < lattitude.Count; x++)
            {
                newComplete += "[" + longitude[x] + "," + lattitude[x] + "],";
            }
            newComplete = newComplete.Remove(newComplete.Length - 1, 1);
            locationString = locationString.Replace("insertCoordinates", newComplete);
            return locationString;
        }

    }
}