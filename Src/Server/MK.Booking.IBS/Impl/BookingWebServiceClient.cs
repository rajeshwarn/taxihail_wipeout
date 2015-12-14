#region

using System;
using System.Collections.Generic;
using System.Globalization;
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
        private readonly IServerSettings _serverSettings;
        private readonly IIBSServiceProvider _ibsServiceProvider;

        public BookingWebServiceClient(IServerSettings serverSettings, ILogger logger)
            : base(serverSettings.ServerData.IBS, logger)
        {
            _serverSettings = serverSettings;
        }

        public BookingWebServiceClient(IServerSettings serverSettings, IBSSettingContainer ibsSettings, ILogger logger)
            : base(ibsSettings, logger)
        {
            // for now, server settings is for the home server, so if one day we want a real roaming mode (not network),
            // this will need to be changed

            _serverSettings = serverSettings;
        }

        public IbsVehiclePosition[] GetAvailableVehicles(double latitude, double longitude, int? vehicleTypeId)
        {
            var result = new IbsVehiclePosition[0];

            var optionEnabled = _serverSettings.ServerData.AvailableVehicles.Enabled;

            if (optionEnabled)
            {
                var radius = _serverSettings.ServerData.AvailableVehicles.Radius;
                var count = _serverSettings.ServerData.AvailableVehicles.Count;

                var vehicleTypeFilter = vehicleTypeId.HasValue
                                        ? new[] { new TVehicleTypeItem { ID = vehicleTypeId.Value } }
                                        : new TVehicleTypeItem[0];
                
                UseService(service =>
                {
                    result = service
                        .GetAvailableVehicles_4(UserNameApp, PasswordApp, longitude, latitude, radius, count, false,
                            vehicleTypeFilter)
                        .Select(Mapper.Map<IbsVehiclePosition>)
                        .ToArray();
                });
            }

            return result
                .Where(r => r != null)
                .GroupBy(r => r .VehicleNumber)
                .Select(g => g.First())
                .ToArray();
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

        public IbsFareEstimate GetFareEstimate( double? pickupLat, double? pickupLng, double? dropoffLat, double? dropoffLng, string pickupZipCode, string dropoffZipCode, string accountNumber, 
            int? customerNumber, int? tripDurationInSeconds, int? providerId, int? vehicleType, int defaultVehiculeTypeId)
        {
            var result = new IbsFareEstimate();
            UseService(service =>
            {
                var tbook = new TBookOrder_8();
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
                        Longitude = (double)dropoffLng,
                    };
                }

                tbook.AccountNum = accountNumber;
                tbook.VehicleTypeID = vehicleType ?? defaultVehiculeTypeId;
                tbook.ChargeTypeID = -1;

                tbook.ServiceProviderID = providerId ?? -1;
                tbook.PickupDate = new TWEBTimeStamp { Year = DateTime.Now.Year, Month = DateTime.Now.Month, Day = DateTime.Now.Day };
                tbook.PickupTime = new TWEBTimeStamp { Hour = DateTime.Now.Hour, Minute = DateTime.Now.Minute };
            

                if (!string.IsNullOrEmpty(pickupZipCode))
                {
                    tbook.PickupAddress.Postal = pickupZipCode;
                }

                if (!string.IsNullOrEmpty(dropoffZipCode))
                {
                    tbook.DropoffAddress.Postal = dropoffZipCode;
                }

                if ( accountNumber.HasValue()  && customerNumber.HasValue) 
                {
                    tbook.CustomerNum = customerNumber.Value;
                }
                else
                {
                    tbook.CustomerNum = -1;
                }

                if (tripDurationInSeconds.HasValue && tripDurationInSeconds > 0)
                {
                    tbook.WaitTime = tripDurationInSeconds.Value;
                }

                double fare;
                double tolls;
                double distance;
                double tripTime;

                result.FareEstimate = service.EstimateFare_8(UserNameApp, PasswordApp, tbook, out fare, out tolls, out distance, out tripTime);
                if (result.FareEstimate == 0)
                {
                    result.FareEstimate = fare;
                }
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
                var status = GetOrdersStatus(ibsOrdersIds, service)
                    .Select(orderInfoFromIbs => new IBSOrderInformation(orderInfoFromIbs));

                result.AddRange(status);
            });

            return result;
        }

		// This method is used to help find the correct GetOrdersStatus in IBS.
        private IEnumerable<TOrderStatus_5> GetOrdersStatus(IEnumerable<int> ibsOrdersIds, WebOrder7Service service)
        {
            var ibsOrders = ibsOrdersIds.ToArray();

            try
            {
                return service.GetOrdersStatus_5(UserNameApp, PasswordApp, ibsOrders);
            }
            catch (Exception)
            {
                Logger.LogMessage("GetOrdersStatus_5 is not available doing a fallback to GetOrdersStatus_4");
            }

            try
            {
                return service.GetOrdersStatus_4(UserNameApp, PasswordApp, ibsOrders)
                    // We need to update the returned object to version 5 of TOrderStatus.
                    .Select(OrderStatus4ToOrderStatus5);
            }
            catch (Exception)
            {
                Logger.LogMessage("GetOrdersStatus_4 is not available doing a fallback to GetOrdersStatus_3");
            }

            try
            {
                return service.GetOrdersStatus_3(UserNameApp, PasswordApp, ibsOrders)
                    // We need to update the returned object to version 5 of TOrderStatus.
                    .Select(OrderStatus3ToOrderStatus5);
            }
            catch (Exception)
            {
                Logger.LogMessage("GetOrdersStatus_3 is not available doing a fallback to GetOrdersStatus_2");
            }  
             
            return service.GetOrdersStatus_2(UserNameApp, PasswordApp, ibsOrders)
				// We need to update the returned object to version 5 of TOrderStatus.
				.Select(OrderStatus2ToOrderStatus5);

        }

        private static TOrderStatus_5 OrderStatus2ToOrderStatus5(TOrderStatus_2 orderStatus)
        {
            return new TOrderStatus_5
            {
                OrderStatus = orderStatus.OrderStatus,
                CallNumber = orderStatus.CallNumber,
                DriverFirstName = orderStatus.DriverFirstName,
                DriverLastName = orderStatus.DriverLastName,
                DriverMobilePhone = orderStatus.DriverMobilePhone,
                ETATime = orderStatus.ETATime,
                Fare = orderStatus.Fare,
                OrderID = orderStatus.OrderID,
                ReferenceNumber = orderStatus.ReferenceNumber,
                TerminalId = orderStatus.TerminalId,
                Tips = orderStatus.Tips,
                Tolls = orderStatus.Tolls,
                VAT = orderStatus.VAT,
                VehicleColor = orderStatus.VehicleColor,
                VehicleCoordinateLat = orderStatus.VehicleCoordinateLat,
                VehicleCoordinateLong = orderStatus.VehicleCoordinateLong,
                VehicleMake = orderStatus.VehicleMake,
                VehicleModel = orderStatus.VehicleModel,
                VehicleNumber = orderStatus.VehicleNumber,
                VehicleRegistration = orderStatus.VehicleRegistration
            };
        }

        private static TOrderStatus_5 OrderStatus3ToOrderStatus5(TOrderStatus_3 orderStatus)
        {
            var orderStatus5 = OrderStatus2ToOrderStatus5(orderStatus);
            orderStatus5.PairingCode = orderStatus.PairingCode;
            orderStatus5.Surcharge = orderStatus.Surcharge;

            return orderStatus5;
        }

        private static TOrderStatus_5 OrderStatus4ToOrderStatus5(TOrderStatus_4 orderStatus)
        {
            var orderStatus5 = OrderStatus3ToOrderStatus5(orderStatus);
            orderStatus5.DriverNumber = orderStatus.DriverNumber;
            orderStatus5.OriginalImg = orderStatus.OriginalImg;
            orderStatus5.ThumbnailImg = orderStatus.ThumbnailImg;
            orderStatus5.WebImg = orderStatus.WebImg;

            return orderStatus5;
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

        /// <summary>
        /// Informs IBS and terminal in vehicle that the user will pay using the app.  
        /// This should disable the terminal in the vehicle.
        /// </summary>
        /// <param name="ibsAccountId"></param>
        /// <param name="ibsOrderId"></param>
        /// <param name="chargeTypeId"></param>
        /// <returns></returns>
        public bool UpdateOrderPaymentType(int ibsAccountId, int ibsOrderId, int? chargeTypeId)
        {
            if (!chargeTypeId.HasValue)
            {
                Logger.LogMessage(
                    "WebService UpdateOrderPaymentType : No Ibs ChargeType Id set for CardOnFile, skipping call to UpdateOrderPaymentType");
                return true;
            }

            Logger.LogMessage("WebService UpdateOrderPaymentType : ibsAccountId={0},ibsOrderId={1},chargeTypeId={2}", 
                ibsAccountId, ibsOrderId, chargeTypeId);

            var success = false;
            UseService(service =>
            {
                var result = service.UpdateJobPaymentType(UserNameApp, PasswordApp, ibsAccountId, ibsOrderId, chargeTypeId.Value);
                success = result == 1;
            });
            return success;
        }
        
        public bool ConfirmExternalPayment(Guid orderID, int ibsOrderId, decimal totalAmount, decimal tipAmount, decimal meterAmount, string type, string provider, string transactionId,
           string authorizationCode, string cardToken, int accountID, string name, string phone, string email, string os, string userAgent, decimal fareAmount = 0, decimal extrasAmount = 0, 
           decimal vatAmount = 0, decimal discountAmount = 0, decimal tollAmount = 0, decimal surchargeAmount = 0)
        {
            var success = false;

            try
            {
                UseService(service =>
                {
                    int result = 0;
                    int collect = 0;
                    int balance = 0;
                    result = service.SaveExtrPayment_3(UserNameApp, PasswordApp, ibsOrderId, transactionId,
                        authorizationCode, cardToken, type, provider, ToCents(tipAmount), ToCents(tollAmount),
                        ToCents(fareAmount), ToCents(extrasAmount),
                        ToCents(tipAmount), ToCents(meterAmount), ToCents(totalAmount), accountID, name,
                        CleanPhone(phone),
                        email, os, userAgent, orderID.ToString(), ToCents(vatAmount), ToCents(surchargeAmount),
                        ToCents(discountAmount), collect, balance);

                    success = result == 0;

                    SendMsg3dPartyPaymentAuth();
                });

                return success;
            }
            catch (Exception)
            {
                Logger.LogMessage("SaveExtrPayment_3 is not available doing a fallback to SaveExtrPayment_2");
            }

            UseService(service =>
            {
                int result = 0;

                result = service.SaveExtrPayment_2(UserNameApp, PasswordApp, ibsOrderId, transactionId, authorizationCode, cardToken, type, provider, 0, 0, 0, 0,
                    ToCents(tipAmount), ToCents(meterAmount), ToCents(totalAmount), accountID, name, CleanPhone(phone), email, os, userAgent, orderID.ToString());

                success = result == 0;

                SendMsg3dPartyPaymentAuth();
            });

            return success;
        }

        private bool SendMsg3dPartyPaymentAuth()
        {
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
            return false;
        }

        public int? SendAccountInformation(Guid orderId, int ibsOrderId, string type, string cardToken, int accountId, string name, string phone, string email)
        {
            int? result = null;
            try
            {
                UseService(service =>
                {
                    int collect = 0;
                    int balance = 0;
                    result = service.SaveExtrPayment_3(UserNameApp, PasswordApp, ibsOrderId, "", "", cardToken, type,
                        null, 0, 0, 0, 0,
                        0, 0, 0, accountId, name, CleanPhone(phone), email, "", "", orderId.ToString(), 0, 0, 0,
                        collect, balance);

                    if (result < -9000) //Hack until we support more code and we get the list of code.
                    {
                        service.CancelBookOrder(UserNameApp, PasswordApp, ibsOrderId, CleanPhone(phone), null, accountId);
                        result = -10000;
                    }
                    else
                    {
                        result = null;
                    }
                });
            }
            catch (Exception)
            {
                Logger.LogMessage("SaveExtrPayment_3 is not available doing a fallback to SaveExtrPayment_2");
            }

            UseService(service =>
            {
                result = service.SaveExtrPayment_2(UserNameApp, PasswordApp, ibsOrderId, "", "", cardToken, type, null, 0, 0, 0, 0, 
                    0, 0, 0, accountId, name, CleanPhone(phone), email, "", "", orderId.ToString());

                if (result < -9000) //Hack until we support more code and we get the list of code.
                {
                    service.CancelBookOrder(UserNameApp, PasswordApp, ibsOrderId, CleanPhone(phone), null, accountId);
                    result = -10000;
                }
                else
                {
                    result = null;
                }
            });

            return result;
        }

        public int? CreateOrder(int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff, string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId, double? tipIncentive, Fare fare = default(Fare))
        {
            var order = CreateIbsOrderObject(providerId, accountId, passengerName, phone, nbPassengers, vehicleTypeId,
                chargeTypeId, note, pickupDateTime, pickup, dropoff, accountNumber, customerNumber, prompts,
                promptsLength, defaultVehiculeTypeId, tipIncentive, fare);

            int? orderId = null;

            UseService(service =>
            {
                Logger.LogMessage("WebService Creating IBS Order : " +
                                  JsonSerializer.SerializeToString(order, typeof(TBookOrder_8)));
                Logger.LogMessage("WebService Creating IBS Order pickup : " +
                                  JsonSerializer.SerializeToString(order.PickupAddress, typeof(TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Order dest : " +
                                  JsonSerializer.SerializeToString(order.DropoffAddress, typeof(TWEBAddress)));

                orderId = service.SaveBookOrder_10(UserNameApp, PasswordApp, order);
                Logger.LogMessage("WebService Create Order, orderid receveid : " + orderId);
            });
            return orderId;
        }

        public IbsHailResponse Hail(Guid orderId, int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff, string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId, IEnumerable<IbsVehicleCandidate> vehicleCandidates, double? tipIncentive, Fare fare = default(Fare))
        {
            var order = CreateIbsOrderObject(providerId, accountId, passengerName, phone, nbPassengers, vehicleTypeId,
                chargeTypeId, note, pickupDateTime, pickup, dropoff, accountNumber, customerNumber, prompts,
                promptsLength, defaultVehiculeTypeId, tipIncentive, fare, orderId);

            var orderKey = new TBookOrderKey();
            var vehicleComps = Mapper.Map<TVehicleComp[]>(vehicleCandidates);

            UseService(service =>
            {
                Logger.LogMessage("WebService Creating IBS Hail : " +
                                  JsonSerializer.SerializeToString(order, typeof(TBookOrder_11)));
                Logger.LogMessage("WebService Creating IBS Hail pickup : " +
                                  JsonSerializer.SerializeToString(order.PickupAddress, typeof(TWEBAddress)));
                Logger.LogMessage("WebService Creating IBS Hail dest : " +
                                  JsonSerializer.SerializeToString(order.DropoffAddress, typeof(TWEBAddress)));

                orderKey = service.SaveBookOrder_12(UserNameApp, PasswordApp, order, vehicleComps);
                Logger.LogMessage("WebService Create Hail, orderid received : " + orderKey.OrderID + ", orderGUID received : " + orderKey.GUID);
            });

            return new IbsHailResponse
            {
                OrderKey = new IbsOrderKey
                {
                    TaxiHailOrderId = orderKey.GUID.HasValue() ? Guid.Parse(orderKey.GUID) : Guid.Empty,
                    IbsOrderId = orderKey.OrderID
                }
            };
        }

        public IbsVehicleCandidate[] GetVehicleCandidates(IbsOrderKey orderKey)
        {
            var vehicleCandidates = new TVehicleComp[0];

            UseService(service =>
            {
                Logger.LogMessage("WebService Getting Vehicle Candidates : " + JsonSerializer.SerializeToString(orderKey, typeof(IbsOrderKey)));

                vehicleCandidates = service.GetVehicleCandidates(UserNameApp, PasswordApp, new TBookOrderKey { GUID = orderKey.ToString(), OrderID = orderKey.IbsOrderId });
                Logger.LogMessage("WebService Getting Vehicle Candidates, candidates received : " + JsonSerializer.SerializeToString(vehicleCandidates, typeof(TVehicleComp)));            
            });

            return Mapper.Map<IbsVehicleCandidate[]>(vehicleCandidates);
        }

        public int? ConfirmHail(IbsOrderKey orderKey, IbsVehicleCandidate selectedVehicle)
        {
            var ibsOrderKey = Mapper.Map<TBookOrderKey>(orderKey);
            var ibsVehicleCandidate = Mapper.Map<TVehicleComp>(selectedVehicle);

            int? result = null;

            UseService(service =>
            {
                Logger.LogMessage("WebService Confirming Hail for orderkey: " + JsonSerializer.SerializeToString(ibsOrderKey, typeof(TBookOrderKey)));
                Logger.LogMessage("WebService Confirming Hail for vehicle: " + JsonSerializer.SerializeToString(ibsVehicleCandidate, typeof(TVehicleComp)));

                result = service.UpdateJobToVehicle(UserNameApp, PasswordApp, ibsOrderKey, ibsVehicleCandidate);
                Logger.LogMessage("WebService Confirm Hail, orderid : " + ibsOrderKey.OrderID + ", orderGUID : " + ibsOrderKey.GUID);
            });

            return result;
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

        public bool InitiateCallToDriver(int ibsOrderId, string vehicleNumber)
        {
            var success = false;
            UseService(service =>
            {
                var result = service.SendP2DCall(UserNameApp, PasswordApp, vehicleNumber, ibsOrderId);
                success = result == 0;
            });
            return success;
        }

        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }

        private TBookOrder_12 CreateIbsOrderObject(int? providerId, int accountId, string passengerName, string phone, int nbPassengers, int? vehicleTypeId, int? chargeTypeId, string note, DateTime pickupDateTime, IbsAddress pickup, IbsAddress dropoff, string accountNumber, int? customerNumber, string[] prompts, int?[] promptsLength, int defaultVehiculeTypeId, double? tipIncentive, Fare fare = default(Fare), Guid? taxiHailOrderId = null)
        {
            Logger.LogMessage("WebService Create Order call : accountID=" + accountId);

            var order = new TBookOrder_12
            {
                ServiceProviderID = providerId.GetValueOrDefault(),
                AccountID = accountId,
                Customer = passengerName,
                Phone = CleanPhone(phone),
                AccountNum = accountNumber
            };

            if (taxiHailOrderId.HasValue)
            {
                order.GUID = taxiHailOrderId.ToString();
            }

            if (!_serverSettings.ServerData.HideFareEstimateFromIBS)
            {
                order.Fare = Convert.ToDouble(fare.AmountExclTax);
                order.VAT = Convert.ToDouble(fare.TaxAmount);
            }

            order.DispByAuto = _ibsSettings.AutoDispatch;
            order.Priority = _ibsSettings.OrderPriority ? 1 : 0;

            order.PickupDate = new TWEBTimeStamp
            {
                Year = pickupDateTime.Year, Month = pickupDateTime.Month, Day = pickupDateTime.Day
            };
            order.PickupTime = new TWEBTimeStamp
            {
                Hour = pickupDateTime.Hour, Minute = pickupDateTime.Minute, Second = 0, Fractions = 0
            };

            order.AccountNum = accountNumber;

            if (accountNumber.HasValue() && customerNumber.HasValue)
            {
                order.CustomerNum = customerNumber.Value;
            }
            else
            {
                order.CustomerNum = -1;
            }

            order.ChargeTypeID = chargeTypeId ?? -1;
            var aptRing = Params.Get(pickup.Apartment, pickup.RingCode).Where(s => s.HasValue()).JoinBy(" / ");

            order.PickupAddress = new TWEBAddress
            {
                StreetPlace = pickup.FullAddress, AptBaz = aptRing, Longitude = pickup.Longitude, Latitude = pickup.Latitude, Postal = pickup.ZipCode
            };
            order.DropoffAddress = dropoff == null ? new TWEBAddress() : new TWEBAddress
            {
                StreetPlace = dropoff.FullAddress, AptBaz = dropoff.Apartment, Longitude = dropoff.Longitude, Latitude = dropoff.Latitude, Postal = dropoff.ZipCode
            };

            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId ?? defaultVehiculeTypeId;
            order.Note = note;
            order.ContactPhone = CleanPhone(phone);
            order.OrderStatus = TWEBOrderStatusValue.wosPost;
            order.JobOfferPrompt = _serverSettings.ServerData.MessagePromptedToDriver;

            var currentCultureInfo = CultureInfo.GetCultureInfo(_serverSettings.ServerData.PriceFormat);

            if (tipIncentive.HasValue && tipIncentive > 0)
            {
                order.JobOfferPrompt = string.Format(currentCultureInfo, "{0} + {1:C} Bonus", order.JobOfferPrompt, tipIncentive);
            }

            SetPrompts(order, prompts, promptsLength);

            return order;
        }

        private int ToCents(decimal dollarAmout)
        {
            return Convert.ToInt32(dollarAmout*100);
        }

        private string CleanPhone(string phone)
        {
            var regEx = new Regex(@"\D");
            return regEx.Replace(phone, "");
        }

        private void SetPrompts(TBookOrder_8 order, string[] prompts, int?[] promptsLength)
        {
            if (prompts != null)
            {
                if (prompts.Length >= 1)
                {
                    order.Prompt1 = prompts[0];
                }
                if (prompts.Length >= 2)
                {
                    order.Prompt2 = prompts[1];
                }
                if (prompts.Length >= 3)
                {
                    order.Prompt3 = prompts[2];
                }
                if (prompts.Length >= 4)
                {
                    order.Prompt4 = prompts[3];
                }
                if (prompts.Length >= 5)
                {
                    order.Prompt5 = prompts[4];
                }
                if (prompts.Length >= 6)
                {
                    order.Prompt6 = prompts[5];
                }
                if (prompts.Length >= 7)
                {
                    order.Prompt7 = prompts[6];
                }
                if (prompts.Length >= 8)
                {
                    order.Prompt8 = prompts[7];
                }
            }
            if (promptsLength != null)
            {
                if (promptsLength.Length >= 1)
                {
                    order.Field1 = ConvertToString(promptsLength[0]);
                }
                if (promptsLength.Length >= 2)
                {
                    order.Field2 = ConvertToString(promptsLength[1]);
                }
                if (promptsLength.Length >= 3)
                {
                    order.Field3 = ConvertToString(promptsLength[2]);
                }
                if (promptsLength.Length >= 4)
                {
                    order.Field4 = ConvertToString(promptsLength[3]);
                }
                if (promptsLength.Length >= 5)
                {
                    order.Field5 = ConvertToString(promptsLength[4]);
                }
                if (promptsLength.Length >= 6)
                {
                    order.Field6 = ConvertToString(promptsLength[5]);
                }
                if (promptsLength.Length >= 7)
                {
                    order.Field7 = ConvertToString(promptsLength[6]);
                }
                if (promptsLength.Length >= 8)
                {
                    order.Field8 = ConvertToString(promptsLength[7]);
                }
            }
        }

        private string ConvertToString(int? v)
        {
            if (v.HasValue)
            {
                return v.Value.ToString();
            }
            return null;
        }
    }
}