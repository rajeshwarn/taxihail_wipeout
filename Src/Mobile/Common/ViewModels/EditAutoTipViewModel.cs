using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class EditAutoTipViewModel : PageViewModel, ISubViewModel<int>
    {
        private readonly IOrderWorkflowService _orderWorkflowService;
        private readonly IPaymentService _paymentService;
        private readonly IBookingService _bookingService;
		private const int TIP_MAX_PERCENT = 100;

        public EditAutoTipViewModel(IOrderWorkflowService orderWorkflowService,
            IPaymentService paymentService,
            IBookingService bookingService)
        {
            _orderWorkflowService = orderWorkflowService;
            _paymentService = paymentService;
            _bookingService = bookingService;
        }

		public async void Init(int tip = -1)
		{
			if (_paymentPreferences == null)
			{
				_paymentPreferences = Container.Resolve<PaymentDetailsViewModel>();
				await _paymentPreferences.Start();
			}
			if (tip > -1)
			{
				_paymentPreferences.Tip = tip;
			}
			PaymentPreferences = _paymentPreferences;
		}

		private PaymentDetailsViewModel _paymentPreferences;
		public PaymentDetailsViewModel PaymentPreferences 
		{ 
			get{ return _paymentPreferences;} 
			private set 
			{ 
				_paymentPreferences = value; 
				RaisePropertyChanged(); 
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
							if(PaymentPreferences.Tip > TIP_MAX_PERCENT)
						{
							await this.Services().Message.ShowMessage(null, this.Services().Localize["TipPercent_Error"]);
						}
						else
						{
	                        var activeOrder = await _orderWorkflowService.GetLastActiveOrder();
	                        if (activeOrder != null)
	                        {
	                            bool autoTipUpdated;
								
								if (activeOrder.Item1.IsManualRideLinq)
								{
									// Manual ride linq rides
									autoTipUpdated = await _bookingService.UpdateAutoTipForManualRideLinq(activeOrder.Item1.Id, PaymentPreferences.Tip);
									if(autoTipUpdated)
									{
										this.ReturnResult(PaymentPreferences.Tip);
									}
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
									this.Services().Message
										.ShowMessage(this.Services().Localize["Error_EditAutoTipTitle"], this.Services().Localize["Error_EditAutoTipMessage"])
										.FireAndForget();
								}
	                        }
						}
                    }
                });
            }
        }
    }
}