using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
using System.Globalization;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    class ProcessOrderConfirmation : IBL
    {
        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message, long IdVehicle)
        {

            if (message.tPotvrdaNaNaracka == null)
                return message;           

            try
            {

                VehiclesContainer.Instance.addNewOrderConfirmation(message.tPotvrdaNaNaracka);
                
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("ProcessOrderConfirmation", e);
            }

            return message;
        }

        #endregion
    }
}
