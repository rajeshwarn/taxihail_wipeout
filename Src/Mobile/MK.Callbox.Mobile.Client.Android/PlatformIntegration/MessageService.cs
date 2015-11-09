using System;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Callbox.Mobile.Client.Activities;
using apcurium.MK.Callbox.Mobile.Client.Messages;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Common.Extensions;
using Android.Util;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.Droid.Views;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.Droid;

namespace apcurium.MK.Callbox.Mobile.Client.PlatformIntegration
{
    public class MessageService : IMessageService
    {
        public const string ACTION_SERVICE_MESSAGE = "Mk_Taxi.ACTION_SERVICE_MESSAGE";
        public const string ACTION_EXTRA_MESSAGE = "Mk_Taxi.ACTION_EXTRA_MESSAGE";

		readonly Context _context;
		readonly ITinyMessengerHub _messengerHub;
		readonly IMvxViewDispatcher _viewDispatcher;
		readonly IMvxAndroidCurrentTopActivity _topActivity;

		public MessageService(IMvxAndroidGlobals globals,
			ITinyMessengerHub messengerHub,
			IMvxViewDispatcher viewDispatcher,
			IMvxAndroidCurrentTopActivity topActivity)
        {
			_topActivity = topActivity;
			_viewDispatcher = viewDispatcher;
			_messengerHub = messengerHub;
			_context = globals.ApplicationContext;
        }


		/// <summary>
		/// put the content of on activity on a modal dialog ( type = viewmodel Type )
		/// </summary>
		public void ShowDialogActivity(Type type)
		{
			var presenter = new MvxAndroidViewPresenter();
			presenter.Show(new MvxViewModelRequest(type, null, null, MvxRequestedBy.UserAction));
		}

		public async Task ShowMessage(string title, string message)
		{
			var ownerId = Guid.NewGuid().ToString();

			_viewDispatcher.RequestMainThreadAction(() =>{
				var intent = new Intent(_context, typeof(AlertDialogActivity));
				intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				intent.PutExtra("Title", title);
				intent.PutExtra("Message", message);
				intent.PutExtra("OwnerId", ownerId );
				_context.StartActivity(intent); 
			});

			var tcs = new TaskCompletionSource<object>();

			var token = _messengerHub.Subscribe<ActivityCompleted>(
				a => tcs.TrySetResult(null), 
				a => a.OwnerId == ownerId);

			await tcs.Task;

			_messengerHub.Unsubscribe<ActivityCompleted>(token);
			token.DisposeIfDisposable();
		}
        
        public Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction)
        {
	        return ShowMessage(title, message, positiveButtonTitle, positiveAction, negativeButtonTitle, negativeAction, null, null);
        }

