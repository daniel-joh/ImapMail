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

        
        private void CheckSmtpAuth_Checked(object sender, RoutedEventArgs e)
        {                                                        
                txtSmtpUser.IsEnabled = true;
                txtSmtpPassword.IsEnabled = true;            
        }
      
        private void CheckSmtpAuth_Unchecked(object sender, RoutedEventArgs e)
        {              
                txtSmtpUser.IsEnabled = false;
                txtSmtpPassword.IsEnabled = false;
        }

        /// <summary>
        /// Loads user settings from Windows.Storage.ApplicationData.Current.LocalSettings into UI
        /// </summary>
        public void LoadSettings()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            txtImapHost.Text = (string)localSettings.Values["imapHost"];
            txtImapPort.Text = (string)localSettings.Values["imapPort"];
            txtImapUser.Text = (string)localSettings.Values["imapUser"];
            txtImapPassword.Password = (string)localSettings.Values["imapPassword"];

            string tempImapSsl = (string)localSettings.Values["imapSsl"];
            if (tempImapSsl.Equals("true"))
            {
                checkImapSsl.IsChecked = true;
            }
            else
            {
                checkImapSsl.IsChecked = false;
            }

            string tempSmtpSsl = (string)localSettings.Values["smtpSsl"];
            if (tempSmtpSsl.Equals("true"))
            {
                checkSmtpSsl.IsChecked = true;
            }
            else 
            {
                checkSmtpSsl.IsChecked = false;
            }

            string tempSmtpAuth = (string)localSettings.Values["smtpAuth"];
            if (tempSmtpAuth.Equals("true"))
            {
               checkSmtpAuth.IsChecked = true;
            }
            else
            {
                checkSmtpAuth.IsChecked = false;
            }         

            txtSmtpHost.Text = (string)localSettings.Values["smtpHost"];
            txtSmtpPort.Text = (string)localSettings.Values["smtpPort"];
            txtSmtpUser.Text = (string)localSettings.Values["smtpUser"];
            txtSmtpPassword.Password = (string)localSettings.Values["smtpPassword"];
          
            //If smtp authentication is disabled, the txtSmtpUser and txtSmtpPassword textboxes should be disabled
            if (tempSmtpAuth.Equals("false"))
            {           
                txtSmtpUser.IsEnabled = false;
                txtSmtpPassword.IsEnabled = false;              
            }

        }

        /// <summary>
        /// Saves values from UI to Windows.Storage.ApplicationData.Current.LocalSettings and MailHandler properties
        /// </summary>
        public bool SaveSettings()
        {

            if (txtImapHost.Text.Equals(""))
            {             
                DisplayFailedSaveDialog("IMAP host empty.");
                txtImapHost.Focus(FocusState.Programmatic);
                return false;
            }

            if (txtImapPort.Text.Equals(""))
            {             
                DisplayFailedSaveDialog("IMAP port empty.");
                txtImapPort.Focus(FocusState.Programmatic);
                return false;
            }

            if (txtImapUser.Text.Equals(""))
            {
                DisplayFailedSaveDialog("IMAP user empty.");
                txtImapUser.Focus(FocusState.Programmatic);
                return false;
            }

            if (txtImapPassword.Password.Equals(""))
            {
                DisplayFailedSaveDialog("IMAP password empty.");
                txtImapPassword.Focus(FocusState.Programmatic);
                return false;
            }

            if (txtSmtpHost.Text.Equals(""))
            {
                DisplayFailedSaveDialog("SMTP host empty.");
                txtSmtpHost.Focus(FocusState.Programmatic);
                return false;
            }

            if (txtSmtpPort.Text.Equals(""))
            {
                DisplayFailedSaveDialog("SMTP port empty.");
                txtSmtpPort.Focus(FocusState.Programmatic);
                return false;
            }

            //If smtp authentication is required, check both smtp user and password
            if ((bool)checkSmtpAuth.IsChecked)
            {
                if (txtSmtpUser.Text.Equals(""))
                {
                    DisplayFailedSaveDialog("SMTP username empty.");
                    txtSmtpUser.Focus(FocusState.Programmatic);
                    return false;
                }

                if (txtSmtpPassword.Password.Equals(""))
                {
                    DisplayFailedSaveDialog("SMTP password empty.");
                    txtSmtpPassword.Focus(FocusState.Programmatic);
                    return false;
                }
            }

            /* Sets LocalSettings */
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["imapHost"] = txtImapHost.Text;
            localSettings.Values["imapPort"] = txtImapPort.Text;
            localSettings.Values["imapUser"] = txtImapUser.Text;
            localSettings.Values["imapPassword"] = txtImapPassword.Password;

            bool tempImapSsl = (bool)checkImapSsl.IsChecked;
            localSettings.Values["imapSsl"] = tempImapSsl.ToString().ToLower();

            localSettings.Values["smtpHost"] = txtSmtpHost.Text;
            localSettings.Values["smtpPort"] = txtSmtpPort.Text;

            bool tempSmtpAuth = (bool)checkSmtpAuth.IsChecked;
            localSettings.Values["smtpAuth"] = tempSmtpAuth.ToString().ToLower();

            localSettings.Values["smtpUser"] = txtSmtpUser.Text;
            localSettings.Values["smtpPassword"] = txtSmtpPassword.Password;

            bool tempSmtpSsl = (bool)checkSmtpSsl.IsChecked;
            localSettings.Values["smtpSsl"] = tempSmtpSsl.ToString().ToLower();

            /* Sets MailHandler properties */
            MailHandler.ImapHost = txtImapHost.Text;

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

            MailHandler.ImapUser = txtImapUser.Text;
            MailHandler.ImapPassword = txtImapPassword.Password;
            MailHandler.ImapUseSsl = (bool)checkImapSsl.IsChecked;
            MailHandler.SmtpHost = txtSmtpHost.Text;
            MailHandler.SmtpUser = txtSmtpUser.Text;
            MailHandler.SmtpPassword = txtSmtpPassword.Password;
            MailHandler.SmtpAuth = (bool)checkSmtpAuth.IsChecked;
            MailHandler.SmtpUseSsl = (bool)checkSmtpSsl.IsChecked;

            return true;

        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (!SaveSettings())
            {
                //DisplayFailedSaveDialog("test");
            }
            else
            {
                DisplaySaveSuccessDialog();

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

        private async void DisplayFailedSaveDialog(string message)
        {
            ContentDialog failedSaveDialog = new ContentDialog
            {
                Title = "Save failed",
                Content = message + " Try again!",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await failedSaveDialog.ShowAsync();
        }

        private async void DisplaySaveSuccessDialog()
        {
            ContentDialog saveSuccessDialog = new ContentDialog
            {
                Title = "Save was successful",
                Content = "Settings saved!",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await saveSuccessDialog.ShowAsync();
        }
    }
}
