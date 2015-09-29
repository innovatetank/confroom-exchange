using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Config
{
    public class GetConfig
    {
        public class GetConfigRequest : BaseRequest
        {
            public string Email { get; set; }
        }

        public class GetConfigResponse : BaseResponse
        {
            public Models.ConfigSettings ConfigSettings { get; set; }
        }

        private string getCompanyLogoUrl()
        {
            var companyLogoUrl = ConfigurationManager.AppSettings["companyLogoUrl"].ToString();
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("CONFROOMSERVER_LOGOURL")))
            {
                companyLogoUrl = Environment.GetEnvironmentVariable("CONFROOMSERVER_LOGOURL");
            }
            return companyLogoUrl;
        }

        private int getPollingIntervalSeconds()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["pollingIntervalSeconds"].ToString());
        }

        public GetConfigResponse Execute(GetConfigRequest request)
        {
            var response = new GetConfigResponse();

            var companyLogoUrl = getCompanyLogoUrl();
            int pollingIntervalSeconds = getPollingIntervalSeconds();
            
            var configSettings = new Models.ConfigSettings
            {
                AvailableColor = "green",
                BusyColor = "red",
                CompanyLogoUrl = companyLogoUrl,
                PollingIntervalSeconds = pollingIntervalSeconds
            };
            response.ConfigSettings = configSettings;

            return response;
        }

    }
}
