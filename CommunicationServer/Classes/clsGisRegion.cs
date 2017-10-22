using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Classes
{
    public class clsGisRegion
    {



        private long _idRegion;

        public long IDRegion
        {
            get { return _idRegion; }
            set { _idRegion = value; }
        }

        private string _gisRegionName;

        public string GisRegionName
        {
            get { return _gisRegionName; }
            set { _gisRegionName = value; }
        }
    }
}
