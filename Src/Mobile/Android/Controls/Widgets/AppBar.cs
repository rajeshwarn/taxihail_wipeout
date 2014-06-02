using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class AppBar: MvxFrameControl, IChangePresentation
    {
        public AppBar(Context context, IAttributeSet attrs) :
            base(Resource.Layout.SubView_AppBar, context, attrs)
        {
            this.DelayBind(() => {

                var estimate = Content.FindViewById(Resource.Id.btnEstimateLayout);
                var bookNow = Content.FindViewById<Button>(Resource.Id.btnBookNow);
                var bookLater = Content.FindViewById(Resource.Id.btnBookLaterLayout);

                var cancelReview = Content.FindViewById<Button>(Resource.Id.btnCancelReview);
                var confirm = Content.FindViewById<Button>(Resource.Id.btnConfirm);
                var edit = Content.FindViewById<Button>(Resource.Id.btnEdit);

                var cancelEdit = Content.FindViewById<Button>(Resource.Id.btnCancelEdit);
                var save = Content.FindViewById<Button>(Resource.Id.btnSave);

				var set = this.CreateBindingSet<AppBar, BottomBarViewModel>();

                set.Bind(estimate)
                    .For("Click")
                    .To(vm => vm.ChangeAddressSelectionMode);

                set.Bind(bookNow)
                    .For("Click")
                    .To(vm => vm.SetPickupDateAndReviewOrder);

                set.Bind(bookLater)
                    .For("Click")
                    .To(vm => vm.BookLater);

                set.Bind(cancelReview)
                    .For("Click")
                    .To(vm => vm.CancelReview);
                set.Bind(confirm)
                    .For("Click")
                    .To(vm => vm.ConfirmOrder);
                set.Bind(edit)
                    .For("Click")
                    .To(vm => vm.Edit);

                set.Bind(cancelEdit)
                    .For("Click")
                    .To(vm => vm.CancelEdit);
                set.Bind(save)
                    .For("Click")
                    .To(vm => vm.Save);

                set.Apply();
            });
        }

        private void ChangeState(HomeViewModelPresentationHint hint)
        {
            var bookButtons = Content.FindViewById(Resource.Id.order_buttons);
            var reviewButtons = Content.FindViewById(Resource.Id.review_buttons);
            var editButtons = Content.FindViewById(Resource.Id.edit_buttons);

            if (hint.State == HomeViewModelState.Review)
            {
                bookButtons.Visibility = ViewStates.Gone;
                reviewButtons.Visibility = ViewStates.Visible;
                editButtons.Visibility = ViewStates.Gone;
            }
            else if (hint.State == HomeViewModelState.Edit)
            {
                bookButtons.Visibility = ViewStates.Gone;
                reviewButtons.Visibility = ViewStates.Gone;
                editButtons.Visibility = ViewStates.Visible;
            }
            else if (hint.State == HomeViewModelState.Initial)
            {
                bookButtons.Visibility = ViewStates.Visible;
                reviewButtons.Visibility = ViewStates.Gone;
                editButtons.Visibility = ViewStates.Gone;
            }
            else if (hint.State == HomeViewModelState.PickDate)
            {
                // Do nothing
                // this state does not affect this control
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

