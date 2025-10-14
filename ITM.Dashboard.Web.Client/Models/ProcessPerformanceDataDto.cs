// ITM.Dashboard.Web.Client/Models/ProcessPerformanceDataDto.cs
using System;

namespace ITM.Dashboard.Web.Client.Models
{
    public class ProcessPerformanceDataDto
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public int MemoryUsageMB { get; set; }
    }
}
