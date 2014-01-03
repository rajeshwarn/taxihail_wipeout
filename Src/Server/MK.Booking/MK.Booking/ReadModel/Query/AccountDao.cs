﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel.Query.Contract;

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
    }
}