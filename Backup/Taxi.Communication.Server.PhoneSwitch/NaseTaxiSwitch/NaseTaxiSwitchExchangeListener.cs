using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Collections.Specialized;
using System.IO;
using Taxi.Communication.Server.Utils;
using log4net;
using Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch.Parsers;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.Bases;


namespace Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch
{
    public class NaseTaxiSwitchExchangeListener : PhoneSwitchListener
    {

        static SerialPort ComPort;
        private static readonly ASCIIEncoding enc = new ASCIIEncoding();
        private static readonly PhoneExchangeMessageParser phoneParser = new PhoneExchangeMessageParser();
        private static readonly IpPhoneMessageParser ipPhoneParser = new IpPhoneMessageParser();

        private static List<PhoneCalls> activePhoneCalls = new List<PhoneCalls>();

        private object _sync = new object();

        private bool m_work;
        private static ILog log;

        private bool StillWorking = false;

        private System.Text.ASCIIEncoding AscEnc = new System.Text.ASCIIEncoding();
        public AsyncCallback pfnWorkerCallBack;

        PhoneCallRecived delegateHandler;

        List<byte[]> phoneFiles;

        public NaseTaxiSwitchExchangeListener()
        {
            //
        }

        internal NaseTaxiSwitchExchangeListener(string port, int baud)
        {
            m_work = true;
            log = LogManager.GetLogger("MyService");

            phoneFiles = new List<byte[]>();

            //string port = System.Configuration.ConfigurationManager.AppSettings["commPortId"];
            //int baud = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["commPortBaud"]);

            //InitializeComPort(port, baud);
            log.Debug("TEST");
        }



        public override void Start()
        {          
            log.Debug("Starting IP PhoneExcange Listener");

            try
            {
                delegateHandler = _callBack.CallForPhoneCallRecived;


                System.Net.HttpListener listener = new System.Net.HttpListener();

                listener.Prefixes.Add(ConfigurationManager.AppSettings["PhoneHttpListenerBaseAddress"]);

                listener.Start();
                Console.WriteLine("Listenin phone ...");


                while (m_work)
                {
                    HttpListenerContext ctx = listener.GetContext();
                   
                    StringBuilder sb = new StringBuilder();

                    NameValueCollection queryStringCollection = ctx.Request.QueryString;

                    sb.Append(queryStringCollection["id"]);

                    //log.Debug("ZORAN: CENTRALA: " + queryStringCollection["id"]);

                    byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
                    ctx.Response.ContentLength64 = b.Length;
                    ctx.Response.OutputStream.Write(b, 0, b.Length);
                    ctx.Response.OutputStream.Close();

                    Thread ThreadSendToIpPhone = new Thread(delegate() { OnIpDataReceived(queryStringCollection["id"]) ; });

                    ThreadSendToIpPhone.Start();

                    try
                    {
                        char[] delimiterChars = { ',' };

                        string pPhoneCall = queryStringCollection["id"];                                                

                        string[] IpPhoneExcangeValues = pPhoneCall.Split(delimiterChars);

                        if (IpPhoneExcangeValues[IpPhoneExcangeValues.GetLength(0) - 1] == "1")
                        {
                            
                            pPhoneCall = pPhoneCall.Replace("RC", "NA");

                            Thread ThreadSendToIpPhone2 = new Thread(delegate() { OnIpDataReceived(pPhoneCall); });

                            ThreadSendToIpPhone2.Start();
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex.Message);
                    }

                    //new Thread(new Worker(ctx).ProcessRequest).Start();
                }                    
            }
            catch (SocketException se)
            {
                log.Error("ERROR ", se);
            }

            log.Debug("ZORAN:   PROBLEM!!!! Izlegov od WHILE na PhoneListener!!!!");
            log.Debug("+++++++++++++++++++++++++++++++++++++++++");
        }


        public TList<PhoneCalls> getAllActivePhoneCalls()
        {
            TList<PhoneCalls> retVal = new TList<PhoneCalls>(activePhoneCalls);

            return retVal;
        }


        private void InitializeComPort(string port, int baud)
        {
            ComPort = new SerialPort(port, baud);
            ComPort.Parity = Parity.None;
            ComPort.StopBits = StopBits.One;
            ComPort.DataBits = 8;
            ComPort.Handshake = Handshake.RequestToSend;
            ComPort.ReceivedBytesThreshold = 1;
            ComPort.DataReceived += new SerialDataReceivedEventHandler(OnSerialDataReceived);

            try
            {
                if (ComPort.IsOpen)
                {
                    ComPort.Close();
                    Thread.Sleep(1000);
                }
                
                ComPort.Open();

            }
            catch (Exception ex)
            {
                log.Error("ERROR, ComPort.Open(); " + ex.Message);
                Console.WriteLine("ERROR, ComPort.Open(); " + ex.Message);
            }


        }


