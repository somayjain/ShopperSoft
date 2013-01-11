using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using saaman.Resources;
using System.Collections.ObjectModel;


using Microsoft.WindowsAzure.MobileServices;
using System.Diagnostics;

using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Tasks;

namespace saaman
{
    public class TodoItem
    {
        public int Id { get; set; }

        [DataMember(Name = "text")]
        public string Text { get; set; }

        [DataMember(Name = "complete")]
        public bool Complete { get; set; }
    }

    public partial class MainPage : PhoneApplicationPage
    {
        // MobileServiceCollectionView implements ICollectionView (useful for databinding to lists) and 
        // is integrated with your Mobile Service to make it easy to bind your data to the ListView
 //       private ObservableCollection<TodoItem> items2 = new ObservableCollection<TodoItem>();
        private ObservableCollection<TodoItem> buffer = new ObservableCollection<TodoItem>();
        private MobileServiceCollectionView<TodoItem> items;
        public ObservableCollection<TodoItem> retreive = new ObservableCollection<TodoItem>();
       
        public static TodoItem local; 
        
        public static MobileServiceClient MobileService;
        private static IMobileServiceTable<TodoItem> todoTable;

        static bool online = false;
 
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            RefreshTodoItems();
            CheckNetworkAvailability();
            DeviceNetworkInformation.NetworkAvailabilityChanged += new EventHandler<NetworkNotificationEventArgs>(NetworkChange);
             
        }
        private void NetworkChange(object sender, NetworkNotificationEventArgs e)
        {
            CheckNetworkAvailability();
        }
        private async void CheckNetworkAvailability()
        {
            online = NetworkInterface.NetworkInterfaceType.ToString()!="None";
       //     SetNetworkIndication();
            Debug.WriteLine(online);
            if (online)
            {
                MobileService = new MobileServiceClient(
                     "https://saaman.azure-mobile.net/", 
                       "BPWiwYHRGotSjuciwiTyBSHwDnZNvj71"
                );
                todoTable = MobileService.GetTable<TodoItem>();
            //    Debug.WriteLine(todoTable);
            //    XmlTaskService.XmlFile = "buffer.xml";

                buffer = XmlTaskService.GetTasks("buffer.xml");
                
                foreach (TodoItem buffitem in buffer)
                {
                    Debug.WriteLine(buffitem.Id);
                    try
                    {
                        await todoTable.InsertAsync(buffitem);
                    }
                    catch
                    {

                    }
                }
                XmlTaskService.DeleteAllTasks("buffer.xml");
                buffer.Clear();
                Debug.WriteLine(buffer.Count);
            }
        }

        private async void InsertTodoItem(TodoItem todoItem)
        {
            // This code inserts a new TodoItem into the database. When the operation completes
            // and Mobile Services has assigned an Id, the item is added to the CollectionView
            Debug.WriteLine(todoItem.Id);
            local = new TodoItem();
//            local.Id = retreive.Count + 1;
            local.Id = todoItem.Id;        
            local.Text = todoItem.Text;
            local.Complete = todoItem.Complete;

  //          items2.Add(local);

            if (!online)
            {
    //            XmlTaskService.XmlFile = "buffer.xml";
                XmlTaskService.CreateTask(local, "buffer.xml");
            }
 //           XmlTaskService.XmlFile = "tasks.xml";
            XmlTaskService.CreateTask(local, "tasks.xml");
            

            RefreshTodoItems();
    
            if(online)
            {
                await todoTable.InsertAsync(todoItem);
           //     items.Add(todoItem);
            }
            
        }

        private void RefreshTodoItems()
        {
            // This code refreshes the entries in the list view be querying the TodoItems table.
            // The query excludes completed TodoItems
//            items = todoTable
//               .Where(todoItem => todoItem.Complete == false)
//               .ToCollectionView();
            retreive = XmlTaskService.GetTasks("tasks.xml");
            ListItems.ItemsSource = retreive;
        }

        private async void UpdateCheckedTodoItem(TodoItem item)
        {
            // This code takes a freshly completed TodoItem and updates the database. When the MobileService 
            // responds, the item is removed from the list 
            await todoTable.UpdateAsync(item);
            items.Remove(item);
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshTodoItems();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            var todoItem = new TodoItem { Text = TodoInput.Text };
            InsertTodoItem(todoItem);
        }

        private void CheckBoxComplete_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            TodoItem item = cb.DataContext as TodoItem;
            item.Complete = true;
            UpdateCheckedTodoItem(item);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            RefreshTodoItems();
        }
    }
}