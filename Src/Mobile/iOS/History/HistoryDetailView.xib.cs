
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

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class HistoryDetailView : MvxBindingTouchViewController<HistoryDetailViewModel>
    {
        private Order _data;

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
			LoadData(Guid.Parse (ViewModel.OrderId));
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
		    AppButtons.FormatStandardButton((GradientButton)btnHide, Resources.DeleteButton, AppStyle.ButtonColor.Red );

            btnCancel.TouchUpInside += CancelTouchUpInside;
            btnStatus.TouchUpInside += StatusTouchUpInside;
			btnSendReceipt.TouchUpInside += SendReceiptTouchUpInside;
			btnRateTrip.TouchUpInside += RateTripTouchUpInside;
            
            btnCancel.Hidden = true;
            btnStatus.Hidden = true;
			btnSendReceipt.Hidden = true;
			btnRateTrip.Hidden = true;
            
            
            btnHide.TouchUpInside += HideTouchUpInside;          
            btnRebook.TouchUpInside += RebookTouched;
            RefreshData();
        }

        void StatusTouchUpInside(object sender, EventArgs e)
        {
            AppContext.Current.LastOrder = _data.Id;
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
                    var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder( _data.Id);
                    
                    if (isSuccess)
                    {
                        RefreshStatus();
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
            }
            );
        }

		void SendReceiptTouchUpInside(object sender, EventArgs e)
		{
			View.UserInteractionEnabled = false;
			
			ThreadHelper.ExecuteInThread(() =>
			                             {
				try
				{
					var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().SendReceipt( _data.Id);
					
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
			var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
			dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(BookRatingViewModel), null, true, MvxRequestedBy.UserAction));
		}

        void RebookTouched(object sender, EventArgs e)
        {
			InvokeOnMainThread( () => NavigationController.PopToRootViewController(true) );

			TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new RebookRequested(this, _data));
        }

        void HideTouchUpInside(object sender, EventArgs e)
        {
			TinyIoCContainer.Current.Resolve<IBookingService>().RemoveFromHistory( _data.Id );

            this.NavigationController.PopViewControllerAnimated(true);
        }

        public void LoadData(Guid orderId)
        {
			_data = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(orderId);
        }

        private void RefreshData()
        {
            if (txtDestination != null)
            {
                if ( _data.IBSOrderId.HasValue )
                {
                    txtConfirmationNo.Text = "#" + _data.IBSOrderId.ToString();
                }
             

                txtRequested.Text = _data.PickupDate.ToShortDateString() + " - " + _data.PickupDate.ToShortTimeString();
                txtOrigin.Text = _data.PickupAddress.FullAddress;
                txtAptCode.Text = FormatAptRingCode(_data.PickupAddress.Apartment, _data.PickupAddress.RingCode);
                
                txtDestination.Text = _data.DropOffAddress.FullAddress.HasValue() ? _data.DropOffAddress.FullAddress : Resources.ConfirmDestinationNotSpecified;
                txtPickupDate.Text = FormatDateTime(_data.PickupDate, _data.PickupDate);
                
                txtStatus.Text = Resources.LoadingMessage;
                
                
                RefreshStatus();
                // _data.Status;                
            }
        }

        private void RefreshStatus()
        {
            ThreadHelper.ExecuteInThread(() =>
            {
				var bookingService = TinyIoCContainer.Current.Resolve<IBookingService>();
				var status = bookingService.GetOrderStatus( _data.Id);
                
				bool isCompleted = bookingService.IsStatusCompleted(status.IBSStatusId);
				bool isDone = bookingService.IsStatusDone(status.IBSStatusId);

                InvokeOnMainThread(() => {
					txtStatus.Text = status.IBSStatusDescription;
                    btnCancel.Hidden = isCompleted;
                    btnStatus.Hidden = isCompleted;
				    btnHide.Hidden = !isCompleted;
					btnRateTrip.Hidden = !isDone;
				    btnSendReceipt.Hidden = !status.FareAvailable;
            	});
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

