using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.ServiceModel;


namespace Taxi.Communication.Server.Host.Hosts
{
    public class CallBackHost : AHost
    {

        private ServiceCallBack _callBackService;

        public CallBackHost(ILog log, ServiceCallBack callBackService, Uri baseAddress)
            : base(log, "CallBackHost", baseAddress)
        {
            _callBackService = callBackService;
        }

        protected override System.ServiceModel.ServiceHost createService()
        {
            return new ServiceHost(_callBackService, _baseAddress);
        }

        public override void doCleanUp(object sender, EventArgs args)
        {

        }
    }
}
