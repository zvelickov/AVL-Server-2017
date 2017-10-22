using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP72 : IGeneralMessageHandler
    {

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP72")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {
            ParserResponseContainer retVal = new ParserResponseContainer(new byte[1]);
            return retVal;
        }

        #endregion
    }
}
