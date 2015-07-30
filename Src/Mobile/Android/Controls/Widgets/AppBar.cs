using Android.Content;
using Android.Util;
using Android.Views;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Runtime;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.AppBar")]
    public class AppBar: MvxFrameControl, IChangePresentation
    {
        public AppBar(Context context, IAttributeSet attrs) :
            base(Resource.Layout.SubView_AppBar, context, attrs)
        {
        }

        private void ChangeState(HomeViewModelPresentationHint hint)
        {
            var bookButtons = Content.FindViewById(Resource.Id.order_buttons);
            var reviewButtons = Content.FindViewById(Resource.Id.review_buttons);
            var editButtons = Content.FindViewById(Resource.Id.edit_buttons);
            var airportButtons = Content.FindViewById(Resource.Id.airport_buttons);

            if (hint.State == HomeViewModelState.Review)
            {
                bookButtons.Visibility = ViewStates.Gone;
                reviewButtons.Visibility = ViewStates.Visible;
                editButtons.Visibility = ViewStates.Gone;
                airportButtons.Visibility = ViewStates.Gone;
            }
            else if (hint.State == HomeViewModelState.Edit)
            {
                bookButtons.Visibility = ViewStates.Gone;
                reviewButtons.Visibility = ViewStates.Gone;
                editButtons.Visibility = ViewStates.Visible;
                airportButtons.Visibility = ViewStates.Gone;
            }
            else if (hint.State == HomeViewModelState.Initial)
            {
                bookButtons.Visibility = ViewStates.Visible;
                reviewButtons.Visibility = ViewStates.Gone;
                editButtons.Visibility = ViewStates.Gone;
                airportButtons.Visibility = ViewStates.Gone;
            }
            else if ((hint.State == HomeViewModelState.PickDate)||(hint.State == HomeViewModelState.AirportPickDate))
            {
                // Do nothing
                // this state does not affect this control
            }
            else if( hint.State == HomeViewModelState.AirportDetails )
            {
                airportButtons.Visibility = ViewStates.Visible;
                bookButtons.Visibility = ViewStates.Gone;
                reviewButtons.Visibility = ViewStates.Gone;
                editButtons.Visibility = ViewStates.Gone;
            }
        }

        public void ChangePresentation(ChangePresentationHint hint)
        {
            if (hint is HomeViewModelPresentationHint)
            {
                ChangeState((HomeViewModelPresentationHint)hint);
            }
        }
    }
}

