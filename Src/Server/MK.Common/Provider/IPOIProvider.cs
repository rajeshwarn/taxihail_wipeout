#region

using System.Collections.Generic;
using System.Threading.Tasks;
using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Common.Provider
{
    public interface IPOIProvider
    {
        Task<string> GetPOIRefPickupList(string company, string textMatch, int maxRespSize);
		Task<Airline[]> GetPOIRefAirLineList(string company, string textMatch, int maxRespSize);
    }
}