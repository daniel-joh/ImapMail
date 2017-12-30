using MimeKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ImapMail
{
    static class HelperUtils
    {      
        /// <summary>
        /// Simple method to determine if user settings has been set
        /// </summary>
        /// <returns></returns>
        public static bool AreSettingsAvailable()
        {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            //Needs only to check one value
            if (localSettings.Values["imapHost"] != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Loads mail settings from local settings to MailHandler
        /// </summary>
        public static void LoadSettings()
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

        public static byte[] ConvertAttachmentToByteArray(MimeEntity attachment)
        {
            if (attachment != null)
            {
                byte[] byteArray;

                MemoryStream stream = new MemoryStream();

                //Converts attachment to a stream                              
                if (attachment is MessagePart)
                {
                    var messagePart = (MessagePart)attachment;

                    //Writes the message to the stream
                    messagePart.Message.WriteTo(stream);
                }
                else
                {
                    var part = (MimePart)attachment;

                    //Decodes the MimePart to the stream
                    part.ContentObject.DecodeTo(stream);
                }

                //Converts the stream to a byte[]
                byteArray = stream.ToArray();

                return byteArray;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Converts byte array to StorageFile async
        /// </summary>
        /// <param name="byteArray"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<StorageFile> ConvertsToStorageFileAsync(byte[] byteArray, string fileName)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;

            try
            {
                storageFile = await storageFolder.GetFileAsync(fileName);

                await FileIO.WriteBytesAsync(storageFile, byteArray);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return storageFile;
        }

        public static async Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;

            if (file == null)
                return null;

            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new Windows.Storage.Streams.DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }
            return fileBytes;
        }
    }

}
