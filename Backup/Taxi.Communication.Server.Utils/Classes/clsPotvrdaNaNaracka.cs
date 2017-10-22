using System;
using System.Collections.Generic;
using System.Text;


namespace Taxi.Communication.Server.Utils.Classes
{
    public class clsPotvrdaNaNaracka
    {
        private long _idVehicle;

        public long IdVehicle
        {
            get { return _idVehicle; }
            set { _idVehicle = value; }
        }

        private long _idPhoneCall;

        public long IdPhoneCall
        {
            get { return _idPhoneCall; }
            set { _idPhoneCall = value; }
        }

        private int _minuti;

        public int Minuti
        {
            get { return _minuti; }
            set { _minuti = value; }
        }
    }
}
