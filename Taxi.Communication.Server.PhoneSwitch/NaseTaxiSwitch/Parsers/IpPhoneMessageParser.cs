using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using GlobSaldo.AVL.Entities;



namespace Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch.Parsers
{
    public class IpPhoneMessageParser
    {
        public IpPhoneMessageParser()
        {
            //
        }

        public PhoneCalls parseIncommingMessage(string pPhoneCall)
        {
            return parseMessage(pPhoneCall);
        }


        private PhoneCalls parseMessage(string pPhoneCall)
        {

            char[] delimiterChars = { ',' };

            string[] IpPhoneExcangeValues = pPhoneCall.Split(delimiterChars);


            PhoneCalls newMsg = new PhoneCalls();

            

            //IFormatProvider culture = new CultureInfo("en-US", true);

            //newMsg.RinglDateTime = DateTime.Parse( tempString, culture, DateTimeStyles.NoCurrentDateDefault);

            newMsg.RinglDateTime = DateTime.Parse(IpPhoneExcangeValues[0]);
            newMsg.GroupCode = IpPhoneExcangeValues[1];
            newMsg.Extension = IpPhoneExcangeValues[2];
            newMsg.PhoneNumber = IpPhoneExcangeValues[3];
            newMsg.RingDuration = Int32.Parse(IpPhoneExcangeValues[4]);
            newMsg.CallDuration = Int32.Parse(IpPhoneExcangeValues[5]);
            Boolean missed = false;

            if (IpPhoneExcangeValues[7] == "1")
            {
                missed = true;
            }

            newMsg.IsMissedCall = missed;

            switch (IpPhoneExcangeValues[6])
            {

                case "NA":
                    newMsg.MessageType = PhoneExchangeMessageType.NA.ToString();
                    break;
                case "RC":
                    newMsg.MessageType = PhoneExchangeMessageType.RC.ToString();
                    break;
                case "AN":
                    newMsg.MessageType = PhoneExchangeMessageType.AN.ToString();
                    break;
                case "CC":
                    newMsg.MessageType = PhoneExchangeMessageType.CC.ToString();
                    break;
                case "CA":      // ZORAN:   Ova e povik od titka 
                                //          Procedurata e: 
                                //                  Stiga poraka so RC, so "1" na kraj vo porakata. Taa ja pustame niz sistem
                                //                  a vednas potoa generirame ista poraka no menuvame RC so NA i taa ja pustame niz sistem
                                //                  Dispecerot povikuva WEB service na centralata i povikot se vraka so CA
                    newMsg.MessageType = PhoneExchangeMessageType.CA.ToString();
                    break;
                case "AC":      // ZORAN:   Prezemanje na povik sto bil staven na on-hold
                    newMsg.MessageType = PhoneExchangeMessageType.AC.ToString();
                    break;
                case "PA":      // ZORAN:   Prezemanje na povik sto prethodno go obrabotuval drug dispecer
                    newMsg.MessageType = PhoneExchangeMessageType.PA.ToString();
                    break;
                default:
                    newMsg.MessageType = PhoneExchangeMessageType.NN.ToString();
                    break;

            }

            //newMsg.DataLength = dataBuffer.Length;

            //newMsg.Data = dataBuffer;

            return newMsg;
        }

    }
}
