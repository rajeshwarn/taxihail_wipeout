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
				FullAddress = "7250 Rue du Mile End, Montréal, QC H2R 2W1, Canada",
				StreetNumber = "7250",
				Street = "Rue du Mile End",
				City = "Montréal",
				ZipCode = "H2R 2W1",
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
				FullAddress = "7250 Rue du Mile End, Montréal, QC H2R 2W1, Canada",
				StreetNumber = "7250",
				Street = "Rue du Mile End",
				City = "Montréal",
				ZipCode = "H2R 2W1",
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
				FullAddress = "7250 Rue du Mile End, Montréal, QC H2R 2W1, Canada",
				StreetNumber = "7250",
				Street = "Rue du Mile End",
				City = "Montréal",
				ZipCode = "H2R 2W1",
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
				FullAddress = "7250 Rue du Mile End, Montréal, QC H2R 2W1, Canada",
				StreetNumber = "7250",
				Street = "Rue du Mile End",
				City = "Montréal",
				ZipCode = "H2R 2W1",
				State = "QC",
				RingCode = "",
                Latitude = 45.4169,
                Longitude = -75.6951
            };
        }
    }
}