using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils
{

    public delegate void GPSMessageRecived(long ID_Company, Vehicle vehicle);
    public interface ICallbacksForGPSmessageRecived
    {
        void CallBackGPSMessageRecived(long ID_Company, Vehicle vehicle);           
    }
}
