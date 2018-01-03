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
using System.IO;
using System.Threading;
using MailKit.Search;
using MimeKit.Utils;

namespace ImapMail
{
    public class MailHandler
    {
        private static ImapClient client;

        internal static bool LoggedIn { get; set; }

        internal static string UserEmail { get; set; }

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

        internal static MimeMessage Message { get; set; }
        internal static bool ReplyFlag { get; set; }
        internal static bool ForwardFlag { get; set; }

        /// <summary>
        /// Logs in to imap mail server
        /// </summary>
        public static bool Login()
        {
            if (client == null)
            {
                client = new ImapClient();

                if (client.IsConnected == false && client.IsAuthenticated == false)
                {
                    //client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                    try
                    {
                        client.Connect(ImapHost, ImapPort, ImapUseSsl);

                        client.AuthenticationMechanisms.Remove("XOAUTH2");
                        client.Authenticate(ImapUser, ImapPassword);
                        LoggedIn = true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Error logging in to mail server. Message: " + e.ToString());
                        return false;
                    }
                }
            }

            return true;
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

            if (LoggedIn)
            {
                var inbox = client.Inbox;
                inbox.Open(FolderAccess.ReadWrite);

                //Debug.WriteLine("Total messages: {0}", inbox.Count);

                var summaryItems = inbox.Fetch(0, -1, MessageSummaryItems.Envelope | MessageSummaryItems.UniqueId).ToList();

                //Sorts messagesummaries in reverse arrival order
                var orderBy = new[] { MailKit.Search.OrderBy.ReverseArrival };
                MessageSorter.Sort(summaryItems, orderBy);

                foreach (MessageSummary summary in summaryItems)
                {
                    MailHeader mailHeader = ConvertMessageSummary(summary);
                    headerList.Add(mailHeader);
                }              
            }
            else
            {

                if (Login())
                {
                    GetMailHeaders();
                }
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
            var mailHeader = new MailHeader();

            mailHeader.Subject = msgSum.Envelope.Subject;

            InternetAddressList fromList = msgSum.Envelope.From;
            string fromString = "";

            //For all InternetAddresses in InternetAddressList, add display name (if it exists) 
            //to the fromString, otherwise add email address
            foreach (var from in fromList)
            {
                if (!string.IsNullOrEmpty(from.Name))
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

            string date = "";

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
            MimeMessage mail = null;

            if (LoggedIn)
            {
                var inbox = client.Inbox;

                if (!inbox.IsOpen)
                {
                    inbox.Open(FolderAccess.ReadWrite);
                }

                mail = inbox.GetMessage(uid);
            }
            else
            {
                if (Login())
                {
                    GetSpecificMail(uid);
                }
            }

            return mail;
        }

        /// <summary>
        /// Connects to smtp server and sends mail
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="subject"></param>
        /// <param name="bodyText"></param>
        /// <returns></returns>
        public static bool SendMail(MimeMessage msg, string from, string to, string cc, string bcc, string subject, string bodyText)
        {
            var message = new MimeMessage();

            //If the message is a reply message
            if (ReplyFlag == true)
            {
                //message.From and message.To needs to be cleared first
                msg.From.Clear();
                msg.From.Add(new MailboxAddress(from));

                msg.To.Clear();
                msg.To.Add(new MailboxAddress(to));

                message = msg;
            }
            //If the message is a forwarded message
            else if (ForwardFlag==true)
            {              
                msg.To.Add(new MailboxAddress(to));
                message = msg;
            }
            //If standard mail message
            else
            {
                message.From.Add(new MailboxAddress(from));
                message.To.Add(new MailboxAddress(to));
            }

            if (!string.IsNullOrEmpty(cc))
            {              
                message.Cc.Add(new MailboxAddress(cc));
            }
            if (!string.IsNullOrEmpty(bcc))
            {
                message.Bcc.Add(new MailboxAddress(bcc));
            }

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

            var client = new SmtpClient();

            try
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
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error message: " + e.Message);
                return false;
            }

            client = null;

            Debug.WriteLine(" Mail sent!");

            return true;
        }

        /// <summary>
        /// Creates a reply message
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MimeMessage CreateReply(MimeMessage message)
        {
            var replyMessage = new MimeMessage();

            var userMailbox = new MailboxAddress(UserEmail);

            replyMessage.From.Add(userMailbox);

            //Adds the To address(es)
            if (message.ReplyTo.Count > 0)
            {
                replyMessage.To.AddRange(message.ReplyTo);
            }
            else if (message.From.Count > 0)
            {
                replyMessage.To.AddRange(message.From);
            }
            else if (message.Sender != null)
            {
                replyMessage.To.Add(message.Sender);
            }

            //Sets the subject of the reply message       
            replyMessage.Subject = "Re: " + message.Subject;

            //Creates the In-Reply-To and References headers
            if (!string.IsNullOrEmpty(message.MessageId))
            {
                replyMessage.InReplyTo = message.MessageId;
                foreach (var id in message.References)
                    replyMessage.References.Add(id);
                replyMessage.References.Add(message.MessageId);
            }

            //Quotes the original message text
            using (var quoted = new StringWriter())
            {
                MailboxAddress sender;
                if (message.Sender != null)
                {
                    sender = message.Sender;
                }
                else
                {
                    sender = userMailbox;
                }

                string tempMessageDate = message.Date.ToString("u");
                quoted.WriteLine("On {0}, {1} wrote:", tempMessageDate.Substring(0, 16),
                    !string.IsNullOrEmpty(sender.Name) ? sender.Name : sender.Address);

                StringReader reader;

                //If the body part is Text/Plain
                if (message.TextBody != null)
                {
                    reader = new StringReader(message.TextBody);
                }
                //If the body part is Text/Html
                else
                {
                    reader = new StringReader(message.HtmlBody);
                }

                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    quoted.Write("> ");
                    quoted.WriteLine(line);
                }

                //Sets the body part of the message
                var textPart = new TextPart("plain");
                textPart.Text = quoted.ToString();
                replyMessage.Body = textPart;

                reader.Close();
                reader = null;
            }

            return replyMessage;
        }

