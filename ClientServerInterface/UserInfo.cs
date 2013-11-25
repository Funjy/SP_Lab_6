using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace ClientServerInterface
{
    public class UserInfo
    {
        public string UserName { get; set; }
        public IPEndPoint IpEp { get; set; }
    }
}
