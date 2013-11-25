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


        //port
        private const int LocalPort = 11337;

        public ObservableCollection<ConnectionInfo> ConnectionInfos { get; set; }
        
        public bool IsStarted { get; private set; }

        private Socket _listener;
        private readonly CancellationTokenSource _tokenSource;
        private readonly Mutex _mut;
        private Task _task;

        public Server()
        {
            ConnectionInfos = new ObservableCollection<ConnectionInfo>();
            _mut = new Mutex();
            _tokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            if (IsStarted)
                return;
            
            _task = Task.Factory.StartNew(StartServer, _tokenSource.Token);
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
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, LocalPort);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(50);

                _listener.BeginAccept(
                        AcceptCallback,
                        _listener);

                IsStarted = true;

            }
            catch (Exception e)
            {
                throw e;
                var me = new ExInfo()
                    {
                        AdditionalInfo = "On start",
                        Exception = e
                    };
                OnServerError(me);
            }

        }

        public void Stop()
        {
            if (!IsStarted)
                return;
            try
            {
                _mut.WaitOne(2000);
            }
            catch (Exception)
            {
                
            }
            _mut.Close();
            _tokenSource.Cancel();
            try
            {
                _task.Wait(1000, _tokenSource.Token);
            }
            catch (Exception)
            {
                throw;
            }
            IsStarted = false;

        }

        private void AcceptCallback(IAsyncResult ar)
        {
            _mut.WaitOne();
            var listener = (Socket)ar.AsyncState;
            var sock = listener.EndAccept(ar);

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
            finally
            {
                _mut.ReleaseMutex();
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            _mut.WaitOne();

            var connection = (ConnectionInfo)ar.AsyncState;
            var sock = connection.Socket;

            int bytesRead = sock.EndReceive(ar);

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
                        DoStop(connection, cm.Sender);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            try
            {
                sock.BeginReceive(connection.Buffer, 0, ConnectionInfo.BufferSize, 0,
                                  ReadCallback, connection);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                _mut.ReleaseMutex();
            }
        }

        void DoMessage(ConnectionInfo connection, ClientMessage cm)
        {

            var receiver = FindConnectionByUserName(cm.Receiver);

            //Сказать, что пользователь офлайн
            if (receiver == null)
            {
                var users = CreateUserList();
                var mes = UserListToMessage(users);
                Send(connection, MySerializer.SerializeSomethingToBytes(mes));
                return;
            }

            if (cm.IsPrivate)
            {
                Send(receiver, MySerializer.SerializeSomethingToBytes(cm));
            }
            else
            {
                foreach (var info in ConnectionInfos)
                {
                    if(info.Equals(connection))
                        continue;
                    Send(info, MySerializer.SerializeSomethingToBytes(cm));
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
                ConnectionInfos.Add(connection);
                SendNames();
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
                Send(connection, MySerializer.SerializeSomethingToBytes(mes));*/
            }
            else
            {
                //Сделать чтоб не пускало
                var users = CreateUserList();
                var mes = UserListToMessage(users);
                Send(fuser, MySerializer.SerializeSomethingToBytes(mes));
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
            var removed = ConnectionInfos.Remove(connection);
            if (removed)
            {
                OnNewLogRecord(new LogRecord
                    {
                        UserName = name,
                        Event = "Пользователь вышел из системы",
                        Date = DateTime.Now
                    });
            }
            SendNames();
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
                Send(info, MySerializer.SerializeSomethingToBytes(mes));
            }
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

        void Send(ConnectionInfo connection, byte[] data)
        {
            connection.Socket.Send(data);
        }

    }

    class ExInfo
    {
        public Exception Exception { get; set; }
        public string AdditionalInfo { get; set; }
    }

}
