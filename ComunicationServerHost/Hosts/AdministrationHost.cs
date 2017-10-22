using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.ServiceModel;


namespace Taxi.Communication.Server.Host.Hosts
{
    public class AdministrationHost : AHost
    {

        public AdministrationHost(ILog log, Uri baseAddress)
            : base(log, "AdministrationHost", baseAddress)
        {

        }

        protected override System.ServiceModel.ServiceHost createService()
        {
            return new ServiceHost(typeof(AdministrationService), _baseAddress);
        }

        public override void doCleanUp(object sender, EventArgs args)
        {
           
        }
    }
}
