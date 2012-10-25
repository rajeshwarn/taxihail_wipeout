using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel
{
    public class PopularAddressDetails
    {
        [Key]
        public Guid Id { get; set; }

        public string FriendlyName
        {
            get;
            set;
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
