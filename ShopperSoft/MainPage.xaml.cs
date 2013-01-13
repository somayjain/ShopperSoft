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

        private static IMobileServiceTable<Relations> relationsTable;
        public static Relations relation = new Relations();
       
        private static IMobileServiceTable<Users> userTable;
        public static Users user = new Users();

        public static IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

        public string pnumber;
        public int user_id;
        public string user_name;

        public static bool online;

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

        private void FillFriendsInformation(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
        	// TODO: Add event handler implementation here.
            if (((Pivot)sender).SelectedIndex == 1)
            {
                FriendsListPanel.Children.Clear();

                var itemList = new Dictionary<int, String>();
                itemList.Add(1, "Item 1");
                itemList.Add(2, "Item 2");
                itemList.Add(3, "Item 3");
                itemList.Add(4, "Item 4");
                itemList.Add(5, "Item 5");

                AddFriendInformation("Akshet", 1, itemList);
                AddFriendInformation("Akshet2", 2, itemList);
                AddFriendInformation("Akshet3", 3, itemList);
                AddFriendInformation("Akshet4", 4, itemList);
                AddFriendInformation("Akshet5", 5, itemList);
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

        private void phoneNumberChooserTask_Completed(object sender, PhoneNumberResult e)
        {
            if (online)
            {
                if (e.TaskResult == TaskResult.OK)
                {
                    MessageBox.Show("Adding " + e.DisplayName + " with phone no. " + e.PhoneNumber + " as friend. Press ok to continue");

                    userTable = MobileService.GetTable<Users>();
                    foreach (var item in userTable.Where(user2 => user2.Phone_no == e.PhoneNumber).ToCollectionView())
                    {
                        System.Diagnostics.Debug.WriteLine(item);
                    }
                }

            }
        }
    }
}