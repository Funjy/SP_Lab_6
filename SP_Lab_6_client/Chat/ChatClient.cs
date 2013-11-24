using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{
    //public delegate void ReceviedMessage(string sender, string message);
    public delegate void ReceviedMessage(ClientMessage mes);
    public delegate void GotNames(object sender, List<string> names);

    public class ChatClient// : ISendChatServiceCallback
    {

        #region eventsRegion
        public event ReceviedMessage ReceiveMsg;

        //protected virtual void OnReceiveMsg(string sender, string message)
        //{
        //    ReceviedMessage handler = ReceiveMsg;
        //    if (handler != null) handler(sender, message);
        //}

        protected virtual void OnReceiveMsg(ClientMessage mes)
        {
            ReceviedMessage handler = ReceiveMsg;
            if (handler != null) handler(mes);
        }

        public event GotNames NewNames;

        protected virtual void OnNewNames(List<string> names)
        {
            GotNames handler = NewNames;
            if (handler != null) handler(this, names);
        }
        #endregion

        //---------------------------------------------------------------

        public ChatClient(string name)
        {
            Name = name;
        }

        public string Name { get; set; }
        //InstanceContext _inst;
        //SendChatServiceClient _chatClient;
        
        public void Start()
        {
            //_inst = new InstanceContext(this);

            //_chatClient = new SendChatServiceClient(_inst);
            //_chatClient.Endpoint.Address = new EndpointAddress("net.tcp://" + AliveInfo.ClientHelper.GetServerIp + ":38003/Chat/ChatService");
            //try
            //{
            //    _chatClient.Start(Name);
            //}
            //catch (Exception)
            //{
                
            //    throw;
            //}
            
        }

        public void SendMessage(byte[] msg, string receiver)
        {
            //_chatClient.SendMessage(msg, Name, receiver);
        }

        public void Stop()
        {
            //_chatClient.Stop(Name);
        }

        public void ReceiveMessage(string msg, string receiver)
        {
            //if (ReceiveMsg != null)
            //    ReceiveMsg(receiver, msg);
        }

        public void SendNames(string[] names)
        {
            //if (NewNames != null)
            //    NewNames(this, names.ToList());
        }
    }
}
