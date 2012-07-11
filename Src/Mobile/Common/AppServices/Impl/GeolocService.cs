using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using TinyIoC;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{

    public class GeolocService : IGeolocService
    {
        public Address ValidateAddress(string address)
        {
            try
            {
                var addresses = TinyIoCContainer.Current.Resolve<GeocodingServiceClient>().Search( address );
                return addresses.Addresses.FirstOrDefault();
            }
            catch
            {
                return null; ;
            }
        }


        public Address[] SearchAddress(double latitude, double longitude)
        {
            try
            {
                var addresses =  TinyIoCContainer.Current.Resolve<GeocodingServiceClient>().Search(latitude, longitude);
                return addresses.Addresses;                
            }
            catch
            {
                return new Address[0];
            }

        }

        public DirectionInfo GetDirectionInfo(double originLong, double originLat, double destLong, double destLat)
        {

            try
            {
                var direction = TinyIoCContainer.Current.Resolve<DirectionsServiceClient>().GetDirectionDistance(originLat, originLong, destLat, destLong);
                return direction;
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


    }
}