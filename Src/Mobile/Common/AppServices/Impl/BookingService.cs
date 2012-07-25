using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;


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



            ThreadPool.QueueUserWorkItem(o =>
            {
                TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache();
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
            var status = GetOrderStatus(orderId);
            return IsStatusCompleted(status.IBSStatusId);                
        }

        public bool IsStatusCompleted(string statusId)
        {
            return statusId.SoftEqual("wosSCHED") || 
                    statusId.SoftEqual("wosCANCELLED") || 
                    statusId.SoftEqual("wosDONE") || 
                    statusId.SoftEqual("wosNOSHOW") || 
                    statusId.SoftEqual("wosCANCELLED_DONE");
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

