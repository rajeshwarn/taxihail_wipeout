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


namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : IBookingService
    {

        public BookingService()
        {
        }

        
        

        public bool IsValid(ref BookingInfoData info)
        {
            return info.PickupLocation.FullAddress.HasValue() && info.PickupLocation.Latitude != 0 && info.PickupLocation.Longitude != 0;
        }

   
        protected ILogger Logger
        {
            get { return TinyIoCContainer.Current.Resolve<ILogger>(); }
        }

        public int CreateOrder(Account user, BookingInfoData info, out string error)
        {
            error = "";
            string errorResult = "";
            int r = 0;
            //UseService(service =>
            //{
				

				
            //    for (int i = 0; i < info.Settings.NumberOfTaxi; i++)
            //    {
					
					
					
            //    var sessionId = service.Authenticate("iphone", "test", 1);

            //    new OrderMapping().ToWSOrder(info);
            //    var order = new IBS.OrderInfo();
            //    order.ChargeTypeId = info.Settings.ChargeType;
            //    order.CompanyId = info.Settings.Company;
            //    order.ContactPhone = info.Settings.Phone;
            //    order.Name = info.Settings.Name;
            //    order.NumberOfPassenger = info.Settings.Passengers;
            //    order.VehicleTypeId = info.Settings.VehicleType;


            //        order.MobileNote = TinyIoCContainer.Current.Resolve <IAppResource> ().MobileUser;
            //        order.MobileNote += "\n\n" + TinyIoCContainer.Current.Resolve <IAppResource> ().PaiementType + " " + info.Settings.ChargeTypeName;
            //        order.MobileNote += "\n\n" + TinyIoCContainer.Current.Resolve <IAppResource> ().Notes + " " + info.Notes;
            //    if (info.PickupLocation.IsGPSNotAccurate)
            //    {
					
            //            var note = "\n\n" + TinyIoCContainer.Current.Resolve <IAppResource> ().OrderNote;
            //        note += TinyIoCContainer.Current.Resolve<IAppResource>().OrderNoteGPSApproximate;
            //            order.MobileNote += note;
            //    }
					

            //        Console.WriteLine( order.MobileNote );

            //    if (info.PickupLocation.RingCode.HasValue())
            //    {
            //        order.RingCode = info.PickupLocation.RingCode;
            //    }


            //    order.PickupAddress = new AccountMapping().ToWSLocationData(info.PickupLocation);


            //    if (info.PickupDate.HasValue)
            //    {
            //        order.PickupTime = info.PickupDate.Value;
            //    }
            //    else
            //    {
            //        order.PickupTime = DateTime.Now.AddMinutes(5);
            //    }

            //    if (info.DestinationLocation != null)
            //    {
            //        info.DestinationLocation.Address = info.DestinationLocation.Address.SelectOrDefault(a => a, "");
            //        order.DropoffAddress = new AccountMapping().ToWSLocationData(info.DestinationLocation);
            //    }



            //    order.OrderDate = DateTime.Now;



            //    Logger.LogMessage("Create order  :" + user.Email);

            //    var result = service.CreateOrder(sessionId, user.Email, user.Password, order);



            //    if (result.OrderId > 0)
            //    {
            //        r = result.OrderId;
            //        Logger.LogMessage("Order Created :" + result.OrderId.ToString());
            //    }
            //    else
            //    {
            //        errorResult = result.ErrorMessage;
            //        Logger.LogMessage("Error order creation :" + errorResult);
            //    }
            //    }
            //});
            error = errorResult;
            return r;
        }

        public apcurium.MK.Booking.Mobile.Data.OrderStatus GetOrderStatus(Account user, int orderId)
        {
            apcurium.MK.Booking.Mobile.Data.OrderStatus r = null;
            //UseService(service =>
            //{

            //    TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("Begin GetOrderStatus");

            //    var sessionId = service.Authenticate("iphone", "test", 1);
            //    var result = service.GetVehicleLocation(sessionId, user.Email, user.Password, orderId);
            //    if (result.Error == IBS.ErrorCode.NoError)
            //    {
            //        if ((result.OrderStatus == null) || (result.OrderStatus.Description.IsNullOrEmpty()))
            //        {
            //            Logger.LogMessage("Status cannot be found for order #" + orderId.ToString());

            //            var status = new OrderStatus();
            //            status.Latitude = 0;
            //            status.Longitude = 0;
            //            status.Status = TinyIoCContainer.Current.Resolve<IAppResource>().StatusInvalid;
            //            status.Id = result.OrderStatus.SelectOrDefault(o => o.Id, 0);
            //            r = status;
            //        }
            //        else
            //        {
            //            var status = new OrderStatus();
            //            if (result.OrderStatus.Id == 13)
            //            {
            //                status.Latitude = result.Latitude;
            //                status.Longitude = result.Longitude;
            //            }
            //            status.Status = result.OrderStatus.SelectOrDefault(o => o.Description, "");
            //            status.Id = result.OrderStatus.SelectOrDefault(o => o.Id, 0);
            //            r = status;

            //            if (result.NoVehicle.HasValue() && (result.OrderStatus.Id == 13))
            //            {
            //                status.Status += Environment.NewLine + string.Format(TinyIoCContainer.Current.Resolve<IAppResource>().CarAssigned, result.NoVehicle);
            //            }
            //        }



            //    }
            //    else
            //    {
            //        TinyIoCContainer.Current.Resolve<ILogger>().LogMessage(result.ErrorMessage);
            //    }

            //    TinyIoCContainer.Current.Resolve<ILogger>().LogMessage("End GetOrderStatus");

            //});
            return r;
        }

        public bool IsCompleted(Account user, int orderId)
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

        public bool CancelOrder(Account user, int orderId)
        {
            bool isCompleted = false;
            //UseService(service =>
            //{

            //    var sessionId = service.Authenticate("iphone", "test", 1);

            //    var result = service.CancelOrder(sessionId, user.Email, user.Password, new IBS.OrderInfo { Id = orderId });
            //    if (result.Error == IBS.ErrorCode.NoError)
            //    {
            //        isCompleted = true;
            //    }

            //});
            return isCompleted;
        }


    }
}

