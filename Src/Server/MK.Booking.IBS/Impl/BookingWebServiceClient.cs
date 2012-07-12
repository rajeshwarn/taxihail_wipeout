using System;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;

namespace apcurium.MK.Booking.IBS.Impl
{
    public class BookingWebServiceClient : BaseService<WebOrder7Service>, IBookingWebServiceClient
    {
        protected override string GetUrl()
        {
            return base.GetUrl() + "IWEBOrder_7";
        }


        private ILogger _logger;

        public BookingWebServiceClient(IConfigurationManager configManager, ILogger logger)
            : base(configManager, logger)
        {
            _logger = logger;
        }

        public Tuple<string, double?, double?> GetOrderStatus(int orderId, int accountId)
        {
            var status = new Tuple<string, double?, double?>(TWEBOrderStatusValue.wosNone.ToString(), null, null);
            UseService(service =>
            {
                var orderStatus = service.GetOrderStatus(_userNameApp, _passwordApp, orderId, string.Empty, string.Empty, accountId);

                double latitude = 0;
                double longitude = 0;
                var result = service.GetVehicleLocation(_userNameApp, _passwordApp, orderId, ref latitude, ref longitude);
                if (result == 0)
                {
                    status = new Tuple<string, double?, double?>(orderStatus.ToString(), latitude, longitude);
                }
                else
                {
                    status = new Tuple<string, double?, double?>(orderStatus.ToString(), null, null);
                }
            });
            return status;
        }

        public int? CreateOrder(int providerId, int accountId, string passengerName, string phone, int nbPassengers, int vehicleTypeId, string note, DateTime? pickupDateTime, IBSAddress pickup, IBSAddress dropoff)
        {
            var order = new TBookOrder_5();

            order.ServiceProviderID = providerId;
            order.AccountID = accountId;
            order.Customer = passengerName;
            order.Phone = phone;

            //TODO : need to check ibs setup for shortesst time.
            DateTime pDate = pickupDateTime.HasValue ? pickupDateTime.Value : DateTime.Now.AddMinutes(2);
            order.PickupDate = new TWEBTimeStamp { Year = pDate.Year, Month = pDate.Month, Day = pDate.Day };
            order.PickupTime = new TWEBTimeStamp { Hour = pDate.Hour, Minute = pDate.Minute, Second = 0, Fractions = 0 };

            order.PickupAddress = new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = pickup.Apartment, Longitude = pickup.Longitude, Latitude = pickup.Latitude };
            order.DropoffAddress = dropoff == null ? new TWEBAddress() : new TWEBAddress { StreetPlace = pickup.FullAddress, AptBaz = pickup.Apartment, Longitude = pickup.Longitude, Latitude = pickup.Latitude };
            order.Passengers = nbPassengers;
            order.VehicleTypeID = vehicleTypeId;
            order.Note = note;
            order.ContactPhone = phone;
            order.OrderStatus = TWEBOrderStatusValue.wosPost;

            int? orderId = null;


