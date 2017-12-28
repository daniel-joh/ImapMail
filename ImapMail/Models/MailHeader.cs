using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImapMail
{
    public class MailHeader
    {
        public string From { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public UniqueId UniqueId { get; set; }

    }
}
