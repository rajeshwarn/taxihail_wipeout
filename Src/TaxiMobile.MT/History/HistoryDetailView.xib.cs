using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TaxiMobile.Controls;
using TaxiMobile.Helper;
using TaxiMobile.Lib.Data;
using TaxiMobile.Lib.Framework.Extensions;
using TaxiMobile.Lib.Practices;
using TaxiMobile.Lib.Services;
using TaxiMobile.Localization;

namespace TaxiMobile.History
{
	public partial class HistoryDetailView : UIViewController
	{
		private HistoryTabView _parent;
		private BookingInfoData _data;
		
		private UIButton _btnGlossyCancel;
		private UIButton _btnGlossyStatus;
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public HistoryDetailView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public HistoryDetailView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public HistoryDetailView (HistoryTabView parent) : base("HistoryDetailView", null)
		{
			_parent = parent;
			Initialize ();
		}

		void Initialize ()
		{
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			var view = AppContext.Current.Controller.GetTitleView (null, Resources.HistoryDetailViewTitle, true);
			
			this.NavigationItem.HidesBackButton = false;
			this.NavigationItem.TitleView = view;
			
			
			lblConfirmationNo.Text = Resources.HistoryDetailConfirmationLabel;
			lblRequested.Text = Resources.HistoryDetailRequestedLabel;
			lblOrigin.Text = Resources.HistoryDetailOriginLabel;
			lblDestination.Text = Resources.HistoryDetailDestinationLabel;
			lblStatus.Text = Resources.HistoryDetailStatusLabel;
			lblPickupDate.Text = Resources.HistoryDetailPickupDateLabel;
			lblAptRingCode.Text = Resources.HistoryDetailAptRingCodeLabel;
			btnHide.SetTitle (Resources.HistoryDetailHideButton, UIControlState.Normal);
			btnRebook.SetTitle (Resources.HistoryDetailRebookButton, UIControlState.Normal);
			
			
			btnCancel.SetTitle (Resources.StatusActionCancelButton, UIControlState.Normal);
			btnStatus.SetTitle (Resources.HistoryViewStatusButton, UIControlState.Normal);
			
			_btnGlossyCancel = GlassButton.Wrap (btnCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor);
			_btnGlossyCancel.TouchUpInside +=CancelTouchUpInside;
			
			_btnGlossyStatus = GlassButton.Wrap (btnStatus, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor);
			_btnGlossyStatus.TouchUpInside +=StatusTouchUpInside;
			
			_btnGlossyCancel.Hidden = true;
			_btnGlossyStatus.Hidden = true;
			
			
			GlassButton.Wrap (btnHide, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside += HideTouchUpInside;			
			GlassButton.Wrap (btnRebook, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor).TouchUpInside += RebookTouched;
			RefreshData ();
		}

		void StatusTouchUpInside (object sender, EventArgs e)
		{
			AppContext.Current.LastOrder = _data.Id;
			AppContext.Current.Controller.Rebook (null);
			this.NavigationController.PopViewControllerAnimated (true);
		}

		void CancelTouchUpInside (object sender, EventArgs e)
		{
			LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
			View.UserInteractionEnabled = false;
			
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					var isSuccess = ServiceLocator.Current.GetInstance<IBookingService> ().CancelOrder (AppContext.Current.LoggedUser, _data.Id);
					
					if (isSuccess)
					{
						RefreshStatus();
					}

					else
					{
						
						MessageHelper.Show (Resources.StatusConfirmCancelRideErrorTitle, Resources.StatusConfirmCancelRideError);
					}
				}
				finally
				{
					InvokeOnMainThread (() =>
					{
						LoadingOverlay.StopAnimatingLoading (this.View);
						View.UserInteractionEnabled = true;
					});
				}
			});
		}

		void RebookTouched (object sender, EventArgs e)
		{
			AppContext.Current.Controller.Rebook (_data);
			this.NavigationController.PopViewControllerAnimated (true);
		}

		void HideTouchUpInside (object sender, EventArgs e)
		{
			_data.Hide = true;
			AppContext.Current.UpdateLoggedInUser (AppContext.Current.LoggedUser, false);
			_parent.Selected ();
			this.NavigationController.PopViewControllerAnimated (true);
		}


		public void LoadData (BookingInfoData data)
		{
			_data = data;
		}

		private void RefreshData ()
		{
			if (txtDestination != null)
			{
				txtConfirmationNo.Text = "#" + _data.Id.ToString ();
				txtRequested.Text = _data.RequestedDateTime.Value.ToShortDateString () + " - " + _data.RequestedDateTime.Value.ToShortTimeString ();
				txtOrigin.Text = _data.PickupLocation.Address;
				txtAptCode.Text = FormatAptRingCode (_data.PickupLocation.Apartment, _data.PickupLocation.RingCode);
				
				txtDestination.Text = _data.DestinationLocation.Address.HasValue () ? _data.DestinationLocation.Address : Resources.ConfirmDestinationNotSpecified;
				txtPickupDate.Text = FormatDateTime (_data.PickupDate, _data.PickupDate);
				
				txtStatus.Text = Resources.LoadingMessage;
				
				
				RefreshStatus ();
				// _data.Status;				
			}
		}

		private void RefreshStatus ()
		{
			ThreadHelper.ExecuteInThread (() =>
			{
			
				
				var status = ServiceLocator.Current.GetInstance<IBookingService> ().GetOrderStatus (AppContext.Current.LoggedUser, _data.Id);
				
				bool isCompleted = ServiceLocator.Current.GetInstance<IBookingService> ().IsCompleted (status.Id);
				
				InvokeOnMainThread (() => txtStatus.Text = Resources.GetValue(status.Status.ToString()));
				InvokeOnMainThread (() => _btnGlossyCancel.Hidden = isCompleted);
				InvokeOnMainThread (() => _btnGlossyStatus.Hidden = isCompleted);
				
			});
		}
		private string FormatDateTime (DateTime? pickupDate, DateTime? pickupTime)
		{
			string result = pickupDate.HasValue ? pickupDate.Value.ToShortDateString () : Resources.DateToday;
			result += @" / ";
			result += pickupTime.HasValue ? pickupTime.Value.ToShortTimeString () : Resources.TimeNow;
			return result;
		}

		private string FormatAptRingCode (string apt, string rCode)
		{
			string result = apt.HasValue () ? apt : Resources.ConfirmNoApt;
			result += @" / ";
			result += rCode.HasValue () ? rCode : Resources.ConfirmNoRingCode;
			return result;
		}
		
		#endregion
	}
}

