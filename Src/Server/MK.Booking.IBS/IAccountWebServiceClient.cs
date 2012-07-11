using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.IBS
{
    public interface IAccountWebServiceClient
    {
        int CreateAccount(Guid accountId, string email, string name, string lastName, string phone);
    }
}
