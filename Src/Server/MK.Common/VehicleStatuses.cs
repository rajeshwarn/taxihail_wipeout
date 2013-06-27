using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Common
{
    public class VehicleStatuses
    {

        public static string[] DoneStatuses = new[] { Common.Done, Common.Cancelled, Common.CancelledDone, Unknown.None, Common.NoShow };

        public class Common
        {
            public const string Done = "wosDONE";
            public const string Waiting = "wosWAITING";
            public const string Assigned = "wosASSIGNED";
            public const string Arrived = "wosARRIVED";
            public const string Loaded = "wosLOADED";
            public const string NoShow = "wosNOSHOW";
            public const string Scheduled = "wosSCHED";

            public const string Cancelled = "wosCANCELLED";
            public const string CancelledDone = "wosCANCELLED_DONE";
        }

        public class Addresses
        {
            public const string AddressValidQry = "wosAddrValidQry";
            public const string AddressValidating = "wosAddrValidating";
            public const string AddressValidatingTimeout = "wosAddrValidatingTimeout";
            public const string AddressNotValid = "wosAddrNotValid";
            public const string AddressValid = "wosAddrValid";
            public const string AddressPostalValidQry = "wosAddrPostalValidQry";
        }

        public class CreditCards
        {
            public const string CreditCardPreauthorizedQry = "wosCCPreauthQry";
            public const string CreditCardSaleQry = "wosCCSaleQry";
            public const string CreditCardProcessing = "wosCCProcessing";
            public const string CreditCardProcDone = "wosCCProcDone";
            public const string CreditCardEncryptInfo = "wosCCEncryptCCInfo";
            public const string CreditCardEncryptInfoDone = "wosCCEncryptCCInfoDone";
        }

        public class Unknown
        {
            public const string Post = "wosPost";
            public const string Move = "wosMove";
            public const string Moved = "wosMoved";

            public const string None = "wosNone";
            public const string PriceQry = "wosPriceQry";
            public const string PriceCalculating = "wosPriceCalculating";
            public const string PriceDone = "wosPriceDone";
            public const string ReportQry = "wosReportQry";
            public const string ReportProcessing = "wosReportProcessing";
            public const string ReportDone = "wosReportDone";
         }



    }
}
