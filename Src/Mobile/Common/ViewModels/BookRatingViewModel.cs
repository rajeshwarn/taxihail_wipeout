using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BookRatingViewModel : BaseSubViewModel<OrderRated>
    {
        private List<RatingModel> _ratingList;
        public List<RatingModel> RatingList
        {
            get { return _ratingList; }
            set { _ratingList = value; 
				FirePropertyChanged(()=>RatingList); }
        }
        
        private string _note;

        public string Note
        {
            get { return _note; }
            set { 
				_note = value; 
				FirePropertyChanged(()=>Note);
			}
        }

        private bool _canRating;
        public bool CanRating
        {
            get
            {
                return _canRating;
            }
            set {
				_canRating = value;
				FirePropertyChanged(()=>CanRating); 
			}

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
					FirePropertyChanged(()=>CanSubmit);
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
            set 
			{
				_orderId = value; 
				FirePropertyChanged(()=>OrderId); 
			}

        }

        public BookRatingViewModel(string messageId):base(messageId)
        {
            RatingList = this.Services().Booking.GetRatingType().Select(c => new RatingModel
			{
				RatingTypeId = c.Id, 
				RatingTypeName = c.Name 
			})
				.OrderBy(c=>c.RatingTypeId).ToList();
            CanRating = false;
        }
       

		public BookRatingViewModel (string messageId, string orderId, string canRate="false"):base(messageId)
		{
            var ratingTypes = this.Services().Booking.GetRatingType();
            RatingList = ratingTypes.Select(c => new RatingModel(bool.Parse(canRate)) 
			{
				RatingTypeId = c.Id, 
				RatingTypeName = c.Name 
			}).OrderBy(c=>c.RatingTypeId).ToList();

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
                var orderRatings = this.Services().Booking.GetOrderRating(Guid.Parse(orderId));
                Note = orderRatings.Note;
                RatingList = orderRatings.RatingScores.Select(c=> new RatingModel
				{
					RatingTypeId = c.RatingTypeId,
					Score = c.Score,
					RatingTypeName = c.Name
				}).OrderBy(c=>c.RatingTypeId).ToList();

            }
        }

        void HandleRatingPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
			CanSubmit = _ratingList.All(c => c.Score != 0);
        }

        public AsyncCommand RateOrder
        {
            get
            {
                return GetCommand(() =>
                {
                    if(_ratingList.All(c => c.Score != 0))
                    {
                        var orderRating = new OrderRatings
                        {
                            Note = Note,
                            OrderId = OrderId,
                            RatingScores =
                                _ratingList.Select(
                                    c => new RatingScore
								{ 
									RatingTypeId = c.RatingTypeId, 
									Score = c.Score, 
									Name = c.RatingTypeName
								}).ToList()
                        };

                        this.Services().Booking.SendRatingReview(orderRating);
						ReturnResult(new OrderRated(this, OrderId));

                    }
                });
            }
        }
    }
}