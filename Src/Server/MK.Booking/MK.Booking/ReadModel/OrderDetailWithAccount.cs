using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.ReadModel
{
    public class OrderDetailWithAccount
    {
        public OrderDetailWithAccount()
        {

            PaymentInformation = new PaymentInformationDetails();
            PickupAddress = new Address();
            DropOffAddress = new Address();
        }

        public Guid Id { get; set; }
        
        public Guid AccountId { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public string Phone { get; set; }

        public int IBSAccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IBSOrderId { get; set; }
       
        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public PaymentInformationDetails PaymentInformation { get; set; }

        public int Status { get; set; }

        public double? Fare { get; set; }

        public double? Toll { get; set; }

        public double? Tip { get; set; }

        public bool IsRemovedFromHistory { get; set; }

        public bool IsRated { get; set; }

        public string UserAgent { get; set; }

         

    }
}