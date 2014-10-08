using System.Collections.Generic;

namespace Cupertino.Model
{
    public class RegisteredDevices
    {
        public IEnumerable<RegisteredDevice> Devices { get; set; }
    }

    public class RegisteredDevice
    {
        // deviceId to look for in the profiles
        public string DeviceId { get; set; }

        public string Name { get; set; }

        // udid
        public string DeviceNumber { get; set; }

        public string DevicePlatform { get; set; }
        public string Status { get; set; }
    }
}