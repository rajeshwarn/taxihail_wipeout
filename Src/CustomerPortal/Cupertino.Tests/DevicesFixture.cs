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

            Assert.IsTrue(response.DeviceUDIDs.Contains("b28f53617e9e79c987003e3dba98ed586cd35f1c"));
        }
    }
}
