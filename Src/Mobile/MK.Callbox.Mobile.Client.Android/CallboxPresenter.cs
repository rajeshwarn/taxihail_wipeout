using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.Droid.Views;

namespace apcurium.MK.Callbox.Mobile.Client
{
    public class CallboxPresenter: MvxAndroidViewPresenter
    {
        public override void Show(Cirrious.MvvmCross.ViewModels.MvxViewModelRequest request)
        {
            var removeFromHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("removeFromHistory");

            var clearHistory = request.ParameterValues != null
                                     && request.ParameterValues.ContainsKey("clearNavigationStack");

            var preventShowViewAnimation = request.ParameterValues != null && request.ParameterValues.ContainsKey("preventShowViewAnimation");

            var intent = CreateIntentForRequest(request);

            Show(intent, removeFromHistory, clearHistory, preventShowViewAnimation);
        }

        private void Show(Intent intent, bool removeFromHistory, bool clearHistory, bool preventShowViewAnimation)
        {
            var activity = Activity;
            if (activity == null)
            {
                MvxTrace.Warning("Cannot Resolve current top activity");
                return;
            }

            if (clearHistory)
            {
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
            }

            if (preventShowViewAnimation)
            {
                intent.AddFlags(ActivityFlags.NoAnimation);
            }

            activity.StartActivity(intent);
            if (removeFromHistory)
            {
                activity.Finish();
            }
        }
    }
}