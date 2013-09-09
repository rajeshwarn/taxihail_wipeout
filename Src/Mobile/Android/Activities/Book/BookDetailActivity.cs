using Android.App;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Threading.Tasks;
using System.Threading;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Infrastructure;

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
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            bool isThriev = true;//appSettings.ApplicationName == "Thriev";
            SetContentView(isThriev ? Resource.Layout.View_BookingDetail_Thriev : Resource.Layout.View_BookingDetail);

			FindViewById<EditText>(Resource.Id.noteEditText).FocusChange += HandleFocusChange;

            //if ( !ViewModel.ShowRingCodeField )
            //{
            //    FindViewById<TableLayout>( Resource.Id.tableAptRingCode ).Visibility = Android.Views.ViewStates.Gone;
            //}
			ViewModel.Load();
        }

        void HandleFocusChange (object sender, View.FocusChangeEventArgs e)
        {
			if (e.HasFocus) {
		
				Task.Factory.StartNew ( () =>
				                       {
					Thread.Sleep( 300 );
				 	RunOnUiThread( () => FindViewById<ScrollView> (Resource.Id.mainScroll).FullScroll (FocusSearchDirection.Down) );
				});
			}
		
        }
    }
}
