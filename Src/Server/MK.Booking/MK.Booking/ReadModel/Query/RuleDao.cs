using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel.Query
{
    public class RuleDao : IRuleDao
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public RuleDao(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public IList<RuleDetail> GetAll()
        {
            using (var context = _contextFactory.Invoke())
            {
                return context.Query<RuleDetail>().ToList();
            }
        }


        public RuleDetail GetActiveDisableRule(bool isFutureBooking, DateTime pickupDate)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Query<RuleDetail>().Where(r => (r.Category == (int)RuleCategory.DisableRule) &&
                                                                   ((!isFutureBooking && r.AppliesToCurrentBooking) || (isFutureBooking && r.AppliesToFutureBooking)) &&
                                                                   r.IsActive)
                                    .OrderBy(r => r.Priority).FirstOrDefault();
                return rule;
            }
        }

        public RuleDetail GetActiveWarningRule(bool isFutureBooking, DateTime pickupDate)
        {
            using (var context = _contextFactory.Invoke())
            {
                var rule = context.Query<RuleDetail>().Where(r => (r.Category == (int)RuleCategory.WarningRule) &&
                                                                   ((!isFutureBooking && r.AppliesToCurrentBooking) || (isFutureBooking && r.AppliesToFutureBooking)) && 
                                                                   r.IsActive)                    
                                    .OrderBy( r=>r.Priority ).FirstOrDefault();
                return rule;

            }
        }

    }
}