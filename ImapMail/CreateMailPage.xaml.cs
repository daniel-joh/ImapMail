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
using System.Threading.Tasks;
using MimeKit;

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

            this.Loaded += new RoutedEventHandler(CreateMailPage_Loaded);

            for (int i = 0; i < 9; i++)
                Message.Text += Environment.NewLine;

            MailHandler.attachedFiles = new Dictionary<string, byte[]>();

        }

        void CreateMailPage_Loaded(object sender, RoutedEventArgs e)
        {
            InitUi();

        }

        private void InitUi()
        {
            From.Text = MailHandler.UserEmail;

            //If the user has clicked Reply
            if (MailHandler.ReplyFlag == true)
            {
                if (MailHandler.Message != null)
                {
                    MailHandler.Message = MailHandler.CreateReply(MailHandler.Message);
                }

                var message = MailHandler.Message;

                //Adds From text
                List<MailboxAddress> tempList = message.From.Mailboxes.ToList();
                From.Text = tempList[0].Address;

                //Adds To text
                string tempTo = message.To.ToString();
                int startIndex = tempTo.IndexOf('<');
                int endIndex = tempTo.LastIndexOf('>');
                int length = endIndex - startIndex - 1;

                //If '<' and '>' was found, set the email address to the To textbox without those characters, 
                //otherwise just set the To textbox
                if (startIndex != -1 || length != -1)
                {
                    //email==string between "<" and ">" only 
                    string email = tempTo.Substring(startIndex + 1, length);
                    To.Text = email;
                }
                else
                {
                    To.Text = tempTo;
                }

                Subject.Text = message.Subject;

                Message.Text = message.TextBody;

            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonSendMail)
            {
                HandleSendingMail();
            }
            else if (e.OriginalSource == AppBarButtonAttachFile)
            {
                AttachFile();
            }

        }

        private void HandleSendingMail()
        {
            //Add success/failure code
            if (MailHandler.ReplyFlag == true)
                MailHandler.SendMail(MailHandler.Message, From.Text, To.Text, Subject.Text, Message.Text);
            else
                MailHandler.SendMail(From.Text, To.Text, Subject.Text, Message.Text);
        }

        private async void DisplaySendSuccessDialog()
        {
            ContentDialog sendSuccessDialog = new ContentDialog
            {
                Title = "Send was successful",
                Content = "Mail sent!",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await sendSuccessDialog.ShowAsync();
        }

        private async void DisplayFailedSendDialog(string message)
        {
            ContentDialog failedSendDialog = new ContentDialog
            {
                Title = "Send failed",
                Content = message + " Try again!",
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await failedSendDialog.ShowAsync();
        }

        public async void AttachFile()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                byte[] byteArray = await HelperUtils.GetBytesAsync(file);
                MailHandler.attachedFiles.Add(file.Name, byteArray);

            }

        }

    }
}
