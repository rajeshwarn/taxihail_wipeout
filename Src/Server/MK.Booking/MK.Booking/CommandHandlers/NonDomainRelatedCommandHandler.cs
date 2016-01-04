using System;
using System.Reflection;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.CommandHandlers
{
    public class NonDomainRelatedCommandHandler : 
        ICommandHandler<LogApplicationStartUp>,
        ICommandHandler<SaveTemporaryOrderCreationInfo>,
        ICommandHandler<SaveTemporaryOrderPaymentInfo>,
        ICommandHandler<AddOrUpdateVehicleIdMapping>,
        ICommandHandler<SaveTemporaryCompanyPaymentSettings>
    {
        private readonly Func<BookingDbContext> _bookingContextFactory;
        private readonly Func<ConfigurationDbContext> _configurationContextFactory; 

        public NonDomainRelatedCommandHandler(Func<BookingDbContext> bookingContextFactory, Func<ConfigurationDbContext> configurationContextFactory)
        {
            _bookingContextFactory = bookingContextFactory;
            _configurationContextFactory = configurationContextFactory;
        }

        public void Handle(LogApplicationStartUp command)
        {
            using (var context = _bookingContextFactory.Invoke())
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
            using (var context = _bookingContextFactory.Invoke())
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
            using (var context = _bookingContextFactory.Invoke())
            {
                context.Save(new TemporaryOrderPaymentInfoDetail
                {
                    OrderId = command.OrderId,
                    Cvv = command.Cvv
                });
            }
        }

        public void Handle(AddOrUpdateVehicleIdMapping command)
        {
            using (var context = _bookingContextFactory.Invoke())
            {
                var existingMappingForOrder = context.Find<VehicleIdMappingDetail>(command.OrderId);
                if (existingMappingForOrder != null)
                {
                    existingMappingForOrder.DeviceName = command.DeviceName;
                    existingMappingForOrder.LegacyDispatchId = command.LegacyDispatchId;

                    context.Save(existingMappingForOrder);
                }
                else
                {
                    context.Save(new VehicleIdMappingDetail
                    {
                        OrderId = command.OrderId,
                        DeviceName = command.DeviceName,
                        LegacyDispatchId = command.LegacyDispatchId,
                        CreationDate = DateTime.UtcNow
                    });
                }
            }
        }

        public void Handle(SaveTemporaryCompanyPaymentSettings command)
        {
            if (!command.ServerPaymentSettings.CompanyKey.HasValue())
            {
                // Only cache those payment settings for non-local/network companies
                return;
            }

            using (var context = _configurationContextFactory.Invoke())
            {
                // Remove old settings
                context.RemoveWhere<ServerPaymentSettings>(x => x.CompanyKey == command.ServerPaymentSettings.CompanyKey);

                // Add new settings
                context.ServerPaymentSettings.Add(command.ServerPaymentSettings);

                context.SaveChanges();
            }
        }
    }
}