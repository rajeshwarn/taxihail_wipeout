
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class HistoryDetailView : UIViewController
    {
        private HistoryTabView _parent;
        private Order _data;
        
        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        public HistoryDetailView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public HistoryDetailView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public HistoryDetailView(HistoryTabView parent) : base("HistoryDetailView", null)
        {
            _parent = parent;
            Initialize();
        }

        void Initialize()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            var view = AppContext.Current.Controller.GetTitleView(null, Resources.HistoryDetailViewTitle, true);
            
            this.NavigationItem.HidesBackButton = false;
            this.NavigationItem.TitleView = view;
            
            
            lblConfirmationNo.Text = Resources.HistoryDetailConfirmationLabel;
            lblRequested.Text = Resources.HistoryDetailRequestedLabel;
            lblOrigin.Text = Resources.HistoryDetailOriginLabel;
            lblDestination.Text = Resources.HistoryDetailDestinationLabel;
            lblStatus.Text = Resources.HistoryDetailStatusLabel;
            lblPickupDate.Text = Resources.HistoryDetailPickupDateLabel;
            lblAptRingCode.Text = Resources.HistoryDetailAptRingCodeLabel;
            btnHide.SetTitle(Resources.HistoryDetailHideButton, UIControlState.Normal);
            btnRebook.SetTitle(Resources.HistoryDetailRebookButton, UIControlState.Normal);
            
            
            btnCancel.SetTitle(Resources.StatusActionCancelButton, UIControlState.Normal);
            btnStatus.SetTitle(Resources.HistoryViewStatusButton, UIControlState.Normal);

            btnCancel.TouchUpInside += CancelTouchUpInside;

            btnStatus.TouchUpInside += StatusTouchUpInside;
            
            btnCancel.Hidden = true;
            btnStatus.Hidden = true;
            
            
            btnHide.TouchUpInside += HideTouchUpInside;          
            btnRebook.TouchUpInside += RebookTouched;
            RefreshData();
        }

        void StatusTouchUpInside(object sender, EventArgs e)
        {
            AppContext.Current.LastOrder = _data.Id;
            AppContext.Current.Controller.Rebook(null);
            this.NavigationController.PopViewControllerAnimated(true);
        }

        void CancelTouchUpInside(object sender, EventArgs e)
        {
            LoadingOverlay.StartAnimatingLoading(this.View, LoadingOverlayPosition.Center, null, 130, 30);
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
                        LoadingOverlay.StopAnimatingLoading(this.View);
                        View.UserInteractionEnabled = true;
                    }
                    );
                }
            }
            );
        }

        void RebookTouched(object sender, EventArgs e)
        {
            AppContext.Current.Controller.Rebook(_data);
            this.NavigationController.PopViewControllerAnimated(true);
        }

        void HideTouchUpInside(object sender, EventArgs e)
        {
            //TODO : Is this still used ?
//            _data.Hide = true;
//            AppContext.Current.UpdateLoggedInUser(AppContext.Current.LoggedUser, false);
//            _parent.Selected();
            this.NavigationController.PopViewControllerAnimated(true);
        }

        public void LoadData(Order data)
        {
            _data = data;
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
            
                
                var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus( _data.Id);
                
                bool isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsStatusCompleted(status.IBSStatusId);
                
                InvokeOnMainThread(() => txtStatus.Text = status.IBSStatusDescription);
                InvokeOnMainThread(() => btnCancel.Hidden = isCompleted);
                InvokeOnMainThread(() => btnStatus.Hidden = isCompleted);
                
            }
            );
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

