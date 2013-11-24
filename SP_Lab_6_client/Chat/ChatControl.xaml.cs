using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{
    /// <summary>
    /// Логика взаимодействия для ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl, IDisposable
    {
        private ObservableCollection<TabItem> _windows;
        private string _general;

        public ChatControl()
        {
            InitializeComponent();
            Init();
        }

        public void UiLanguageChanged()
        {
            UsersListLabel.Content = Res.Rm.GetString("UsersList", AliveInfo.CurrentCulture);
            SendButtonTitle.Text = Res.Rm.GetString("Send", AliveInfo.CurrentCulture);

            _general = Res.Rm.GetString("General", AliveInfo.CurrentCulture);

            _windows[0].Header = _general;

        }

        public void Init()
        {
            //Create Send Command
            var myCommand1 = new RoutedCommand();
            CommandBindings.Add(new CommandBinding(myCommand1, SendButton_OnClick));
            myCommand1.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            
            AliveInfo.Chat = new ChatClient("228");

            //Chat init
            AliveInfo.Chat.Start();
            AliveInfo.Chat.NewNames += ChatOnNewNames;
            AliveInfo.Chat.ReceiveMsg += ChatOnReceiveMsg;
            
            //Tabs init
            _windows = new ObservableCollection<TabItem>();
            ChatWindows.ItemsSource = _windows;
            _general = Res.Rm.GetString("General", AliveInfo.CurrentCulture);
            var ti = new TabItem
                {
                    Header = _general,
                    Content = new ChatWindow(_general),
                    IsSelected = true,
                    FontFamily = new FontFamily("SegoeUI")
                };
            _windows.Add(ti);

            UiLanguageChanged();

        }

        //ToImplement
        private void ChatOnReceiveMsg(ClientMessage cl)
        {

            if(cl.Sender == AliveInfo.Chat.Name)
                return;

            //cl = ClientMessage.DeserializeMessage(message);
            //cl.Sender = sender;
            cl.Side = MessageSide.You;

            AddMessageUi(cl);
        }

        private void ChatOnNewNames(object sender, List<string> names)
        {
            //Убираем свое имя из списка
            names.Remove(AliveInfo.Chat.Name);
            UserContainer.ItemsSource = NamesToUsers(names);
            var view = (CollectionView)CollectionViewSource.GetDefaultView(UserContainer.ItemsSource);
            view.SortDescriptions.Add(new SortDescription("UserName", ListSortDirection.Ascending));
        }

        private void AddMessageUi(ClientMessage cm)
        {
            string sender;
            if (!cm.IsPrivate)
            {
                sender = _general;
            }
            else
                sender = cm.Sender;

            var win = _windows.FirstOrDefault(x => x.Header.ToString() == sender);

            if (win == null)
            {
                win = AddUserTab(sender, new ChatWindow(sender));
            }

            var cw = win.Content as ChatWindow;
            if (cm.Side == MessageSide.Me)
                cm.Sender = Res.Rm.GetString("Me", AliveInfo.CurrentCulture);
            cw.MesItems.Add(cm);
        }

        private IEnumerable<User> NamesToUsers(IEnumerable<string> names)
        {
            var toRet = new ObservableCollection<User>();
            foreach (var name in names)
            {
                toRet.Add(new User(name));
            }
            return toRet;
        }

        public void Dispose()
        {
            AliveInfo.Chat.Stop();
        }

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            var receiver = DetermineReceiver();

            var m = new ClientMessage
                {
                    TimeStamp = DateTime.Now,
                    Message = WriteBox.Text,
                    IsPrivate = true,
                    Sender = AliveInfo.Chat.Name
                };

            if (receiver == _general)
            {
                receiver = FunctionsParameters.GENERAL_MESSAGE;
                m.IsPrivate = false;
            }
            AliveInfo.Chat.SendMessage(m.Serialize(), receiver);
            m.Sender = AliveInfo.Chat.Name;
            AddMessageUi(m);
            WriteBox.Clear();
            
        }

        private string DetermineReceiver()
        {
            var ti = ChatWindows.SelectedItem as TabItem;
            return ti.Header.ToString();
        }

        private void UserContainer_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var view = sender as ListBox;
            var u = view.SelectedItem as User;

            var win = _windows.FirstOrDefault(x => x.Header.ToString() == u.UserName);

            if (win == null)
            {
                win = AddUserTab(u.UserName, new ChatWindow(u.UserName));
            }

            win.IsSelected = true;
        }

        private TabItem AddUserTab(string header, ChatWindow content)
        {
            var win = new TabItem { Header = header, Content = content, FontFamily = new FontFamily("SegoeUI")};
            win.MouseDoubleClick += WinOnMouseDoubleClick;
            _windows.Add(win);
            return win;
        }

        private void RemoveUserTab(TabItem tab)
        {
            _windows.Remove(tab);
        }

        private void WinOnMouseDoubleClick(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var tabItem = (sender as TabItem);
            var mo = (tabItem.Content as UIElement).IsMouseOver;
            if (!mo)
                RemoveUserTab(sender as TabItem);
        }
    }
}
