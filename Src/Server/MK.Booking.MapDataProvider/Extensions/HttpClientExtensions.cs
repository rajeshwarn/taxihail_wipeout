using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using apcurium.MK.Common.Extensions;

namespace MK.Booking.Google.Android.Extensions
{
    public static class HttpClientExtensions
    {
        public static Task<TResult> PostAsync<TResult>(this HttpClient client, string absoluteOrRelativeUrl, object content)
        {
            return Task.Run(async () =>
            {
                var httpContent = new StringContent(content.ToJson(), Encoding.UTF8, "application/json");

                var httpResponseMessage = await client.PostAsync(absoluteOrRelativeUrl, httpContent);

                httpResponseMessage.EnsureSuccessStatusCode();

                var result = await httpResponseMessage.Content.ReadAsStringAsync();

                return result.FromJson<TResult>();
            });
        }
    }
}