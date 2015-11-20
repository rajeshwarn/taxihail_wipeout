using System.Linq;
using System.Text.RegularExpressions;
using ServiceStack.ServiceHost;

namespace apcurium.MK.Common.Extensions
{
    public static class ReturnExtensions
    {
        public static string GetUrlFromRoute<T>(this IReturn<T> request)
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

            var routeAttribute = requestType.GetCustomAttributes(typeof(RouteAttribute), true)
                .OfType<RouteAttribute>()
                .FirstOrDefault(route => properties.All(p => route.Address.Contains(p.Name)));

            if (routeAttribute == null)
            {
                return string.Empty;
            }

            var matches = Regex.Matches(routeAttribute.Address, "\\{[a-zA-Z]+\\}", RegexOptions.Compiled)
                .Cast<Match>()
                .Select(match => match.Value);

            var address = routeAttribute.Address;

            foreach (var propertyName in matches)
            {
                var property = properties.First(p => p.Name == propertyName);

                address = address.Replace(propertyName, property.Value.ToString());
            }


            return address;
        }
    }
}