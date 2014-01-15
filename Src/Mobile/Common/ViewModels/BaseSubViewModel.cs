using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public abstract class BaseSubViewModel<TResult>: BaseViewModel
	{
		public void Init(string messageId)
		{
			this.MessageId = messageId;
		}

		protected string MessageId
		{
			get;
			private set;
		}
		
		protected void ReturnResult(TResult result)
		{
			var message = new SubNavigationResultMessage<TResult>(this, MessageId, result);		
			Close(this);
            this.Services().MessengerHub.Publish(message);
		}
	}
}

