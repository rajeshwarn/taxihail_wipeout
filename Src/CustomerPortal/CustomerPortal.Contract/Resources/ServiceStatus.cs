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
        public bool IsSqlAvailable { get; set; }
        public bool IsCustomerPortalAvailable { get; set; }
        public string LastOrderUpdateId { get; set; }
        public string LastOrderUpdateServer { get; set; }
        public string LastOrderUpdateDate { get; set; }
    }
}
