#region

using System;

#endregion

namespace apcurium.MK.Booking.IBS
{
    public interface IAccountWebServiceClient
    {
        int CreateAccount(Guid accountId, string email, string name, string lastName, string phone);
    }
}