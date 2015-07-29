using System;
using System.Linq;
using Cirrious.MvvmCross.Binding.BindingContext;
using UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using Foundation;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
	public partial class OrderAirportView : BaseBindableChildView<OrderAirportViewModel>
	{
		// Offset to fix potential issue with iPhone 6+ that would not scroll the viewport completely.
		private const float ScrollingOffset = 3f;

		public OrderAirportView (IntPtr handle) : base(handle)
		{
		}

		private UIView ContentView 
		{
			get { return Subviews[0].Subviews[0].Subviews[0]; }
		}

		private void Initialize()
		{
			BackgroundColor = UIColor.Clear;

			foreach (FlatTextField textField in ContentView.Subviews.Where(x => x is FlatTextField))
			{
				textField.BackgroundColor = UIColor.FromRGB(242, 242, 242);
				DismissKeyboardOnReturn(textField);
			}

			txtFlightNum.Maybe (x => x.ShowCloseButtonOnKeyboard ());
			txtPickupDate.Enabled = false;

			lblAirport.Maybe (x => x.Text = Localize.GetValue ("BookingAirportTitle"));
			lblAirlines.Maybe (x => x.Text = Localize.GetValue ("BookingAirportAirlineslbl"));
			lblFlightNum.Maybe (x => x.Text = Localize.GetValue ("BookingAirportFlightNumlbl"));
			lblPUPoints.Maybe (x => x.Text = Localize.GetValue ("BookingAirportPUPointslbl"));
			lblPickupDate.Maybe ( x=> x.Text = Localize.GetValue("BookingAirportPUDatelbl")); 


			txtAirlines.Configure (Localize.GetValue ("BookingAirportAirlineslbl"), () => ViewModel.Airlines.ToArray (), () => ViewModel.AirlineId, x => ViewModel.AirlineId = x.Id);
			txtPUPoints.Configure (Localize.GetValue ("BookingAirportPUPointslbl"), () => ViewModel.PUPoints.ToArray (), () => ViewModel.PUPointsId, x => ViewModel.PUPointsId = x.Id);

			Foundation.NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.WillShowNotification, ObserveKeyboardShown);
		}

		// Places the visible area of the scrollviewer at the top of the driver note.
		private void ObserveKeyboardShown(NSNotification notification)
		{    
			var isKeyboardVisible = notification.Name == UIKeyboard.WillShowNotification;
			var keyboardFrame = isKeyboardVisible 
				? UIKeyboard.FrameEndFromNotification(notification)
				: UIKeyboard.FrameBeginFromNotification(notification);

			var duration = UIKeyboard.AnimationDurationFromNotification(notification);

			UIView.SetAnimationCurve((UIViewAnimationCurve)UIKeyboard.AnimationCurveFromNotification(notification));

			AnimateAsync(duration, async () => 
				{
					// We need to wait until the default animation from iOS stops before ajusting the scrollviewer to the correct location.
					await Task.Delay(1000);
					var activeView = KeyboardGetActiveView();
					if (activeView == null)
					{
						return;
					}

					var scrollView = activeView.FindSuperviewOfType(this, typeof(UIScrollView)) as UIScrollView;
					if (scrollView == null)
					{
						return;
					}

					var contentInsets = new UIEdgeInsets(0.0f, 0.0f, keyboardFrame.Height, 0.0f);
					scrollView.ContentInset = contentInsets;
					scrollView.ScrollIndicatorInsets = contentInsets;

					// Move the active field to the top of the active view area.
					var offset = activeView.Frame.Y - ScrollingOffset;
					scrollView.ContentOffset = new CoreGraphics.CGPoint(0, offset);
				});
		}

		private void InitializeBinding()
		{
			var set = this.CreateBindingSet<OrderAirportView, OrderAirportViewModel> ();

			set.BindSafe(lblAirport)
				.To(vm => vm.Title);

			set.BindSafe(txtPickupDate)
				.To(vm => vm.PickupTimeStamp);
					
			set.BindSafe (txtFlightNum)
				.To (vm => vm.FlightNum);

			set.BindSafe (txtAirlines)
				.To (vm => vm.AirlineName);

			set.BindSafe (txtPUPoints)
				.To (vm => vm.PUPointsName);
			
			set.Apply();
		}

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();

			var nib = NibHelper.GetNibForView("OrderAirportView");
			var view = (UIView)nib.Instantiate(this, null)[0];
			AddSubview(view);

			Initialize();

			this.DelayBind (() => {
				InitializeBinding();
			});
		}	}
}

