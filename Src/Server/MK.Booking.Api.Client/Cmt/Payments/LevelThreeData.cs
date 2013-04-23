using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class LevelThreeData
    {
        [XmlElement("purchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
    }
}
