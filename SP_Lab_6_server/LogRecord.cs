using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SP_Lab_6_server
{
    class LogRecord
    {
        public string UserName { get; set; }
        public string Event { get; set; }
        public DateTime Date { get; set; }

        public string LogString()
        {
            return UserName + ';' + Event + ';' + Date.ToString(CultureInfo.InvariantCulture);
        }
    }
}
