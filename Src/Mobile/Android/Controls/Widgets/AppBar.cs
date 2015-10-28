using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.Content;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Runtime;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBar")]
    public class AppBar: MvxFrameControl
    {
		public AppBar(Context context, IAttributeSet attrs) :
            base(Resource.Layout.SubView_AppBar, context, attrs)
        {
			this.DelayBind(SetupBinding);
        }

        private void SetupBinding()
        {
            var bookButtons = Content.FindViewById(Resource.Id.order_buttons);
            var reviewButtons = Content.FindViewById(Resource.Id.review_buttons);
            var editButtons = Content.FindViewById(Resource.Id.edit_buttons);
            var airportButtons = Content.FindViewById(Resource.Id.airport_buttons);
            var bookButton = Content.FindViewById(Resource.Id.bookButton);
            var bookButtonNow = Content.FindViewById(Resource.Id.btnBookNow);

	        var set = this.CreateBindingSet<AppBar, BottomBarViewModel>();

			set.Bind(airportButtons)
				.For(v => v.Visibility)
                .To(vm => vm.ParentViewModel.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.AirportDetails });

	        set.Bind(bookButtons)
		        .For(v => v.Visibility)
                .To(vm => vm.ParentViewModel.CurrentViewState)
		        .WithConversion("HomeViewStateToVisibility", new[] {HomeViewModelState.Initial});

            set.Bind(bookButton)
                .For(v => v.Enabled)
                .To(vm => vm.ParentViewModel.Map.BookCannotExecute)
                .WithConversion("BoolInverter");

            set.Bind(bookButtonNow)
                .For(v => v.Enabled)
                .To(vm => vm.ParentViewModel.Map.BookCannotExecute)
                .WithConversion("BoolInverter");

			set.Bind(editButtons)
				.For(v => v.Visibility)
                .To(vm => vm.ParentViewModel.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Edit });

			set.Bind(reviewButtons)
				.For(v => v.Visibility)
                .To(vm => vm.ParentViewModel.CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Review });

			set.Apply();
        }
    }
}

