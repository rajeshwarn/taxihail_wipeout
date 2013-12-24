#region

using System;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query.Contract
{
    public interface ICreditCardDao
    {
        IList<CreditCardDetails> FindByAccountId(Guid accountId);
        CreditCardDetails FindByToken(string cardToken);
    }
}