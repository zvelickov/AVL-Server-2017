using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP71 : IGeneralMessageHandler
    {
        protected readonly ASCIIEncoding enc = new ASCIIEncoding();

        protected int bytesInMessageHeader = 11;

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP71")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {


            // Go trgam header-ot
            byte[] m_dataBuffer = new byte[msg.DataLength - bytesInMessageHeader];

            for (int n = 0; n < m_dataBuffer.Length; n++)
            {
                m_dataBuffer[n] = (byte)msg.data[n + bytesInMessageHeader];
            }

            ReceivedFreeText tReceivedFreeText = new ReceivedFreeText();

            tReceivedFreeText = parseData(m_dataBuffer);

            tReceivedFreeText.DateTimeReceived = System.DateTime.Now;


            ParserResponseContainer retVal = new ParserResponseContainer(tReceivedFreeText);
            return retVal;


        }

        #endregion

        private ReceivedFreeText parseData(byte[] msg)
        {
            ReceivedFreeText retVal = new ReceivedFreeText();

            byte[] tmpByte = new byte[2];
            tmpByte[0] = msg[0];

            Int16 m_MessageNumber = System.BitConverter.ToInt16(tmpByte, 0);

            retVal.MessageNumber = m_MessageNumber;


            byte[] m_string = new byte[msg.Length - 1];

            for (int n = 0; n < m_string.Length; n++)
            {
                m_string[n] = msg[n + 1];
            }

            string m_Poraka = enc.GetString(m_string);

            retVal.MessageText = m_Poraka;

            return retVal;
        }
    }
}
