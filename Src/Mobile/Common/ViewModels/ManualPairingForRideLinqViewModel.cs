using System;
using System.ComponentModel;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Reactive;
using System.Reactive.Linq;
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
                        using (this.Services().Message.ShowProgress())
                        {
                            //var orderManualRideLinqDetail = await _bookingService.ManualRideLinqPair(PairingCode);

							var orderManualRideLinqDetail = new OrderManualRideLinqDetail
								{
									PairingCode = "1234567",
									Medallion = "mkt1",
									PairingDate = DateTime.Now,
									Distance = 55,
									Fare = 35,
									Tax = 9,
									Tip = 5,
									Total = (35+9+5)
								};

                            ShowViewModelAndClearHistory<ManualRideLinqStatusViewModel>(new
                            {
                                orderManualRideLinqDetail = orderManualRideLinqDetail.SerializeToString()
                            });
                        }
                    });
            }
        }
    }
}