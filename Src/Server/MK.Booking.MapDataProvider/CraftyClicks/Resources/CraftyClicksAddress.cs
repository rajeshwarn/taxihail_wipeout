namespace apcurium.MK.Booking.MapDataProvider.CraftyClicks.Resources
{
    public class CraftyClicksAddress
    {
        public DeliveryPoint[] Delivery_points { get; set; }
        public int Delivery_point_count { get; set; }
        public string Postal_country { get; set; }
        public string Traditional_contry { get; set; }
        public string Town { get; set; }
        public string Postcode { get; set; }

        public OSGeoCode GeoCode { get; set; }
    }
}