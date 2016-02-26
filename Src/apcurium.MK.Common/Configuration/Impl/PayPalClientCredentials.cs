using apcurium.MK.Common.Services;

namespace apcurium.MK.Common.Configuration.Impl
{
    public class PayPalClientCredentials
    {
        public PayPalClientCredentials()
        {
#if DEBUG
            ClientId = "Aan0DBDTb5Q6W2eZo3Y1oZbnGFl7C-K79ksF1JVlIZghi_uxANJ3S6fdX8_O";
#endif
        }

		[PropertyEncrypt]
        public string ClientId { get; set; }
    }
}
