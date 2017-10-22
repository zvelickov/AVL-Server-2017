using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public interface IParser
    {
        ParserResponseContainer parseData(DeviceMessage msg);
    }
}
