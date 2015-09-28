using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Config
{
    public class ConfigModule : NancyModule
    {
        public ConfigModule()
            : base()
        {
            registerRoutes();
        }

        private void registerRoutes()
        {
            Get["/config"] = GetConfig();
        }

        private Func<dynamic, dynamic> GetConfig()
        {
            return p =>
            {
                var config = new {
                    AvailableColor = "green",
                    BusyColor = "red"
                };
                return Response.AsJson(config);
            };
        }

    } //end of class
}
