using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;


namespace Taxi.Communication.Server.Utils
{

    public delegate void SendOrderToVehicle(long ID_Vehicle, long ID_User, PhoneCalls phoneCall);

    public interface ICallbacksForOrders
    {
        event SendOrderToVehicle OnOrderRecived;
        void CallBackForOrderRecived(long ID_Vehicle, long ID_User, PhoneCalls phoneCall);    
    }
}
