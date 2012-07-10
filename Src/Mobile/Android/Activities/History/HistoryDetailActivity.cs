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
using apcurium.Framework.Extensions;
using TinyIoC;

using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;

namespace apcurium.MK.Booking.Mobile.Client.Activities.History
{
    [Activity(Label = "History Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
  	public  class HistoryDetailActivity: Activity
    {
      private BookingInfoData _data;

      protected override void OnCreate(Bundle bundle)
      {
          base.OnCreate(bundle);
          SetContentView(Resource.Layout.HistoryDetail);
          SetHistoryData(Intent.Extras.GetInt(NavigationStrings.HistorySelectedId.ToString()));
          UpdateUI();
      }

      private void UpdateUI()
      {
          FindViewById<TextView>(Resource.Id.ConfirmationTxt).Text = _data.Id.ToString();
          FindViewById<TextView>(Resource.Id.RequestedTxt).Text = FormatDateTime(_data.RequestedDateTime, _data.RequestedDateTime);
          FindViewById<TextView>(Resource.Id.OriginTxt).Text = _data.PickupLocation.FullAddress;
          FindViewById<TextView>(Resource.Id.AptRingTxt).Text = FormatAptRingCode(_data.PickupLocation.Apartment, _data.PickupLocation.RingCode);
          FindViewById<TextView>(Resource.Id.DestinationTxt).Text = _data.DestinationLocation.FullAddress.HasValue() ? _data.DestinationLocation.FullAddress : Resources.GetString(Resource.String.ConfirmDestinationNotSpecified);
          FindViewById<TextView>(Resource.Id.PickUpDateTxt).Text = FormatDateTime(_data.PickupDate, _data.PickupDate);
          FindViewById<TextView>(Resource.Id.StatusTxt).Text = Resources.GetString(Resource.String.LoadingMessage);
          RefreshStatus();
          var btnCancel = FindViewById<Button>(Resource.Id.CancelTripBtn);
          var btnStatus = FindViewById<Button>(Resource.Id.StatusBtn);          
          var btnRebook = FindViewById<Button>(Resource.Id.RebookTripBtn);

          btnCancel.Visibility = ViewStates.Invisible;
          btnStatus.Visibility = ViewStates.Invisible;

          btnCancel.Click += new EventHandler(btnCancel_Click);
          btnStatus.Click += new EventHandler(btnStatus_Click);          
          btnRebook.Click += new EventHandler(btnRebook_Click);
      }

      void btnRebook_Click(object sender, EventArgs e)
      {
            Intent intent = new Intent();
            intent.SetFlags(ActivityFlags.ForwardResult);
            intent.PutExtra("Rebook", _data.Id);
            SetResult(Result.Ok, intent);
         	Finish();
      }
   

      void btnStatus_Click(object sender, EventArgs e)
      {
          Intent intent = new Intent();
          intent.SetFlags(ActivityFlags.ForwardResult);
          intent.PutExtra("Book", _data.Id);
          SetResult(Result.Ok, intent);
          Finish();
      }

      void btnCancel_Click(object sender, EventArgs e)
      {
          
          ThreadHelper.ExecuteInThread(this, () =>
            {
               

                    var isSuccess = TinyIoCContainer.Current.Resolve<IBookingService>().CancelOrder(AppContext.Current.LoggedUser, _data.Id);
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
      }

      private void RefreshStatus()
      {
          ThreadHelper.ExecuteInThread(this,() =>
          {


              var status = TinyIoCContainer.Current.Resolve<IBookingService>().GetOrderStatus(AppContext.Current.LoggedUser, _data.Id);

              bool isCompleted = TinyIoCContainer.Current.Resolve<IBookingService>().IsCompleted(status.Id);

              RunOnUiThread(() => FindViewById<TextView>(Resource.Id.StatusTxt).Text = status.Status);
              var btnCancel = FindViewById<Button>(Resource.Id.CancelTripBtn);
              var btnStatus = FindViewById<Button>(Resource.Id.StatusBtn);              
              RunOnUiThread(() => btnCancel.Visibility = isCompleted ? ViewStates.Invisible : ViewStates.Visible);
              RunOnUiThread(() => btnStatus.Visibility = isCompleted ? ViewStates.Invisible : ViewStates.Visible);

          },false);
      }

      private void SetHistoryData(int id)
      {
          //TODO:Fix this
          //_data=AppContext.Current.LoggedUser.BookingHistory.Single(o => o.Id == id && !o.Hide);
      }

      private string FormatDateTime(DateTime? date, DateTime? time)
      {
          string result = date.HasValue ? date.Value.ToShortDateString() :Resources.GetString(Resource.String.DateToday);
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
