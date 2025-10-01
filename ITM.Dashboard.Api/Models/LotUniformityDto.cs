// ITM.Dashboard.Api/Models/LotUniformityDto.cs
using System.Collections.Generic;

namespace ITM.Dashboard.Api.Models
{
    public class LotUniformityDataPointDto
    {
        public int Point { get; set; } 
        public double Value { get; set; }
        public double X { get; set; }
        public double Y { get; set; }

        // ▼▼▼ [수정] int?를 double?로 변경합니다. ▼▼▼
        public double? DieRow { get; set; }
        public double? DieCol { get; set; }
        // ▲▲▲ [수정] 여기까지 ▲▲▲
    }

    public class LotUniformitySeriesDto
    {
        public int WaferId { get; set; }
        public List<LotUniformityDataPointDto> DataPoints { get; set; } = new();
    }
}
