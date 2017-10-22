using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;



namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP55 : IGeneralMessageHandler
    {
        protected int bytesInMessageHeader = 9;

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP55")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {


            ReceivedShortMessage tmpReceivedShortMessage = null;


            // Prvo parsiranje
            tmpReceivedShortMessage = parseShortMessage(msg);

            // Ako e se OK, se dopolnuva objektot i se vraka vo GpsListener

            if (tmpReceivedShortMessage != null)
            {
                tmpReceivedShortMessage.DateTimeReceived = DateTime.Now;

                ParserResponseContainer retVal = new ParserResponseContainer(tmpReceivedShortMessage);
                return retVal;
            }

            else
            {
                return null;
            }


        }

        #endregion



        private ReceivedShortMessage parseShortMessage(DeviceMessage msg)
        {
            ReceivedShortMessage ReceivedShortMessage = new ReceivedShortMessage();

            byte[] tmpByte = new byte[2];

            tmpByte[0] = msg.data[0 + bytesInMessageHeader];
            tmpByte[1] = msg.data[1 + bytesInMessageHeader];

            ReceivedShortMessage.IdShortMessage = System.BitConverter.ToInt16(tmpByte, 0);

            return ReceivedShortMessage;

        }

    }

}
