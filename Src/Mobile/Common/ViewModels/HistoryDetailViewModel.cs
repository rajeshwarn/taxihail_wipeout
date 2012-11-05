using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cirrious.MvvmCross.Commands;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class HistoryDetailViewModel : BaseViewModel
    {
        private string _orderId;

        public string OrderId
        {
            get
            {
                return _orderId;
            }
            set { _orderId = value; FirePropertyChanged("OrderId"); }

        }

        private bool _canRate;

        public bool CanRate
        {
            get
            {
                return _canRate;
            }
            set { _canRate = value; FirePropertyChanged("CanRate"); }

        }

        public HistoryDetailViewModel()
        {
            
        }

        public HistoryDetailViewModel(string orderId)
        {
            OrderId = orderId;
        }

        public HistoryDetailViewModel(string orderId, string canRate)
        {
            OrderId = orderId;
            CanRate = bool.Parse(canRate);
        }

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                                               {
                                                   RequestNavigate<BookRatingViewModel>(new { orderId = OrderId, canRate = CanRate.ToString() });
                                               });
            }
        }
    }
}