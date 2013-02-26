using System;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtReferenceDataServiceClient : IReferenceDataServiceClient
    {
        #region IReferenceDataServiceClient implementation

        public apcurium.MK.Booking.Api.Contract.Resources.ReferenceData GetReferenceData ()
        {
            return new ReferenceData();
        }

        #endregion
    }
}

