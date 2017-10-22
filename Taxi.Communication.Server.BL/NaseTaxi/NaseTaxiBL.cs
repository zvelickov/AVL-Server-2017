using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    public class NaseTaxiBL : IBL
    {
        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message,long IdVehicle)
        {
            //LoyaltyProgram loyalty = new LoyaltyProgram();
            FiscalLoyalty fiscal = new FiscalLoyalty();
            ProcessRfIdCard rfIds = new ProcessRfIdCard();
            ProcessOrderConfirmation pOC = new ProcessOrderConfirmation();
            ProcessStatusInfo pSI = new ProcessStatusInfo();

            message = rfIds.processBL(message, IdVehicle);
            //message = loyalty.processBL(message,veh);
            message = fiscal.processBL(message, IdVehicle);
            message = pOC.processBL(message, IdVehicle);
            message = pSI.processBL(message, IdVehicle);
            return message;
        }

        #endregion
    }
}
