using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

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

	        var set = this.CreateBindingSet<AppBar, BottomBarViewModel>();

	        set.Bind(bookButtons)
		        .For(v => v.Visibility)
		        .To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
		        .WithConversion("HomeViewStateToVisibility", new[] {HomeViewModelState.Initial});

			set.Bind(editButtons)
				.For(v => v.Visibility)
				.To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Edit });

			set.Bind(reviewButtons)
				.For(v => v.Visibility)
				.To(vm => ((HomeViewModel)vm.Parent).CurrentViewState)
				.WithConversion("HomeViewStateToVisibility", new[] { HomeViewModelState.Review });

			set.Apply();
        }
    }
}

