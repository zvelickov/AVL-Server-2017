using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Classes
{
    public class clsGeoLocation
    {
        private long _IdGeoLocation;

        public long IdGeoLocation
        {
            get { return _IdGeoLocation; }
            set { _IdGeoLocation = value; }
        }

        private string _geoLocationName;

        public string GeoLocationName
        {
            get { return _geoLocationName; }
            set { _geoLocationName = value; }
        }


    }

}
