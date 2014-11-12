#region

using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources;
using System.Collections.Generic;

#endregion

namespace apcurium.MK.Booking.Api.Client
{
    public interface IIbsChargeAccountServiceClient
    {
        Task<IbsChargeAccount> GetChargeAccount(string accountNumber, string customerNumber);

        Task<IbsChargeAccountValidation> ValidateChargeAccount(string accountNumber, string customerNumber);

        Task<IEnumerable<IbsChargeAccount>> GetAllChargeAccount();
    }
}