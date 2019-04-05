using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BugTracker.Models
{
    public class EmailService : IIdentityMessageService
    {
        //Getting the settings from our private.config setup in web.config
        private string SmtpHost = "smtp.mailtrap.io";
        private int SmtpPort = 2525;
        private string SmtpUsername = "e4ffc6c1f9b78a";
        private string SmtpPassword = "0e3ad862fa6817";
        private string SmtpFrom = "nour@gmail.com";

        /// <summary>
        /// Sends an e-mail
        /// </summary>
        /// <param name="to">The destination of the e-mail</param>
        /// <param name="body">The body of the e-mail</param>
        /// <param name="subject">The subject of the e-mail</param>
        public void Send(string to,
            string body,
            string subject)
        {
            //Creates a MailMessage required to send messages
            var message = new MailMessage(SmtpFrom, to);
            message.Body = body;
            message.Subject = subject;
            message.IsBodyHtml = true;

            //Creates a SmtpClient required to handle the communication
            //between our application and the SMTP Server
            var smtpClient = new SmtpClient(SmtpHost, SmtpPort);
            smtpClient.Credentials =
                new NetworkCredential(SmtpUsername, SmtpPassword);
            smtpClient.EnableSsl = true;

            //Send the message
            smtpClient.Send(message);
        }

        /// <summary>
        /// Required by Microsoft Interface IIdentityMessageService in order to work with Identity Framework
        /// </summary>
        /// <param name="message">Object containing the necessary information to send e-mails</param>
        /// <returns></returns>
        public Task SendAsync(IdentityMessage message)
        {
            //Caling our sync method in a async way.
            return Task.Run(() =>
            Send(message.Destination, message.Body, message.Subject));
        }
    }
}