using System;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Api.Contract.Resources
{

    public class Account : BaseDTO  
    {
        public Guid Id { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }  
      
        public string FirstName { get; set; }      
  
        public string LastName { get; set; }        

        public string Phone { get; set; }

        public int IBSAccountid { get; set; }

        public string FacebookId { get; set; }

        public string TwitterId { get; set; }

        public BookingSettings Settings { get; set; }

        public string Language { get; set; }

        public bool IsAdmin { get; set; }

        public Guid? DefaultCreditCard { get; set; }

        public double? DefaultTipAmount { get; set; }

        public double? DefaultTipPercent { get; set; }
    }
}
