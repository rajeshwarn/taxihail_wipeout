﻿#region

using System.Globalization;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using ServiceStack.ServiceInterface;

#endregion

namespace apcurium.MK.Booking.Api.Services.Maps
{
    public class DefaultGeoLocationService : Service
    {
        private readonly IServerSettings _serverSettings;

        public DefaultGeoLocationService(IServerSettings serverSettings)
        {
            _serverSettings = serverSettings;
        }

        public object Get(DefaultGeoLocationRequest request)
        {
            return new Address
            {
                Latitude = _serverSettings.ServerData.GeoLoc.DefaultLatitude,
                Longitude = _serverSettings.ServerData.GeoLoc.DefaultLongitude
            };
        }
    }
}