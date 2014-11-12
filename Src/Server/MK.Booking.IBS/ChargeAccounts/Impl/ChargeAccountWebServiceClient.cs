using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources;
using CustomerPortal.Client.Http.Extensions;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse;
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

        public IbsAccount GetIbsAccount(string accountNumber, string customerNumber)
        {
            var account = Get<IbsAccountResponse>("/account/corporate/{0}/{1}".FormatWith(accountNumber, customerNumber));
            return account.Result;
        }

        public IbsAccountValidation ValidateIbsChargeAccount(IEnumerable<string> prompts, string accountNumber, string customerNumber)
        {
            var validation = Post<IbsAccountValidationResponse>("/account/validate/", new {prompts, accountNumber, customerNumber});
            return validation.Result;
        }
        
        public IEnumerable<IbsAccount> GetAllAccount()
        {
            var allAccounts = Get<IbsAccountCollectionResponse>("/account/corporate/all/");
            return allAccounts.Accounts;
        }
    }
}
