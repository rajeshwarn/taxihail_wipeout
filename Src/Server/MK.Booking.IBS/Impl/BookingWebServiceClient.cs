﻿#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using apcurium.MK.Common;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Common.Extensions;
using AutoMapper;
using ServiceStack.Text;

#endregion

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {
        private const int InvalidZoneErrorCode = -1002;
        private readonly IStaticDataWebServiceClient _staticDataWebServiceClient;

        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger,
            IStaticDataWebServiceClient staticDataWebServiceClient)
            : base(configManager, logger)
        {
            _staticDataWebServiceClient = staticDataWebServiceClient;
        }

        public IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude)
        {
            var result = new IbsVehiclePosition[0];

            var optionEnabled = ConfigManager.GetSetting("AvailableVehicles.Enabled", true);

            if (optionEnabled)
            {
                var radius = ConfigManager.GetSetting("AvailableVehicles.Radius", 2000);
                var count = ConfigManager.GetSetting("AvailableVehicles.Count", 10);

                UseService(service =>
                {
                    result = service
                        .GetAvailableVehicles(UserNameApp, PasswordApp, longitude, latitude, radius, count)
                        .Select(Mapper.Map<IbsVehiclePosition>)
                        .ToArray();
                });
            }

            return result;
        }

        public void ConfirmExternalPayment(int orderId, decimal amount, string type, string provider,
            string transactionId, string authorizationCode)
        {
            var result = 0;
            UseService(service =>
            {
                //   string param = string.Format(@"{{""authorizationCode"":""{0}"",""transactionId"":""{1}"",""type"":""{2}"",""provider"":""{3}""}}", transactionId, authorizationCode, type, provider);

                result = service.SaveExtrPayment(UserNameApp, PasswordApp, orderId, Convert.ToDouble(amount),
                    authorizationCode);
            });

            if (result != 1) throw new Exception("SaveExtrPayment failed");
        }

        public IbsOrderStatus GetOrderStatus(int orderId, int accountId, string contactPhone)
        {
            var status = new IbsOrderStatus
            {
                Status = TWEBOrderStatusValue.wosNone.ToString()
            };

            UseService(service =>
            {
                var orderStatus = service.GetOrderStatus(UserNameApp, PasswordApp, orderId, contactPhone, string.Empty,
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
                var order = service.GetBookOrder_7(UserNameApp, PasswordApp, orderId, contactPhone, null, accountId);
                if (order != null)
                {
                    result.VehicleNumber = order.CabNo.ToSafeString().Trim();
// ReSharper disable CompareOfFloatsByEqualityOperator
                    result.Fare = order.Fare == 0 ? (double?) null : order.Fare;
                    result.Toll = order.Tolls == 0 ? (double?) null : order.Tolls;
                    result.Tip = order.Tips == 0 ? (double?) null : order.Tips; //TODO à enlever
                    result.VAT = order.VAT == 0 ? (double?) null : order.VAT;
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
                        Latitude = (double) pickupLat,
                        Longitude = (double) pickupLng
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
                foreach (var orderInfoFromIBS in status)
                {
                    var statusInfos = new IBSOrderInformation(orderInfoFromIBS);

                    result.Add(statusInfos);
                }
            });

            return result;
        }


        public bool SendMessageToDriver(string message, string carId)
        {
            var success = false;
            UseService(service =>
            {
                var resultat = service.SendDriverMsg(UserNameApp, PasswordApp, carId, message);
                success = resultat == 1;
            });
            return success;
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
                Phone = phone,
                Fare = (double) fare.AmountExclTax,
                VAT = (double) fare.TaxAmount
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
            order.ContactPhone = phone;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;


            int? orderId = null;

            if (!ValidateZoneAddresses(order))
            {
                return InvalidZoneErrorCode;
            }

            UseService(service =>
            {
                Logger.LogMessage("WebService Creating IBS Order : " +
                                  JsonSerializer.SerializeToString(order, typeof (TBookOrder_7)));
                Logger.LogMessage("WebService Creating IBS Order pickup : " +
                                  JsonSerializer.SerializeToString(order.PickupAddress, typeof (TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Order dest : " +
                                  JsonSerializer.SerializeToString(order.DropoffAddress, typeof (TWEBAddress)));


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
                IbsOrderStatus status;

                //We need to try 5 times because sometime the IBS cancel method doesn't return an error but doesn't cancel the ride... after 5 time, we are giving up.
                do
                {
                    if (count > 0)
                    {
                        Logger.LogMessage("WebService Cancel Order is not working!  Trying again in 500ms  : " + orderId +
                                          " " + accountId);
                        Thread.Sleep(500);
                    }

                    var result = service.CancelBookOrder(UserNameApp, PasswordApp, orderId, contactPhone, null,
                        accountId);
                    count++;
                    isCompleted = result == 0;
                    status = GetOrderStatus(orderId, accountId, contactPhone);
                } while ((status.Status.ToSafeString().Contains("Cancel")) && (count <= 5));
            });
            return isCompleted;
        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }

        private bool ValidateZoneAddresses(TBookOrder_7 order)
        {
            if (
                !ValidateZone(order.PickupAddress, order.ServiceProviderID, "IBS.ValidatePickupZone",
                    "IBS.PickupZoneToExclude"))
            {
                return false;
            }

// ReSharper disable CompareOfFloatsByEqualityOperator
            if ((order.DropoffAddress != null) && (order.DropoffAddress.Latitude != 0) &&
                (order.DropoffAddress.Latitude != 0))
            {
                // ReSharper restore CompareOfFloatsByEqualityOperator
                if (
                    !ValidateZone(order.DropoffAddress, order.ServiceProviderID, "IBS.ValidateDestinationZone",
                        "IBS.DestinationZoneToExclude"))
                {
                    return false;
                }
            }

            return true;
        }

        private bool ValidateZone(TWEBAddress twebAddress, int? providerId, string enableValidationKey,
            string excludedZoneKey)
        {
            var isValidationEnabled = bool.Parse(ConfigManager.GetSetting(enableValidationKey));
            if (isValidationEnabled)
            {
                Logger.LogMessage("Validating Zone " +
                                  JsonSerializer.SerializeToString(twebAddress, typeof(TWEBAddress)));
                var zone = _staticDataWebServiceClient.GetZoneByCoordinate(providerId, twebAddress.Latitude,
                    twebAddress.Longitude);


                Logger.LogMessage("Zone returned : " + zone.ToSafeString());

                if (zone.ToSafeString().Trim().IsNullOrEmpty())
                {
                    return false;
                }

                var excludedZones = ConfigManager.GetSetting(excludedZoneKey).Split(',');
                if (excludedZones.Any() && excludedZones.Any(z => z.SoftEqual(zone)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}