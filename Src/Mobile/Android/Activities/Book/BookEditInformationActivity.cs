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
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Android.Text;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class BookEditInformationActivity : BaseBindingActivity<BookEditInformationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }

        protected override void OnViewModelSet()
        {


            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            bool isThriev = appSettings.ApplicationName == "Thriev";

            SetContentView(isThriev ? Resource.Layout.View_BookEditInformation_Thriev : Resource.Layout.View_BookEditInformation);

            FindViewById<EditText>(Resource.Id.largeBagsEditText).Maybe(x => x.InputType = InputTypes.ClassNumber);

            ViewModel.Load();
        }
    }
}
