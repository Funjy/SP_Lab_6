using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SP_Lab_6_client.Chat
{
    class User
    {
        public string UserName { get; set; }

        public User(string name)
        {
            UserName = name;
        }

        public override string ToString()
        {
            return UserName;
        }

    }
}
