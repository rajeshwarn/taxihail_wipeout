using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class EditAutoTipViewModel : PageViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;

        public EditAutoTipViewModel(IOrderWorkflowService orderWorkflowService,
            IPaymentService paymentService,
            IBookingService bookingService)
        {
            _orderWorkflowService = orderWorkflowService;
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

        private PaymentDetailsViewModel _paymentPreferences;
        public PaymentDetailsViewModel PaymentPreferences
        {
            get
            {
                if (_paymentPreferences == null)
                {
                    _paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
                    _paymentPreferences.Start();
                }
                return _paymentPreferences;
            }
        }

        public ICommand SaveAutoTipChangeCommand
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    using (this.Services().Message.ShowProgress())
                    {
                        var activeOrder = await _orderWorkflowService.GetLastActiveOrder();
                        if (activeOrder != null)
                        {
                            bool autoTipUpdated;

                            if (activeOrder.Item1.IsManualRideLinq)
                            {
                                // Manual ride linq rides
                                autoTipUpdated = await _bookingService.UpdateAutoTipForManualRideLinq(activeOrder.Item1.Id, PaymentPreferences.Tip);
                            }
                            else
                            {
                                // Normal rides
                                autoTipUpdated = await _paymentService.UpdateAutoTip(activeOrder.Item1.Id, PaymentPreferences.Tip);
                            }

                            if (autoTipUpdated)
                            {
                                Close(this);
                            }
                            else
                            {
                                this.Services().Message.ShowMessage(this.Services().Localize["Error_EditAutoTipTitle"], this.Services().Localize["Error_EditAutoTipMessage"]);
                            }
                        }
                    }
                });
            }
        }
    }
}