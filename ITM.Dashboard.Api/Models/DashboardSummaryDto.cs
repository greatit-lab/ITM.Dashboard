// ITM.Dashboard.Api/Models/DashboardSummaryDto.cs
namespace ITM.Dashboard.Api.Models
{
    public class DashboardSummaryDto
    {
        public int TotalEqpCount { get; set; }
        public int OnlineAgentCount { get; set; }
        public int TodayErrorCount { get; set; }
        public long TodayDataCount { get; set; }
        public int NewAlarmCount { get; set; } // [추가] 신규 알람 수
    }
}
