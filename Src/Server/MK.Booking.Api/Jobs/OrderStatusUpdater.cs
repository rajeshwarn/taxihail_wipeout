using System.Globalization;
using System.Linq;
using Infrastructure.Messaging;
using ServiceStack.Text;
using apcurium.MK.Booking.Commands;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Entity;
using apcurium.MK.Common.Extensions;
using log4net;

namespace apcurium.MK.Booking.Api.Jobs
{
    public class OrderStatusUpdater
    {
        readonly IConfigurationManager _configurationManager;
        readonly ICommandBus _commandBus;
        static readonly ILog Logger = LogManager.GetLogger(typeof(OrderStatusUpdater));

        public OrderStatusUpdater(IConfigurationManager configurationManager, ICommandBus commandBus)
        {
            _configurationManager = configurationManager;
            _commandBus = commandBus;
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

            var command = new ChangeOrderStatus
                              {
                                  Status = order,
                                  Fare = ibsStatus.Fare,
                                  Toll = ibsStatus.Toll,
                                  Tip = ibsStatus.Tip
                              };

            ibsStatus.Update(order);

            Logger.Debug("Status from IBS Webservice " + ibsStatus.Dump());
            Logger.Debug("Status Changed for " + order.OrderId + " -- new status IBS : " + ibsStatus.Status);
            string description = null;

            if (ibsStatus.IsAssigned)
            {
                description = string.Format(_configurationManager.GetSetting("OrderStatus.CabDriverNumberAssigned"),
                                            ibsStatus.VehicleNumber);

                if (ibsStatus.Eta.HasValue)
                {
                    description += " - " +
                                   string.Format(_configurationManager.GetSetting("OrderStatus.CabDriverETA"),
                                                 ibsStatus.Eta.Value.ToString("t"));
                }
            }
            else if (ibsStatus.IsComplete)
            {
                order.Status = OrderStatus.Completed;

                if (ibsStatus.Fare.HasValue || ibsStatus.Tip.HasValue || ibsStatus.Toll.HasValue)
                {
                    //FormatPrice
                    var total =
                        Params.Get(ibsStatus.Toll, ibsStatus.Fare, ibsStatus.Tip)
                              .Where(amount => amount.HasValue)
                              .Select(amount => amount)
                              .Sum();

                    description = string.Format(_configurationManager.GetSetting("OrderStatus.OrderDoneFareAvailable"), FormatPrice(total));
                    order.FareAvailable = true;
                }
            }

            order.IBSStatusDescription = description.HasValue()
                                             ? description
                                             : _configurationManager.GetSetting("OrderStatus." + ibsStatus.Status);

            _commandBus.Send(command);
        }

        private string FormatPrice(double? price)
        {
            var culture = _configurationManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }
    }
}