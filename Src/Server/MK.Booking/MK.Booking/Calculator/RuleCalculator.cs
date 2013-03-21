using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Calculator
{
    public class RuleCalculator : IRuleCalculator
    { 
        private readonly IRuleDao _ruleDao;

        public RuleCalculator(IRuleDao ruleDao)
        {
            _ruleDao = ruleDao;
        }
      

        public RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, string zone)
        {
            var rules = _ruleDao.GetActiveWarningRule(isFutureBooking, zone).ToArray();

            return GetMatching(rules,  pickupDate);

        }

        public RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, string zone)
        {
            var rules = _ruleDao.GetActiveDisableRule(isFutureBooking, zone).ToArray();

            return GetMatching(rules, pickupDate);

        }

        private RuleDetail GetMatching(IList<RuleDetail> rulesList, DateTime pickupDate)
        {
            // Case 1: A rule exists for the specific date
            var rule = (from r in rulesList
                        where r.Type == (int)RuleType.Date
                        where IsDayMatch(r, pickupDate)
                        select r).FirstOrDefault();

            // Case 2: A rule exists for the day of the week
            if (rule == null)
            {
                rule = (from r in rulesList
                        where r.Type == (int)RuleType.Recurring
                        where IsRecurringMatch(r, pickupDate)
                        select r).FirstOrDefault();
            }

            // Case 3: return default
            if (rule == null)
            {
                rule = (from r in rulesList
                        where r.Type == (int) RuleType.Default
                        select r).FirstOrDefault();
            }

            return rule;
        }


        private bool IsDayMatch(RuleDetail rule, DateTime date)
        {
            if (rule.Type == (decimal) RuleType.Date)
            {
                return date >= rule.ActiveFrom && date < rule.ActiveTo;
            }
            return false;
        }

        private bool IsRecurringMatch(RuleDetail rule, DateTime date)
        {
            if (rule.Type == (int)RuleType.Recurring)
            {
                // Represents the candidate date day of the week value in the DayOfTheWeek enum
                var dayOfTheWeek = 1 << (int) date.DayOfWeek;

                var startTime = DateTime.MinValue.AddHours(rule.StartTime.Value.Hour).AddMinutes(rule.StartTime.Value.Minute);
                var endTime = DateTime.MinValue.AddHours(rule.EndTime.Value.Hour).AddMinutes(rule.EndTime.Value.Minute);
                var time = DateTime.MinValue.AddHours(date.Hour).AddMinutes(date.Minute);


                // Determine if the candidate date is between start time and end time
                bool isInRange = time >= startTime && time < endTime;

                if (isInRange)
                {
                    // Now determine if the day of the week is correct
                    if (startTime.Date == time.Date)
                    {
                        // The candidate date is the same day defined for the rule
                        return (rule.DaysOfTheWeek & dayOfTheWeek) == dayOfTheWeek;
                    }
                }
            }
            return false;
        }
    }
}
