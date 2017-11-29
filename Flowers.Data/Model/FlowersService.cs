using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flowers.Api;
using Flowers.Api.Requests;
using GalaSoft.MvvmLight.Ioc;

namespace Flowers.Model
{
    public class FlowersService : IFlowersService
    {
        private IApiClient _apiClient;
        private IClock _clock;

        public FlowersService(IApiClient apiClient, IClock clock)
        {
            _apiClient = apiClient;
            _clock = clock;
        }

        public async Task<IList<Flower>> Refresh()
        {
            var flowersResponse = await _apiClient.Execute(new FlowersRequest(_clock.Now().Ticks)).ConfigureAwait(false);

            foreach (var model in flowersResponse.DeserializedResponse.Data)
            {
                model.HasChanges = false;
            }

            return flowersResponse.DeserializedResponse.Data;
        }

        public async Task<bool> Save(Flower flower)
        {
            try
            {
                var saveResponse = await _apiClient.Execute(new SaveFlowerRequest(flower, _clock.Now().Ticks)).ConfigureAwait(false);
                flower.HasChanges = false;

                return true;
            }
            catch (ApiException)
            {
                return false;
            }
        }
    }
}