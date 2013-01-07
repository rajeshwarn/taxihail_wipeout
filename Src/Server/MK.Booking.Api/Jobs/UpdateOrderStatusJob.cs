using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.ServiceClient.Web;
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
        private readonly IConfigurationManager _configManager;
        private readonly IBookingWebServiceClient _bookingWebServiceClient;
        private readonly ICommandBus _commandBus;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateOrderStatusJob));
        private const string AssignedStatus = "wosASSIGNED";
        private const string DoneStatus = "wosDONE";

        public UpdateOrderStatusJob(IOrderDao orderDao, IConfigurationManager configManager, IBookingWebServiceClient bookingWebServiceClient, ICommandBus commandBus)
        {
            _orderDao = orderDao;
            _configManager = configManager;
            _bookingWebServiceClient = bookingWebServiceClient;
            _commandBus = commandBus;
        }

        public void CheckStatus()
        {
            try
            {
                var orders = _orderDao.GetOrdersInProgress();
                foreach (var orderStatusDetail in orders)
                {
                    Logger.Debug("Get Status for " + orderStatusDetail.OrderId);
                    var ibsStatus = _bookingWebServiceClient.GetOrderStatus(orderStatusDetail.IBSOrderId.Value, 0, string.Empty);

                    if (ibsStatus.Status.HasValue()
                        && orderStatusDetail.IBSStatusId != ibsStatus.Status)
                    {
                        string description = null;
                        Logger.Debug("Status Changed for " + orderStatusDetail.OrderId);
                        var command = new ChangeOrderStatus {Status = orderStatusDetail};
                        orderStatusDetail.IBSStatusId = ibsStatus.Status;
                        orderStatusDetail.Status = OrderStatus.Pending;

                        if (ibsStatus.Status.SoftEqual(AssignedStatus))
                        {
                            var orderDetails = _bookingWebServiceClient.GetOrderDetails(orderStatusDetail.IBSOrderId.Value, 0, string.Empty);
                            if ((orderDetails != null) && (orderDetails.VehicleNumber.HasValue()))
                            {
                                Logger.Debug("Vehicle number :  " + orderDetails.VehicleNumber);
                                orderStatusDetail.VehicleNumber = orderDetails.VehicleNumber;
                                description = string.Format(_configManager.GetSetting("OrderStatus.CabDriverNumberAssigned"),
                                                     orderDetails.VehicleNumber);
                                if (!string.IsNullOrEmpty(orderDetails.CallNumber))
                                {
                                    var driverInfos = _bookingWebServiceClient.GetDriverInfos(orderDetails.CallNumber);
                                    orderStatusDetail.DriverInfos.FirstName = driverInfos.FirstName;
                                    orderStatusDetail.DriverInfos.LastName = driverInfos.LastName;
                                    orderStatusDetail.DriverInfos.MobilePhone = driverInfos.MobilePhone;
                                    orderStatusDetail.DriverInfos.VehicleColor = driverInfos.VehicleColor;
                                    orderStatusDetail.DriverInfos.VehicleMake = driverInfos.VehicleMake;
                                    orderStatusDetail.DriverInfos.VehicleModel = driverInfos.VehicleModel;
                                    orderStatusDetail.DriverInfos.VehicleRegistration = driverInfos.VehicleRegistration;
                                    orderStatusDetail.DriverInfos.VehicleType = driverInfos.VehicleType;
                                }
                            }
                            

                            if (_configManager.GetSetting("OrderStatus.DemoMode") == "true")
                            {
                                Logger.Debug("DEMO MODE IS ACTIVE!");
                                DemoModeFakePosition(orderStatusDetail);
                            }
                            else if (ibsStatus.VehicleLatitude.HasValue && ibsStatus.VehicleLongitude.HasValue)
                            {
                                Logger.Debug(string.Format("Vehicle poistion : Lat {0} : Lng{1}", ibsStatus.VehicleLatitude, ibsStatus.VehicleLongitude));
                                orderStatusDetail.VehicleLatitude = ibsStatus.VehicleLatitude;
                                orderStatusDetail.VehicleLongitude = ibsStatus.VehicleLongitude;
                            }
                            else
                            {
                                Logger.Debug("CANNOT GET VEHICULE POSITION");
                            }
                        }

                        if (ibsStatus.Status.SoftEqual(DoneStatus))
                        {
                            orderStatusDetail.Status = OrderStatus.Completed;
                            var orderDetails = _bookingWebServiceClient.GetOrderDetails(orderStatusDetail.IBSOrderId.Value, 0, string.Empty);

                            if ((orderDetails != null) && ((orderDetails.Fare.HasValue || orderDetails.Tip.HasValue || orderDetails.Toll.HasValue)))
                            {
                                //FormatPrice
                                var total = Params.Get<double?>(orderDetails.Toll, orderDetails.Fare, orderDetails.Tip).Where(amount => amount.HasValue).Select(amount => amount.Value).Sum();
                                description = string.Format(_configManager.GetSetting("OrderStatus.OrderDoneFareAvailable"), FormatPrice(total));
                                orderStatusDetail.FareAvailable = true;
                            }

                            command.Fare = orderDetails != null ? orderDetails.Fare : null;
                            command.Toll = orderDetails != null ? orderDetails.Toll : null;
                            command.Tip = orderDetails != null ? orderDetails.Tip : null;

                        }

                        if (description.HasValue())
                        {
                            orderStatusDetail.IBSStatusDescription = description;
                        }
                        else
                        {
                            orderStatusDetail.IBSStatusDescription = _configManager.GetSetting("OrderStatus." + ibsStatus.Status);
                        }

                        
                        _commandBus.Send(command);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }
        }

        private string FormatPrice(double? price)
        {
            var culture = _configManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
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