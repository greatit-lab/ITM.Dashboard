// ITM.Dashboard.Web.Client/Models/ProcessMemoryDataDto.cs
using System;

namespace ITM.Dashboard.Web.Client.Models
{
    public class ProcessMemoryDataDto
    {
        public DateTime Timestamp { get; set; }
        public string ProcessName { get; set; }
        public int MemoryUsageMB { get; set; }
    }
}
