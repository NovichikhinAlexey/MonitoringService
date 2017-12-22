using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Newtonsoft.Json;
using Common.Log;
using Core.Models;
using Core.Services;
using Core.Exceptions;

namespace Services
{
    //Uses HttpClient.
    public class IsAliveService : IIsAliveService
    {
        private readonly HttpClient _httpClient;
        private readonly ILog _log;

        public IsAliveService(ILog log)
        {
            _log = log;
            _httpClient = new HttpClient();
        }

        public async Task<IApiStatusObject> GetStatusAsync(string url, CancellationToken cancellationToken)
        {
            IApiStatusObject statusObject = null;
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new SimpleHttpResponseException(response.StatusCode, content);
            }
            try
            {
                var content = await response.Content.ReadAsStringAsync();
                statusObject = JsonConvert.DeserializeObject<ApiStatusObject>(content);
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync(nameof(IsAliveService), nameof(GetStatusAsync), e);
            }

            return statusObject ?? new ApiStatusObject()
            {
                Version = "undefined"
            };
        }
    }
}
