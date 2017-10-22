using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.ServiceModel;


namespace Taxi.Communication.Server.Host.Hosts
{
    public abstract class AHost
    {
        protected ILog _log;
        protected ServiceHost _host;
        private string _hostName;
        protected Uri _baseAddress;

        protected AHost(ILog log, string hostName, Uri baseAddress)
        {
            _log = log;
            _host = null;
            _hostName = hostName;
            _baseAddress = baseAddress;
        }

        public void Open()
        {
            if (_host == null)
            {
                _host = this.createService();
                _host.Faulted += new EventHandler(_host_Faulted);
            }
            _host.Open();
        }

        private void _host_Faulted(object sender, EventArgs e)
        {
            try
            {
                this.doCleanUp(sender, e);
                _host.Abort();
                _host = null;
                _log.Error("ERROR: Host Faulted restarting " + _hostName);
                this.Open();
            }
            catch (Exception ex)
            {
                _log.Fatal("Unable to handle Faulted", ex);
            }
        }


        public void Close()
        {
            if (_host != null)
            {
                _host.Close();
            }
        }

        protected abstract ServiceHost createService();
        public abstract void doCleanUp(object sender, EventArgs args);
    }
}
