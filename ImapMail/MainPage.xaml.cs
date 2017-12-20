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

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public ObservableCollection<MailHeader> MailHeaderList { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            HandlePreferences();

            HandleMail();
      
        }

        /// <summary>
        /// Handles user preferences
        /// </summary>
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
            MailHeaderList = MailHandler.GetMailHeaders();
            MimeMessage mail = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            SetContentToWebView(mail);
        }

        public void RefreshMail()
        {
            if (MailHeaderList != null)
                MailHeaderList.Clear();
            //MailHandler.Login();
            MailHeaderList = MailHandler.GetMailHeaders();

            Bindings.Update();              //Updates the ListView data
            MimeMessage mail = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            SetContentToWebView(mail);
        }

        /// <summary>
        /// Handles event when user clicks the listview - sets the clicked mail´s content to the webview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            MailHeader msg = (MailHeader)e.ClickedItem;

            MimeMessage mail = MailHandler.GetSpecificMail(msg.UniqueId);

            SetContentToWebView(mail);          
          
        }

        /// <summary>
        /// Sets mail content (html or text depending on what´s available) to the webview
        /// </summary>
        /// <param name="mail"></param>
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

        /// <summary>
        /// Handles clicks on the appbar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonRefresh)
                RefreshMail();

            if (e.OriginalSource == AppBarButtonSettings)
                Debug.WriteLine("Open settings page!");

            if (e.OriginalSource == AppBarButtonCompose)
                //Frame.Navigate(typeof(CreateMailPage));
                OpenCreateMailPage();

        
        }

        /// <summary>
        /// Opens CreateMailPage
        /// </summary>
        public async void OpenCreateMailPage()
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(CreateMailPage), null);
                Window.Current.Content = frame;
                Window.Current.Activate();         
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);

        }
    
    }
}
