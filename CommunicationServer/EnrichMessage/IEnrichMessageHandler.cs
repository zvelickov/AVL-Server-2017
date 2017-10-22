using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.EnrichMessage
{
    interface IEnrichMessageHandler
    {
        bool canHandle(ParserResponseContainer message);
        long enrichMessage(ParserResponseContainer message);
    }
}
