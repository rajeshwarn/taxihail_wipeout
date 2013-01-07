using System;
using System.Linq;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Widget;
using ServiceStack.Text;
using apcurium.Framework.Extensions;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Client.Activities.Setting;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait )]
    public class BookDetailActivity : BaseBindingActivity<BookConfirmationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }


        protected override void OnViewModelSet()
        {            
            SetContentView(Resource.Layout.View_BookingDetail);
			ViewModel.Load();
        }
    }
}
