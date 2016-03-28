
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common.Extensions;

namespace CMTPayment
{
    public static class CmtErrorCodes
    {
        public const int TripNotFound = 441;

        public const int UnableToPair = 103;

		public const int CreditCardDeclinedOnPreauthorization = 104;

		public const int UnablePreauthorizeCreditCard = 110;

        public const int TripUnpaired = 111;

        public const int TripEndedNoPairing = 112;

        public const int PairingTimedOut = 113; 

        public const int CardDeclined = 114;

        public const int PaymentProcessingError = 115;

        /// <summary>
        /// Errors that mean we should stop polling
        /// </summary>
        public static List<int> TerminalErrors
        {
            get
            {
                return new List<int>
                {
                    UnableToPair,
                    CreditCardDeclinedOnPreauthorization,
                    PairingTimedOut,
                    UnablePreauthorizeCreditCard,
                    TripEndedNoPairing,
                    TripUnpaired
                };
            }
        }

        public static bool IsTerminalError(string pairingError)
        {
            // Using EndsWith here since pairingError is saved in the DB with the following pattern: CMT Pairing Error Code: {code}
            return pairingError.HasValueTrimmed() && TerminalErrors.Any(error => pairingError.EndsWith(error.ToString()));
        }

        public static int? ExtractTerminalError(string pairingError)
        {
            if (!pairingError.HasValueTrimmed())
            {
                return null;
            }

            return TerminalErrors
                .Select(error => (int?) error)
                .FirstOrDefault(error => error.HasValue && pairingError.EndsWith(error.Value.ToString()));
        }
    }
}
