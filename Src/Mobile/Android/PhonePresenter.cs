using System;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.AppServices;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.PresentationHints;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter : MvxAndroidViewPresenter
    {
	    public override void Show(Cirrious.MvvmCross.ViewModels.MvxViewModelRequest request)
        {
            var removeFromHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("removeFromHistory");

            var clearHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("clearNavigationStack");

            var intent = CreateIntentForRequest (request);

            Show(intent, removeFromHistory, clearHistory);
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

        private void Show (Intent intent, bool removeFromHistory, bool clearHistory)
        {
            var activity = Activity;
            if (activity == null)
            {
                MvxTrace.Warning ("Cannot Resolve current top activity", new object[0]);
                return;
            }

            if (clearHistory)
            {
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            activity.StartActivity(intent);
            if (removeFromHistory)
            {
                activity.Finish();
            }
        }

	    public override void Close(IMvxViewModel viewModel)
	    {
		    base.Close(viewModel);

			if (viewModel is TutorialViewModel)
			{
				var tutorialService = Mvx.Resolve<ITutorialService>();

				tutorialService.NotifyTutorialEnded();
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
                MvxTrace.Warning("Can't change home view state, keeping last presentation hint");
            }
        }
    }
}

