using System;
using System.Collections.Generic;
using System.Diagnostics;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImapMail
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {

        public SettingsPage()
        {
            this.InitializeComponent();

            if (HelperUtils.AreSettingsAvailable())
            {
                LoadSettings();
            }
        }

        public void LoadSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
          
            txtImapHost.Text = (string)localSettings.Values["imapHost"];
            txtImapPort.Text = (string)localSettings.Values["imapPort"];
            txtImapUser.Text = (string)localSettings.Values["imapUser"];
            txtImapPassword.Text = (string)localSettings.Values["imapPassword"];

            //Need to set these values also..
            //checkImapSsl.Text = (string)localSettings.Values["imapSsl"];
            //checkSmtpSsl.Text = (string)localSettings.Values["smtpSsl"];
            //checkSmtpAuth.Text = (string)localSettings.Values["smtpAuth"];

            txtSmtpHost.Text = (string)localSettings.Values["smtpHost"];
            txtSmtpPort.Text = (string)localSettings.Values["smtpPort"];
            txtSmtpUser.Text = (string)localSettings.Values["smtpUser"];
            txtSmtpPassword.Text = (string)localSettings.Values["smtpPassword"];
     
        }

        /// <summary>
        /// Saves values from UI to Windows.Storage.ApplicationData.Current.LocalSettings and MailHandler properties
        /// </summary>
        public bool SaveSettings()
        {           

            if ((bool)!checkSmtpAuth.IsChecked)
            {
                //Note: Should disable these textboxes:

                //txtSmtpUser
                //txtSmtpPassword

            }

            if (txtImapHost.Text.Equals(""))
            {
                Debug.WriteLine("Imap host empty!");
                return false;
            }

            if (txtImapPort.Text.Equals(""))
            {
                Debug.WriteLine("Imap port empty!");
                return false;
            }

            if (txtImapUser.Text.Equals(""))
            {
                Debug.WriteLine("Imap user empty!");
                return false;
            }

            if (txtImapPassword.Text.Equals(""))
            {
                Debug.WriteLine("Imap password empty!");
                return false;
            }

            if (txtSmtpHost.Text.Equals(""))
            {
                Debug.WriteLine("Smtp host empty!");
                return false;
            }

            if (txtSmtpPort.Text.Equals(""))
            {
                Debug.WriteLine("Smtp port empty!");
                return false;
            }

            //If smtp authentication is required, check both smtp user and password
            if ((bool)checkSmtpAuth.IsChecked)
            {
                if (txtSmtpUser.Text.Equals(""))
                {
                    Debug.WriteLine("Smtp user empty!");
                    return false;
                }

                if (txtSmtpPassword.Text.Equals(""))
                {
                    Debug.WriteLine("Smtp password empty!");
                    return false;
                }
            }

            /* Sets LocalSettings */
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
               
            localSettings.Values["imapHost"] = txtImapHost.Text;
            localSettings.Values["imapPort"] = txtImapPort.Text;
            localSettings.Values["imapUser"] = txtImapUser.Text;
            localSettings.Values["imapPassword"] = txtImapPassword.Text;

            bool tempImapSsl=(bool)checkImapSsl.IsChecked;
            localSettings.Values["imapSsl"] = tempImapSsl.ToString().ToLower();

            localSettings.Values["smtpHost"] = txtSmtpHost.Text;
            localSettings.Values["smtpPort"] = txtSmtpPort.Text;

            bool tempSmtpAuth = (bool)checkSmtpAuth.IsChecked;
            localSettings.Values["smtpAuth"] = tempSmtpAuth.ToString().ToLower();

            localSettings.Values["smtpUser"] = txtSmtpUser.Text;
            localSettings.Values["smtpPassword"] = txtSmtpPassword.Text;
       
            bool tempSmtpSsl = (bool)checkSmtpSsl.IsChecked;
            localSettings.Values["smtpSsl"] = tempSmtpSsl.ToString().ToLower();

            /* Sets MailHandler properties */
            MailHandler.ImapHost= txtImapHost.Text;

            try
            {
                MailHandler.ImapPort = int.Parse(txtImapPort.Text);
                MailHandler.SmtpPort = int.Parse(txtSmtpPort.Text);
            }
            catch (FormatException e)
            {
                Debug.WriteLine("User entered incorrect value for port!");
                return false;
            }
          
            MailHandler.ImapUser= txtImapUser.Text;
            MailHandler.ImapPassword = txtImapPassword.Text;
            MailHandler.ImapUseSsl = (bool)checkImapSsl.IsChecked;               
            MailHandler.SmtpHost = txtSmtpHost.Text;                  
            MailHandler.SmtpUser = txtSmtpUser.Text;
            MailHandler.SmtpPassword = txtSmtpPassword.Text;
            MailHandler.SmtpAuth = (bool)checkSmtpAuth.IsChecked;
            MailHandler.SmtpUseSsl = (bool)checkSmtpSsl.IsChecked;

            return true;

        }

        //Dialogs need to be added..
        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!SaveSettings())
            {
               
                Debug.WriteLine("Save failed. Try again!");
            }
            else
            {
                Debug.WriteLine("Settings saved!!");

                Debug.WriteLine(MailHandler.ImapHost);
                Debug.WriteLine(MailHandler.ImapPort);
                Debug.WriteLine(MailHandler.ImapUser);
                Debug.WriteLine(MailHandler.ImapPassword);
                Debug.WriteLine(MailHandler.ImapUseSsl);

                Debug.WriteLine(MailHandler.SmtpHost);
                Debug.WriteLine(MailHandler.SmtpPort);
                Debug.WriteLine(MailHandler.SmtpUser);
                Debug.WriteLine(MailHandler.SmtpPassword);
                Debug.WriteLine(MailHandler.SmtpAuth);
                Debug.WriteLine(MailHandler.SmtpUseSsl);
            }

                  
        }
    }
}
