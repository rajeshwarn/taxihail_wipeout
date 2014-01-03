#region

using System.Globalization;
using System.Linq;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using Infrastructure.Messaging;

#endregion

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        private readonly ICommandBus _commandBus;
        private readonly IConfigurationManager _configurationManager;
        private readonly IOrderPaymentDao _orderPayementDao;
        private readonly dynamic _resources;

        public OrderStatusUpdater(IConfigurationManager configurationManager, ICommandBus commandBus,
            IOrderPaymentDao orderPayementDao)
        {
            _configurationManager = configurationManager;
            _commandBus = commandBus;
            _orderPayementDao = orderPayementDao;
            var applicationKey = configurationManager.GetSetting("TaxiHail.ApplicationKey");
            _resources = new DynamicResources(applicationKey);
        }

        public void Update(IbsOrderInformation ibsStatus, OrderStatusDetail order)
        {
            var statusChanged = (ibsStatus.Status.HasValue() && order.IbsStatusId != ibsStatus.Status)
                                || order.VehicleLatitude != ibsStatus.VehicleLatitude
                                || order.VehicleLongitude != ibsStatus.VehicleLongitude;

            if (!statusChanged)
            {
                return;
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
                description = string.Format((string) _resources.OrderStatus_CabDriverNumberAssigned,
                    ibsStatus.VehicleNumber);

                if (ibsStatus.Eta.HasValue)
                {
                    description += " - " +
                                   string.Format((string) _resources.OrderStatus_CabDriverETA,
                                       ibsStatus.Eta.Value.ToString("t"));
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
                    description = string.Format((string) _resources.OrderStatus_OrderDoneFareAvailable,
                        FormatPrice(total));
                    order.FareAvailable = true;
                }
                else
                {
                    description = (string) _resources.OrderStatus_wosDONE;
                    order.FareAvailable = false;
                }

                var payment = _orderPayementDao.FindByOrderId(order.OrderId);
                if (payment != null)
                {
                    order.FareAvailable = true;
                }
            }

            order.IbsStatusDescription = description.HasValue()
                ? description
                : (string) _resources.GetString("OrderStatus_" + ibsStatus.Status);

            _commandBus.Send(command);
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}