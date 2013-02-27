using System;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtReferenceDataServiceClient : IReferenceDataServiceClient
    {
        #region IReferenceDataServiceClient implementation

        public apcurium.MK.Booking.Api.Contract.Resources.ReferenceData GetReferenceData ()
        {
			var result = new ReferenceData();
			result.CompaniesList = new List<ListItem>();
			result.VehiclesList = new List<ListItem>();
			result.PaymentsList = new List<ListItem>();
			result.PickupCityList = new List<ListItem>();
			result.DropoffCityList = new List<ListItem>();
			return result;
        }

        #endregion
    }
}

