using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.SMS
{
    public interface ISmsService
    {
        void Send(string toNumber, string message);
    }
}
