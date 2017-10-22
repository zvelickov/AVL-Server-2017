using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils
{

    public delegate void PhoneCallRecived(object sender, PhoneCalls e);

    public interface ICallbacksForPhoneSwitch
    {            
        event PhoneCallRecived OnPhoneCallRecived;
        void CallForPhoneCallRecived(object sender, PhoneCalls e);

    }
}
