using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Config.Models
{
    [Serializable]
    public class ConfigSettings
    {
        public string AvailableColor { get; set; }
        public string BusyColor { get; set; }
        //public string CompanyLogoUrl { get; set; }
        public string CompanyLogoImage { get; set; }
        public int PollingIntervalSeconds { get; set; }
    }
}
