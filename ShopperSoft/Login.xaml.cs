using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace ShopperSoft
{
    public partial class Login : PhoneApplicationPage
    {
        public string username;
        public string phoneNo;
        public Login()
        {
            InitializeComponent();
        }

        private void PerformUserLogin(object sender, System.Windows.Input.GestureEventArgs e)
        {
            username = userName.Text;
            phoneno = userPhone.Text;

            // TODO: send this username and phoneno. to be added into the database

            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}