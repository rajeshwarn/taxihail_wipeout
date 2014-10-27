using System.Xml.Serialization;

namespace CMTPayment
{
    public class LevelThreeData
    {
        [XmlElement("purchaseOrderNumber")]
        public string PurchaseOrderNumber { get; set; }
    }
}