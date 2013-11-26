﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
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
            ChatWindows.SelectionChanged += ChatWindows_SelectionChanged;
            //Init();
        }

        void ChatWindows_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabs = sender as TabControl;
            var chatw = tabs.SelectedItem as TabItem;
            chatw.DataContext = null;
            var cw = chatw.Content as ChatWindow;
            if (cw.Online)
                SendButton.IsEnabled = true;
            else
                SendButton.IsEnabled = false;

            //var prop = chatw.DataContext as TabHeaderProp;
            //if (prop != null)
            //    prop.NewMessage = false;
        }

        public void UiLanguageChanged()
        {
            UsersListLabel.Content = Res.Rm.GetString("UsersList", AliveInfo.CurrentCulture);
            SendButtonTitle.Text = Res.Rm.GetString("Send", AliveInfo.CurrentCulture);

            _general = Res.Rm.GetString("General", AliveInfo.CurrentCulture);

            _windows[0].Header = _general;

        }

        public void Init(List<UserInfo> users)
        {
            //Create Send Command
            var myCommand1 = new RoutedCommand();
            CommandBindings.Add(new CommandBinding(myCommand1, SendButton_OnClick));
            myCommand1.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            
            //AliveInfo.Chat = new ChatClient("");

            //Chat init
            //AliveInfo.Chat.Start();
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

            ChatOnNewNames(null, users);
            //UserContainer.ItemsSource = new ObservableCollection<UserInfo>(users);

            UiLanguageChanged();

        }

        void ChangeSendState(bool enabled)
        {
            if (enabled)
            {
                SendButton.IsEnabled = true;
                var myCommand1 = new RoutedCommand();
                CommandBindings.Add(new CommandBinding(myCommand1, SendButton_OnClick));
                myCommand1.InputGestures.Add(new KeyGesture(Key.Enter, ModifierKeys.Control));
            }
            else
            {
                SendButton.IsEnabled = false;
                CommandBindings.Clear();
            }
        }

        private void ChatOnReceiveMsg(ClientMessage cl)
        {

            if(cl.Sender == AliveInfo.Chat.Name)
                return;

            //cl = ClientMessage.DeserializeMessage(message);
            //cl.Sender = sender;
            cl.Side = MessageSide.You;

            AddMessageUi(cl);
        }

        private void ChatOnNewNames(object sender, List<UserInfo> names)
        {
            //Убираем свое имя из списка
            var f = names.FirstOrDefault(u => u.UserName == AliveInfo.Chat.Name);
            if (f != null)
                names.Remove(f);
            Dispatcher.Invoke(new Action(() =>
                {
                    UserContainer.ItemsSource = new ObservableCollection<UserInfo>(names);
                    var view = (CollectionView)CollectionViewSource.GetDefaultView(UserContainer.ItemsSource);
                    view.SortDescriptions.Add(new SortDescription("UserName", ListSortDirection.Ascending));

                    foreach (TabItem tab in ChatWindows.Items)
                    {
                        if (tab.Header.ToString() == _general)
                            continue;
                        var name = names.FirstOrDefault(n => n.UserName == tab.Header.ToString());
                        if (name != null)
                        {
                            var cw = tab.Content as ChatWindow;
                            if (!cw.Online)
                                cw.Online = true;
                            if (tab.IsSelected)
                            {
                                ChangeSendState(true);
                            }
                        }
                        else
                        {
                            var cw = tab.Content as ChatWindow;
                            if(cw.Online)
                                cw.Online = false;
                            if (tab.IsSelected)
                            {
                                ChangeSendState(false);
                            }
                        }
                    }
                }));            
            

        }

        private void AddMessageUi(ClientMessage cm)
        {
            Dispatcher.Invoke(new Action(() =>
                {
                    TabItem win = null;
                    if (!cm.IsPrivate)
                    {
                        win = _windows.FirstOrDefault(x => x.Header.ToString() == _general);
                    }
                    else if(cm.Sender == AliveInfo.Chat.Name)
                    {
                        win = _windows.FirstOrDefault(x => x.Header.ToString() == cm.Receiver);
                    }
                    else if (cm.Receiver == AliveInfo.Chat.Name)
                    {
                        win = _windows.FirstOrDefault(x => x.Header.ToString() == cm.Sender);
                    }
                    if (win == null)
                    {
                        win = AddUserTab(cm.Sender, new ChatWindow(cm.Sender));
                        //win.DataContext = new TabHeaderProp() { NewMessage = true };
                    }
                    if (cm.Sender != AliveInfo.Chat.Name)
                    {
                        if (cm.Receiver == FunctionsParameters.GENERAL_MESSAGE)
                        {
                            if (DetermineReceiver() != _general)
                                win.DataContext = new TabHeaderProp();
                        }
                        else
                            win.DataContext = new TabHeaderProp();
                        //(win.Header as Control).Background = new SolidColorBrush(Colors.Orange);
                    }
                    var cw = win.Content as ChatWindow;
                    if (cm.Side == MessageSide.Me)
                        cm.Sender = Res.Rm.GetString("Me", AliveInfo.CurrentCulture);
                    cw.MesItems.Add(cm);
                }));            
        }

        /*private IEnumerable<UserInfo> NamesToUsers(IEnumerable<string> names)
        {
            var toRet = new ObservableCollection<UserInfo>();
            foreach (var name in names)
            {
                toRet.Add(new UserInfo(){UserName = name});
            }
            return toRet;
        }*/

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
                    Sender = AliveInfo.Chat.Name,
                    Receiver = receiver
                };

            if (receiver == _general)
            {
                receiver = FunctionsParameters.GENERAL_MESSAGE;
                m.Receiver = receiver;
                m.IsPrivate = false;
            }            
            AliveInfo.Chat.SendMessage(m);
            //m.Sender = AliveInfo.Chat.Name;
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
            var u = view.SelectedItem as UserInfo;

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

        private void UserControl_Initialized_1(object sender, EventArgs e)
        {

        }
    }

    class TabHeaderProp
    {
        public bool NewMessage { get; set; }

        public TabHeaderProp()
        {
            NewMessage = true;
        }
    }

}
