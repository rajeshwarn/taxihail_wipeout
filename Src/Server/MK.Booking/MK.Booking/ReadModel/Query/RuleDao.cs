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
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                return context.Query<RuleDetail>().ToList();
            }
        }


        public IList<RuleDetail> GetActiveDisableRule(bool isFutureBooking)
        {
            return GetActiveRule(isFutureBooking, RuleCategory.DisableRule);
        }


        public IList<RuleDetail> GetActiveWarningRule(bool isFutureBooking)
        {
            return GetActiveRule(isFutureBooking, RuleCategory.WarningRule);
        }

        private IList<RuleDetail> GetActiveRule(bool isFutureBooking, RuleCategory category)
        {
            using (BookingDbContext context = _contextFactory.Invoke())
            {
                List<RuleDetail> rules = context.Query<RuleDetail>().Where(r => (r.Category == (int) category) &&
                                                                                ((!isFutureBooking &&
                                                                                  r.AppliesToCurrentBooking) ||
                                                                                 (isFutureBooking &&
                                                                                  r.AppliesToFutureBooking)) &&
                                                                                r.IsActive)
                    .OrderBy(r => r.Priority).ToList();

                return rules;
            }
        }
    }
}