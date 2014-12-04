using System.Collections.Generic;
namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class IbsChargeAccount
    {
        public List<Prompt> Prompts { get; set; }
        public string Message { get; set; }
        public string CustomerNumber { get; set; }
        public int NumberOfAccounts { get; set; }
        public string AccountNumber { get; set; }

        public override string ToString()
        {
            return AccountNumber + "/" + CustomerNumber.ToString() + " - " + Message;
        }
    }

    public class Prompt
    {
        public string Caption { get; set; }
        public int Length { get; set; }
        public int PromptNumber { get; set; }
        public string Type { get; set; }
        public bool ToBeValidated { get; set; }

        public override string ToString()
        {
            var validate = ToBeValidated ? " To be Validated" : "Not Validated"; // ? Need to see with Chris L.
            return PromptNumber + " - " + Caption + ", " + Type + "(" + Length + ") - " + validate;
        }
    }
}
