using Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Repositories
{
    public interface IBackUp
    {
        string Key { get; set; }
        string SerializedObject { get; set; }
    }

    public class BackUp : IBackUp
    {
        public string Key { get; set; }
        public string SerializedObject { get; set; }
    }

    public interface IBackUpRepository
    {
        Task InsertAsync(IBackUp backUp);
        Task<IBackUp> GetAsync(string key);
    }
}
