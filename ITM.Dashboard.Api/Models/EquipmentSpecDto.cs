// ITM.Dashboard.Api/Models/EquipmentSpecDto.cs
using System;

namespace ITM.Dashboard.Api.Models
{
    public class EquipmentSpecDto
    {
        // from agent_info
        public string EqpId { get; set; }
        public string Type { get; set; }
        public string PcName { get; set; }
        public bool IsOnline { get; set; }
        public string IpAddress { get; set; }
        public string MacAddress { get; set; }
        public string Os { get; set; }
        public string SystemType { get; set; }
        public string Locale { get; set; }
        public string Timezone { get; set; }
        public string Cpu { get; set; }
        public string Memory { get; set; }
        public string Disk { get; set; }
        public string Vga { get; set; }
        public DateTime? LastContact { get; set; }

        // from itm_info
        public string SystemModel { get; set; }
        public string SerialNum { get; set; }
        public string Application { get; set; }
        public string Version { get; set; }
        public string DbVersion { get; set; }
    }
}
