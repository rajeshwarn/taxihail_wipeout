using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    /// <summary>
    /// For pre-authorized transactions, it is necessary to perform a delayed capture. The Capture
    /// resource provides this interface. Authorized transactions will return a transaction ID which 
    /// must be used when calling the Capture resource. If a capture is not sent, the preauthorization will expire after 3 days and the transaction will have to be re-authorized.
    /// </summary>
    public class CmtPaymentCaptureClient
    {
        public static string ENDPOINT = "capture/";
    }
}
