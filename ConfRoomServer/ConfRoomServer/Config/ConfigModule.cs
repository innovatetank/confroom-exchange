using ConfRoomServer.Logic.Config;
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

        // GET: /config?email=mailboxemail@domain.com
        private Func<dynamic, dynamic> GetConfig()
        {
            return p =>
            {
                var request = new GetConfig.GetConfigRequest
                {
                    Email = Request.Query.email
                };
                var response = new GetConfig().Execute(request);
                return Response.AsJson(response);
            };
        }

    } //end of class
}
