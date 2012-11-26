namespace apcurium.MK.Booking.Api.Contract.Resources.Cmt
{
    public class PreCogMessage
    {
        public string Text { get; set; }
        public int Delay { get; set; }
        public bool Wipe { get; set; }
        public PreCogChoice[] Choices { get; set; }
    }
}