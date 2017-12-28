using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;

namespace ImapMail
{
    public class AttachedFile
    {
        public StorageItemThumbnail Thumbnail { get; set; }
        public string FileName { get; set; }
    }
}
