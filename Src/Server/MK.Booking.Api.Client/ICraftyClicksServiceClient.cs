using System.Threading.Tasks;
using apcurium.MK.Common.Entity;


namespace apcurium.MK.Booking.Api.Client
{
    public interface ICraftyClicksServiceClient
    {
        Task<CraftyClicksAddress> GetAddressInformation(string postalCode );
    }
}