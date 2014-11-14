using System;

namespace apcurium.MK.Common.Entity
{
	public class AccountCharge
	{
	    public Guid AccountChargeId;
	    public string Number;
	    public string Name;
	    public AccountChargeQuestion[] Questions;
	}
}