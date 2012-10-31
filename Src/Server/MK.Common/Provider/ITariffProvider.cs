using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Provider
{
    public interface ITariffProvider
    {
        IEnumerable<Tariff> GetTariffs();
    }
}
