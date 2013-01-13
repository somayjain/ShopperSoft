using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;

using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;
using ShopperSoft.Model;
using Microsoft.Phone.Net.NetworkInformation;

using Microsoft.WindowsAzure.MobileServices;
using Microsoft.Phone.Tasks;
using Windows.Phone.Speech.Recognition;

namespace ShopperSoft
{
    public partial class MainPage : PhoneApplicationPage
    {
        /* variables imported */
        private MobileServiceCollectionView<Items> items;

        public ObservableCollection<Items> retreive = new ObservableCollection<Items>();
        public ObservableCollection<Items> retreive2 = new ObservableCollection<Items>();
        private ObservableCollection<Items> buffer = new ObservableCollection<Items>();

    //    public static Items local;
        public static Items checked_item;
        public static Items local = new Items();

        public static MobileServiceClient MobileService;
        private static IMobileServiceTable<Items> itemTable;
       
        private static IMobileServiceTable<Users> userTable;
        public static Users user = new Users();

        public static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public string pnumber;
        public int user_id;
        public string user_name;

        public static bool online;




        private static IMobileServiceTable<Relations> relationsTable;
        private MobileServiceCollectionView<Relations> relation;

        PhoneNumberChooserTask phoneNumberChooserTask;



        // Constructor
        public MainPage()
        {
            InitializeComponent();
            /////
             
            InitializeSettings();
            // TODO:
            // Refresh to do item
            CheckNetworkAvailability();
            RefreshTodoItems();
            RefreshTheirCart();
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkChange);
              
            /////
            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded +=new RoutedEventHandler(MainPage_Loaded);
        }
        
        private void InitializeSettings()
        {
        }

        private void NetworkChange(object sender, NetworkNotificationEventArgs e)
        {
            CheckNetworkAvailability();
        }

