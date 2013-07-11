namespace apcurium.MK.Booking.ReadModel.Query
{
    public interface ICreditCardPaymentDao
    {
        CreditCardPaymentDetail FindByTransactionId(string transactionId);
    }
}