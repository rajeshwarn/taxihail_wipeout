using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderDetail
    {

        public OrderDetail()
        {
            //required by EF
            Settings = new BookingSettingsDetails();
        }

        [Key]
        public Guid Id { get; set; }
        
        public Guid AccountId { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime RequestedDateTime { get; set; }

        public string PickupAddress { get; set; }

        public double PickupLongitude { get; set; }

        public double PickupLatitude { get; set; }

        public string PickupApartment { get; set; }

        public string PickupRingCode { get; set; }

        public string DropOffAddress { get; set; }

        public double? DropOffLongitude { get; set; }

        public double? DropOffLatitude { get; set; }

        public BookingSettingsDetails Settings { get; set; }

        public string Status { get; set; }
    }
}
