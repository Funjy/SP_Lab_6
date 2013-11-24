using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Client_Server_Interface;

namespace SP_Lab_6_client.Chat
{
    /// <summary>
    /// Логика взаимодействия для ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : UserControl
    {
        
        private const int MaxMessages = 200;

        public MessageCollection MesItems { get; set; }

        //MessageContentPresenter _pres;

        public ChatWindow Win {
            get { return this; }
        }

        public string Title { get; private set; }

        private ChatWindow()
        {
            InitializeComponent();
            MesItems = new MessageCollection();
            MesItems.CollectionChanged += (sender, args) =>
                {
                    Scroller.ScrollToBottom();
                    if (MesItems.Count > MaxMessages)
                    {
                        
                        var t = new Thread(() => Dispatcher.Invoke(new VoidDelegate(() => MesItems.RemoveAt(0))));
                        t.Start();
                    }
                };
            Items.ItemsSource = MesItems;
        }

        public ChatWindow(string title) : this()
        {
            Title = title;
        }


    }
}
