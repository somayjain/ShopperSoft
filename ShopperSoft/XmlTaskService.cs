using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using ShopperSoft.Model;
//using TaskMaster.Abstract;
//using TaskMaster.Models;
//using TaskMaster.ViewModels;

namespace ShopperSoft
{
    public class XmlTaskService
    {
        //       public const string XmlFile = "tasks.xml";

        public static ObservableCollection<Items> GetTasks(string XmlFile)
        {
            ObservableCollection<Items> tasks_ret = new ObservableCollection<Items>();
            var tasks = new List<Items>();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(XmlFile))
                {
                    //store.DeleteFile(XmlFile);
                    //XDocument doc = XDocument.Load(store.OpenFile(XmlFile, FileMode.Open));
                    using (var sr = new StreamReader(new IsolatedStorageFileStream(XmlFile, FileMode.Open, store)))
                    {
                        XDocument doc = XDocument.Load(sr);
                        tasks = (from d in doc.Descendants("task")
                                 select new Items
                                 {
                                     Id = (int)d.Element("Id"),
                                     Text = (string)d.Element("Text"),
                                     shared = (bool)d.Element("shared"),
                                     User_Id = (int)d.Element("User_Id"),
                                     complete = (bool)d.Element("complete")
                                 }).ToList<Items>();
                    }
                    System.Diagnostics.Debug.WriteLine("get tasks");
                    foreach (Items item in tasks)
                    {
                        System.Diagnostics.Debug.WriteLine(item.Id);
                        tasks_ret.Add(item);
                    }

                }
            }
            return tasks_ret;
        }

        public static Items GetTasksById(int id, string XmlFile)
        {
            Items item = new Items();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(XmlFile))
                {
                    //store.DeleteFile(XmlFile);
                    //XDocument doc = XDocument.Load(store.OpenFile(XmlFile, FileMode.Open));
                    using (var sr = new StreamReader(new IsolatedStorageFileStream(XmlFile, FileMode.Open, store)))
                    {
                        XDocument doc = XDocument.Load(sr);

                        foreach (XElement ele in doc.Descendants("task"))
                        {

                            if (ele != null && int.Parse(ele.Element("Id").Value) == id)
                            {
                                item.Id = int.Parse(ele.Element("Id").Value);
                                item.Text = (string)ele.Element("Text").Value;
                                item.User_Id = int.Parse(ele.Element("User_Id").Value);
                                item.shared = ele.Element("shared").Value == "true" ? true : false;
                                item.complete = ele.Element("complete").Value == "true" ? true : false;
                                break;
                            }
                        }

                    }


                }
            }
            return item;
        }

        public static Items GetTasksByText(string text, string XmlFile)
        {
            Items item = new Items();

            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(XmlFile))
                {
                    //store.DeleteFile(XmlFile);
                    //XDocument doc = XDocument.Load(store.OpenFile(XmlFile, FileMode.Open));
                    using (var sr = new StreamReader(new IsolatedStorageFileStream(XmlFile, FileMode.Open, store)))
                    {
                        XDocument doc = XDocument.Load(sr);
                        foreach (XElement ele in doc.Descendants("task"))
                        {

                            if (ele != null && (ele.Element("Text").Value) == text)
                            {
                                item.Id = int.Parse(ele.Element("Id").Value);
                                item.Text = (string)ele.Element("Text").Value;
                                item.User_Id = int.Parse(ele.Element("User_Id").Value);
                                item.shared = ele.Element("shared").Value == "true" ? true : false;
                                item.complete = ele.Element("complete").Value == "true" ? true : false;
                                break;
                            }
                        }
                    }
                }
            }
            return item;
        }

        public static void UpdateId(int id, Items item, string XmlFile)
        {


            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(XmlFile))
                {
                    //store.DeleteFile(XmlFile);
                    //XDocument doc = XDocument.Load(store.OpenFile(XmlFile, FileMode.Open));
                    using (var sr = new StreamReader(new IsolatedStorageFileStream(XmlFile, FileMode.Open, store)))
                    {
                        XDocument doc = XDocument.Load(sr);

                        foreach (var ele in doc.Descendants())
                        {
                            if (ele != null && (ele.Element("task").Element("Text").Value) == item.Text)
                            {
                                ele.Element("task").Element("Id").SetValue(id.ToString());
                                break;
                            }

                        }
                    }


                }
            }


        }

        public static bool DeleteAllTasks(string XmlFile)
        {
            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    store.DeleteFile(XmlFile);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public static bool CreateTask(Items t, string XmlFile)
        {

            var doc = ReadXml(XmlFile);
            try
            {
                if (doc.Element("tasks") != null)
                {
                    //this is an append
                    doc.Element("tasks").Add(
                        new XElement("task",
                                     new XElement("Text", t.Text),
                                     new XElement("shared", t.shared),
                                     new XElement("Id", t.Id),
                                     new XElement("User_Id", t.User_Id),
                                     new XElement("complete", "false")
                            ));
                    WriteXml(doc, XmlFile);
                }
                else
                {
                    doc.Add(
                           new XElement("tasks",
                                   new XElement("task",
                                         new XElement("Text", t.Text),
                                         new XElement("shared", "false"),
                                         new XElement("Id", t.Id),
                                         new XElement("User_Id", t.User_Id),
                                         new XElement("complete", "false")
                                   )
                      ));
                    WriteXml(doc, XmlFile);
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        private static XDocument ReadXml(string XmlFile)
        {
            var doc = new XDocument();
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    if (store.FileExists(XmlFile))
                    {
                        using (var sr = new StreamReader(new IsolatedStorageFileStream(XmlFile, FileMode.OpenOrCreate, store)))
                        {
                            doc = XDocument.Load(sr);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return doc;
        }

        private static bool WriteXml(XDocument document, string XmlFile)
        {
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try
                {
                    using (var sw = new StreamWriter(new IsolatedStorageFileStream(XmlFile, FileMode.Create, store)))
                    {
                        sw.Write(document.ToString());
                    }
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool MarkTaskComplete(string id, string XmlFile)
        {
            XDocument doc = ReadXml(XmlFile);

            foreach (var ele in doc.Descendants())
            {
                if (ele != null && (string)ele.Attribute("Id") == id)
                {
                    ele.SetElementValue("shared", "true");
                    break;
                }
            }
            return WriteXml(doc, XmlFile);
        }

        public static bool DeleteTask(string text, string XmlFile)
        {
            XDocument doc = ReadXml(XmlFile);

            foreach (XElement ele in doc.Descendants("task"))
            {

                if (ele != null && (ele.Element("Text").Value) == text)
                {
                    ele.Remove();
                    break;
                }
            }

            
            return WriteXml(doc, XmlFile);
        }
    }
}