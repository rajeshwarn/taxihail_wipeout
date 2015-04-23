#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;

#endregion

namespace apcurium.MK.Booking.Calculator
{
    public class RuleCalculator : IRuleCalculator
    {
        private readonly IRuleDao _ruleDao;
        private readonly IServerSettings _serverSettings;

        public RuleCalculator(IRuleDao ruleDao, IServerSettings serverSettings)
        {
            _ruleDao = ruleDao;
            _serverSettings = serverSettings;
        }

        public RuleDetail GetActiveWarningFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market)
        {
            var rules = new RuleDetail[0];

            if (market.HasValue() && _serverSettings.ServerData.ValidateAdminRulesForExternalMarket)
            {
                // External market with admin defined rules validation
                rules = _ruleDao.GetActiveWarningRule(isFutureBooking)
                    .Where(rule => rule.Market == market)
                    .ToArray();
            }
            else if (!market.HasValue())
            {
                // Home market rules validation
                rules = _ruleDao.GetActiveWarningRule(isFutureBooking)
                    .ToArray();
            }

            if (rules.Any())
            {
                rules = FilterRulesByZone(rules, pickupZoneGetterFunc(), dropOffZoneGetterFunc());
            }

            return GetMatching(rules, pickupDate);
        }

        public RuleDetail GetActiveDisableFor(bool isFutureBooking, DateTime pickupDate, Func<string> pickupZoneGetterFunc, Func<string> dropOffZoneGetterFunc, string market)
        {
            var rules = new RuleDetail[0];

            if (market.HasValue() && _serverSettings.ServerData.ValidateAdminRulesForExternalMarket)
            {
                // External market with admin defined rules validation
                rules = _ruleDao.GetActiveDisableRule(isFutureBooking)
                    .Where(rule => rule.Market == market)
                    .ToArray();
            }
            else if (!market.HasValue())
            {
                // Home market rules validation
                rules = _ruleDao.GetActiveDisableRule(isFutureBooking)
                    .ToArray();
            }

            if (rules.Any())
            {
                rules = FilterRulesByZone(rules, pickupZoneGetterFunc(), dropOffZoneGetterFunc());
            }

            return GetMatching(rules, pickupDate);
        }

        private RuleDetail[] FilterRulesByZone(RuleDetail[] rules, string pickupZone, string dropOffZone)
        {
            var rulesWithoutZone = from rule in rules
                                    where IsTrimmedNullOrEmpty(rule.ZoneList) 
                                          && !rule.ZoneRequired
                                    select rule;

            var rulesZoneRequired = from rule in rules
                                    where IsTrimmedNullOrEmpty(rule.ZoneList) 
                                          && rule.ZoneRequired
                                          && ((IsTrimmedNullOrEmpty(pickupZone) && rule.AppliesToPickup)
                                          || (IsTrimmedNullOrEmpty(dropOffZone) && rule.AppliesToDropoff))
                                   select rule;

            var rulesForPickup = from rule in rules
                                    where !IsTrimmedNullOrEmpty(rule.ZoneList)
                                            && rule.AppliesToPickup
                                            && (!IsTrimmedNullOrEmpty(pickupZone)
                                                && rule.ZoneList.ToLower().Split(',').Contains(pickupZone.ToLower().Trim()))
                                    select rule;

            var rulesForDropOff = from rule in rules
                                 where !IsTrimmedNullOrEmpty(rule.ZoneList)
                                         && rule.AppliesToDropoff
                                         && (!IsTrimmedNullOrEmpty(dropOffZone)
                                             && rule.ZoneList.ToLower().Split(',').Contains(dropOffZone.ToLower().Trim()))
                                 select rule;

            return rulesWithoutZone.Concat(rulesForPickup).Concat(rulesForDropOff).Concat(rulesZoneRequired).ToArray();
        }

        private bool IsTrimmedNullOrEmpty(string value)
        {
            return value.ToSafeString().Trim().IsNullOrEmpty();
        }

        private RuleDetail GetMatching(IEnumerable<RuleDetail> rulesList, DateTime pickupDate)
        {
            rulesList = rulesList.ToArray();
            // Case 1: A rule exists for the specific date
            var rulesDate = (from r in rulesList
                where r.Type == (int) RuleType.Date
                where IsDayMatch(r, pickupDate)
                select r);

            // Case 2: A rule exists for the day of the week            
            var rulesRecuring = (from r in rulesList
                where r.Type == (int) RuleType.Recurring
                where IsRecurringMatch(r, pickupDate)
                select r);

            // Case 3: return default
            var rulesDefault = (from r in rulesList
                where r.Type == (int) RuleType.Default
                select r);

            return rulesDate.Concat(rulesRecuring)
                .Concat(rulesDefault)
                .OrderBy(r => r.Priority)
                .FirstOrDefault();
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
            if (rule.Type == (int) RuleType.Recurring)
            {
                // Represents the candidate date day of the week value in the DayOfTheWeek enum
                var dayOfTheWeek = 1 << (int) date.DayOfWeek;

                if (rule.StartTime != null
                    && rule.EndTime != null)
                {
                    var dayOffset = 0;
                    if (rule.StartTime.Value.Date != rule.EndTime.Value.Date)
                    {
                        // end time is on the next day
                        dayOffset = 1;
                    }

                    var startTime =
                        DateTime.MinValue.AddHours(rule.StartTime.Value.Hour).AddMinutes(rule.StartTime.Value.Minute);
                    var endTime =
                        DateTime.MinValue.AddDays(dayOffset).AddHours(rule.EndTime.Value.Hour).AddMinutes(rule.EndTime.Value.Minute);
                    var time = DateTime.MinValue.AddHours(date.Hour).AddMinutes(date.Minute);

                    // Determine if the candidate date is between start time and end time
                    var isInRange = time >= startTime && time < endTime;

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
            }
            return false;
        }
    }
}