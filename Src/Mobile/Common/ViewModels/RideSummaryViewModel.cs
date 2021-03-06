using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Models;
using System;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: PageViewModel, ISubViewModel<OrderRated>
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IBookingService _bookingService;

		public RideSummaryViewModel(IOrderWorkflowService orderWorkflowService, IBookingService bookingService)
		{
			_orderWorkflowService = orderWorkflowService;
			_bookingService = bookingService;
		}

		public async void Init(Guid orderId)
		{			
			OrderId = orderId;

			CanRate = false;

			using (this.Services().Message.ShowProgress())
			{
				if (Settings.RatingEnabled) 
				{
					await InitRating ();
				}
			}
		}

		private List<RatingModel> _ratingList;
		public List<RatingModel> RatingList
		{
			get { return _ratingList; }
			set
            { 
				_ratingList = value; 
				RaisePropertyChanged();
			}
		}

		private string _note;
		public string Note
		{
			get { return _note; }
			set 
            { 
				_note = value; 
				RaisePropertyChanged();
			}
		}

		private bool _canRate;
		public bool CanRate
		{
			get { return _canRate; }
			set 
            {
				_canRate = value;
				RaisePropertyChanged(); 
				UpdateRatingList ();
			}
		}	
			
		private void UpdateRatingList()
		{
			if (RatingList != null) 
			{
				RatingList.ForEach(x => x.CanRate = _canRate);
				RaisePropertyChanged(() => RatingList);
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

		public async Task InitRating()
		{
			if (OrderId.HasValue())
			{
				// Set the last unrated order here 
				// if the user doesn't do anything and kills the app, we want to set the value
				_bookingService.SetLastUnratedOrderId(OrderId);

				var orderRatings = await _bookingService.GetOrderRatingAsync(OrderId);
				var ratingTypes = await _bookingService.GetRatingTypes();

                HasRated = orderRatings.RatingScores.Any();
				CanRate = !HasRated;

                if (CanRate) 
				{
                    RatingList = ratingTypes.Select(c => new RatingModel(CanRate)
                    {
                        RatingTypeId = c.Id,
                        RatingTypeName = c.Name
                    }).OrderBy(c => c.RatingTypeId).ToList();
				}
				else
				{
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

		bool _hasRated;		
		public bool HasRated 
		{
			get { return _hasRated; }
			set 
			{ 
				_hasRated = value;
				RaisePropertyChanged ();
			}
		}
			
	    public ICommand RateOrderAndNavigateToHome
	    {
	        get
	        {
	            return this.GetCommand(async () =>
	            {
					await CheckAndSendRatings(true);

					if (CanUserLeaveScreen ())
					{
						PrepareNewOrder.ExecuteIfPossible();
						Close(this);
					}
	            });
	        }
	    }

		public ICommand PrepareNewOrder
		{
			get
			{
				return this.GetCommand(async () => 
				{
					_bookingService.ClearLastOrder();
					var address = await _orderWorkflowService.SetAddressToUserLocation();
					if(address.HasValidCoordinate())
					{
						ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
					}
				});
			}
		}

        public async Task CheckAndSendRatings(bool sendRatingButtonWasPressed = false)
		{
			if (!Settings.RatingEnabled || HasRated)
			{
				return;
			}

			if (RatingList == null)
			{
				// Prevent the user from getting stuck on this screen
				HasRated = true;

			    return;
			}

			if (RatingList.Any(c => c.Score == 0))
			{
                if (Settings.RatingRequired                                      // button was pressed, send feedback to user in case of error
                    || (Settings.RatingRequired && sendRatingButtonWasPressed))  // CheckAndSendRatings is also called when exiting the view                       
				{
					this.Services().Message.ShowMessage(this.Services().Localize["BookRatingErrorTitle"],
						this.Services().Localize["BookRatingErrorMessage"]);
				}

				// We don't send the review since it's not complete. The user will have the
				// possibility to go back to the order history to rate it later if he so desires
				return;
			} 

			try
			{
				var orderRating = new OrderRatings
				{
					Note = Note,
					OrderId = OrderId,
					RatingScores =
						RatingList.Select(
							c => new RatingScore
						{ 
							RatingTypeId = c.RatingTypeId, 
							Score = c.Score, 
							Name = c.RatingTypeName
						}).ToList()
					};

				await _bookingService.SendRatingReview(orderRating);
			}
			catch(Exception ex)
			{
				Logger.LogMessage("Error while SendRatingReview");
				Logger.LogError(ex);
			}

			HasRated = true;
			CanRate = false;
		}

		public bool CanUserLeaveScreen()
		{
			if (!HasRated 
				&& Settings.RatingEnabled 
				&& Settings.RatingRequired)
			{
				return false;
			}
			return true;
		}
	}
}