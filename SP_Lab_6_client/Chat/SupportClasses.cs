using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Text;
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{

    public class Res
    {
        public static ResourceManager Rm = new ResourceManager(typeof (LocalRes.SP_Lab_6));
    }

    public static class AliveInfo
    {
        public static List<UserInfo> Users { get; set; }

        public static CultureInfo CurrentCulture { get; set; }

        public static ChatClient Chat { get; set; }

        static AliveInfo()
        {
            CurrentCulture = new CultureInfo("ru");
        }
    }

    // ReSharper disable InconsistentNaming
    public static class FunctionsParameters
    {
        public const string GENERAL_MESSAGE = "gnrl_msg";
    }
    // ReSharper restore InconsistentNaming

    public delegate void VoidDelegate();

}
