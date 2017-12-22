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
        private const int _maxContentLength = 30000;
        private readonly HttpClient _httpClient;
        private readonly ILog _log;

        public IsAliveService(ILog log)
        {
            _log = log;
            _httpClient = new HttpClient();
        }

        public async Task<IApiStatusObject> GetStatusAsync(string url, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            string content = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                if (content.Length > _maxContentLength)
                    content = content.Substring(0, _maxContentLength);
                throw new SimpleHttpResponseException(response.StatusCode, content);
            }

            IApiStatusObject statusObject = null;
            try
            {
                statusObject = JsonConvert.DeserializeObject<ApiStatusObject>(content);
            }
            catch (Exception)
            {
                // hiding such exception, cause we can get response from our web-sites with a page content here.
            }

            return statusObject ?? new ApiStatusObject()
            {
                Version = "undefined"
            };
        }
    }
}
