using System.Net;
using Infrastructure.Messaging;
using ServiceStack.Common.Web;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Google.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System.Globalization;
using System.Collections.Generic;
using System;
using ServiceStack.ServiceClient.Web;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<OrderStatusRequest>
    {
        private static Dictionary<Guid, List<Location>> _fakeTaxiPositions = new Dictionary<Guid, List<Location>>();
        private static Dictionary<Guid, int> _fakeTaxiPositionsIndex = new Dictionary<Guid, int>();

        private const string _assignedStatus = "wosASSIGNED";
        private const string _doneStatus = "wosDONE";


        private IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private IOrderDao _orderDao;
        private IAccountDao _accountDao;
        private IConfigurationManager _configManager;

        public OrderStatusService(IOrderDao orderDao, IAccountDao accountDao, IConfigurationManager configManager, IBookingWebServiceClient bookingWebServiceClient, ICommandBus commandBus)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _commandBus = commandBus;
            _configManager = configManager;

        }

        public override object OnGet(OrderStatusRequest request)
        {
            var status = new OrderStatusDetail();
            var order = _orderDao.FindById(request.OrderId);
            var account = _accountDao.FindById(new Guid(this.GetSession().UserAuthId));

            if (!order.IBSOrderId.HasValue)
            {
                return new OrderStatusDetail { IBSStatusDescription = "Error getting the order status" };
            }

            if (account.Id != order.AccountId)
            {
                throw new HttpError(HttpStatusCode.Unauthorized, "Can't access another account's order");
            }

            try
            {
                var statusDetails = _bookingWebServiceClient.GetOrderStatus(order.IBSOrderId.Value, account.IBSAccountId);

                status.OrderId = request.OrderId;
                status.IBSOrderId = order.IBSOrderId;
                status.IBSStatusId = statusDetails.Status;

                string desc = "";
                if (status.IBSStatusId.HasValue())
                {
                    if (status.IBSStatusId.SoftEqual(_assignedStatus))
                    {
                        var orderDetails = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);
                        if ((orderDetails != null) && (orderDetails.VehicleNumber.HasValue()))
                        {
                            status.VehicleNumber = orderDetails.VehicleNumber;
                            desc = string.Format(_configManager.GetSetting("OrderStatus.CabDriverNumberAssigned"), orderDetails.VehicleNumber);
                        }
                        else
                        {
                            desc = _configManager.GetSetting("OrderStatus." + status.IBSStatusId);
                        }

                        DemoModeFakePosition(status, order);
                    }
                    else if (status.IBSStatusId.SoftEqual(_doneStatus))
                    {
                        var orderDetails = _bookingWebServiceClient.GetOrderDetails(order.IBSOrderId.Value, account.IBSAccountId, order.Settings.Phone);

                        if ((orderDetails != null) && (orderDetails.Fare.HasValue))
                        {
                            //FormatPrice
                            desc = string.Format(_configManager.GetSetting("OrderStatus.OrderDoneFareAvailable"), FormatPrice(orderDetails.Fare), FormatPrice(orderDetails.Toll));
                        }
                        else
                        {
                            desc = _configManager.GetSetting("OrderStatus." + status.IBSStatusId);
                        }

                        if(order.Status != (int)OrderStatus.Completed)
                        {
                            var completeOrder = new CompleteOrder {Date = DateTime.UtcNow};
                            if(orderDetails != null)
                            {
                                completeOrder.Fare = orderDetails.Fare;
                                completeOrder.Toll = orderDetails.Toll;
                                completeOrder.Tip = orderDetails.Tip;
                            }
                            _commandBus.Send(completeOrder);
                        }
                    }
                    else
                    {
                        desc = _configManager.GetSetting("OrderStatus." + status.IBSStatusId);
                    }

                }

                status.IBSStatusDescription = desc.HasValue() ? desc : status.IBSStatusId;

            }
            catch
            {
                //TODO: erreur ? Status ?
            }
            return status;
        }

        private void DemoModeFakePosition(OrderStatusDetail status, ReadModel.OrderDetail order)
        {
            if (_configManager.GetSetting("OrderStatus.DemoMode") == "true")
            {
                //For demo purpose only
                var leg = BuildFakeDirectionForOrder(status.OrderId, order.PickupLatitude, order.PickupLongitude);

                if (leg != null)
                {
                    status.VehicleLatitude = leg.Lat;
                    status.VehicleLongitude = leg.Lng;
                }
            }
        }

        private Location BuildFakeDirectionForOrder(Guid guid, double lat, double lng)
        {

            if (!_fakeTaxiPositions.ContainsKey(guid))
            {
                var result = new DirectionInfo();

                var client = new JsonServiceClient("http://maps.googleapis.com/maps/api/");

                var resource = string.Format(CultureInfo.InvariantCulture, "directions/json?origin={0},{1}&destination={2},{3}&sensor=false", lat, lng, lat + 0.008, lng + 0.008);

                var directions = client.Get<DirectionResult>(resource);

                List<Location> steps = new List<Location>();

                foreach (var s in directions.Routes[0].Legs[0].Steps)
                {
                    steps.Add(s.Start_location);
                    steps.Add(s.End_location);
                    //steps.Add(s);
                    //steps.Add(s);
                    //steps.Add(s);                    
                }
                

                _fakeTaxiPositions.Add(guid, steps);
                _fakeTaxiPositionsIndex.Add(guid, 0);
            }

            var index = _fakeTaxiPositionsIndex[guid];

            _fakeTaxiPositionsIndex[guid] = index + 1;

            if (_fakeTaxiPositionsIndex[guid] >= _fakeTaxiPositions[guid].Count)
            {
                return new  Location { Lat = lat , Lng = lng };
            }
            return _fakeTaxiPositions[guid][_fakeTaxiPositions[guid].Count - index - 1];

        }

        private string FormatPrice(double? price)
        {

            var culture = _configManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }

    }
}