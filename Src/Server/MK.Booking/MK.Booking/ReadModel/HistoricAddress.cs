using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class HistoricAddress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public string Apartment
        {
            get;
            set;
        }

        public string FullAddress
        {
            get;
            set;
        }

        public string RingCode
        {
            get;
            set;
        }

        public double Longitude
        {
            get;
            set;
        }

        public double Latitude
        {
            get;
            set;
        }
    }
}
