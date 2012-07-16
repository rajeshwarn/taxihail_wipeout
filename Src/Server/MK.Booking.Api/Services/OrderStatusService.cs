using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;
using System.Globalization;

namespace apcurium.MK.Booking.Api.Services
{
    public class OrderStatusService : RestServiceBase<OrderStatusRequest>
    {
        private const string _assignedStatus = "wosASSIGNED";
        private const string _doneStatus = "wosDONE";

        private IBookingWebServiceClient _bookingWebServiceClient;
        private IOrderDao _orderDao;
        private IAccountDao _accountDao;
        private IConfigurationManager _configManager;

        public OrderStatusService(IOrderDao orderDao, IAccountDao accountDao, IConfigurationManager configManager, IBookingWebServiceClient bookingWebServiceClient)
        {
            _accountDao = accountDao;
            _orderDao = orderDao;
            _bookingWebServiceClient = bookingWebServiceClient;
            _configManager = configManager;

        }



        public override object OnGet(OrderStatusRequest request)
        {

            OrderStatusDetail status = new OrderStatusDetail();
            try
            {
                var order = _orderDao.FindById(request.OrderId);
                var account = _accountDao.FindById(request.AccountId);

                if (!order.IBSOrderId.HasValue)
                {
                    return new OrderStatusDetail { IBSStatusDescription = "Error getting the order status" };
                }

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
                        if ((orderDetails != null) && (orderDetails.CabNumber.HasValue()))
                        {
                            desc = string.Format(_configManager.GetSetting("OrderStatus.CabDriverNumberAssigned"), orderDetails.CabNumber);
                        }
                        else
                        {
                            desc = _configManager.GetSetting("OrderStatus." + status.IBSStatusId);
                        }
                        
                        status.VehicleLatitude = order.PickupLatitude + 0.001;
                        status.VehicleLongitude = order.PickupLongitude + 0.001;

                        //status.VehicleLatitude = statusDetails.VehicleLatitude;
                        //status.VehicleLongitude = statusDetails.VehicleLongitude;
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
                //TO DO: erreur ? Status ?
            }
            return status;
        }

        private string FormatPrice(double? price)
        {

            var culture = _configManager.GetSetting("PriceFormat");
            return string.Format(new CultureInfo(culture), "{0:C}", price.HasValue ? price.Value : 0);
        }

    }
}