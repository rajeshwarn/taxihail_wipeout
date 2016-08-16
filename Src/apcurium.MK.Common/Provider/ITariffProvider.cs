using System.Collections.Generic;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Provider
{
    public interface ITariffProvider
    {
        IEnumerable<Tariff> GetTariffs();
    }
}