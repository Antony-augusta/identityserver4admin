using System.Threading.Tasks;
using FMCApp.Core.Dtos.Common;
using FMCApp.Entity.Entities;

namespace FMCApp.Core.Repositories
{
    public interface ILogRepository
    {
        Task<PagedList<Log>> GetLogsAsync(string search, int page = 1, int pageSize = 10);
    }
}