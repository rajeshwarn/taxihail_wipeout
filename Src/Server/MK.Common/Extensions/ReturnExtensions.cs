using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Common.Extensions
{
    public static class ReturnExtensions
    {
        public static string GetUrlFromRoute<T>(this IReturn<T> request, bool setPropertiesAsUrlParams = false)
        {
            var requestType = request.GetType();

            var properties = requestType.GetProperties()
                .Select(property => new
                {
                    property.Name,
                    Value = property.GetValue(request)
                })
                .Where(property => property.Value != null)
                .ToArray();

            var routeAttributes = requestType.GetCustomAttributes(typeof(RouteAttribute), true)
                .OfType<RouteAttribute>()
                .ToArray();

            if (routeAttributes.None())
            {
                return string.Empty;
            }

            var routeAttribute = routeAttributes.Length == 1
                ? routeAttributes.First()
                : routeAttributes.SelectCorrectRouteAttribute(properties.Select(p => p.Name));

            var matches = Regex.Matches(routeAttribute.Address, "\\{[a-zA-Z]+\\}", RegexOptions.Compiled)
                .Cast<Match>()
                .Select(match => new
                {
                    PropertyName = match.Value.Replace("{", "").Replace("}", ""),
                    Tag = match.Value
                })
                .ToArray();

            var address = routeAttribute.Address;

            foreach (var match in matches)
            {
                var property = properties.First(p => p.Name == match.PropertyName);

                address = address.Replace(match.Tag, property.Value.ToString());
            }

            if (!setPropertiesAsUrlParams)
            {
                return address;
            }

            var parameters = properties
                .Where(prop => matches.None(match => prop.Name == match.PropertyName))
                .Select(prop => prop.Name + "=" + Uri.EscapeUriString(prop.Value.ToString()))
                .ToArray();

            return parameters.Any()
                ? address + "?" + parameters.JoinBy("&")
                : address;
        }

        private static RouteAttribute SelectCorrectRouteAttribute(this IEnumerable<RouteAttribute> routes, IEnumerable<string> propertyInfos)
        {
            var activePropertyNames = propertyInfos as string[] ?? propertyInfos.ToArray();
            var routeAttributes = routes as RouteAttribute[] ?? routes.ToArray();

            var routeAttribute = routeAttributes.FirstOrDefault(route => activePropertyNames.All(p => route.Address.Contains(p)));

            if (routeAttribute != null)
            {
                return routeAttribute;
            }

            return routeAttributes.Select(route => new
                {
                    Route = route,
                    propertyNames = activePropertyNames.Where(p => route.Address.Contains(p)).ToArray()
                })
                .OrderByDescending(p => p.propertyNames.Length)
                .Select(route => route.Route)
                .FirstOrDefault();
        }
    }
}