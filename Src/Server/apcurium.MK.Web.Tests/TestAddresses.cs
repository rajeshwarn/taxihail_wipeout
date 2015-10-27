using apcurium.MK.Common.Entity;

namespace apcurium.MK.Web.Tests
{
    public class TestAddresses
    {
        public static Address GetAddress1()
        {
            return new Address
            {
                Apartment = "3939",
				FullAddress = "1236 Rue St-Hubert, Montréal, QC H2L 2W1, Canada",
				StreetNumber = "1236",
				Street = "Rue St-Hubert",
				City = "Montréal",
				ZipCode = "H2L 2W1",
				State = "QC",
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
				FullAddress = "5254 Rue Ferrier, Montréal, QC H4P 1L3, Canada",
				StreetNumber = "5254",
				Street = "Rue Ferrier",
				City = "Montréal",
				ZipCode = "H4P 1L3",
				State = "QC",
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
				FullAddress = "5196 Rue de la Savane, Montréal, QC H4P 2W1, Canada",
				StreetNumber = "5196",
				Street = "Rue de la Savane",
				City = "Montréal",
				ZipCode = "H4P 2W1",
				State = "QC",
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
				FullAddress = "355 Cooper St, Ottawa, ON K2P 0G8, Canada",
				StreetNumber = "355",
				Street = "Cooper St",
				City = "Montréal",
				ZipCode = "K2P 0G8",
				State = "QC",
				RingCode = "",
                Latitude = 45.4169,
                Longitude = -75.6951
            };
        }
    }
}