using System.Threading.Tasks;
using FMCApp.Core.Dtos.Log;

namespace FMCApp.Core.Services
{
    public interface ILogService
    {
        Task<LogsDto> GetLogsAsync(string search, int page = 1, int pageSize = 10);
    }
}