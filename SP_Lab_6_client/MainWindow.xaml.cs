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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using SP_Lab_6_client.Chat;

namespace SP_Lab_6_client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private volatile bool _disc;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            var chat = new ChatControl();
            ChatContainer.Children.Add(chat);
            var uWin = new UserNameWindow();
            if (uWin.ShowDialog() == true)
            {
                chat.Init(AliveInfo.Users);
                AliveInfo.Chat.ServerDisconnect += ChatOnServerDisconnect;
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void ChatOnServerDisconnect()
        {
            if (!_disc)
            {
                MessageBox.Show("Сервер отключился. Программа будет закрыта.");
                _disc = true;
                Dispatcher.Invoke(new Action(Application.Current.Shutdown));
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            AliveInfo.Chat.Stop();
        }
    }
}
