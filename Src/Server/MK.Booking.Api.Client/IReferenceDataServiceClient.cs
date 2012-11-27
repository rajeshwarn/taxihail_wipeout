using System;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Api.Client
{
    public interface IReferenceDataServiceClient
    {
        ReferenceData GetReferenceData();
    }
}

