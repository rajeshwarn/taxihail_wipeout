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

		    var preventShowViewAnimation = request.ParameterValues != null && request.ParameterValues.ContainsKey("preventShowViewAnimation");

            var intent = CreateIntentForRequest (request);

			Show(intent, removeFromHistory, clearHistory, preventShowViewAnimation);
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
	        var presentationHint = hint as ChangePresentationHint;
	        if (presentationHint != null)
            {
                TryChangeViewPresentation(presentationHint);
            }
            else
            {
                base.ChangePresentation(hint);
            }
        }

	    private void Show (Intent intent, bool removeFromHistory, bool clearHistory, bool preventShowViewAnimation)
        {
            var activity = Activity;
            if (activity == null)
            {
                MvxTrace.Warning ("Cannot Resolve current top activity");
                return;
            }

            if (clearHistory)
            {
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

		    if(preventShowViewAnimation)
		    {
			    intent.AddFlags(ActivityFlags.NoAnimation);
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

