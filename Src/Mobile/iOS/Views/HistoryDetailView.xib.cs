
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
            btnStatus.SetTitle(Resources.HistoryViewStatusButton, UIControlState.Normal);
			btnSendReceipt.SetTitle (Resources.HistoryViewSendReceiptButton, UIControlState.Normal);
			btnRateTrip.SetTitle(Resources.RateBtn, UIControlState.Normal);
			btnViewRating.SetTitle(Resources.ViewRatingBtn, UIControlState.Normal);
		    AppButtons.FormatStandardButton((GradientButton)btnHide, Resources.DeleteButton, AppStyle.ButtonColor.Red );
			AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.StatusActionCancelButton, AppStyle.ButtonColor.Red );

            btnCancel.Hidden = true;
            btnStatus.Hidden = true;
			btnSendReceipt.Hidden = true;
			btnRateTrip.Hidden = true;
			btnViewRating.Hidden = true;
            
			this.AddBindings(new Dictionary<object, string>()                            
            {
				{ btnStatus, "{'Hidden':{'Path': 'IsCompleted'}, 'TouchUpInside':{'Path':'NavigateToOrderStatus'}}"},
				{ btnRateTrip, "{'Hidden':{'Path': 'ShowRateButton', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'NavigateToRatingPage'}}"},
				{ btnViewRating, "{'Hidden':{'Path': 'HasRated', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'NavigateToRatingPage'}}"},
				{ btnCancel, "{'Hidden':{'Path': 'IsCompleted'}, 'TouchUpInside':{'Path':'CancelOrder'}}"},
				{ btnSendReceipt, "{'Hidden':{'Path': 'Status.FareAvailable', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'SendReceipt'}}"},
				{ btnHide, "{'Hidden':{'Path': 'IsCompleted', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'DeleteOrder'}}"},
				{ btnRebook, "{'TouchUpInside':{'Path':'RebookOrder'}}"},
				{ txtConfirmationNo, "{'Text':{'Path': 'ConfirmationTxt'}}"},
				{ txtDestination, "{'Text':{'Path': 'DestinationTxt'}}"},
				{ txtOrigin, "{'Text':{'Path': 'OriginTxt'}}"},
				{ txtRequested, "{'Text':{'Path': 'RequestedTxt'}}"},
				{ txtAptCode, "{'Text':{'Path': 'AptRingTxt'}}"},
				{ txtStatus, "{'Text':{'Path': 'Status.IBSStatusDescription'}}"},
				{ txtPickupDate, "{'Text':{'Path': 'PickUpDateTxt'}}"}
			});

        }
    }
}

#endregion