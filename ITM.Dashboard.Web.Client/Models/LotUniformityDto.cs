// ITM.Dashboard.Web.Client/Models/LotUniformityDto.cs
using System.Collections.Generic;

public class LotUniformityDataPointDto
{
    public int Point { get; set; } 
    public double Value { get; set; }
    public double X { get; set; }
    public double Y { get; set; }

    public double? DieRow { get; set; }
    public double? DieCol { get; set; }
}

public class LotUniformitySeriesDto
{
    public int WaferId { get; set; }
    public List<LotUniformityDataPointDto> DataPoints { get; set; } = new();
}

public class LotUniformityChartData
{
    public int WaferId { get; set; }
    public int Point { get; set; }
    public double Value { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double? DieRow { get; set; }
    public double? DieCol { get; set; }

    public double? MapX { get; set; }
    public double? MapY { get; set; }
}
