using System;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderViewModel : BaseViewModel
    {
        public Guid Id { get; set; }

        public int? IbsOrderId { get; set; }

        public DateTime CreatedDate { get; set; }

        public OrderStatusDetail OrderStatus { get; set; }

        public string FormatedCreatedDate
        {
            get {  return CreatedDate.ToShortTimeString(); }
        }

        public string IbsOrderIdString
        {
            get { return IbsOrderId.ToString(); }
        }

        public AsyncCommand CancelOrder
        {
            get
            {
				return this.GetCommand(() => this.Services().MessengerHub.Publish(new OrderDeleted(this, Id, null)));
            }
        }

        public bool CanBeCancelled
        {
			get { return !this.Services().Booking.IsCallboxStatusCompleted(OrderStatus.IbsStatusId); }
        }

        public ColorEnum TextColor
        {
            get
            {
				if (OrderStatus.IbsStatusId != null)
                {
					switch (OrderStatus.IbsStatusId)
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
				if (OrderStatus.IbsStatusId != null && OrderStatus.IbsStatusId.Equals("wosARRIVED"))
                {
                    return 25;
                }
                return 18;
            }
        }
    }
}  