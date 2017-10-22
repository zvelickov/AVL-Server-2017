using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.BL
{
    public class BLFactory
    {
        public static string LOG_NAME = "MyService";

        public static IBL createBL(long ID_Company)
        {
            IBL bl = new NaseTaxi.NaseTaxiBL();

            return bl;
        }
    }
}
