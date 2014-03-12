using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.PresentationHints;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter : MvxAndroidViewPresenter
    {
        public PhonePresenter()
        {
        }

        public override void Show(Cirrious.MvvmCross.ViewModels.MvxViewModelRequest request)
        {
            bool removeFromHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("removeFromHistory");

            Intent intent = this.CreateIntentForRequest (request);
            this.Show (intent, removeFromHistory);
        }

        public override void ChangePresentation(Cirrious.MvvmCross.ViewModels.MvxPresentationHint hint)
        {
            if (hint is ChangePresentationHint)
            {
                TryChangeViewPresentation((ChangePresentationHint)hint);
            }
            else
            {
                base.ChangePresentation(hint);
            }
        }

        private void Show (Intent intent, bool removeFromHistory)
        {
            Activity activity = this.Activity;
            if (activity == null)
            {
                MvxTrace.Warning ("Cannot Resolve current top activity", new object[0]);
                return;
            }
            activity.StartActivity (intent);
            if (removeFromHistory)
            {
                activity.Finish();
            }
        }

        private void TryChangeViewPresentation(ChangePresentationHint hint)
        {
            var homeView = Activity as IChangePresentation;
            if (homeView != null)
            {
                homeView.ChangePresentation(hint);
            }
            else
            {
                MvxTrace.Warning("Can't change home view state");
            }

        }
    }
}

