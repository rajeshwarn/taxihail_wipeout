using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Entity;
using System.Net;
using CMTPayment;
using MK.Common.Exceptions;
using apcurium.MK.Booking.Mobile.AppServices.Orders;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Booking.Mobile.Infrastructure;

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
							var orderManualRideLinqDetail = await _bookingService.PairWithManualRideLinq(pairingCode, pickupAddress, _kountSessionId);

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
                    catch (WebServiceException ex)
                    {
                        Logger.LogError(ex);
                        this.Services().Message.ShowMessage(localize["ManualPairingForRideLinQ_Error_Title"], localize["ManualPairingForRideLinQ_Error_Message"]).FireAndForget();
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);

						var errorCode = 0;
						var hasErrorCode = ex.Data != null && ex.Data.Contains("ErrorCode");
						hasErrorCode = hasErrorCode ? int.TryParse(ex.Data["ErrorCode"].ToSafeString(), out errorCode) : false;

						switch (errorCode)
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
                });
            }
        }
    }
}