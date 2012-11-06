using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookRatingViewModel : BaseViewModel
    {
        private List<RatingModel> _ratingList;

        public List<RatingModel> RatingList
        {
            get { return _ratingList; }
            set { _ratingList = value; FirePropertyChanged("RatingList"); }
        }
        
        private string _note;

        public string Note
        {
            get { return _note; }
            set { _note = value; FirePropertyChanged("Note"); }
        }

        private bool _canRating;

        public bool CanRating
        {
            get
            {
                return _canRating;
            }
            set { _canRating = value;FirePropertyChanged("CanRating"); }

        }

        private string _orderId;

        public string OrderId
        {
            get
            {
                return _orderId;
            }
            set { _orderId = value; FirePropertyChanged("OrderId"); }

        }

        public BookRatingViewModel()
        {
            RatingList = TinyIoCContainer.Current.Resolve<IBookingService>().GetRatingType().Select(c => new RatingModel() { RatingTypeId = c.Id, RatingTypeName = c.Name }).ToList();
            CanRating = false;
        }
       

        public BookRatingViewModel(string orderId, string canRate="false")
        {
            RatingList = TinyIoCContainer.Current.Resolve<IBookingService>().GetRatingType().Select(c => new RatingModel(canRate: bool.Parse(canRate)) { RatingTypeId = c.Id, RatingTypeName = c.Name }).ToList();
            OrderId = orderId;
            CanRating = bool.Parse(canRate);
            if(!CanRating)
            {
                var orderRatings = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderRating(Guid.Parse(orderId));
                Note = orderRatings.Note;
                RatingList = orderRatings.RatingScores.Select(c=> new RatingModel(canRate:false){Score = c.Score}).ToList();
            }
        }

        public IMvxCommand RateOrder
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    var orderRating = new OrderRatings()
                    {
                        Note = this.Note,
                        OrderId = Guid.Parse(this.OrderId),
                        RatingScores =
                            this._ratingList.Select(
                                c => new RatingScore() { RatingTypeId = c.RatingTypeId, Score = c.Score }).
                            ToList()
                    };
                    TinyIoCContainer.Current.Resolve<IBookingService>().SendRatingReview(orderRating);
                });
            }
        }

    }
}