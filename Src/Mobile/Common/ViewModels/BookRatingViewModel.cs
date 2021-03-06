using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Models;
using apcurium.MK.Common.Entity;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class BookRatingViewModel : PageViewModel, ISubViewModel<OrderRated>
    {
		private readonly IBookingService _bookingService;

		public BookRatingViewModel(IBookingService bookingService)
		{
			_bookingService = bookingService;
		}

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

		public async void Init(string orderId, bool canRate = false)
		{
			if (orderId != null) 
			{
				Guid id;

				if (Guid.TryParse (orderId, out id)) 
				{
					OrderId = id;
				}

				if (!canRate) 
				{
					var orderRatings = await _bookingService.GetOrderRatingAsync (Guid.Parse (orderId));

					if (orderRatings.OrderId != Guid.Empty) 
					{
						_hasRated = true;
						Note = orderRatings.Note;
						RatingList = orderRatings.RatingScores.Select (c => new RatingModel {
							RatingTypeId = c.RatingTypeId,
							Score = c.Score,
							RatingTypeName = c.Name
						}).OrderBy (c => c.RatingTypeId).ToList ();
					} 
					else 
					{
						// There is no existing rating for this order, so we enable rating. Useful for broken ratings.
						canRate = true;
					}
				}

				if (canRate) 
				{
					var ratingTypes = await _bookingService.GetRatingTypes ();

					RatingList = ratingTypes.Select (c => new RatingModel (true) {
						RatingTypeId = c.Id,
						RatingTypeName = c.Name
					})
						.OrderBy (c => c.RatingTypeId).ToList ();
				}

				CanRate = canRate;
			}
        }

		public ICommand RateOrder
        {
            get
            {
				return this.GetCommand(async () =>
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

					await _bookingService.SendRatingReview(orderRating);
					this.ReturnResult(new OrderRated(this, OrderId));
				});
            }
        }

		public async Task CheckAndSendRatings()
        {
            if (!Settings.RatingEnabled || _hasRated)
            {
                return;
            }

            if (_ratingList.Any(c => c.Score == 0))
            {
                if (Settings.RatingRequired)
                {
                    this.Services().Message.ShowMessage(this.Services().Localize["BookRatingErrorTitle"],
                                                        this.Services().Localize["BookRatingErrorMessage"]);
                    return;
                }
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

            await _bookingService.SendRatingReview(orderRating);
            _hasRated = true;
        }

        bool _hasRated;		

        public bool CanUserLeaveScreen()
        {
            if (!_hasRated
                && Settings.RatingEnabled
                && Settings.RatingRequired
                && !Settings.CanSkipRatingRequired)
            {
                return false;
            }
            return true;
        }
    }
}