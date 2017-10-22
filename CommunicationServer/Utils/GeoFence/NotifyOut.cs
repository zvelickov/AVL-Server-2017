using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Data.Bases;
//using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils.GeoFence
{
    public class NotifyOut : INotifyStrategy
    {

        #region INotifyStrategy Members

        public bool isForNotification(Locations loc, GeoFences gf, long ID_LastGeoFence)
        {
            return (loc.IDGeoFence != gf.IDGeoFence);
        }

        #endregion
    }
}
