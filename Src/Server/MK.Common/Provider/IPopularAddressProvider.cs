#region

using System.Collections.Generic;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Common.Provider
{
    public interface IPopularAddressProvider
    {
        IEnumerable<Address> GetPopularAddresses();
    }
}