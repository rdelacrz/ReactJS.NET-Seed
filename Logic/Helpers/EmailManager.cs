using static Logic.EnvironmentSettings;
using Logic.Exceptions;
using Logic.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Helpers
{
    public class EmailManager
    {
        public const int MAX_BATCH_SIZE = 50;

        /// <summary>
        /// Asynchronously sendx email with given subject and body to the given email address(es).
        /// </summary>
        /// <param name="subject">Subject of email being sent.</param>
        /// <param name="body">Body of email being sent.</param>
        /// <param name="addressTo">Email address(es) to which the email is being sent to (To).</param>
        /// <param name="addressCC">Email address(es) to which the email is being sent to (CC).</param>
        /// <param name="addressBCC">Email address(es) to which the email is being sent to (BCC).</param>
        /// <param name="addressReplyTo">Email address(es) to which replies should be sent to (Reply To).</param>
        /// <param name="attachments">List of attachment file paths to add to email, if any.</param>
        /// <param name="isBodyHTML">True if body should be formatted as HTML (false by default).</param>
        public async static Task SendEmail(string subject, string body, string addressTo = null, string addressCC = null,
            string addressBCC = null, string addressReplyTo = null, IEnumerable<string> attachments = null, bool isBodyHTML = false)
        {
            bool noEmailAddresses = string.IsNullOrEmpty(addressTo) && string.IsNullOrEmpty(addressCC) && string.IsNullOrEmpty(addressBCC);

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(body) && !noEmailAddresses)
            {
                var mailAddressTo = ConvertAddresses(addressTo);
                var mailAddressCC = ConvertAddresses(addressCC);
                var mailAddressBCC = ConvertAddresses(addressBCC);
                var mailAddressReplyTo = ConvertAddresses(addressReplyTo);
                MailMessage emailMsg = SetupEmailMessage(subject, body, mailAddressTo, mailAddressCC,
                    mailAddressBCC, mailAddressReplyTo, attachments, isBodyHTML);
                await SendEmailCoreLogic(emailMsg);
            }
        }

        /// <summary>
        /// Asynchronously sendx email with given subject and body to the given email address(es).
        /// </summary>
        /// <param name="subject">Subject of email being sent.</param>
        /// <param name="body">Body of email being sent.</param>
        /// <param name="addressTo">Email address(es) to which the email is being sent to (To).</param>
        /// <param name="addressCC">Email address(es) to which the email is being sent to (CC).</param>
        /// <param name="addressBCC">Email address(es) to which the email is being sent to (BCC).</param>
        /// <param name="addressReplyTo">Email address(es) to which replies should be sent to (Reply To).</param>
        /// <param name="attachments">List of attachment file paths to add to email, if any.</param>
        /// <param name="isBodyHTML">True if body should be formatted as HTML (false by default).</param>
        public async static Task SendEmail(string subject, string body, IEnumerable<MailAddress> addressTo = null,
            IEnumerable<MailAddress> addressCC = null, IEnumerable<MailAddress> addressBCC = null,
            IEnumerable<MailAddress> addressReplyTo = null, IEnumerable<string> attachments = null, bool isBodyHTML = false)
        {
            bool noEmailAddresses = addressTo.Count() == 0 && addressCC.Count() == 0 && addressBCC.Count() == 0;

            if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(body) && !noEmailAddresses)
            {
                MailMessage emailMsg = SetupEmailMessage(subject, body, addressTo, addressCC, addressBCC, addressReplyTo, attachments, isBodyHTML);
                await SendEmailCoreLogic(emailMsg);
            }
        }

        /// <summary>
        /// Send email with given subject and body to the given email address(es). 
        /// <br /><br />
        /// Note 1: To e-mail addresses are sent individually rather than everyone at once.<br />
        /// Note 2: All e-mails are done in batches of 50 or less to prevent overload of SMTP server.
        /// </summary>
        /// <param name="subject">Subject of email being sent.</param>
        /// <param name="body">Body of email being sent.</param>
        /// <param name="addressTos">List of email addresses to which the email is being sent to (To).</param>
        /// <param name="addressCCs">List of email addresses to which the email is being sent to (CC).</param>
        /// <param name="addressBCCs">List of email addresses to which the email is being sent to (BCC).</param>
        /// <param name="addressReplyTos">List of email addresses to which replies should be sent to (Reply To).</param>
        /// <param name="attachments">List of attachment file paths to add to email, if any.</param>
        /// <param name="isBodyHTML">True if body should be formatted as HTML (false by default).</param>
        public async static Task SendManyEmails(string subject, string body, IEnumerable<string> addressTos = null,
            IEnumerable<string> addressCCs = null, IEnumerable<string> addressBCCs = null, IEnumerable<string> addressReplyTos = null,
            IEnumerable<string> attachments = null, bool isBodyHTML = false)
        {
            int batchSize = addressTos.Count() + addressCCs.Count() + addressBCCs.Count();

            // Sends everything in normal manner
            if (batchSize <= MAX_BATCH_SIZE && addressTos.Count() <= 1)
            {
                string addressTo = addressTos.ToCommaSeparatedString();
                string addressCC = addressCCs.ToCommaSeparatedString();
                string addressBCC = addressBCCs.ToCommaSeparatedString();
                string addressReplyTo = addressReplyTos.ToCommaSeparatedString();
                await SendEmail(subject, body, addressTo, addressCC, addressBCC, addressReplyTo, attachments, isBodyHTML);
            }
            else
            {
                // Sends to "TO" addresses individually
                foreach (string addressTo in addressTos)
                {
                    await SendEmail(subject, body, addressTo, attachments: attachments);
                }

                // Sends to "CC" addresses in batches
                await SendEmailBatch(ConvertAddresses(addressCCs.ToCommaSeparatedString()).ToList(), async (addressBatch) => {
                    await SendEmail(subject, body, addressCC: addressBatch, attachments: attachments, isBodyHTML: isBodyHTML);
                });

                // Sends to "BCC" addresses in batches
                await SendEmailBatch(ConvertAddresses(addressBCCs.ToCommaSeparatedString()).ToList(), async (addressBatch) => {
                    await SendEmail(subject, body, addressBCC: addressBatch, attachments: attachments, isBodyHTML: isBodyHTML);
                });
            }
        }

        /// <summary>
        /// Send email with given subject and body to the given email address(es). 
        /// <br /><br />
        /// Note 1: To e-mail addresses are sent individually rather than everyone at once.<br />
        /// Note 2: All e-mails are done in batches of 50 or less to prevent overload of SMTP server.
        /// </summary>
        /// <param name="subject">Subject of email being sent.</param>
        /// <param name="body">Body of email being sent.</param>
        /// <param name="addressTos">List of email addresses to which the email is being sent to (To).</param>
        /// <param name="addressCCs">List of email addresses to which the email is being sent to (CC).</param>
        /// <param name="addressBCCs">List of email addresses to which the email is being sent to (BCC).</param>
        /// <param name="addressReplyTo">List of email addresses to which replies should be sent to (Reply To).</param>
        /// <param name="attachments">List of attachment file paths to add to email, if any.</param>
        /// <param name="isBodyHTML">True if body should be formatted as HTML (false by default).</param>
        public async static Task SendManyEmails(string subject, string body, IEnumerable<MailAddress> addressTos = null,
            IEnumerable<MailAddress> addressCCs = null, IEnumerable<MailAddress> addressBCCs = null,
            IEnumerable<MailAddress> addressReplyTos = null, IEnumerable<string> attachments = null, bool isBodyHTML = false)
        {
            int batchSize = addressTos.Count() + addressCCs.Count() + addressBCCs.Count();

            // Sends everything in normal manner
            if (batchSize <= MAX_BATCH_SIZE && addressTos.Count() <= 1)
            {
                await SendEmail(subject, body, addressTos, addressCCs, addressBCCs, addressReplyTos, attachments, isBodyHTML);
            }
            else
            {
                // Sends to "TO" addresses individually
                await SendEmail(subject, body, addressTos, addressReplyTo: addressReplyTos, attachments: attachments);

                // Sends to "CC" addresses in batches
                await SendEmailBatch(addressCCs.ToList(), async (addressBatch) => {
                    await SendEmail(subject, body, addressCC: addressBatch, addressReplyTo: addressReplyTos,
                        attachments: attachments, isBodyHTML: isBodyHTML);
                });

                // Sends to "BCC" addresses in batches
                await SendEmailBatch(addressBCCs.ToList(), async (addressBatch) => {
                    await SendEmail(subject, body, addressBCC: addressBatch, addressReplyTo: addressReplyTos,
                        attachments: attachments, isBodyHTML: isBodyHTML);
                });
            }
        }

        /// <summary>
        /// Implements core logic for sending email.
        /// </summary>
        /// <param name="emailMsg">Email message to send.</param>
        private async static Task SendEmailCoreLogic(MailMessage emailMsg)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient())
                {
                    await smtpClient.SendMailAsync(emailMsg);
                }
            }
            catch (Exception e)
            {
                // Uses a Validation Exception so that error messages can be outputted within a modal
                if (EnvironmentSettings.IsDev)
                {
                    throw new ValidationException("SMTP server may not be set up for the current non-production environment.");
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Send email batch based on provided sendEmail function.
        /// </summary>
        /// <param name="emailAddresses">List of e-mail addresses to which the email is being sent to.</param>
        /// <param name="sendEmail">E-mail function that determines how e-mail is being sent.</param>
        private async static Task SendEmailBatch(List<MailAddress> emailAddresses, Func<List<MailAddress>, Task> sendEmail)
        {
            List<MailAddress> batch = new List<MailAddress>();
            int i = 0;
            while (i < emailAddresses.Count)
            {
                batch.Add(emailAddresses[i]);

                // Sends e-mail if batch max size is reached or final e-mail in BCC list is reached
                if (batch.Count == MAX_BATCH_SIZE || i == emailAddresses.Count - 1)
                {
                    await sendEmail(batch);
                    batch = new List<MailAddress>();         // Resets batch
                }

                i++;
            }
        }

        /// <summary>
        /// Sets up e-mail message contents.
        /// </summary>
        /// <param name="subject">Subject of email being sent.</param>
        /// <param name="body">Body of email being sent.</param>
        /// <param name="addressTo">Email address(es) to which the email is being sent to (To).</param>
        /// <param name="addressCC">Email address(es) to which the email is being sent to (CC).</param>
        /// <param name="addressBCC">Email address(es) to which the email is being sent to (BCC).</param>
        /// <param name="addressReplyTo">Email address(es) to which replies should be sent to (Reply To).</param>
        /// <param name="attachments">List of attachment file paths to add to email, if any.</param>
        /// <param name="isBodyHTML">True if body should be formatted as HTML (false by default).</param>
        /// <returns>Sets up mail message.</returns>
        private static MailMessage SetupEmailMessage(string subject, string body, IEnumerable<MailAddress> addressTo,
            IEnumerable<MailAddress> addressCC, IEnumerable<MailAddress> addressBCC, IEnumerable<MailAddress> addressReplyTo,
            IEnumerable<string> attachments = null, bool isBodyHTML = false)
        {
            MailMessage emailMsg = new MailMessage
            {
                BodyEncoding = Encoding.UTF8,
                IsBodyHtml = isBodyHTML,
                From = new MailAddress(ContactInfo.EMAIL_HELP_DESK.EmailAddress, ContactInfo.EMAIL_HELP_DESK.Name),
                Subject = subject,
                Body = body
            };

            // Add emails to the To, CC, and BCC sections
            if (addressTo != null)
            {
                foreach (var addr in addressTo)
                {
                    emailMsg.To.Add(addr);
                }
            }
            if (addressCC != null)
            {
                foreach (var addr in addressCC)
                {
                    emailMsg.To.Add(addr);
                }
            }
            if (addressBCC != null)
            {
                foreach (var addr in addressBCC)
                {
                    emailMsg.To.Add(addr);
                }
            }
            if (addressBCC != null)
            {
                foreach (var addr in addressBCC)
                {
                    emailMsg.To.Add(addr);
                }
            }
            if (addressReplyTo != null)
            {
                foreach (var addr in addressReplyTo)
                {
                    emailMsg.ReplyToList.Add(addr);
                }
            }

            // Adds attachments (if any)
            if (attachments != null)
            {
                foreach (var attachment in attachments)
                {
                    emailMsg.Attachments.Add(new Attachment(attachment));
                }
            }

            return emailMsg;
        }

        /// <summary>
        /// Converts comma-delimited address string into mail address list.
        /// </summary>
        /// <param name="addresses">String containing comma-delimited address string.</param>
        /// <returns>List of Mail Address objects.</returns>
        private static IEnumerable<MailAddress> ConvertAddresses(string addresses)
        {
            if (string.IsNullOrWhiteSpace(addresses))
            {
                return null;
            }
            return addresses.Split(',').Select(addr => new MailAddress(addr.Trim()));
        }
    }
}
