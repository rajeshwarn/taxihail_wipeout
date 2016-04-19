#region

using System;
using apcurium.MK.Booking.Api.Contract.Validation;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Http;
using apcurium.MK.Common.Http.Response;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [RouteDescription("/admin/tariffs", "GET, POST")]
    [RouteDescription("/admin/tariffs/{Id}", "PUT, DELETE")]
    [TariffNameValidation, TariffRatesValidator]
    public class Tariff
    {
        public Guid Id { get; set; }
        public TariffType Type { get; set; }
        public string Name { get; set; }
        public double MinimumRate { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double PerMinuteRate { get; set; }
        public double MarginOfError { get; set; }
        public double KilometerIncluded { get; set; }
        public DayOfTheWeek DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int? VehicleTypeId { get; set; }
    }

    public class TariffResponse : IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }
}