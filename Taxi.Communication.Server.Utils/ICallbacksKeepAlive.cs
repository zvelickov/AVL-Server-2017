using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils
{

    public delegate void KeepAlive();
    public interface ICallbacksKeepAlive
    {
        void CallBackKeepAlive();           
    }
}
