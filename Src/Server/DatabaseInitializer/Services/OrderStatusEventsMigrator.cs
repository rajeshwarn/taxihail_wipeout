using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.Serialization;
using Infrastructure.Sql.EventSourcing;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;
using apcurium.MK.Common.Entity;
using log4net;

namespace DatabaseInitializer.Services
{
    public class OrderStatusEventsMigrator : IEventsMigrator
    {
        private readonly Func<EventStoreDbContext> _contextFactory;
        private readonly ITextSerializer _textSerializer;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private ILog _loggger;

        public OrderStatusEventsMigrator(Func<EventStoreDbContext> contextFactory, ITextSerializer textSerializer, IBookingWebServiceClient bookingWebServiceClient)
        {
            _contextFactory = contextFactory;
            _textSerializer = textSerializer;
            _bookingWebServiceClient = bookingWebServiceClient;
            _loggger = LogManager.GetLogger("DatabaseInitializer");
        }

        public void Do(string version)
        {
            var versionNumbers = version.Split('.');
            var versionNumber = int.Parse(versionNumbers[1]);
            var minorVersionNumber = 0;
            if (versionNumbers.Length > 2)
            {
                minorVersionNumber = int.Parse(versionNumbers[2]);
            }


            if (versionNumber < 3
                || (versionNumber == 3 && minorVersionNumber == 0))
            {
                _loggger.Debug("Migrating Order StatusEvent");
                using (var context = _contextFactory.Invoke())
                {
                    var orderCreatedEvents = context.Set<Event>().Where(x => x.EventType == typeof(OrderCreated).FullName).ToList();

                    var ibsOrderIds = new List<int>();
                    foreach (var orderCreatedMessage in orderCreatedEvents)
                    {
                        var orderCreatedEvent = _textSerializer.Deserialize<OrderCreated>(orderCreatedMessage.Payload);
                        ibsOrderIds.Add(orderCreatedEvent.IBSOrderId);
                    }

                    var ibsOrderItsSplit = ibsOrderIds.Select((x, i) => new { Index = i, Value = x })
                                            .GroupBy(x => x.Index / 50)
                                            .Select(x => x.Select(v => v.Value).ToList())
                                            .ToList();

                    var ibsOrdersInformations = new List<IBSOrderInformation>();
                    foreach (var listOfIds in ibsOrderItsSplit)
                    {
                        var infosFromIbs = _bookingWebServiceClient.GetOrdersStatus(listOfIds.ToArray());
                        ibsOrdersInformations.AddRange(infosFromIbs);
                    }

                    foreach (var orderCreatedMessage in orderCreatedEvents)
                    {
                        var orderCreatedEvent = _textSerializer.Deserialize<OrderCreated>(orderCreatedMessage.Payload);

                        
                        var versionEvent = (from @event in context.Set<Event>()
                                   where @event.AggregateId == orderCreatedMessage.AggregateId
                                   select @event.Version).Max() + 1;
                        var statusChangedEvent = new OrderStatusChanged
                                                     {
                                                         EventDate = DateTime.UtcNow,
                                                         SourceId = orderCreatedMessage.AggregateId,
                                                         Version = versionEvent,
                                                         Status = new OrderStatusDetail{ OrderId = orderCreatedMessage.AggregateId, IBSOrderId = orderCreatedEvent.IBSOrderId}
                                                     };

                        CompleteStatusFromIbs(statusChangedEvent, ibsOrdersInformations.FirstOrDefault(x => x.IBSOrderId == orderCreatedEvent.IBSOrderId));


                        var statusChangedEventMessage = new Event
                                                            {
                                                                AggregateId = orderCreatedMessage.AggregateId,
                                                                AggregateType = orderCreatedMessage.AggregateType,
                                                                CorrelationId = orderCreatedMessage.CorrelationId,
                                                                EventDate = DateTime.UtcNow,
                                                                Payload = _textSerializer.Serialize(statusChangedEvent),
                                                                Version = versionEvent,
                                                                EventType = typeof(OrderStatusChanged).FullName
                                                            };
                        context.Set<Event>().Add(statusChangedEventMessage);
                    }
                    context.SaveChanges();
                }
            }
        }

        private void CompleteStatusFromIbs(OrderStatusChanged statusChangedEvent, IBSOrderInformation ibsInformations)
        {
            statusChangedEvent.Status.IBSStatusId = ibsInformations.Status;
            statusChangedEvent.Status.Status = MapStatus(ibsInformations.Status);
            statusChangedEvent.Status.DriverInfos.FirstName = ibsInformations.FirstName;
            statusChangedEvent.Status.DriverInfos.LastName = ibsInformations.LastName;
            statusChangedEvent.Status.DriverInfos.MobilePhone = ibsInformations.MobilePhone;
            statusChangedEvent.Status.DriverInfos.VehicleColor = ibsInformations.VehicleColor;
            statusChangedEvent.Status.DriverInfos.VehicleMake = ibsInformations.VehicleMake;
            statusChangedEvent.Status.DriverInfos.VehicleModel = ibsInformations.VehicleModel;
            statusChangedEvent.Status.DriverInfos.VehicleRegistration = ibsInformations.VehicleRegistration;
            statusChangedEvent.Status.DriverInfos.VehicleType = ibsInformations.VehicleType;
            statusChangedEvent.Status.VehicleLatitude = ibsInformations.VehicleLatitude;
            statusChangedEvent.Status.VehicleLongitude = ibsInformations.VehicleLongitude;
        }



        private OrderStatus MapStatus(string statusIBS)
        {
            var status = OrderStatus.Pending;
            switch (statusIBS)
            {
                case VehicleStatuses.Common.Assigned:
                    return status = OrderStatus.Pending;
                    break;
                case VehicleStatuses.Common.Done:
                    return status = OrderStatus.Completed;
                    break;
                case VehicleStatuses.Common.Cancelled:
                    return status = OrderStatus.Canceled;
                    break;
                case VehicleStatuses.Unknown.None:
                    return status = OrderStatus.Completed;
                    break;
                case VehicleStatuses.Common.NoShow:
                    return status = OrderStatus.Completed;
                    break;
                case VehicleStatuses.Common.Waiting:
                    return status = OrderStatus.Pending;
                    break;
            }
            return status;
        }
    }
}