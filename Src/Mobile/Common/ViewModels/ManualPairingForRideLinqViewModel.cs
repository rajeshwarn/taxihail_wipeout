using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class ManualPairingForRideLinqViewModel: PageViewModel
    {
        private readonly IBookingService _bookingService;
        private string _pairingCode;

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

        public ICommand PairWithRideLinq
        {
            get
            {
                return this.GetCommand(async () =>
                {
                    using (this.Services().Message.ShowProgress())
                    {
                        var orderManualRideLinqDetail = await _bookingService.ManualRideLinqPair(PairingCode);

                        ShowViewModelAndRemoveFromHistory<ManualRideLinqStatusViewModel>(new
                        {
                            orderManualRideLinqDetail = orderManualRideLinqDetail.SerializeToString()
                        });
                    }
                });
            }
        }

    }
}