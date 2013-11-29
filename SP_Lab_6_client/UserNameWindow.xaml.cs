using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
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
        private const string IpRegex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
        private string _prevIp = "127.0.0.1";
        private CancellationTokenSource _ctSource;
        private Task _task;

        private Timer _tim;

        public UserNameWindow()
        {
            InitializeComponent();
            IpBox.Text = _prevIp;

            var myCommand1 = new RoutedCommand();
            CommandBindings.Add(new CommandBinding(myCommand1, AcceptButton_OnClick));
            myCommand1.InputGestures.Add(new KeyGesture(Key.Enter));

            AliveInfo.Chat = new ChatClient();
            AliveInfo.Chat.ReceiveMsg += ChatOnReceiveMsg;
            AliveInfo.Chat.NewNames += ChatOnNewNames;
        }

        private void ChatOnNewNames(object sender, List<UserInfo> names)
        {
            
            _tim.Change(Timeout.Infinite, Timeout.Infinite);
            AliveInfo.Users = names;
            AliveInfo.Chat.ReceiveMsg -= ChatOnReceiveMsg;
            AliveInfo.Chat.NewNames -= ChatOnNewNames;
            UseDispatcher(() =>
                {
                    DialogResult = true;
                    Close();
                });
        }

        public void UseDispatcher(Action action)
        {
            Dispatcher.Invoke(action);
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(UserNameBox.Text))
            {
                MessageBox.Show("Введите имя пользователя");
                return;
            }
            IsEnabled = false;
            var userName = UserNameBox.Text;
            AliveInfo.Chat.Name = userName;
            AliveInfo.Chat.Ip = IPAddress.Parse(IpBox.Text);
            _ctSource = new CancellationTokenSource();
            _task = Task.Factory.StartNew(AliveInfo.Chat.Start, _ctSource.Token);
            _tim = new Timer(TimCallback, null, 1200, 0);

            /*try
            {
                //t.Wait(1200);
                //_ctSource.Cancel();
                
                //_task.Wait();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof (AggregateException))
                {
                    if (ex.InnerException != null)
                        MessageBox.Show(ex.InnerException.Message);
                    IsEnabled = true;
                }
                //throw;
            }*/
            

        }

        private void TimCallback(object state)
        {
            _tim.Change(Timeout.Infinite, Timeout.Infinite);

            _ctSource.Cancel();
            try
            {
                //t.Wait(1200);
                _ctSource.Cancel();
                _task.Wait(2000);
                MessageBox.Show("Сервер не ответил за установленное время");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof (AggregateException))
                {
                    if (ex.InnerException != null)
                        MessageBox.Show(ex.InnerException.Message);
                    else
                    {
                        MessageBox.Show(ex.Message);
                    }
                    //UseDispatcher(() => IsEnabled = true);
                }
            }
            finally
            {
                UseDispatcher(() =>
                {
                    IsEnabled = true;
                });
            }
        }

        private void ChatOnReceiveMsg(ClientMessage mes)
        {
            _tim.Change(Timeout.Infinite, Timeout.Infinite);
            if (mes.MesType == MessageType.UserList)
            {
                var uList = MySerializer.DeserializeFromBase64String<List<UserInfo>>(mes.Message);
                AliveInfo.Users = uList;
                UseDispatcher(() =>
                {
                    DialogResult = true;
                    Close();
                });
            }
            else if(mes.MesType == MessageType.System)
            {
                if (mes.Message == SystemMessageTypes.USER_EXIST)
                {
                    MessageBox.Show("Пользователь с таким именем уже в системе");
                    AliveInfo.Chat.Stop();
                }
            }
            else
            {
                MessageBox.Show("Получено неожиданное сообщение");
            }
            UseDispatcher(() =>
            {
                IsEnabled = true;
            });
        }

        private void IpBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var tb = sender as TextBox;
            var regex = new Regex(IpRegex);
            IPAddress ip;
            if (!regex.IsMatch(tb.Text) || !IPAddress.TryParse(tb.Text, out ip))
            {
                var ci = tb.CaretIndex;
                
                tb.Text = _prevIp;
                if (ci < tb.Text.Length && ci >= 0)
                    tb.CaretIndex = ci;
            }
            else
            {
                _prevIp = tb.Text;
            }
        }
    }
}
