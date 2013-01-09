using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public class CallboxOrderViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>
    {
        private IBookingService _bookingService;
        public CallboxOrderViewModel()
        {
            _bookingService = this.GetService<IBookingService>();
        }
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

        public IMvxCommand CancelOrder
        {
            get
            {
                return this.GetCommand(() => this.MessengerHub.Publish<OrderDeleted>(new OrderDeleted(this, Id, null)));
            }
        }

        public bool CanBeCancelled
        {
            get { return !_bookingService.IsCallboxStatusCompleted(OrderStatus.IBSStatusId); }
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
                            break;
                        case "wosARRIVED":
                            return ColorEnum.Green;
                            break;
                        case "wosASSIGNED":
                            return ColorEnum.Black;
                            break;
                    };
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