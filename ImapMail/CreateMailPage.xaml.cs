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

            for (int i = 0; i < 5; i++)
                Message.Text += Environment.NewLine;

            MailHandler.attachedFiles = new Dictionary<string, byte[]>();
        }

         
        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource == AppBarButtonSendMail)
            {
                Debug.WriteLine("Send mail clicked");
            
                MailHandler.SendMail(From.Text, To.Text, Subject.Text, Message.Text);
            }
            else if (e.OriginalSource == AppBarButtonAttachFile)
            {
                Debug.WriteLine("Attach file clicked");
                AttachFile();
            }
       
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
                byte[] byteArray= await HelperUtils.GetBytesAsync(file);
                MailHandler.attachedFiles.Add(file.Name, byteArray);   
                
            }
           

        }

      

    }
}
