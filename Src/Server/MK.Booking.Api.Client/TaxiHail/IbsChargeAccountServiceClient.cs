#region

using System.Collections.Generic;
using apcurium.MK.Booking.Api.Client.Extensions;

using System.Globalization;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Braintree;

#endregion

namespace apcurium.MK.Booking.Api.Client.TaxiHail
{
    public class IbsChargeAccountServiceClient : BaseServiceClient
    {
        public IbsChargeAccountServiceClient(string url, string sessionId, IPackageInfo packageInfo)
            : base(url, sessionId, packageInfo)
        {
        }

        public Task<IbsChargeAccount> GetChargeAccount(string accountNumber, string customerNumber)
        {
            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibschargeaccount?AccountNumber={0}&CustomerNumber={1}",
                    accountNumber, customerNumber);
            var result = Client.GetAsync<IbsChargeAccount>(req);
            return result;
        }

        public Task<IbsChargeAccountValidation> ValidateChargeAccount(IbsChargeAccountValidationRequest validationRequest)
        {
            var req = string.Format(CultureInfo.InvariantCulture,
                "/ibschargeaccount/");
            var result = Client.PostAsync<IbsChargeAccountValidation>(req, validationRequest);
            return result;
        }

        public Task<IEnumerable<IbsChargeAccount>> GetAllChargeAccount()
        {
            var req = string.Format(CultureInfo.InvariantCulture, "/ibschargeaccount/");
            var result = Client.GetAsync<IEnumerable<IbsChargeAccount>>(req);
            return result;
        }
    }
}