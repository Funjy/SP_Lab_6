using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SP_Lab_6_server
{
    internal class Server
    {
        public delegate void UsersListChangedDelegate(List<UserInfo> users);

        public event UsersListChangedDelegate UsersListChanged;

        protected virtual void OnUsersListChanged(List<UserInfo> users)
        {
            UsersListChangedDelegate handler = UsersListChanged;
            if (handler != null) handler(users);
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


        //port
        private const int LocalPort = 11337;

        public ObservableCollection<UserInfo> UserInfos { get; set; }
        
        public bool IsStarted { get; private set; }

        private Socket _listener;
        private CancellationTokenSource _tokenSource;
        private Mutex _mut;
        private Task _task;

        public Server()
        {
            UserInfos = new ObservableCollection<UserInfo>();
            _mut = new Mutex();
        }

        public void Start()
        {
            _tokenSource = new CancellationTokenSource();

            _task = Task.Factory.StartNew(StartServer, _tokenSource.Token);
            //_task.Start();
        }

        private void StartServer()
        {
            if(IsStarted)
                return;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            var localEndPoint = new IPEndPoint(ipAddress, LocalPort);

            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            try
            {
                _listener.Bind(localEndPoint);
                _listener.Listen(50);

                _listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
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

            var userConnection = new UserInfo { Socket = sock };
            sock.BeginReceive(userConnection.Buffer, 0, UserInfo.BufferSize, 0,
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

            var user = (UserInfo)ar.AsyncState;
            var sock = user.Socket;

            int bytesRead = sock.EndReceive(ar);

            if (bytesRead > 0)
            {
                //Обработать сообщение
            }

            try
            {
                sock.BeginReceive(user.Buffer, 0, UserInfo.BufferSize, 0,
                              ReadCallback, user);
            }
            catch (Exception)
            {
                
                throw;
            }

            _mut.ReleaseMutex();
        }
    }

    class ExInfo
    {
        public Exception Exception { get; set; }
        public string AdditionalInfo { get; set; }
    }

}
