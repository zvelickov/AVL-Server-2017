using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP56 : IGeneralMessageHandler
    {
        protected int bytesInMessageHeader = 9;

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP56")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {

            ReceivedRegionsTo m_ReceivedRegionsTo = null;
            // Prvo parsiranje

            m_ReceivedRegionsTo = parseReceivedRegionsTo(msg);


            // Ako e se OK, se dopolnuva objektot i se vraka vo GpsListener

            if (m_ReceivedRegionsTo != null)
            {

                m_ReceivedRegionsTo.DateTimeReceived = System.DateTime.Now;

                ParserResponseContainer retVal = new ParserResponseContainer(m_ReceivedRegionsTo);
                return retVal;
            }

            else
            {
                return null;
            }



        }

        #endregion

        private ReceivedRegionsTo parseReceivedRegionsTo(DeviceMessage msg)
        {
            ReceivedRegionsTo m_ReceivedRegionsTo = new ReceivedRegionsTo();

            byte[] tmpByte = new byte[2];

            tmpByte[0] = msg.data[0 + bytesInMessageHeader];
            tmpByte[1] = msg.data[1 + bytesInMessageHeader];

            m_ReceivedRegionsTo.IdRegion = System.BitConverter.ToInt16(tmpByte, 0);

            return m_ReceivedRegionsTo;

        }



    }
}
