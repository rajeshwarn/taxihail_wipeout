using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public abstract class BaseSubViewModel<TResult>: BaseViewModel
	{
		public BaseSubViewModel(string messageId)
		{
			MessageId = messageId;
		}

		protected string MessageId {
			get;
			private set;
		}

		protected void Cancel()
		{
			ReturnResult(default(TResult));
		}
		
		protected void ReturnResult(TResult result)
		{
			var message = new SubNavigationResultMessage<TResult>(this, MessageId, result);		
			Close();
            this.Services().MessengerHub.Publish(message);
		}
	}
}

