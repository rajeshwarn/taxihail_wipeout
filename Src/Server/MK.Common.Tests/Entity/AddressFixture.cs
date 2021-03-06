#region

using System.Text.RegularExpressions;
using apcurium.MK.Common.Entity;
using NUnit.Framework;

#endregion

namespace apcurium.MK.Common.Tests.Entity
{
    [TestFixture]
    public class AddressFixture
    {
        [Test]
        public void DisplayAddress_concatenation_test()
        {
            var address = new Address
            {
                BuildingName = "BuildingName",
                StreetNumber = "13A",
                Street = "Street",
                City = "City",
                State = "State",
                ZipCode = "Zip",
            };

            var expected = "BuildingName - 13A Street, City, State Zip";
            Assert.AreEqual(expected, address.DisplayAddress);
        }

        [Test]
        public void DisplayAddress_should_start_with_BuildingName()
        {
            var address = new Address
            {
                BuildingName = "the building name",
                StreetNumber = "13A",
                Street = "Street name",
                City = "A City"
            };

            Assert.IsTrue(address.DisplayAddress.StartsWith("the building name"));
        }

        [Test]
        public void FullAddress_should_always_contain_city()
        {
            var address1 = new Address
            {
                FriendlyName = "Costco Wholesale",
                StreetNumber = "11000",
                Street = "Garden Grove Blvd",
                City = "Garden Grove",
                FullAddress = "11000 Garden Grove Blvd",
                State = "CA",
                ZipCode = "92843",
                AddressType = "postal"
            };

            Assert.AreEqual("11000 Garden Grove Blvd, Garden Grove, CA 92843", address1.DisplayAddress);
            Assert.AreEqual("11000 Garden Grove Blvd, Garden Grove, CA 92843", address1.FullAddress);
            Assert.AreEqual("Costco Wholesale", address1.DisplayLine1);
            Assert.AreEqual("11000 Garden Grove Blvd, Garden Grove, CA 92843", address1.DisplayLine2);

            var address2 = new Address
            {
                FriendlyName = "Knotts Scary Farm",
                StreetNumber = "",
                Street = null,
                City = "Buena Park",
                FullAddress = "Knotts Scary Farm",
                State = "CA",
                ZipCode = "90620",
                AddressType = "postal",
                AddressLocationType = AddressLocationType.Unspecified
            };

            Assert.AreEqual("Knotts Scary Farm, Buena Park, CA 90620", address2.DisplayAddress);
            Assert.AreEqual("Knotts Scary Farm, Buena Park, CA 90620", address2.FullAddress);
            Assert.AreEqual("Knotts Scary Farm", address2.DisplayLine1);
            Assert.AreEqual("Buena Park, CA 90620", address2.DisplayLine2);
        }

        [Test]
        public void remove_place_category_from_building_name()
        {
            var buildingName = "Gibeau (Orange) Julep (restaurant)";

            var pattern = @"
\(         # Look for an opening parenthesis
[^\)]+     # Take all characters that are not a closing parenthesis
\)$        # Look for a closing parenthesis at the end of the string";
            var result =
                new Regex(pattern, RegexOptions.IgnorePatternWhitespace).Replace(buildingName, string.Empty).Trim();

            Assert.AreEqual("Gibeau (Orange) Julep", result);
        }
    }
}