using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using log4net;
using Taxi.Communication.Server.ConnectionListeners;
using System.Threading;


namespace Taxi.Communication.Server
{
    public class Global : System.Web.HttpApplication
    {

        //private GPSListener m_gpsListeners;
        //private Thread oThread;

        protected void Application_Start(object sender, EventArgs e)
        {
            
            log4net.Config.XmlConfigurator.Configure();
            //log4net.Config.DOMConfigurator.Configure();
            log4net.LogManager.GetLogger("MyService").Info("STARTING");

            //m_gpsListeners = new GPSListener();

            //oThread = new Thread(new ThreadStart(m_gpsListeners.Start));
            //while (!oThread.IsAlive) ;

        }

        protected void Application_End(object sender, EventArgs e)
        {
            /*
            if (oThread != null)
            {
                m_gpsListeners.Stop();
                oThread.Join();
            }
             */
        }
    }
}