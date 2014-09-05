using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Services;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class SmsCommandHandler : ICommandHandler<SendAccountConfirmationSMS>
    {
        private readonly INotificationService _notificationService;

        public SmsCommandHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public void Handle(SendAccountConfirmationSMS command)
        {
            _notificationService.SendAccountConfirmationSMS(command.PhoneNumber, command.Code, command.ClientLanguageCode);
        }
    }
}