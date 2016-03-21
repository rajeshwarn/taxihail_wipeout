#region

using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Booking.Api.Contract.Validation;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface.ServiceModel;

#endregion

namespace apcurium.MK.Booking.Api.Contract.Requests
{
    [Route("/admin/tariffs", "GET, POST")]
    [Route("/admin/tariffs/{Id}", "PUT, DELETE")]
    [TariffNameValidation, TariffRatesValidator]
    public class Tariff
    {
        public Guid Id { get; set; }
        public TariffType Type { get; set; }
        [Required]
        public string Name { get; set; }
        public double MinimumRate { get; set; }
        [Range(0, double.MaxValue)]
        public decimal FlatRate { get; set; }
        [Range(0, double.MaxValue)]
        public double KilometricRate { get; set; }
        [Range(0, double.MaxValue)]
        public double PerMinuteRate { get; set; }
        [Range(0, double.MaxValue)]
        public double MarginOfError { get; set; }
        [Range(0, double.MaxValue)]
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