        private async void CheckNetworkAvailability()
        {
            online = NetworkInterface.NetworkInterfaceType.ToString()!="None";
 
            Debug.WriteLine(online);
            if (online)
            {
                // Connect to Azure 

                //TODO Change link
                MobileService = new MobileServiceClient(
                     "https://shopappdata.azure-mobile.net/",
                       "dkwwuiuHYYQwbozjKaWRJYYpEiTjFt73"
                );
                itemTable = MobileService.GetTable<Items>();
                
                // Add buffer items in tasks
                buffer = XmlTaskService.GetTasks("buffer.xml");
                foreach (Items buffitem in buffer)
                {
                    try
                    {
                        await itemTable.InsertAsync(buffitem);
                        XmlTaskService.CreateTask(buffitem, "tasks.xml");
                    }
                    catch
                    {

                    }
                }
                XmlTaskService.DeleteAllTasks("buffer.xml");
                buffer.Clear();


                buffer = XmlTaskService.GetTasks("checked.xml");
                foreach (Items buffitem in buffer)
                {
                    Debug.WriteLine("Item is "); Debug.WriteLine(buffitem.Text) ;
                    checked_item = XmlTaskService.GetTasksByText(buffitem.Text, "tasks.xml");
                    checked_item.shared = true;
                    Debug.WriteLine("checked"); Debug.WriteLine(checked_item.Text);
                    try
                    {
                        await itemTable.UpdateAsync(checked_item);
                    }
                    catch
                    {
                    }
                }
                XmlTaskService.DeleteAllTasks("checked.xml");
                buffer.Clear();

                buffer = XmlTaskService.GetTasks("delete.xml");
                foreach (Items buffitem in buffer)
                {
                    checked_item = XmlTaskService.GetTasksByText(buffitem.Text, "tasks.xml");
                    try
                    {
                        await itemTable.DeleteAsync(checked_item);
                    }
                    catch
                    {
                    }
                }
                XmlTaskService.DeleteAllTasks("delete.xml");
                buffer.Clear();
            }
        }



        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CheckNetworkAvailability();
            // TODO : improve 
            if (settings.Contains("Pnumber"))
            {
                pnumber = (string)settings["Pnumber"];
                user_id = (int)settings["id"];
                user_name = (string)settings["name"];
                RefreshMyCart();
			//	RefreshTheirCart();
            }
            else
            {
                ((PhoneApplicationFrame)Application.Current.RootVisual).Navigate(new Uri("/Login.xaml", UriKind.Relative));
            }

            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }

            phoneNumberChooserTask = new PhoneNumberChooserTask();
            phoneNumberChooserTask.Completed += new EventHandler<PhoneNumberResult>(phoneNumberChooserTask_Completed);

        }

		private void AddNewItemToItemGrid(String itemLabel, int itemId, bool shared) {
			var newGrid = new Grid();
            newGrid.Tag = itemId.ToString();
            newGrid.Height = 70;
            newGrid.VerticalAlignment = VerticalAlignment.Top;
            newGrid.Width = 480;

            var linearGradient = new LinearGradientBrush();
            linearGradient.EndPoint = new Point(0.5, 1);
            linearGradient.StartPoint = new Point(0.5, 0);

            var gradientStop = new GradientStop();
            gradientStop.Color = Colors.Black;
            gradientStop.Offset = 0;
            linearGradient.GradientStops.Add(gradientStop);

            gradientStop = new GradientStop();
            var color = new Color();
            color.A = 255;
            color.R = 38;
            color.G = 38;
            color.B = 38;
            gradientStop.Color = color;
            gradientStop.Offset = 1;
            linearGradient.GradientStops.Add(gradientStop);

            newGrid.Background = linearGradient;

            var textBlock = new TextBlock();
            textBlock.Text = itemLabel;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Width = 310;
            textBlock.FontSize = 32;
            textBlock.Margin = new Thickness(0);
            textBlock.Padding = new Thickness(15, 9, 0, 0);

            var button = new Button();
            button.Tag = itemId.ToString();
            button.Margin = new Thickness(315, 0, 85, 0);
            button.BorderThickness = new Thickness(2);
            button.BorderBrush = null;
            button.Foreground = null;

            if (shared == false)
            {
                button.Tap += ShareItemWithFriends;
            }
            else
            {
                var solidBrush = new SolidColorBrush(Colors.Green);
                button.BorderBrush = solidBrush;
            }

            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(@"\Assets\Icons\appbar.cloud.upload.png", UriKind.Relative));
            imageBrush.Stretch = Stretch.None;

            button.Background = imageBrush;

            var button2 = new Button();
            button2.Tag = itemId.ToString();
            button2.Margin = new Thickness(400, 0, 0, 0);
            button2.BorderThickness = new Thickness(0);
            button2.BorderBrush = null;
            button2.Foreground = null;
            button2.Tap += DeleteItem;

            var imageBrush2 = new ImageBrush();
            imageBrush2.ImageSource = new BitmapImage(new Uri(@"\Assets\Icons\appbar.delete.png", UriKind.Relative));
            imageBrush2.Stretch = Stretch.None;

            button2.Background = imageBrush2;

            newGrid.Children.Add(textBlock);
            newGrid.Children.Add(button);
            newGrid.Children.Add(button2);

            ItemListBox.Items.Add(newGrid);
		}



		
		// Buy  wala
		private void AddNewItemToBuyGrid(String itemLabel, int itemId) {
			var newGrid = new Grid();
            newGrid.Tag = itemId.ToString();
            newGrid.Height = 70;
            newGrid.VerticalAlignment = VerticalAlignment.Top;
            newGrid.Width = 480;

            var linearGradient = new LinearGradientBrush();
            linearGradient.EndPoint = new Point(0.5, 1);
            linearGradient.StartPoint = new Point(0.5, 0);

            var gradientStop = new GradientStop();
            gradientStop.Color = Colors.Black;
            gradientStop.Offset = 0;
            linearGradient.GradientStops.Add(gradientStop);

            gradientStop = new GradientStop();
            var color = new Color();
            color.A = 255;
            color.R = 38;
            color.G = 38;
            color.B = 38;
            gradientStop.Color = color;
            gradientStop.Offset = 1;
            linearGradient.GradientStops.Add(gradientStop);

            newGrid.Background = linearGradient;

            var textBlock = new TextBlock();
            textBlock.Text = itemLabel;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Width = 310;
            textBlock.FontSize = 32;
            textBlock.Margin = new Thickness(0);
            textBlock.Padding = new Thickness(15, 9, 0, 0);
/*
            var button = new Button();
            button.Tag = itemId.ToString();
            button.Margin = new Thickness(315, 0, 85, 0);
            button.BorderThickness = new Thickness(0);
            button.BorderBrush = null;
            button.Foreground = null;
            button.Tap += ShareItemWithFriends;

            var imageBrush = new ImageBrush();
            imageBrush.ImageSource = new BitmapImage(new Uri(@"\Assets\Icons\appbar.cloud.upload.png", UriKind.Relative));
            imageBrush.Stretch = Stretch.None;

            button.Background = imageBrush;
            */
            var button2 = new Button();
            button2.Tag = itemId.ToString();
            button2.Margin = new Thickness(400, 0, 0, 0);
            button2.BorderThickness = new Thickness(0);
            button2.BorderBrush = null;
            button2.Foreground = null;
            button2.Tap += BuyItem;

            var imageBrush2 = new ImageBrush();
            imageBrush2.ImageSource = new BitmapImage(new Uri(@"\Assets\Icons\appbar.cart.png", UriKind.Relative));
            imageBrush2.Stretch = Stretch.None;

            button2.Background = imageBrush2;

            newGrid.Children.Add(textBlock);
//            newGrid.Children.Add(button);
            newGrid.Children.Add(button2);

            ItemBuyBox.Items.Add(newGrid);
		}

		
		
		
		
		

        private void AddFriendInformation(String friendName, int friendId, Dictionary<int, String> itemList)
        {
            var textBlock = new TextBlock();
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.Text = friendName;
            textBlock.Tag = friendId.ToString();
            textBlock.Height = 70;
            textBlock.Padding = new Thickness(20, 10, 0, 0);
            textBlock.FontSize = 32;
            textBlock.ManipulationStarted += ToggleFriendItemList;
            
            FriendsListPanel.Children.Add(textBlock);

            var stackPanel = new StackPanel();
            stackPanel.Visibility = Visibility.Collapsed;
            stackPanel.Tag = friendId.ToString();

            foreach (KeyValuePair <int, String> items in itemList) {
                textBlock = new TextBlock();
                textBlock.TextWrapping = TextWrapping.Wrap;
                textBlock.Height = 48;
                textBlock.FontSize = 24;
                textBlock.Padding = new Thickness(35, 5, 0, 0);
                textBlock.Text = items.Value;
                textBlock.Tag = items.Key;

                stackPanel.Children.Add(textBlock);
            }

            FriendsListPanel.Children.Add(stackPanel);
        }

        private void ToggleFriendItemList(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            var friendId = ((TextBlock)sender).Tag;

            foreach (var childNode in FriendsListPanel.Children)
            {
                if (childNode is StackPanel)
                {
                    var checkId = ((StackPanel)childNode).Tag;
                    if (checkId.ToString() == friendId.ToString())
                    {
                        if (((StackPanel)childNode).Visibility == Visibility.Collapsed)
                        {
                            ((StackPanel)childNode).Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ((StackPanel)childNode).Visibility = Visibility.Collapsed;
                        }
                    }
                }
            }
        }

        private async void AddNewItem(object sender, System.Windows.Input.GestureEventArgs e)
        {
            var itemName = NewItemTextBox.Text;
            NewItemTextBox.Text = "";

            local.Id = 0;        
            local.Text = itemName;
 //           local.shared = todoItem.shared;
            local.shared = false;
            local.User_Id = (int)settings["id"];

            if (online)
            {
                await itemTable.InsertAsync(local);
            }

            if (!online)
            {
                XmlTaskService.CreateTask(local, "buffer.xml");
            }
            else
            {
                Debug.WriteLine(XmlTaskService.CreateTask(local, "tasks.xml"));
            }
          //  int itemId = 0;// REMOVE THIS
            int itemId = local.Id;
            /*
             * <insert item to database>
             * var itemId = <GetFromDatabase>
             */

            AddNewItemToItemGrid(itemName, itemId, false);
        }

        private async void ShareItemWithFriends(object sender, System.Windows.Input.GestureEventArgs e)
        {

            string itemName="";
            

            foreach (var objects in ((Grid)((Button)sender).Parent).Children) {
                if (objects is TextBlock)
                {
                    itemName = ((TextBlock)objects).Text;
                }
            }

            var solidBrush = new SolidColorBrush(Colors.Green);
            ((Button)sender).BorderBrush = solidBrush;
            ((Button)sender).Tap -= ShareItemWithFriends;


            Items item = new Items();
            
            if (online)
            {
                item = XmlTaskService.GetTasksByText(itemName, "tasks.xml");
                XmlTaskService.DeleteTask(item.Text, "tasks.xml");

                item.shared = true;
                XmlTaskService.CreateTask(item, "tasks.xml");

                await itemTable.UpdateAsync(item);

                item = XmlTaskService.GetTasksByText(itemName, "tasks.xml");
            }
            else
            {
                item = XmlTaskService.GetTasksByText(itemName, "tasks.xml");
                item.Text = itemName;
                XmlTaskService.CreateTask(item, "checked.xml");
            }
        	// TODO: Use item id to set the share flag to true in the database
        }

        private async void DeleteItem(object sender, System.Windows.Input.GestureEventArgs e)
        {
            string itemName = "";
            Items item = new Items();
            
            foreach (var objects in ((Grid)((Button)sender).Parent).Children)
            {
                if (objects is TextBlock)
                {
                    itemName = ((TextBlock)objects).Text;
                }
            }

            var parent = (ListBox)((Grid)((Button)sender).Parent).Parent;
            parent.Items.Remove(((Grid)((Button)sender).Parent));

            if (online)
            {
                item = XmlTaskService.GetTasksByText(itemName, "tasks.xml");
                await itemTable.DeleteAsync(item);
                XmlTaskService.DeleteTask(itemName, "tasks.xml");
                RefreshTodoItems();
            }
            else
            {
                item = XmlTaskService.GetTasksByText(itemName, "tasks.xml");
                XmlTaskService.CreateTask(item, "delete.xml");
            }
            // TODO: Use item id to remove the item form database
        }

        // Function to buy item
        private async void BuyItem(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }


        private async void FillFriendsInformation(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // TODO: Add event handler implementation here.
            if (((Pivot)sender).SelectedIndex == 1)
            {
                MobileService = new MobileServiceClient(
                  "https://shopappdata.azure-mobile.net/",
                    "dkwwuiuHYYQwbozjKaWRJYYpEiTjFt73"
             );
      //          FriendsListPanel.Children.Clear();

                relationsTable = MobileService.GetTable<Relations>();

                
                var myList1 = await relationsTable.Where(itemabc => itemabc.Id > -1 ).ToListAsync();

                var myList2 = await relationsTable.Where(item2 => item2.Receiver_Id == user_id).ToListAsync();

                Debug.WriteLine(myList1.Count);
                Debug.WriteLine(myList2.Count);

                List<int> userList = new List<int>();
                //if (myList1 != null)
                {
                    foreach (var val in myList1)
                    {
                        Debug.WriteLine("1");
                        userList.Add(val.Receiver_Id);
                    }
                }

                if (myList2 != null)
                {

                    foreach (var val in myList2)
                    {
                        Debug.WriteLine("1");
                        userList.Add(val.Sender_Id);
                    }

                }
                itemTable = MobileService.GetTable<Items>();
                foreach (var user in userList)
                {
                   var items= await itemTable.Where(user2 => ((user2.User_Id==user) && (user2.shared==true))  ).ToListAsync();
                   var itemList = new Dictionary<int, String>();
                   foreach(var item in items)
                   {
                       itemList.Add(item.Id, item.Text);
                   }
                   userTable = MobileService.GetTable<Users>();
                   var list = await userTable.Where(user3 => (user3.Id == user)).ToListAsync();
                   
                   AddFriendInformation(list[0].Name, user, itemList);


                }

              
            }

        }

        private void RefreshTodoItems()
        {
            retreive = XmlTaskService.GetTasks("tasks.xml");
            retreive2 = XmlTaskService.GetTasks("buffer.xml");
            foreach (Items buffitem in retreive)
            {
                AddNewItemToItemGrid(buffitem.Text, buffitem.Id, buffitem.shared);
            }
            foreach (Items buffitem in retreive2)
            {
                AddNewItemToItemGrid(buffitem.Text, buffitem.Id, buffitem.shared);
            }
            
        }

        private void AddNewFriend(object sender, System.EventArgs e)
        {
            phoneNumberChooserTask.Show();
        }


        private async void phoneNumberChooserTask_Completed(object sender, PhoneNumberResult e)

        {
            if (online)
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    MessageBox.Show("Adding " + e.DisplayName + " with phone no. " + e.PhoneNumber + " as friend. Press ok to continue");


                    var userTable = MobileService.GetTable<Users>();
                    var list = await userTable.Where(user2 => user2.Phone_no == e.PhoneNumber).ToListAsync();

                    Relations friend = new Relations();
                    friend.Receiver_Id= user_id;
                    try
                    {
                        friend.Sender_Id = list[0].Id;
                        friend.Status = 3;

                        relationsTable = MobileService.GetTable<Relations>();

                        await relationsTable.InsertAsync(friend);
                    }
                    catch { }


                }

            }
        }


		private void RefreshMyCart()
		{
            retreive = XmlTaskService.GetTasks("tasks.xml");
            retreive2 = XmlTaskService.GetTasks("buffer.xml");
            foreach (Items buffitem in retreive)
            {
                AddNewItemToBuyGrid(buffitem.Text, buffitem.Id);
            }
            foreach (Items buffitem in retreive2)
            {
                AddNewItemToBuyGrid(buffitem.Text, buffitem.Id);
            }
			// Drawing Rectangle

            var Rectangle = new Rectangle();
            Rectangle.Height = 10;
            Rectangle.Width = 480;
            
            Rectangle.Fill = new SolidColorBrush(System.Windows.Media.Colors.LightGray);

            ItemBuyBox.Items.Add(Rectangle);

        }
		private async void RefreshTheirCart()
		{
            if(online)
            {
                Debug.WriteLine("Refresh their");

                
                 relationsTable = MobileService.GetTable<Relations>();
                var myList1 = await relationsTable.Where(item2 => (( item2.Sender_Id== user_id) && (item2.Status == 3))).ToListAsync();
       
                var myList2 = await relationsTable.Where(item2 => ((item2.Receiver_Id == user_id) && (item2.Status == 3))).ToListAsync();

                List<int> userList= new List<int>();
                foreach (var val in myList1)
                {
                    Debug.WriteLine("Refresh their");
                    userList.Add(val.Receiver_Id);
                }
                foreach (var val in myList2)
                {
                    Debug.WriteLine("Refresh their2");
                    userList.Add(val.Sender_Id);
                }

                itemTable = MobileService.GetTable<Items>();

                List<Items> final = new List<Items>();
                var tempList= new List<Items>();
                foreach(var val in userList)
                {
                    Debug.WriteLine("Refresh their3");
                    tempList = await itemTable.Where(item2 => item2.User_Id == val).ToListAsync();
                    foreach(var val2 in tempList)
                    {
                        final.Add(val2);
                    }
                }
                foreach (Items buffitem in final)
                {
                    AddNewItemToBuyGrid(buffitem.Text, buffitem.Id);
                }
			
             }

		}

		private async void Button_Tap_1(object sender, System.Windows.Input.GestureEventArgs e)
		{
            // Create an instance of SpeechRecognizerUI.
            var recoWithUI = new SpeechRecognizerUI();

            // Start recognition (load the dictation grammar by default).
            SpeechRecognitionUIResult recoResult = await recoWithUI.RecognizeWithUIAsync();

            // Do something with the recognition result.
            MessageBox.Show(string.Format("You said {0}.", recoResult.RecognitionResult.Text));
            NewItemTextBox.Text = recoResult.RecognitionResult.Text;
        }

    }
}