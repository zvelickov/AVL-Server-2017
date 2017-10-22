using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;


namespace Taxi.Communication.Server.Utils.GeoFence
{
    public interface INotifyStrategy
    {
        bool isForNotification(Locations loc, GeoFences gf,long ID_LastGeoFence);
    }
}
