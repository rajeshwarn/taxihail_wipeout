using System;

namespace CMTPayment
{
    public partial class CmtPaymentServiceClient
    {
        partial void ClientSetup()
        {
            Client.Timeout = new TimeSpan(0, 0, 2, 0, 0);
            Client.LocalHttpWebRequestFilter = SignRequest;
            Client.LocalHttpWebResponseFilter = LogErrorBody;
        }
    }
}
