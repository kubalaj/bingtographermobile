/*===============================MainPage.xaml.cs-======================*/
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



namespace Bingtographer_App
{

    /*Set the variable used for the phone most of the variables are strings 
     and types used to push to the database. We also have varible to detirmine
     which button and modes are clicked for different options*/
    public partial class MainPage : PhoneApplicationPage
    {


        // Road Type Constants

        //Bikes
        bool bike = false;
        const string BikePath = "pedestrian";
        const string BikeTrail = "tertiary";
        const string BikeLane = "motorway";
        // Walking
        bool walking = true;
        const string Sidewalk = "pedestrian";
        const string WalkingTrail = "tertiary";
        const string Path = "pedestrian";
        //Car
        bool road = false;
        const string OffRoadTrail = "tertiary";
        const string Road = "motorway";
        const string Alley = "secondary";

        bool nextPage = false;
        bool checkedBox = false;
        string mode = "";



        string type = "";

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

  
        /*This is the button that will navigate to the next page. It includes error checking.
         if any any of the options aren't select an error message will pop up
         telling the user to first select the fill in the missing field*/
        private void hyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            if (textbox1.Text == "")
            {
                MessageBoxResult result = MessageBox.Show("Please add a name in the namebox before tracking");
                nextPage = false;
            }
            else if (checkedBox == false)
            {
                MessageBoxResult result = MessageBox.Show("Please add select the type of path you are taking");
            }
            else
            {
                nextPage = true;
            }

            if (nextPage == true && checkedBox == true)
            {
                
                MessageBoxResult result =
                    MessageBox.Show("You are about to submit \n"+textbox1.Text+" as the name of your path and \n"+type+" as the type of path is this okay?",
                    "New Trace",
                    MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    NavigationService.Navigate(new Uri("/Page2.xaml?msg=" + textbox1.Text + "&type=" + type + "&mode="+mode, UriKind.Relative));
                }
            
            }
            
        }



  /*======================Modes of Transportation=============================*/
        /*Depending on whether the walking, driving, or biking buttons are selected
         the application will change the right icon as well as set variable to let the
         other methods know which was selected*/
        private void walker_Click(object sender, RoutedEventArgs e)
        {
            walking = true;
            mode = "walking";
            bike = false;
            road = false;

            option1.Content = Sidewalk;
            option2.Content = WalkingTrail;
            option3.Content = Path;
            walkericonsolid.Opacity = 100;
            walkericon.Opacity = 0;

            bikeicon.Opacity = 100;
            bikeiconsolid.Opacity = 0;

            caricon.Opacity = 100;
            cariconsolid.Opacity = 0;

        }


        private void bike_Click(object sender, RoutedEventArgs e)
        {
            walking = false;
            bike = true;
            mode = "bike";
            road = false;

            option1.Content = BikePath;
            option2.Content = BikeTrail;
            option3.Content = BikeLane;


            bikeiconsolid.Opacity = 100;
            bikeicon.Opacity = 0;

            walkericon.Opacity = 100;
            walkericonsolid.Opacity = 0;

            caricon.Opacity = 100;
            cariconsolid.Opacity = 0;
        }

        private void car_Click(object sender, RoutedEventArgs e)
        {
            walking = false;
            bike = false;
            mode = "road";
            road = true;

            option1.Content = OffRoadTrail;
            option2.Content = Road;
            option3.Content = Alley;

            bikeiconsolid.Opacity = 0;
            bikeicon.Opacity = 100;

            walkericon.Opacity = 100;
            walkericonsolid.Opacity = 0;

            caricon.Opacity = 0;
            cariconsolid.Opacity = 100;
        }

        /*============================================================*/



        /*===========option_check===================================*/
       /*This set will check to see what option was checked and set that
          a varaible to that type for further method used, more importantly
        it changes the radioboxs used to show the options for travel in the application*/
        private void option1_Checked(object sender, RoutedEventArgs e)
        {
            checkedBox = true;
            if (walking == true)
            {
                type = Sidewalk;
            }
            else if (bike == true)
            {
                type = BikePath;
            }
            else if (road == true)
            {
                type = OffRoadTrail;
            }
        }


        private void option2_Checked(object sender, RoutedEventArgs e)
        {
            checkedBox = true;
            if (walking == true)
            {
                type = WalkingTrail;
            }
            else if (bike == true)
            {
                type = BikeTrail;
            }
            else if (road == true)
            {
                type = Road;
            }
        }



        private void option3_Checked(object sender, RoutedEventArgs e)
        {
            checkedBox = true;
            if (walking == true)
            {
                type = Path;
            }
            else if (bike == true)
            {
                type = BikePath;

            }
            else if(road == true)
            {
                type = Alley;
            }
        }

        /*============================================================*/

 
    }
}