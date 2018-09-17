using System.Collections.Generic;

namespace FMCApp.Core.Dtos.Log
{
    public class LogsDto
    {
        public LogsDto()
        {
            Logs = new List<LogDto>();
        }

        public List<LogDto> Logs { get; set; }

        public int TotalCount { get; set; }

        public int PageSize { get; set; }
    }
}
