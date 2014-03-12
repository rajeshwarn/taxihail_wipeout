#region

using ServiceStack.ServiceHost;

#endregion



namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Tokenize
{
    [Route("v2/tokenize/{CardToken}/")]
    public class TokenizeDeleteRequest : IReturn<TokenizeDeleteResponse>
    {
        public string CardToken { get; set; }
    }
}