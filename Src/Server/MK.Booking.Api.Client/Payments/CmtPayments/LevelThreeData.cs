#region

using System.Xml.Serialization;

#endregion

namespace apcurium.MK.Booking.Api.Client.Cmt.Payments
{
    public class LevelThreeData
    {
        [XmlElement("purchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
    }
}