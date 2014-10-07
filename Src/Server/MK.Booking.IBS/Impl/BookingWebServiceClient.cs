#region

using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using ServiceStack.Text;
using System.Text.RegularExpressions;

#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {
        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger)
            : base(configManager, logger)
        {
        }

        public IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude, int? vehicleTypeId)
        {
            var result = new IbsVehiclePosition[0];

            var optionEnabled = ConfigManager.GetSetting("AvailableVehicles.Enabled", true);

            if (optionEnabled)
            {
                var radius = ConfigManager.GetSetting("AvailableVehicles.Radius", 2000);
                var count = ConfigManager.GetSetting("AvailableVehicles.Count", 10);

                var vehicleTypeFilter = vehicleTypeId.HasValue
                                        ? new[] { new TVehicleTypeItem { ID = vehicleTypeId.Value } }
                                        : new TVehicleTypeItem[0];
                UseService(service =>
                {
                    result = service
                        .GetAvailableVehicles_4(UserNameApp, PasswordApp, longitude, latitude, radius, count, false, vehicleTypeFilter)
                        .Select(Mapper.Map<IbsVehiclePosition>)
                        .ToArray();
                });
            }

            return result;
        }

        public IbsOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone)
        {
            var status = new IbsOrderStatus
            {
                Status = TWEBOrderStatusValue.wosNone.ToString()
            };

            UseService(service =>
            {
                var orderStatus = service.GetOrderStatus(UserNameApp, PasswordApp, orderId, CleanPhone(contactPhone), string.Empty,
                    accountId);
                status.Status = orderStatus.ToString();

                double latitude = 0;
                double longitude = 0;
                var result = service.GetVehicleLocation(UserNameApp, PasswordApp, orderId, ref longitude, ref latitude);

                if (result != 0) return;

                status.VehicleLatitude = latitude;
                status.VehicleLongitude = longitude;
            });
            return status;
        }

        public IbsOrderDetails GetOrderDetails(int orderId, int accountId, string contactPhone)
        {
            var result = new IbsOrderDetails();
            UseService(service =>
            {
                var order = service.GetBookOrder_7(UserNameApp, PasswordApp, orderId, CleanPhone(contactPhone), null, accountId);
                if (order != null)
                {
                    result.VehicleNumber = order.CabNo.ToSafeString().Trim();
                    // ReSharper disable CompareOfFloatsByEqualityOperator
                    result.Fare = order.Fare == 0 ? (double?)null : order.Fare;
                    result.Toll = order.Tolls == 0 ? (double?)null : order.Tolls;
                    result.Tip = order.Tips == 0 ? (double?)null : order.Tips; //TODO à enlever
                    result.VAT = order.VAT == 0 ? (double?)null : order.VAT;
                    // ReSharper restore CompareOfFloatsByEqualityOperator
                    result.CallNumber = order.CallNumber;
                }
            });
            return result;
        }

        public IbsFareEstimate GetFareEstimate(double? pickupLat, double? pickupLng, double? dropoffLat,
            double? dropoffLng)
        {
            var result = new IbsFareEstimate();
            UseService(service =>
            {
                var tbook = new TBookOrder_7();
                if (pickupLat != null
                    && pickupLng != null)
                {
                    tbook.PickupAddress = new TWEBAddress
                    {
                        Latitude = (double)pickupLat,
                        Longitude = (double)pickupLng
                    };
                }
                if (dropoffLat != null
                    && dropoffLng != null)
                {
                    tbook.DropoffAddress = new TWEBAddress
                    {
                        Latitude = (double)dropoffLat,
                        Longitude = (double)dropoffLng
                    };
                }
                double fare;
                double tolls;
                double distance;
                result.FareEstimate = service.EstimateFare(UserNameApp, PasswordApp, tbook, out fare, out tolls,
                    out distance);
                result.Distance = distance;
                result.Tolls = tolls;
            });

            return result;
        }

        public IEnumerable<IBSOrderInformation> GetOrdersStatus(IList<int> ibsOrdersIds)
        {
            var result = new List<IBSOrderInformation>();
            UseService(service =>
            {
                var status = service.GetOrdersStatus_2(UserNameApp, PasswordApp, ibsOrdersIds.ToArray());
                foreach (var orderInfoFromIbs in status)
                {
                    var statusInfos = new IBSOrderInformation(orderInfoFromIbs);

                    result.Add(statusInfos);
                }
            });

            return result;
        }

        public bool SendMessageToDriver(string message, string vehicleNumber)
        {
            var success = false;
            UseService(service =>
            {
                var result = service.SendDriverMsg(UserNameApp, PasswordApp, vehicleNumber, message);
                success = result == 0;

            });
            return success;
        }

        public bool ConfirmExternalPayment(Guid orderID, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
           string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent)
        {
            var success = false;

            UseService(service =>
               {
                   int result = 0;
                   result = service.SaveExtrPayment_2(UserNameApp, PasswordApp, ibsOrderId, transactionId, authorizationCode, cardToken, type, provider, 0, 0, 0, 0,
                    ToCents(tipAmount), ToCents(meterAmount), ToCents(totalAmount), accountID, name, CleanPhone(phone), email, os, userAgent, orderID.ToString());
                   success = result == 0;
                   
                   //*********************************Keep this code.  MK is testing this method as soon as it's ready, 
                   //var auth = new TPaymentAuthorization3dParty
                   //{
                   //    ApprovalText = text,
                   //    Approved = true,
                   //    ApprovedAmount = string.Format("{0:C}", amount),
                   //    AuthorizationNumber = authorizationCode,
                   //    TransactionTime = DateTime.Now.ToString("hh:mm:ss tt", CultureInfo.InvariantCulture),
                   //    TransactionDate = DateTime.Now.ToString("dd/MM/yy", CultureInfo.InvariantCulture),
                   //    CCSequenceNumber = transactionId,
                   //    CardNumber = cardNumber,
                   //    CardType = cardType,
                   //    FareAmount = string.Format("{0:C}", fareAmount),
                   //    DiscountAmount = string.Format("{0:C}", 0),
                   //    ExpiryDate = cardExpiry,
                   //    JobNumber = orderId,
                   //    PayType = 3,

                   //};

                   //var result = service.SendMsg_3dPartyPaymentAuth(UserNameApp, PasswordApp, vehicleId, auth);
                   //success = result == 0;


               });

            return success;
        }

        public int? SendAccountInformation(Guid orderId, int ibsOrderId, string type, string cardToken, int accountID, string name, string phone, string email)
        {
            int? result = null;
            UseService(service =>
            {
                
                result = service.SaveExtrPayment_2(UserNameApp, PasswordApp, ibsOrderId, "", "", cardToken, type,null , 0, 0, 0, 0,
                 0, 0, 0, accountID, name, CleanPhone(phone), email, "", "", orderId.ToString());
                
                if ( result < -9000 ) //Hack unitl we support more code and we get the list of code.
                {
                    service.CancelBookOrder( UserNameApp, PasswordApp, ibsOrderId, CleanPhone(phone ), null , accountID );
                    result = -10000;
                }
                else
                {
                    result = null;
                }



            });

            return result;
        }

        

        private int ToCents(decimal dollarAmout)
        {
            return Convert.ToInt32(dollarAmout * 100);
        }

        private string CleanPhone(string phone)
        {
            var regEx = new Regex(@"\D");
            return regEx.Replace(phone, "");
        }
        public int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers,
            int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup,
            IbsAddress dropoff, Fare fare = default(Fare))
        {
            Logger.LogMessage("WebService Create Order call : accountID=" + accountId);

            
            var order = new TBookOrder_7
            {
                ServiceProviderID = providerId.GetValueOrDefault(),
                AccountID = accountId,
                Customer = passengerName,
                Phone = CleanPhone( phone ),
                Fare = (double)fare.AmountExclTax,
                VAT = (double)fare.TaxAmount
            };

            var autoDispatch =
                ConfigManager.GetSetting("IBS.AutoDispatch").SelectOrDefault(bool.Parse, true);
            order.DispByAuto = autoDispatch;

            var priority = ConfigManager.GetSetting("IBS.OrderPriority")
                .SelectOrDefault(bool.Parse, true);
            order.Priority = priority ? 1 : 0;

            order.PickupDate = new TWEBTimeStamp
            {
                Year = pickupDateTime.Year,
                Month = pickupDateTime.Month,
                Day = pickupDateTime.Day
            };
            order.PickupTime = new TWEBTimeStamp
            {
                Hour = pickupDateTime.Hour,
                Minute = pickupDateTime.Minute,
                Second = 0,
                Fractions = 0
            };

            order.ChargeTypeID = chargeTypeId ?? 0;
            var aptRing = Params.Get(pickup.Apartment, pickup.RingCode).Where(s => s.HasValue()).JoinBy(" / ");

            order.PickupAddress = new TWEBAddress
            {
                StreetPlace = pickup.FullAddress,
                AptBaz = aptRing,
                Longitude = pickup.Longitude,
                Latitude = pickup.Latitude,
                Postal = pickup.ZipCode
            };
            order.DropoffAddress = dropoff == null
                ? new TWEBAddress()
                : new TWEBAddress
                {
                    StreetPlace = dropoff.FullAddress,
                    AptBaz = dropoff.Apartment,
                    Longitude = dropoff.Longitude,
                    Latitude = dropoff.Latitude,
                    Postal = dropoff.ZipCode
                };
            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId ?? 0;
            order.Note = note;
            order.ContactPhone = CleanPhone( phone );
            order.OrderStatus = TWEBOrderStatusValue.wosPost;


            int? orderId = null;

            UseService(service =>
            {
                Logger.LogMessage("WebService Creating IBS Order : " +
                                  JsonSerializer.SerializeToString(order, typeof(TBookOrder_7)));
                Logger.LogMessage("WebService Creating IBS Order pickup : " +
                                  JsonSerializer.SerializeToString(order.PickupAddress, typeof(TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Order dest : " +
                                  JsonSerializer.SerializeToString(order.DropoffAddress, typeof(TWEBAddress)));


                orderId = service.SaveBookOrder_7(UserNameApp, PasswordApp, order);
                Logger.LogMessage("WebService Create Order, orderid receveid : " + orderId);
            });
            return orderId;
        }

        public bool CancelOrder(int orderId, int accountId, string contactPhone)
        {
            Logger.LogMessage("WebService Cancel Order call : " + orderId + " " + accountId);
            var isCompleted = false;
            UseService(service =>
            {
                var count = 0;
                var result = service.CancelBookOrder(UserNameApp, PasswordApp, orderId, CleanPhone(contactPhone), null, accountId);
                var status = GetOrderStatus(orderId, accountId, CleanPhone(contactPhone));
                isCompleted = (result == 0) && (VehicleStatuses.CompletedStatuses.Contains(status.Status));
            });
            return isCompleted;
        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }
    }
}