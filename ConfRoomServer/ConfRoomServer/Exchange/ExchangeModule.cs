using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConfRoomServer.Logic.Exchange;

namespace ConfRoomServer.Exchange
{
    public class ExchangeModule : NancyModule
    {
        public ExchangeModule()
            : base()
        {
            registerRoutes();
        }

        private void registerRoutes()
        {
            Get["/exchange/hello"] = GetHello();
            Get["/exchange/test"] = GetTest();
        }

        // GET: /exchange/hello
        private Func<dynamic, dynamic> GetHello()
        {
            return p =>
            {
                var hi = new
                {
                    Message = "Hello World!"
                };

                return Response.AsJson(hi);
            };
        }

        // GET: /exchange/test?mailbox=emailaddress@domain.com
        private Func<dynamic, dynamic> GetTest()
        {
            return p =>
            {
                var request = new GetTestCalendarItems.GetTestCalendarItemsRequest(Request.Query.mailbox);
                var response = new GetTestCalendarItems().Execute(request);

                return Response.AsJson(response);
            };
        }
        
    } //end of class
}
