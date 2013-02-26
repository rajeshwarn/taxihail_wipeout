using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Requests.Cmt;
using apcurium.MK.Booking.Api.Contract.Resources.Cmt;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Api.Client.Cmt
{
    public class CmtPreCogServiceClient : CmtBaseServiceClient
    {

        private readonly string[] _propertiesToExcludeIfNull = new[]{ "linkedVehiculeId" };
        private readonly string[] _propertiesToExcludeINoGps = new[] { "gpsLat", "gpsLon", "gpsSpeed", "gpsBearing", "gpsLat", "gpsAccuracy", "gpsAltitude" };


        public CmtPreCogServiceClient(string url) : base(url)
        {
        }

        public CmtPreCogServiceClient(string url, CmtAuthCredentials cmtAuthCredentials) : base(url, cmtAuthCredentials)
        {
        }

        public PreCogResponse Send(PreCogRequest request, bool activeGps)
        {
            var req = string.Format("/precog/{0}?{1}", Credentials.AccountId, FormatRequest(request, activeGps));
            var response = Client.Get<PreCogResponse>(req);
            return response;
        }

        private string FormatRequest(PreCogRequest request, bool activeGps)
        {
            JsConfig.EmitCamelCaseNames = true;
            var keyValues = request.ToStringDictionary().ToDictionary(x => x.Key, y => string.IsNullOrEmpty(y.Value) ? y.Value : y.Value.ToLowerInvariant());
            
            keyValues["type"] = GetEnumDescription(request.Type).ToLowerInvariant();

            if (request.LocTime != null)
            {
                keyValues["locTime"] = request.LocTime.Value.ToString("MMddyyyyHHmmss");
            }

            if (!activeGps)
            {
                _propertiesToExcludeINoGps.ForEach(x => keyValues.Remove(x));
            }

            //remove properties when null and sould not be send
            var parameterToRemoved = keyValues.Where(x => _propertiesToExcludeIfNull.Contains(x.Key) && string.IsNullOrEmpty(x.Value)).Select(x => x.Key).ToList();
            parameterToRemoved.ForEach(x => keyValues.Remove(x));



            var queryString = keyValues.Skip(1).Aggregate(keyValues.First().Key + "=" + keyValues.First().Value, (qs, property) => qs + "&" + property.Key + "=" + property.Value);
            return queryString;
        }

        public static string GetEnumDescription(Enum value)
        {
            var fi = value.GetType().GetField(value.ToString());

            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Any())
                return attributes[0].Description;
            else
                return value.ToString();
        }

    }
}