using System;
using System.Collections.Generic;
using System.Linq;
using MailKit.Net.Imap;
using MailKit;
using MimeKit;
using System.Diagnostics;
using System.Collections.ObjectModel;
using MailKit.Net.Smtp;
using System.Globalization;

namespace ImapMail
{
    public class MailHandler
    {
        private static ImapClient client;

        internal static bool LoggedIn { get; set; }
        
        /* IMAP properties */
        internal static string ImapHost { get; set; }
        internal static int ImapPort { get; set; } 
        internal static bool ImapUseSsl { get; set; } 
        internal static string ImapUser { get; set; }
        internal static string ImapPassword { get; set; }

        /* SMTP properties */
        internal static string SmtpHost { get; set; } 
        internal static int SmtpPort { get; set; } 
        internal static bool SmtpUseSsl { get; set; } 
        internal static bool SmtpAuth { get; set; } 
        internal static string SmtpUser { get; set; }
        internal static string SmtpPassword { get; set; }

        //Dictionary for attached files
        internal static Dictionary<string, byte[]> attachedFiles { get; set; }

        /// <summary>
        /// Logs in to imap mail server
        /// </summary>
        public static void Login()
        {
            if (client == null)
            {
                client = new ImapClient();

                if (client.IsConnected == false && client.IsAuthenticated == false)
                {
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    client.Connect(ImapHost, ImapPort, ImapUseSsl);

                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(ImapUser, ImapPassword);

                    LoggedIn = true;
                }
            }

        }

        /// <summary>
        /// Logs out from imap mail server
        /// </summary>
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

        /// <summary>
        /// Gets mail headers
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<MailHeader> GetMailHeaders()
        {
            ObservableCollection<MailHeader> headerList = new ObservableCollection<MailHeader>();

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
                    MailHeader mailHeader=ConvertMessageSummary(summary);
                    headerList.Add(mailHeader);
                }
              
            }
            else
            {
                Login();
                GetMailHeaders();
            }

            return headerList;
        }

        /// <summary>
        /// Converts (parts of) MessageSummary to MailHeader 
        /// </summary>
        /// <param name="msgSum"></param>
        /// <returns></returns>
        public static MailHeader ConvertMessageSummary(MessageSummary msgSum)
        {
            MailHeader mailHeader = new MailHeader();

            mailHeader.Subject = msgSum.Envelope.Subject;

            InternetAddressList fromList = msgSum.Envelope.From;
            string fromString = "";

            //For all InternetAddresses in InternetAddressList, add display name (if it exists) 
            //to the fromString, otherwise add email address
            foreach(var from in fromList)
            {                           
                if (from.Name==null || from.Name.Equals(""))
                {
                    if (fromList.Count > 1)
                    {
                        fromString = from.ToString() + "," + fromString;
                    }
                    else
                        fromString = from.ToString();
                }
                else
                {
                    if (fromList.Count > 1)
                    {
                        fromString = from.Name + "," + fromString;
                    }
                    else
                        fromString = from.Name;              
                }
              
            }

            mailHeader.From = fromString;

            string date="";

            //If the message´s date is today - set date string to Hour+Minute only. Otherwise set to full date&time
            if (msgSum.Date.Day == DateTimeOffset.Now.Day)
            {                    
                date = msgSum.Date.Hour + ":" + msgSum.Date.Minute;              
            }
            else
            {
                string format = "yyyy-MM-dd HH:mm";

                //Formats the date to swedish culture
                date = msgSum.Date.ToString(format, new CultureInfo("sv-SE"));
           
            }

            mailHeader.Date = date;
            mailHeader.UniqueId = msgSum.UniqueId;

            return mailHeader;
        }

        /// <summary>
        /// Gets a specific mail
        /// </summary>
        /// <param name="uid">UniqueId</param>
        /// <returns>MimeMessage</returns>
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

        /// <summary>
        /// Connects to smtp server and sends mail
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="subject"></param>
        /// <param name="bodyText"></param>
        public static void SendMail(string from, string to, string subject, string bodyText)
        {
            var message = new MimeMessage();
      
            message.From.Add(new MailboxAddress(from));
            message.To.Add(new MailboxAddress(to));
            message.Subject = subject;

            var builder = new BodyBuilder();

            builder.TextBody = bodyText;
           
            if (attachedFiles.Count != 0)
            {
                //Adds all attached files to the message
                foreach (var file in attachedFiles)
                {
                    builder.Attachments.Add(file.Key, file.Value);
                    Debug.WriteLine(file.Key + file.Value + "\n");
                }
            }

            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {                  
                client.Connect(SmtpHost, SmtpPort, SmtpUseSsl);

                client.AuthenticationMechanisms.Remove("XOAUTH2");

                //If smtp authentication is required
                if (SmtpAuth == true)
                {
                    client.Authenticate(SmtpUser, SmtpPassword);
                }
                
                client.Send(message);
                client.Disconnect(true);

                Debug.WriteLine(" Mail sent!");
            }
        }

    }
}



