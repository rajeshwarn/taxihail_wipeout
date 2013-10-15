using System;
using System.Collections.Generic;
using apcurium.MK.Common.Configuration.Impl;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface ICreditCardDao
    {
        IList<CreditCardDetails> FindByAccountId(Guid accountId);
        CreditCardDetails FindByToken(string cardToken);
    }
}