            UseService(service =>
            {
                try
                {
                    orderId = service.SaveBookOrder_5(_userNameApp, _passwordApp, order);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex);
                    throw;
                }
            });
            return orderId;
        }

        //public bool IsValid(ref BookingInfoData info)
        //{
        //    return info.PickupLocation.Address.HasValue() && info.PickupLocation.Latitude.HasValue && info.PickupLocation.Longitude.HasValue;
        //}

        //public LocationData[] SearchAddress(double latitude, double longitude)
        //{

        //    string param = "latlng={0},{1}";
        //    param = string.Format(param, latitude.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat), longitude.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat));
        //    return SearchAddressUsingGoogle(param);
        //}

        //public LocationData[] SearchAddress(string address)
        //{
        //    if (address.HasValue())
        //    {

        //        LocationData[] result = new LocationData[0];
        //        string param = "";
        //        if (!address.Contains(","))
        //        {
        //            param = "address={0},montreal,qc,canada&region=ca";
        //            param = string.Format(param, address.Replace(" ", "+"));
        //            result = SearchAddressUsingGoogle(param);
        //        }

        //        if (result.Count() > 0)
        //        {
        //            return result;
        //        }
        //        else
        //        {

        //            param = "address={0},qc,canada&region=ca";
        //            param = string.Format(param, address.Replace(" ", "+"));
        //            return SearchAddressUsingGoogle(param);
        //        }
        //    }
        //    else
        //    {
        //        return new LocationData[0];
        //    }

        //}

        //private LocationData[] SearchAddressUsingGoogle(string param)
        //{
        //    var result = new LocationData[0];
        //    try
        //    {
        //        if (param.HasValue())
        //        {
        //            var url = "http://maps.googleapis.com/maps/api/geocode/xml?{0}&sensor=true";

        //            string encodedUrl = string.Format(url, param);

        //            Console.WriteLine(encodedUrl);

        //            var doc = XDocument.Load(encodedUrl);

        //            IEnumerable<LocationData> addresses = from item in doc.Descendants("result")
        //                                                  where item.Element("formatted_address").Value.ToLower().Contains("canada")
        //                                                  select ConvertToData(item);

        //            result = addresses.Where(a => a != null).ToArray();

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Search failed : " + param);
        //        ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
        //    }
        //    return result.ToArray();
        //}

        //public double? GetRouteDistance(double originLong, double originLat, double destLong, double destLat)
        //{

        //    //lat,long40.714224,-73.961452
        //    double? result = null;
        //    try
        //    {

        //        var url = "http://maps.googleapis.com/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor=false";



        //        string encodedUrl = string.Format(url, ToUSFormat(originLat), ToUSFormat(originLong), ToUSFormat(destLat), ToUSFormat(destLong));

        //        Console.WriteLine(encodedUrl);

        //        var doc = XDocument.Load(encodedUrl);

        //        var route = doc.Descendants("route").FirstOrDefault();


        //        if (route != null)
        //        {
        //            var leg = route.Descendants("leg").FirstOrDefault();
        //            if (leg != null)
        //            {
        //                result = ConvertToDouble(leg.Element("distance").Element("value").Value);
        //            }

        //        }



        //    }
        //    catch (Exception ex)
        //    {
        //        ServiceLocator.Current.GetInstance<ILogger>().LogMessage("GetRouteDistance failed");
        //        ServiceLocator.Current.GetInstance<ILogger>().LogError(ex);
        //    }


        //    var d = result.HasValue ? result.Value.ToString() : "NULL";


        //    ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Route distance : " + d);

        //    return result;

        //    //http://maps.googleapis.com/maps/api/directions/xml?origin={0},{1}&destination={2},{3}&sensor=false

        //}



        //private string ToUSFormat(double l)
        //{
        //    return l.ToString(CultureInfo.GetCultureInfo("en-US").NumberFormat);
        //}

        //private LocationData ConvertToData(XElement item)
        //{
        //    LocationData result = new LocationData { Address = "" };


        //    if ((item.Element("type").Value == "locality") || (item.Element("type").Value == "political") || (item.Element("type").Value == "postal_code") || (item.Element("type").Value == "administrative_area_level_2") || (item.Element("type").Value == "country"))
        //    {
        //        return null;
        //    }


        //    var addresses = from e in item.Descendants("address_component")
        //                    where e.Element("type").Value == "street_number"
        //                    select (string)e.Element("short_name").Value;

        //    if (addresses.Count() > 0)
        //    {
        //        if (addresses.ElementAt(0).Contains("-"))
        //        {
        //            result.Address = addresses.ElementAt(0).Split('-')[0];
        //        }
        //        else
        //        {
        //            result.Address = addresses.ElementAt(0);
        //        }
        //    }



        //    var citys = from e in item.Descendants("address_component")
        //                where e.Element("type").Value == "locality"
        //                select (string)e.Element("short_name").Value;


        //    var streets = from e in item.Descendants("address_component")
        //                  where e.Element("type").Value == "route"
        //                  select (string)e.Element("long_name").Value;


        //    if (streets.Count() > 0)
        //    {
        //        result.Address = result.Address.Trim() + " " + streets.ElementAt(0).Trim() + ", " + citys.ElementAt(0).Trim();
        //    }

        //    if (result.Address.IsNullOrEmpty())
        //    {
        //        result.Address = item.Element("formatted_address").Value;
        //    }

        //    result.Latitude = ConvertToDouble((string)item.Element("geometry").Element("location").Element("lat").Value);
        //    result.Longitude = ConvertToDouble((string)item.Element("geometry").Element("location").Element("lng").Value);
        //    return result;
        //}

        //public double? ConvertToDouble(string val)
        //{

        //    if (val.HasValue())
        //    {


        //        try
        //        {
        //            return Convert.ToDouble(val);
        //        }
        //        catch
        //        {
        //            try
        //            {
        //                return Convert.ToDouble(val.Replace(".", ","));
        //            }
        //            catch
        //            {
        //                return Convert.ToDouble(val.Replace(",", "."));
        //            }
        //        }
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        //public LocationData[] FindSimilar(string address)
        //{
        //    var addresses = new List<LocationData>();

        //    if (address.IsNullOrEmpty())
        //    {
        //        return addresses.ToArray();

        //    }

        //    var loggedUser = ServiceLocator.Current.GetInstance<IAppContext>().LoggedUser;
        //    if (loggedUser.FavoriteLocations.Count() > 0)
        //    {
        //        addresses.AddRange(loggedUser.FavoriteLocations);
        //    }


        //    var historic = loggedUser.BookingHistory.Where(b => !b.Hide && b.PickupLocation.Name.IsNullOrEmpty()).OrderByDescending(b => b.RequestedDateTime).Select(b => b.PickupLocation).ToArray();

        //    if (historic.Count() > 0)
        //    {

        //        foreach (var hist in historic)
        //        {
        //            if (addresses.None(a => a.IsSame(hist)))
        //            {
        //                addresses.Add(hist);
        //            }
        //        }
        //    }

        //    return addresses.Where(a => a.Address.HasValue() && a.Address.ToLower().StartsWith(address.ToLower())).ToArray();

        //}

        //public void UpdateHistory(AccountData user)
        //{
        //    UseService(service =>
        //    {
        //        var orders = service.GetOrderHistory_6(userNameApp, passwordApp, user.Id, DateTime.Now.AddMonths(-3).ToWSDateTime(), DateTime.Now.ToWSDateTime(), 0);
        //        if (orders != null
        //            && orders.OrderCount > 0)
        //        {
        //            var mapper = new OrderMapping();
        //            var dataService = ServiceLocator.Current.GetInstance<IStaticDataService>();
        //            mapper.UpdateHistory(user, orders.OrderList, dataService.GetVehiclesList(), dataService.GetCompaniesList(), dataService.GetPaymentsList());
        //        }

        //    });
        //}

        //public int CreateOrder(AccountData user, BookingInfoData info, out string error)
        //{
        //    error = "";
        //    string errorResult = "";
        //    int r = 0;
        //    UseService(service =>
        //    {
        //        var orderWS = new OrderMapping().ToWSOrder(info, user);
        //        orderWS.OrderStatus = TWEBOrderStatusValue.wosPost;
        //        Logger.LogMessage("Create order  :" + user.Email);

        //        var result = service.SaveBookOrder_6(userNameApp, passwordApp, orderWS);
        //        if (result > 0)
        //        {
        //            r = result;
        //            Logger.LogMessage("Order Created :" + result.ToString());
        //        }
        //        else
        //        {
        //            errorResult = "Can't create order";
        //            Logger.LogMessage("Error order creation :" + errorResult);
        //        }

        //    });
        //    error = errorResult;
        //    return r;
        //}

        //public OrderStatus GetOrderStatus(AccountData user, int orderId)
        //{
        //    OrderStatus r = null;
        //    UseService(service =>
        //    {
        //        ServiceLocator.Current.GetInstance<ILogger>().LogMessage("Begin GetOrderStatus");

        //        var orderStatus = service.GetOrderStatus(userNameApp, passwordApp, orderId, string.Empty, string.Empty, user.Id);
        //        var status = new OrderStatus();
        //        status.Status = (OrderStatus.WsStatus)orderStatus;

        //        double latitude = 0;
        //        double longitude = 0;
        //        //var result = service.GetVehicleLocation(userNameApp, passwordApp, orderId, ref latitude, ref longitude);
        //        //if (result == 0)
        //        //{
        //        //    status.Longitude = longitude;
        //        //    status.Latitude = latitude;
        //        //}
        //        r = status;
        //        Logger.LogMessage("End GetOrderStatus");

        //    });
        //    return r;
        //}

        //public bool IsCompleted(AccountData user, int orderId)
        //{
        //    bool isCompleted = false;
        //    UseService(service =>
        //    {
        //        var result = service.GetOrderStatus(userNameApp, passwordApp, orderId, string.Empty, string.Empty, user.Id);
        //        isCompleted = IsCompleted(result);
        //    });
        //    return isCompleted;

        //}

        //public bool IsCompleted(int statusId)
        //{
        //    return (statusId == (decimal)TWEBOrderStatusValue.wosDONE) || (statusId == (decimal)TWEBOrderStatusValue.wosCANCELLED);
        //}

        //private bool IsCompleted(TWEBOrderStatusValue statusId)
        //{
        //    return (statusId == TWEBOrderStatusValue.wosDONE) || (statusId == TWEBOrderStatusValue.wosCANCELLED);
        //}

        //public bool CancelOrder(AccountData user, int orderId)
        //{
        //    bool isCompleted = false;
        //    UseService(service =>
        //    {
        //        var result = service.CancelBookOrder(userNameApp, passwordApp, orderId, user.DefaultSettings.Phone, null, user.Id);
        //        isCompleted = result == 0;
        //    });
        //    return isCompleted;
        //}
    }
}