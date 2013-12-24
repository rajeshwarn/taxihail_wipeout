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
        public void BookAddress_concatenation_test()
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

            var expected = "13A Street, City, State Zip";
            Assert.AreEqual(expected, address.BookAddress);
        }

        [Test]
        public void BookAddress_should_not_contain_BuildingName()
        {
            var address = new Address
            {
                BuildingName = "the building name",
                StreetNumber = "13A",
                Street = "Street name",
            };

            Assert.IsFalse(address.BookAddress.Contains("the building name"));
        }

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
            };

            Assert.IsTrue(address.DisplayAddress.StartsWith("the building name"));
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