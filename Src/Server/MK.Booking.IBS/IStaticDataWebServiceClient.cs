﻿using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.IBS
{
    public interface IStaticDataWebServiceClient
    {
        ListItem[] GetCompaniesList();
        ListItem[] GetVehiclesList(ListItem company);
        ListItem[] GetPaymentsList(ListItem company);        
        string GetZoneByCoordinate(int? providerId, double latitude, double longitude);
    }
}