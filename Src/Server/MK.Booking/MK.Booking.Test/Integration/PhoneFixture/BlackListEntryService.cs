using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Booking.Test.Integration.CompanyFixture;
using NUnit.Framework;

namespace apcurium.MK.Booking.Test.Integration.PhoneFixture
{
    [TestFixture]
    public class given_a_phone_numer_to_blacklist : given_a_read_model_database
    {
        protected BlackListEntryService Sut;

        public given_a_phone_numer_to_blacklist()
        {
            Sut = new BlackListEntryService(() => new BookingDbContext(DbName));
        }

        [Test]
        public void when_adding_an_entry_to_blacklist()
        {
            Sut.DeleteAll();

            var entry = new BlackListEntry {Id = Guid.NewGuid(), PhoneNumber = "5145145145"};

            Sut.Add(entry);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Query<BlackListEntry>().FirstOrDefault(c => c.Id.Equals(entry.Id));

                Assert.NotNull(dto);
                Assert.AreEqual("5145145145", dto.PhoneNumber);
            }
        }

        [Test]
        public void when_deleting_a_phone_number_to_blacklist()
        {
            Sut.DeleteAll();

            var entry = new BlackListEntry {Id = Guid.NewGuid(), PhoneNumber = "5145145145"};
            Sut.Add(entry);

            Sut.Delete(entry.Id);

            using (var context = new BookingDbContext(DbName))
            {
                var dto = context.Query<BlackListEntry>().FirstOrDefault(c => c.Id.Equals(entry.Id));

                Assert.IsNull(dto);
            }
        }

        [Test]
        public void when_getting_all_blacklisted_phones()
        {
            Sut.DeleteAll();

            Sut.Add(new BlackListEntry {Id = Guid.NewGuid(), PhoneNumber = "5145145145" });
            Sut.Add(new BlackListEntry {Id = Guid.NewGuid(), PhoneNumber = "4384384384" });

            var result = Sut.GetAll();

            Assert.NotNull(result);
            Assert.Equals(result.Count, 2);
        }

        [Test]
        public void when_finding_a_blacklisted_phone()
        {
            Sut.DeleteAll();

            var entry = new BlackListEntry {Id = Guid.NewGuid(), PhoneNumber = "5145145145"};

            Sut.Add(entry);

            var result = Sut.FindByPhoneNumber(entry.PhoneNumber);

            Assert.NotNull(result);
            Assert.AreEqual("5145145145", result.PhoneNumber);
            Assert.AreEqual(entry.Id, result.Id);
        }
    }
}
