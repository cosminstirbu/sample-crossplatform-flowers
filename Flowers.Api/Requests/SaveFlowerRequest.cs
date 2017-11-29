using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Flowers.Api.Data;

namespace Flowers.Api.Requests
{
    public class SaveFlowerRequest : ApiRequest<Void>
    {
        private static string Path = "labs/Flowers/FlowersService.ashx?action=save&{0}={1}";

        public SaveFlowerRequest(Flower flower, long ticks)
        {
            var json = JsonConvert.SerializeObject(flower);

            var content = new FormUrlEncodedContent(
                new[]
                {
                    new KeyValuePair<string, string>("flower", json)
                });

            HttpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(string.Format(Path, "ticks", ticks), UriKind.Relative),
                Content = content,
                Method = HttpMethod.Post
            };
        }
    }
}
