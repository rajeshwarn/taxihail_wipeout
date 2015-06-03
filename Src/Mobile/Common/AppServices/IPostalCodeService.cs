using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.AppServices
{
    public interface IPostalCodeService
    {
        Task<Address[]> GetAddressFromPostalCode(string postalCode);
    }
}