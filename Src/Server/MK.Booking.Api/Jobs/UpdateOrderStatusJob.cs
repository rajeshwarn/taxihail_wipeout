﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Common;
using System.Threading;
using log4net;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Common.Configuration;
using System.Reactive.Linq;
using System.Diagnostics;
using apcurium.MK.Common.Enumeration;
using HoneyBadger;
using HoneyBadger.Responses;
using ServiceStack.Common;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class UpdateOrderStatusJob : IUpdateOrderStatusJob
    {
        private readonly IOrderDao _orderDao;
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IOrderStatusUpdateDao _orderStatusUpdateDao;
        private readonly OrderStatusUpdater _orderStatusUpdater;
        private readonly HoneyBadgerServiceClient _honeyBadgerServiceClient;
        private readonly IServerSettings _serverSettings;

        private static readonly ILog Log = LogManager.GetLogger(typeof(UpdateOrderStatusJob));

        private const int NumberOfConcurrentServers = 2;

        public UpdateOrderStatusJob(IOrderDao orderDao,
            IIBSServiceProvider ibsServiceProvider,
            IOrderStatusUpdateDao orderStatusUpdateDao,
            OrderStatusUpdater orderStatusUpdater,
            HoneyBadgerServiceClient honeyBadgerServiceClient,
            IServerSettings serverSettings)
        {
            _orderStatusUpdateDao = orderStatusUpdateDao;
            _orderDao = orderDao;
            _ibsServiceProvider = ibsServiceProvider;
            _orderStatusUpdater = orderStatusUpdater;
            _honeyBadgerServiceClient = honeyBadgerServiceClient;
            _serverSettings = serverSettings;
        }

        public void CheckStatus(Guid orderId)
        {
            Func<OrderDetail> getOrderDetails = () =>
            {
                var o = _orderDao.FindById(orderId);
                if (o == null)
                {
                    throw new Exception();
                }
                return o;
            };

            try
            {
                var order = getOrderDetails.Retry(TimeSpan.FromMilliseconds(300), 7);
                if (order != null && order.IBSOrderId.HasValue)
                {
                    var orderStatus = _orderDao.FindOrderStatusById(orderId);
                    var status = _ibsServiceProvider.Booking(orderStatus.CompanyKey).GetOrdersStatus( new [] { order.IBSOrderId.Value });
                 
                    _orderStatusUpdater.Update(status.ElementAt(0), orderStatus);
                }
            }
            catch
            {}
        }

        public bool CheckStatus(string updaterUniqueId, int pollingValue)
        {
            var lastUpdate = _orderStatusUpdateDao.GetLastUpdate();
            bool hasOrdersWaitingForPayment = false;
            
            Log.DebugFormat("Attempting to CheckStatus with {0}", updaterUniqueId);

            if ((lastUpdate == null) ||
                (lastUpdate.UpdaterUniqueId == updaterUniqueId) ||
                (DateTime.UtcNow.Subtract(lastUpdate.LastUpdateDate).TotalSeconds > NumberOfConcurrentServers * pollingValue))
            {
                // Update LastUpdateDate while processing to block the other instance from starting while we're executing the try block
                var timer = Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(pollingValue))
                                .Subscribe(_ =>_orderStatusUpdateDao.UpdateLastUpdate(updaterUniqueId, DateTime.UtcNow));

                Log.DebugFormat("CheckStatus was allowed for {0}", updaterUniqueId);

                try
                {
                    var orders = _orderDao.GetOrdersInProgress();
                    var groupedOrders = orders.GroupBy(x => new { x.CompanyKey, x.Market });

                    foreach (var orderGroup in groupedOrders)
                    {
                        var ordersForCompany = orderGroup.ToArray();

                        Log.DebugFormat("Starting BatchUpdateStatus with {0} orders{1}", ordersForCompany.Count(), string.IsNullOrWhiteSpace(orderGroup.Key.CompanyKey)
                            ? string.Empty
                            : string.Format(" for company {0}", orderGroup.Key.CompanyKey));
                        Log.DebugFormat("Starting BatchUpdateStatus with {0} orders of status {1}", ordersForCompany.Count(o => o.Status == OrderStatus.WaitingForPayment), "WaitingForPayment");
                        BatchUpdateStatus(orderGroup.Key.CompanyKey, orderGroup.Key.Market, ordersForCompany.Where(o => o.Status == OrderStatus.WaitingForPayment));
                        Log.DebugFormat("Starting BatchUpdateStatus with {0} orders of status {1}", ordersForCompany.Count(o => o.Status == OrderStatus.Pending), "Pending");
                        BatchUpdateStatus(orderGroup.Key.CompanyKey, orderGroup.Key.Market, ordersForCompany.Where(o => o.Status == OrderStatus.Pending));
                        Log.DebugFormat("Starting BatchUpdateStatus with {0} orders of status {1}", ordersForCompany.Count(o => o.Status == OrderStatus.TimedOut), "TimedOut");
                        BatchUpdateStatus(orderGroup.Key.CompanyKey, orderGroup.Key.Market, ordersForCompany.Where(o => o.Status == OrderStatus.TimedOut));
                        Log.DebugFormat("Starting BatchUpdateStatus with {0} orders of status {1}", ordersForCompany.Count(o => o.Status == OrderStatus.Created), "Created");
                        BatchUpdateStatus(orderGroup.Key.CompanyKey, orderGroup.Key.Market, ordersForCompany.Where(o => o.Status == OrderStatus.Created));
                    }
                    
                    hasOrdersWaitingForPayment = orders.Any(o => o.Status == OrderStatus.WaitingForPayment);
                }
                finally
                {
                    timer.Dispose();
                }
            }
            else
            {
                Log.DebugFormat("CheckStatus was blocked for {0}", updaterUniqueId);
            }

            return hasOrdersWaitingForPayment;
        }

        private void BatchUpdateStatus(string companyKey, string market, IEnumerable<OrderStatusDetail> orders)
        {
            // Enumerate orders to avoid multiple enumerations of IEnumerable
            var orderStatusDetails = orders as OrderStatusDetail[] ?? orders.ToArray();

            var manualRideLinqOrders = orderStatusDetails.Where(o => o.IsManualRideLinq);
            foreach (var orderStatusDetail in manualRideLinqOrders)
            {
                _orderStatusUpdater.HandleManualRidelinqFlow(orderStatusDetail);
            }

            var ibsOrdersIds = orderStatusDetails
                .Where(order => !order.IsManualRideLinq)
                .Select(statusDetail => statusDetail.IBSOrderId ?? 0)
                .ToList();

            const int take = 10;
            for (var skip = 0; skip < ibsOrdersIds.Count; skip = skip + take)
            {
                var nextGroup = ibsOrdersIds.Skip(skip).Take(take).ToList();
                var orderStatuses = _ibsServiceProvider.Booking(companyKey).GetOrdersStatus(nextGroup).ToArray();

                // If HoneyBadger for local market is enabled, we need to fetch the vehicle position from HoneyBadger instead of using the position data from IBS
                var honeyBadgerVehicleStatuses = GetVehicleStatusesFromHoneyBadgerIfNecessary(orderStatuses, market).ToArray();

                foreach (var ibsStatus in orderStatuses)
                {
                    if (honeyBadgerVehicleStatuses.Any())
                    {
                        // Update vehicle position with matching data available data from HoneyBadger
                        var honeyBadgerVehicleStatus = honeyBadgerVehicleStatuses.FirstOrDefault(v => v.Medallion == ibsStatus.VehicleNumber);
                        if (honeyBadgerVehicleStatus != null)
                        {
                            ibsStatus.VehicleLatitude = honeyBadgerVehicleStatus.Latitude;
                            ibsStatus.VehicleLongitude = honeyBadgerVehicleStatus.Longitude;
                        }
                    }

                    var order = orderStatusDetails.FirstOrDefault(o => o.IBSOrderId == ibsStatus.IBSOrderId);
                    if (order == null)
                    {
                        continue;
                    }

                    Log.DebugFormat("Starting OrderStatusUpdater for order {0} (IbsOrderId: {1})", order.OrderId, order.IBSOrderId);

                    _orderStatusUpdater.Update(ibsStatus, order);
                }
            }
        }

        private IEnumerable<VehicleResponse> GetVehicleStatusesFromHoneyBadgerIfNecessary(IBSOrderInformation[] orderStatuses, string market)
        {

            if (_serverSettings.ServerData.AvailableVehiclesMode == AvailableVehiclesModes.HoneyBadger
                && _serverSettings.ServerData.AvailableVehiclesMarket.HasValue())
            {
                var vehicleMedallions = orderStatuses.Select(x => x.VehicleNumber);
                var vehicleMarket = !market.HasValue()
                    ? _serverSettings.ServerData.AvailableVehiclesMarket // Local market
                    : market;                                            // External market

                // Get vehicle statuses/position from HoneyBadger
                return _honeyBadgerServiceClient.GetVehicleStatus(vehicleMarket, vehicleMedallions);
            }

            return new VehicleResponse[0];
        }
    }
}