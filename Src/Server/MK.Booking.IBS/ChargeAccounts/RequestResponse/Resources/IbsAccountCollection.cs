using System.Collections.Generic;
using System.Runtime.Serialization;

namespace apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources
{
    [DataContract]
    public class IbsAccountCollection
    {
        [DataMember(Name = "accounts")]
        public List<IbsAccount> Accounts;
    }
}
