using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using Nustache.Core;

namespace apcurium.MK.Booking.Email
{
    public class EmailSender : IEmailSender
    {
        public void Send(MailMessage message)
        {
            using(var client = new System.Net.Mail.SmtpClient())
            {
                client.Send(message);
            }
        }
    }
}
