using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{
    public delegate void ComingFileDelegate(FileOperation fo);

    public class FileCarrier
    {
        #region events
        public static event ComingFileDelegate IncomingFile;

        private static void OnIncomingFile(FileOperation fo)
        {
            ComingFileDelegate handler = IncomingFile;
            if (handler != null) handler(fo);
        }

        public static event ComingFileDelegate DepartingFile;

        private static void OnDepartingFile(FileOperation fo)
        {
            ComingFileDelegate handler = DepartingFile;
            if (handler != null) handler(fo);
        }

        public static event ComingFileDelegate RejectFile;

        private static void OnRejectFile(FileOperation fo)
        {
            ComingFileDelegate handler = RejectFile;
            if (handler != null) handler(fo);
        }

        public static event ComingFileDelegate CompleteFile;

        private static void OnCompleteFile(FileOperation fo)
        {
            ComingFileDelegate handler = CompleteFile;
            if (handler != null) handler(fo);
        }

        #endregion

        private const int BufLength = 10000;
        private static int _blocksNum = 4;
        //private static readonly List<FileOperation> FileOperations;
        private static readonly Dictionary<Guid, FileOperation> FileOperations;

        public static int BlocksNumber {get { return _blocksNum; } set { _blocksNum = value; }}

        static FileCarrier()
        {
            //FileOperations = new List<FileOperation>();
            FileOperations = new Dictionary<Guid, FileOperation>();
            AliveInfo.Chat.ReceiveFile += ChatOnReceiveFile;
        }

        public static FileOperation SendFile(string filePath, string sender, string receiver)
        {
            var fo = new FileOperation
                {
                    Filer = new FilePreparer(filePath, true, _blocksNum),
                    SendSide = FileOperation.Side.Sending
                };

            //Send request
            var cm = new ClientMessage
                {
                    IsPrivate = true,
                    MesType = MessageType.File,
                    Sender = sender,
                    Receiver = receiver,
                    TimeStamp = DateTime.Now,
                    File = new MessageFile
                        {
                            OperationType = MessageFile.MessageFileType.SendRequest,
                            QueueLength = _blocksNum,
                            QueuePosition = 0,
                            FileName = fo.Filer.FileName,
                            DataLength = fo.Filer.FileLength,
                            BlockLength = fo.Filer.BlockSize,
                            TransactionId = Guid.NewGuid()
                        },
                };
            fo.NewMessage(cm);            
            FileOperations.Add(fo.Messages[0].File.TransactionId, fo);
            AliveInfo.Chat.SendMessage(cm);
            return fo;
        }

        //Подтверждаем прием файла
        public static void AcceptReceiving(FileOperation fo)
        {
            //Prepare Sockets and send in message

            var mes = MySerializer.SerializeSomethingToBase64String(PrepareSockets(fo.Messages[0].File.QueueLength, ref fo));

            //Send responce accept
            var cm = new ClientMessage
                {
                    IsPrivate = fo.Messages[0].IsPrivate,
                    Message = mes,
                    //Sender = fo.Messages[0].Sender,
                    Receiver = fo.Messages[0].Sender,
                    MesType = MessageType.File,
                    File = new MessageFile
                        {
                            TransactionId = fo.Messages[0].File.TransactionId,
                            OperationType = MessageFile.MessageFileType.SendResponseAccept
                        }
                };
            AliveInfo.Chat.SendMessage(cm);
        }

        //Отменяем прием файла
        public static void RejectReceiving(FileOperation fo)
        {
            //Send responce reject
            var cm = new ClientMessage
                {
                    IsPrivate = fo.Messages[0].IsPrivate,
                    //Sender = fo.Messages[0].Sender,
                    Receiver = fo.Messages[0].Sender,
                    MesType = MessageType.File,
                    File = new MessageFile
                        {
                            TransactionId = fo.Messages[0].File.TransactionId,
                            OperationType = MessageFile.MessageFileType.SendResponseReject
                        }
                };
            FileOperations.Remove(fo.Messages[0].File.TransactionId);
            AliveInfo.Chat.SendMessage(cm);
        }

        private static List<SocketInfo> PrepareSockets(int count, ref FileOperation fo)
        {
            var l = new List<SocketInfo>(count);
            fo.Sockets = new List<Socket>(count);
            while (count-- > 0)
            {
                var soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                soc.Bind(new IPEndPoint(IPAddress.Any, 0));
                var state = new StateObject
                    {
                        FileOp = fo,
                        Soc = soc
                    };
                soc.Listen(3);
                soc.BeginAccept(AcceptCallback, state);
                fo.Sockets.Add(soc);
                l.Add(new SocketInfo
                    {
                        //Ip = (soc.LocalEndPoint as IPEndPoint).Address.ToString(),
                        Port = (soc.LocalEndPoint as IPEndPoint).Port
                    });
            }
            return l;
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            Socket listener = state.Soc;
            var soc = listener.EndAccept(ar);
            state.Buf = new byte[BufLength];
            state.Soc = soc;
            soc.BeginReceive(state.Buf, 0, BufLength, SocketFlags.None, ReceiveCallback, state);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var soc = state.Soc;

            int bytesRead;
            bytesRead = soc.EndReceive(ar);
            if (bytesRead == 0)
                return;
            var mes = ClientMessage.DeserializeMessage(state.Buf, bytesRead);
            if (mes.MesType != MessageType.File || mes.File == null ||
                mes.File.OperationType != MessageFile.MessageFileType.SendAttempt)
                return;
            ChatOnReceiveFile(mes);
        }

        //Определение типа входящего сообщения
        private static void ChatOnReceiveFile(ClientMessage mes)
        {
            switch (mes.File.OperationType)
            {
                case MessageFile.MessageFileType.SendRequest:
                    DoRequest(mes);
                    break;
                case MessageFile.MessageFileType.SendResponseAccept:
                    DoAccept(mes);
                    break;
                case MessageFile.MessageFileType.SendResponseReject:
                    DoReject(mes);
                    break;
                case MessageFile.MessageFileType.SendAttempt:
                    DoReceive(mes);
                    break;
                case MessageFile.MessageFileType.SendCompleteCheck:
                    DoCompleteCheck(mes);
                    break;
                case MessageFile.MessageFileType.SendCompleteConfirm:
                    DoCompleteConfirm(mes);
                    break;
                case MessageFile.MessageFileType.SendLostBlock:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void DoCompleteConfirm(ClientMessage mes)
        {
            var fo = FileOperations[mes.File.TransactionId];
            OnCompleteFile(fo);
        }

        private static void DoCompleteCheck(ClientMessage mes)
        {
            var fo = FileOperations[mes.File.TransactionId];
            var l = fo.LostMessages();
            var m = fo.Messages[0];
            if (!l.Any())
            {
                //Send complete confirm
                var cm = new ClientMessage
                    {
                        IsPrivate = m.IsPrivate,
                        Sender = m.Receiver,
                        Receiver = m.Sender,
                        MesType = MessageType.File,
                        File = new MessageFile
                            {
                                OperationType = MessageFile.MessageFileType.SendCompleteConfirm,
                                TransactionId = m.File.TransactionId
                            }
                    };
                AliveInfo.Chat.SendMessage(cm);
                ReceiveOperationComplete(fo);
            }
        }

        private static void ReceiveOperationComplete(FileOperation fo)
        {
            var m = fo.Messages[0];
            fo.Filer = new FilePreparer(m.File.FileName);
            fo.Filer.OpenWrite();
            foreach (var message in fo.Messages)
            {
                if(message.Value.File.QueuePosition == 0)
                    continue;
                fo.Filer.Write(message.Value.File.Data);
            }
            fo.Filer.Close();
        }

        private static void DoReceive(ClientMessage mes)
        {
            var fo = FileOperations[mes.File.TransactionId];
            fo.NewMessage(mes);
            OnIncomingFile(fo);
        }

        //Отмена отправки
        private static void DoReject(ClientMessage mes)
        {
            var g = mes.File.TransactionId;
            if (FileOperations.ContainsKey(g))
            {
                OnRejectFile(FileOperations[g]);
                FileOperations.Remove(g);
            }
        }

        //Отправляем файлик и храним полную копию в буфере
        private static void DoAccept(ClientMessage mes)
        {
            var socks = MySerializer.DeserializeFromBase64String<List<SocketInfo>>(mes.Message, true);
            var fo = FileOperations[mes.File.TransactionId];
            fo.Filer.OpenRead();
            fo.Sockets = new List<Socket>(socks.Count);
            foreach (var socInfo in socks)
            {
                //Init socket
                var soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                {
                    ReceiveTimeout = 1500,
                    SendTimeout = 1500
                };
                var remoteEp = new IPEndPoint(IPAddress.Parse(socInfo.Ip), socInfo.Port);
                soc.Connect(remoteEp);
                var data2Send = fo.Filer.ReadBlock();
                //Create message

                var cm = new ClientMessage
                    {
                        Receiver = fo.Messages[0].Receiver,
                        MesType = MessageType.File,
                        File = new MessageFile
                            {
                                Data = data2Send,
                                DataLength = data2Send.Length,
                                TransactionId = fo.Messages[0].File.TransactionId,
                                OperationType = MessageFile.MessageFileType.SendAttempt,
                                FileName = fo.Messages[0].File.FileName,
                                QueueLength = fo.Messages[0].File.QueueLength,
                                QueuePosition = fo.Filer.BlocksRead
                            }
                    };
                //cm.File.SetData(data2Send);
                //cm.File.Data = data2Send;
                //var b2S = cm.Serialize();
                var state = new StateObject
                {
                    FileOp = fo,
                    Soc = soc,
                    Buf = cm.Serialize()
                };
                soc.BeginSend(state.Buf, 0, state.Buf.Length, SocketFlags.None, SendCallback, state);
                //soc.Send(cm.Serialize());
                //OnDepartingFile(fo);
                fo.NewMessage(cm);
                fo.Sockets.Add(soc);
            }
            fo.Filer.Close();            
        }

        private static void SendCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var soc = state.Soc;
            soc.EndSend(ar);
            OnDepartingFile(state.FileOp);
            //Проверка, все ли части файла отправлены
            if (state.FileOp.Messages[0].File.QueueLength == state.FileOp.Messages.Count - 1)
            {
                SendFileCompleteCheckRequest(state.FileOp);
            }
        }

        private static void SendFileCompleteCheckRequest(FileOperation fo)
        {
            //Send complete check
            var cm = new ClientMessage
                {
                    IsPrivate = fo.Messages[0].IsPrivate,
                    Sender = fo.Messages[0].Sender,
                    Receiver = fo.Messages[0].Receiver,
                    MesType = MessageType.File,
                    File = new MessageFile
                        {
                            OperationType = MessageFile.MessageFileType.SendCompleteCheck,
                            TransactionId = fo.Messages[0].File.TransactionId
                        }
                };
            AliveInfo.Chat.SendMessage(cm);
        }

        private static void DoRequest(ClientMessage mes)
        {
            if (FileOperations.ContainsKey(mes.File.TransactionId))
            {
                return;
            }
            var fo = new FileOperation
                {
                    SendSide = FileOperation.Side.Receiving,
                };

            fo.NewMessage(mes);
            FileOperations.Add(fo.Messages[0].File.TransactionId, fo);
            OnIncomingFile(fo);
        }
    }

    public class FileOperation
    {
        public FilePreparer Filer { get; set; }
        public SortedDictionary<int, ClientMessage> Messages;
        public Side SendSide { get; set; }
        public List<Socket> Sockets { get; set; }

        private Mutex _mut;

        public FileOperation()
        {
            Messages = new SortedDictionary<int, ClientMessage>();
            _mut = new Mutex();
        }

        public void NewMessage(ClientMessage cm)
        {
            _mut.WaitOne();
            if (Messages.ContainsKey(cm.File.QueuePosition))
            {
                //Что-то сделать
                return;
            }
            Messages.Add(cm.File.QueuePosition, cm);
            _mut.ReleaseMutex();
            //var d = new Dictionary<int, string>();
        }

        public List<int> LostMessages()
        {
            _mut.WaitOne();
            var count = Messages[0].File.QueueLength;
            var toRet = new List<int>();
            while (count-- > 0)
            {
                if(!Messages.ContainsKey(count))
                    toRet.Add(count);
            }
            _mut.ReleaseMutex();
            return toRet;
        }

        public enum Side
        {
            Sending,
            Receiving
        }
    }

    public class StateObject
    {
        public Socket Soc { get; set; }
        public FileOperation FileOp { get; set; }
        public byte[] Buf { get; set; }
    }

}
