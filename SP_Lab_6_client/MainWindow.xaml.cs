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
using SP_Lab_6_client.Chat;

namespace SP_Lab_6_client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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
                chat.Init();
            }
        }
    }
}
