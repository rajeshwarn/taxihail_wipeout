#region

using apcurium.MK.Common.Entity;

#endregion

namespace apcurium.MK.Web.Tests
{
    public class TestAddresses
    {
        public static Address GetAddress1()
        {
            return new Address
            {
                Apartment = "3939",
                FullAddress = "1234 rue Saint-Hubert",
                RingCode = "3131",
                BuildingName = "Hôtel de Ville",
                Latitude = 45.515065,
                Longitude = -73.558064
            };
        }

        public static Address GetAddress2()
        {
            return new Address
            {
                Apartment = "709",
                FullAddress = "5250 Ferrier",
                RingCode = "777",
                Latitude = 45.498069,
                Longitude = -73.656974
            };
        }


        public static Address GetAddress3()
        {
            return new Address
            {
                Apartment = "204",
                FullAddress = "5200 De la savane",
                RingCode = "",
                Latitude = 45.499242,
                Longitude = -73.658648
            };
        }


        internal static Address GetAddress1InZone()
        {
            return new Address
            {
                Apartment = "204",
                FullAddress = "11 hines road, Kanata, ON K2K 2X1",
                RingCode = "",
                Latitude = 45.4169,
                Longitude = -75.6951
            };
        }
    }
}