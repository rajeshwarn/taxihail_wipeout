using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse;
using apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;

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
            try
            {
                var account = Get<ChargeAccountResponse>("/account/corporate/{0}/{1}".FormatWith(accountNumber, customerNumber));
                return account == null
                    ? null
                    : account.Result;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex);
                return null;
            }
        }

        public ChargeAccountValidation ValidateIbsChargeAccount(IEnumerable<string> prompts, string account_number, string customer_number)
        {
            var content = new {prompts = prompts.ToArray(), account_number, customer_number};

            try
            {
                var validation = Post<ChargeAccountValidationResponse>("/account/validate/", content);
                return validation.Result;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex);
                Logger.LogMessage($"Data sent: {content.ToJson()}");

                return new ChargeAccountValidation()
                {
                    Valid = false,
                    Message = "Validation failed."
                };
            }
        }
        
        public IEnumerable<ChargeAccount> GetAllAccount()
        {
            try
            {
                var allAccounts = Get<ChargeAccountCollectionResponse>("/account/corporate/all/");
                return allAccounts.Accounts;
            }
            catch (HttpRequestException ex)
            {
                Logger.LogError(ex);
                return new ChargeAccount[0];
            }
        }
    }
}
