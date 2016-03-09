#region

using System;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/tariffs", "GET, POST")]
    [Route("/admin/tariffs/{Id}", "PUT, DELETE")]
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