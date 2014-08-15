#region

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.Database;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.Maps.Geo;
using apcurium.MK.Booking.PushNotifications;
using apcurium.MK.Booking.ReadModel;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;
using System;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderPaymentDao _orderPaymentDao;
        private readonly IOrderDao _orderDao;
        private readonly Resources.Resources _resources;

        public OrderStatusUpdater(IConfigurationManager configurationManager, 
            ICommandBus commandBus, 
            IOrderPaymentDao orderPaymentDao, 
            IOrderDao orderDao)
        {
            _orderDao = orderDao;
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPaymentDao = orderPaymentDao;
            _resources = new Resources.Resources(configurationManager.GetSetting("TaxiHail.ApplicationKey"));
        }

        private void UpdateVehiclePositionIfNecessary(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatus)
        {
            if (orderStatus.VehicleLatitude != ibsOrderInfo.VehicleLatitude
                || orderStatus.VehicleLongitude != ibsOrderInfo.VehicleLongitude)
            {
                bool taxiNearbyPushSent;
                _orderDao.UpdateVehiclePosition(orderStatus.OrderId, ibsOrderInfo.Status, ibsOrderInfo.VehicleLatitude, ibsOrderInfo.VehicleLongitude, out taxiNearbyPushSent);
                
                // modify orderStatus object here to make sure that if the status is changed and a command is sent
                // using this object, the values set in the dao will not be erased by automapping in the eventhandler
                orderStatus.VehicleLatitude = ibsOrderInfo.VehicleLatitude;
                orderStatus.VehicleLongitude = ibsOrderInfo.VehicleLongitude;
                orderStatus.IsTaxiNearbyNotificationSent = taxiNearbyPushSent;
            }
        }

        private void CheckForPairingAndHandleIfNecessary(Guid orderId, IBSOrderInformation ibsOrderInfo)
        {
            var pairingInfo = _orderDao.FindOrderPairingById(orderId);

            // for cmt ridelinq
            if ((pairingInfo != null) && (pairingInfo.AutoTipPercentage.HasValue))
            {
                var tip = ((double)pairingInfo.AutoTipPercentage.Value) / 100;
                ibsOrderInfo.Tip = Math.Round(ibsOrderInfo.Fare * (tip), 2);
            }
        }

        public void Update(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            UpdateVehiclePositionIfNecessary(ibsOrderInfo, orderStatusDetail);

            if (!OrderWasUpdated(ibsOrderInfo, orderStatusDetail))
            {
                return;
            }
            
            // populate orderStatusDetail with ibsOrderInfo data
            ibsOrderInfo.Update(orderStatusDetail);
            
            CheckForPairingAndHandleIfNecessary(orderStatusDetail.OrderId, ibsOrderInfo);

            UpdateStatusIfNecessary(orderStatusDetail, ibsOrderInfo);
            
            orderStatusDetail.IBSStatusDescription = GetDescription(orderStatusDetail.OrderId, ibsOrderInfo);
            orderStatusDetail.FareAvailable = GetFareAvailable(orderStatusDetail.OrderId, ibsOrderInfo);

            // be careful, the orderStatusDetail will be directly automapped to the database entry
            // so if you send another command or modify orderStatusDetail directly, make sure you also set
            // the value in orderStatusDetail before sending this command
            _commandBus.Send(new ChangeOrderStatus
            {
                Status = orderStatusDetail,
                Fare = ibsOrderInfo.Fare,
                Toll = ibsOrderInfo.Toll,
                Tip = ibsOrderInfo.Tip,
                Tax = ibsOrderInfo.VAT,
            });
        }

        private void UpdateStatusIfNecessary(OrderStatusDetail orderStatusDetail, IBSOrderInformation ibsOrderInfo)
        {
            if (ibsOrderInfo.IsCanceled)
            {
                orderStatusDetail.Status = OrderStatus.Canceled;
            }
            else if (ibsOrderInfo.IsTimedOut)
            {
                orderStatusDetail.Status = OrderStatus.TimedOut;
            }
            else if (ibsOrderInfo.IsComplete)
            {
                orderStatusDetail.Status = OrderStatus.Completed;
            }
        }

        private bool GetFareAvailable(Guid orderId, IBSOrderInformation ibsOrderInfo)
        {
            var fareAvailable = ibsOrderInfo.Fare > 0;
            var payment = _orderPaymentDao.FindByOrderId(orderId);
            if (payment != null)
            {
                fareAvailable = true;
            }
            return fareAvailable;
        }

        private bool OrderWasUpdated(IBSOrderInformation ibsOrderInfo, OrderStatusDetail orderStatusDetail)
        {
            return (ibsOrderInfo.Status.HasValue() && orderStatusDetail.IBSStatusId != ibsOrderInfo.Status)  // ibs status changed
                    || (!orderStatusDetail.FareAvailable && ibsOrderInfo.Fare > 0);                          // fare was not available and ibs now has the information
        }

        private string GetDescription(Guid orderId, IBSOrderInformation ibsOrderInfo)
        {
            var orderDetail = _orderDao.FindById(orderId);
            var languageCode = orderDetail != null ? orderDetail.ClientLanguageCode : "en";

            string description = null;
            if (ibsOrderInfo.IsAssigned)
            {
                description = string.Format(_resources.Get("OrderStatus_CabDriverNumberAssigned", languageCode), ibsOrderInfo.VehicleNumber);

                if (ibsOrderInfo.Eta.HasValue)
                {
                    description += " - " + string.Format(_resources.Get("OrderStatus_CabDriverETA", languageCode), ibsOrderInfo.Eta.Value.ToString("t"));
                }
            }
            else if (ibsOrderInfo.IsCanceled)
            {
                description = _resources.Get("OrderStatus_" + ibsOrderInfo.Status, languageCode);
            }
            else if (ibsOrderInfo.IsComplete)
            {
                //FormatPrice
                var total =
                    Params.Get(ibsOrderInfo.Toll, ibsOrderInfo.Fare, ibsOrderInfo.Tip, ibsOrderInfo.VAT)
                            .Select(amount => amount)
                            .Sum();

                description = total > 0
                    ? string.Format(_resources.Get("OrderStatus_OrderDoneFareAvailable", languageCode), FormatPrice(total))
                    : _resources.Get("OrderStatus_wosDONE", languageCode);
            }

            return description.HasValue()
                        ? description
                        : _resources.Get("OrderStatus_" + ibsOrderInfo.Status, languageCode);
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}