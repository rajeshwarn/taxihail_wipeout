
namespace apcurium.MK.Booking.Api.Services.OrderCreation
{
    public class ChargeAccountValidationResult
    {
        public string[] Prompts { get; set; }

        public int?[] PromptsLength { get; set; }

        public string ChargeTypeKeyOverride { get; set; }

        public bool IsChargeAccountPaymentWithCardOnFile { get; set; }
    }
}
