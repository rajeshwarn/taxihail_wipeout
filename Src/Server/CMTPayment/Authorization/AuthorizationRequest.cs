using System;
using System.Reflection;
using ServiceStack.ServiceHost;

namespace CMTPayment.Authorization
{
    [Route("merchants/{MerchantToken}/authorize")]
    public class MerchantAuthorizationRequest : AuthorizationRequest, IReturn<AuthorizationResponse>
    {
        private static readonly PropertyInfo[] BaseRequestProperties = typeof(AuthorizationRequest).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        private static readonly Type MerchantRequestType = typeof(MerchantAuthorizationRequest);

        public string MerchantToken { get; set; }

        public MerchantAuthorizationRequest()
        {
        }

        public MerchantAuthorizationRequest(AuthorizationRequest request, string merchantToken)
            : this()
        {
            SetFromBaseRequest(request);
            MerchantToken = merchantToken;
        }

        private void SetFromBaseRequest(AuthorizationRequest request)
        {
            foreach (PropertyInfo baseProperty in BaseRequestProperties)
            {
                var property = MerchantRequestType.GetProperty(baseProperty.Name);
                if (property.CanWrite)
                {
                    property.SetValue(this, baseProperty.GetValue(request));
                }
            }
        }
    }

    [Route("fleet/{FleetToken}/device/{DeviceId}/authorize")]
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