using ServiceStack.ServiceHost;

namespace CMTPayment.Authorization
{
    [Route("merchants/{MerchantToken}/authorize")]
    public class MerchantAuthorizationRequest : IReturn<AuthorizationResponse>
    {
        public MerchantAuthorizationRequest()
        {
            TransactionType = TransactionTypes.Sale;
            CardReaderMethod = CardReaderMethods.CardOnFile;
        }

        public MerchantAuthorizationRequest(AuthorizationRequest request)
        {
            TransactionType = TransactionTypes.Sale;
            CardReaderMethod = CardReaderMethods.CardOnFile;

            Amount = request.Amount;
            CardOnFileToken = request.CardOnFileToken;
            CustomerReferenceNumber = request.CustomerReferenceNumber;
            Cvv2 = request.Cvv2;
            TripId = request.TripId;
            DriverId = request.DriverId;
            Fare = request.Fare;
            Tip = request.Tip;
            Tolls = request.Tolls;
            Surcharge = request.Surcharge;
            Tax = request.Tax;
            Extras = request.Extras;
            ConvenienceFee = request.ConvenienceFee;
            EmployeeId = request.EmployeeId;
            ShiftUuid = request.ShiftUuid;
            FleetToken = request.FleetToken;
            DeviceId = request.DeviceId;
        }

        public string MerchantToken { get; set; }

        public string TransactionType { get; private set; }

        public int Amount { get; set; }

        public int CardReaderMethod { get; private set; }

        public string CardOnFileToken { get; set; }

        public string CustomerReferenceNumber { get; set; }

        public string Cvv2 { get; set; }

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
