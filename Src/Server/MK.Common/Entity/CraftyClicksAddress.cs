namespace MK.Common.Android.Entity
{
    public class CraftyClicksAddress
    {
        public DeliveryPoint[] Delivery_Points { get; set; }
        public int Delivery_Point_Count { get; set; }
        public string Postal_Country { get; set; }
        public string Traditional_Contry { get; set; }
        public string Town { get; set; }
        public string PostalCode { get; set; }

        public OSGeoCode GeoCode { get; set; }
    }
}