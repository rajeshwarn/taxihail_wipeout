using System;
using apcurium.MK.Common.Extensions;
using System.Linq;

namespace apcurium.MK.Common.Entity
{
    public class DirectionSetting
    {
        public enum TarifMode
        {
            AppTarif,
            Ibs,
            Both
        }

        public bool NeedAValidTarif { get; set; }
    }
}