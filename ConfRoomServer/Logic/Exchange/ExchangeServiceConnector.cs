using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer.Logic.Exchange
{
    public class ExchangeServiceConnector
    {
        public static string DomainName {
            get
            {
                var domain = Environment.GetEnvironmentVariable("CONFROOMSERVER_DOMAIN");
                return domain;
            }
        }

        public static ExchangeService GetService(string emailAddress)
        {
            //var uri = GetExchangeUri();

            var userName = Environment.GetEnvironmentVariable("CONFROOMSERVER_USER");
            var password = Environment.GetEnvironmentVariable("CONFROOMSERVER_PASSWORD");
            var domain = DomainName;

            var service = new ExchangeService();
            //service.Url = new Uri(uri);
            service.AutodiscoverUrl(emailAddress);
            service.Credentials = new WebCredentials(userName, password, domain);
            System.Net.ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);
            return service;
        }

        //public static string GetExchangeUri()
        //{
        //    string uri = "";
        //    var uriAppConfig = ConfigurationManager.AppSettings["exchangeUri"].ToString();
        //    var uriEnv = Environment.GetEnvironmentVariable("CONFROOMSERVER_EXCHANGEURI");
        //    if (!string.IsNullOrWhiteSpace(uriEnv))
        //    {
        //        uri = uriEnv;
        //    }
        //    else
        //    {
        //        uri = uriAppConfig;
        //    }

        //    return uri;
        //}
    }
}
