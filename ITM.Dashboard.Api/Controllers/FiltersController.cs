// ITM.Dashboard.Api/Controllers/FiltersController.cs
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ITM.Dashboard.Api.Models;
using ITM.Dashboard.Api;
using System.Text;
using System.Linq;

[Route("api/[controller]")]
[ApiController]
public class FiltersController : ControllerBase
{
    // ▼▼▼ [추가] 페이지별 데이터 소스에 따른 EQP ID 조회를 위한 공용 메서드 ▼▼▼
    private async Task<ActionResult<IEnumerable<string>>> GetEqpIdsBySource(string sourceTable, string? sdwt, string? site)
    {
        if (string.IsNullOrEmpty(sourceTable) || (string.IsNullOrEmpty(sdwt) && string.IsNullOrEmpty(site)))
        {
            return BadRequest("Source table and either SDWT or Site is required.");
        }

        var results = new List<string>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();

        // SQL Injection 방지를 위해 허용된 테이블 이름 목록을 사용합니다.
        var allowedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "public.plg_wf_flat",
            "public.eqp_perf",
            "public.plg_error",
            "public.plg_prealign"
        };
        if (!allowedTables.Contains(sourceTable))
        {
            return BadRequest("Invalid source table specified.");
        }

        var sql = new StringBuilder($@"
            SELECT DISTINCT T1.eqpid
            FROM public.ref_equipment AS T1
            INNER JOIN {sourceTable} AS T2 ON UPPER(TRIM(T1.eqpid)) = UPPER(TRIM(T2.eqpid))
            INNER JOIN public.ref_sdwt AS T3 ON UPPER(TRIM(T1.sdwt)) = UPPER(TRIM(T3.sdwt))
            WHERE T3.is_use = 'Y'");

        if (!string.IsNullOrEmpty(sdwt))
        {
            sql.Append(" AND UPPER(TRIM(T1.sdwt)) = UPPER(TRIM(@sdwt))");
        }
        else if (!string.IsNullOrEmpty(site))
        {
            sql.Append(" AND UPPER(TRIM(T3.site)) = UPPER(TRIM(@site))");
        }
        sql.Append(" ORDER BY T1.eqpid;");

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        if (!string.IsNullOrEmpty(sdwt))
        {
            cmd.Parameters.AddWithValue("sdwt", sdwt);
        }
        if (!string.IsNullOrEmpty(site))
        {
            cmd.Parameters.AddWithValue("site", site);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(reader.GetString(0));
        }
        return Ok(results);
    }

    // Site 목록 조회 API (변경 없음)
    [HttpGet("sites")]
    public async Task<ActionResult<IEnumerable<string>>> GetSites()
    {
        var results = new List<string>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();
        var sql = "SELECT DISTINCT site FROM public.ref_sdwt WHERE is_use = 'Y' ORDER BY site;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) { results.Add(reader.GetString(0)); }
        return Ok(results);
    }

    // 특정 Site에 속한 SDWT 목록 조회 API (변경 없음)
    [HttpGet("sdwts/{site}")]
    public async Task<ActionResult<IEnumerable<string>>> GetSdwts(string site)
    {
        var results = new List<string>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();
        var sql = "SELECT DISTINCT sdwt FROM public.ref_sdwt WHERE site = @site AND is_use = 'Y' ORDER BY sdwt;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("site", site);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync()) { results.Add(reader.GetString(0)); }
        return Ok(results);
    }

    // ▼▼▼ [수정] 기존 GetEqpids는 WaferFlatData 페이지를 위해 유지하되, 내부적으로 공용 메서드를 호출하도록 변경 ▼▼▼
    [HttpGet("eqpids/{sdwt?}")]
    public Task<ActionResult<IEnumerable<string>>> GetEqpids(string sdwt)
    {
        return GetEqpIdsBySource("public.plg_wf_flat", sdwt, null);
    }

    [HttpGet("eqpidsbysite/{site}")]
    public Task<ActionResult<IEnumerable<string>>> GetEqpidsBySite(string site)
    {
        return GetEqpIdsBySource("public.plg_wf_flat", null, site);
    }

    // ▼▼▼ [추가] 각 페이지를 위한 새로운 EQP ID 조회 API ▼▼▼
    [HttpGet("eqpids/performance/{sdwt}")]
    public Task<ActionResult<IEnumerable<string>>> GetPerformanceEqpIds(string sdwt) => GetEqpIdsBySource("public.eqp_perf", sdwt, null);

