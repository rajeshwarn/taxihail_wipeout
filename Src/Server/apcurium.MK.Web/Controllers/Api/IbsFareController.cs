using System;
using System.Linq;
using System.Web.Http;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Api.Services;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.ReadModel.Query.Contract;
using apcurium.MK.Booking.Resources;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Web.Controllers.Api
{
    [RoutePrefix("ibsfare")]
    public class IbsFareController : BaseApiController
    {
        private readonly IIBSServiceProvider _ibsServiceProvider;
        private readonly IServerSettings _serverSettings;
        private readonly IVehicleTypeDao _vehicleTypeDao;
        private readonly Resources _resources;

        public IbsFareController(Resources resources, IVehicleTypeDao vehicleTypeDao, IServerSettings serverSettings, IIBSServiceProvider ibsServiceProvider)
        {
            _resources = resources;
            _vehicleTypeDao = vehicleTypeDao;
            _serverSettings = serverSettings;
            _ibsServiceProvider = ibsServiceProvider;
        }

        [HttpGet, Route("ibsfares")]
        public IHttpActionResult GetIbsFare([FromUri]IbsFareRequest request)
        {
            var tripDurationInMinutes = (request.TripDurationInSeconds.HasValue ? (int?)TimeSpan.FromSeconds(request.TripDurationInSeconds.Value).TotalMinutes : null);

            var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();

            var fare = _ibsServiceProvider.Booking().GetFareEstimate(
                request.PickupLatitude,
                request.PickupLongitude,
                request.DropoffLatitude,
                request.DropoffLongitude,
                request.PickupZipCode,
                request.DropoffZipCode,
                request.AccountNumber,
                request.CustomerNumber,
                tripDurationInMinutes,
                _serverSettings.ServerData.DefaultBookingSettings.ProviderId,
                request.VehicleType,
                defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1);

            if (fare.FareEstimate != null)
            {
                var distance = fare.Distance ?? 0;

                return Ok(new DirectionInfo
                {
                    Distance = distance,
                    Price = fare.FareEstimate,
                    FormattedDistance = _resources.FormatDistance(distance),
                    FormattedPrice = _resources.FormatPrice(fare.FareEstimate)
                });
            }

            return Ok(new DirectionInfo());
        }

        [HttpGet, Route("")]
        public IHttpActionResult Get([FromUri]IbsDistanceRequest request)
        {
            var tripDurationInMinutes = (request.WaitTime.HasValue ? (int?)TimeSpan.FromSeconds(request.WaitTime.Value).TotalMinutes : null);

            var defaultVehicleType = _vehicleTypeDao.GetAll().FirstOrDefault();

            var distance = request.Distance.ToDistanceInRightUnit(_serverSettings.ServerData.DistanceFormat);

            var fare = _ibsServiceProvider.Booking().GetDistanceEstimate(
                distance,
                tripDurationInMinutes,
                request.StopCount,
                request.PassengerCount,
                request.VehicleType,
                defaultVehicleType != null ? defaultVehicleType.ReferenceDataVehicleId : -1,
                request.AccountNumber,
                request.CustomerNumber,
                request.TripTime
                );

            if (fare.TotalFare != null)
            {
                return Ok(new DirectionInfo
                {
                    Distance = distance,
                    Price = fare.TotalFare,
                    FormattedDistance = _resources.FormatDistance(distance),
                    FormattedPrice = _resources.FormatPrice(fare.TotalFare)
                });
            }

            return Ok(new DirectionInfo());
        }
    }
}
