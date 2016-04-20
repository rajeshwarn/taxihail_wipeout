using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Provider
{
    public interface IPopularAddressProvider
    {
        IEnumerable<Address> GetPopularAddresses();

        Task<IEnumerable<Address>> GetPopularAddressesAsync();
    }
}