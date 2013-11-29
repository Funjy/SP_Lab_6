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

namespace SP_Lab_6_client.Chat
{
    /// <summary>
    /// Логика взаимодействия для SendFileElement.xaml
    /// </summary>
    public partial class SendFileElement : IFileCarryView
    {
        private readonly FileOperation _fo;

        public string FileNameText { get; set; }
        public string RejectText { get; set; }

        private SendFileElement()
        {
            InitializeComponent();
        }

        public SendFileElement(FileOperation fo) : this()
        {
            _fo = fo;
            //FileNameBox.Text = fo.Messages[0].File.FileName;
            FileNameText = fo.Messages[0].File.FileName;
            ProgressBarControl.Maximum = fo.Messages[0].File.QueueLength;
            SignEvents();
        }

        void SignEvents()
        {
            FileCarrier.RejectFile += FileCarrierOnRejectFile;
            FileCarrier.DepartingFile += FileCarrierOnDepartingFile;
            FileCarrier.CompleteFile += FileCarrierOnCompleteFile;
        }

        void UnsignEvents()
        {
            FileCarrier.RejectFile -= FileCarrierOnRejectFile;
            FileCarrier.DepartingFile -= FileCarrierOnDepartingFile;
            FileCarrier.CompleteFile -= FileCarrierOnCompleteFile;
        }

        private void FileCarrierOnCompleteFile(FileOperation fo)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
                Complete();
        }

        private void FileCarrierOnDepartingFile(FileOperation fo)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
                Dispatcher.Invoke(new Action(() =>
                    {
                        ProgressBarControl.Value++;
                    }));
        }

        private void FileCarrierOnRejectFile(FileOperation fo)
        {
            if (_fo.Messages[0].File.TransactionId == fo.Messages[0].File.TransactionId)
                Reject();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.RejectReceiving(_fo);
            Reject();
        }

        private void Complete()
        {
            UnsignEvents();
            Dispatcher.Invoke(new Action(()=>
                {
                    RequestPanel.Visibility = Visibility.Collapsed;
                    CompletePanel.Visibility = Visibility.Visible;
                }));
        }

        //ToImplement
        private void Reject()
        {
            UnsignEvents();
            Dispatcher.Invoke(new Action(() =>
            {
                RequestPanel.Visibility = Visibility.Collapsed;
                RejectPanel.Visibility = Visibility.Visible;
            }));
        }
    }
}
