using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class ManualPairingForRideLinqViewModel: BaseViewModel
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

                        ShowViewModelAndRemoveFromHistory<ManualRideLinqDetailsViewModel>(new
                        {
                            orderManualRideLinqDetail = orderManualRideLinqDetail.SerializeToString()
                        });
                    }
                });
            }
        }

    }
}