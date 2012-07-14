using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;


namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : BaseService, IBookingService
    {

        public BookingService()
        {
        }




        public bool IsValid(ref CreateOrder info)
        {
            return info.PickupAddress.FullAddress.HasValue() && info.PickupAddress.Latitude != 0 && info.PickupAddress.Longitude != 0;
        }

   
        protected ILogger Logger
        {
            get { return TinyIoCContainer.Current.Resolve<ILogger>(); }
        }

        public OrderStatusDetail CreateOrder(CreateOrder order)
        {
            var account  = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
            order.AccountId = account.Id;
            order.Note = TinyIoCContainer.Current.Resolve<IAppResource>().MobileUser;
            var orderDetail = new OrderStatusDetail();
            UseServiceClient<OrderServiceClient>(service =>
                {
                    orderDetail = service.CreateOrder(order);                   
                });

            return orderDetail;
           
        }

        public OrderStatusDetail GetOrderStatus(Guid orderId)
        {
            OrderStatusDetail r = new OrderStatusDetail();

            UseServiceClient<OrderServiceClient>(service =>
                {
                    var AccountId = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount.Id;
                    r = service.GetOrderStatus(AccountId, orderId);
                });
            
            return r;
        }

        public bool IsCompleted(Guid orderId)
        {
            bool isCompleted = false;
            //UseService(service =>
            //{

            //    var sessionId = service.Authenticate("iphone", "test", 1);
            //    var result = service.GetVehicleLocation(sessionId, user.Email, user.Password, orderId);
            //    if (result.Error == IBS.ErrorCode.NoError)
            //    {

            //        int statusId = result.OrderStatus.SelectOrDefault(o => o.Id, 0);
            //        isCompleted = IsCompleted(statusId);
            //    }


            //});
            return isCompleted;

        }

        public bool IsCompleted(int statusId)
        {
            return (statusId == 0) || (statusId == 7) || (statusId == 18) || (statusId == 22) || (statusId == 11);
        }

        public bool CancelOrder( Guid orderId)
        {
            bool isCompleted = false;
            
                UseServiceClient<OrderServiceClient>(service =>
                {
                    var accountId = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount.Id;
                    service.CancelOrder( accountId , orderId );
                    isCompleted = true;
                });            
            return isCompleted;
        }


    }
}

