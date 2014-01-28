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
            set { 
				_ratingList = value; 
				RaisePropertyChanged();
			}
        }
        
        private string _note;
        public string Note
        {
            get { return _note; }
            set { 
				_note = value; 
				RaisePropertyChanged();
			}
        }

		private bool _canRate;
		public bool CanRate
        {
			get { return _canRate; }
            set {
				_canRate = value;
				RaisePropertyChanged(); 
			}
        }

        private Guid _orderId;
        public Guid OrderId
        {
			get { return _orderId; }
            set 
			{
				_orderId = value; 
				RaisePropertyChanged(); 
			}
        }

		public void Init(string orderId, bool canRate = false)
		{
			RatingList = this.Services().Booking.GetRatingType().Select(c => new RatingModel
				{
					RatingTypeId = c.Id, 
					RatingTypeName = c.Name 
				})
				.OrderBy(c=>c.RatingTypeId).ToList();

			CanRate = false;

			if (orderId != null)
			{
				var ratingTypes = this.Services().Booking.GetRatingType();
				RatingList = ratingTypes.Select(c => new RatingModel(canRate) 
					{
						RatingTypeId = c.Id, 
						RatingTypeName = c.Name 
					}).OrderBy(c=>c.RatingTypeId).ToList();

				Guid id;
				if (Guid.TryParse (orderId, out id)) {
					OrderId = id;
				}

				CanRate = canRate;
				if(!CanRate)
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
        }

        public AsyncCommand RateOrder
        {
            get
            {
				return GetCommand(() =>
				{
					if (_ratingList.Any(c => c.Score == 0))
					{
							this.Services().Message.ShowMessage(this.Services().Localize["BookRatingErrorTitle"], this.Services().Localize["BookRatingErrorMessage"]);
						return;
					} 

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
                    
				});
            }
        }
    }
}