using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class BB87 : IGeneralMessageHandler
    {


        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "BB87")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {

            ParserResponseContainer retVal = new ParserResponseContainer(0);
            return retVal;
        }

        #endregion
    }
}
