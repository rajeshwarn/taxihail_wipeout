using System;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Enumeration;

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

        public string Vehicle { get; set; }

        public int IBSAccountId { get; set; }

        public DateTime PickupDate { get; set; }

        public DateTime CreatedDate { get; set; }

        public int? IBSOrderId { get; set; }
       
        public Address PickupAddress { get; set; }

        public Address DropOffAddress { get; set; }

        public PaymentInformationDetails PaymentInformation { get; set; }

        public int Status { get; set; }

        public double? MdtFare { get; set; }

        public double? MdtToll { get; set; }

        public double? MdtTip { get; set; }

        public bool IsRemovedFromHistory { get; set; }

        public bool IsRated { get; set; }

        public string UserAgent { get; set; }


        public string VehicleNumber { get; set; }

        public string VehicleType { get; set; }

        public string VehicleMake { get; set; }

        public string VehicleModel { get; set; }

        public string VehicleColor { get; set; }

        public string VehicleRegistration { get; set; }

        public string DriverFirstName { get; set; }

        public string DriverLastName { get; set; }


        public  decimal? PaymentMeterAmount  { get; set; }

        public  decimal? PaymentTipAmount { get; set; }

        public  decimal? PaymentTotalAmount { get; set; }

        


        public string CardToken { get; set; }
        public string PayPalToken { get; set; }

        public PaymentType? PaymentType { get; set; }
        public PaymentProvider? PaymentProvider { get; set; }

        public string PayPalPayerId { get; set; }
        public string TransactionId { get; set; }
        public string AuthorizationCode { get; set; }

        public bool IsCancelled { get; set; }
        public bool IsCompleted { get; set; }


    }
}