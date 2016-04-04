#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Email;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Booking.Services;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

#endregion

namespace apcurium.MK.Booking.CommandHandlers
{
    public class EmailCommandHandler : ICommandHandler<SendPasswordResetEmail>,
        ICommandHandler<SendAccountConfirmationEmail>,
        ICommandHandler<SendBookingConfirmationEmail>,
        ICommandHandler<SendReceipt>
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger _logger;

        public EmailCommandHandler(INotificationService notificationService, ILogger logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        public void Handle(SendAccountConfirmationEmail command)
        {
            _notificationService.SendAccountConfirmationEmail(command.ConfirmationUrl, command.EmailAddress, command.ClientLanguageCode);
        }

        public void Handle(SendBookingConfirmationEmail command)
        {
            _notificationService.SendBookingConfirmationEmail(command.IBSOrderId, command.Note, command.PickupAddress, command.DropOffAddress, 
                command.PickupDate, command.Settings, command.EmailAddress, command.ClientLanguageCode);
        }

        public void Handle(SendPasswordResetEmail command)
        {
            _notificationService.SendPasswordResetEmail(command.Password, command.EmailAddress, command.ClientLanguageCode);
        }

        public void Handle(SendReceipt command)
        {
            _notificationService.SendTripReceiptEmail(command.OrderId, command.IBSOrderId, command.VehicleNumber, command.DriverInfos, command.Fare, command.Toll, command.Tip,
                    command.Tax, command.Extra, command.Surcharge, command.BookingFees, command.TotalFare, command.PaymentInfo, command.PickupAddress, command.DropOffAddress,
                    command.PickupDate, command.UtcDropOffDate, command.EmailAddress, command.ClientLanguageCode, command.AmountSavedByPromotion, command.PromoCode,
                    command.CmtRideLinqFields).FireAndForget();
        }
    }
}