using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
//using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch.Parsers
{
    public class PhoneExchangeMessageParser
    {
        //private Regex m_telephoneMessage;

        public PhoneExchangeMessageParser()
        {
            //this.m_telephoneMessage = new Regex(@"^BB.*$");
        }

        public PhoneCalls parseIncommingMessage(byte[] dataBuffer)
        {
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            string str = enc.GetString(dataBuffer);

            //if (m_telephoneMessage.IsMatch (str))
            return parseMessage(dataBuffer);

            //throw new Exception();
        }


        private PhoneCalls parseMessage(byte[] dataBuffer)
        {

            
            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            
            if (enc.GetString(dataBuffer, 25, 3) != "<I>")
                return null;

            String tempString;

            PhoneCalls newMsg = new PhoneCalls();

            tempString = enc.GetString(dataBuffer, 0, 8);

            IFormatProvider culture = new CultureInfo("en-US", true);

            newMsg.RinglDateTime = DateTime.Parse(tempString, culture, DateTimeStyles.NoCurrentDateDefault);

            tempString = enc.GetString(dataBuffer, 9, 7);

            DateTime test = DateTime.Parse(tempString, culture, DateTimeStyles.NoCurrentDateDefault);

            newMsg.RinglDateTime = newMsg.RinglDateTime.AddHours(test.Hour);
            newMsg.RinglDateTime = newMsg.RinglDateTime.AddMinutes(test.Minute);            

            tempString = enc.GetString(dataBuffer, 18, 4);

            newMsg.Extension = tempString;
            
            tempString = enc.GetString(dataBuffer, 22, 2);

            newMsg.LineIn = Int32.Parse(tempString);

            tempString = enc.GetString(dataBuffer, 28, 12);

            try
            {
                Int32.Parse(enc.GetString(dataBuffer, 28, 3));
            }
            catch(Exception )
            {
                return null;
            }
            //newMsg.PhoneNumber = Regex.Replace(tempString, "?", "").Trim();
            newMsg.PhoneNumber = tempString.Replace("?", "").Trim(); 

            int tmpTimeSec = 0;

            if (enc.GetString(dataBuffer, 41, 4).Trim() != "")
            {
                try
                {
                    tempString = enc.GetString(dataBuffer, 41, 4).Trim();
                    tempString = enc.GetString(dataBuffer, 41, 1).Trim();

                    tmpTimeSec = Int32.Parse(tempString) * 60;

                    tempString = enc.GetString(dataBuffer, 43, 2);

                    tmpTimeSec = tmpTimeSec + Int32.Parse(tempString);
                }
                catch (ArgumentException )
                {
                    tmpTimeSec = 0;
                }
                catch (FormatException )
                {
                    tmpTimeSec = 0;
                }
                catch (Exception )
                {
                    tmpTimeSec = 0;
                }

            }

            newMsg.RingDuration = tmpTimeSec;

            tmpTimeSec = 0;

            if (enc.GetString(dataBuffer, 46, 8).Trim() != "")
            {
                try
                {
                    tempString = enc.GetString(dataBuffer, 46, 2);

                    tmpTimeSec = Int32.Parse(tempString) * 3600;

                    tempString = enc.GetString(dataBuffer, 49, 2);

                    tmpTimeSec = tmpTimeSec + Int32.Parse(tempString) * 60;

                    tempString = enc.GetString(dataBuffer, 52, 2);

                    tmpTimeSec = tmpTimeSec + Int32.Parse(tempString);

                }
                catch (ArgumentException )
                {
                    tmpTimeSec = 0;
                }
                catch (FormatException )
                {
                    tmpTimeSec = 0;
                }
                catch (Exception )
                {
                    tmpTimeSec = 0;
                }
            }

            newMsg.CallDuration = tmpTimeSec;

            tempString = enc.GetString(dataBuffer, 55, 5);

            if (tempString != "")
            {
                try
                {
                    newMsg.Cost = Int32.Parse(tempString);
                }
                catch (ArgumentException )
                {
                    newMsg.Cost = 0;
                }
                catch (FormatException )
                {
                    tmpTimeSec = 0;
                }
                catch (Exception )
                {
                    tmpTimeSec = 0;
                }
            }

            tempString = enc.GetString(dataBuffer, 66, 4);

            newMsg.AccountCode = tempString;

            tempString = enc.GetString(dataBuffer, 77, 2);

            switch (tempString)
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
                case "  ":
                    newMsg.MessageType = PhoneExchangeMessageType.CC.ToString();
                    break;
                case " ?":
                    newMsg.MessageType = PhoneExchangeMessageType.CC.ToString();
                    break;
                case "? ":
                    newMsg.MessageType = PhoneExchangeMessageType.CC.ToString();
                    break;
                case "??":
                    newMsg.MessageType = PhoneExchangeMessageType.CC.ToString();
                    break;
                default:
                    newMsg.MessageType = PhoneExchangeMessageType.NN.ToString();
                    break;

            }

            newMsg.DataLength = dataBuffer.Length;

            newMsg.Data = dataBuffer;

            //ZORAN:    Ova e dodadeno za da se vodi smetka za GroupCode (da se znae za koja kompanija e povikot)
            //          Vo Extension na RC doaga prvo GroupCode, koj se prepbrisuva na podiganje na slusalka

            if(tempString == "RC")
                newMsg.GroupCode = newMsg.Extension;

            

            return newMsg;
        }

    }
}