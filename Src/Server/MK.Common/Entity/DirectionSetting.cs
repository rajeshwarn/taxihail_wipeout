using System.ComponentModel.DataAnnotations;

namespace apcurium.MK.Common.Entity
{
    public enum TarifMode
    {
        [Display(Name = "Use TaxiHail fare calculation")]
        AppTarif,
        [Display(Name = "Use IBS fare calculation")]
        Ibs,
        [Display(Name = "Use both IBS and TaxiHail fare calculations")]
        Both,
        [Display(Name = "Use newer IBS fare calculation using distance")]
        Ibs_Distance
    }

    public enum MapProvider
    {
        Google,
        TomTom
    }
}