using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.ServiceModel;


namespace Taxi.Communication.Server.Host.Hosts
{
    public class TaxiAdminHost : AHost
    {

        public TaxiAdminHost(ILog log, Uri baseAddress)
            : base(log, "TaxiAdminHost", baseAddress)
        {

        }

        protected override System.ServiceModel.ServiceHost createService()
        {
            return new ServiceHost(typeof(TaxiAdministrationService), _baseAddress);
        }

        public override void doCleanUp(object sender, EventArgs args)
        {

        }
    }
}
