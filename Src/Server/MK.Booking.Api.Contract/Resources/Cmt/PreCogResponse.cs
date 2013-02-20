namespace apcurium.MK.Booking.Api.Contract.Resources.Cmt
{
    public class PreCogResponse
    {
        public int? Interval { get; set; }
        public bool Alert { get; set; }
        public bool Init { get; set; }
        public PreCogMessage[] Messages { get; set; }
        public PreCogFeature[] Features { get; set; }
        public PreCogMapItem[] MapItems { get; set; }
    }
}