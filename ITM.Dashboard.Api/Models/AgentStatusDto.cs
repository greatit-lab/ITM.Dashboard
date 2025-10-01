// ITM.Dashboard.Api/Models/AgentStatusDto.cs
using System;

namespace ITM.Dashboard.Api.Models
{
    public class AgentStatusDto
    {
        public string EqpId { get; set; }
        public bool IsOnline { get; set; }
        public DateTime? LastContact { get; set; }
        public string PcName { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public string AppVersion { get; set; }
        public string Type { get; set; }
        public string IpAddress { get; set; }
        public string Os { get; set; }
        public string SystemType { get; set; }
        public string Locale { get; set; }
        public string Timezone { get; set; }
        public int TodayAlarmCount { get; set; }
        public double? ClockDrift { get; set; } // [추가] 시간 차이 (초)
    }
}
