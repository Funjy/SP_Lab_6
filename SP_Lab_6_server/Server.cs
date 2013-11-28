using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ClientServerInterface;

namespace SP_Lab_6_server
{
    internal class Server
    {
        //public delegate void UsersListChangedDelegate(List<ConnectionInfo> users);

        //public event UsersListChangedDelegate UsersListChanged;

        //protected virtual void OnUsersListChanged(List<ConnectionInfo> users)
        //{
        //    UsersListChangedDelegate handler = UsersListChanged;
        //    if (handler != null) handler(users);
        //}

        public delegate void VoidDelegate();

        public event VoidDelegate UsersListUpdated;

        protected virtual void OnUsersListUpdated()
        {
            VoidDelegate handler = UsersListUpdated;
            if (handler != null) handler();
        }

        public delegate void LogRecordDelegate(LogRecord record);

        public event LogRecordDelegate NewLogRecord;

        protected virtual void OnNewLogRecord(LogRecord newRecord)
        {
            LogRecordDelegate handler = NewLogRecord;
            if (handler != null) handler(newRecord);
        }

        public delegate void ServerErrorDelegate(ExInfo ex);

        public event ServerErrorDelegate ServerError;

        protected virtual void OnServerError(ExInfo ex)
        {
            ServerErrorDelegate handler = ServerError;
            if (handler != null) handler(ex);
        }

        private Thread _dispThread;

        //port
        private const int LocalPort = 11337;

        public List<ConnectionInfo> ConnectionInfos { get; set; }
        
        public bool IsStarted { get; private set; }

        private Socket _listener;
        private CancellationTokenSource _tokenSource;
        //private Mutex _mut;
        private Task _task;

        public Server()
        {
            ConnectionInfos = new List<ConnectionInfo>();
            _dispThread = Thread.CurrentThread;
            //_mut = new Mutex();
            //_tokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            if (IsStarted)
                return;
            _tokenSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(StartServer, _tokenSource.Token);
            IsStarted = true;
            try
            {
                _task.Wait();
            }
            catch (Exception)
            {
                
                throw;
            }
            
            //_task.Start();
        }

        private void StartServer()
        {
            //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Any;
            var localEndPoint = new IPEndPoint(ipAddress, LocalPort);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(50);
                _listener.BeginAccept(AcceptCallback, _listener);

                IsStarted = true;

            }
            catch (Exception e)
            {
                //throw e;
                var me = new ExInfo
                    {
                        AdditionalInfo = "Ошибка при запуске сервера.",
                        Exception = e,
                        StopServer = true
                    };
                OnServerError(me);
            }

        }

        public void Stop()
        {
            if (!IsStarted)
                return;
            
            _tokenSource.Cancel();
            try
            {
                _task.Wait(1000, _tokenSource.Token);
            }
            catch (Exception)
            {
                throw;
            }
            try
            {
                _listener.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException)
            {
            }
            finally
            {
                _listener.Close();
            }
            foreach (var info in ConnectionInfos)
            {
                try
                {
                    info.Socket.Disconnect(true);
                }
                catch (SocketException) { }
                try
                {
                    info.Socket.Shutdown(SocketShutdown.Both);
                }
                catch (SocketException) { }
                info.Socket.Close();
            }
            ConnectionInfos.Clear();
            OnUsersListUpdated();
            _tokenSource.Dispose();

            IsStarted = false;

        }

        void AddConnection(ConnectionInfo connection)
        {
            ConnectionInfos.Add(connection);
            SendNames();
        }

        void CloseCnnection(ConnectionInfo connection)
        {            
            try
            {
                connection.Socket.Disconnect(true);
                     
            }
            catch (SocketException) { }

            try
            {
                connection.Socket.Shutdown(SocketShutdown.Both);
            }
            catch (SocketException) { }

            connection.Socket.Close();
            if (ConnectionInfos.Contains(connection))
            {
                ConnectionInfos.Remove(connection);
                SendNames();
            }
        }

