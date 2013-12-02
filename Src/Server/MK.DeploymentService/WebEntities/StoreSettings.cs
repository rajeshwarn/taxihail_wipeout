using System.Collections.Generic;


namespace CustomerPortal.Web.Entities
{
    public class StoreSettings
    {
        public StoreSettings()
        {
            UniqueDeviceIdentificationNumber = new List<string>();
        }    

        public string Keywords { get; set; }
        public virtual List<string> UniqueDeviceIdentificationNumber { get; set; }

    }
}