using System.Net.Mail;

namespace apcurium.MK.Booking.Email
{
    public interface IEmailSender
    {
        void Send(MailMessage message);
    }
}