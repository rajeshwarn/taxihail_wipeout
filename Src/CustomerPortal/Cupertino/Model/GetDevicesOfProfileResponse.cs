using System.Collections.Generic;

namespace Cupertino.Model
{
    public class GetDevicesOfProfileResponse
    {
        public IEnumerable<string> DeviceUDIDs { get; set; }
        public bool IsSuccessful { get; set; }
        public string ErrorMessage { get; set; }
    }
}