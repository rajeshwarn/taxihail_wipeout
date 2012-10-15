using System;
using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Booking.ReadModel
{
    public class AddressDetails
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

        public bool IsHistoric { get; set; }

        public string FriendlyName
        {
            get; set;
        }

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

        public string BuildingName
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
