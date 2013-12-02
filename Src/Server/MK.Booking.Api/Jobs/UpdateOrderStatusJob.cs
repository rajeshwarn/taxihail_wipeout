using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.ServiceClient.Web;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using log4net;

namespace apcurium.MK.Booking.Api.Jobs
{
    public class UpdateOrderStatusJob : IUpdateOrderStatusJob
    {
        private readonly IOrderDao _orderDao;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateOrderStatusJob));

        public UpdateOrderStatusJob(IOrderDao orderDao, IBookingWebServiceClient bookingWebServiceClient, OrderStatusUpdater orderStatusUpdater)
        {
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _orderStatusUpdater = orderStatusUpdater;
        }

        public void CheckStatus()
        {
            var orders = _orderDao.GetOrdersInProgress();

            
            var ibsOrdersIds= orders
                .Where(statusDetail => statusDetail.PickupDate >= DateTime.Now.AddDays(-1) || statusDetail.IBSStatusId == VehicleStatuses.Common.Scheduled)
                 .Where(statusDetail => statusDetail.IBSOrderId.HasValue)
                .Select(statusDetail => statusDetail.IBSOrderId.Value)
                .ToList();


            var ibsOrders = new List<IBSOrderInformation>();

            const int take = 5;
            for (var skip = 0; skip < ibsOrdersIds.Count; skip=skip+take)
            {
                var nextGroup = ibsOrdersIds.Skip(skip).Take(take).ToList();
                ibsOrders.AddRange(_bookingWebServiceClient.GetOrdersStatus(nextGroup));
            }

            foreach (var order in orders)
            {
                var ibsStatus = ibsOrders.FirstOrDefault(status => status.IBSOrderId == order.IBSOrderId);

                if (ibsStatus == null) continue;

                _orderStatusUpdater.Update(ibsStatus, order);
            }            


        }

        

        private void DemoModeFakePosition(OrderStatusDetail status)
        {
            var order = _orderDao.FindById(status.OrderId);
            //For demo purpose only
            var leg = BuildFakeDirectionForOrder(status.OrderId, order.PickupAddress.Latitude, order.PickupAddress.Longitude);

            if (leg != null)
            {
                status.VehicleLatitude = leg.Lat;
                status.VehicleLongitude = leg.Lng;
            }

        }

        private static readonly Dictionary<Guid, List<Location>> _fakeTaxiPositions = new Dictionary<Guid, List<Location>>();
        private static readonly Dictionary<Guid, int> _fakeTaxiPositionsIndex = new Dictionary<Guid, int>();
        private readonly OrderStatusUpdater _orderStatusUpdater;

        private Location BuildFakeDirectionForOrder(Guid guid, double lat, double lng)
        {

            if (!_fakeTaxiPositions.ContainsKey(guid))
            {
                var result = new DirectionInfo();

                var client = new JsonServiceClient("http://maps.googleapis.com/maps/api/");

                var resource = string.Format(CultureInfo.InvariantCulture, "directions/json?origin={0},{1}&destination={2},{3}&sensor=false", lat, lng, lat + 0.008, lng + 0.008);

                var directions = client.Get<DirectionResult>(resource);

                var steps = new List<Location>();

                foreach (var s in directions.Routes[0].Legs[0].Steps)
                {
                    steps.Add(s.Start_location);
                    steps.Add(s.End_location);
                }
                _fakeTaxiPositions.Add(guid, steps);
                _fakeTaxiPositionsIndex.Add(guid, 0);
            }

            var index = _fakeTaxiPositionsIndex[guid];

            _fakeTaxiPositionsIndex[guid] = index + 1;

            if (_fakeTaxiPositionsIndex[guid] >= _fakeTaxiPositions[guid].Count)
            {
                return new Location { Lat = lat, Lng = lng };
            }
            return _fakeTaxiPositions[guid][_fakeTaxiPositions[guid].Count - index - 1];

        }
    }
}