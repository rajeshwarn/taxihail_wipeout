﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class CreditCardDao : ICreditCardDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public CreditCardDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<CreditCardDetails> FindByAccountId(Guid accountId)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<CreditCardDetails>().Where(c => c.AccountId.Equals(accountId)).ToList();
            }
        }

        public CreditCardDetails FindByToken(string cardToken)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<CreditCardDetails>().FirstOrDefault(c => c.Token == cardToken);
            }
        }
    }
}