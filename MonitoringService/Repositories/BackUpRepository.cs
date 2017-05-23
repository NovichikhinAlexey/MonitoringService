using AzureStorage;
using Common.Log;
using Core.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class BackUpRepository : IBackUpRepository
    {
        private readonly IBlobStorage _blobStorage;
        private readonly string _blobName;
        private readonly ILog _log;

        public BackUpRepository(string blobName, IBlobStorage blobStorage, ILog log)
        {
            _log = log;
            _blobName = blobName;
            _blobStorage = blobStorage;
        }

        public async Task<IBackUp> GetAsync(string key)
        {
            try
            {
                var stream = await _blobStorage.GetAsync(_blobName, key);
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    string result = await reader.ReadToEndAsync();
                    IBackUp backUp = Newtonsoft.Json.JsonConvert.DeserializeObject<IBackUp>(result);

                    return backUp;
                }
            }
            catch (Exception e)
            {
                await _log.WriteErrorAsync("BackUpRepository", "GetAsync", "", e, DateTime.UtcNow);
                return null;
            }
        }

        public async Task InsertAsync(IBackUp backUp)
        {
            string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(backUp);
            byte[] byteArray = Encoding.UTF8.GetBytes(serialized);

            await _blobStorage.SaveBlobAsync(_blobName, backUp.Key, byteArray);
        }
    }
}
