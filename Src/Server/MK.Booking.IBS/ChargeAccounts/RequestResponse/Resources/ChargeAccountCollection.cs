using System.Collections.Generic;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources
{
    [DataContract]
    public class ChargeAccountCollection
    {
        [DataMember(Name = "accounts")]
        public List<ChargeAccount> Accounts;
    }
}
