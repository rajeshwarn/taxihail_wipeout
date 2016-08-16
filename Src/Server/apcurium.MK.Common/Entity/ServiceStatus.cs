using System;

namespace CustomerPortal.Contract.Resources
{
    public class ServiceStatus
    {
        public bool IsIbsAvailable { get; set; }
        public string IbsUrl { get; set; }
        public bool? IsGeoAvailable { get; set; }
        public string GeoUrl { get; set; }
        public bool? IsHoneyBadgerAvailable { get; set; }
        public string HoneyBadgerUrl { get; set; }
        public bool? IsMapiAvailable { get; set; }
        public string MapiUrl { get; set; }
        public bool? IsPapiAvailable { get; set; }
        public string PapiUrl { get; set; }
        public bool IsSqlAvailable { get; set; }
        public bool IsCustomerPortalAvailable { get; set; }
        public string LastOrderUpdateId { get; set; }
        public string LastOrderUpdateServer { get; set; }
        public DateTime LastOrderUpdateDate { get; set; }

        public DateTime? CycleStartDate { get; set; }

        public bool IsUpdaterDeadlocked { get; set; }


        public bool IsServerHealthy()
        {
            return IsIbsAvailable &&
                   (IsGeoAvailable ?? true) &&
                   (IsHoneyBadgerAvailable ?? true) &&
                   (IsMapiAvailable ?? true) &&
                   (IsPapiAvailable ?? true) &&
                   IsCustomerPortalAvailable &&
                   IsSqlAvailable &&
                   !IsUpdaterDeadlocked;
        }
    }
}
