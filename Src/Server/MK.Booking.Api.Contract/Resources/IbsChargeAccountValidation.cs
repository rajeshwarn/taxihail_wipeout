using System.Collections.Generic;
namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class IbsChargeAccountValidation
    {
        public string Message { get; set; }
        public string CustomerNumber { get; set; }
        public int NumberOfAccounts { get; set; }
        public string AccountNumber { get; set; }
        public List<bool> ValidResponse { get; set; }
        public bool Valid { get; set; }
        
        public override string ToString()
        {
            return CustomerNumber.ToString() + " - " + Message;
        }
    }
}
