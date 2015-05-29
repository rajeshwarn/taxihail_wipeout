using System.Threading.Tasks;
using MK.Common.Android.Entity;

namespace apcurium.MK.Booking.Api.Client
{
    public interface ICraftyClicksServiceClient
    {
        Task<CraftyClicksAddress> GetAddressInformation(string postalCode );
    }
}