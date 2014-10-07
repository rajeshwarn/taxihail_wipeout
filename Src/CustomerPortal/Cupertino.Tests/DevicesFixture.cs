using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Cupertino.Tests
{
    [TestClass]
    public class DevicesFixture : BaseFixture
    {
        [TestMethod]
        public void GetSelectedDeviceUDIDsOfProfile_returns_UDIDs_of_profile()
        {
            var response = agent.GetDevicesOfProfile(TestUsername, TestPassword, TestTeam, TestAppId);

            Assert.IsTrue(response.DeviceUDIDs.Contains("2e00e12d970a3be9423369484fdd18f01235c400"));
        }
    }
}
