using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Get["/exchange/test"] = GetTest();
        }

        private Func<dynamic, dynamic> GetTest()
        {
            return p =>
            {
                var testObject = new
                {
                    TestProperty = "Hello World"
                };

                return Response.AsJson(testObject);
            };
        }
        
    } //end of class
}
