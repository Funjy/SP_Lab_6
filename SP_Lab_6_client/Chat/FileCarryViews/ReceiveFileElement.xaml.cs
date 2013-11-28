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
    /// Логика взаимодействия для ReceiveFileElement.xaml
    /// </summary>
    public partial class ReceiveFileElement : IFileCarryView
    {
        private readonly FileOperation _fo;

        private ReceiveFileElement()
        {
            InitializeComponent();
        }

        public ReceiveFileElement(FileOperation fo) : this()
        {
            _fo = fo;
            FileNameBox.Text = _fo.Messages[0].File.FileName;
            ProgressBarControl.Maximum = fo.Messages[0].File.QueueLength;
            FileCarrier.RejectFile += FileCarrierOnRejectFile;
            FileCarrier.IncomingFile += FileCarrierOnIncomingFile;
        }
        
        private void FileCarrierOnRejectFile(FileOperation fo)
        {
            Reject();
        }

        private void AcceptButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.AcceptReceiving(_fo);
            AcceptButton.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.RejectReceiving(_fo);
            Reject();
        }

        private void FileCarrierOnIncomingFile(FileOperation fo)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                ProgressBarControl.Value++;
            }));
            
        }

        //ToImplement
        private void Reject()
        {
            
        }

    }
}
