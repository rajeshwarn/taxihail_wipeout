#region

using System;
using System.Collections.Generic;

#endregion

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

        public string AndroidStoreUrl { get; set; }
        

        public string AppleStoreId { get; set; }

        public string AppleStoreUrl { get; set; }


        public DateTime? PublishedDate { get; set; }

        public string PublishedDateDisplay
        {
            get
            {
                if ( PublishedDate.HasValue )
                {
                    return PublishedDate.Value.ToString("MMM dd, yy");
                }
                else
                {
                    return "";
                }

            }
        }
    }
}