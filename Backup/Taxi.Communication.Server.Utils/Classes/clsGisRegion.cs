using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Classes
{
    public class clsGisRegion
    {



        private long _idRegion;

        public long IdRegion
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
