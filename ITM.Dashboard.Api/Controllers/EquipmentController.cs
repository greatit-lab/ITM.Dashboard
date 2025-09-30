// ITM.Dashboard.Api/Controllers/EquipmentController.cs
using ITM.Dashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ITM.Dashboard.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private string GetConnectionString() => new DatabaseInfo().GetConnectionString();

        [HttpGet("details")]
        public async Task<ActionResult<IEnumerable<EquipmentSpecDto>>> GetEquipmentDetails([FromQuery] string? site, [FromQuery] string? sdwt, [FromQuery] string? eqpid)
        {
            var results = new List<EquipmentSpecDto>();
            await using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            var sql = new StringBuilder(@"
                SELECT
                    a.eqpid, a.pc_name, COALESCE(s.status, 'OFFLINE') = 'ONLINE' AS is_online,
                    a.ip_address, s.last_perf_update, a.os, a.system_type, a.timezone,
                    a.mac_address, a.cpu, a.memory, a.disk, a.vga, a.type, a.locale,
                    i.system_model, i.serial_num, i.application, i.version, i.db_version
                FROM public.agent_info a
                JOIN public.ref_equipment r ON a.eqpid = r.eqpid
                LEFT JOIN public.agent_status s ON a.eqpid = s.eqpid
                LEFT JOIN public.itm_info i ON a.eqpid = i.eqpid
            ");

            var whereClauses = new List<string>();
            var cmd = new NpgsqlCommand();

            if (!string.IsNullOrEmpty(eqpid))
            {
                whereClauses.Add("r.eqpid = @eqpid");
                cmd.Parameters.AddWithValue("eqpid", eqpid);
            }
            else if (!string.IsNullOrEmpty(sdwt))
            {
                whereClauses.Add("r.sdwt = @sdwt");
                cmd.Parameters.AddWithValue("sdwt", sdwt);
            }
            else if (!string.IsNullOrEmpty(site))
            {
                whereClauses.Add("r.sdwt IN (SELECT sdwt FROM public.ref_sdwt WHERE site = @site)");
                cmd.Parameters.AddWithValue("site", site);
            }

            if (whereClauses.Count > 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
            }
            sql.Append(" ORDER BY a.eqpid;");

            cmd.Connection = conn;
            cmd.CommandText = sql.ToString();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new EquipmentSpecDto
                {
                    EqpId = reader.GetString(0),
                    PcName = reader.IsDBNull(1) ? "N/A" : reader.GetString(1),
                    IsOnline = reader.GetBoolean(2),
                    IpAddress = reader.IsDBNull(3) ? "N/A" : reader.GetString(3),
                    LastContact = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Os = reader.IsDBNull(5) ? "N/A" : reader.GetString(5),
                    SystemType = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    Timezone = reader.IsDBNull(7) ? "N/A" : reader.GetString(7),
                    MacAddress = reader.IsDBNull(8) ? "N/A" : reader.GetString(8),
                    Cpu = reader.IsDBNull(9) ? "N/A" : reader.GetString(9),
                    Memory = reader.IsDBNull(10) ? "N/A" : reader.GetString(10),
                    Disk = reader.IsDBNull(11) ? "N/A" : reader.GetString(11),
                    Vga = reader.IsDBNull(12) ? "N/A" : reader.GetString(12),
                    Type = reader.IsDBNull(13) ? "N/A" : reader.GetString(13),
                    Locale = reader.IsDBNull(14) ? "N/A" : reader.GetString(14),
                    SystemModel = reader.IsDBNull(15) ? "N/A" : reader.GetString(15),
                    SerialNum = reader.IsDBNull(16) ? "N/A" : reader.GetString(16),
                    Application = reader.IsDBNull(17) ? "N/A" : reader.GetString(17),
                    Version = reader.IsDBNull(18) ? "N/A" : reader.GetString(18),
                    DbVersion = reader.IsDBNull(19) ? "N/A" : reader.GetString(19)
                });
            }
            return Ok(results);
        }

        [HttpGet("eqpids")]
        public async Task<ActionResult<IEnumerable<string>>> GetEquipmentIds([FromQuery] string? site, [FromQuery] string? sdwt)
        {
            var results = new List<string>();
            if (string.IsNullOrEmpty(site) && string.IsNullOrEmpty(sdwt)) return Ok(results);

            await using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            var sql = new StringBuilder("SELECT r.eqpid FROM public.ref_equipment r");
            var whereClauses = new List<string>();
            var cmd = new NpgsqlCommand();

            if (!string.IsNullOrEmpty(sdwt))
            {
                whereClauses.Add("r.sdwt = @sdwt");
                cmd.Parameters.AddWithValue("sdwt", sdwt);
            }
            else if (!string.IsNullOrEmpty(site))
            {
                whereClauses.Add("r.sdwt IN (SELECT sdwt FROM public.ref_sdwt WHERE site = @site)");
                cmd.Parameters.AddWithValue("site", site);
            }

            if (whereClauses.Count > 0)
            {
                sql.Append(" WHERE ").Append(string.Join(" AND ", whereClauses));
            }
            sql.Append(" ORDER BY r.eqpid;");

            cmd.Connection = conn;
            cmd.CommandText = sql.ToString();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(reader.GetString(0));
            }
            return Ok(results);
        }
    }
}
