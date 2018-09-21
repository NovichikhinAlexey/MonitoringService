using System.Threading.Tasks;

namespace Core.Services
{
    public interface IBackUpService
    {
        Task CreateBackupAsync();

        Task RestoreBackupAsync();
    }
}
