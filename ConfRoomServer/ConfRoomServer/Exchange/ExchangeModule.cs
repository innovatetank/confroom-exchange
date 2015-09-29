﻿using Nancy;
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
            Get["/exchange/items"] = GetItems();
            Get["/exchange/mailbox"] = GetMailbox();
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

        // GET: /exchange/items?mailbox=emailaddress@domain.com
        private Func<dynamic, dynamic> GetItems()
        {
            return p =>
            {
                var request = new GetCalendarItems.GetCalendarItemsRequest(
                    mailboxEmail: Request.Query.mailbox,
                    daysToLoad: 1);
                var response = new GetCalendarItems().Execute(request);

                return Response.AsJson(response);
            };
        }

        // GET: /exchange/mailbox?email=emailaddress@domain.com
        private Func<dynamic, dynamic> GetMailbox()
        {
            return p =>
            {
                var request = new GetMailboxInfo.GetMailboxInfoRequest(
                    email: Request.Query.email);
                var response = new GetMailboxInfo().Execute(request);

                return Response.AsJson(response);
            };
        }

    } //end of class
}
