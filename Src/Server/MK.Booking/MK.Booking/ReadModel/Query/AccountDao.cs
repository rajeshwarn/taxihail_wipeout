#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;

#endregion

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class AccountDao : IAccountDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public AccountDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<AccountDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountDetail>().ToList();
            }
        }

        public AccountDetail FindById(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountDetail>().SingleOrDefault(c => c.Id == id);
            }
        }

        public AccountDetail FindByEmail(string email)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountDetail>().SingleOrDefault(c => c.Email.ToLower() == email.ToLower());
            }
        }

        public AccountDetail FindByFacebookId(string id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountDetail>().SingleOrDefault(c => c.FacebookId == id);
            }
        }

        public AccountDetail FindByTwitterId(string id)
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<AccountDetail>().SingleOrDefault(c => c.TwitterId == id);
            }
        }

        public int? GetIbsAccountId(Guid accountId, string companyKey)
        {
            using (var context = _contextFactory.Invoke())
            {
                if (companyKey == null)
                {
                    var account = context.Query<AccountDetail>().First(c => c.Id == accountId);
                    return account.IBSAccountId;
                }

                var accountIbsInfo = context.Query<AccountIbsDetail>().SingleOrDefault(c => c.AccountId == accountId && c.CompanyKey == companyKey);
                return accountIbsInfo != null
                    ? (int?)accountIbsInfo.IBSAccountId
                    : null;
            }
        }

        public string GetPayPalRefreshToken(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payPalAccountInfo = context.Query<PayPalAccountDetails>().SingleOrDefault(c => c.AccountId == id);
                return payPalAccountInfo != null ? payPalAccountInfo.AuthCode : null;
            }
        }
    }
}