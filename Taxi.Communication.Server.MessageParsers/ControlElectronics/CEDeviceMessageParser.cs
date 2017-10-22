using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
//using Taxi.Communication.Server.ConnectionListeners;
using System.Text;
using Taxi.Communication.Server.Utils;
using log4net;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics
{
    public class CEDeviceMessageParser
    {
        System.Text.ASCIIEncoding AsciiEncoding = new System.Text.ASCIIEncoding();

        private ILog log;

        public CEDeviceMessageParser()
        {
            log = LogManager.GetLogger("MyService");
        }

        public List<DeviceMessage> ParseMessageHeader(SocketPacket socketDataBuffer)
        {

            List<DeviceMessage> retVal = new List<DeviceMessage>();

            bool izlez = true;

            socketDataBuffer.SocketMessageBuffer = deleteLeadingNotNeededData(ref socketDataBuffer.SocketMessageBuffer, -1);

            if (socketDataBuffer.SocketMessageBuffer.Length > 0)
                izlez = false;

            while (!izlez)
            {
                DeviceMessage retDeviceMessage = parseHeader(ref socketDataBuffer.SocketMessageBuffer);

                if (retDeviceMessage != null)
                {
                    retVal.Add(retDeviceMessage);
                }
                else
                    izlez = true;
            }

            if (retVal.Count > 0)
                return retVal;
            else
                return null;
        }

        public DeviceMessage parseHeader(ref byte[] dataBuffer)
        {
            byte[] ByteVoMessageIndicator = new byte[2];
            byte[] ByteVoBrojNaUred = new byte[5];
            byte[] ByteVoKomanda = new byte[2];
            byte[] ByteZaReturnMessage = new byte[11];

            int m_dataLenght;
            int i;

            // Mora da ima min 11 byte-i za da moze da se izvadi header-ot !!! 
            // Tuka e kalkulirano AA=2 + xxxxx=5 + yy=2 + cc=2 
            // Poslednovo (cc) moze da e ili checksum, ako e odgovor na pratena poraka, ili broj na byte-i / poraki

            if (dataBuffer.Length < 11)
            {

                return null;
            }

            DeviceMessage newMsg = new DeviceMessage();

            // Zemam AA, BB, ...
            // *************************************************************************************
            for (i = 0; i < 2; i++)
            {
                ByteVoMessageIndicator[i] = (byte)dataBuffer[i];

                //Tuka si stavam i vo promenlivata sto mi treba da mu ja vratam na Marjan, kako odgovor (heder + SS)
                // ByteZaReturnMessage[i] = (byte)dataBuffer[i];
            }

            ByteZaReturnMessage[0] = (byte)'B';
            ByteZaReturnMessage[1] = (byte)'B';

            newMsg.MessageIndicator = AsciiEncoding.GetString(ByteVoMessageIndicator);

            // Broj na ured
            // *************************************************************************************
            for (i = 2; i < 7; i++)
            {
                ByteVoBrojNaUred[i - 2] = (byte)dataBuffer[i];

                //Tuka si stavam i vo promenlivata sto mi treba da mu ja vratam na Marjan, kako odgovor (heder + SS)
                ByteZaReturnMessage[i] = (byte)dataBuffer[i];
            }

            newMsg.DeviceNumber = AsciiEncoding.GetString(ByteVoBrojNaUred);


            // Broj na komanda
            // *************************************************************************************
            for (i = 7; i < 9; i++)
            {
                ByteVoKomanda[i - 7] = (byte)dataBuffer[i];

                //Tuka si stavam i vo promenlivata sto mi treba da mu ja vratam na Marjan, kako odgovor (heder + SS)
                ByteZaReturnMessage[i] = (byte)dataBuffer[i];
            }

            newMsg.Command = AsciiEncoding.GetString(ByteVoKomanda);


            // E, zaradi pederot Marjan, tuka treba da razmisluvame kolku byte-i ni e samata poraka
            // Vo porakata ke go stavam i header-ot
            // *************************************************************************************


            // Prvo si gi zemam 2-ta byti koi mi kazuvaat kolku byte-i imam vo porakata
            // Ova ne treba za AA08

            byte[] m_NoOfBytes = new byte[2];

            for (int k = 0; k < 2; k++)
                m_NoOfBytes[k] = dataBuffer[k + 9];



            m_dataLenght = 0;


            //Console.WriteLine("");
            //Console.WriteLine("Vo ParseHeader: " + newMsg.MessageIndicator + newMsg.Command);
            //Console.WriteLine("");

            switch (newMsg.MessageIndicator + newMsg.Command)
            {
                case "AA08":
                    m_dataLenght = 73;
                    break;

                case "AA09":
                    m_dataLenght = 73;
                    break;

                case "FF08":
                    m_dataLenght = 73;
                    break;


                case "FF10":
                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF10): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 9 + 2; // se dodava header-ot i chksum

                    break;

                case "FF11":
                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF11): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 9 + 2; // se dodava header-ot i chksum

                    break;


                case "FF12":

                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    log.Debug("FISKALNA PORAKA: " + AsciiEncoding.GetString(dataBuffer));

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF12): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 11 + 2; // se dodava header-ot i chksum

                    break;



                case "FF13":

                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    log.Debug("FISKALNA PORAKA: " + AsciiEncoding.GetString(dataBuffer));

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF13): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 11 + 2; // se dodava header-ot i chksum

                    break;



                case "BB04":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return newMsg;
                    break;

                case "BB87":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return newMsg;
                    break;

                case "BB34":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return newMsg;
                    break;

                case "BB35":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return newMsg;
                    break;


                case "BB36":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return newMsg;
                    break;

                case "BB33":



                    // ZORAN:   Stavam vo log koga e dobiena potvrda od vozilo za konkretna naracka
                    //          Ova treba da se trgne!


                    //log.Debug("------------- Dobiena potvrda za naracka za ured: " + newMsg.DeviceNumber);
                    

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return null;

                    break;

                case "BB50":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    Console.WriteLine("Vrateno BB50");

                    return newMsg;
                    break;

                case "BB45":
                    m_dataLenght = 0;

                    // Stavi null za AA ili BB na pocetok
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    Console.WriteLine("Vrateno BB45");

                    return newMsg;
                    break;

                case "PP55":
                    m_dataLenght = 13;
                    break;

                case "PP56":
                    m_dataLenght = 13;
                    break;

                case "PP57":
                    m_dataLenght = 21;
                    break;

                case "PP58":
                    m_dataLenght = 17;
                    break;

                case "PP71":
                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF11): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 11 + 2; // se dodava header-ot i chksum

                    break;
               

                case "PP72":
                    m_dataLenght = 12;
                    break;


                case "PP73":
                    m_dataLenght = 44;
                    break;


                case "PP74":
                    m_dataLenght = CalculateNumberOfBytesFromMessage(newMsg.MessageIndicator + newMsg.Command, m_NoOfBytes);

                    if (m_dataLenght == 0) // Ili stvarno e 0, ili e greska. Vo dvata slucai, brisi ja porakata, nesto ne e vo red
                    {
                        // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                        dataBuffer[0] = 0x00;
                        dataBuffer[1] = 0x00;

                        dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                        // tuka treba vo baza/log
                        //ServiceCallBack.log.Error("Gresna poraka za fiskalen aparat za ured (FF11): " + newMsg.DeviceNumber.ToString());

                        //i izlezi
                        return null;
                    }
                    else
                        m_dataLenght = m_dataLenght + 11 + 2; // se dodava header-ot i chksum

                    break;

                default:

                    //int m;

                    //for (m=0; m<dataBuffer.Length; m++)
                    //    ServiceCallBack.log.Debug("LOSA PORAKA: " + dataBuffer[m].ToString());

                    // Brisi ja tekovnata poraka, da ne se vrtime vo krug
                    dataBuffer[0] = 0x00;
                    dataBuffer[1] = 0x00;

                    dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                    return null;

                //break;
            }

            /*
             * Se zema cela poraka bez SS
             */


            newMsg.data = new byte[m_dataLenght - 2];


            if (dataBuffer.Length >= m_dataLenght)
            {
                for (i = 0; i < m_dataLenght - 2; i++)
                {
                    //Console.Write(Byte.Parse(dataBuffer[i].ToString()));
                    newMsg.data[i] = Byte.Parse(dataBuffer[i].ToString());
                }
                


                newMsg.DataLength = m_dataLenght - 2;


                // Dodavam chksum na headerot sto se vraka do uredot
                newMsg.ReturnMessage = addChkSum(ByteZaReturnMessage);


                newMsg.CheckSum[0] = (byte)dataBuffer[newMsg.DataLength];
                newMsg.CheckSum[1] = (byte)dataBuffer[newMsg.DataLength + 1];



                



                // Stavi null za AA ili BB na pocetok
                dataBuffer[0] = 0x00;
                dataBuffer[1] = 0x00;

                dataBuffer = deleteLeadingNotNeededData(ref dataBuffer, -1);

                // Zavrsivme, sega moze da vrakame nazad vrednosti
                if ((newMsg.MessageIndicator + newMsg.Command) != "FF12" || (newMsg.MessageIndicator + newMsg.Command) != "FF13")
                {
                    if (!newMsg.validate())
                    {
                        
                        return null;
                    }
                }
                return newMsg;
            }

            else
            {
                return null;
            }


        }


        // Brisenje na gjubre sto postoi na pocetokot na porakata, ako ima
        //
        // *************************************************************************

        public byte[] deleteLeadingNotNeededData(ref byte[] m_message, int intEndOfMsg)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string tmpString = enc.GetString(m_message);

            int mIntPrvo = intEndOfMsg;

            if (intEndOfMsg < 0)
            {
                mIntPrvo = -1;

                List<int> tSort = new List<int>();

                int m_intAA = tmpString.IndexOf("AA");
                if (m_intAA != -1)
                    tSort.Add(m_intAA);

                int m_intBB = tmpString.IndexOf("BB");
                if (m_intBB != -1)
                    tSort.Add(m_intBB);

                int m_intFF = tmpString.IndexOf("FF");
                if (m_intFF != -1)
                    tSort.Add(m_intFF);

                int m_intPP = tmpString.IndexOf("PP");
                if (m_intPP != -1)
                    tSort.Add(m_intPP);


                tSort.Sort();

                if (tSort.Count > 0)
                    mIntPrvo = tSort[0];

                // OK, nema nitu edno AA, BB, ...
                // Znaci, buferot e neupotrebliv i treba da se isprazni
                if (mIntPrvo == -1)
                {
                    Array.Resize(ref m_message, 0);
                    return m_message;
                }

            }

            // Ima AA, BB, ...
            // Sega mIntPrvo go ima indeksot na prvoto pojavuvanje na AA, ili BB ili FF
            // gi pomestuvam site byte-ti od porakata, ako ima potreba, za tolku vo levo

            int n = 0;

            if (mIntPrvo > 0)
            {
                for (int j = mIntPrvo; j <= m_message.Length - 1; j++, n++)
                    m_message[j - mIntPrvo] = m_message[j];

                // OK, sega go namaluvame bufferot za da ostane samo ostatokot od byte-ite (sega se veke siftirani na pocetokot
                // Od prethodnoto se gleda kolku treba da ostane byte-ti vo bafer-ot: n byte-i

                Array.Resize(ref m_message, n);
            }
            return m_message;
        }



        // Kalkulacija kolku byte-i ima vo porakata
        // Zavisi od porakata
        //
        // *************************************************************************

        public int CalculateNumberOfBytesFromMessage(string p_message, byte[] p_NoOfBytes)
        {
            int retVal = 0;

            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

            string m_stringDolzina = enc.GetString(p_NoOfBytes);


            switch (p_message)
            {
                case "FF10":
                    try
                    {
                        retVal = Int16.Parse(m_stringDolzina);
                    }
                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;

                case "FF11":
                    try
                    {
                        retVal = Int16.Parse(m_stringDolzina);
                    }

                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;

                case "FF12":
                    try
                    {
                        retVal = System.BitConverter.ToInt16(p_NoOfBytes,0);
                        //retVal = Int16.Parse(m_stringDolzina);
                    }
                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;

                case "FF13":
                    try
                    {
                        retVal = System.BitConverter.ToInt16(p_NoOfBytes, 0);
                        //retVal = Int16.Parse(m_stringDolzina);
                    }
                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;
                case "PP71":
                    try
                    {
                        retVal = Int16.Parse(m_stringDolzina);
                    }
                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;


                case "PP74":
                    try
                    {
                        retVal = Int16.Parse(m_stringDolzina);
                    }
                    catch (Exception ex)
                    {
                        log.Error(" Error ", ex);
                        retVal = 0;
                    }

                    break;

                default:
                    break;
            }


            return retVal;


        }


        // Zoran:   Ova e checksum sto e iskopiran od clcMEssageCreator
        //          Vo praksa ne e OK, no vaka mi e najlesno vo momentov
        //          Tuka go koristam za da mu vratam na Marjan korekten chksum na headerot sto mu go vrakam
        // ************************************************************************************************

        public byte[] addChkSum(byte[] m_message)
        {
            byte[] retVal = new byte[m_message.Length + 2];

            for (int i = 0; i < m_message.Length; i++)
            {
                retVal[i] = m_message[i];
            }

            byte tmpByte = (byte)0;


            foreach (byte item in m_message)
            {
                tmpByte = (byte)(tmpByte ^ item);
            }

            retVal[retVal.Length - 1] = (byte)((tmpByte & 0x0f) | 0x30);
            retVal[retVal.Length - 2] = (byte)(((tmpByte & 0xf0) >> 4) | 0x30);

            return retVal;
        }




    }
}
