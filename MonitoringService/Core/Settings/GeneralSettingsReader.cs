using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Core.Settings
{
    public static class GeneralSettingsReader
    {
        public static T ReadGeneralSettings<T>(string url)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(url);
            var settingsData = httpClient.GetStringAsync("").Result;

            return Lykke.SettingsReader.SettingsProcessor.Process<T>(settingsData);
        }
    }

    public class SettingsWrapper
    {
        public BaseSettings MonitoringService { get; set; }
    }
}
