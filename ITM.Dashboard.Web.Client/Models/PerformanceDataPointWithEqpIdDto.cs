// ITM.Dashboard.Web.Client/Models/PerformanceDataPointWithEqpIdDto.cs
using System;

namespace ITM.Dashboard.Web.Client.Models
{
    public class PerformanceDataPointWithEqpIdDto
    {
        public string EqpId { get; set; }
        public DateTime Timestamp { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }

        // ▼▼▼ [추가] API에서 보내주는 새로운 속성들을 정의합니다. ▼▼▼
        public double CpuTemp { get; set; }
        public double GpuTemp { get; set; }
        public double FanSpeed { get; set; }
    }
}
