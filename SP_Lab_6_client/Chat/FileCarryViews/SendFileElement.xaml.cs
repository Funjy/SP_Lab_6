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
    public partial class SendFileElement : UserControl
    {
        private readonly FileOperation _fo;

        private SendFileElement()
        {
            InitializeComponent();
        }

        public SendFileElement(FileOperation fo)
        {
            _fo = fo;
            FileNameBox.Text = fo.Messages[0].File.FileName;
            ProgressBarControl.Maximum = fo.Messages[0].File.QueueLength;
            FileCarrier.RejectFile += FileCarrierOnRejectFile;
            FileCarrier.DepartingFile += FileCarrierOnDepartingFile;
        }

        private void FileCarrierOnDepartingFile(FileOperation fo)
        {
            ProgressBarControl.Value++;
        }

        private void FileCarrierOnRejectFile(FileOperation fo)
        {
            Reject();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            FileCarrier.RejectReceiving(_fo);
            Reject();
        }

        //ToImplement
        private void Reject()
        {
            
        }
    }
}
