using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Data;
using Microsoft.Practices.ServiceLocation;
using apcurium.MK.Booking.Mobile.Infrastructure;


namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class BookingService : IBookingService
    {

        public BookingService()
        {
        }

        
        

        public bool IsValid(ref BookingInfoData info)
        {
            return info.PickupLocation.Address.HasValue() && info.PickupLocation.Latitude.HasValue && info.PickupLocation.Longitude.HasValue;
        }

        public LocationData[] SearchAddress(double latitude, double longitude)
        {

            string param = "latlng={0},{1}";
            param = string.Format(param, latitude.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat), longitude.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat));
            return SearchAddressUsingGoogle(param);
        }

        public LocationData[] SearchAddress(string address)
        {
            if (address.HasValue())
            {

                LocationData[] result = new LocationData[0];
                string param = "";
                if (!address.Contains(","))
                {
                    param = "address={0},montreal,qc,canada&region=ca";
                    param = string.Format(param, address.Replace(" ", "+"));
                    result = SearchAddressUsingGoogle(param);
                }

                if (result.Count() > 0)
                {
                    return result;
                }
                else
                {

                    param = "address={0},qc,canada&region=ca";
                    param = string.Format(param, address.Replace(" ", "+"));
                    return SearchAddressUsingGoogle(param);
                }
            }
            else
            {
                return new LocationData[0];
            }

        }

        private LocationData[] SearchAddressUsingGoogle(string param)
        {



            var result = new LocationData[0];
            try
            {
                if (param.HasValue())
                {
                    var url = "http://maps.googleapis.com/maps/api/geocode/xml?{0}&sensor=false";

                    string encodedUrl = string.Format(url, param);

                    Console.WriteLine(encodedUrl);

                    CallWithTimeout(() =>
                        {

                            var doc = XDocument.Load(encodedUrl);



                            IEnumerable<LocationData> addresses = from item in doc.Descendants("result")
                                                                  where item.Element("formatted_address").Value.ToLower().Contains("canada")
                                                                  select ConvertToData(item);



                            result = addresses.Where(a => a != null).ToArray();
                        }, 10000);

                }
            }
            catch (Exception ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Search failed : " + param);
                ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
            }
            return result.ToArray();
        }

        static void CallWithTimeout(Action action, int timeoutMilliseconds)
        {
            Thread threadToKill = null;
            Action wrappedAction = () =>
            {
                threadToKill = Thread.CurrentThread;
                action();
            };

            IAsyncResult result = wrappedAction.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
            {
                wrappedAction.EndInvoke(result);
            }
            else
            {
                threadToKill.Abort();
                throw new TimeoutException();
            }
        }
        public double? GetRouteDistance(double originLong, double originLat, double destLong, double destLat)
        {

            //lat,long40.714224,-73.961452
            double? result = null;
            try
            {

                var url = "http://maps.googleapis.com/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor=false";



                string encodedUrl = string.Format(url, ToUSFormat(originLat), ToUSFormat(originLong), ToUSFormat(destLat), ToUSFormat(destLong));

                Console.WriteLine(encodedUrl);

                var doc = XDocument.Load(encodedUrl);

                var route = doc.Descendants("route").FirstOrDefault();


                if (route != null)
                {
                    var leg = route.Descendants("leg").FirstOrDefault();
                    if (leg != null)
                    {
                        result = ConvertToDouble(leg.Element("distance").Element("value").Value);
                    }

                }



            }
            catch (Exception ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().LogMessage("GetRouteDistance failed");
                ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
            }


            var d = result.HasValue ? result.Value.ToString() : "NULL";


            ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Route distance : " + d);

            return result;

            //http://maps.googleapis.com/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor=false

        }

        private string ToUSFormat(double l)
        {
            return l.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat);
        }

        private LocationData ConvertToData(XElement item)
        {

            LocationData result = new LocationData { Address = "" };


            if ((item.Element("type").Value == "locality") || (item.Element("type").Value == "political") || (item.Element("type").Value == "postal_code") || (item.Element("type").Value == "administrative_area_level_2") || (item.Element("type").Value == "country"))
            {
                return null;
            }


            var addresses = from e in item.Descendants("address_component")
                            where e.Element("type").Value == "street_number"
                            select (string)e.Element("short_name").Value;

            if (addresses.Count() > 0)
            {
                if (addresses.ElementAt(0).Contains("-"))
                {
                    result.Address = addresses.ElementAt(0).Split('-')[0];
                }
                else
                {
                    result.Address = addresses.ElementAt(0);
                }
            }



            var citys = from e in item.Descendants("address_component")
                        where e.Element("type").Value == "locality"
                        select (string)e.Element("short_name").Value;


            var streets = from e in item.Descendants("address_component")
                          where e.Element("type").Value == "route"
                          select (string)e.Element("long_name").Value;


            if (streets.Count() > 0)
            {
                result.Address = result.Address.Trim() + " " + streets.ElementAt(0).Trim() + ", " + citys.ElementAt(0).Trim();
            }

            if (result.Address.IsNullOrEmpty())
            {
                result.Address = item.Element("formatted_address").Value;
            }

            result.Latitude = ConvertToDouble((string)item.Element("geometry").Element("location").Element("lat").Value);
            result.Longitude = ConvertToDouble((string)item.Element("geometry").Element("location").Element("lng").Value);
            return result;
        }

        public double? ConvertToDouble(string val)
        {

            if (val.HasValue())
            {


                try
                {
                    return Convert.ToDouble(val);
                }
                catch
                {
                    try
                    {
                        return Convert.ToDouble(val.Replace(".", ","));
                    }
                    catch
                    {
                        return Convert.ToDouble(val.Replace(",", "."));
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public LocationData[] FindSimilar(string address)
        {
            var addresses = new List<LocationData>();

            if (address.IsNullOrEmpty())
            {
                return addresses.ToArray();

            }

            var favAddresses = ServiceLocator.Current.GetInstance<IAccountService>().GetFavoriteAddresses();
            if (favAddresses.Count() > 0)
            {
                addresses.AddRange(favAddresses );
            }

            var historic  = ServiceLocator.Current.GetInstance<IAccountService>().GetHistoryAddresses();
            

            if (historic.Count() > 0)
            {

                foreach (var hist in historic)
                {
                    if (addresses.None(a => a.IsSame(hist)))
                    {
                        addresses.Add(hist);
                    }
                }
            }

            return addresses.Where(a => a.Address.HasValue() && a.Address.ToLower().StartsWith(address.ToLower())).ToArray();

        }

        protected ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public int CreateOrder(AccountData user, BookingInfoData info, out string error)
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


            //        order.MobileNote = ServiceLocator.Current.GetInstance <IAppResource> ().MobileUser;
            //        order.MobileNote += "\n\n" + ServiceLocator.Current.GetInstance <IAppResource> ().PaiementType + " " + info.Settings.ChargeTypeName;
            //        order.MobileNote += "\n\n" + ServiceLocator.Current.GetInstance <IAppResource> ().Notes + " " + info.Notes;
            //    if (info.PickupLocation.IsGPSNotAccurate)
            //    {
					
            //            var note = "\n\n" + ServiceLocator.Current.GetInstance <IAppResource> ().OrderNote;
            //        note += ServiceLocator.Current.GetInstance<IAppResource>().OrderNoteGPSApproximate;
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

        public OrderStatus GetOrderStatus(AccountData user, int orderId)
        {
            OrderStatus r = null;
            //UseService(service =>
            //{

            //    ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Begin GetOrderStatus");

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
            //            status.Status = ServiceLocator.Current.GetInstance<IAppResource>().StatusInvalid;
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
            //                status.Status += Environment.NewLine + string.Format(ServiceLocator.Current.GetInstance<IAppResource>().CarAssigned, result.NoVehicle);
            //            }
            //        }



            //    }
            //    else
            //    {
            //        ServiceLocator.Current.GetInstance<ILogger>().LogMessage(result.ErrorMessage);
            //    }

            //    ServiceLocator.Current.GetInstance<ILogger>().LogMessage("End GetOrderStatus");

            //});
            return r;
        }

        public bool IsCompleted(AccountData user, int orderId)
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

        public bool CancelOrder(AccountData user, int orderId)
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

