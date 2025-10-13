// ITM.Dashboard.Web.Client/Models/LampLifeDto.cs
using System;

namespace ITM.Dashboard.Web.Client.Models
{
    public class LampLifeDto
    {
        public string EqpId { get; set; }
        public string LampId { get; set; }
        public int AgeHour { get; set; }
        public int LifespanHour { get; set; }
        public DateTime? LastChanged { get; set; }
        public DateTime Ts { get; set; }

        // ▼▼▼ [수정] "RemainingLifePercentage" -> "UsedLifePercentage" 로 변경 ▼▼▼
        /// <summary>
        /// 램프의 총 수명 대비 현재까지의 사용률(%)을 계산합니다.
        /// </summary>
        public double UsedLifePercentage
        {
            get
            {
                if (LifespanHour <= 0 || AgeHour <= 0) return 0;
                if (AgeHour >= LifespanHour) return 100;
                return ((double)AgeHour / LifespanHour) * 100;
            }
        }
    }
}
