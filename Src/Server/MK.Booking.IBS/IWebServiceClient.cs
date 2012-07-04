using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.IBS
{
    public interface IWebServiceClient
    {
        int CreateAccount(Guid accountId, string email, string firstName, string lastName, string phone);
    }
}
