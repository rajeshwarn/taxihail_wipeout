namespace apcurium.MK.Booking.Api.Contract.Resources
{
    public class OrderValidationResult
    {
        public bool HasWarning { get; set; }

        public string Message { get; set; }

		public bool HasError { get; set; }
    }
}