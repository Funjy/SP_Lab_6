using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;


namespace ClientServerInterface
{
    public class MySerializer
    {
        public static byte[] SerializeSomethingToBytes(object toSerialize)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, toSerialize);
            }
            return ms.ToArray();
        }

        //public static byte[] SerializeSomethingToBytes(object toSerialize)
        //{
        //    var ms = new MemoryStream();
        //    using (var writer = new BsonWriter(ms))
        //    {
        //        var serializer = new JsonSerializer();
        //        serializer.Serialize(writer, toSerialize);
        //    }
        //    return ms.ToArray();
        //}

        public static string SerializeSomethingToBase64String(object toSerialize)
        {
            //var ms = new MemoryStream();
            //using (var writer = new BsonWriter(ms))
            //{
            //    var serializer = new JsonSerializer();
            //    serializer.Serialize(writer, toSerialize);
            //}
            return Convert.ToBase64String(SerializeSomethingToBytes(toSerialize));
        }

        public static T DeserializeFromBytes<T>(byte[] toSerialize, bool isArray = false, int length = -1)
        {
            int len = toSerialize.Length;
            if (length != -1)
                len = length;
            T toRet;
            var ms = new MemoryStream(toSerialize, 0, len);
            using (var reader = new BsonReader(ms))
            {
                reader.ReadRootValueAsArray = isArray;
                var serializer = new JsonSerializer();
                toRet = serializer.Deserialize<T>(reader);
            }
            return toRet;
        }

        public static T DeserializeFromBase64String<T>(string toSerialize, bool isArray = false)
        {
            T toRet;
            byte[] data = Convert.FromBase64String(toSerialize);
            var ms = new MemoryStream(data);
            using (var reader = new BsonReader(ms))
            {
                reader.ReadRootValueAsArray = isArray;
                var serializer = new JsonSerializer();
                toRet = serializer.Deserialize<T>(reader);
            }
            return toRet;
        }
    }
}
