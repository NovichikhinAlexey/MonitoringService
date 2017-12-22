using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IBackUpService
    {
        Task CreateBackupAsync();

        Task RestoreBackupAsync();
    }
}
