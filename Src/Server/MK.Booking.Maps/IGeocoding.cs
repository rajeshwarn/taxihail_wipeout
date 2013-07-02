﻿using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Common.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace apcurium.MK.Booking.Maps
{
    public interface IGeocoding
    {
        Address[] Search(string addressName, GeoResult geoResult = null);
        Address[] Search(double latitude, double longitude, GeoResult geoResult = null, bool searchPopularAddresses  = false);

    }
}
