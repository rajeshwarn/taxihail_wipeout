#region

using System.Collections.Generic;

#endregion
// ReSharper disable once CheckNamespace
namespace CustomerPortal.Web.Entities
{
    public class StoreSettings
    {
        public StoreSettings()
        {
            UniqueDeviceIdentificationNumber = new List<string>();
        }

        public string Keywords { get; set; }
        public List<string> UniqueDeviceIdentificationNumber { get; set; }
    }
}