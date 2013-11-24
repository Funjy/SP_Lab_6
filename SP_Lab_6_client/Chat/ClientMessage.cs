//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;

//namespace SP_Lab_6_client.Chat
//{
//    public class ClientMessage
//    {
//        public string Sender { get; set; }
//        public string Message { get; set; }
//        public DateTime TimeStamp { get; set; }
//        public MessageSide Side { get; set; }
//        public bool IsPrivate { get; set; }
//        public List<AdditionalData> MoreInfo { get; set; }

//        public ClientMessage()
//        {
//            Side = MessageSide.Me;
//        }

//        public static ClientMessage DeserializeMessage(string jsonString)
//        {
//            //return JsonConvert.DeserializeObject<ClientMessage>(jsonString);
//        }

//        public string Serialize()
//        {
//            //return JsonConvert.SerializeObject(this);
//        }
//    }

//    public enum MessageSide
//    {
//        Me,
//        You
//    }

//    public class MessageCollection : ObservableCollection<ClientMessage>
//    {
//    }

//    public class AdditionalData
//    {
//        public string Parameter { get; set; }
//        public string Value { get; set; }
//    }
//}
