// ITM.Dashboard.Api/Controllers/LampLifeController.cs
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
    public class LampLifeController : ControllerBase
    {
        private string GetConnectionString() => new DatabaseInfo().GetConnectionString();

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LampLifeDto>>> GetLampLifeData(
            [FromQuery] string site, [FromQuery] string? sdwt = null, [FromQuery] string? eqpid = null)
        {
            var results = new List<LampLifeDto>();
            await using var conn = new NpgsqlConnection(GetConnectionString());
            await conn.OpenAsync();

            var sqlBuilder = new StringBuilder(@"
                SELECT T1.eqpid, T1.lamp_id, T1.age_hour, T1.lifespan_hour, T1.last_changed, T1.serv_ts
                FROM public.eqp_lamp_life AS T1
                INNER JOIN (
                    SELECT eqpid, lamp_id, MAX(serv_ts) as max_serv_ts
                    FROM public.eqp_lamp_life
                    GROUP BY eqpid, lamp_id
                ) AS T2 ON T1.eqpid = T2.eqpid AND T1.lamp_id = T2.lamp_id AND T1.serv_ts = T2.max_serv_ts
                INNER JOIN public.ref_equipment AS T3 ON T1.eqpid = T3.eqpid
                INNER JOIN public.ref_sdwt AS T4 ON T3.sdwt = T4.sdwt
                WHERE T4.is_use = 'Y'");

            await using var cmd = new NpgsqlCommand();

            if (!string.IsNullOrEmpty(eqpid))
            {
                sqlBuilder.Append(" AND T1.eqpid = @eqpid");
                cmd.Parameters.AddWithValue("eqpid", eqpid);
            }
            else if (!string.IsNullOrEmpty(sdwt))
            {
                sqlBuilder.Append(" AND T3.sdwt = @sdwt");
                cmd.Parameters.AddWithValue("sdwt", sdwt);
            }
            else if (!string.IsNullOrEmpty(site))
            {
                sqlBuilder.Append(" AND T4.site = @site");
                cmd.Parameters.AddWithValue("site", site);
            }

            sqlBuilder.Append(" ORDER BY T1.eqpid, T1.lamp_id;");

            cmd.Connection = conn;
            cmd.CommandText = sqlBuilder.ToString();

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(new LampLifeDto
                {
                    EqpId = reader.GetString(0),
                    LampId = reader.GetString(1),
                    AgeHour = reader.GetInt32(2),
                    LifespanHour = reader.GetInt32(3),
                    LastChanged = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                    Ts = reader.GetDateTime(5)
                });
            }

            return Ok(results);
        }
    }
}
