using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;

namespace apcurium.MK.Booking.Maps.Impl
{
    public class PriceCalculator : IPriceCalculator
    {
        private readonly IConfigurationManager _configManager;
        private readonly IRateProvider _rateProvider;

        public PriceCalculator(IConfigurationManager configManager, IRateProvider rateProvider)
        {
            _configManager = configManager;
            _rateProvider = rateProvider;
        }

        public double? GetPrice(int? distance, DateTime pickupDate)
        {
            var rate = GetRateFor(pickupDate);

            if (rate == null) return null;

            double maxDistance = double.Parse(_configManager.GetSetting("Direction.MaxDistance"), CultureInfo.InvariantCulture);
            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    double km = ((double)distance.Value / 1000);

                    if (km < maxDistance)
                    {
                        decimal d = 1;
                        
                        price = ((double)rate.FlatRate + (km*rate.DistanceMultiplicator))*(1 + rate.TimeAdjustmentFactor/100);
                    }
                    else
                    {
                        price = 1000;
                    }

                    if (price.HasValue)
                    {

                        price = Math.Round(price.Value, 2);

                        Console.WriteLine(price);

                        price = price.Value * 100;

                        int q = (int)(price.Value / 5.0);

                        int r;

                        Math.DivRem((int)price.Value, 5, out r);
                        Console.WriteLine(" r : " + r.ToString());
                        if (r > 0)
                        {
                            q++;
                        }

                        price = q * 5;

                        Console.WriteLine(price);

                        price = price.Value / 100;

                        Console.WriteLine(price);

                    }
                }
            }
            catch
            {
            }

            return price;
        }

        public Rate GetRateFor(DateTime pickupDate)
        {
            var rates = _rateProvider.GetRates().ToArray();

            // Case 1: A rate exists for the specific date
            var rate = (from r in rates
                        where r.Type == (int) RateType.Day
                        where IsDayMatch(r, pickupDate)
                        select r).FirstOrDefault();

            // Case 2: A rate exists for the day of the week
            if (rate == null)
            {
                rate = (from r in rates
                        where r.Type == (int) RateType.Recurring
                        where IsRecurringMatch(r, pickupDate)
                        select r).FirstOrDefault();
            }

            // Case 3: Use default rate
            if(rate == null)
            {
                rate = rates.FirstOrDefault(x => x.Type == (int) RateType.Default);
            }

            return rate;

        }

        private bool IsDayMatch(Rate rate, DateTime date)
        {
            if (rate.Type == (int)RateType.Day)
            {
                var startTime = rate.StartTime;
                var endTime = rate.StartTime.Date.AddHours(rate.EndTime.Hour).AddMinutes(rate.EndTime.Minute);

                if (endTime < startTime)
                {
                    //The rate spans across two days
                    endTime = endTime.AddDays(1);
                }

                return date >= startTime && date < endTime;
            }
            return false;
        }

        private bool IsRecurringMatch(Rate rate, DateTime date)
        {
            if (rate.Type == (int)RateType.Recurring)
            {
                // Represents the candidate date day of the week value in the DayOfTheWeek enum
                var dayOfTheWeek = 1 << (int) date.DayOfWeek;

                var startTime = DateTime.MinValue.AddHours(rate.StartTime.Hour).AddMinutes(rate.StartTime.Minute);
                var endTime = DateTime.MinValue.AddHours(rate.EndTime.Hour).AddMinutes(rate.EndTime.Minute);
                var time = DateTime.MinValue.AddHours(date.Hour).AddMinutes(date.Minute);

                if (endTime < startTime)
                {
                    //The rate spans across two days
                    if (time < endTime)
                    {
                        //The candidate date is on the second day of the rate
                        time = time.AddDays(1);
                    }
                    endTime = endTime.AddDays(1);
                }

                // Determine if the candidate date is between start time and end time
                bool isInRange = time >= startTime && time < endTime;

                if (isInRange)
                {
                    // Now determine if the day of the week is correct
                    if (startTime.Date == time.Date)
                    {
                        // The candidate date is the same day defined for the rate
                        return (rate.DaysOfTheWeek & dayOfTheWeek) == dayOfTheWeek;
                    }
                    else if (endTime.Date == time.Date)
                    {
                        // The candidate date is the next day defined for the rate
                        // We have to check if a rate exist for the previous day
                        var previousDayOfTheWeek = dayOfTheWeek == (int)DayOfTheWeek.Sunday
                                                       ? (int) DayOfTheWeek.Saturday
                                                       : dayOfTheWeek >> 1;
                        return (rate.DaysOfTheWeek & previousDayOfTheWeek) == previousDayOfTheWeek;
                    }
                }
            }
            return false;
        }
    }
}
