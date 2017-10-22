using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.ServiceModel;


namespace Taxi.Communication.Server.Host.Hosts
{
    public class IPphoneExchangeHost : AHost
    {

        private IPphoneExchangeService _phoneExchangeService;

        public IPphoneExchangeHost(ILog log, IPphoneExchangeService phoneExchangeService, Uri baseAddress)
            : base(log, "IPphoneExchangeHost", baseAddress)
        {
            this._phoneExchangeService = phoneExchangeService;
        }

        protected override System.ServiceModel.ServiceHost createService()
        {
            return new ServiceHost(_phoneExchangeService, _baseAddress);
        }

        /*
        protected override System.ServiceModel.ServiceHost createService()
        {
            return new ServiceHost(typeof(IPphoneExchangeService), _baseAddress);
        }
        */
        public override void doCleanUp(object sender, EventArgs args)
        {
           
        }
    }
}
