using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Database;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

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


        public IList<RuleDetail> GetActiveDisableRule(bool isFutureBooking, string zone)
        {
            using (var context = _contextFactory.Invoke())
            {
                /*var rule = context.Query<RuleDetail>().Where(r => (r.Category == (int)RuleCategory.DisableRule) &&
                                                                   ((!isFutureBooking && r.AppliesToCurrentBooking) || (isFutureBooking && r.AppliesToFutureBooking)) &&
                                                                   r.IsActive && pickupDate > r.ActiveFrom && pickupDate < r.ActiveTo)
                                    .OrderBy(r => r.Priority).ToList();
                return (from ruleDetail in rule let zoneList = ruleDetail.ZoneList.Split(',') where zoneList.SingleOrDefault(c => c.Equals(zone)).HasValue() select ruleDetail).FirstOrDefault();*/
                var rule = context.Query<RuleDetail>().Where(r => (r.Category == (int)RuleCategory.DisableRule) &&
                                                                   ((!isFutureBooking && r.AppliesToCurrentBooking) || (isFutureBooking && r.AppliesToFutureBooking)) &&
                                                                   r.IsActive )
                                    .OrderBy(r => r.Priority).ToList();
                return (from ruleDetail in rule where !string.IsNullOrEmpty(ruleDetail.ZoneList) let zoneList = ruleDetail.ZoneList.Split(',') where zoneList.SingleOrDefault(c => c.Equals(zone)).HasValue() select ruleDetail).ToList();
            }
        }



        public IList<RuleDetail> GetActiveWarningRule(bool isFutureBooking, string zone)
        {
            using (var context = _contextFactory.Invoke())
            {
               var rule = context.Query<RuleDetail>().Where(r => (r.Category == (int) RuleCategory.WarningRule) &&
                                                       ((!isFutureBooking && r.AppliesToCurrentBooking) ||
                                                        (isFutureBooking && r.AppliesToFutureBooking)) &&
                                                       r.IsActive )
                       .OrderBy(r => r.Priority).ToList();


                return (from r in rule where !string.IsNullOrEmpty(r.ZoneList) let zoneList = r.ZoneList.Split(',') where zoneList.SingleOrDefault(c=>c.Equals(zone)).HasValue() select r).ToList();
            }
        }

    }
}
