using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Services;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class SmsCommandHandler : ICommandHandler<SendAccountConfirmationSMS>,
		ICommandHandler<SendPasswordResetSMS>
    {
        private readonly INotificationService _notificationService;

        public SmsCommandHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Handle(SendAccountConfirmationSMS command)
        {
            _notificationService.SendAccountConfirmationSMS(command.CountryCode, command.PhoneNumber, command.Code, command.ClientLanguageCode);
        }

		public void Handle(SendPasswordResetSMS command)
		{
			_notificationService.SendPasswordResetSMS(command.CountryCode, command.PhoneNumber, command.Password, command.ClientLanguageCode);
		}
    }
}