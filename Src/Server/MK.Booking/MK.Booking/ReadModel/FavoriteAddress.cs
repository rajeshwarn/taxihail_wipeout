using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.ReadModel
{
    public class FavoriteAddress
    {
        [Key]
        public Guid Id { get; set; }
        public Guid AccountId { get; set; }

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
