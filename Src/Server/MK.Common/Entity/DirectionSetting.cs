namespace apcurium.MK.Common.Entity
{
    public class DirectionSetting
    {
        public bool NeedAValidTarif { get; set; }
    }

    public enum TarifMode
    {
        AppTarif,
        Ibs,
        Both
    }

    public enum MapProvider
    {
        Google,
        TomTom
    }
}