        private void AcceptCallback(IAsyncResult ar)
        {
            var listener = (Socket)ar.AsyncState;
            Socket sock;
            try
            {
                sock = listener.EndAccept(ar);
            }
            catch (SocketException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                /*OnServerError(new ExInfo
                    {
                        StopServer = true,
                        Exception = ex
                    });*/
                return;
            }
            

            var userConnection = new ConnectionInfo { Socket = sock };
            sock.BeginReceive(userConnection.Buffer, 0, ConnectionInfo.BufferSize, 0,
                              ReadCallback, userConnection);
            
            try
            {
                _listener.BeginAccept(AcceptCallback, _listener);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            var connection = (ConnectionInfo)ar.AsyncState;
            var sock = connection.Socket;

            int bytesRead;

            try
            {
                bytesRead = sock.EndReceive(ar);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException)
            {
                OnNewLogRecord(new LogRecord()
                    {
                        UserName = connection.UserName,
                        Event = "Пользователь неожиданно отключился",
                        Date = DateTime.Now
                    });
                CloseCnnection(connection);
                return;
            }

            bool rec = true;

            if (bytesRead > 0)
            {
                var cm = ClientMessage.DeserializeMessage(connection.Buffer, bytesRead);
                
                switch (cm.MesType)
                {
                    case MessageType.Text:
                        DoMessage(connection, cm);
                        break;
                    case MessageType.Start:
                        DoStart(connection, cm.Sender);
                        break;
                    case MessageType.Stop:
                        rec = false;
                        DoStop(connection, cm.Sender);
                        break;
                    case MessageType.File:
                        DoFile(connection, cm);
                        break;
                    default:
                        OnNewLogRecord(new LogRecord
                        {
                            Date = DateTime.Now,
                            UserName = cm.Sender,
                            Event = "Неверный тип сообщения: " + cm.MesType.ToString()
                        });
                        break;
                }
            }

            try
            {
                if(rec)
                    sock.BeginReceive(connection.Buffer, 0, ConnectionInfo.BufferSize, 0,
                                  ReadCallback, connection);
                else
                {
                    CloseCnnection(connection);
                }
            }
            catch (SocketException)
            {
                CloseCnnection(connection);
            }
            catch (ObjectDisposedException) { }
        }

        void DoFile(ConnectionInfo connection, ClientMessage cm)
        {
            var receiver = FindConnectionByUserName(cm.Receiver);

            //Сказать, что пользователь офлайн
            if (receiver == null)
            {
                var users = CreateUserList();
                var mes = UserListToMessage(users);
                SendMessage(connection, mes);
                return;
            }
            SendMessage(receiver, cm);
        }

        void DoMessage(ConnectionInfo connection, ClientMessage cm)
        {
            if (cm.IsPrivate)
            {
                var receiver = FindConnectionByUserName(cm.Receiver);

                //Сказать, что пользователь офлайн
                if (receiver == null)
                {
                    var users = CreateUserList();
                    var mes = UserListToMessage(users);
                    SendMessage(connection, mes);
                    return;
                }

                SendMessage(receiver, cm);
            }
            else
            {
                foreach (var info in ConnectionInfos)
                {
                    /*if(info.Equals(connection))
                        continue;*/
                    SendMessage(info, cm);
                }
            }
        }

        void DoStart(ConnectionInfo connection, string name)
        {
            if(string.IsNullOrEmpty(name))
                return;
            var fuser = ConnectionInfos.FirstOrDefault(u => u.UserName == name);
            if (fuser == null)
            {
                connection.UserName = name;
                AddConnection(connection);
                //ConnectionInfos.Add(connection);
                //SendNames();
                OnNewLogRecord(new LogRecord
                    {
                        UserName = name,
                        Event = "Пользователь вошел в систему",
                        Date = DateTime.Now
                    });
                /*var users = new List<UserInfo>(ConnectionInfos.Count);
                foreach (var info in ConnectionInfos)
                {
                    users.Add(new UserInfo
                        {
                            UserName = info.UserName,
                            IpAddress = (info.Socket.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes()
                        });
                }
                var mesData = MySerializer.SerializeSomethingToBase64String(users);
                var mes = new ClientMessage {MesType = MessageType.UserList, Message = mesData};
                SendMessage(connection, MySerializer.SerializeSomethingToBytes(mes));*/
            }
            else
            {
                //Сделать чтоб не пускало

                var cm = new ClientMessage
                    {
                        MesType = MessageType.System,
                        Message = SystemMessageTypes.USER_EXIST
                    };

                //var users = CreateUserList();
                //var mes = UserListToMessage(users);
                SendMessage(connection, cm);
            }
        }
        
        void DoStop(ConnectionInfo connection, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;
            /*var fuser = ConnectionInfos.FirstOrDefault(u => u.UserName == name);
            if (fuser == null)
                return;*/
            if(!ConnectionInfos.Contains(connection))
                return;
            //var removed = ConnectionInfos.Remove(connection);
            CloseCnnection(connection);
            OnNewLogRecord(new LogRecord
                {
                    UserName = name,
                    Event = "Пользователь вышел из системы",
                    Date = DateTime.Now
                });
            //SendNames();
        }

        void SendNames()
        {
            //var users = new List<UserInfo>(ConnectionInfos.Count);
            //foreach (var info in ConnectionInfos)
            //{
            //    users.Add(new UserInfo
            //    {
            //        UserName = info.UserName,
            //        IpAddress = (info.Socket.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes()
            //    });
            //}
            var users = CreateUserList();
            /*var mesData = MySerializer.SerializeSomethingToBase64String(users);
            var mes = new ClientMessage {MesType = MessageType.UserList, Message = mesData};*/
            var mes = UserListToMessage(users);
            foreach (var info in ConnectionInfos)
            {
                SendMessage(info, mes);
            }
            OnUsersListUpdated();
        }

        ConnectionInfo FindConnectionByUserName(string name)
        {
            return ConnectionInfos.FirstOrDefault(c => c.UserName == name);
        }

        ClientMessage UserListToMessage(List<UserInfo> uList)
        {
            var mesData = MySerializer.SerializeSomethingToBase64String(uList);
            var mes = new ClientMessage { MesType = MessageType.UserList, Message = mesData };
            return mes;
        }

        List<UserInfo> CreateUserList()
        {
            var users = new List<UserInfo>(ConnectionInfos.Count);
            foreach (var info in ConnectionInfos)
            {
                users.Add(new UserInfo
                {
                    UserName = info.UserName,
                    IpAddress = (info.Socket.RemoteEndPoint as IPEndPoint).Address.GetAddressBytes()
                });
            }
            return users;
        }

        void SendMessage(ConnectionInfo connection, ClientMessage mes)
        {
            try
            {
                connection.Socket.Send(mes.Serialize());
            }
            catch (SocketException)
            {
                CloseCnnection(connection);
            }
            catch (ObjectDisposedException) { }
        }

    }

    class ExInfo
    {
        public Exception Exception { get; set; }
        public bool StopServer { get; set; }
        public string AdditionalInfo { get; set; }
    }

}
