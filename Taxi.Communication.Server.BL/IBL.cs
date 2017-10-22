using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.BL
{
    public interface IBL
    {
        ParserResponseContainer processBL(ParserResponseContainer message, long IdVehicle);
    }
}
