using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using MailKit;
using MimeKit;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateMailPage : Page
    {
        public CreateMailPage()
        {
            this.InitializeComponent();

            Message.Text=Environment.NewLine+Environment.NewLine+Environment.NewLine +Environment.NewLine+
            Environment.NewLine + Environment.NewLine;
        }

        private void SendMail_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Send mail button clicked");
                     
            MailHandler.SendMail(From.ToString(), To.ToString(), Subject.ToString(), Message.ToString());
        }

        private void CancelSend_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Cancel button clicked!");
            Frame.Navigate(typeof(MainPage));
        }

       
    }
}
