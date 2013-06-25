using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AutoMapper;
using Infrastructure.Messaging;
using MK.Common.Android;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;

namespace apcurium.MK.Booking.Api.Services
{
    public class VehicleService: Service
    {
        readonly IBookingWebServiceClient _bookingWebServiceClient;
        public VehicleService(IBookingWebServiceClient bookingWebServiceClient)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
        }

        public AvailableVehiclesResponse Get(AvailableVehicles request)
        {
            var vehicles = _bookingWebServiceClient.GetAvailableVehicles(request.Latitude, request.Longitude, 2000, 10);
            if (vehicles.Any())
            {
                return new AvailableVehiclesResponse(vehicles.Select(Mapper.Map<AvailableVehicle>));
            }
            return new AvailableVehiclesResponse();
        }

        public SendMessageToDriverResponse Post(SendMessageToDriverRequest request)
        {
            return new SendMessageToDriverResponse()
                {
                    Success = _bookingWebServiceClient.SendMessageToDriver(request.Message, request.CarNumber)
                };
        }


    }
}
