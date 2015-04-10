using System;
using System.ComponentModel;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Reactive;
using System.Reactive.Linq;
using ServiceStack.ServiceClient.Web;
using Observable = System.Reactive.Linq.Observable;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualPairingForRideLinqViewModel: PageViewModel
    {
        private readonly IBookingService _bookingService;
        private string _pairingCode;
        private string _pairingCodeLeft;
        private string _pairingCodeRight;

        public ManualPairingForRideLinqViewModel(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }


        public string PairingCode
        {
            get { return _pairingCode; }
            set
            {
                _pairingCode = value;
                RaisePropertyChanged();
            }
        }

        public string PairingCodeLeft
        {
            get { return _pairingCodeLeft; }
            set
            {
                _pairingCodeLeft = value;

                PairingCode = PairingCodeLeft + PairingCodeRight;
                RaisePropertyChanged();
            }
        }

        public string PairingCodeRight
        {
            get { return _pairingCodeRight; }
            set
            {
                _pairingCodeRight = value;
                PairingCode = PairingCodeLeft + PairingCodeRight;

                RaisePropertyChanged();
            }
        }

        public ICommand PairWithRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                    {
                        try
                        {
                            using (this.Services().Message.ShowProgress())
                            {
                                var orderManualRideLinqDetail = await _bookingService.ManualRideLinqPair(PairingCode);

                                ShowViewModelAndClearHistory<ManualRideLinqStatusViewModel>(new
                                {
                                    orderManualRideLinqDetail = orderManualRideLinqDetail.SerializeToString()
                                });
                            }
                        }
                        catch (WebServiceException)
                        {
                            this.Services().Message.ShowMessage("Error", "An error occurred while pairing.").HandleErrors();
                        }
                        catch (Exception)
                        {
                            this.Services().Message.ShowMessage("Invalid Code", "The pairing code is invalid.").HandleErrors();
                        }
                        
                    });
            }
        }
    }
}