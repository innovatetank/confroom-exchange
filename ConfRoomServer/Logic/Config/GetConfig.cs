using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
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

        private string getSettingFromAppSettingsOverriddenByEnvironment(
            string appSettingsName, string environmentVariableName, string defaultValue)
        {
            var resultValue = ConfigurationManager.AppSettings[appSettingsName].ToString();
            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(environmentVariableName)))
            {
                resultValue = Environment.GetEnvironmentVariable(environmentVariableName);
            }

            if (string.IsNullOrWhiteSpace(resultValue)) resultValue = defaultValue;

            return resultValue;
        }

        private string getCompanyLogoUrl()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "companyLogoUrl",
                environmentVariableName: "CONFROOMSERVER_LOGOURL",
                defaultValue: "");
        }

        private int getPollingIntervalSeconds()
        {
            var setting = getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "pollingIntervalSeconds",
                environmentVariableName: "CONFROOMSERVER_POLLINGINTERVAL",
                defaultValue: "60");

            return Convert.ToInt32(setting);
        }

        private string getAvailableColor()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "availableColor",
                environmentVariableName: "CONFROOMSERVER_AVAILABLECOLOR",
                defaultValue: "#8DC63F");
        }

        private string getBusyColor()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "busyColor",
                environmentVariableName: "CONFROOMSERVER_BUSYCOLOR",
                defaultValue: "#C20000");
        }

        private string getAvailableRoomText()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "availableRoomText",
                environmentVariableName: "CONFROOMSERVER_AVAILABLEROOMTEXT",
                defaultValue: "AVAILABLE");
        }

        private string getBusyRoomText()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "busyRoomText",
                environmentVariableName: "CONFROOMSERVER_BUSYROOMTEXT",
                defaultValue: "IN USE");
        }

        private string getAdminMenuPassword()
        {
            return getSettingFromAppSettingsOverriddenByEnvironment(
                appSettingsName: "adminMenuPassword",
                environmentVariableName: "CONFROOMSERVER_ADMINMENUPWD",
                defaultValue: "12345");
        }

        public GetConfigResponse Execute(GetConfigRequest request)
        {
            var response = new GetConfigResponse();

            var configSettings = new Models.ConfigSettings
            {
                AvailableColor = getAvailableColor(),
                BusyColor = getBusyColor(),
                CompanyLogoImage = getCompanyLogoImage(),
                PollingIntervalSeconds = getPollingIntervalSeconds(),
                AvailableRoomText = getAvailableRoomText(),
                BusyRoomText = getBusyRoomText(),
                AdminMenuPassword = getAdminMenuPassword()
            };
            response.ConfigSettings = configSettings;
            response.Success = true;

            return response;
        }

        private string getCompanyLogoImage()
        {
            var companyLogoUrl = getCompanyLogoUrl();

            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;

                var byteArray = client.DownloadData(companyLogoUrl);
                return "data:image/jpeg;base64," + Convert.ToBase64String(byteArray);
            }
        }

    }
}
