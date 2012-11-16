
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using System.Threading.Tasks;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class HistoryDetailView : MvxBindingTouchViewController<HistoryDetailViewModel>
    {
        //private Order _data;

        #region Constructors

		public HistoryDetailView() 
			: base(new MvxShowViewModelRequest<BookViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
		}
		
		public HistoryDetailView(MvxShowViewModelRequest request) 
			: base(request)
		{
		}
		
		public HistoryDetailView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			ViewModel.PropertyChanged += HandlePropertyChanged;
			ViewModel.Initialize();

            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            this.NavigationItem.HidesBackButton = false;
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_HistoryDetail"), true);

            lblConfirmationNo.Text = Resources.HistoryDetailConfirmationLabel;
            lblRequested.Text = Resources.HistoryDetailRequestedLabel;
            lblOrigin.Text = Resources.HistoryDetailOriginLabel;
            lblDestination.Text = Resources.HistoryDetailDestinationLabel;
            lblStatus.Text = Resources.HistoryDetailStatusLabel;
            lblPickupDate.Text = Resources.HistoryDetailPickupDateLabel;
            lblAptRingCode.Text = Resources.HistoryDetailAptRingCodeLabel;

            btnRebook.SetTitle(Resources.HistoryDetailRebookButton, UIControlState.Normal);
            btnCancel.SetTitle(Resources.StatusActionCancelButton, UIControlState.Normal);
            btnStatus.SetTitle(Resources.HistoryViewStatusButton, UIControlState.Normal);
			btnSendReceipt.SetTitle (Resources.HistoryViewSendReceiptButton, UIControlState.Normal);
			btnRateTrip.SetTitle(Resources.RateBtn, UIControlState.Normal);
			btnViewRating.SetTitle(Resources.ViewRatingBtn, UIControlState.Normal);
		    AppButtons.FormatStandardButton((GradientButton)btnHide, Resources.DeleteButton, AppStyle.ButtonColor.Red );

            btnCancel.TouchUpInside += CancelTouchUpInside;
            btnStatus.TouchUpInside += StatusTouchUpInside;
			btnSendReceipt.TouchUpInside += SendReceiptTouchUpInside;
			btnRateTrip.TouchUpInside += RateTripTouchUpInside;
			btnViewRating.TouchUpInside += ViewRatingTouchUpInside;
            
            btnCancel.Hidden = true;
            btnStatus.Hidden = true;
			btnSendReceipt.Hidden = true;
			btnRateTrip.Hidden = true;
			btnViewRating.Hidden = true;
            
			btnHide.TouchUpInside += HideTouchUpInside;
            btnRebook.TouchUpInside += RebookTouched;

			this.AddBindings(new Dictionary<object, string>()                            {
				{ btnHide, "{'Hidden':{'Path': 'IsCompleted', 'Converter':'BoolInverter'}}"}
			});

        }

        void HandlePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Order") {
				RefreshData();
			} else if(e.PropertyName == "Status") {
				RefreshStatus();
			}
        }

        void StatusTouchUpInside(object sender, EventArgs e)
        {
            AppContext.Current.LastOrder = ViewModel.OrderId;
			InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new RebookRequested(this, null)));
			this.NavigationController.PopToRootViewController( true ); 
        }

        void CancelTouchUpInside(object sender, EventArgs e)
        {
			
            View.UserInteractionEnabled = false;
            
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(ViewModel.OrderId);
                    
                    if (isSuccess)
                    {
						ViewModel.LoadStatus();
                    }
                    else
                    {
                        
                        MessageHelper.Show(Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
                    }
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading();
                        View.UserInteractionEnabled = true;
                    }
                    );
                }
            });
        }

		void SendReceiptTouchUpInside(object sender, EventArgs e)
		{
			View.UserInteractionEnabled = false;
			
			ThreadHelper.ExecuteInThread(() =>
			                             {
				try
				{
					var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().SendReceipt(ViewModel.OrderId);
					
					if (isSuccess)
					{
						MessageHelper.Show(Resources.HistoryViewSendReceiptSuccess);
					}
					else
					{
						
						MessageHelper.Show(Resources.HistoryViewSendReceiptError);
					}
				}
				finally
				{
					InvokeOnMainThread(() =>
					                   {
						LoadingOverlay.StopAnimatingLoading();
						View.UserInteractionEnabled = true;
					}
					);
				}
			}
			);
		}

		void RateTripTouchUpInside (object sender, EventArgs e)
		{
			ViewModel.NavigateToRatingPage.Execute();
		}

		void ViewRatingTouchUpInside (object sender, EventArgs e)
		{
			ViewModel.NavigateToRatingPage.Execute();
		}

        void RebookTouched(object sender, EventArgs e)
        {
			InvokeOnMainThread( () => NavigationController.PopToRootViewController(true) );

			TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new RebookRequested(this, ViewModel.Order));
        }

        void HideTouchUpInside(object sender, EventArgs e)
        {
			ViewModel.DeleteOrder.Execute(ViewModel.OrderId);
        }

        private void RefreshData()
        {
			InvokeOnMainThread (() => {
				var order = ViewModel.Order;
	            if (txtDestination != null)
	            {
					if ( order.IBSOrderId.HasValue )
	                {
						txtConfirmationNo.Text = "#" + order.IBSOrderId.ToString();
	                }
	             

					txtRequested.Text = order.PickupDate.ToShortDateString() + " - " + order.PickupDate.ToShortTimeString();
					txtOrigin.Text = order.PickupAddress.FullAddress;
					txtAptCode.Text = FormatAptRingCode(order.PickupAddress.Apartment, order.PickupAddress.RingCode);
	                
					txtDestination.Text = order.DropOffAddress.FullAddress.HasValue() ? order.DropOffAddress.FullAddress : Resources.ConfirmDestinationNotSpecified;
					txtPickupDate.Text = FormatDateTime(order.PickupDate, order.PickupDate);
	                
	                txtStatus.Text = Resources.LoadingMessage;
	                        
	            }
			});
        }

        private void RefreshStatus()
        {
			var bookingService = TinyIoCContainer.Current.Resolve<IBookingService>();
            
			bool isCompleted = bookingService.IsStatusCompleted(ViewModel.Status.IBSStatusId);
			bool isDone = bookingService.IsStatusDone(ViewModel.Status.IBSStatusId);

            InvokeOnMainThread(() => {
				txtStatus.Text = ViewModel.Status.IBSStatusDescription;
                btnCancel.Hidden = isCompleted;
                btnStatus.Hidden = isCompleted;
				btnRateTrip.Hidden = !(isDone && !ViewModel.HasRated);
				btnViewRating.Hidden = !(isDone && ViewModel.HasRated);
				btnSendReceipt.Hidden = !ViewModel.Status.FareAvailable;
        	});
		}

        private string FormatDateTime(DateTime? pickupDate, DateTime? pickupTime)
        {
            string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString() : Resources.DateToday;
            result += @" / ";
            result += pickupTime.HasValue ? pickupTime.Value.ToShortTimeString() : Resources.TimeNow;
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            string result = apt.HasValue() ? apt : Resources.ConfirmNoApt;
            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.ConfirmNoRingCode;
            return result;
        }
        
        #endregion
    }
}