        public async Task ShowMessage(string title, string message, string positiveButtonTitle, Action positiveAction, string negativeButtonTitle, Action negativeAction, string neutralButtonTitle, Action neutralAction)
        {
            var ownerId = Guid.NewGuid().ToString();
	        _viewDispatcher.RequestMainThreadAction(() =>
	        {
		        var intent = new Intent(_context, typeof (AlertDialogActivity));
		        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
		        intent.PutExtra("Title", title);
		        intent.PutExtra("Message", message);

		        intent.PutExtra("PositiveButtonTitle", positiveButtonTitle);
		        intent.PutExtra("NegativeButtonTitle", negativeButtonTitle);
		        if (neutralButtonTitle != null)
		        {
			        intent.PutExtra("NeutralButtonTitle", neutralButtonTitle);
		        }

		        intent.PutExtra("OwnerId", ownerId);

				_context.StartActivity(intent);
	        });

			var tcs = new TaskCompletionSource<object>();
			var token = _messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                if (a.Content == positiveButtonTitle)
                {
                    positiveAction();
                }
                else if (a.Content == negativeButtonTitle)
                {
                    negativeAction();
                }
                else if (a.Content == neutralButtonTitle && neutralAction != null)
                {
                    neutralAction();
                }

				tcs.SetResult(null);
            }, a => a.OwnerId == ownerId);

	        await tcs.Task;

			_messengerHub.Unsubscribe<ActivityCompleted>(token);
			token.Dispose();
        }

        public void ShowMessage (string title, string message, List<KeyValuePair<string,Action>> additionalButton)
		{
			throw new NotImplementedException();
		}

        public async Task ShowMessage(string title, string message, Action additionalAction)
        {            
			var ownerId = Guid.NewGuid().ToString();

	        _viewDispatcher.RequestMainThreadAction(() =>
	        {
		        var intent = new Intent(_context, typeof (AlertDialogActivity));
		        intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
		        intent.PutExtra("Title", title);
		        intent.PutExtra("Message", message);

		        intent.PutExtra("NeutralButtonTitle", "OK");
		        intent.PutExtra("OwnerId", ownerId);

				_context.StartActivity(intent);
	        });
			
			var tcs = new TaskCompletionSource<object>();
			var token = _messengerHub.Subscribe<ActivityCompleted>(a =>
			{				
				additionalAction();	
				tcs.SetResult(null);
				
			}, a => a.OwnerId == ownerId);

	        await tcs.Task;

			_messengerHub.Unsubscribe<ActivityCompleted>(token);
			token.Dispose();
        }

	    private readonly Stack<ProgressDialog> _progressDialogs = new Stack<ProgressDialog>();

        public void ShowProgress (bool show)
		{
			_viewDispatcher.RequestMainThreadAction(() =>{
				var activity = _topActivity.Activity;

				if(show)
				{ 		
					var progress = new ProgressDialog(activity);
					_progressDialogs.Push(progress);
					progress.SetTitle(string.Empty);
					progress.SetMessage(activity.GetString(Resource.String.LoadingMessage));
                    progress.SetCanceledOnTouchOutside(false);
					progress.Show();

				}
				else
				{
					if (_progressDialogs.None())
					{
						return;
					}
					var progressPrevious = _progressDialogs.Pop();
					if(progressPrevious != null && progressPrevious.IsShowing)
					{
						try
						{
							progressPrevious.Dismiss();
						}
						catch
						{
							// on peut avoir une exception ici si activity est plus présente, pas grave
						} 
					}
				}
			});
        }

        public void ShowProgressNonModal(bool show)
	    {
			var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity>();
			var rootView = topActivity.Activity.Window.DecorView.RootView as ViewGroup;

		    if (rootView == null)
		    {
			    return;
		    }
		    var progress = rootView.FindViewWithTag("Progress") as ProgressBar;

		    if ((progress == null) && (show))
		    {
			    var contentView = rootView.GetChildAt(0);
			    rootView.RemoveView(contentView);

			    var relLayout = new RelativeLayout(topActivity.Activity.ApplicationContext)
			    {
				    LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent)
			    };
			    relLayout.AddView(contentView);

			    var b = new ProgressBar(topActivity.Activity.ApplicationContext, null, Android.Resource.Attribute.ProgressBarStyleHorizontal)
			    {
				    Progress = 25,
				    LayoutParameters = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent),
				    Indeterminate = true,
				    Tag = "Progress"
			    };

				((RelativeLayout.LayoutParams)b.LayoutParameters).TopMargin = GetPixels(78);
			    relLayout.AddView(b);
			    rootView.AddView(relLayout);
		    }
		    else if (progress != null)
		    {
			    progress.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
			    progress.Indeterminate = true;
			    progress.Enabled = true;
		    }
	    }

		public static int GetPixels(float dipValue)
		{
			return (int)TypedValue.ApplyDimension(ComplexUnitType.Dip, dipValue, Application.Context.Resources.DisplayMetrics);
		}

	    public IDisposable ShowProgress()
		{
			ShowProgress (true);
			return Disposable.Create (() => ShowProgress(false));
		}

	    public IDisposable ShowProgressNonModal()
	    {
		    ShowProgressNonModal(true);
		    return Disposable.Create(() => ShowProgressNonModal(false));
	    }

	    public void ShowDialog(Type type)
	    {
			var presenter = new MvxAndroidViewPresenter();
			presenter.Show(new MvxViewModelRequest(type, null, null, MvxRequestedBy.UserAction));
	    }

	    public void ShowToast(string message, ToastDuration duration )
        {
			_viewDispatcher.RequestMainThreadAction(() =>
			{
	            var toast = Toast.MakeText(_context, message , duration == ToastDuration.Short ?  ToastLength.Short : ToastLength.Long );
	            toast.Show();
			});
        }

		public void ShowDialog<T> (string title, IEnumerable<T> items, Func<T, string> displayNameSelector, Action<T> onResult)
		{
			var list = items.ToArray();
			if (displayNameSelector == null) 
			{
				displayNameSelector = x => x.ToString ();
			}
			if (onResult == null) 
			{
				onResult = result => {};
			}

			var displayList = list.Select(displayNameSelector).ToArray();
			var ownerId = Guid.NewGuid().ToString();
			_viewDispatcher.RequestMainThreadAction(() =>
			{
				
				var i = new Intent(_context, typeof (SelectItemDialogActivity));
				i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
				i.PutExtra("Title", title);
				i.PutExtra("Items", displayList);
				i.PutExtra("OwnerId", ownerId);
				_context.StartActivity(i); 
			});

			var tcs = new TaskCompletionSource<object>();
			var token = _messengerHub.Subscribe<SubNavigationResultMessage<int>>(msg =>
			{
				onResult(list[msg.Result]);
				tcs.SetResult(null);
			}, msg => msg.MessageId == ownerId);
			

			if (token != null)
			{
				_messengerHub.Unsubscribe<SubNavigationResultMessage<int>>(token);
			}

		}

	    public Task<string> ShowPromptDialog(string title, string message, Action cancelAction = null, bool isNumericOnly = false, string inputText = "")
	    {
			var tcs = new TaskCompletionSource<string>();

			var dispatcher = Mvx.Resolve<IMvxViewDispatcher>();

			dispatcher.RequestMainThreadAction(() =>
			{
				try
				{
					ShowEditTextDialog(title, message, Mvx.Resolve<ILocalization>()["Ok"], content => tcs.TrySetResult(content));
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
				
			});

			return tcs.Task;
	    }


	    public void ShowEditTextDialog(string title, string message, string positiveButtonTitle, Action<string> positiveAction)
        {
            var ownerId = Guid.NewGuid().ToString();
            var i = new Intent(_context, typeof(EditTextDialogActivity));
            i.AddFlags(ActivityFlags.NewTask | ActivityFlags.ReorderToFront);
            i.PutExtra("Title", title);
            i.PutExtra("Message", message);
            i.PutExtra("PositiveButtonTitle", positiveButtonTitle);
            i.PutExtra("OwnerId", ownerId);

            TinyMessageSubscriptionToken token = null;
			token = _messengerHub.Subscribe<ActivityCompleted>(a =>
            {
                positiveAction(a.Content);
				_messengerHub.Unsubscribe<ActivityCompleted>(token);
                token.Dispose();
            }, a => a.OwnerId == ownerId);
            _context.StartActivity(i);
        }
    }
}