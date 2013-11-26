using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SP_Lab_6_server
{
    class ConnectionInfo
    {
        protected bool Equals(ConnectionInfo other)
        {
            return Equals(Socket, other.Socket) && string.Equals(UserName, other.UserName);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Socket != null ? Socket.GetHashCode() : 0)*397) ^ (UserName != null ? UserName.GetHashCode() : 0);
            }
        }

        public string UserName { get; set; }

        public Socket Socket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];

        public override string ToString()
        {
            return UserName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ConnectionInfo) obj);
        }

    }
}
