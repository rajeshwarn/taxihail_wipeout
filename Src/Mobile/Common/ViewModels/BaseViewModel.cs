using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using TinyIoC;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public abstract class BaseViewModel : MvxViewModel 
    {
		private readonly CompositeDisposable _subscriptions = new CompositeDisposable();

        public static Action NoAction = () => { };

		protected TinyIoCContainer Container
        {
            get { return TinyIoCContainer.Current; }
        }
			
        protected ILogger Logger { get { return Container.Resolve<ILogger>(); } }

		public TaxiHailSetting Settings { get { return Container.Resolve<IAppSettings>().Data; } }

        public virtual void OnViewLoaded()
        {
        }

        public virtual void OnViewStarted(bool firstTime)
        {
        }

        public virtual void OnViewStopped()
        {
        }

        public virtual void OnViewUnloaded()
        {
			_subscriptions.Dispose();
        }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				_subscriptions.Dispose();
			}
		}

		protected bool ShowSubViewModel<TViewModel, TResult>(object parameterValuesObject, Action<TResult> onResult)
				where TViewModel : BaseSubViewModel<TResult>
		{
			return ShowSubViewModel<TViewModel, TResult>(parameterValuesObject.ToSimplePropertyDictionary(), onResult);
		}

		protected bool ShowSubViewModel<TViewModel, TResult>(IDictionary<string, string> parameterValues,
                                                               Action<TResult> onResult)
            where TViewModel : BaseSubViewModel<TResult>
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
                    this.Services().MessengerHub.Unsubscribe
                        <SubNavigationResultMessage<TResult>>(token);
                // ReSharper restore AccessToModifiedClosure

                onResult(msg.Result);
            },
            msg => msg.MessageId == messageId);

			return ShowViewModel<TViewModel>(parameterValues);
        }
		       
		
		public ICommand CloseCommand
		{
			get
			{
				return this.GetCommand(() => Close(this));
			}
		}


		protected new void RaisePropertyChanged([CallerMemberName]string whichProperty = null)
		{
			base.RaisePropertyChanged(whichProperty);
		}

		protected void ShowViewModelAndRemoveFromHistory<TViewModel>(object parameter) where TViewModel : IMvxViewModel
		{
            var dictionary = parameter.ToSimplePropertyDictionary();
            dictionary = dictionary ?? new Dictionary<string,string>();
            dictionary.Add("removeFromHistory", "notUsed");
            base.ShowViewModel<TViewModel>(dictionary);
		}

		protected TViewModel AddChild<TViewModel>(Func<TViewModel> builder)
			where TViewModel: ChildViewModel
		{
			var viewModel = builder.Invoke();
			viewModel.CallBundleMethods("Init", new MvxBundle());
			viewModel.DisposeWith(_subscriptions);
			return viewModel;
		}

		protected virtual TViewModel AddChild<TViewModel>()
			where TViewModel: ChildViewModel
		{
			return AddChild<TViewModel>(() => Mvx.IocConstruct<TViewModel>());
		}

		protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
		{
			observable
				.Subscribe(x => InvokeOnMainThread(() => onNext(x)))
				.DisposeWith(_subscriptions);
		}

    }

}

