using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public interface IGeneralMessageHandler
    {
        bool canHandle(DeviceMessage msg);
        ParserResponseContainer ProcessMessage(DeviceMessage msg);
    }
}
