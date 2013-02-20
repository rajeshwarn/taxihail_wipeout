using System;
using System.Reactive.Linq;
using System.Threading;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Client.Cmt;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.AppServices.Impl
{
    public class PreCogService  : IPreCogService
    {
        private readonly ILocationService _locationService;
        private readonly CmtPreCogServiceClient _preCogServiceClient;
        private IDisposable _statusDaemon;
        private static int _defaulStatusInterval = 60;
        private bool _isInitRequest = true;
        private bool _userLocationHasBeenSend = false;
        private Position _userLocation;
        private string _linkedVehiculeId;

        public PreCogService(ILocationService locationService, CmtPreCogServiceClient preCogServiceClient)
        {
            _locationService = locationService;
            _preCogServiceClient = preCogServiceClient;
        }

        public void Start()
        {
             _locationService.GetPositionAsync(5000, 50, 2000, 2000, new CancellationToken())
                .ContinueWith(t => LaunchStatusDaemon(_defaulStatusInterval));
        }

        private void StopDaemon()
        {
           if (_statusDaemon != null)
           {
               _statusDaemon.Dispose();
           }
        }

        public void SetUserLocation(Position position)
        {
            _userLocation = position;
        }

        public PreCogResponse SendRequest(PreCogRequest request)
        {
            FillGpsAndExtractStatusInformation(request);
            var response = _preCogServiceClient.Send(request, _locationService.IsGpsActive);
            CheckIntervalFromServer(response);
            return response;
        }
        

        private void LaunchStatusDaemon(int interval)
        {
            if (_statusDaemon != null) _statusDaemon.Dispose();
            _statusDaemon = Observable.Timer(TimeSpan.FromSeconds(interval)).Subscribe(SendStatus);
        }

        private void SendStatus(long notUsed)
        {
            var gpsLocation = _locationService.LastKnownPosition;

            //first time we send a location we should set the flag init to true
            if (!_userLocationHasBeenSend
                && (gpsLocation != null || _userLocation !=null))
            {
                _isInitRequest = true;
                _userLocationHasBeenSend = true;
            }

            var request = new PreCogRequest
            {
                Init = _isInitRequest,
                Type = PreCogType.Status,
                LinkedVehiculeId = _linkedVehiculeId,
            };

            //add GPS infos
            FillGpsAndExtractStatusInformation(request);

            if (_userLocation != null)
            {
                request.LocLon = _userLocation.Latitude;
                request.LocLat = _userLocation.Longitude;
                request.LocTime = _userLocation.Time;
                request.LocDesc = _userLocation.Description;
            }
            
            var response = _preCogServiceClient.Send(request, _locationService.IsGpsActive);

            //if response containe true, so can stop to send the init = true in subsequent calls
            if (response.Init)
            {
                _isInitRequest = false;
            }

            CheckIntervalFromServer(response);
        }

        private void CheckIntervalFromServer(PreCogResponse response)
        {
            //server can stop or change the interval of the status request
            if (!response.Interval.HasValue) return;

            if (response.Interval == 0)
            {
                StopDaemon();

            }else if (response.Interval != _defaulStatusInterval)
            {
                _defaulStatusInterval = response.Interval.Value;
                LaunchStatusDaemon(response.Interval.Value);
            }
        }

        private void FillGpsAndExtractStatusInformation(PreCogRequest request)
        {
             _linkedVehiculeId = request.LinkedVehiculeId;
            if (_locationService.LastKnownPosition != null)
            {
                request.GpsLon = _locationService.LastKnownPosition.Longitude;
                request.GpsLat = _locationService.LastKnownPosition.Latitude;
                request.GpsAccuracy = _locationService.LastKnownPosition.Accuracy;
                request.GpsAltitude = _locationService.LastKnownPosition.Altitude;
                request.GpsBearing = _locationService.LastKnownPosition.Bearing;
                request.GpsSpeed = _locationService.LastKnownPosition.Speed;
                request.LocLon = _locationService.LastKnownPosition.Longitude;
                request.LocLat = _locationService.LastKnownPosition.Latitude;
                request.LocTime = _locationService.LastKnownPosition.Time;
            }
        }
    }
}