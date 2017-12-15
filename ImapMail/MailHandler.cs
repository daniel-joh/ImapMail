using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit;
using MimeKit;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace ImapMail
{
    public class MailHandler
    {
        private static ImapClient client;
        private static string user = "USER";
        private static string password = "PASSWORD";


        public static ObservableCollection<MessageSummary> GetMailSummaries()
        {

            ObservableCollection<MessageSummary> summaryList;

            using (client = new ImapClient())
            {
                //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("imap.gmail.com", 993, true);

                //client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate(user, password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                //Debug.WriteLine("Total messages: {0}", inbox.Count);

                summaryList = new ObservableCollection<MessageSummary>();

                var summaryItems = inbox.Fetch(0, -1, MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId).ToList();

                //Sorts messages in reverse arrival order
                var orderBy = new[] { MailKit.Search.OrderBy.ReverseArrival };
                MessageSorter.Sort(summaryItems, orderBy);

                foreach (MessageSummary summary in summaryItems)
                {
                    summaryList.Add(summary);

                }

                //client.Disconnect(true);

            }

            return summaryList;
        }

        public static MimeMessage GetSpecificMail(UniqueId uid)
        {
            MimeMessage returnMail;

            using (client = new ImapClient())
            {
                //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect("imap.gmail.com", 993, true);

                //client.AuthenticationMechanisms.Remove("XOAUTH2");

                client.Authenticate(user, password);

                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                returnMail = inbox.GetMessage(uid);

                //client.Disconnect(true);
            }

            return returnMail;

        }

        
    }
}



