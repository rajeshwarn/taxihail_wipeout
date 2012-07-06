using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace apcurium.MK.Booking.Email
{
    public interface IEmailSender
    {
        void Send(MailMessage message);
    }
}
