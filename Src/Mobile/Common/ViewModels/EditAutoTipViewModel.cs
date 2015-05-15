using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class EditAutoTipViewModel : PageViewModel
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPaymentService _paymentService;

        public EditAutoTipViewModel(IOrderWorkflowService orderWorkflowService, IPaymentService paymentService)
        {
            _orderWorkflowService = orderWorkflowService;
            _paymentService = paymentService;
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
                            // TODO: Update ride settings?

                            if (activeOrder.Item1.IsManualRideLinq)
                            {
                                // TODO
                                // Manual ride linq rides
                            }
                            else
                            {
                                // Normal rides

                                var autoTipUpdated = await _paymentService.UpdateAutoTip(activeOrder.Item1.Id, PaymentPreferences.Tip);
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
                    }
                });
            }
        }
    }
}