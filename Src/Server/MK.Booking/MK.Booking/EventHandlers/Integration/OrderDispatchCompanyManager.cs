using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.Events;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration.Impl;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using CustomerPortal.Client;
using CustomerPortal.Contract.Resources;
using CustomerPortal.Contract.Response;
using Infrastructure.Messaging;
using Infrastructure.Messaging.Handling;

namespace apcurium.MK.Booking.EventHandlers.Integration
{
    public class OrderDispatchCompanyManager : IIntegrationEventHandler,
        IEventHandler<OrderTimedOut>
    {
        private readonly ICommandBus _commandBus;
        private readonly Func<BookingDbContext> _contextFactory;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly ITaxiHailNetworkServiceClient _taxiHailNetworkServiceClient;

        public OrderDispatchCompanyManager(
            ICommandBus commandBus,
            Func<BookingDbContext> contextFactory,
            IIBSServiceProvider ibsServiceProvider,
            ITaxiHailNetworkServiceClient taxiHailNetworkServiceClient)
        {
            _contextFactory = contextFactory;
            _ibsServiceProvider = ibsServiceProvider;
            _commandBus = commandBus;
            _taxiHailNetworkServiceClient = taxiHailNetworkServiceClient;
        }

        public async void Handle(OrderTimedOut @event)
        {
            using (var context = _contextFactory.Invoke())
            {
                var details = context.Find<OrderStatusDetail>(@event.SourceId);
                if (details == null)
                {
                    throw new InvalidOperationException("Order Status not found");
                }

                var order = context.Find<OrderDetail>(@event.SourceId);
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found");
                }

                var pickUpPosition = new MapCoordinate
                {
                    Latitude = order.PickupAddress.Latitude,
                    Longitude = order.PickupAddress.Longitude
                };

                // Get network fleet from customer portal
                var networkFleet = await _taxiHailNetworkServiceClient.GetNetworkFleetAsync(details.CompanyKey, pickUpPosition.Latitude, pickUpPosition.Longitude);

                var nextDispatchCompany = FindNextDispatchCompany(details.CompanyKey, pickUpPosition, networkFleet);
                if (nextDispatchCompany != null)
                {
                    _commandBus.Send(new PrepareOrderForNextDispatch
                    {
                        OrderId = details.OrderId,
                        DispatchCompanyName = nextDispatchCompany.CompanyName,
                        DispatchCompanyKey = nextDispatchCompany.CompanyKey
                    });
                }
            }
        }

        private NetworkFleetResponse FindNextDispatchCompany(string currentCompanyKey, MapCoordinate userPosition, IList<NetworkFleetResponse> networkFleet)
        {
            if (networkFleet == null || !networkFleet.Any())
            {
                return null;
            }

            NetworkFleetResponse nextDispatchCompany = currentCompanyKey.IsNullOrEmpty()
                ? networkFleet.First()
                : FindNextAvailableCompanyInIbsZone(currentCompanyKey, userPosition, networkFleet);

            return nextDispatchCompany;
        }

        private NetworkFleetResponse FindNextAvailableCompanyInIbsZone(string currentCompanyKey, MapCoordinate userPosition, IList<NetworkFleetResponse> networkFleet)
        {
            // Find the list index of the current company
            var currentDispatchCompany = networkFleet.First(f => f.CompanyKey == currentCompanyKey);
            var currentDispatchCompanyIndex = networkFleet.IndexOf(currentDispatchCompany);

            if (currentDispatchCompanyIndex == networkFleet.Count - 1)
            {
                // End of list, no more company in fleet
                return currentDispatchCompany;
            }

            var nextDispatchCompany = networkFleet[currentDispatchCompanyIndex + 1];

            // Check if company is in IBS zone
            var ibsZone = _ibsServiceProvider.StaticData(nextDispatchCompany.CompanyKey)
                .GetZoneByCoordinate(null, userPosition.Latitude, userPosition.Longitude);

            // If company is not IBS zone, check the next one until we have browsed the whole fleet
            return string.IsNullOrWhiteSpace(ibsZone)
                ? FindNextAvailableCompanyInIbsZone(nextDispatchCompany.CompanyKey, userPosition, networkFleet)
                : nextDispatchCompany;
        }
    }
}
