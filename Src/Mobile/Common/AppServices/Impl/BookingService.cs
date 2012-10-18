using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Contacts;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using Address = apcurium.MK.Common.Entity.Address;
using ServiceStack.ServiceClient.Web;
using Cirrious.MvvmCross.Interfaces.Platform.Tasks;
using apcurium.MK.Booking.Mobile.Extensions;

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
            order.Note = TinyIoCContainer.Current.Resolve<IAppResource>().GetString( "BookingService_MobileBookingNote");
            if ( order.PickupAddress.BuildingName.HasValue())
            {
                var buildingNote = TinyIoCContainer.Current.Resolve<IAppResource>().GetString( "BookingService_MobileBookingNoteBuildingName");
                order.Note += @"\" + string.Format(buildingNote, order.PickupAddress.BuildingName);
            }

            var orderDetail = new OrderStatusDetail();
            UseServiceClient<OrderServiceClient>(service =>
                {
                    orderDetail = service.CreateOrder(order);
                }, ex => HandleCreateOrderError(ex, order));


            ThreadPool.QueueUserWorkItem(o =>
            {
                TinyIoCContainer.Current.Resolve<IAccountService>().RefreshCache(true);
            });

            return orderDetail;

        }

        private void HandleCreateOrderError(Exception ex, CreateOrder order)
        {
            var appResource = TinyIoCContainer.Current.Resolve<IAppResource>();
            var title = appResource.GetString("ErrorCreatingOrderTitle");


            var message = appResource.GetString("ServiceError_ErrorCreatingOrderMessage"); //= Resources.GetString(Resource.String.ServiceErrorDefaultMessage);


            try
            {
                if (ex is WebServiceException)
                {
                    message = appResource.GetString("ServiceError" + ((WebServiceException) ex).ErrorCode);
                }
            }
            catch
            {

            }


            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            string err = string.Format(message, settings.ApplicationName, settings.PhoneNumberDisplay(order.Settings.ProviderId.HasValue ? order.Settings.ProviderId.Value : 0));

            TinyIoCContainer.Current.Resolve<IMessageService>().ShowMessage(title, err, "Call", () => CallCompany(settings.ApplicationName, settings.PhoneNumber(order.Settings.ProviderId.HasValue ? order.Settings.ProviderId.Value : 0)), "Cancel", RefreshBookingView);
        }

        private void CallCompany(string name, string number)
        {
            var settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            TinyIoCContainer.Current.Resolve<IMvxPhoneCallTask>().MakePhoneCall(name, number);
            RefreshBookingView();
        }
 
        private void RefreshBookingView()
        {

        }

        public OrderStatusDetail GetOrderStatus(Guid orderId)
        {
            OrderStatusDetail r = new OrderStatusDetail();

            UseServiceClient<OrderServiceClient>(service =>
                {
                    r = service.GetOrderStatus(orderId);
                }, ex=> TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex));

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

        public bool IsStatusDone(string statusId)
        {
            return statusId.SoftEqual("wosDONE");
        }

        public string GetFareEstimateDisplay(CreateOrder order, string formatString , string defaultFare)
        {
            var appResource = TinyIoCContainer.Current.Resolve<IAppResource>();
            var fareEstimate = appResource.GetString(defaultFare);

            if (order != null && order.PickupAddress.HasValidCoordinate() && order.DropOffAddress.HasValidCoordinate())
            {
                var directionInfo = TinyIoCContainer.Current.Resolve<IGeolocService>().GetDirectionInfo(order.PickupAddress.Latitude, order.PickupAddress.Longitude, order.DropOffAddress.Latitude, order.DropOffAddress.Longitude);
                if (directionInfo != null)
                {
                    if (directionInfo.Price.HasValue)
                    {
                        if (directionInfo.Price.Value > 100)
                        {
                            fareEstimate = appResource.GetString("EstimatePriceOver100");
                        }
                        else
                        {
                            if (formatString.HasValue())
                            {
                                fareEstimate = String.Format(appResource.GetString(formatString), directionInfo.FormattedPrice);
                            }
                            else
                            {
                                fareEstimate = directionInfo.FormattedPrice;
                            }
                            
                        }

                        if (directionInfo.Distance.HasValue)
                        {
                            fareEstimate += " " + String.Format(appResource.GetString("EstimateDistance"), directionInfo.FormattedDistance);

                        }
                    }
                    else
                    {
                        fareEstimate = String.Format(appResource.GetString("EstimatedFareNotAvailable"));
                    }


                }

            }

            return fareEstimate;
        }
        public bool CancelOrder(Guid orderId)
        {
            bool isCompleted = false;

            UseServiceClient<OrderServiceClient>(service =>
            {
                service.CancelOrder(orderId);
                isCompleted = true;
            });
            return isCompleted;
        }

        public bool SendReceipt(Guid orderId)
        {
            bool isCompleted = false;

            UseServiceClient<OrderServiceClient>(service =>
            {
                service.SendReceipt(orderId);
                isCompleted = true;
            });
            return isCompleted;
        }

        public List<Address> GetAddressFromAddressBook(Predicate<Contact> criteria)
        {
            var contacts = new List<Address>();
			var queryable = _addresBook.Where(c => criteria(c) && c.Addresses.Any() ).ToList();

            foreach (var contact in queryable)
            {
                contact.Addresses.Where( a => a.StreetAddress != null ).ForEach(c => {
					var list = new List<string>();
					c.StreetAddress.Maybe( () => list.Add( c.StreetAddress ) );
					c.City.Maybe( () => list.Add( c.City ) );
					c.Country.Maybe( () => list.Add( c.Country ) );
					contacts.Add(new Address
                                {
                                    FriendlyName = contact.DisplayName,
									FullAddress = string.Join(", ", list ),
                                    City = c.City,
                                    IsHistoric = false,
                                    ZipCode = c.PostalCode,
                                    AddressType = "localContact"
					});
				});

            }
            return contacts;
        }

        private Task _task;

        public Task LoadContacts()
        {
            if (_task == null)
            {
                _task = new Task(() =>
                {
					var book = TinyIoCContainer.Current.Resolve<IAddressBookService>();
					_addresBook = book.LoadContacts();
					_addresBook.ToString();
				});
                _task.Start();
            }
            return _task;
        }
    }
}

