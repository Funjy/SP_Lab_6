﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Globalization;
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

namespace SP_Lab_6_server
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ObservableCollection<LogRecord> _logs;
        //private ObservableCollection<ListViewItem> _userListItems;
        
        // ReSharper disable InconsistentNaming
        private const string DEPLOY_SERVER = "deploy cupcakes";
        private const string UNDEPLOY_SERVER = "undeploy cupcakes";
        private const string STARTED_SERVER = "Работает";
        private const string STOPPED_SERVER = "Не запущен";
        // ReSharper restore InconsistentNaming

        private Server _server;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_OnInitialized(object sender, EventArgs e)
        {
            Init();
        }

        void Init()
        {
            //Log grid
            var dgc1 = new DataGridTextColumn
                {
                    Header = "Пользователь",
                    Binding = new Binding("UserName"),
                };
            var dgc2 = new DataGridTextColumn
                {
                    Header = "Событие",
                    Binding = new Binding("Event"),
                };
            var dgc3 = new DataGridTextColumn
                {
                    Header = "Время",
                    Binding = new Binding("Date") {StringFormat = "ddd, HH:mm",},
                };
            LogGrid.Columns.Add(dgc1);
            LogGrid.Columns.Add(dgc2);
            LogGrid.Columns.Add(dgc3);

            _logs = new ObservableCollection<LogRecord>();
            LogGrid.ItemsSource = _logs;

            ICollectionView dataView = CollectionViewSource.GetDefaultView(LogGrid.ItemsSource);
            var sd = new SortDescription("Время", ListSortDirection.Descending);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();

            //Server

            _server = new Server();
            _server.NewLogRecord += ServerOnNewLogRecord;

            //User list

            //_userListItems = new ObservableCollection<ListViewItem>();
            //UsersContainer.ItemsSource = _server.ConnectionInfos;
            _server.UsersListUpdated += () => Dispatcher.Invoke(new Action(
                                                                    () =>
                                                                        {
                                                                            UsersContainer.ItemsSource = new ObservableCollection<ConnectionInfo>(_server.ConnectionInfos);
                                                                            UsersCountBlock.Text = _server.ConnectionInfos.Count.ToString(CultureInfo.InvariantCulture);
                                                                        }));
            /*_server.ConnectionInfos.CollectionChanged += (sender, args) =>
                {
                    UsersCountBlock.Text = _server.ConnectionInfos.Count.ToString(CultureInfo.InvariantCulture);
                };*/

            StatusBlock.Text = STOPPED_SERVER;
            UsersCountBlock.Text = "0";

            //tmp
            //_logs.Add(new LogRecord { Date = DateTime.Now, Event = "Event", UserName = "User" });
            //_userListItems.Add(new ListViewItem {Content = "User 1337"});

        }

        private void ServerOnNewLogRecord(LogRecord record)
        {
            Dispatcher.Invoke(new Action(() => _logs.Add(record)));
        }

        private void DeployButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_server.IsStarted)
            {
                CupcakeText.Text = DEPLOY_SERVER;
                _server.Stop();
                StatusBlock.Text = STOPPED_SERVER;
            }
            else
            {
                CupcakeText.Text = UNDEPLOY_SERVER;
                _server.Start();
                StatusBlock.Text = STARTED_SERVER;
            }
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            _server.Stop();
        }
    }

}
