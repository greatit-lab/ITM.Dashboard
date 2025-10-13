// ITM.Dashboard.Api/Models/LampLifeDto.cs
using System;

namespace ITM.Dashboard.Api.Models
{
    public class LampLifeDto
    {
        public string EqpId { get; set; }
        public string LampId { get; set; }
        public int AgeHour { get; set; }
        public int LifespanHour { get; set; }
        public DateTime? LastChanged { get; set; }
        public DateTime Ts { get; set; } // 수집 시간
    }
}
