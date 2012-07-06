using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Email
{
    public interface ITemplateService
    {
        string Find(string templateName);
        string Render(string template, object data);
    }
}
