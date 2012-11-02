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
        
        private String _note;

        public string Note
        {
            get { return _note; }
            set { _note = value; FirePropertyChanged("Note"); }
        }


       

        public BookRatingViewModel()
        {
            _ratingList = TinyIoCContainer.Current.Resolve<IBookingService>().GetRatingType().Select(c => new RatingModel() { RatingTypeId = c.Id, RatingTypeName = c.Name }).ToList();
        }




        public IMvxCommand RateOrder
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    var orderRating = new OrderRatings()
                    {
                        Note = this._note,
                        OrderId = Guid.Parse("C03C043B-1532-4EC9-AFB7-7171D02CA1A7"),
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