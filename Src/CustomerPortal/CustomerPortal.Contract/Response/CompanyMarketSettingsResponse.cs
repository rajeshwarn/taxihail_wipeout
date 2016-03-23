﻿using System.Collections.Generic;
using apcurium.MK.Common.Entity;
using CustomerPortal.Contract.Resources;

namespace CustomerPortal.Contract.Response
{
    public class CompanyMarketSettingsResponse
    {
        public CompanyMarketSettingsResponse()
        {
            DispatcherSettings = new DispatcherSettings();
        }

        public string Market { get; set; }

        public bool EnableDriverBonus { get; set; }

        public bool EnableFutureBooking { get; set; }

        public string FutureBookingReservationProvider { get; set; }

        public int FutureBookingTimeThresholdInMinutes { get; set; }

        public bool DisableOutOfAppPayment { get; set; }

        public string ReceiptFooter { get; set; }

        public DispatcherSettings DispatcherSettings { get; set; }

        public bool EnableAppFareEstimates { get; set; }

        public Tariff MarketTariff { get; set; }

        public bool ShowCallDriver { get; set; }

        public IDictionary<string, IDictionary<string, string>> ReceiptLines { get; set; }
    }
}