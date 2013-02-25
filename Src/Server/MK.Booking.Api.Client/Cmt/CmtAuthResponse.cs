namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtAuthResponse
    {
        public int OauthExpiresInSeconds { get; set; }
        public string OauthToken { get; set; }
        public string OauthTokenSecret { get; set; }
    }
}
