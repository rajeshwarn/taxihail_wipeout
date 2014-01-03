namespace apcurium.MK.Booking.Api.Contract.Security
{
    public class AuthenticationData
    {
        public string SessionId { get; set; }
        public string UserName { get; set; }
        public string AccountId { get; set; }
        public string ReferrerUrl { get; set; }
        public string AccessToken { get; set; }
        public string AccessTokenSecret { get; set; }
    }
}