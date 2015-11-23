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
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
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
			GratuitySelected = new bool[4] { false, false, false, false };
		}

        public async void Init(Guid orderId, bool needToSelectGratuity)
		{			
			OrderId = orderId;
			CanRate = false;
            NeedToSelectGratuity = needToSelectGratuity;

			using (this.Services().Message.ShowProgress())
			{
				if (Settings.RatingEnabled) 
				{
					await InitRating ();
				}
			}
		}

		private bool _needToSelectGratuity;
		public bool NeedToSelectGratuity {
			get
			{
				return _needToSelectGratuity;
			}
			set
			{
				_needToSelectGratuity = value;
				RaisePropertyChanged ();
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
				_bookingService.SetLastUnratedOrderId(OrderId, NeedToSelectGratuity);

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


		private int _selectedGratuity;
		public int SelectedGratuity
		{
			get
			{
				return _selectedGratuity;
			}
			set
			{
				_selectedGratuity = value;
				RaisePropertyChanged();
			}
		}

		private bool[] _gratuitySelected;
		public bool[] GratuitySelected
		{
			get
			{
				return _gratuitySelected;
			}

			set
			{
				_gratuitySelected = value;
				RaisePropertyChanged();
			}

		}

		public ICommand SelectGratuity
		{
			get
			{
				return this.GetCommand<long>(commandParameter =>
					{
						var selectedIndex = (int)commandParameter;
						SelectedGratuity = Gratuity.GratuityOptions[selectedIndex];
						GratuitySelected = new bool[4].Select((x, index) => index == selectedIndex).ToArray();
					});
			}
		}

		public ICommand PayGratuity {
			get {
				return this.GetCommand (async () => {
					await _bookingService.PayGratuity (new Gratuity { OrderId = _orderId, Percentage = SelectedGratuity });
					NeedToSelectGratuity = false;
				});
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
			Logger.LogMessage("CheckAndSendRatings starting");
			if (!Settings.RatingEnabled || HasRated)
			{
				return;
			}

			if (RatingList == null)
			{
				Logger.LogMessage("RatingList is null");

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

				Logger.LogMessage("RatingRequired and some ratings are not set");

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

