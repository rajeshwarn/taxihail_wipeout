using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.CrossCore.Core;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public interface ISubViewModel<TResult>
	{
		string MessageId { get; }
	}

	public static class ISubViewModelExtensions
	{
		public static void ReturnResult<TViewModel, TResult>(this TViewModel viewModel, TResult result) where TViewModel: MvxViewModel, ISubViewModel<TResult>
		{
			var message = new SubNavigationResultMessage<TResult>(viewModel, viewModel.MessageId, result);
			Dispatcher.ChangePresentation(new MvxClosePresentationHint(viewModel));

			viewModel.Services().MessengerHub.Publish(message);
		}

		private static IMvxViewDispatcher Dispatcher
		{
			get
			{
				return ((IMvxViewDispatcher)MvxSingleton<IMvxMainThreadDispatcher>.Instance);
			}
		}
	}
}

