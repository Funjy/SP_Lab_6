using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace SP_Lab_6_client.Chat
{
    /// <summary>
    /// Логика взаимодействия для ReceiveFileElement.xaml
    /// </summary>
    public partial class ReceiveFileElement : IFileCarryView
    {
        private readonly FileOperation _fo;
        private string _filePath;

        public string FileNameText { get; set; }
        public string RejectText { get; set; }

        private ReceiveFileElement()
        {
            InitializeComponent();
            DataContext = this;
        }

        public ReceiveFileElement(FileOperation fo) : this()
        {
            _fo = fo;
            FileNameText = _fo.Messages[0].File.FileName;
            _filePath = FileNameText;
            RejectText = "Отменено.";
            ProgressBarControl.Maximum = fo.Messages[0].File.QueueLength;
            SignEvents();
        }

        private void FileCarrierOnCompleteFile(FileOperation fo)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
                Complete();
        }

        private void FileCarrierOnRejectFile(FileOperation fo, string message)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
            {
                Reject(message);
            }
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.AcceptReceiving(_fo);
            AcceptButton.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.RejectReceiving(_fo);
            //Reject();
        }

        private void FileCarrierOnIncomingFile(FileOperation fo)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
                Dispatcher.Invoke(new Action(() =>
                {
                    ProgressBarControl.Value++;
                }));
            
        }

        private void Complete()
        {
            UnsignEvents();
            Dispatcher.Invoke(new Action(() =>
                {
                    RequestPanel.Visibility = Visibility.Collapsed;
                    CompletePanel.Visibility = Visibility.Visible;
                }));
        }
        
        private void Reject(string message)
        {
            UnsignEvents();
            Dispatcher.Invoke(new Action(() =>
                {
                    RequestPanel.Visibility = Visibility.Collapsed;
                    RejectPanel.Visibility = Visibility.Visible;
                    RejectTextBox.Text = message;
                }));
        }

        void SignEvents()
        {
            FileCarrier.RejectFile += FileCarrierOnRejectFile;
            FileCarrier.IncomingFile += FileCarrierOnIncomingFile;
            FileCarrier.CompleteFile += FileCarrierOnCompleteFile;
        }

        void UnsignEvents()
        {
            FileCarrier.RejectFile -= FileCarrierOnRejectFile;
            FileCarrier.IncomingFile -= FileCarrierOnIncomingFile;
            FileCarrier.CompleteFile -= FileCarrierOnCompleteFile;
        }

        private void OpenButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(_filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void ShowButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fi = new FileInfo(_filePath);
            var fn = fi.FullName;
            Process.Start("explorer.exe", "/select," + fn);
        }
    }
}
