using System.Collections.Generic;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.IBS.ChargeAccounts
{
    public class ChargeAccountWebServiceClient : BaseServiceClient, IChargeAccountWebServiceClient
    {
        public ChargeAccountWebServiceClient(IServerSettings serverSettings, IBSSettingContainer ibsSettings, ILogger logger)
            : base(ibsSettings, logger)
        {
        }

        public ChargeAccount GetIbsAccount(string accountNumber, string customerNumber)
        {
            var account = Get<ChargeAccountResponse>("/account/corporate/{0}/{1}".FormatWith(accountNumber, customerNumber));
            return account == null 
                ? null 
                : account.Result;
        }

        public ChargeAccountValidation ValidateIbsChargeAccount(IEnumerable<string> prompts, string account_number, string customer_number)
        {
            var validation = Post<ChargeAccountValidationResponse>("/account/validate/", new {prompts, account_number, customer_number});
            return validation.Result;
        }
        
        public IEnumerable<ChargeAccount> GetAllAccount()
        {
            var allAccounts = Get<ChargeAccountCollectionResponse>("/account/corporate/all/");
            return allAccounts.Accounts;
        }
    }
}
