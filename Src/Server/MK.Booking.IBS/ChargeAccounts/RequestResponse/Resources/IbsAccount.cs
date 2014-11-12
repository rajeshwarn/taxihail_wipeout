using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources
{
    [DataContract]
    public class IbsAccount
    {
        [DataMember(Name = "prompts")]
        public List<Prompt> Prompts { get; set; }

        [DataMember(Name = "message")]
        public String Message { get; set; }

        [DataMember(Name = "customer_number")]
        public String CustomerNumber { get; set; }

        [DataMember(Name = "number_of_accounts")]
        public Int16 NumberOfAccount { get; set; }

        [DataMember(Name = "account_number")]
        public String AccountNumber { get; set; }
    }
}
