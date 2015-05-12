// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace apcurium.MK.Booking.Services
{
    /// <summary>
    /// See Moneris Response Codes document:
    /// https://developer.moneris.com/Downloads/Canada/Support/Other%20Useful%20Stuff.aspx
    /// </summary>
    public static class MonerisResponseCodes
    {
        public static int DECLINED = 50;

        public static int RESTRICTED_CARD = 59;

        public static int UNABLE_TO_AUTHORIZE = 74;

        public static int INSUFFICIENT_FUNDS = 76;

        public static int PRE_AUTH_FULL = 77;

        public static int MAX_ONLINE_REFUND_REACHED = 79;

        public static int NUMBER_OF_TIMES_USED_EXCEEDED = 82;

        public static int AMOUNT_OVER_MAXIMUM = 95;

        public static int OVER_RETAILER_LIMIT = 106;

        public static int OVER_DAILY_LIMIT = 107;

        public static int USAGE_EXCEEDED = 110;

        public static int OVER_RETAILER_LIMIT_2 = 204;

        public static int AMEX_DECLINED1 = 437;

        public static int AMEX_DECLINED_2 = 438;

        public static int CARD_REJECTED = 476;

        public static int DECLINED_UNKNOWN_ACCOUNT = 477;

        public static int DECLINE_HOLD_CARD = 478;

        public static int DECLINE_PICK_UP_CARD = 479;

        public static int DECLINE_PICK_UP_CARD_2 = 480;

        public static int DECLINE_GENERIC = 481;

        public static int RESTRICTED_CARD_NEG = 902;

        public static int RESTRICTED_CARD_CAF = 903;

        public static int MAX_USE_EXCEEDED = 905;

        public static int CAPTURE_DELINQUENT = 906;

        public static int CAPTURE_OVER_LIMIT = 907;

        public static IEnumerable<int> GetDeclinedCodes()
        {
            return new[]
            {
                RESTRICTED_CARD,
                UNABLE_TO_AUTHORIZE,
                INSUFFICIENT_FUNDS,
                PRE_AUTH_FULL,
                MAX_ONLINE_REFUND_REACHED,
                NUMBER_OF_TIMES_USED_EXCEEDED,
                AMOUNT_OVER_MAXIMUM,
                OVER_RETAILER_LIMIT,
                OVER_DAILY_LIMIT,
                USAGE_EXCEEDED,
                OVER_RETAILER_LIMIT_2,
                AMEX_DECLINED1,
                AMEX_DECLINED_2,
                CARD_REJECTED,
                DECLINED_UNKNOWN_ACCOUNT,
                DECLINE_HOLD_CARD,
                DECLINE_PICK_UP_CARD,
                DECLINE_PICK_UP_CARD_2,
                DECLINE_GENERIC,
                RESTRICTED_CARD_NEG,
                RESTRICTED_CARD_CAF,
                MAX_USE_EXCEEDED,
                CAPTURE_DELINQUENT,
                CAPTURE_OVER_LIMIT
            };
        }
    }
}
