#region

using System;
using System.Globalization;
using System.Linq;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Provider;
using MK.Common.iOS.Configuration;

#endregion

namespace apcurium.MK.Booking.Maps.Impl
{
    public class PriceCalculator : IPriceCalculator
    {
        private readonly IAppSettings _appSettings;
        private readonly ITariffProvider _tariffProvider;
        private readonly ILogger _logger;

        public PriceCalculator(IAppSettings appSettings, ITariffProvider tariffProvider, ILogger logger)
        {
            _appSettings = appSettings;
            _tariffProvider = tariffProvider;
            _logger = logger;
        }

        public double? GetPrice(int? distance, DateTime pickupDate)
        {
            var tariff = GetTariffFor(pickupDate);

            if (tariff == null) return null;

            var maxDistance = _appSettings.Data.MaxDistance;
            double? price = null;
            try
            {
                if (distance.HasValue && (distance.Value > 0))
                {
                    var km = ((double) distance.Value/1000) - tariff.KilometerIncluded;
                    km = km < 0 ? 0 : km;

                    if (km < maxDistance)
                    {
                        price = ((double) tariff.FlatRate + (km*tariff.KilometricRate))*(1 + tariff.MarginOfError/100);
                    }
                    else
                    {
                        price = 1000;
                    }
                    
                    price = Math.Round(price.Value, 2);
                    price = price.Value*100;
                    var q = (int) (price.Value/5.0);
                    int r;

                    Math.DivRem((int) price.Value, 5, out r);
                    if (r > 0)
                    {
                        q++;
                    }
                    price = q*5;
                    price = price.Value/100;
                    
                }
            }
            catch(Exception e)
            {
                _logger.LogError(e);
            }

            return price;
        }

        public Tariff GetTariffFor(DateTime pickupDate)
        {
            var tariffs = _tariffProvider.GetTariffs().ToArray();

            // Case 1: A tariff exists for the specific date
            var tariff = (from r in tariffs
                where r.Type == (int) TariffType.Day
                where IsDayMatch(r, pickupDate)
                select r).FirstOrDefault();

            // Case 2: A tariff exists for the day of the week
            if (tariff == null)
            {
                tariff = (from r in tariffs
                    where r.Type == (int) TariffType.Recurring
                    where IsRecurringMatch(r, pickupDate)
                    select r).FirstOrDefault();
            }

            // Case 3: Use default tariff
            if (tariff == null)
            {
                tariff = tariffs.FirstOrDefault(x => x.Type == (int) TariffType.Default);
            }

            return tariff;
        }

        private bool IsDayMatch(Tariff tariff, DateTime date)
        {
            if (tariff.Type == (int) TariffType.Day)
            {
                var startTime = tariff.StartTime;
                var endTime = tariff.StartTime.Date.AddHours(tariff.EndTime.Hour).AddMinutes(tariff.EndTime.Minute);

                if (endTime < startTime)
                {
                    //The tariff spans across two days
                    endTime = endTime.AddDays(1);
                }

                return date >= startTime && date < endTime;
            }
            return false;
        }

        private bool IsRecurringMatch(Tariff tariff, DateTime date)
        {
            if (tariff.Type == (int) TariffType.Recurring)
            {
                // Represents the candidate date day of the week value in the DayOfTheWeek enum
                var dayOfTheWeek = 1 << (int) date.DayOfWeek;

                var startTime = DateTime.MinValue.AddHours(tariff.StartTime.Hour).AddMinutes(tariff.StartTime.Minute);
                var endTime = DateTime.MinValue.AddHours(tariff.EndTime.Hour).AddMinutes(tariff.EndTime.Minute);
                var time = DateTime.MinValue.AddHours(date.Hour).AddMinutes(date.Minute);

                if (endTime <= startTime)
                {
                    //The tariff spans across two days
                    if (time < endTime)
                    {
                        //The candidate date is on the second day of the tariff
                        time = time.AddDays(1);
                    }
                    endTime = endTime.AddDays(1);
                }

                // Determine if the candidate date is between start time and end time
                var isInRange = time >= startTime && time < endTime;

                if (isInRange)
                {
                    // Now determine if the day of the week is correct
                    if (startTime.Date == time.Date)
                    {
                        // The candidate date is the same day defined for the tariff
                        return (tariff.DaysOfTheWeek & dayOfTheWeek) == dayOfTheWeek;
                    }
                    if (endTime.Date == time.Date)
                    {
                        // The candidate date is the next day defined for the tariff
                        // We have to check if a tariff exist for the previous day
                        var previousDayOfTheWeek = dayOfTheWeek == (int) DayOfTheWeek.Sunday
                            ? (int) DayOfTheWeek.Saturday
                            : dayOfTheWeek >> 1;
                        return (tariff.DaysOfTheWeek & previousDayOfTheWeek) == previousDayOfTheWeek;
                    }
                }
            }
            return false;
        }
    }
}