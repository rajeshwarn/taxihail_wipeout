using System;
using System.Reflection;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class NonDomainRelatedCommandHandler : 
        ICommandHandler<LogApplicationStartUp>,
        ICommandHandler<SaveTemporaryOrderCreationInfo>,
        ICommandHandler<SaveTemporaryOrderPaymentInfo>
    {
        private readonly Func<BookingDbContext> _contextFactory;

        public NonDomainRelatedCommandHandler(Func<BookingDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void Handle(LogApplicationStartUp command)
        {
            using (var context = _contextFactory.Invoke())
            {
                // Check if a log from this user already exists. If not, create it.
                var log = context.Find<AppStartUpLogDetail>(command.UserId) ?? new AppStartUpLogDetail
                {
                    UserId = command.UserId
                };

                // Update log details
                log.DateOccured = command.DateOccured;
                log.ApplicationVersion = command.ApplicationVersion;
                log.Platform = command.Platform;
                log.PlatformDetails = command.PlatformDetails;
                log.ServerVersion = Assembly.GetAssembly(typeof(AppStartUpLogDetail)).GetName().Version.ToString();
                log.Latitude = command.Latitude;
                log.Longitude = command.Longitude;

                context.Save(log);
            }
        }

        public void Handle(SaveTemporaryOrderCreationInfo command)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new TemporaryOrderCreationInfoDetail
                {
                    OrderId = command.OrderId,
                    SerializedOrderCreationInfo = command.SerializedOrderCreationInfo
                });
            }
        }

        public void Handle(SaveTemporaryOrderPaymentInfo command)
        {
            using (var context = _contextFactory.Invoke())
            {
                context.Save(new TemporaryOrderPaymentInfoDetail
                {
                    OrderId = command.OrderId,
                    Cvv = command.Cvv
                });
            }
        }
    }
}