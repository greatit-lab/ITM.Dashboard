// ITM.Dashboard.Api/Models/ProcessMemoryDataDto.cs
using System;

namespace ITM.Dashboard.Api.Models
{
    public class ProcessMemoryDataDto
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public int MemoryUsageMB { get; set; }
    }
}
