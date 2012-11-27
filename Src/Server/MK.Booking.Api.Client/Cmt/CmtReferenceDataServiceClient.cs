using System;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtReferenceDataServiceClient : CmtBaseServiceClient, IReferenceDataServiceClient
    {
        public CmtReferenceDataServiceClient(string url, CmtAuthCredentials credentials)
            : base(url, credentials)
        {
            
        }

        #region IReferenceDataServiceClient implementation

        public apcurium.MK.Booking.Api.Contract.Resources.ReferenceData GetReferenceData ()
        {
            return null;
        }

        #endregion
    }
}

