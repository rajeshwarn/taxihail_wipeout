﻿using System;
using System.Globalization;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using log4net;
using apcurium.MK.Booking.ReadModel.Query;

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private readonly IConfigurationManager _configurationManager;
        readonly ICommandBus _commandBus;
        private readonly IOrderPaymentDao _orderPayementDao;
        private readonly IOrderDao _orderDao;
        static readonly ILog Logger = LogManager.GetLogger(typeof(OrderStatusUpdater));
        readonly dynamic _resources;

        public OrderStatusUpdater(IConfigurationManager configurationManager, ICommandBus commandBus, IOrderPaymentDao orderPayementDao, IOrderDao orderDao)
        {
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPayementDao = orderPayementDao;
            _orderDao = orderDao;
            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new DynamicResources(applicationKey);
        }

        public void Update(IBSOrderInformation ibsStatus, OrderStatusDetail order)
        {
            var statusChanged = (ibsStatus.Status.HasValue() && order.IBSStatusId != ibsStatus.Status)
                                || order.VehicleLatitude != ibsStatus.VehicleLatitude
                                || order.VehicleLongitude != ibsStatus.VehicleLongitude;

            if (!statusChanged)
            {
                return;
            }


            var pairingInfo = _orderDao.FindOrderPairingById(order.OrderId);
            if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {
                double tip =  ((double) pairingInfo.AutoTipPercentage.Value) / 100;
                ibsStatus.Tip =  Math.Round(ibsStatus.Fare * (tip), 2);
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
                description = string.Format((string)_resources.OrderStatus_CabDriverNumberAssigned, ibsStatus.VehicleNumber);

                if (ibsStatus.Eta.HasValue)
                {
                    description += " - " + string.Format((string)_resources.OrderStatus_CabDriverETA, ibsStatus.Eta.Value.ToString("t"));
                }
            }
            else if (ibsStatus.IsCanceled)
            {
                order.Status = OrderStatus.Canceled;
                description = (string) _resources.GetString("OrderStatus_" + ibsStatus.Status);
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
                    description = string.Format((string)_resources.OrderStatus_OrderDoneFareAvailable, FormatPrice(total));
                    order.FareAvailable = true;
                }
                else
                {
                    description = (string)_resources.OrderStatus_wosDONE;
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
                                             : (string)_resources.GetString("OrderStatus_" + ibsStatus.Status);

            
            _commandBus.Send(command);
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}