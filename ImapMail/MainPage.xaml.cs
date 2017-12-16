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

        public MainPage()
        {
            this.InitializeComponent();

            HandlePreferences();

            HandleMail();

        }

        public void HandlePreferences()
        {
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.StorageFolder localFolder =
                Windows.Storage.ApplicationData.Current.LocalFolder;

            MailHandler.User = (string)localSettings.Values["user"];
            MailHandler.Password = (string)localSettings.Values["password"];

        }

        /// <summary>
        /// Logs in to mail server, gets mail summaries (headers) and sets the first mail´s content to the webview
        /// </summary>
        public void HandleMail()
        {
            MailHandler.Login();
            SummaryList = MailHandler.GetMailSummaries();
            MimeMessage mail = MailHandler.GetSpecificMail(SummaryList[0].UniqueId);
            SetContentToWebView(mail);
        }

        /// <summary>
        /// Handles event when user clicks the listview - sets the clicked mail´s content to the webview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            MessageSummary msg = (MessageSummary)e.ClickedItem;

            MimeMessage mail = MailHandler.GetSpecificMail(msg.UniqueId);

            SetContentToWebView(mail);
          
        }

        public void SetContentToWebView(MimeMessage mail)
        {
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
