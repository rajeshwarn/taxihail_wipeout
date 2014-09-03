#region

using apcurium.MK.Booking.Api.Client.Cmt.Payments;
using ServiceStack.ServiceHost;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Authorization
{
    [Route("v2/fleet/{FleetToken}/device/{DeviceId}/authorize")]
    public class AuthorizationRequest : IReturn<AuthorizationResponse>
    {
        public AuthorizationRequest()
        {
            TransactionType = TransactionTypes.Sale;
            CardReaderMethod = CardReaderMethods.CardOnFile;
        }

        public string TransactionType { get; private set; }

        public int Amount { get; set; }

        public int CardReaderMethod { get; private set; }

        public string CardOnFileToken { get; set; }

        public string CustomerReferenceNumber { get; set; }

        public int TripId { get; set; }

        public int DriverId { get; set; }

        public int Fare { get; set; }

        public int Tip { get; set; }

        public int Tolls { get; set; }

        public int Surcharge { get; set; }

        public int Tax { get; set; }

        public int Extras { get; set; }

        public int ConvenienceFee { get; set; }

        public string EmployeeId { get; set; }

        public string ShiftUuid { get; set; }

        public string FleetToken { get; set; }

        public string DeviceId { get; set; }

        public class CardReaderMethods
        {
            public const int Swipe = 0;
            public const int RfidTap = 1;
            public const int Manual = 2;
            public const int CardOnFile = 6;
        }

        public class TransactionTypes
        {
            public const string Sale = "S";
            public const string PreAuthorized = "P";
        }
    }
}