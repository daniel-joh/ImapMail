﻿using System;
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
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace ImapMail
{
    /// <summary>
    /// MainPage class
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ObservableCollection<MailHeader> MailHeaderList { get; set; }
        MimeMessage CurrentMessage { get; set; }

        ObservableCollection<AttachedFile> AttachedFileList { get; set; }
        int SelectedIndex { get; set; }

        public MainPage()
        {
            this.InitializeComponent();

            HelperUtils.LoadSettings();             //Loads mail settings from local settings to MailHandler

            HandleMail();

            AttachedFileList = new ObservableCollection<AttachedFile>();

            this.AttachedFilesListView.ItemsSource = AttachedFileList;

        }

        /// <summary>
        /// Logs in to mail server, gets mail summaries (headers) and sets the first mail´s content to the 
        /// webview and listview (attachments)
        /// </summary>
        public void HandleMail()
        {
            MailHandler.Login();
            MailHeaderList = MailHandler.GetMailHeaders();
            Bindings.Update();

            CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            HandleAttachmentsAsync(CurrentMessage);
            SetContent(CurrentMessage);
        }

        /// <summary>
        /// Updates mail data
        /// </summary>
        public void RefreshMail()
        {
            if (MailHeaderList != null)
                MailHeaderList.Clear();

            if (AttachedFileList != null)
                AttachedFileList.Clear();

            MailHeaderList = MailHandler.GetMailHeaders();
            Bindings.Update();              //Updates the ListView data

            CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
            HandleAttachmentsAsync(CurrentMessage);
            SetContent(CurrentMessage);
        }

        /// <summary>
        /// Handles event when user clicks the listview - sets the clicked mail´s content to the webview&listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MailListView_ItemClicked(object sender, ItemClickEventArgs e)
        {
            AttachedFileList.Clear();

            MailHeader msg = (MailHeader)e.ClickedItem;

            CurrentMessage = MailHandler.GetSpecificMail(msg.UniqueId);
            HandleAttachmentsAsync(CurrentMessage);
            SetContent(CurrentMessage);
        }

        /// <summary>
        /// Converts attachments to AttachedFile(s)
        /// </summary>
        /// <param name="message"></param>
        private async void HandleAttachmentsAsync(MimeMessage message)
        {
            byte[] byteArray;

            //If Attachments is not empty
            if (message.Attachments.Count() != 0)
            {
                AttachedFilesListView.Visibility = Visibility.Visible;

                foreach (var attachment in message.Attachments)
                {
                    var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                    byteArray = HelperUtils.ConvertAttachmentToByteArray(attachment);

                    if (byteArray == null)
                    {
                        DisplayErrorDialog("Attachment error", "Error displaying attachments");
                        return;
                    }

                    //Converts byte[] to StorageFile
                    StorageFile storageFile = await HelperUtils.ConvertsToStorageFileAsync(byteArray, fileName);

                    if (storageFile != null)
                    {
                        //Creates new AttachedFile and adds it to AttachedFileList
                        AttachedFileList.Add(new AttachedFile
                        {
                            FileName = storageFile.Name,
                            Thumbnail = await storageFile.GetThumbnailAsync(ThumbnailMode.ListView)
                        });
                    }

                }
            }
            else
            {
                //Makes the listview invisible, since there are no attachments to show
                AttachedFilesListView.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles right taps on the listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout(sender as FrameworkElement);

            SelectedIndex = AttachedFilesListView.SelectedIndex;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            bool success = await SaveAttachment();

            if (!success)
            {
                DisplayErrorDialog("Save file failed", "Save failed. Try again!");
            }
        }

        /// <summary>
        /// Saves the selected attachment to file
        /// </summary>
        private async Task<bool> SaveAttachment()
        {
            if (SelectedIndex != -1)
            {
                byte[] byteArray;

                List<MimeEntity> list = CurrentMessage.Attachments.ToList();
                var attachment = list[SelectedIndex];

                var fileName = attachment.ContentDisposition?.FileName ?? attachment.ContentType.Name;

                //Converts the attachment to a byte array
                byteArray = HelperUtils.ConvertAttachmentToByteArray(attachment);

                if (byteArray == null)
                    return false;

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;

                //Gets file extension
                int index = fileName.LastIndexOf(".");
                string fileType = fileName.Substring(index);

                savePicker.FileTypeChoices.Add(fileType.Substring(1), new List<string>() { fileType });

                savePicker.DefaultFileExtension = fileType;
                savePicker.SuggestedFileName = fileName;

                //Lets the user choose how to save the file
                StorageFile file = await savePicker.PickSaveFileAsync();

                if (file != null)
                {
                    Windows.Storage.CachedFileManager.DeferUpdates(file);

                    //Writes byte array to Storagefile
                    await Windows.Storage.FileIO.WriteBytesAsync(file, byteArray);

                    Windows.Storage.Provider.FileUpdateStatus status =
                        await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);

                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("File " + file.Name + " couldn't be saved");
                        return false;
                    }
                }
                else
                {
                    Debug.WriteLine("Operation cancelled");
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Displays a dialog when an operation has failed
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="message"></param>
        private async void DisplayErrorDialog(string title, string message)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await errorDialog.ShowAsync();
        }

        /// <summary>
        /// Launches the attached file
        /// </summary>
        /// <param name="fileName"></param>
        private async void LaunchAttachedFile(string fileName)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;

            StorageFile file = await storageFolder.GetFileAsync(fileName);

            if (file != null)
            {
                //Sets launching options
                var options = new Windows.System.LauncherOptions();
                options.DisplayApplicationPicker = true;

                //Launches the attached file
                bool success = await Windows.System.Launcher.LaunchFileAsync(file, options);

                if (!success)
                {
                    DisplayErrorDialog("Launch file failed", "Launch file failed. Try again!");
                }
            }
            else
            {
                DisplayErrorDialog("Launch file failed", "File not available. Try again!");
            }
        }

        /// <summary>
        /// Launches the doubleclicked file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AttachedFilesListView_DoubleClick(object sender, DoubleTappedRoutedEventArgs e)
        {
            var attachedFile = (AttachedFile)AttachedFilesListView.SelectedItem;

            LaunchAttachedFile(attachedFile.FileName);
        }

        /// <summary>
        /// Handles clicks on the AppBars
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonForwardMail)
            {
                Debug.WriteLine("Forward mail clicked");
            }
            else if (e.OriginalSource == AppBarButtonReplyMail)
            {
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
        /// Sets mail content to the webview and textblocks
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

        //If navigated to MainPage, clear the attached files list in MailHandler (used for sending files)
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MailHandler.attachedFiles != null)
            {
                if (MailHandler.attachedFiles.Count > 0)
                {
                    MailHandler.attachedFiles.Clear();
                }
            }

            /*
            if (MailHeaderList != null)
                MailHeaderList.Clear();

            if (AttachedFileList != null)
                AttachedFileList.Clear();*/
        }

    }
}
