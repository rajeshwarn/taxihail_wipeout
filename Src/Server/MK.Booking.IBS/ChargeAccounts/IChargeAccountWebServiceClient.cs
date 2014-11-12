using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse;
using System.Collections.Generic;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources;

namespace apcurium.MK.Booking.IBS.ChargeAccounts
{
    public interface IChargeAccountWebServiceClient
    {
        IbsAccount GetIbsAccount(string accountNumber, string customerNumber);

        IbsAccountValidation ValidateIbsChargeAccount(IEnumerable<string> prompts, string accountNumber, string customerNumber);

        IEnumerable<IbsAccount> GetAllAccount();
    }
}
