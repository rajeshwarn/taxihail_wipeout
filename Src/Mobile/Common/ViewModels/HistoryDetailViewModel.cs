using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cirrious.MvvmCross.Commands;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;

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

        private bool _isDone;

        public bool IsDone
        {
            get
            {
                return _isDone;
            }
            set { _isDone = value; FirePropertyChanged("IsDone"); }

        }

        private bool _hasRated;

        public bool HasRated
        {
            get
            {
                return _hasRated;
            }
            set { _hasRated = value; FirePropertyChanged("HasRated"); }

        }

        private bool _showRateButton;

        public bool ShowRateButton
        {
            get { return IsDone && !HasRated; }
            set { _showRateButton = value; FirePropertyChanged("ShowRateButton"); }

        }

        public HistoryDetailViewModel()
        {
            
        }

        public HistoryDetailViewModel(string orderId)
        {
            OrderId = orderId;
            HasRated = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderRating(Guid.Parse(orderId)).RatingScores.Any();
            var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(Guid.Parse(orderId));
            IsDone = TinyIoCContainer.Current.Resolve<IBookingService>().IsStatusDone(status.IBSStatusId);
        }

        public MvxRelayCommand NavigateToRatingPage
        {
            get
            {
                return new MvxRelayCommand(() =>
                                               {
                                                   var canRate = IsDone && !HasRated;
                                                   RequestNavigate<BookRatingViewModel>(new { orderId = OrderId, canRate = canRate.ToString() });
                                               });
            }
        }
    }
}