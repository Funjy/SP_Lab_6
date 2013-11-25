using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace ClientServerInterface
{
    internal class MySerializer<T>
    {
        public static byte[] SerializeSomethingToBytes(T toSerialize)
        {
            var ms = new MemoryStream();
            using (var writer = new BsonWriter(ms))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, toSerialize);
            }
            return ms.ToArray();
        }

        public static string SerializeSomethingToBase64String(T toSerialize)
        {
            //var ms = new MemoryStream();
            //using (var writer = new BsonWriter(ms))
            //{
            //    var serializer = new JsonSerializer();
            //    serializer.Serialize(writer, toSerialize);
            //}
            return Convert.ToBase64String(SerializeSomethingToBytes(toSerialize));
        }

        public static T DeserializeFromBytesToUserInfos(byte[] toSerialize, int length = -1)
        {
            int len = toSerialize.Length;
            if (length != -1)
                len = length;
            T toRet;
            var ms = new MemoryStream(toSerialize, 0, len);
            using (var reader = new BsonReader(ms))
            {
                var serializer = new JsonSerializer();
                toRet = serializer.Deserialize<T>(reader);
            }
            return toRet;
        }

        public static T DeserializeFromBase64StringToUserInfos(string toSerialize)
        {
            //T toRet;
            byte[] data = Convert.FromBase64String(toSerialize);
            return DeserializeFromBytesToUserInfos(data);
            //var ms = new MemoryStream(data);
            //using (var reader = new BsonReader(ms))
            //{
            //    var serializer = new JsonSerializer();
            //    toRet = serializer.Deserialize<T>(reader);
            //}
            //return toRet;
        }
    }
}
