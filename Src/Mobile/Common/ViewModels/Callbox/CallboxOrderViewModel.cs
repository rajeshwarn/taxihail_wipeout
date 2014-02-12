using System;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderViewModel : BaseViewModel
    {
        public Guid Id { get; set; }

        public int? IBSOrderId { get; set; }

        public DateTime CreatedDate { get; set; }

        public OrderStatusDetail OrderStatus { get; set; }

        public string FormatedCreatedDate
        {
            get {  return CreatedDate.ToShortTimeString(); }
        }

        public string IBSOrderIdString
        {
            get { return IBSOrderId.ToString(); }
        }

        public AsyncCommand CancelOrder
        {
            get
            {
				return GetCommand(() => this.Services().MessengerHub.Publish(new OrderDeleted(this, Id, null)));
            }
        }

        public bool CanBeCancelled
        {
			get { return !this.Services().Booking.IsCallboxStatusCompleted(OrderStatus.IBSStatusId); }
        }

        public ColorEnum TextColor
        {
            get
            {
				if (OrderStatus.IBSStatusId != null)
                {
					switch (OrderStatus.IBSStatusId)
                    {
                        case "wosWAITING":
                            return ColorEnum.LightGray;
                        case "wosARRIVED":
                            return ColorEnum.Green;
                        case "wosASSIGNED":
                            return ColorEnum.Black;
                    }
                }
                return ColorEnum.LightGray;
            }
        }

        public int TextSize
        {
            get
            {
				if (OrderStatus.IBSStatusId != null && OrderStatus.IBSStatusId.Equals("wosARRIVED"))
                {
                    return 25;
                }
                return 18;
            }
        }
    }
}  