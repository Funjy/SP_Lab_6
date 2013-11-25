using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ClientServerInterface;
using SP_Lab_6_client.Chat;

namespace SP_Lab_6_client
{
    /// <summary>
    /// Логика взаимодействия для UserNameWindow.xaml
    /// </summary>
    public partial class UserNameWindow
    {
        public UserNameWindow()
        {
            InitializeComponent();
            AliveInfo.Chat = new ChatClient("");
            AliveInfo.Chat.ReceiveMsg += ChatOnReceiveMsg;
            
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            var userName = UserNameBox.Text;
            AliveInfo.Chat.Name = userName;
            AliveInfo.Chat.Start();
        }

        private void ChatOnReceiveMsg(ClientMessage mes)
        {
            if (mes.MesType == MessageType.UserList)
            {
                var uList = MySerializer.DeserializeFromBase64String<List<UserInfo>>(mes.Message);
                DialogResult = true;
                Close();
            }
            else if(mes.MesType == MessageType.System)
            {
                MessageBox.Show(mes.Message);
            }
            else
            {
                MessageBox.Show("Получено неожиданное сообщение");
            }
            IsEnabled = true;
        }
    }
}
