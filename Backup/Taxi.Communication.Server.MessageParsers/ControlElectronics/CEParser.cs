using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics
{

    public class CEParser : IParser
    {
        private List<IGeneralMessageHandler> _lstParsers;

        public CEParser()
        {
            _lstParsers = new List<IGeneralMessageHandler>();
            _lstParsers.Add(new AA08());
            _lstParsers.Add(new AA09());
            _lstParsers.Add(new FF08());
            _lstParsers.Add(new FF10());
            _lstParsers.Add(new FF11());
            _lstParsers.Add(new FF12());
            _lstParsers.Add(new FF13());
            _lstParsers.Add(new BB04());
            _lstParsers.Add(new BB87());
            _lstParsers.Add(new PP55());
            _lstParsers.Add(new PP56());
            _lstParsers.Add(new PP57());
            _lstParsers.Add(new PP58());
            _lstParsers.Add(new PP71());
            _lstParsers.Add(new PP72());
            _lstParsers.Add(new PP73());
            _lstParsers.Add(new PP74());
        }

        public ParserResponseContainer parseData(DeviceMessage msg)
        {
            bool haveFoundParser = false;
            ParserResponseContainer retVal = null;
            foreach (IGeneralMessageHandler parser in _lstParsers)
            {
                if (parser.canHandle(msg))
                {
                    haveFoundParser = true;
                    retVal = parser.ProcessMessage(msg);
                    break; // Have found one do NOT look for anything else
                }
            }

            if (!haveFoundParser)
            {
                throw new Exception();
            }
            else
            {
                return retVal;
            }
        }
    }
}
