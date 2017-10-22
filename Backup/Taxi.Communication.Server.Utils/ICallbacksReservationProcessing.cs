using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
// //using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils
{

    public delegate void ReservationProcessing(long ID_Company, TList<Reservations> reservations);
    public interface ICallbacksReservationProcessing
    {
        void CallBackReservationProcessing(long ID_Company, TList<Reservations> reservations);           
    }
}
