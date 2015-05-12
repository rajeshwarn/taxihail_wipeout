namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderValidationResult
    {
        public bool HasError { get; set; }

        public bool HasWarning { get; set; }

        public string Message { get; set; }

        public bool AppliesToCurrentBooking { get; set; }

        public bool AppliesToFutureBooking { get; set; }

        public bool DisableFutureBooking { get; set; }
    }
}