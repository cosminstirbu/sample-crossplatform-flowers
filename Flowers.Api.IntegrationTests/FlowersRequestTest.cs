using NUnit.Framework;
using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flowers.Api.Requests;
using FluentAssertions;
using System.Net;

namespace Flowers.Api.IntegrationTests
{
    [TestFixture]
    public class FlowersRequestTest
    {
        [Test]
        public async Task FlowersRequestTest_FetchesData()
        {
            // Given
            var apiClient = GivenAnApiClient();

            // When
            var flowersResponse = await apiClient.Execute(new FlowersRequest(DateTime.Now.Ticks));

            // Then
            flowersResponse.HttpResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            flowersResponse.DeserializedResponse.Data.Should().NotBeEmpty();
        }

        private IApiClient GivenAnApiClient()
        {
            var apiClient = new ApiClient(new HttpClient
            {
                BaseAddress = new Uri("http://www.galasoft.ch/")
            });

            apiClient.DefaultQueryParameters = new Dictionary<string, string>
            {
                { WebConstants.AuthenticationKey, WebConstants.AuthenticationId }
            };

            return apiClient;
        }
    }
}
