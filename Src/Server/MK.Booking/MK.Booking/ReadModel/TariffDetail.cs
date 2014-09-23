#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace apcurium.MK.Booking.ReadModel
{
    public class TariffDetail
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }
        public string Name { get; set; }
        public double MinimumRate { get; set; }
        public decimal FlatRate { get; set; }
        public double KilometricRate { get; set; }
        public double PerMinuteRate { get; set; }
        public double MarginOfError { get; set; }
        public double KilometerIncluded { get; set; }
        public int DaysOfTheWeek { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Type { get; set; }
        public int? VehicleTypeId { get; set; }
    }
}