using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace CustomerPortal.Web.Services
{
    public interface IEmailSender
    {
        void SendEmail(string details, string tag, string company, string userName, string userEmail, string server);
    }
}
