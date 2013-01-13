using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ShopperSoft.Model;
using Microsoft.WindowsAzure.MobileServices;

namespace ShopperSoft
{
    public partial class Login : PhoneApplicationPage
    {
        public string username;
        public string phoneNo;
        public int user_id;

        public static MobileServiceClient MobileService;
        private static IMobileServiceTable<Users> userTable;

        public Login()
        {
            InitializeComponent();
        }

        private async void PerformUserLogin(object sender, System.Windows.Input.GestureEventArgs e)
        {
            username = userName.Text;
            phoneNo = userPhone.Text;

            if (MainPage.online == true)
            {
                Users user = new Users();
                user.Name = username;
                user.Phone_no = phoneNo;
                user.uri = "uri here";
                MobileService = new MobileServiceClient(
                     "https://shopappdata.azure-mobile.net/",
                       "dkwwuiuHYYQwbozjKaWRJYYpEiTjFt73"
                );
                userTable = MobileService.GetTable<Users>();

                await userTable.InsertAsync(user);
                user_id = user.Id;

                MainPage.settings.Add("id", user_id);
                MainPage.settings.Add("Pnumber", phoneNo);
                MainPage.settings.Add("name", username);
            }
            else
            {
                // Prompt
            }

            // TODO: send this username and phoneno. to be added into the database


            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}