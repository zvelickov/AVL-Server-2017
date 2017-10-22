using System.Collections.Generic;
//using Taxi.Communication.Server.PhoneSwitch;

namespace Taxi.Communication.Server.Utils.Parsers
{

    public class DeviceMessage
    {

        public DeviceMessage()
        {
            m_checkSum = new byte[2];
        }

        #region ExtraHeaderData
        private string _extraData;

        public string ExtraHeaderData
        {
            get { return _extraData; }
            set { _extraData = value; }
        }
        #endregion

        #region MessageIndicator

        private string m_messageIndicator;

        public string MessageIndicator
        {
            get { return m_messageIndicator; }
            set { m_messageIndicator = value; }
        }
        #endregion

        #region DeviceNumber

        private string m_deviceNumber;

        public string DeviceNumber
        {
            get { return m_deviceNumber; }
            set { m_deviceNumber = value; }
        }


        #endregion

        #region Command

        private string m_command;

        public string Command
        {
            get { return m_command; }
            set { m_command = value; }
        }


        #endregion

        #region NumberOldData

        private int m_numberOldData;

        public int NumberOldData
        {
            get { return m_numberOldData; }
            set { m_numberOldData = value; }
        }


        #endregion

        #region DataLength

        private int m_dataLength;

        public int DataLength
        {
            get { return m_dataLength; }
            set { m_dataLength = value; }
        }


        #endregion

        #region Data

        public byte[] data;

        #endregion

        #region CheckSum

        private byte[] m_checkSum;

        public byte[] CheckSum
        {
            get { return m_checkSum; }
            set { m_checkSum = value; }
        }


        #endregion

        #region ReturnMessage

        private byte[] m_returnMessage;

        public byte[] ReturnMessage
        {
            get { return m_returnMessage; }
            set { m_returnMessage = value; }
        }


        #endregion

        public bool validate()
        {
            byte[] retVal = new byte[2];
            byte tmpByte = (byte)0;


            foreach (byte item in data)
            {
                tmpByte = (byte)(tmpByte ^ item);
            }


            retVal[0] = (byte)((tmpByte & 0x0f) | 0x30);
            retVal[1] = (byte)(((tmpByte & 0xf0) >> 4) | 0x30);

            bool chkTest = true;

            if (retVal[1] != m_checkSum[0])
                chkTest = false;
            if (retVal[0] != m_checkSum[1])
                chkTest = false;

            return chkTest;
        }
    }
}
