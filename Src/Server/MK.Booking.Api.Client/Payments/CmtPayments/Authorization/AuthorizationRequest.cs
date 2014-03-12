﻿#region

using apcurium.MK.Booking.Api.Client.Cmt.Payments;

#endregion

namespace apcurium.MK.Booking.Api.Client.Payments.CmtPayments.Authorization
{
    [Route("v2/merchants/{MerchantToken}/authorize")]
    public class AuthorizationRequest : IReturn<AuthorizationResponse>
    {
        public AuthorizationRequest()
        {
            CardReaderMethod = CardReaderMethods.Manual;
        }

        public string TransactionType { get; set; }

        public int Amount { get; set; }

        public int CardReaderMethod { get; set; }

        public string CardOnFileToken { get; set; }

        public string CurrencyCode { get; set; }

        public string CustomerReferenceNumber { get; set; }

        public string DeviceName { get; set; }

        public string EmployeeId { get; set; }

        public LevelThreeData L3Data { get; set; }

        public string MerchantToken { get; set; }

        public class CardReaderMethods
        {
            public const int Swipe = 0;
            public const int RfidTap = 1;
            public const int Manual = 2;
        }

        public class TransactionTypes
        {
            public const string Sale = "S";
            public const string PreAuthorized = "P";
        }
    }
}