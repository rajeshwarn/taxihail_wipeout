using System;

namespace apcurium.MK.Common.Entity
{
	public class AccountCharge
	{
        public Guid AccountChargeId { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public AccountChargeQuestion[] Questions { get; set; }
	}
}