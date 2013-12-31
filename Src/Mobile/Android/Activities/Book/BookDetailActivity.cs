using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content.PM;
using Android.Views;
using Android.Widget;

using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.ViewModels;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client.Activities.Book
{
    [Activity(Label = "Book Details", Theme = "@android:style/Theme.NoTitleBar",
        ScreenOrientation = ScreenOrientation.Portrait)]
    public class BookDetailActivity : BaseBindingActivity<BookConfirmationViewModel>
    {
        protected override int ViewTitleResourceId
        {
            get { return Resource.String.View_BookingDetail; }
        }


        protected override void OnViewModelSet()
        {
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            bool isThriev = appSettings.ApplicationName == "Thriev";
            SetContentView(isThriev ? Resource.Layout.View_BookingDetail_Thriev : Resource.Layout.View_BookingDetail);

            FindViewById<EditText>(Resource.Id.noteEditText).FocusChange += HandleFocusChange;

            //if ( !ViewModel.ShowRingCodeField )
            //{
            //    FindViewById<TableLayout>( Resource.Id.tableAptRingCode ).Visibility = Android.Views.ViewStates.Gone;
            //}
            ViewModel.Load();
        }

        private void HandleFocusChange(object sender, View.FocusChangeEventArgs e)
        {
            if (e.HasFocus)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(300);
                    RunOnUiThread(
                        () => FindViewById<ScrollView>(Resource.Id.mainScroll).FullScroll(FocusSearchDirection.Down));
                });
            }
        }
    }
}