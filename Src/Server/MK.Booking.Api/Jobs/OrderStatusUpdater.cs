#region

using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using System;
using apcurium.MK.Booking.Maps;
using apcurium.MK.Booking.EventHandlers.Integration;
using log4net;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderPaymentDao _orderPayementDao;
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;
        private readonly IDirections _directions;
        private readonly IIbsOrderService _ibs;

        private ReadModel.OrderDetail _orderDetails;

        private static readonly ILog Log = LogManager.GetLogger(typeof(CreateOrderService));

        public OrderStatusUpdater(IConfigurationManager configurationManager, ICommandBus commandBus,
            IOrderPaymentDao orderPayementDao, IOrderDao orderDao, IDirections directions, IIbsOrderService ibs)
        {
            _orderDao = orderDao;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPayementDao = orderPayementDao;
            _directions = directions;
            _ibs = ibs;
            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new Resources.Resources(applicationKey);
        }

        public void Update(IBSOrderInformation ibsStatus, OrderStatusDetail order)
        {
            var statusChanged = (ibsStatus.Status.HasValue() && order.IBSStatusId != ibsStatus.Status)
                                || order.VehicleLatitude != ibsStatus.VehicleLatitude
                                || order.VehicleLongitude != ibsStatus.VehicleLongitude
                                || (!order.FareAvailable && ibsStatus.Fare > 0) ; //fare was not available and ibs has the information

            if (!statusChanged)
            {
                return;
            }

            _orderDetails = _orderDao.FindById(order.OrderId);
            var languageCode = _orderDetails != null ? _orderDetails.ClientLanguageCode : "en";
            var pairingInfo = _orderDao.FindOrderPairingById(order.OrderId);
            if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {
                double tip = ((double)pairingInfo.AutoTipPercentage.Value) / 100;
                ibsStatus.Tip = Math.Round(ibsStatus.Fare * (tip), 2);
            }

            var command = new ChangeOrderStatus
            {
                Status = order,
                Fare = ibsStatus.Fare,
                Toll = ibsStatus.Toll,
                Tip = ibsStatus.Tip,
                Tax = ibsStatus.VAT,
            };

            ibsStatus.Update(order);

            string description = null;

            if (ibsStatus.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", languageCode), ibsStatus.VehicleNumber);

                if (_orderDetails != null &&  _configurationManager.GetSetting("Client.ShowEta", false))
                {
                    try
                    {
                        SendEtaMessageToDriver((double) order.VehicleLatitude, (double) order.VehicleLongitude,
                            ibsStatus.VehicleNumber);
                    }
                    catch (Exception e)
                    {
                        Log.Error("Cannot Send Eta to Vehicle Number " + ibsStatus.VehicleNumber);
                    }
                }
            }
            else if (ibsStatus.IsCanceled)
            {
                order.Status = OrderStatus.Canceled;
                description = _resources.Get("OrderStatus_" + ibsStatus.Status, languageCode);
            }
            else if (ibsStatus.IsTimedOut)
            {
                order.Status = OrderStatus.TimedOut;
            }
            else if (ibsStatus.IsComplete)
            {
                order.Status = OrderStatus.Completed;

                //FormatPrice
                var total =
                    Params.Get(ibsStatus.Toll, ibsStatus.Fare, ibsStatus.Tip, ibsStatus.VAT)
                            .Select(amount => amount)
                            .Sum();

                if (total > 0)
                {
                    description = string.Format(_resources.Get("OrderStatus_OrderDoneFareAvailable", languageCode), FormatPrice(total));
                    order.FareAvailable = true;
                }
                else
                {
                    description = _resources.Get("OrderStatus_wosDONE", languageCode);
                    order.FareAvailable = false;
                }

                var payment = _orderPayementDao.FindByOrderId(order.OrderId);
                if (payment != null)
                {
                    order.FareAvailable = true;
                }
            }

            order.IBSStatusDescription = description.HasValue()
                                             ? description
                                             : _resources.Get("OrderStatus_" + ibsStatus.Status, languageCode);


            _commandBus.Send(command);
        }

        private void SendEtaMessageToDriver(double vehicleLatitude, double vehicleLongitude, string vehicleNumber)
        {
            var eta = _directions.GetEta(_orderDetails.PickupAddress.Latitude, _orderDetails.PickupAddress.Longitude, vehicleLatitude, vehicleLongitude);

            if (eta != null && eta.IsValidEta())
            {
                _ibs.SendMessageToDriver("ETA displayed to client is " + eta.FormattedDistance + " and " + eta.Duration + " min", vehicleNumber);
            }
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}