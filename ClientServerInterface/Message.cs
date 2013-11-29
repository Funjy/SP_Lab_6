using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace ClientServerInterface
{
    public class ClientMessage
    {
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public MessageSide Side { get; set; }
        public MessageType MesType { get; set; }
        public bool IsPrivate { get; set; }
        public MessageFile File { get; set; }

        public object FileSendContent { get; set; }

        public ClientMessage()
        {
            Side = MessageSide.Me;
            MesType = MessageType.Text;
            File = null;
        }

        public static ClientMessage DeserializeMessage(byte[] buff, int length = -1)
        {
            int len = buff.Length;
            if (length != -1)
                len = length;
            var ms = new MemoryStream(buff, 0, len);
            ClientMessage mes;
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer();
                mes = serializer.Deserialize<ClientMessage>(reader);
            }
            return mes;
        }

        public byte[] Serialize()
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, this);
            }
            return ms.ToArray();
        }
    }

    public enum MessageSide
    {
        Me,
        You
    }

    public enum  MessageType
    {
        Text,
        Start,
        Stop,
        UserList,
        File,
        System
    }

    public class MessageCollection : ObservableCollection<ClientMessage>
    {
    }

    public class AdditionalData
    {
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class MessageFile
    {
        //Позиция блока
        public int QueuePosition { get; set; }
        //Количество блоков
        public int QueueLength { get; set; }
        //Длина массива данных
        public int DataLength { get; set; }
        //Длина блока данных
        public int BlockLength { get; set; }
        //Массив данных
        public byte[] Data { get; set; }
        //public string DataBytes { get; set; }
        //Имя файла
        public string FileName { get; set; }
        //Тип содержимого
        public MessageFileType OperationType { get; set; }
        //Id файла
        public Guid TransactionId { get; set; }

        //public void SetData(IList<byte> data)
        //{
        //    var bs = new List<DataValue>(data.Count);
        //    foreach (var b in data)
        //    {
        //        bs.Add(new DataValue(b));
        //    }
        //    DataBytes = JsonConvert.SerializeObject(bs);
        //}

        //public IList<byte> GetDataBytes()
        //{
        //    //return JsonConvert.DeserializeObject<IList<byte>>(DataBytes);
        //    var bs = JsonConvert.DeserializeObject<List<DataValue>>(DataBytes);
        //    var mas = new byte[bs.Count];
        //    int i = 0;
        //    foreach (var value in bs)
        //    {
        //        mas[i++] = (byte)value.Value;
        //    }
        //    return new List<byte>();
        //}

        public enum MessageFileType
        {
            SendRequest,            //Запрос на передачу
            SendResponseAccept,     //Ращрешение передачи
            SendResponseReject,     //Отмена передачи
            SendAttempt,            //Передача
            //SendBlockReceived,      //Блок получен
            SendCompleteCheck,      //Все части файла отправлены
            SendCompleteConfirm,    //Все части файла получены
            SendLostBlock           //Запрос недостающего блока
        }

        public class DataValue
        {
            public UInt16 Value { get; set; }

            public DataValue(byte value)
            {
                Value = value;
            }
        }

    }

    public static class SystemMessageTypes
    {
        // ReSharper disable InconsistentNaming
        public const string CHECK_HERE = "chk";
        public const string USER_EXIST = "usr_exst";
        // ReSharper restore InconsistentNaming
    }

}
