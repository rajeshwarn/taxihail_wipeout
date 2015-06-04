using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.MapDataProvider
{
    public interface IPostalCodeService
    {
        Task<Address[]> GetAddressFromPostalCodeAsync(string postalCode);

        Address[] GetAddressFromPostalCode(string postalCode);


        bool IsValidPostCode(string postalCode);
    }
}