#region

using System.Net.Mail;

#endregion

namespace apcurium.MK.Booking.Email
{
    public interface IEmailSender
    {
        void Send(MailMessage message);
    }
}