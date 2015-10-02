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

        public string GetPayPalEncryptedRefreshToken(Guid id)
        {
            using (var context = _contextFactory.Invoke())
            {
                var payPalAccountInfo = context.Query<PayPalAccountDetails>().SingleOrDefault(c => c.AccountId == id);
                return payPalAccountInfo != null ? payPalAccountInfo.EncryptedRefreshToken : null;
            }
        }

		public AccountDetail[] FindByNamePattern(string name)
		{
			using (var context = _contextFactory.Invoke())
			{
				var accountsWithName = from account in context.Query<AccountDetail>()
									   where account.Name.Contains(name)
									   select account;

				return accountsWithName.ToArray();
			}
		}

		public AccountDetail[] FindByEmailPattern(string email)
		{
			using (var context = _contextFactory.Invoke())
			{
				var accountsWithEmail = from account in context.Query<AccountDetail>()
										where account.Email.Contains(email)
										select account;

				return accountsWithEmail.ToArray();
			}
		}

		public AccountDetail[] FindByPhoneNumberPattern(string phoneNumber)
		{
			using (var context = _contextFactory.Invoke())
			{
				var accountsWithPhoneNumber = from account in context.Query<AccountDetail>()
						 where account.Settings.Phone.Contains(phoneNumber)
						 select account;

				return accountsWithPhoneNumber.ToArray();
			}
		}
    }
}