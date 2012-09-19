using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Contacts;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Address = apcurium.MK.Booking.Api.Contract.Resources.Address;


namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : BaseService, IBookingService
    {
        private List<Contact> _addresBook;

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
            order.Note = TinyIoCContainer.Current.Resolve<IAppResource>().MobileUser;
            var orderDetail = new OrderStatusDetail();
            UseServiceClient<OrderServiceClient>(service =>
                {
                    orderDetail = service.CreateOrder(order);                   
                });

            if ((orderDetail == null) || !orderDetail.IBSOrderId.HasValue || orderDetail.IBSOrderId.Value <= 0)
            {
                var res = TinyIoCContainer.Current.Resolve<IAppResource>();

                TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(res.GetString("ErrorCreatingOrderTitle"), res.GetString("ErrorCreatingOrderMessage"));
                
                return orderDetail;
            }

            ThreadPool.QueueUserWorkItem(o =>
            {
                TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache(true);
            });

            return orderDetail;
           
        }

        public OrderStatusDetail GetOrderStatus(Guid orderId)
        {
            OrderStatusDetail r = new OrderStatusDetail();

            UseServiceClient<OrderServiceClient>(service =>
                {
                    r = service.GetOrderStatus(orderId);
                });
            
            return r;
        }

        public void RemoveFromHistory(Guid orderId)
        {
            UseServiceClient<OrderServiceClient>(service => service.RemoveFromHistory(orderId));
        }

        public bool IsCompleted(Guid orderId)
        {                        
            var status = GetOrderStatus(orderId);
            return IsStatusCompleted(status.IBSStatusId);                
        }

        public bool IsStatusCompleted(string statusId)
        {
            return statusId.IsNullOrEmpty() ||                     
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
                    service.CancelOrder(orderId );
                    isCompleted = true;
                });            
            return isCompleted;
        }

        public List<Address> GetAddressFromAddressBook(Predicate<Contact> criteria)
        {
            var contacts = new List<Address>();
            var queryable = _addresBook.Where(c => criteria(c)).Take(50).ToList();

            foreach (var contact in queryable)
            {
                if (contact.Addresses.Any())
                {
                    contact.Addresses.ForEach(c => contacts.Add(new Address
                                    {
                                        FriendlyName = contact.DisplayName,
                                        FullAddress = c.StreetAddress,
                                        City = c.City,
                                        IsHistoric = false,
                                        ZipCode = c.PostalCode,
                                        AddressType = "localContact"
                                    }));
                }
            }
            return contacts;
        }

        private Task _task;

        public Task LoadContacts()
        {
            if(_task == null)
            {
                _task = new Task(() =>
                {
                    var book = TinyIoCContainer.Current.Resolve<AddressBook>();
                    book.PreferContactAggregation = true;
                    _addresBook = book.ToList();

                });
                _task.Start();
            }
            return _task;
        }
    }
}

