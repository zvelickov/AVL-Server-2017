using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public interface IDeviceMessageParser
    {
        /// <summary>
        /// Parses all the messages currently in the buffer.  
        /// In the buffer there might be more than one message. 
        /// So all the parsed messages MUST be deleted from the buffer!
        /// socketDataBuffer.SocketMessageBuffer
        /// </summary>
        /// <param name="socketDataBuffer"></param>
        /// <returns>List<DeviceMessage> a list containing all the complete messages currently in the buffer</returns>
        List<DeviceMessage> ParseMessageHeader(SocketPacket socketDataBuffer);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <returns></returns>
        
        
        int CalculateNumberOfBytesFromMessage(string p_message, byte[] p_NoOfBytes);
        
        //Pajo comment: Ne se koristi globalno znaci nema potreba da stoi vo Interface!
        //DeviceMessage parseHeader(ref byte[] dataBuffer);
        //byte[] deleteLeadingNotNeededData(ref byte[] m_message, int intEndOfMsg);
        //byte[] addChkSum(byte[] m_message);
    }
}
