using System;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Collections.Generic;
using Cirrious.MvvmCross.Platform;
using apcurium.MK.Booking.Mobile.Messages;
using TinyMessenger;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel: MvxViewModel, IDisposable
    {
        protected readonly CompositeDisposable Subscriptions = new CompositeDisposable();
		protected new void RaisePropertyChanged([CallerMemberName]string whichProperty = null)
		{
			base.RaisePropertyChanged(whichProperty);
		}
		
        protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
        {
            observable
                .Subscribe(x => InvokeOnMainThread(() => onNext(x)))
                .DisposeWith(Subscriptions);
        }

		public void Dispose()
		{
			Dispose(true);
		}

		protected TinyIoCContainer Container
		{
			get { return TinyIoCContainer.Current; }
		}

		protected ILogger Logger { get { return Container.Resolve<ILogger>(); } }

		public TaxiHailSetting Settings { get { return Container.Resolve<IAppSettings>().Data; } }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				Subscriptions.Dispose();
			}
		}

        protected bool ShowSubViewModel<TViewModel, TResult>(object parameterValuesObject, Action<TResult> onResult)
            where TViewModel : MvxViewModel, ISubViewModel<TResult>
        {
            return ShowSubViewModel<TViewModel, TResult>(parameterValuesObject.ToSimplePropertyDictionary(), onResult);
        }

        protected bool ShowSubViewModel<TViewModel, TResult>(IDictionary<string, string> parameterValues,
            Action<TResult> onResult)
            where TViewModel : MvxViewModel, ISubViewModel<TResult>
        {
            parameterValues = parameterValues ?? new Dictionary<string, string>();

            if (parameterValues.ContainsKey("messageId"))
                throw new ArgumentException("parameterValues cannot contain an item with the key 'messageId'");

            string messageId = Guid.NewGuid().ToString();
            parameterValues["messageId"] = messageId;
            TinyMessageSubscriptionToken token = null;
            // ReSharper disable once RedundantAssignment
            token = this.Services().MessengerHub.Subscribe<SubNavigationResultMessage<TResult>>(msg =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (token != null)
                    this.Services().MessengerHub.Unsubscribe<SubNavigationResultMessage<TResult>>(token);
                // ReSharper restore AccessToModifiedClosure

                onResult(msg.Result);
            },
                msg => msg.MessageId == messageId);

            return ShowViewModel<TViewModel>(parameterValues);
        }

        protected void ShowViewModelAndRemoveFromHistory<TViewModel>(object parameter) where TViewModel : IMvxViewModel
        {
            var dictionary = parameter.ToSimplePropertyDictionary();
            dictionary = dictionary ?? new Dictionary<string,string>();
            dictionary.Add("removeFromHistory", "notUsed");
            base.ShowViewModel<TViewModel>(dictionary);
        }

		protected TViewModel AddChild<TViewModel>(Func<TViewModel> builder)
            where TViewModel: BaseViewModel
		{
			var viewModel = builder.Invoke();
			viewModel.CallBundleMethods("Init", new MvxBundle());
			viewModel.DisposeWith(Subscriptions);
			return viewModel;
		}

		protected virtual TViewModel AddChild<TViewModel>()
            where TViewModel: BaseViewModel
		{
			return AddChild<TViewModel>(() => Mvx.IocConstruct<TViewModel>());
		}

		protected override void InitFromBundle(IMvxBundle parameters)
		{
			base.InitFromBundle(parameters);
			if(parameters.Data.ContainsKey("messageId"))
			{
				this.MessageId = parameters.Data["messageId"];
			}
		}

		public string MessageId
		{
			get;
			private set;
		}

	
    }
}

