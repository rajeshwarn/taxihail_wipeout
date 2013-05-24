using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Messaging;
using MK.Common.Android;
using ServiceStack.ServiceInterface;
using apcurium.MK.Booking.IBS;
using apcurium.MK.Booking.IBS.Impl;

namespace apcurium.MK.Booking.Api.Services
{
    public class VehicleService: Service
    {
        private IBookingWebServiceClient _bookingWebServiceClient;
        public VehicleService(IBookingWebServiceClient bookingWebServiceClient)
        {
            _bookingWebServiceClient = bookingWebServiceClient;
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
