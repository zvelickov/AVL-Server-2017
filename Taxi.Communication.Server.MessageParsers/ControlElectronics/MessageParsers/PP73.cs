using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Utils.Classes;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP73 : IGeneralMessageHandler
    {
        protected readonly ASCIIEncoding enc = new ASCIIEncoding();

        protected int bytesInMessageHeader = 9;


        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP73")
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

            clsBaranjeStatus tBaranjeStatus = new clsBaranjeStatus();

            tBaranjeStatus = parseData(m_dataBuffer);

            ParserResponseContainer retVal = new ParserResponseContainer(tBaranjeStatus);

            return retVal;
        }



        private clsBaranjeStatus parseData(byte[] msg)
        {
            clsBaranjeStatus retVal = new clsBaranjeStatus();

            byte[] tmpByte = new byte[2];
            tmpByte[0] = msg[0];

            Int16 m_KodNaBaranje = System.BitConverter.ToInt16(tmpByte, 0);
            retVal.KodNaBaranje = m_KodNaBaranje;

            byte[] tmpByteVrednost = new byte[2];
            tmpByteVrednost[0] = msg[1];
            tmpByteVrednost[1] = msg[2];

            Int16 m_Vrednost = System.BitConverter.ToInt16(tmpByteVrednost, 0);
            retVal.Vrednost = m_Vrednost;


            byte[] m_string = new byte[msg.Length - 3];

            for (int n = 0; n < m_string.Length; n++)
            {
                m_string[n] = msg[n + 3];
            }

            int mNulaVrednost = Array.IndexOf(m_string, ' ');

            string m_TextZaBaranje;
                  
            if (mNulaVrednost > 0)
            {
                m_TextZaBaranje = enc.GetString(m_string, 0, mNulaVrednost);
            }
            else
            {
                m_TextZaBaranje = enc.GetString(m_string).Trim();
            }

            retVal.TextZaBaranje = m_TextZaBaranje;

            return retVal;
        }
    }
}
