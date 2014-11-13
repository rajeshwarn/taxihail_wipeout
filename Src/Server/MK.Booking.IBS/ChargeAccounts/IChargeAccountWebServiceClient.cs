using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse;
using System.Collections.Generic;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources;

namespace apcurium.MK.Booking.IBS.ChargeAccounts
{
    public interface IChargeAccountWebServiceClient
    {
        ChargeAccount GetIbsAccount(string accountNumber, string customerNumber);

        ChargeAccountValidation ValidateIbsChargeAccount(IEnumerable<string> prompts, string accountNumber, string customerNumber);

        IEnumerable<ChargeAccount> GetAllAccount();
    }
}
