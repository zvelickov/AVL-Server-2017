using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Classes
{
    public class clsBaranjeStatus
    {
        private int _kodNaBaranje;

        public int KodNaBaranje
        {
            get { return _kodNaBaranje; }
            set { _kodNaBaranje = value; }
        }

        private int _vrednost;

        public int Vrednost
        {
            get { return _vrednost; }
            set { _vrednost = value; }
        }

        private string _textZaBaranje;

        public string TextZaBaranje
        {
            get { return _textZaBaranje; }
            set { _textZaBaranje = value; }
        }
    }
}
