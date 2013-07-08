using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Interfaces.Commands;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Binding;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class RideSummaryPage  : BaseViewController<RideSummaryViewModel>
	{     
		public RideSummaryPage(MvxShowViewModelRequest request) 
		: base(request)
		{
		}    

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			TitleLabel.Text = Resources.GetValue ("View_BookingStatus_ThankYouTitle");
			MessageLabel.Text = Resources.GetValue ("View_BookingStatus_ThankYouMessage");
			SendRecieptButton.SetTitle (Resources.GetValue ("HistoryDetailSendReceiptButton"), UIControlState.Normal);
			RateButton.SetTitle (Resources.GetValue ("RateBtn"), UIControlState.Normal);

			
			this.AddBindings(new Dictionary<object, string>(){
				{ SendRecieptButton, new B("TouchUpInside","SendReceiptCommand")
									.Add("Hidden", "IsSendReceiptButtonShown", "BoolInverter") },
				{ RateButton, new B("TouchUpInside","NavigateToRatingPage")
									.Add("Hidden", "IsRatingButtonShown", "BoolInverter")  },
			});

		}


		
		

	}
}

