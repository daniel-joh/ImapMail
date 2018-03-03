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
        private ObservableCollection<MailHeader> MailHeaderList { get; set; }
        private MimeMessage CurrentMessage { get; set; }

        private ObservableCollection<AttachedFile> AttachedFileList { get; set; }
        private int SelectedIndex { get; set; }

        private bool isGettingMailSuccess { get; set; }

        private ContentDialog errorDialog;

        public MainPage()
        {
            this.InitializeComponent();

            HelperUtils.LoadSettings();             //Loads mail settings from local settings to MailHandler

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);

            isGettingMailSuccess = HandleMail();

            //If fetching mail went OK
            if (isGettingMailSuccess)
            {
                AttachedFileList = new ObservableCollection<AttachedFile>();

                AttachedFilesListView.ItemsSource = AttachedFileList;
            }

        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            //If fetching mail failed, display error dialog
            if (!isGettingMailSuccess)
            {
                DisplayErrorDialog("Mail error", "No mail found. Check your mail settings!");                           
            }          
        }

        /// <summary>
        /// Logs in to mail server, gets mail summaries (headers) and sets the first mail´s content to the 
        /// webview and listview (attachments)
        /// </summary>
        /// <returns></returns>
        public bool HandleMail()
        {
            MailHandler.Login();

            ObservableCollection<MailHeader> tempMailList = MailHandler.GetMailHeaders(null);
            if (tempMailList==null)
            {              
                return false;
            }
            else
            {
                MailHeaderList = tempMailList;

                Bindings.Update();

                CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
                MailListView.SelectedIndex = 0;
                HandleAttachmentsAsync(CurrentMessage);
                SetContent(CurrentMessage);
            }

            return true;
        }

        /// <summary>
        /// Updates mail data
        /// </summary>
        public void RefreshMail()
        {
            if (AttachedFileList != null)
                AttachedFileList.Clear();

            ObservableCollection<MailHeader> tempMailList=MailHandler.GetMailHeaders(asb.Text);

            //If no mail was found on server, display error dialog
            if (tempMailList==null)
            {
                DisplayErrorDialog("Mail error", "No mail found. Try again!");
            }
            //If mail was found, replace MailHeaderList with the list with new mail and update UI
            else
            {
                if (MailHeaderList != null)
                    MailHeaderList.Clear();

                MailHeaderList = tempMailList;

                Bindings.Update();              //Updates the ListView data

                CurrentMessage = MailHandler.GetSpecificMail(MailHeaderList[0].UniqueId);
                MailListView.SelectedIndex = 0;
                HandleAttachmentsAsync(CurrentMessage);
                SetContent(CurrentMessage);
            }        
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
            errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "Ok"
            };

            errorDialog.Closed += ErrorDialog_Closed;
            
            ContentDialogResult result = await errorDialog.ShowAsync();          
        }

        /// <summary>
        /// Handles the dialog closed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ErrorDialog_Closed(ContentDialog sender, ContentDialogClosedEventArgs args)
        {
            //If the user´s search for mail returned null
            if (!MailHandler.isMailSearchSuccess && sender.Title.Equals("Mail error"))
            {
                errorDialog.Closed -= ErrorDialog_Closed;
                errorDialog = null;
                return;
            }

            //If no mail exists on server, navigate to SettingsPage so that the user can check settings
            if (sender.Title.Equals("Mail error"))
            {
                errorDialog.Closed -= ErrorDialog_Closed;
                errorDialog = null;
                Frame.Navigate(typeof(SettingsPage));
            }
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
        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonForwardMail)
            {
                MailHandler.ForwardFlag = true;
                MailHandler.Message = CurrentMessage;
                Frame.Navigate(typeof(CreateMailPage));
            }
            else if (e.OriginalSource == AppBarButtonReplyMail)
            {
                MailHandler.ReplyFlag = true;
                MailHandler.Message = CurrentMessage;
                Frame.Navigate(typeof(CreateMailPage));

            }
            else if (e.OriginalSource == AppBarButtonDeleteMail)
            {
                bool userChoosedDelete = await DisplayDeleteDialog();

                if (userChoosedDelete)
                {
                    bool success = MailHandler.DeleteMessage(CurrentMessage);

                    if (success)
                    {
                        RefreshMail();
                    }
                }
            }
            else if (e.OriginalSource == AppBarButtonRefreshMail)
            {
                RefreshMail();
            }
        }

        /// <summary>
        /// Shows a dialog for deleting a mail
        /// </summary>
        /// <returns></returns>
        private async Task<bool> DisplayDeleteDialog()
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Delete mail?",
                Content = "Do you want to delete this mail?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Delete"
            };

            ContentDialogResult result = await deleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                return true;
            }
            else
            {
                return false;
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

        /// <summary>
        /// If navigated to MainPage, clear the attached files list in MailHandler (used for sending files)
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (MailHandler.attachedFiles != null)
            {
                if (MailHandler.attachedFiles.Count > 0)
                {
                    MailHandler.attachedFiles.Clear();
                }
            }        
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            RefreshMail();
        }     

    }
}
