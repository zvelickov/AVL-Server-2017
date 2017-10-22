using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;
using System.IO.Ports;

using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;

using log4net;

namespace Taxi.Communication.Server.HttpListener
{
    public class SmsListener
    {
        private static ILog log;


        private SerialPort SMSPort;
        public static bool _Continue = false;
        public static bool _ContSMS = false;
        public static bool _ReadPort = false;
        public delegate void SendingEventHandler(bool Done);
        public delegate void DataReceivedEventHandler(string Message);

        System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();

        TList<SmSsent> lstSmSsent = null;

        //Ovie 4 variabli mi se samo za test sto se vrakja posle pratenSMS.
        //
        public string LastPhoneNumberSentTo = "";
        public string LastSMSmessageReceived = "";
        public DateTime LastPhoneNumberSendTime = System.DateTime.Now;
        public DateTime LastSMSreceivedTime = System.DateTime.Now;
        

        public void start()
        {
            log = LogManager.GetLogger("MyService");

            string port = System.Configuration.ConfigurationManager.AppSettings["commPortId"];
            int baud = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["commPortBaud"]);

            InitializeComPort(port, baud);

            try
            {

                Console.WriteLine("Listening SmsListener ...");



                while (true)
                {
                    lstSmSsent = DataRepository.SmSsentProvider.GetUnSentSmsMessages();

                    if (lstSmSsent != null && lstSmSsent.Count > 0)
                    {
                        foreach (SmSsent tmpSmsSent in lstSmSsent)
                        {
                            SendSMS(tmpSmsSent.PhoneNumber, tmpSmsSent.SmStext);

                            //Slednive 2reda samo zaDEBUG, treba da se trgnat...
                            LastPhoneNumberSentTo = tmpSmsSent.PhoneNumber;
                            LastPhoneNumberSendTime = System.DateTime.Now;

                            Thread.Sleep(5000);

                            tmpSmsSent.DateTimeSent = System.DateTime.Now;
                            DataRepository.SmSsentProvider.Update(tmpSmsSent);
                        }
                    }

                    lstSmSsent = null;

                    Thread.Sleep(5000);                   
                }
            }
            catch (Exception se)
            {
                log.Error(se.Message);
            }
        }


        private void InitializeComPort(string port, int baud)
        {
            try 
            {
                SMSPort = new SerialPort(port, baud);
                SMSPort.Parity = Parity.None;
                SMSPort.StopBits = StopBits.One;
                SMSPort.DataBits = 8;
                SMSPort.Handshake = Handshake.RequestToSend;
                SMSPort.DtrEnable = true;
                SMSPort.RtsEnable = true;
                SMSPort.NewLine = System.Environment.NewLine;
                SMSPort.ReceivedBytesThreshold = 1;
                SMSPort.DataReceived += new SerialDataReceivedEventHandler(OnSerialDataReceived);

                Open();
            }
            catch (Exception ex)
            {
                log.Error("SMS error (InitializeComPort): ", ex);
            }

            //ReadThread = new Thread(
            //    new System.Threading.ThreadStart(ReadPort));


        }


        public void OnSerialDataReceived(object sender, SerialDataReceivedEventArgs args)
        {

            try
            {
                string message = SMSPort.ReadLine();

                //Samo za debug, trebada se trgne !!!
                if (LastPhoneNumberSendTime > LastSMSreceivedTime)   //Znaci deka se pratilo do nov broj
                {                    
                    LastSMSmessageReceived = "";
                }

                else
                    LastSMSmessageReceived = LastSMSmessageReceived + message;
                                          
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

            LastSMSreceivedTime = System.DateTime.Now;

            return;
        }



        public bool SendSMS(string CellNumber, string SMSMessage)
        {
            bool retVal = false;

            string MyMessage = null;

            try
            {
                if (SMSMessage.Length <= 160)
                    MyMessage = SMSMessage;
                else
                    MyMessage = SMSMessage.Substring(0, 160);

               
                if (SMSPort.IsOpen == true)
                {
                    //SMSPort.WriteLine("AT+CMGF=1" + System.Environment.NewLine);

                    SMSPort.WriteLine("AT+CMGS=" + CellNumber + System.Environment.NewLine);

                    SMSPort.WriteLine(MyMessage + System.Environment.NewLine + (char)(26));                    
                }

                retVal = true;
            }
            catch (Exception ex)
            {
                log.Error("SMS error: ", ex);
            }

            return retVal;
        }


        public void Open()
        {
            if (SMSPort.IsOpen == false)
            {
                SMSPort.Open();
                //ReadThread.Start();
            }
        }

        public void Close()
        {
            if (SMSPort.IsOpen == true)
            {
                SMSPort.Close();
            }
        }    
    }
}
