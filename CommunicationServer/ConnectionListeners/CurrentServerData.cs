using System;
using System.Collections.Generic;
using System.Text;
//using JP.Data.Utils;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.ConnectionListeners
{
    public class CurrentServerData
    {
        public static TList<PhoneCalls> gl_PhoneExchangeMessages = new TList<PhoneCalls>();
        
        //public static Dictionary<long, TList<Users>>

        public CurrentServerData(TList<PhoneCalls> gl_phoneMsg)
        {
            gl_PhoneExchangeMessages = gl_phoneMsg;
        }
    }
}
