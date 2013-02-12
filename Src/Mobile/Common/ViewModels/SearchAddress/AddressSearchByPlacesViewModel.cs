//using System;
//using System.Collections.Generic;
//using System.Linq;
//using TinyIoC;
//using apcurium.Framework.Extensions;
//using apcurium.MK.Booking.Mobile.AppServices;
//using apcurium.MK.Common.Entity;
//using apcurium.MK.Booking.Mobile.Infrastructure;
//using System.Threading;
//
//namespace apcurium.MK.Booking.Mobile.ViewModels.SearchAddress
//{
//    public class AddressSearchByPlacesViewModel : AddressSearchBaseViewModel
//    {
//        private IGoogleService _googleService;
//
//       	public AddressSearchByPlacesViewModel(IGoogleService googleService)
//        {
//            _googleService = googleService;
//        }
//
//       	protected override IEnumerable<AddressViewModel> SearchAddresses ()
//		{
//			Func<Address, bool> predicate = c => true;
//			if (Criteria.HasValue ()) {
//				predicate = x => (x.FriendlyName != null && x.FriendlyName.ToLowerInvariant ().Contains (Criteria)) || (x.FullAddress != null && x.FullAddress.ToLowerInvariant ().Contains (Criteria));
//			}
//
//			var position = TinyIoCContainer.Current.Resolve<ILocationService> ().LastKnownPosition;
//            if (position == null)
//            {
//                var cancellationToken = new CancellationTokenSource();
//                var locService = TinyIoCContainer.Current.Resolve<ILocationService>().GetPositionAsync(1000, 1000, 1000, 5000, cancellationToken.Token);
//                locService.ContinueWith(p => position = p.Result);
//                locService.Start();
//                locService.Wait();
//                
//                if (position == null)
//                {
//                    return Enumerable.Empty<AddressViewModel> ();
//                }
//            }
//
//           
//			var fullAddresses = _googleService.GetNearbyPlaces (position.Latitude, position.Longitude);
//			var addresses = fullAddresses.Where (predicate).ToList ();
//			return addresses.Select (a => new AddressViewModel () { Address = a, ShowPlusSign = false, ShowRightArrow = false, IsFirst = a.Equals(addresses.First()), IsLast = a.Equals(addresses.Last()) }).ToList ();
//		}
//    }
//}