        public override void Stop()
        {
            this.m_work = false;
            ComPort.Close();
        }


# region SERIAL communication

        public void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs args)
        {

            try
            {
                string message = ComPort.ReadLine();

                PhoneCalls phoneMessage;
                phoneMessage = phoneParser.parseIncommingMessage(enc.GetBytes(message));

                if (phoneMessage == null)
                {
                    //log.Debug("COMM - " + ComPort.PortName + " - MESSAGE - " + message);
                    return;
                }

                phoneMessage.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();

                switch (phoneMessage.MessageType)
                {

                    case "NA":

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.RingDuration = phoneMessage.RingDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }

                                activePhoneCalls.Remove(phoneCall);
                                
                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                break;
                            }
                        }
                        break;

                    case "RC":

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {

                                log.Error("ERROR : Problem so centralata ");
                                break;
                            }
                        }

                        // ZORAN:   Ova go dodavam tuka za Dipecerot da ima info za adresata ako toj telefon veke postoi vo baza:
                        //          Dilema mi e dali da ja baram adresata tuka, ili vo moment na AN?

                        //          Celoto ke go stavam vo try/catch, bidejki ima nekolku mesta kade moze da zezne, 
                        //          a bilo kade da se sluci seedno e, ne e korisen ovoj del od kodot

                        try
                        {
                            GisPhoneNumbers gisPhoneNumber = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneMessage.PhoneNumber);

                            if (gisPhoneNumber != null) // Ako nema, nisto, si ostanuva null
                            {

                                // OK, ima zapis, vednas da si napravam DeepLoad po se sto mi treba:

                                Type[] tl = new Type[3];
                                tl[0] = typeof(GisObjects);
                                tl[1] = typeof(GisStreets);
                                tl[2] = typeof(GisRegions);

                                DataRepository.GisPhoneNumbersProvider.DeepLoad(gisPhoneNumber, true,  DeepLoadType.IncludeChildren, tl);

                                if (gisPhoneNumber.LocationType == 1)               // Znaci, ulica e!
                                {
                                    //phoneMessage.oAddressFrom.oGisAddressModel = GeneralFunctions.findGisAddressModelForIDStreetHouseNumber(gisPhoneNumber.IdStreet, gisPhoneNumber.HouseNumber ?? 0);

                                    phoneMessage.oAddressFrom.oGisStreets = gisPhoneNumber.IdStreetSource;

                                    if (gisPhoneNumber.HouseNumber == null)
                                        phoneMessage.oAddressFrom.HouseNumber = 0;
                                    else
                                        phoneMessage.oAddressFrom.HouseNumber = (int)gisPhoneNumber.HouseNumber;

                                }
                                else                                                // Znaci objekt e. Tuka treba poveke kontrola, no i vaka ke pomine
                                {
                                    phoneMessage.oAddressFrom.oGisObjects = gisPhoneNumber.IdObjectSource;
                                }

                               
                                // Sega da gi stavam rabotite sto se zaednicki, nezavisno dali e adresa ili objekt

                                phoneMessage.oAddressFrom.oGisRegions = gisPhoneNumber.IdRegionSource;
                                phoneMessage.oAddressFrom.LocationX = (double)gisPhoneNumber.LongitudeX;
                                phoneMessage.oAddressFrom.LocationY = (double)gisPhoneNumber.LatitudeY;

                                if (gisPhoneNumber.LocationQuality != null) // Vo start site ke bidat null, bidejki e novo pole. Treba da se trgne ovaa kontrola pokasno...
                                    phoneMessage.oAddressFrom.LocationQuality = (int)gisPhoneNumber.LocationQuality;
                                else
                                    phoneMessage.oAddressFrom.LocationQuality = 4;
                            }
                        }

                        catch (Exception ex)
                        {
                            log.Error("ZORAN (problem vo obrabotka na RC od centrala): Nekoj problem so citaneje na GisPhoneNumbers", ex);
                        }

                        if (DataRepository.PhoneCallsProvider.Insert(phoneMessage))
                        {
                            activePhoneCalls.Add(phoneMessage);
                        }
                        else
                        {
                            log.Error("ERROR : Problem so baza ");
                        }


                        //if (_callBack.OnPhoneCallRecived != null)
                        //{
                        //    _callBack.OnPhoneCallRecived(this, phoneMessage);
                        //}

                        delegateHandler(this, phoneMessage);

                        break;
                    case "AN":

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.RingDuration = phoneMessage.RingDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                phoneCall.IdUserInOut = getUserInOutForExtension(phoneCall.Extension);
                                

                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }
                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                break;
                            }
                        }

                        break;
                    case "CC":

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.CallDuration = phoneMessage.CallDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }

                                activePhoneCalls.Remove(phoneCall);
                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                break;
                            }
                        }

                        break;
                    default:
                        //newMsg.MessageType = PhoneExchangeMessageType.NN.ToString();
                        break;

                }

            }
            catch (IOException ioex)
            {
                log.Error("ERROR ", ioex);
            }
            catch (UnauthorizedAccessException uaex)
            {
                log.Error("ERROR ", uaex);
            }
            catch (Exception e) //TimeoutException)
            {
                log.Error("ERROR ", e);
            }

            return;
        }