    [HttpGet("eqpidsbysite/performance/{site}")]
    public Task<ActionResult<IEnumerable<string>>> GetPerformanceEqpIdsBySite(string site) => GetEqpIdsBySource("public.eqp_perf", null, site);

    [HttpGet("eqpids/error/{sdwt}")]
    public Task<ActionResult<IEnumerable<string>>> GetErrorEqpIds(string sdwt) => GetEqpIdsBySource("public.plg_error", sdwt, null);

    [HttpGet("eqpidsbysite/error/{site}")]
    public Task<ActionResult<IEnumerable<string>>> GetErrorEqpIdsBySite(string site) => GetEqpIdsBySource("public.plg_error", null, site);

    [HttpGet("eqpids/prealign/{sdwt}")]
    public Task<ActionResult<IEnumerable<string>>> GetPreAlignEqpIds(string sdwt) => GetEqpIdsBySource("public.plg_prealign", sdwt, null);

    [HttpGet("eqpidsbysite/prealign/{site}")]
    public Task<ActionResult<IEnumerable<string>>> GetPreAlignEqpIdsBySite(string site) => GetEqpIdsBySource("public.plg_prealign", null, site);

    // 특정 EQPID의 데이터 기간(최소/최대) 조회 API (변경 없음)
    [HttpGet("daterange")]
    public async Task<ActionResult<DateRangeDto>> GetDataDateRange([FromQuery] string? eqpid)
    {
        if (string.IsNullOrEmpty(eqpid)) { return Ok(new DateRangeDto()); }
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();
        var sql = @"SELECT MIN(serv_ts), MAX(serv_ts) FROM public.plg_wf_flat WHERE eqpid = @eqpid;";
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("eqpid", eqpid);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync() && !reader.IsDBNull(0) && !reader.IsDBNull(1))
        {
            return Ok(new DateRangeDto { MinDate = reader.GetDateTime(0), MaxDate = reader.GetDateTime(1) });
        }
        return Ok(new DateRangeDto());
    }

    [HttpGet("availablemetrics")]
    public async Task<ActionResult<IEnumerable<string>>> GetAvailableMetrics(
        [FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] string? lotId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
    {
        var availableMetrics = new List<string>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();

        // 1. [수정] DB의 cgf_lot_uniformity_metrics 테이블에서 제외할 컬럼 목록을 가져옵니다.
        var excludedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var exclusionSql = "SELECT metric_name FROM public.cgf_lot_uniformity_metrics WHERE is_excluded = 'Y';";
        await using (var cmd = new NpgsqlCommand(exclusionSql, conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                excludedColumns.Add(reader.GetString(0));
            }
        }

        // 2. plg_wf_flat 테이블의 모든 숫자 타입 컬럼 목록을 가져옵니다.
        var allNumericColumns = new List<string>();
        var columnSql = @"
            SELECT column_name
            FROM information_schema.columns
            WHERE table_schema = 'public'
              AND table_name   = 'plg_wf_flat'
              AND data_type IN ('integer', 'bigint', 'smallint', 'numeric', 'real', 'double precision');";

        await using (var cmd = new NpgsqlCommand(columnSql, conn))
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                allNumericColumns.Add(reader.GetString(0));
            }
        }

        // 3. 코드에서 두 목록을 비교하여 제외되지 않은 숫자 컬럼만 필터링합니다.
        var potentialMetrics = allNumericColumns.Where(c => !excludedColumns.Contains(c)).ToList();

        // 4. 현재 필터 조건으로 WHERE 절 구성
        var whereClauses = new List<string>();
        var parameters = new Dictionary<string, object>();

        void AddCondition(string? value, string columnName)
        {
            if (!string.IsNullOrEmpty(value))
            {
                whereClauses.Add($"{columnName} = @{columnName}");
                parameters[columnName] = value;
            }
        }
        AddCondition(eqpid, "eqpid");
        AddCondition(lotId, "lotid");
        AddCondition(cassetteRcp, "cassettercp");
        AddCondition(stageGroup, "stagegroup");
        AddCondition(film, "film");

        if (startDate.HasValue) { whereClauses.Add("serv_ts >= @startDate"); parameters["startDate"] = startDate.Value; }
        if (endDate.HasValue) { whereClauses.Add("serv_ts <= @endDate"); parameters["endDate"] = endDate.Value.AddDays(1).AddTicks(-1); }

        string whereQuery = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";

        // 5. 최종 후보 컬럼들에 대해 실제 데이터가 있는지 확인
        foreach (var metric in potentialMetrics)
        {
            var checkSql = $"SELECT 1 FROM public.plg_wf_flat {whereQuery} AND \"{metric}\" IS NOT NULL LIMIT 1;";
            await using var checkCmd = new NpgsqlCommand(checkSql, conn);
            foreach (var p in parameters)
            {
                checkCmd.Parameters.AddWithValue(p.Key, p.Value);
            }

            var result = await checkCmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                availableMetrics.Add(metric);
            }
        }

        return Ok(availableMetrics.OrderBy(m => m));
    }

    private async Task<ActionResult<IEnumerable<string>>> GetFilteredDistinctValues(
        string targetColumn,
        [FromQuery] string eqpid,
        [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate,
        [FromQuery] string? lotId, [FromQuery] int? waferId,
        [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp,
        [FromQuery] string? stageGroup, [FromQuery] string? film)
    {
        var results = new List<string>();
        var dbInfo = DatabaseInfo.CreateDefault();
        await using var conn = new NpgsqlConnection(dbInfo.GetConnectionString());
        await conn.OpenAsync();

        var whereClauses = new List<string> { "eqpid = @eqpid", $"{targetColumn} IS NOT NULL" };
        var parameters = new Dictionary<string, object> { { "eqpid", eqpid } };

        void AddCondition(string? value, string columnName, bool isNumeric = false)
        {
            if (string.IsNullOrEmpty(value) || columnName == targetColumn) return;
            if (isNumeric && int.TryParse(value, out var numValue))
            {
                whereClauses.Add($"{columnName} = @{columnName}");
                parameters[columnName] = numValue;
            }
            else if (!isNumeric)
            {
                whereClauses.Add($"{columnName} = @{columnName}");
                parameters[columnName] = value;
            }
        }

        if (startDate.HasValue) { whereClauses.Add("serv_ts >= @startDate"); parameters["startDate"] = startDate.Value; }
        if (endDate.HasValue) { whereClauses.Add("serv_ts <= @endDate"); parameters["endDate"] = endDate.Value.AddDays(1).AddTicks(-1); }

        AddCondition(lotId, "lotid");
        AddCondition(cassetteRcp, "cassettercp");
        AddCondition(stageRcp, "stagercp");
        AddCondition(stageGroup, "stagegroup");
        AddCondition(film, "film");

        if (waferId.HasValue && "waferid" != targetColumn)
        {
            whereClauses.Add("waferid = @waferid");
            parameters["waferid"] = waferId.Value;
        }

        var whereQuery = "WHERE " + string.Join(" AND ", whereClauses);
        var sql = $"SELECT DISTINCT {targetColumn} FROM public.plg_wf_flat {whereQuery} ORDER BY {targetColumn};";

        await using var cmd = new NpgsqlCommand(sql, conn);
        foreach (var p in parameters) { cmd.Parameters.AddWithValue(p.Key, p.Value); }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
            {
                results.Add(reader[0].ToString()!);
            }
        }
        return Ok(results);
    }

    [HttpGet("cassettercps")]
    public Task<ActionResult<IEnumerable<string>>> GetCassetteRcps([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("cassettercp", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);

    [HttpGet("stagercps")]
    public Task<ActionResult<IEnumerable<string>>> GetStageRcps([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("stagercp", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);

    [HttpGet("stagegroups")]
    public Task<ActionResult<IEnumerable<string>>> GetStageGroups([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("stagegroup", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);

    [HttpGet("films")]
    public Task<ActionResult<IEnumerable<string>>> GetFilms([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("film", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);

    [HttpGet("lotids")]
    public Task<ActionResult<IEnumerable<string>>> GetLotIds([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("lotid", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);

    [HttpGet("waferids")]
    public Task<ActionResult<IEnumerable<string>>> GetWaferIds([FromQuery] string eqpid, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] string? lotId, [FromQuery] int? waferId, [FromQuery] string? cassetteRcp, [FromQuery] string? stageRcp, [FromQuery] string? stageGroup, [FromQuery] string? film)
        => GetFilteredDistinctValues("waferid", eqpid, startDate, endDate, lotId, waferId, cassetteRcp, stageRcp, stageGroup, film);
}
