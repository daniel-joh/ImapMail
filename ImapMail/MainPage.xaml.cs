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
using System.Collections.ObjectModel;
using MimeKit;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<MessageSummary> SummaryList { get; set; }

        public ObservableCollection<MimeMessage> MailList { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            SummaryList = MailHandler.GetMailSummaries();
            MimeMessage mail = MailHandler.GetSpecificMail(SummaryList[0].UniqueId);

            if (mail.HtmlBody != null)
            {
                webView.NavigateToString(mail.HtmlBody);
            }
            else if (mail.TextBody != null)
            {
                webView.NavigateToString(mail.TextBody);
            }

        }

        private void MailListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            MessageSummary msg = (MessageSummary)e.ClickedItem;

            Debug.WriteLine("Unique id: " + msg.UniqueId);

            MimeMessage mail = MailHandler.GetSpecificMail(msg.UniqueId);

            if (mail.HtmlBody != null)
            {
                webView.NavigateToString(mail.HtmlBody);
            }
            else if (mail.TextBody != null)
            {
                webView.NavigateToString(mail.TextBody);
            }


        }
    }
}
