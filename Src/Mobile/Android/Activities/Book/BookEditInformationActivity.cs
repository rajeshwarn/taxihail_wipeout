using Android.App;
using Android.Content.PM;
using Android.Text;
using Android.Widget;

using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookEditInformationActivity : BaseBindingActivity<BookEditInformationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }

        protected override void OnViewModelSet()
        {
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            bool isThriev = appSettings.Data.ApplicationName == "Thriev";

            SetContentView(isThriev
                ? Resource.Layout.View_BookEditInformation_Thriev
                : Resource.Layout.View_BookEditInformation);

            FindViewById<EditText>(Resource.Id.largeBagsEditText).Maybe(x => x.InputType = InputTypes.ClassNumber);

            ViewModel.OnViewLoaded();
        }
    }
}