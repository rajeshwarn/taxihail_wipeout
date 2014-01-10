using System.Threading.Tasks;
using NUnit.Framework;

namespace apcurium.MK.Web.Tests
{
    public class PaymentSettingsClientFixture : BaseTest
    {
        protected PaymentSettingsClientFixture()
        {
        }

        [TestFixtureSetUp]
        public async override Task TestFixtureSetup()
        {
            await base.TestFixtureSetup();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            base.TestFixtureTearDown();
        }

        [Test]
        public void when_setting_settings()
        {
        }
    }
}