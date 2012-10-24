using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Entity;
using TinyIoC;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Maps;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

    public class GeolocService : BaseService, IGeolocService
    {
        public Address ValidateAddress(string address)
        {
            try
            {
                //var addresses = TinyIoCContainer.Current.Resolve<GeocodingServiceClient>().Search(address);
                var addresses = TinyIoCContainer.Current.Resolve<IGeocoding>().Search(address);
                return addresses.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return null; ;
            }
        }


        public Address[] SearchAddress(double latitude, double longitude)
        {
            try
            {                
                //var addresses = TinyIoCContainer.Current.Resolve<GeocodingServiceClient>().Search(latitude, longitude);
				var addresses = TinyIoCContainer.Current.Resolve<IGeocoding>().Search(latitude, longitude);
                return addresses;
            }
            catch
            {
                return new Address[0];
            }
        }

        public Address[] SearchAddress(string address, double latitude, double longitude)
        {
            try
            {                
                //var addresses = TinyIoCContainer.Current.Resolve<SearchLocationsServiceClient>().Search(address, latitude, longitude );
                var addresses = TinyIoCContainer.Current.Resolve<IAddresses>().Search(address, latitude, longitude);
                return addresses;
            }
            catch( Exception ex )
            {
                Console.WriteLine(ex.Message);
                return new Address[0];
            }

        }

        public DirectionInfo GetDirectionInfo(Address origin, Address dest)
        {
            if (origin.HasValidCoordinate() && dest.HasValidCoordinate())
            {
                return GetDirectionInfo(origin.Latitude, origin.Longitude, dest.Latitude, dest.Longitude);
            }
            else
            {
                return new DirectionInfo();
            }
        }
        public DirectionInfo GetDirectionInfo(double originLat, double originLong, double destLat, double destLong)
        {

            try
            {
                //var direction = TinyIoCContainer.Current.Resolve<DirectionsServiceClient>().GetDirectionDistance(originLat, originLong, destLat, destLong);
                var direction = TinyIoCContainer.Current.Resolve<IDirections>().GetDirection(originLat, originLong, destLat, destLong);
                return new DirectionInfo { Distance = direction.Distance, FormattedDistance = direction.FormattedDistance, FormattedPrice = direction.FormattedPrice, Price = direction.Price };
            }
            catch
            {
                return new DirectionInfo();
            }

        }


        public IEnumerable<Address> FindSimilar(string address)
        {
            var addresses = new List<Address>();

            if (address.IsNullOrEmpty())
            {
                return addresses.ToArray();

            }

            var favAddresses = TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();
            if (favAddresses.Count() > 0)
            {
                addresses.AddRange(favAddresses);
            }

            var historic = TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryAddresses();


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

            return addresses.Where(a => a.FullAddress.HasValue() && a.FullAddress.ToLower().StartsWith(address.ToLower())).ToArray();
        }

        /*public IEnumerable<Address> GetPopularAddress(double latitude, double longitude)
        {
            var _popularAddressesCacheKey = "sss";
            var cached = TinyIoCContainer.Current.Resolve<ICacheService>().Get<Address[]>(_popularAddressesCacheKey);

            if (cached != null)
            {
                return cached;
            }
            else
            {

                IEnumerable<Address> result = new Address[0];
                UseServiceClient<PopularAddressServiceClient>(service =>
                {
                    result = service.GetPopular();
                }
                );
                TinyIoCContainer.Current.Resolve<ICacheService>().Set(_popularAddressesCacheKey, result.ToArray());
                return result;
            }
        }*/


    }
}