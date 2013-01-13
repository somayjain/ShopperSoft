using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

using System.Collections.ObjectModel;
using Microsoft.WindowsAzure.MobileServices;

namespace ShopperSoft.Model
{

    public class Users
    {
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "phone_no")]
        public string Phone_no { get; set; }

        [DataMember(Name = "uri")]
        public string uri { get; set; }
    }
    public class Items
    {
        public int Id { get; set; }

        [DataMember(Name = "user_id")]
        public int User_Id { get; set; }

        [DataMember(Name = "name")]
        public string Text { get; set; }

        [DataMember(Name = "complete")]
        public bool complete { get; set; }

        [DataMember(Name = "shared")]
        public bool shared { get; set; }

    }
    public class Relations
    {
        public int Id { get; set; }

        [DataMember(Name = "sender_id")]
        public int Sender_Id { get; set; }

        [DataMember(Name = "receiver_id")]
        public int Receiver_Id { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

    }
    public class Notify
    {
        public int Id { get; set; }

        [DataMember(Name = "user_id")]
        public int User_Id { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }


    }
}
