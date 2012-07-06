using System.Net;
using System.Net.Mail;

namespace apcurium.MK.Booking.Email
{
    public class EmailSender : IEmailSender
    {
        public void Send(MailMessage message)
        {
            var client = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("donotreply@apcurium.com", "2wsxCDE#")
            };

            using(client)
            {
                client.Send(message);
            }
        }
    }
}
