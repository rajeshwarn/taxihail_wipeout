using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using ServiceStack.Text;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualPairingForRideLinqViewModel: PageViewModel
    {
        private readonly IBookingService _bookingService;
        private readonly IOrderWorkflowService _orderWorkflowService;
        private string _pairingCodeLeft;
        private string _pairingCodeRight;

        public ManualPairingForRideLinqViewModel(IBookingService bookingService, IOrderWorkflowService orderWorkflowService)
        {
            _bookingService = bookingService;
            _orderWorkflowService = orderWorkflowService;
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
                            // For the RideLinQ "street pick" feature, we need to use the user and not the pin position
                            await _orderWorkflowService.SetAddressToUserLocation();

                            var pickupAddress = await _orderWorkflowService.GetCurrentAddress();
                            var pairingCode = string.Concat(PairingCodeLeft, PairingCodeRight);
                            var orderManualRideLinqDetail = await _bookingService.ManualRideLinqPair(pairingCode, pickupAddress);

                            ShowViewModelAndClearHistory<ManualRideLinqStatusViewModel>(new
                            {
                                orderManualRideLinqDetail = orderManualRideLinqDetail.SerializeToString()
                            });
                        }
                    }
                    catch (WebServiceException)
                    {
                        this.Services().Message.ShowMessage(localize["ManualPairingForRideLinQ_Error_Title"], localize["ManualPairingForRideLinQ_Error_Message"]).HandleErrors();
                    }
                    catch (Exception)
                    {
                        this.Services().Message.ShowMessage(localize["ManualPairingForRideLinQ_InvalidCode_Title"], localize["ManualPairingForRideLinQ_InvalidCode_Message"]).HandleErrors();
                    } 
                });
            }
        }
    }
}