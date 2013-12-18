
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
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.BindingConverter;
using System.Drawing;
using System.Threading;
using System.Reactive.Linq;
using apcurium.MK.Booking.Mobile.Client.Extensions;

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
			lblAptRingCode.Text = Resources.HistoryDetailAptRingCodeLabel;
            lblDestination.Text = Resources.HistoryDetailDestinationLabel;
			lblDestination.Hidden = ViewModel.HideDestination;
			lblPickupDate.Text = Resources.HistoryDetailPickupDateLabel;
            lblStatus.Text = Resources.HistoryDetailStatusLabel;
			lblAuthorization.Text = Resources.HistoryDetailAuthorizationLabel;
           
            btnRebook.SetTitle(Resources.HistoryDetailRebookButton, UIControlState.Normal);
            btnStatus.SetTitle(Resources.HistoryViewStatusButton, UIControlState.Normal);
			btnSendReceipt.SetTitle (Resources.HistoryViewSendReceiptButton, UIControlState.Normal);
			btnRateTrip.SetTitle(Resources.RateBtn, UIControlState.Normal);
			btnViewRating.SetTitle(Resources.ViewRatingBtn, UIControlState.Normal);
		    AppButtons.FormatStandardButton((GradientButton)btnHide, Resources.DeleteButton, AppStyle.ButtonColor.Red );
			AppButtons.FormatStandardButton((GradientButton)btnCancel, Resources.StatusActionCancelButton, AppStyle.ButtonColor.Red );

			this.AddBindings(new Dictionary<object, string>()                            
            {
				{ btnRebook, "{'Hidden':{'Path': 'RebookIsAvailable', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'RebookOrder'}}"},

				{ btnHide, "{'Hidden':{'Path': 'IsCompleted', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'DeleteOrder'}}"},
				{ btnStatus, "{'Hidden':{'Path': 'IsCompleted'}, 'TouchUpInside':{'Path':'NavigateToOrderStatus'}}"},

				{ btnCancel, "{'Hidden':{'Path': 'IsCompleted'}, 'TouchUpInside':{'Path':'CancelOrder'}}"},
				{ btnRateTrip, "{'Hidden':{'Path': 'ShowRateButton', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'NavigateToRatingPage'}}"},
				{ btnViewRating, "{'Hidden':{'Path': 'HasRated', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'NavigateToRatingPage'}}"},

				{ btnSendReceipt, "{'Hidden':{'Path': 'SendReceiptAvailable', 'Converter':'BoolInverter'}, 'TouchUpInside':{'Path':'SendReceipt'}}"},

                { txtConfirmationNo, "{'Text':{'Path': 'ConfirmationTxt'}}"},
				{ txtDestination, "{'Text':{'Path': 'DestinationTxt'}, 'Hidden':{'Path': 'HideDestination'}}"},
				{ txtOrigin, "{'Text':{'Path': 'OriginTxt'}}"},
				{ txtRequested, "{'Text':{'Path': 'RequestedTxt'}}"},
				{ txtAptCode, "{'Text':{'Path': 'AptRingTxt'}}"},
				{ txtStatus, "{'Text':{'Path': 'Status.IBSStatusDescription'}}"},
				{ txtPickupDate, "{'Text':{'Path': 'PickUpDateTxt'}}"},
                
                { lblAuthorization, new B("Hidden","AuthorizationNumber",typeof(NoValueToTrueConverter))},
                { txtAthorization, "{'Text':{'Path': 'AuthorizationNumber'}}"}
			});

			ViewModel.Loaded+= (sender, e) => {
				InvokeOnMainThread(()=>{
					ReorderButtons();
					ReorderLabels();
				});
			};

            ViewModel.Load();

            this.View.ApplyAppFont ();
        }

		void ReorderButtons()
		{
			var yPositionOfFirstButton = btnRebook.Frame.Y;
			var deltaYBetweenButtons = btnHide.Frame.Y - yPositionOfFirstButton;

			var buttonList = new List<UIButton> ();
			buttonList.Add (btnRebook);
			buttonList.Add (btnStatus);
			buttonList.Add (btnRateTrip);
			buttonList.Add (btnViewRating);
			buttonList.Add (btnSendReceipt);
			buttonList.Add (btnHide);
			buttonList.Add (btnCancel);

			var i = 0;
			foreach (var item in buttonList) {
				if (!item.Hidden) {
					var test = item.Title(UIControlState.Normal);
					item.Frame = new RectangleF(item.Frame.X, yPositionOfFirstButton + (deltaYBetweenButtons * i), item.Frame.Width, item.Frame.Height);
					i++;
				}
			}
		}

		void ReorderLabels()
		{
			var yPositionOfFirstLabel = lblConfirmationNo.Frame.Y;
			var deltaYBetweenLabels = lblRequested.Frame.Y - yPositionOfFirstLabel;

			var labelList = new List<Tuple<UILabel,UILabel>> ();
			labelList.Add (new Tuple<UILabel, UILabel>(lblConfirmationNo, txtConfirmationNo));
			labelList.Add (new Tuple<UILabel, UILabel>(lblRequested, txtRequested));
			labelList.Add (new Tuple<UILabel, UILabel>(lblOrigin, txtOrigin));
			labelList.Add (new Tuple<UILabel, UILabel>(lblAptRingCode, txtAptCode));
			labelList.Add (new Tuple<UILabel, UILabel>(lblDestination, txtDestination));
			labelList.Add (new Tuple<UILabel, UILabel>(lblPickupDate, txtPickupDate));
			labelList.Add (new Tuple<UILabel, UILabel>(lblStatus, txtStatus));
			labelList.Add (new Tuple<UILabel, UILabel>(lblAuthorization, txtAthorization));

			var i = 0;
			foreach (var item in labelList) {
				if (!item.Item1.Hidden) {
					item.Item1.SetY(yPositionOfFirstLabel + (deltaYBetweenLabels * i));
					item.Item2.SetY(yPositionOfFirstLabel + (deltaYBetweenLabels * i));
					i++;
				}
			}
		}
    }
}

#endregion