using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Entity;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;
using CMTPayment;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualPairingForRideLinqViewModel: PageViewModel, ISubViewModel<OrderManualRideLinqDetail>
    {
        private readonly IBookingService _bookingService;
        private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IDeviceCollectorService _deviceCollectorService;

        private string _pairingCodeLeft;
        private string _pairingCodeRight;
		private string _kountSessionId;

		public ManualPairingForRideLinqViewModel(IBookingService bookingService, IOrderWorkflowService orderWorkflowService, IDeviceCollectorService deviceCollectorService)
        {
			_bookingService = bookingService;
            _orderWorkflowService = orderWorkflowService;
			_deviceCollectorService = deviceCollectorService;
        }

		public void Init()
		{
			_kountSessionId = _deviceCollectorService.GetSessionId();
		}

        public string PairingCodeLeft
        {
            get { return _pairingCodeLeft; }
            set
            {
                if (_pairingCodeLeft != value)
                {
                    _pairingCodeLeft = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string PairingCodeRight
        {
            get { return _pairingCodeRight; }
            set
            {
                if (_pairingCodeRight != value)
                {
                    _pairingCodeRight = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ICommand PairWithRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    var localize = this.Services().Localize;

                    if (!PairingCodeLeft.HasValue() || !PairingCodeRight.HasValue())
                    {
                        await this.Services().Message.ShowMessage(localize["ManualPairingForRideLinQ_InvalidCode_Title"], localize["ManualPairingForRideLinQ_InvalidCode_Message"]);
                        return;
                    }

                    try
                    {
                        using (this.Services().Message.ShowProgress())
                        {
							await _orderWorkflowService.ValidateTokenizedCardIfNecessary(true, null, _kountSessionId);

                            // For the RideLinQ "street pick" feature, we need to use the user and not the pin position
                            await _orderWorkflowService.SetAddressToUserLocation();

                            var pickupAddress = await _orderWorkflowService.GetCurrentAddress();
                            var pairingCode = string.Concat(PairingCodeLeft, PairingCodeRight);
							var serviceType = await _orderWorkflowService.GetAndObserveServiceType().Take(1).ToTask();
                            var orderManualRideLinqDetail = await _bookingService.PairWithManualRideLinq(pairingCode, pickupAddress, serviceType, _kountSessionId);
							_deviceCollectorService.GenerateNewSessionIdAndCollect();

							this.ReturnResult(orderManualRideLinqDetail);
                        }
                    }
					catch(InvalidCreditCardException e)
					{
						Logger.LogError(e);

						var title = this.Services().Localize["ErrorCreatingOrderTitle"];
						var message = this.Services().Localize["InvalidCreditCardMessage"];

						this.Services().Message.ShowMessage(title, message,
							this.Services().Localize["InvalidCreditCardUpdateCardButton"], () => {
								if(Settings.MaxNumberOfCardsOnFile > 1)
								{
									ShowViewModelAndRemoveFromHistory<CreditCardMultipleViewModel>();
								}
								else
								{
									ShowViewModelAndRemoveFromHistory<CreditCardAddViewModel>();
								}
							},
							this.Services().Localize["Cancel"], () => {});
					}
					catch (ManualPairingException ex)
					{
						Logger.LogError(ex);

						switch (ex.ErrorCode)
						{
							case CmtErrorCodes.CreditCardDeclinedOnPreauthorization:
								this.Services().Message.ShowMessage(localize["PairingProcessingErrorTitle"], localize["CreditCardDeclinedOnPreauthorizationErrorText"]).FireAndForget();
								break;

							case CmtErrorCodes.UnablePreauthorizeCreditCard:
								this.Services().Message.ShowMessage(localize["PairingProcessingErrorTitle"], localize["CreditCardUnanbleToPreathorizeErrorText"]).FireAndForget();
								break;

							case CmtErrorCodes.UnableToPair:
							default:
								this.Services().Message.ShowMessage(localize["PairingProcessingErrorTitle"], localize["TripUnableToPairErrorText"]).FireAndForget();
								break;
						}
					}
                    catch (Exception ex)
                    {
						Logger.LogError(ex);
						this.Services().Message.ShowMessage(localize["PairingProcessingErrorTitle"], localize["TripUnableToPairErrorText"]).FireAndForget();
                    } 
                });
            }
        }
    }
}