using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;

namespace Lykke.MonitoringServiceApiCaller
{
	/// <summary>
    /// Used to prevent memory leak in RetryPolicy
    /// </summary>
    internal partial class MonitoringService
    {
        public MonitoringService(Uri baseUri, HttpClient client) : base(client)
        {
            Initialize();

            BaseUri = baseUri ?? throw new ArgumentNullException("baseUri");
        }
    }
}

