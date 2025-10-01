// ITM.Dashboard.Api/Controllers/LotUniformityController.cs
using ITM.Dashboard.Api;
using ITM.Dashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class LotUniformityController : ControllerBase
{
    [HttpGet("trend")]
    public async Task<ActionResult<IEnumerable<LotUniformitySeriesDto>>> GetLotUniformityTrend(
        [FromQuery] string lotId,
        [FromQuery] string cassetteRcp,
        [FromQuery] string stageGroup,
        [FromQuery] string? film,
        [FromQuery] string yAxisMetric,
        [FromQuery] string eqpid,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (string.IsNullOrEmpty(lotId) || string.IsNullOrEmpty(yAxisMetric))
        {
            return BadRequest("Invalid parameters.");
        }

        var sqlBuilder = new StringBuilder($@"
            SELECT waferid, point, ""{yAxisMetric}"", x, y, dierow, diecol
            FROM public.plg_wf_flat
            WHERE lotid = @lotId
              AND cassettercp = @cassetteRcp
              AND stagegroup = @stageGroup
              AND eqpid = @eqpid
              AND serv_ts BETWEEN @startDate AND @endDate
              AND point IS NOT NULL
              AND ""{yAxisMetric}"" IS NOT NULL
        ");

        if (!string.IsNullOrEmpty(film))
        {
            sqlBuilder.Append(" AND film = @film");
        }

        sqlBuilder.Append(" ORDER BY waferid, point;");

        var results = new Dictionary<int, LotUniformitySeriesDto>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sqlBuilder.ToString(), conn);
        cmd.Parameters.AddWithValue("lotId", lotId);
        cmd.Parameters.AddWithValue("cassetteRcp", cassetteRcp);
        cmd.Parameters.AddWithValue("stageGroup", stageGroup);
        cmd.Parameters.AddWithValue("eqpid", eqpid);
        cmd.Parameters.AddWithValue("startDate", startDate.Date);
        cmd.Parameters.AddWithValue("endDate", endDate.Date.AddDays(1).AddTicks(-1));

        if (!string.IsNullOrEmpty(film))
        {
            cmd.Parameters.AddWithValue("film", film);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var waferId = reader.GetInt32(0);
            if (!results.ContainsKey(waferId))
            {
                results[waferId] = new LotUniformitySeriesDto { WaferId = waferId };
            }

            results[waferId].DataPoints.Add(new LotUniformityDataPointDto
            {
                Point = reader.GetInt32(1),
                Value = reader.GetDouble(2),
                X = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                Y = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                DieRow = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                DieCol = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6)
            });
        }

        return Ok(results.Values.ToList());
    }
}
