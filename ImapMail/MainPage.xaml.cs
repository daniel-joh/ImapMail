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
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using System.Globalization;

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<MailHeader> MailHeaderList { get; set; }
        public MimeMessage CurrentMessage { get; set; }


        public MainPage()
        {
            this.InitializeComponent();

            LoadSettings();

            HandleMail();

        }

        public void LoadSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            MailHandler.UserEmail = (string)localSettings.Values["userEmail"];

            MailHandler.ImapHost = (string)localSettings.Values["imapHost"];

            string tempImapPort = (string)localSettings.Values["imapPort"];
            MailHandler.ImapPort = int.Parse(tempImapPort);

            MailHandler.ImapUser = (string)localSettings.Values["imapUser"];
            MailHandler.ImapPassword = (string)localSettings.Values["imapPassword"];

            string tempImapSsl = (string)localSettings.Values["imapSsl"];
            MailHandler.ImapUseSsl = bool.Parse(tempImapSsl);

            MailHandler.SmtpHost = (string)localSettings.Values["smtpHost"];

            string tempSmtpPort = (string)localSettings.Values["smtpPort"];
            MailHandler.SmtpPort = int.Parse(tempSmtpPort);

            MailHandler.SmtpUser = (string)localSettings.Values["smtpUser"];
            MailHandler.SmtpPassword = (string)localSettings.Values["smtpPassword"];

            string tempSmtpSsl = (string)localSettings.Values["smtpSsl"];
            MailHandler.SmtpUseSsl = bool.Parse(tempSmtpSsl);

            string tempSmtpAuth = (string)localSettings.Values["smtpAuth"];
            MailHandler.SmtpAuth = bool.Parse(tempSmtpAuth);

        }

        /// <summary>
        /// Logs in to mail server, gets mail summaries (headers) and sets the first mail´s content to the webview
        /// </summary>
        public void HandleMail()
        {
            MailHandler.Login();
            MailHeaderList = MailHandler.GetMailHeaders();
            Bindings.Update();
            CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            SetContent(CurrentMessage);

        }

        public void RefreshMail()
        {
            if (MailHeaderList != null)
                MailHeaderList.Clear();

            MailHeaderList = MailHandler.GetMailHeaders();

            Bindings.Update();              //Updates the ListView data
            CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            SetContent(CurrentMessage);
        }

        /// <summary>
        /// Handles event when user clicks the listview - sets the clicked mail´s content to the webview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            MailHeader msg = (MailHeader)e.ClickedItem;

            CurrentMessage = MailHandler.GetSpecificMail(msg.UniqueId);

            SetContent(CurrentMessage);

        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonForwardMail)
            {
                Debug.WriteLine("Forward mail clicked");

            }
            else if (e.OriginalSource == AppBarButtonReplyMail)
            {
                Debug.WriteLine("Reply mail clicked");

                MailHandler.ReplyFlag = true;
                MailHandler.Message = CurrentMessage;
                Frame.Navigate(typeof(CreateMailPage));

            }
            else if (e.OriginalSource == AppBarButtonDeleteMail)
            {
                Debug.WriteLine("Delete mail clicked");

            }
            else if (e.OriginalSource == AppBarButtonRefreshMail)
            {
                RefreshMail();
            }

        }

        /// <summary>
        /// Sets mail content (html or text depending on what´s available) to the webview
        /// </summary>
        /// <param name="mail"></param>
        public void SetContent(MimeMessage mail)
        {
            if (mail.HtmlBody != null)
            {
                webView.NavigateToString(mail.HtmlBody);
            }
            else if (mail.TextBody != null)
            {
                webView.NavigateToString(mail.TextBody);
            }

            if (mail.Subject != null)
                MessageSubject.Text = mail.Subject;

            string format = "yyyy-MM-dd HH:mm";
            MessageDate.Text = mail.Date.ToString(format, new CultureInfo("sv-SE"));

            var addressList = mail.From;
            MessageFrom.Text = addressList[0].ToString();

        }

    }
}
