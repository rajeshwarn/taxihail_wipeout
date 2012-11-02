using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.Binding.Android.Views;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.Adapters;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.Models;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "BookingRateActivity", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookingRateActivity : BaseBindingActivity<BookRatingViewModel>
    {

        private ListView _listView;
        private EditText _ratingNote;
        private Button _rateOrderButton;
        private List<RatingModel> _ratingTypes;
        private RatingListAdapter _adapter;

        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_RatePage; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);


            //UpdateUI();
        }


        /*private void SetAdapter()
        {
            ThreadHelper.ExecuteInThread(this, () =>
                                                   {
                                                       _ratingTypes = GetRatingsType();
                                                        _adapter = new RatingListAdapter(this, _ratingTypes);

                RunOnUiThread(() =>
                {
                    _listView.Adapter = _adapter;
                    _listView.Divider = null;
                    _listView.DividerHeight = 0;
                    _listView.SetPadding(10, 0, 10, 0);
                });

            }, true);
        }

        private void UpdateUI()
        {
            this.SetContentView(Resource.Layout.View_BookingRating);
            _listView = FindViewById<ListView>(Resource.Id.RatingListView);
            _ratingNote = FindViewById<EditText>(Resource.Id.RatingNote);
            _ratingNote.Text = "";
            _rateOrderButton = FindViewById<Button>(Resource.Id.RateOrderButton);
            _rateOrderButton.Click += RateOrder;
            _listView.CacheColorHint = Color.Transparent;
        }

        private void RateOrder(object sender, EventArgs eventArgs)
        {
            var orderRating = new OrderRatings()
                                  {
                                      Note = this._ratingNote.Text,
                                      OrderId = Guid.Parse("C03C043B-1532-4EC9-AFB7-7171D02CA1A7"),
                                      RatingScores =
                                          this._adapter.ListRating.Select(
                                              c => new RatingScore() {RatingTypeId = c.RatingTypeId, Score = c.Score}).
                                          ToList()
                                  };
            TinyIoCContainer.Current.Resolve<IBookingService>().SendRatingReview(orderRating);
        }

        private List<RatingModel> GetRatingsType()
        {
            return TinyIoCContainer.Current.Resolve<IBookingService>().GetRatingType().Select(c=> new RatingModel(){RatingTypeId = c.Id, RatingTypeName = c.Name}).ToList();
            
        }*/

        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_BookingRating);
        }

        /*protected override void OnResume()
        {
            base.OnResume();
            SetAdapter();
        }*/


    }
}