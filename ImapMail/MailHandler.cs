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
        internal static string User { get; set; }
        internal static string Password { get; set; }
        internal static bool LoggedIn {get; set; }

        public static void Login()
        {
            if (client == null)
            {
                client = new ImapClient();

                if (client.IsConnected == false && client.IsAuthenticated == false)
                {

                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect("imap.gmail.com", 993, true);

                    //client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(User, Password);

                    LoggedIn = true;
                }

            }

        }

        public static void Logout()
        {
            if (client != null)
            {
                if (client.IsConnected == true)
                {
                    client.Disconnect(true);
                    client = null;
                    LoggedIn = false;
                }
            }

        }


        public static ObservableCollection<MessageSummary> GetMailSummaries()
        {
            ObservableCollection<MessageSummary> summaryList = new ObservableCollection<MessageSummary>();

            if (LoggedIn == true)
            {              
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadOnly);

                //Debug.WriteLine("Total messages: {0}", inbox.Count);
             
                var summaryItems = inbox.Fetch(0, -1, MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId).ToList();

                //Sorts messagesummaries in reverse arrival order
                var orderBy = new[] { MailKit.Search.OrderBy.ReverseArrival };
                MessageSorter.Sort(summaryItems, orderBy);

                foreach (MessageSummary summary in summaryItems)
                {
                    summaryList.Add(summary);

                }
              
            }
            else
            {
                Login();
                GetMailSummaries();
            }

            return summaryList;
        }

        public static MimeMessage GetSpecificMail(UniqueId uid)
        {
            MimeMessage mail=null;

            if (LoggedIn == true)
            {
                var inbox = client.Inbox;

                if (inbox.IsOpen == false)
                {
                    inbox.Open(FolderAccess.ReadOnly);
                }

                mail = inbox.GetMessage(uid);             
            }
            else
            {
                Login();
                GetSpecificMail(uid);
            }

            return mail;

        }

    }
}



