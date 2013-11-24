using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SP_Lab_6_server
{
    class UserInfo
    {
        public string UserName { get; set; }

        public Socket Socket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];

    }
}
