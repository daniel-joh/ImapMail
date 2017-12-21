using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static async Task<byte[]> GetBytesAsync(Windows.Storage.StorageFile file)
        {
            byte[] fileBytes = null;
            if (file == null) return null;
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
