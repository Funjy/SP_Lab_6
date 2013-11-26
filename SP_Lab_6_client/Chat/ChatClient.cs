using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{
    //public delegate void ReceviedMessage(string sender, string message);
    public delegate void ReceviedMessage(ClientMessage mes);
    public delegate void GotNames(object sender, List<UserInfo> names);

    //public delegate void VoidDelegate();

    public class ChatClient// : ISendChatServiceCallback
    {

        #region eventsRegion
        public event ReceviedMessage ReceiveMsg;

        protected virtual void OnReceiveMsg(ClientMessage mes)
        {
            ReceviedMessage handler = ReceiveMsg;
            if (handler != null) handler(mes);
        }

        public event GotNames NewNames;

        protected virtual void OnNewNames(List<UserInfo> names)
        {
            GotNames handler = NewNames;
            if (handler != null) handler(this, names);
        }

        public event VoidDelegate ServerDisconnect;

        protected virtual void OnServerDisconnect()
        {
            VoidDelegate handler = ServerDisconnect;
            if (handler != null) handler();
        }

        #endregion

        public class StateObject
        {
            // Client socket.
            public Socket WorkSocket = null;
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] Buffer = new byte[BufferSize];
        }

        //---------------------------------------------------------------

        private Socket _soc;
        private const int ServerPort = 11337;
        //private const int BufLength = 1024;
        //private byte[] _buffer = new byte[BufLength];
        //private const int Port = 11338;

        public ChatClient()
        {
        }

        public ChatClient(string name, IPAddress ip)
        {
            Name = name;
            Ip = ip;
        }

        public string Name { get; set; }
        public IPAddress Ip { get; set; }
        //InstanceContext _inst;
        //SendChatServiceClient _chatClient;
        
        public void Start()
        {
            if (_soc != null)
            {
                if(_soc.Connected)
                    try
                    {
                        _soc.Disconnect(true);
                    }
                    catch (SocketException) { }
                try
                {
                    _soc.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) { }
                _soc.Close();
            }

            _soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 1200,
                    SendTimeout = 1200
                };
            //IPAddress ipAddress = IPAddress.Any;
            var remoteEp = new IPEndPoint(Ip, ServerPort);
            _soc.Connect(remoteEp);

            var cm = new ClientMessage
                {
                    Sender = Name,
                    MesType = MessageType.Start
                };

            var so = new StateObject {WorkSocket = _soc};
            var ep = _soc.RemoteEndPoint;
            _soc.BeginReceiveFrom(so.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref ep, ReceiveCallback, so);

            SendMessage(cm);

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

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;

            var soc = state.WorkSocket;

            int bytesRead;

            try
            {
                bytesRead = soc.EndReceive(ar);
            }
            catch (Exception)
            {
                OnServerDisconnect();
                return;
            }

            if (bytesRead > 0)
            {
                var cm = ClientMessage.DeserializeMessage(state.Buffer, bytesRead);
                switch (cm.MesType)
                {
                    case MessageType.Text:
                        OnReceiveMsg(cm);
                        break;
                    case MessageType.UserList:
                        var users = MySerializer.DeserializeFromBase64String<List<UserInfo>>(cm.Message, true);
                        OnNewNames(users);
                        break;
                    case MessageType.File:
                        break;
                    case MessageType.System:
                        if(cm.Message == SystemMessageTypes.USER_EXIST)
                            OnReceiveMsg(cm);
                        else if(cm.Message == SystemMessageTypes.CHECK_HERE)
                            SendMessage(new ClientMessage
                                {
                                    MesType = MessageType.System,
                                    Message = SystemMessageTypes.CHECK_HERE
                                });
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            try
            {
                var ep = soc.RemoteEndPoint;
                state.Buffer = new byte[StateObject.BufferSize];
                try
                {
                    _soc.BeginReceiveFrom(state.Buffer, 0, StateObject.BufferSize, SocketFlags.None, ref ep, ReceiveCallback, state);
                }
                catch (SocketException)
                {
                    OnServerDisconnect();
                }
                
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            

        }

        public void SendMessage(ClientMessage mes)
        {
            if(_soc == null)
                return;
            try
            {
                _soc.Send(mes.Serialize());
            }
            catch (SocketException)
            {
                OnServerDisconnect();
            }
            
            //_chatClient.SendMessage(msg, Name, receiver);
        }

        public void Stop()
        {
            if(_soc == null)
                return;
            var cm = new ClientMessage {MesType = MessageType.Stop, Sender = Name};
            SendMessage(cm);
            try
            {
                _soc.Shutdown(SocketShutdown.Both);
            }
            catch(SocketException) { }
            _soc.Close();
            _soc = null;
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
