using System.Windows.Input;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using ServiceStack.Text;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Models;
using System;
using System.Linq;
using apcurium.MK.Common.Extensions;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class RideSummaryViewModel: PageViewModel
	{
		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IPaymentService _paymentService;
		private readonly IBookingService _bookingService;

		public RideSummaryViewModel(IOrderWorkflowService orderWorkflowService,
			IPaymentService paymentService,
			IBookingService bookingService)
		{
			_orderWorkflowService = orderWorkflowService;
			_paymentService = paymentService;
			_bookingService = bookingService;
		}

		public async void Init(string order, string orderStatus)
		{			
			Order = order.FromJson<Order> ();
			OrderId = Order.Id;
			OrderStatus = orderStatus.FromJson<OrderStatusDetail>();

			if (Settings.RatingEnabled) 
			{
				await InitRating ();
			}
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
				var orderRatings = await _bookingService.GetOrderRatingAsync(OrderId);
				HasRated = orderRatings.RatingScores.Any();
				bool canRate = !HasRated;
				var ratingTypes = _bookingService.GetRatingType();

				if (canRate) {
					RatingList = ratingTypes.Select (c => new RatingModel (canRate) {
						RatingTypeId = c.Id, 
						RatingTypeName = c.Name 
					}).OrderBy (c => c.RatingTypeId).ToList ();
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

        public override void OnViewStarted(bool firstStart = false)
        {
            base.OnViewStarted(firstStart);
			RaisePropertyChanged(() => IsPayButtonShown);
			RaisePropertyChanged(() => IsResendConfirmationButtonShown);
        }

		private Order Order { get; set; }
		private OrderStatusDetail OrderStatus { get; set;}

		public bool IsPayButtonShown
		{
			get
			{
				var setting = _paymentService.GetPaymentSettings();
				var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
				return isPayEnabled 
						&& !(Settings.RatingEnabled && Settings.RatingRequired && !HasRated)     					 // user must rate before paying
						&& setting.PaymentMode != PaymentMethod.RideLinqCmt 			 // payment is processed automatically
						&& !_paymentService.GetPaymentFromCache(Order.Id).HasValue	     // not already paid
						&& (Order.Settings.ChargeTypeId == null 						 // user is paying with a charge account
						|| Order.Settings.ChargeTypeId != Settings.AccountChargeTypeId);
			}
		}

	    public bool IsResendConfirmationButtonShown
	    {
	        get
	        {
				var setting = _paymentService.GetPaymentSettings();
                var isPayEnabled = setting.IsPayInTaxiEnabled || setting.PayPalClientSettings.IsEnabled;
				return isPayEnabled && setting.PaymentMode != PaymentMethod.RideLinqCmt && _paymentService.GetPaymentFromCache(Order.Id).HasValue;
	        }
	    }
			
		bool _hasRated;		
		public bool HasRated 
		{
			get 
			{ 
				return _hasRated;
			}
			set 
			{ 
				_hasRated = value;
				RaisePropertyChanged ();
				RaisePropertyChanged(() => IsPayButtonShown);
			}
		}

		public ICommand ResendConfirmationCommand
        {
            get {
				return this.GetCommand(() =>
                {
					this.Services().Message.ShowMessage("Confirmation", this.Services().Localize["ConfirmationOfPaymentSent"]);
					_paymentService.ResendConfirmationToDriver(Order.Id);
                });
            }
        }

	    public ICommand RateOrder
	    {
	        get
	        {
	            return this.GetCommand(() =>
	            {
					CheckAndSendRatings();
	            });
	        }
	    }

		public ICommand PayCommand {
			get {
				return this.GetCommand (() => 
					{ 
						this.Services().Analytics.LogEvent("PayButtonTapped");
						ShowViewModel<ConfirmCarNumberViewModel> (new 
							{ 
								order = Order.ToJson (), orderStatus = OrderStatus.ToJson () 
							});
					});
			}
		}

		public ICommand PrepareNewOrder
		{
			get
			{
				return this.GetCommand(async () =>{
					var address = await _orderWorkflowService.SetAddressToUserLocation();
					if(address.HasValidCoordinate())
					{
						ChangePresentation(new ZoomToStreetLevelPresentationHint(address.Latitude, address.Longitude));
					}
				});
			}
		}

		public void CheckAndSendRatings()
		{
            if (!Settings.RatingEnabled || HasRated)
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

                // We don't send the review since it's not complete. The user will have the
                // possibility to go back to the order history to rate it later if he so desires
			    return;
			} 
				
			var orderRating = new apcurium.MK.Common.Entity.OrderRatings
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

			_bookingService.SendRatingReview(orderRating);
			HasRated = true;
		}

		public bool CanUserLeaveScreen()
		{
			if (!HasRated 
				&& Settings.RatingEnabled 
				&& Settings.RatingRequired)
			{
				this.Services().Message.ShowMessage(this.Services().Localize["BookRatingErrorTitle"],
													this.Services().Localize["BookRatingErrorMessage"]);
				return false;
			}
			return true;
		}
	}
}

