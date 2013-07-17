using System;
using NUnit.Framework;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Common.Tests.Entity
{
    [TestFixture]
    public class AddressFixture
    {
        [Test]
        public void BookAddress_should_note_contain_building_name()
        {
            var address = new Address
                              {
                                  BuildingName = "the building name",
                                  StreetNumber = "13A",
                                  Street = "Street name"
                              };

            Assert.IsFalse(address.BookAddress.Contains("the building name"));
        }
    }
}