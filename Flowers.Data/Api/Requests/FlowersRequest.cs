using System;
using System.Net.Http;

using Flowers.Model;

namespace Flowers.Api.Requests
{
    public class FlowersRequest : ApiRequest<FlowersResult>
    {
        private static string Path = "labs/Flowers/FlowersService.ashx?action=get&{0}={1}";

        public FlowersRequest(long ticks)
        {
            HttpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format(Path, "ticks", ticks), UriKind.Relative),
                Method = HttpMethod.Get
            };
        }
    }
}
