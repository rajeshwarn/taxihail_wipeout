using System;
using System.Collections.Generic;
using System.Linq;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Messages;
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

		private bool _canSubmit;
		public bool CanSubmit
		{
			get
			{
				return _canSubmit;
			}
			set
			{
				if(value != _canSubmit)
				{
					_canSubmit = value;
					FirePropertyChanged("CanSubmit");
				}
			}
			
		}

        private Guid _orderId;

        public Guid OrderId
        {
            get
            {
                return _orderId;
            }
            set { _orderId = value; FirePropertyChanged("OrderId"); }

        }

        public BookRatingViewModel()
        {
            RatingList = TinyIoCContainer.Current.Resolve<IBookingService>().GetRatingType().Select(c => new RatingModel() { RatingTypeId = c.Id, RatingTypeName = c.Name }).OrderBy(c=>c.RatingTypeId).ToList();
            CanRating = false;
        }
       

		public BookRatingViewModel (string orderId, string canRate="false")
		{
			var ratingTypes = TinyIoCContainer.Current.Resolve<IBookingService> ().GetRatingType ();
            RatingList = ratingTypes.Select(c => new RatingModel(canRate: bool.Parse(canRate)) { RatingTypeId = c.Id, RatingTypeName = c.Name }).OrderBy(c=>c.RatingTypeId).ToList();
			foreach (var rating in RatingList) {
				rating.PropertyChanged += HandleRatingPropertyChanged;
			}

			Guid id;
			if (Guid.TryParse (orderId, out id)) {
				OrderId = id;
			}
            CanRating = bool.Parse(canRate);
            if(!CanRating)
            {
                var orderRatings = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderRating(Guid.Parse(orderId));
                Note = orderRatings.Note;
                RatingList = orderRatings.RatingScores.Select(c=> new RatingModel(canRate:false){RatingTypeId = c.RatingTypeId,Score = c.Score,RatingTypeName = c.Name}).OrderBy(c=>c.RatingTypeId).ToList();

            }
        }

        void HandleRatingPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			this.CanSubmit = _ratingList.All(c => c.Score != 0);
        }

        public IMvxCommand RateOrder
        {
            get
            {
                return new MvxRelayCommand(() =>
                {
                    if(_ratingList.All(c => c.Score != 0))
                    {
                        var orderRating = new OrderRatings()
                        {
                            Note = this.Note,
                            OrderId = this.OrderId,
                            RatingScores =
                                this._ratingList.Select(
                                    c => new RatingScore() { RatingTypeId = c.RatingTypeId, Score = c.Score, Name = c.RatingTypeName}).
                                ToList()
                        };
                        try
                        {
                            TinyIoCContainer.Current.Resolve<IBookingService>().SendRatingReview(orderRating);
                            InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new OrderRated(this, OrderId)));
							RequestClose(this);

                        }
                        catch (Exception e)
                        {
                            throw;
                        }
                    }
                });
            }
        }
    }
}