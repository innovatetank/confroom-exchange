using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ConfRoomServer
{
    public partial class ConfRoomServerService : ServiceBase
    {
        private NancyHost host;

        public ConfRoomServerService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var uri = ConfigurationManager.AppSettings["listenerUri"].ToString();

            host = new NancyHost(new CustomBootstrapper(), new Uri(uri));
            host.Start();
        }

        protected override void OnStop()
        {
            host.Stop();
            host.Dispose();
            host = null;
        }
    }
}
