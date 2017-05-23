using Core.Services;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Threading;
using Core.Extensions;
using Common.Log;

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

        public async Task<IApiStatusObject> GetStatus(string url, CancellationToken cancellationToken)
        {
            IApiStatusObject statusObject = null;
            var response = await _httpClient.GetAsync(url, cancellationToken);
            await response.EnsureSuccessStatusCodeAsync();

            try
            {
                var content = await response.Content.ReadAsStringAsync();
                statusObject = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiStatusObject>(content);
            }
            catch(Exception e)
            {
                await _log.WriteErrorAsync("IsAliveService", "GetStatus", "", e, DateTime.UtcNow);
            }

            return statusObject;
        }
    }
}
