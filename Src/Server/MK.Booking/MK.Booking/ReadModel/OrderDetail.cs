using System;
using System.ComponentModel.DataAnnotations;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderDetail
    {

        public OrderDetail()
        {
            //required by EF
            Settings = new BookingSettingsDetails();
            PickupAddress = new Address();
            DropOffAddress = new Address();
        }

        [Key]
        public Guid Id { get; set; }
        
        public Guid AccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IBSOrderId { get; set; }
       
        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public BookingSettingsDetails Settings { get; set; }

        public int Status { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Tip { get; set; }
    }
}
