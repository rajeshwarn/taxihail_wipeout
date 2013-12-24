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