using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public interface IAccountPaymentService
	{
		Task<AccountCharge> GetAccountCharge (string accountNumber);
		Task<AccountChargeQuestion[]> GetQuestions (string accountNumber);
	}
}