        /// <summary>
        /// Creates a message to be forwarded
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static MimeMessage CreateForward(MimeMessage original)
        {
            var message = new MimeMessage();

            var userMailbox = new MailboxAddress(UserEmail);
            message.From.Add(userMailbox);           
 
            //Sets the forwarded subject          
            message.Subject = "Fwd: " + original.Subject;

            //Quotes the original message text
            using (var text = new StringWriter())
            {
                text.WriteLine();
                text.WriteLine("-------- Original Message --------");
                text.WriteLine("Subject: {0}", original.Subject);
                text.WriteLine("Date: {0}", DateUtils.FormatDate(original.Date));
                text.WriteLine("From: {0}", original.From);
                text.WriteLine("To: {0}", original.To);
                text.WriteLine();

                //Writes the TextBody if it isn´t empty or null
                if (!string.IsNullOrEmpty(original.TextBody))
                {
                    text.Write(original.TextBody);
                }
                //Writes the HtmlBody if it isn´t empty or null
                else if (!string.IsNullOrEmpty(original.HtmlBody))
                {
                    text.WriteLine(original.HtmlBody);
                }
                else
                {
                    text.WriteLine();
                }

                message.Body = new TextPart("plain")
                {
                    Text = text.ToString()
                };
            }

            return message;
        }

        /// <summary>
        /// Deletes a specific mail
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static bool DeleteMessage(MimeMessage message)
        {
            if (Login())
            {
                try
                {
                    if (client.Inbox.IsOpen)
                    {
                        var uid = client.Inbox.Search(SearchQuery.HeaderContains("Message-Id", message.MessageId));
                        Debug.WriteLine("uid: " + uid);
                        client.Inbox.AddFlags(new UniqueId[] { uid[0] }, MessageFlags.Deleted, true, new CancellationToken());
                        client.Inbox.Expunge();
                    } 
                    else
                    {
                        client.Inbox.Open(FolderAccess.ReadWrite);
                        var uid = client.Inbox.Search(SearchQuery.HeaderContains("Message-Id", message.MessageId));
                        Debug.WriteLine("uid: " + uid);
                        client.Inbox.AddFlags(new UniqueId[] { uid[0] }, MessageFlags.Deleted, true, new CancellationToken());
                        client.Inbox.Expunge();
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString());
                    return false;
                }

                return true;
            }

            return true;
        }
    }
}



