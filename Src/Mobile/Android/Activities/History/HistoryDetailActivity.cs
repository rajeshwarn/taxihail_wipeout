using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.Framework.Extensions;
using TinyIoC;

using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class HistoryDetailActivity : BaseBindingActivity<HistoryDetailViewModel>
    {
        private TinyMessageSubscriptionToken _closeViewToken;       
        private Order _data;

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_HistoryDetail; }
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            _closeViewToken = TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Subscribe<CloseViewsToRoot>(m => Finish());            
            //SetContentView(Resource.Layout.View_HistoryDetail);
        } 

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (_closeViewToken != null)
            {
                TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Unsubscribe<CloseViewsToRoot>(_closeViewToken);
            }
        }

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_HistoryDetail);
            SetHistoryData(Guid.Parse(this.ViewModel.OrderId));
            UpdateUI();
        }

        private void UpdateUI()
        {
            FindViewById<TextView>(Resource.Id.ConfirmationTxt).Text = _data.IBSOrderId.HasValue ? _data.IBSOrderId.Value.ToString() : "Error";
            FindViewById<TextView>(Resource.Id.RequestedTxt).Text = FormatDateTime(_data.PickupDate, _data.PickupDate);
            FindViewById<TextView>(Resource.Id.OriginTxt).Text = _data.PickupAddress.FullAddress;
            FindViewById<TextView>(Resource.Id.AptRingTxt).Text = FormatAptRingCode(_data.PickupAddress.Apartment, _data.PickupAddress.RingCode);
            FindViewById<TextView>(Resource.Id.DestinationTxt).Text = _data.DropOffAddress.FullAddress.HasValue() ? _data.DropOffAddress.FullAddress : Resources.GetString(Resource.String.ConfirmDestinationNotSpecified);
            FindViewById<TextView>(Resource.Id.PickUpDateTxt).Text = FormatDateTime(_data.PickupDate, _data.PickupDate);
            FindViewById<TextView>(Resource.Id.StatusTxt).Text = Resources.GetString(Resource.String.LoadingMessage);
            RefreshStatus();
            var btnCancel = FindViewById<Button>(Resource.Id.CancelTripBtn);
            var btnStatus = FindViewById<Button>(Resource.Id.StatusBtn);
            var btnRebook = FindViewById<Button>(Resource.Id.RebookTripBtn);
            var btnSendReceipt = FindViewById<Button>(Resource.Id.SendReceiptBtn);
            var btnDelete = FindViewById<Button>(Resource.Id.HistoryOrderDeleteBtn);
            var btnRate = FindViewById<Button>(Resource.Id.RateBtn);
            var btnViewRate = FindViewById<Button>(Resource.Id.ViewRatingBtn);

            btnCancel.Visibility = ViewStates.Gone;
            btnStatus.Visibility = ViewStates.Gone;
            btnSendReceipt.Visibility = ViewStates.Gone;

            btnCancel.Click      += new EventHandler(btnCancel_Click);
            btnStatus.Click      += new EventHandler(btnStatus_Click);
            btnRebook.Click      += new EventHandler(btnRebook_Click);
            btnSendReceipt.Click += new EventHandler(btnSendReceipt_Click);
            btnDelete.Click      += new EventHandler(btnDelete_Click);
           // btnRate.Click        += new EventHandler(btnRate_Click);
            //btnViewRate.Click    += new EventHandler(btnViewRate_Click);


        }

        private void btnViewRate_Click(object sender, EventArgs e)
        {
            var parameters = new Dictionary<string, string>() { { "canRate", "false" } };
            var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
            dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(BookRatingViewModel), parameters, false, MvxRequestedBy.UserAction));
        }

        private void btnRate_Click(object sender, EventArgs e)
        {
            var parameters = new Dictionary<string, string>() { { "canRate", "true" } };
            var dispatch = TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
            dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(BookRatingViewModel), parameters, false, MvxRequestedBy.UserAction));
        }

        void btnRebook_Click(object sender, EventArgs e)
        {
            
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new RebookRequested(this, _data));
            TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new CloseViewsToRoot(this));
        }

        void btnSendReceipt_Click(object sender, EventArgs e)
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {
                if (Common.Extensions.GuidExtensions.HasValue(_data.Id))
                {
                    TinyIoCContainer.Current.Resolve<IBookingService>().SendReceipt(_data.Id);
                }

                RunOnUiThread(() => Finish());
            }, true);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            /*ThreadHelper.ExecuteInThread(this, () =>
            {
                if (Common.Extensions.GuidExtensions.HasValue(_data.Id))
                {
                        TinyIoCContainer.Current.Resolve<IBookingService>().RemoveFromHistory(_data.Id);
                }
                

                RunOnUiThread(() => Finish());
            }, true);*/
            this.ViewModel.DeleteOrder.Execute(_data.Id);
        }


        void btnStatus_Click(object sender, EventArgs e)
        {

            /*Intent i = new Intent(this, typeof(BookingStatusActivity));

            OrderStatusDetail orderInfo = new OrderStatusDetail { IBSOrderId = _data.IBSOrderId, IBSStatusDescription = "Loading...", IBSStatusId = "", OrderId = _data.Id, Status = OrderStatus.Unknown, VehicleLatitude = null, VehicleLongitude = null };

            var serialized = _data.Serialize();
            i.PutExtra("Order", serialized);

            serialized = orderInfo.Serialize();
            i.PutExtra("OrderStatusDetail", serialized);


            StartActivityForResult(i, 101);*/
            var orderInfo = new OrderStatusDetail { IBSOrderId = _data.IBSOrderId, IBSStatusDescription = "Loading...", IBSStatusId = "", OrderId = _data.Id, Status = OrderStatus.Unknown, VehicleLatitude = null, VehicleLongitude = null };
            var param = new Dictionary<string, object>() { { "order", _data }, { "orderInfo", orderInfo } };
            ViewModel.NavigateToOrderStatus.Execute(param);
            //Intent intent = new Intent();
            //intent.SetFlags(ActivityFlags.ForwardResult);
            //intent.PutExtra("Book", _data.Id.ToString());
            //SetResult(Result.Ok, intent);
            //Finish();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            var newBooking = new Confirmation();
            newBooking.Action(this, Resource.String.StatusConfirmCancelRide, () =>
            {
                ThreadHelper.ExecuteInThread(this, () =>
                  {


                      var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(_data.Id);
                      if (isSuccess)
                      {
                          RefreshStatus();
                      }
                      else
                      {
                          RunOnUiThread(() =>
                              {
                                  this.ShowAlert(Resources.GetString(Resource.String.StatusConfirmCancelRideErrorTitle), Resources.GetString(Resource.String.StatusConfirmCancelRideError));
                              });
                      }

                  }, false);
            });
        }

        private void RefreshStatus()
        {
            ThreadHelper.ExecuteInThread(this, () =>
            {


                var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(_data.Id);

                bool isCompleted = false;
                bool isDone      = false;
                if (status.IBSStatusId.HasValue())
                {
                    isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsStatusCompleted(status.IBSStatusId);
                }

                RunOnUiThread(() => FindViewById<TextView>(Resource.Id.StatusTxt).Text = status.IBSStatusDescription);
                var btnCancel = FindViewById<Button>(Resource.Id.CancelTripBtn);
                var btnStatus = FindViewById<Button>(Resource.Id.StatusBtn);
                var btnDelete = FindViewById<Button>(Resource.Id.HistoryOrderDeleteBtn);
                var btnSendReceipt = FindViewById<Button>(Resource.Id.SendReceiptBtn);
                RunOnUiThread(() => {
                    btnCancel.Visibility =      isCompleted ? ViewStates.Gone : ViewStates.Visible;
                    btnStatus.Visibility =      isCompleted ? ViewStates.Gone : ViewStates.Visible;
                    btnDelete.Visibility =      isCompleted ? ViewStates.Visible : ViewStates.Gone;
                    btnSendReceipt.Visibility = status.FareAvailable ? ViewStates.Visible : ViewStates.Gone;
                });

            }, false);
        }

        private void SetHistoryData(Guid id)
        {
            _data = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryOrder(id);
            //_data=AppContext.Current.LoggedUser.BookingHistory.Single(o => o.Id == id && !o.Hide);
        }

        private string FormatDateTime(DateTime? date, DateTime? time)
        {
            string result = date.HasValue ? date.Value.ToShortDateString() : Resources.GetString(Resource.String.DateToday);
            result += @" / ";
            result += time.HasValue ? time.Value.ToShortTimeString() : Resources.GetString(Resource.String.TimeNow);
            return result;
        }

        private string FormatAptRingCode(string apt, string rCode)
        {
            string result = apt.HasValue() ? apt : Resources.GetString(Resource.String.ConfirmNoApt);

            result += @" / ";
            result += rCode.HasValue() ? rCode : Resources.GetString(Resource.String.ConfirmNoRingCode);
            return result;
        }
    }
}