# endregion


# region IP communication

        public override void OnIpDataReceived(string pIpPhoneCall)
        {

            try
            {

                PhoneCalls phoneMessage;

                phoneMessage = ipPhoneParser.parseIncommingMessage(pIpPhoneCall);

            

                if (phoneMessage == null)
                {
                    log.Debug("IP - GRESKA vo Parsiranje");
                    return;
                }

                phoneMessage.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();

                switch (phoneMessage.MessageType)
                {

                    case "NA":

                        System.Threading.Thread.Sleep(200);

                        //while (StillWorking == true) { }

                        //StillWorking = true;

                        //log.Debug("ZORAN: NA---" + phoneMessage.PhoneNumber + "--" + phoneMessage.Extension);

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.RingDuration = phoneMessage.RingDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }

                                activePhoneCalls.Remove(phoneCall);

                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                //log.Debug("ZORAN: NA---" + phoneMessage.PhoneNumber + "--" + "NAJDOV");

                                StillWorking = false;
                                break;
                            }
                            
                        }

                        //log.Debug("ZORAN: NA---" + phoneMessage.PhoneNumber + "--" + "NE NAJDOV");

                        //StillWorking = false;
                        break;

                    case "RC":

                        PhoneCalls tmpPhoneCallForRemove = null;                       

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                tmpPhoneCallForRemove = phoneCall;

                                string tmpString = phoneCall.RinglDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                               
                                tmpString = tmpString + " (" + phoneCall.PhoneNumber.ToString() + ")";

                                tmpString = tmpString + ", Grupa (stara/nova): " + phoneCall.GroupCode.ToString() + "/" + phoneMessage.GroupCode.ToString();

                                tmpString = tmpString + ", Lokal (star/nov): " + phoneCall.Extension.ToString() + "/" + phoneMessage.Extension.ToString();

                                log.Debug("Povtoren RC: " + tmpString);
                            }
                        }

                        if (tmpPhoneCallForRemove != null)
                            activePhoneCalls.Remove(tmpPhoneCallForRemove);

                        //ZORAN:    Tuka stavam systemsko vreme vo RingDateTime bidejki oni go prakaat Utc
                        //          Ova NE treba da se stava ako e Titka ili Generiran
                        //          Ova e titka SAMO ako MissedCall = 1 AMA da ne go mislam na site stavam SystemDateTime

                        phoneMessage.RinglDateTime = System.DateTime.Now;


                        //ZORAN:    ova PRIVREMENO!!!!
                        //          Ne go dava GroupCode za Lotus, pa na sila, se sto ne e NaseTaksi, go stavame da e na lotus (292)
                        //          MORA da se trgne!!!

                        ////log.Debug("ZORAN, ovde zamenuvam GroupCOde, posto e prazen, da bide na lotus!!! Pristigna: phoneMessage.GroupCode = " + phoneMessage.GroupCode);

                        if (phoneMessage.GroupCode.Trim() == "" || phoneMessage.GroupCode.Trim() != "291")
                            phoneMessage.GroupCode = "292";

                        ////log.Debug("ZORAN, ovde zamenuvam GroupCOde, posto e prazen, da bide na lotus!!! Zameniv: phoneMessage.GroupCode = " + phoneMessage.GroupCode);

                        // ZORAN:   Ova go dodavam tuka za Dipecerot da ima info za adresata ako toj telefon veke postoi vo baza:
                        //          Dilema mi e dali da ja baram adresata tuka, ili vo moment na AN?

                        //          Celoto ke go stavam vo try/catch, bidejki ima nekolku mesta kade moze da zezne, 
                        //          a bilo kade da se sluci seedno e, ne e korisen ovoj del od kodot

                        try
                        {
                            GisPhoneNumbers gisPhoneNumber = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneMessage.PhoneNumber);

                            if (gisPhoneNumber != null) // Ako nema, nisto, si ostanuva null
                            {

                                // OK, ima zapis, vednas da si napravam DeepLoad po se sto mi treba:

                                Type[] tl = new Type[3];
                                tl[0] = typeof(GisObjects);
                                tl[1] = typeof(GisStreets);
                                tl[2] = typeof(GisRegions);

                                DataRepository.GisPhoneNumbersProvider.DeepLoad(gisPhoneNumber, true,  DeepLoadType.IncludeChildren, tl);

                                if (gisPhoneNumber.LocationType == 1)               // Znaci, ulica e!
                                {
                                    //phoneMessage.oAddressFrom.oGisAddressModel = GeneralFunctions.findGisAddressModelForIDStreetHouseNumber(gisPhoneNumber.IdStreet, gisPhoneNumber.HouseNumber ?? 0);

                                    phoneMessage.oAddressFrom.oGisStreets = gisPhoneNumber.IdStreetSource;
                                    if (gisPhoneNumber.HouseNumber == null)
                                        phoneMessage.oAddressFrom.HouseNumber = 0;
                                    else
                                        phoneMessage.oAddressFrom.HouseNumber = (int)gisPhoneNumber.HouseNumber;

                                }
                                else                                                // Znaci objekt e. Tuka treba poveke kontrola, no i vaka ke pomine
                                {
                                    phoneMessage.oAddressFrom.oGisObjects = gisPhoneNumber.IdObjectSource;
                                }


                                // Sega da gi stavam rabotite sto se zaednicki, nezavisno dali e adresa ili objekt

                                phoneMessage.oAddressFrom.oGisRegions = gisPhoneNumber.IdRegionSource;
                                phoneMessage.oAddressFrom.LocationX = (double)gisPhoneNumber.LongitudeX;
                                phoneMessage.oAddressFrom.LocationY = (double)gisPhoneNumber.LatitudeY;

                                if (gisPhoneNumber.LocationQuality != null) // Vo start site ke bidat null, bidejki e novo pole. Treba da se trgne ovaa kontrola pokasno...
                                    phoneMessage.oAddressFrom.LocationQuality = (int)gisPhoneNumber.LocationQuality;
                                else
                                    phoneMessage.oAddressFrom.LocationQuality = 4;
                            }
                        }

                        catch (Exception ex)
                        {
                            log.Error("ZORAN (problem vo obrabotka na RC od centrala): Nekoj problem so citaneje na GisPhoneNumbers", ex);
                        }




                        if (DataRepository.PhoneCallsProvider.Insert(phoneMessage))
                        {
                            activePhoneCalls.Add(phoneMessage);
                        }
                        else
                        {
                            log.Error("ERROR : Problem so baza ");
                        }


                        //if (_callBack.OnPhoneCallRecived != null)
                        //{
                        //    _callBack.OnPhoneCallRecived(this, phoneMessage);
                        //}

                        delegateHandler(this, phoneMessage);

                        //log.Debug("ZORAN: RC---" + phoneMessage.PhoneNumber + "--" + "OBRABOTIV");

                        StillWorking = false;
                        break;

                    case "AN":

                        //while (StillWorking == true) { }
                        //StillWorking = true;

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if ((phoneCall.PhoneNumber == phoneMessage.PhoneNumber) && phoneCall.MessageType == "AN" )
                            {
                                tmpPhoneCallForRemove = phoneCall;

                                string tmpString = phoneCall.RinglDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                                tmpString = tmpString + " (" + phoneCall.PhoneNumber.ToString() + ")";

                                tmpString = tmpString + ", Grupa (stara/nova): " + phoneCall.GroupCode.ToString() + "/" + phoneMessage.GroupCode.ToString();

                                tmpString = tmpString + ", Lokal (star/nov): " + phoneCall.Extension.ToString() + "/" + phoneMessage.Extension.ToString();

                                //log.Debug("Povtoren AN: " + tmpString);
                            }
                        }

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.RingDuration = phoneMessage.RingDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                phoneCall.IdUserInOut = getUserInOutForExtension(phoneCall.Extension);


                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }
                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                StillWorking = false;
                                break;
                            }
                        }

                        StillWorking = false;
                        break;

                    case "CC":

                        //while (StillWorking == true) { }
                        //StillWorking = true;

                        bool tmpFound = false;

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {
                                tmpFound = true;

                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.CallDuration = phoneMessage.CallDuration;
                                phoneCall.MessageType = phoneMessage.MessageType;

                                if (!DataRepository.PhoneCallsProvider.Update(phoneCall))
                                {
                                    log.Error("ERROR : Problem so baza ");
                                }

                                activePhoneCalls.Remove(phoneCall);
                                //if (_callBack.OnPhoneCallRecived != null)
                                //{
                                //    _callBack.OnPhoneCallRecived(this, phoneCall);
                                //}

                                delegateHandler(this, phoneCall);

                                //StillWorking = false;
                                break;
                            }
                        }

                        if (tmpFound == false)
                        {
                            string tmpString = phoneMessage.RinglDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                            tmpString = tmpString + " (" + phoneMessage.PhoneNumber.ToString() + ")";

                            tmpString = tmpString + ", Grupa: " + phoneMessage.GroupCode.ToString();

                            tmpString = tmpString + ", Lokal: " + phoneMessage.Extension.ToString();

                            //log.Debug("Povtoren CC: " + tmpString);
                        }


                        break;

                    case "CA":

                        //while (StillWorking == true) { }

                        //StillWorking = true;

                        PhoneCalls phoneCallTitka = new PhoneCalls();

                        phoneCallTitka.PhoneNumber = phoneMessage.PhoneNumber;
                        phoneCallTitka.Extension = phoneMessage.Extension;
                        phoneCallTitka.GroupCode = phoneMessage.GroupCode;
                        phoneCallTitka.MessageType = phoneMessage.MessageType;
                        //phoneCallTitka.RinglDateTime = phoneMessage.RinglDateTime;
                        phoneCallTitka.RinglDateTime = System.DateTime.Now;
                        phoneCallTitka.RingDuration = phoneMessage.RingDuration;
                        phoneCallTitka.CallDuration = phoneMessage.CallDuration;
                        phoneCallTitka.oAddressFrom = phoneMessage.oAddressFrom;

                        phoneCallTitka.IdUserInOut = getUserInOutForExtension(phoneCallTitka.Extension);


                        // ZORAN:   Ova go dodavam tuka za Dipecerot da ima info za adresata ako toj telefon veke postoi vo baza:
                        //          Dilema mi e dali da ja baram adresata tuka, ili vo moment na AN?

                        //          Celoto ke go stavam vo try/catch, bidejki ima nekolku mesta kade moze da zezne, 
                        //          a bilo kade da se sluci seedno e, ne e korisen ovoj del od kodot

                        try
                        {
                            GisPhoneNumbers gisPhoneNumber = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneCallTitka.PhoneNumber);

                            if (gisPhoneNumber != null) // Ako nema, nisto, si ostanuva null
                            {

                                // OK, ima zapis, vednas da si napravam DeepLoad po se sto mi treba:

                                Type[] tl = new Type[3];
                                tl[0] = typeof(GisObjects);
                                tl[1] = typeof(GisStreets);
                                tl[2] = typeof(GisRegions);

                                DataRepository.GisPhoneNumbersProvider.DeepLoad(gisPhoneNumber, true,  DeepLoadType.IncludeChildren, tl);

                                if (gisPhoneNumber.LocationType == 1)               // Znaci, ulica e!
                                {
                                    //phoneCallTitka.oAddressFrom.oGisAddressModel = GeneralFunctions.findGisAddressModelForIDStreetHouseNumber(gisPhoneNumber.IdStreet, gisPhoneNumber.HouseNumber ?? 0);

                                    phoneCallTitka.oAddressFrom.oGisStreets = gisPhoneNumber.IdStreetSource;
                                    if (gisPhoneNumber.HouseNumber == null)
                                        phoneCallTitka.oAddressFrom.HouseNumber = 0;
                                    else
                                        phoneCallTitka.oAddressFrom.HouseNumber = (int)gisPhoneNumber.HouseNumber;

                                }
                                else                                                // Znaci objekt e. Tuka treba poveke kontrola, no i vaka ke pomine
                                {
                                    phoneCallTitka.oAddressFrom.oGisObjects = gisPhoneNumber.IdObjectSource;
                                }


                                // Sega da gi stavam rabotite sto se zaednicki, nezavisno dali e adresa ili objekt

                                phoneCallTitka.oAddressFrom.oGisRegions = gisPhoneNumber.IdRegionSource;
                                phoneCallTitka.oAddressFrom.LocationX = (double)gisPhoneNumber.LongitudeX;
                                phoneCallTitka.oAddressFrom.LocationY = (double)gisPhoneNumber.LatitudeY;

                                if (gisPhoneNumber.LocationQuality != null) // Vo start site ke bidat null, bidejki e novo pole. Treba da se trgne ovaa kontrola pokasno...
                                    phoneCallTitka.oAddressFrom.LocationQuality = (int)gisPhoneNumber.LocationQuality;
                                else
                                    phoneCallTitka.oAddressFrom.LocationQuality = 4;
                            }
                        }

                        catch (Exception ex)
                        {
                            log.Error("ZORAN (problem vo obrabotka na CA od centrala): Nekoj problem so citaneje na GisPhoneNumbers", ex);
                        }


                        if (!DataRepository.PhoneCallsProvider.Insert(phoneCallTitka))
                        {
                            log.Error("ERROR : Problem so baza ");
                        }                        

                        delegateHandler(this, phoneCallTitka);
                        
                        //StillWorking = false;
                        break;

                    case "PA":      // ZORAN: Ova e prezemanje na povik od drug dispecer. Oni go vikaat "Parkiranje"

                        //while (StillWorking == true) { }
                        //StillWorking = true;

                        PhoneCalls phoneCallTaken = new PhoneCalls();

                        phoneCallTaken.PhoneNumber = phoneMessage.PhoneNumber;
                        phoneCallTaken.Extension = phoneMessage.Extension;
                        phoneCallTaken.GroupCode = phoneMessage.GroupCode;
                        phoneCallTaken.MessageType = phoneMessage.MessageType;
                        phoneCallTaken.RinglDateTime = phoneMessage.RinglDateTime;
                        //phoneCallTaken.RinglDateTime = System.DateTime.Now;
                        phoneCallTaken.RingDuration = phoneMessage.RingDuration;
                        phoneCallTaken.CallDuration = phoneMessage.CallDuration;
                        phoneCallTaken.oAddressFrom = phoneMessage.oAddressFrom;

                        phoneCallTaken.IdUserInOut = getUserInOutForExtension(phoneCallTaken.Extension);


                        // ZORAN:   Ova go dodavam tuka za Dipecerot da ima info za adresata ako toj telefon veke postoi vo baza:
                        //          Dilema mi e dali da ja baram adresata tuka, ili vo moment na AN?

                        //          Celoto ke go stavam vo try/catch, bidejki ima nekolku mesta kade moze da zezne, 
                        //          a bilo kade da se sluci seedno e, ne e korisen ovoj del od kodot

                        try
                        {
                            GisPhoneNumbers gisPhoneNumber = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneCallTaken.PhoneNumber);

                            if (gisPhoneNumber != null) // Ako nema, nisto, si ostanuva null
                            {

                                // OK, ima zapis, vednas da si napravam DeepLoad po se sto mi treba:

                                Type[] tl = new Type[3];
                                tl[0] = typeof(GisObjects);
                                tl[1] = typeof(GisStreets);
                                tl[2] = typeof(GisRegions);

                                DataRepository.GisPhoneNumbersProvider.DeepLoad(gisPhoneNumber, true,  DeepLoadType.IncludeChildren, tl);

                                if (gisPhoneNumber.LocationType == 1)               // Znaci, ulica e!
                                {
                                    //phoneCallTaken.oAddressFrom.oGisAddressModel = GeneralFunctions.findGisAddressModelForIDStreetHouseNumber(gisPhoneNumber.IdStreet, gisPhoneNumber.HouseNumber ?? 0);

                                    phoneCallTaken.oAddressFrom.oGisStreets = gisPhoneNumber.IdStreetSource;
                                    if (gisPhoneNumber.HouseNumber == null)
                                        phoneCallTaken.oAddressFrom.HouseNumber = 0;
                                    else
                                        phoneCallTaken.oAddressFrom.HouseNumber = (int)gisPhoneNumber.HouseNumber;

                                }
                                else                                                // Znaci objekt e. Tuka treba poveke kontrola, no i vaka ke pomine
                                {
                                    phoneCallTaken.oAddressFrom.oGisObjects = gisPhoneNumber.IdObjectSource;
                                }


                                // Sega da gi stavam rabotite sto se zaednicki, nezavisno dali e adresa ili objekt

                                phoneCallTaken.oAddressFrom.oGisRegions = gisPhoneNumber.IdRegionSource;
                                phoneCallTaken.oAddressFrom.LocationX = (double)gisPhoneNumber.LongitudeX;
                                phoneCallTaken.oAddressFrom.LocationY = (double)gisPhoneNumber.LatitudeY;

                                if (gisPhoneNumber.LocationQuality != null) // Vo start site ke bidat null, bidejki e novo pole. Treba da se trgne ovaa kontrola pokasno...
                                    phoneCallTaken.oAddressFrom.LocationQuality = (int)gisPhoneNumber.LocationQuality;
                                else
                                    phoneCallTaken.oAddressFrom.LocationQuality = 4;
                            }
                        }

                        catch (Exception ex)
                        {
                            log.Error("ZORAN (problem vo obrabotka na PA od centrala): Nekoj problem so citaneje na GisPhoneNumbers", ex);
                        }


                        if (!DataRepository.PhoneCallsProvider.Insert(phoneCallTaken))
                        {
                            log.Error("ERROR : Problem so baza ");
                        }

                        delegateHandler(this, phoneCallTaken);

                        //StillWorking = false;
                        break;


                    case "AC":              // ZORAN:   Ova e koga ke podigne linika sto ja ostavil na cekanje

                        while (StillWorking == true) { }
                        StillWorking = true;

                        tmpFound = false;

                        foreach (PhoneCalls phoneCall in activePhoneCalls)
                        {
                            if (phoneCall.PhoneNumber == phoneMessage.PhoneNumber)
                            {                               

                                // ZORAN: Vo praksa, samo MessageType i CallDuration treba da se azuriraat
                                phoneCall.PhoneNumber = phoneMessage.PhoneNumber;
                                phoneCall.Extension = phoneMessage.Extension;
                                phoneCall.GroupCode = phoneMessage.GroupCode;
                                phoneCall.MessageType = phoneMessage.MessageType;
                                phoneCall.RinglDateTime = phoneMessage.RinglDateTime;
                                phoneCall.RingDuration = phoneCall.RingDuration;
                                phoneCall.CallDuration = phoneCall.CallDuration;
                                phoneCall.oAddressFrom = phoneMessage.oAddressFrom;
                                phoneCall.IdUserInOut = getUserInOutForExtension(phoneCall.Extension);

                                delegateHandler(this, phoneCall);

                                tmpFound = true;

                                StillWorking = false;
                                break;
                            }
                        }

                        if (tmpFound == false)
                            log.Debug("Dobien e AC za broj koj ne postoi (" + phoneMessage.PhoneNumber + ")");
                        StillWorking = false;                            
                        break;

                    default:
                        //newMsg.MessageType = PhoneExchangeMessageType.NN.ToString();
                        StillWorking = false;
                        break;


                }

            }
            catch (IOException ioex)
            {
                log.Error("ERROR ", ioex);
            }
            catch (UnauthorizedAccessException uaex)
            {
                log.Error("ERROR ", uaex);
            }
            catch (Exception e) //TimeoutException)
            {
                log.Error("ERROR ", e);
            }

            return;
        }


