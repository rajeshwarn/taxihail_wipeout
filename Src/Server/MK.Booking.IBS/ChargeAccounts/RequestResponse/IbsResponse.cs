namespace apcurium.MK.Booking.IBS.ChargeAccounts.RequestResponse
{
    public class IbsResponse<T>
    {
        public string Status { get; set; }
        public T Result { get; set; }

        public override string ToString()
        {
            return Status + " - " + Result;
        }
    }
}