# endregion



        // Funkcija za generiranje na CALL
        // Se povikuva od Dispecer. Na ovoj nacin i racno generiranite CALL-ovi se evidentiraat vo bazata
        // Se praka do site dispeceri, a "go gleda" samo dispecerot prijaven na toj extension
        // 
        // Ne se stava vo listata na tekovni povici activePhoneCalls, nema logika
        // ***************************************************************************************************

        public override void GeneratePhoneCall(string strPhoneNumber, string strPhoneExtension)
        {

            //if ((strPhoneNumber != "01") && (strPhoneNumber != "02") && (strPhoneNumber != "03"))
            //    return;

            PhoneCalls phoneMessage = new PhoneCalls();

            phoneMessage.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();

            phoneMessage.PhoneNumber = strPhoneNumber;
            phoneMessage.Extension = strPhoneExtension;

            phoneMessage.MessageType = "GC"; //Demek, od Generated Call...
            phoneMessage.LineIn = 0;
            phoneMessage.CallDuration = 0;
            phoneMessage.RingDuration = 0;
            phoneMessage.RinglDateTime = System.DateTime.Now;
            phoneMessage.IdUserInOut = getUserInOutForExtension(strPhoneExtension);
           

            switch (strPhoneNumber.Substring(0,2) )
            {
                case "01":
                    phoneMessage.GroupCode = "291";
                    break;
                case "02":
                    phoneMessage.GroupCode = "292";
                    break;
                case "03":
                    phoneMessage.GroupCode = "100";
                    break; 
                default:
                    phoneMessage.GroupCode = "777";
                    break;
            }


            // ZORAN:   Ova e identicno kako za RC

            try
            {
                GisPhoneNumbers gisPhoneNumber = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneMessage.PhoneNumber);

                if (gisPhoneNumber != null) // Ako nema, nisto, si ostanuva null
                {

                    // OK, ima zapis, vednas da si napravam DeepLoad po se sto mi treba:

                    Type[] tl = new Type[3];
                    tl[0] = typeof(GisObjects);
                    tl[1] = typeof(GisStreets);
                    tl[2] = typeof(GisRegions);

                    DataRepository.GisPhoneNumbersProvider.DeepLoad(gisPhoneNumber, true,  DeepLoadType.IncludeChildren, tl);

                    if (gisPhoneNumber.LocationType == 1)               // Znaci, ulica e!
                    {
                        //phoneMessage.oAddressFrom.oGisAddressModel = GeneralFunctions.findGisAddressModelForIDStreetHouseNumber(gisPhoneNumber.IdStreet, gisPhoneNumber.HouseNumber ?? 0);

                        phoneMessage.oAddressFrom.oGisStreets = gisPhoneNumber.IdStreetSource;
                        if ( gisPhoneNumber.HouseNumber == null)
                            phoneMessage.oAddressFrom.HouseNumber = 0;
                        else
                            phoneMessage.oAddressFrom.HouseNumber = (int)gisPhoneNumber.HouseNumber;
                    }
                    else                                                // Znaci objekt e. Tuka treba poveke kontrola, no i vaka ke pomine
                    {
                        phoneMessage.oAddressFrom.oGisObjects = gisPhoneNumber.IdObjectSource;
                    }

                    // Sega da gi stavam rabotite sto se zaednicki, nezavisno dali e adresa ili objekt

                    phoneMessage.oAddressFrom.oGisRegions = gisPhoneNumber.IdRegionSource;
                    phoneMessage.oAddressFrom.LocationX = (double)gisPhoneNumber.LongitudeX;
                    phoneMessage.oAddressFrom.LocationY = (double)gisPhoneNumber.LatitudeY;

                    if (gisPhoneNumber.LocationQuality != null) // Vo start site ke bidat null, bidejki e novo pole. Treba da se trgne ovaa kontrola pokasno...
                        phoneMessage.oAddressFrom.LocationQuality = (int)gisPhoneNumber.LocationQuality;
                    else
                        phoneMessage.oAddressFrom.LocationQuality = 4;
                }
            }

            catch (Exception ex)
            {
                log.Error("ZORAN (problem vo obrabotka na GC od centrala): Nekoj problem so citaneje na GisPhoneNumbers", ex);
            }


            try
            {
                DataRepository.PhoneCallsProvider.Insert(phoneMessage);
            }
            catch (Exception ex)
            {
                log.Error("ZORAN (problem vo obrabotka na GC od centrala): Greska so upis na Generiraniot CALL vo baza", ex);
            }

            //if (_callBack.OnPhoneCallRecived != null)
            //{
            //    _callBack.OnPhoneCallRecived(this, phoneMessage);
            //}

            delegateHandler(this, phoneMessage);
        }



        


        private long getUserInOutForExtension(string pPhoneExt)
        {
             TList<UserInOut> lstUserInOut = null;

            try
            {
                UserInOutParameterBuilder searchUserInOut = new UserInOutParameterBuilder();

                searchUserInOut.AppendEquals(UserInOutColumn.PhoneExtension, pPhoneExt);
                searchUserInOut.AppendIsNull(UserInOutColumn.DateTimeOut);


                lstUserInOut = DataRepository.UserInOutProvider.Find(searchUserInOut.GetParameters());
            }
            catch (Exception ex)
            {

            }

            if (lstUserInOut != null && lstUserInOut.Count >= 1)
                return lstUserInOut[0].IdUserLogInOut;
            else
                return 0;
        }



        //ZORAN:    Samo prntam vo log postoeckiot PhoneCall (vo lista) i novo dojdeniot, nezavisno sto e problemot (ke si se vidi vo log)
        private void PrintPhoneMessageErrorLog(PhoneCalls pPhoneCallOld, PhoneCalls pPhoneCallNew)
        {
            log.Debug(
                
                "PhoneCall na server:" + Environment.NewLine + "\t" +

                    pPhoneCallOld.PhoneNumber + Environment.NewLine + "\t" +
                    pPhoneCallOld.Extension + Environment.NewLine + "\t" +
                    pPhoneCallOld.GroupCode + Environment.NewLine + "\t" +
                    pPhoneCallOld.MessageType + Environment.NewLine + "\t" +
                    pPhoneCallOld.RinglDateTime.ToString("HH:mm:ss  dd-mm-yyyy") + Environment.NewLine + "\t" +
                    pPhoneCallOld.RingDuration.Value.ToString("0000") + Environment.NewLine + "\t" +
                    pPhoneCallOld.CallDuration.Value.ToString("0000") + Environment.NewLine + Environment.NewLine +                 
                
                 "PhoneCall nov:" + Environment.NewLine + "\t" +

                    pPhoneCallNew.PhoneNumber + Environment.NewLine + "\t" +
                    pPhoneCallNew.Extension + Environment.NewLine + "\t" +
                    pPhoneCallNew.GroupCode + Environment.NewLine + "\t" +
                    pPhoneCallNew.MessageType + Environment.NewLine + "\t" +
                    pPhoneCallNew.RinglDateTime.ToString("HH:mm:ss  dd-mm-yyyy") + Environment.NewLine + "\t" +
                    pPhoneCallNew.RingDuration.Value.ToString("0000") + Environment.NewLine + "\t" +
                    pPhoneCallNew.CallDuration.Value.ToString("0000") + Environment.NewLine                                
                );



        }


    }
}
