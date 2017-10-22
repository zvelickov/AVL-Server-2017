using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
//using JP.Data.Utils;
using log4net;
using Taxi.Communication.Server.MessageParsers;

using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.Containers;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.Utils.Parsers;
using Taxi.Communication.Server.MessageParsers.ControlElectronics;
using Taxi.Communication.Server.EnrichMessage;
using Taxi.Communication.Server.BL;
using Taxi.Communication.Server.Utils.Containers;



namespace Taxi.Communication.Server.ConnectionListeners
{

    public class GPSListener
    {

        //Synchronization object
        private object _sync = new object();

        private bool m_work;
        private int pingTime = 0;

        ////public delegate void UpdateRichEditCallback(string text);
        ////public delegate void UpdateClientListCallback();
        private ILog log;

        public AsyncCallback pfnWorkerCallBack;
        private Socket m_mainSocket;

        private MapUtils _mapUtils;

        
        private System.Text.ASCIIEncoding AscEnc = new System.Text.ASCIIEncoding();
        private SynchQueue<Locations> m_locQueue;
        private SynchQueue<long> _queueIdVehicle;

        ICallbacksForGPSmessageRecived _callBack;

        private clsMessageCreator tmpMessageCreator = new clsMessageCreator();

        // An ArrayList is used to keep track of worker sockets that are designed
        // to communicate with each connected client. 
        // Make it a synchronized ArrayList for thread safety
        private System.Collections.ArrayList m_workerSocketList =
                ArrayList.Synchronized(new System.Collections.ArrayList());

        private Dictionary<long, Socket> m_VehicleSockets = new Dictionary<long, Socket>();

        // ZORAN:   Ova se konfirmacii za BB... Trebaat samo za naracki, za drugo ne se koristat
        //          za Value == true znaci deka "zeleno svetlo" i moze da se praka
        private Dictionary<long, bool> m_ReceivedSentConfirmations = new Dictionary<long, bool>();

        private GPSMessageRecived gpsHandler;

        public void setCallBack(ICallbacksForGPSmessageRecived callBack)
        {
            _callBack = callBack;
        }

        public GPSListener(MapUtils mapUtils, SynchQueue<Locations> locQueue, SynchQueue<long> queue)
        {
            m_work = true;

            _mapUtils = mapUtils;
            m_locQueue = locQueue;
            _queueIdVehicle = queue;
           


            log = LogManager.GetLogger("MyService");
            log.Debug("TEST");

            List<Vehicle> lstVehicles = VehiclesContainer.Instance.getAllVehicles();

            foreach (Vehicle veh in lstVehicles)
                m_ReceivedSentConfirmations.Add(veh.IdVehicle, true);

            lstVehicles = null;
        }

        public void Start()
        {
            log.Debug("Staring GPS Listener....");

            try
            {
                gpsHandler = _callBack.CallBackGPSMessageRecived;

                int port = Int16.Parse(System.Configuration.ConfigurationManager.AppSettings["socektPort"]);
                // Create the listening socket...
                m_mainSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, port);

                // Bind to local IP Address...
                m_mainSocket.Bind(ipLocal);

                // Start listening...
                // JOVE da proveri sto e 500???
                m_mainSocket.Listen(5000);

                // Create the call back for any client connections...
                m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
            }

            catch (SocketException se)
            {
                log.Error("ERROR ", se);
            }

            while (m_work)
            {
                Thread.Sleep(2000);


                try
                {                   
                    pingTime++;

                    if ((pingTime > 20))
                    {
                        //SendPingMsg();                        
                       
                        Thread ThreadSendPingMsg = new Thread(delegate() { SendPingMsg(); });

                        ThreadSendPingMsg.Start();
                       
                        pingTime = 0;
                    }
                   
                    Dictionary<Vehicle, List<byte[]>> lstVeh = VehiclesContainer.Instance.decreaseTimeOutToAll();                                        

                    foreach (KeyValuePair<Vehicle, List<byte[]>> vehPair in lstVeh)
                    {
                        List<byte[]> lstByte = vehPair.Value;

                        if(lstByte != null && lstByte.Count > 0)
                            foreach(byte[] bt in lstByte)
                                if (this.SendMsgToVehicle(vehPair.Key.IdVehicle, bt) == -1)
                                    {
                                        log.Error("GRESKA vo prakanje poraka (DescreaseTimeout) na vozilo: " + vehPair.Key.DescriptionShort);
                                    }

                        gpsHandler(vehPair.Key.IdCompany, vehPair.Key);

                        //Otkako ke se prari promenetoto vozilo, oznaci deka Ne treba vo sledniot krug
                        VehiclesContainer.Instance.clearStateChanged(vehPair.Key.IdVehicle);
                    }

                   
                    Thread ThreadUpdateDispechers = new Thread(delegate() { UpdateDispechers(); });

                    ThreadUpdateDispechers.Start();

                   
                    //ZORAN:    Sega se pravi decrease na vremeto na tekovni Orders
                    //          Metodata vraka Dictionary<Vehicle, byte[]>. kade sto:
                    //              1. Ako e NULL, togas nikoj ne odgovoril. 
                    //              2. Vo  byte[] se sodrzi porakata za sekoe od vozilata. Pri toa:
                    //                  2.1.    Za prvoto vozilo (SELEKTIRANO za konkretniot Order) se sodrzi polna adresa
                    //                  2.2.    Za ostanatite, ako gi ima, info koe vozilo e prateno

                    List<Dictionary<long, byte[]>> lstSelectedVehiclesForOrder = VehiclesContainer.Instance.decreaseTimeForOrders();

                    if (lstSelectedVehiclesForOrder != null && lstSelectedVehiclesForOrder.Count > 0)
                    {
                        foreach (Dictionary<long, byte[]> dict in lstSelectedVehiclesForOrder)
                        {
                            foreach (KeyValuePair<long, byte[]> kvp in dict)
                            {                                                                
                                this.SendMsgToVehicle(kvp.Key, kvp.Value);
                                Thread.Sleep(100);
                            }
                        }
                    }
                   

                    //ZORAN:    Sega se pravi proverka dali ima aktivni PendingPhoneCall
                    //          Ako ima, metodata vraka Dictionary<Vehicle, byte[]>. kade sto:
                    //                  1. Selektira koli za dadeniot PhoneCall
                    //                  2. Gi stava PhoneCall + site selektiraniKoli vo Dictionary
                    //                  3. Generira Kratka Najava i ja vraka kako Dictionary
                    //          Od ovde se praka samo kratka najava, a se drugo odi standardno, kako i za regularni selekcii    
                    List<Dictionary<long, byte[]>> lstCheckForPendingPhoneCalls = VehiclesContainer.Instance.CheckForPendingPhoneCalls();

                    if (lstCheckForPendingPhoneCalls != null && lstCheckForPendingPhoneCalls.Count > 0)
                    {
                        foreach (Dictionary<long, byte[]> dict in lstCheckForPendingPhoneCalls)
                        {
                            foreach (KeyValuePair<long, byte[]> kvp in dict)
                            {
                                this.SendMsgToVehicle(kvp.Key, kvp.Value);
                                Thread.Sleep(100);
                            }
                        }
                    }

                    //ZORAN:    Sega samo proverka za missed PendingPhoneCalls
                    //          Toa se tie za koi pominalo vremeto za baranje vozilo, a ne im se naslo
                    //          NE se pravi nisto posebno, samo im se prakja poraka na klientite i se azurira vo baza
                    //          Se mislam dali da go pustam kako poseben thread. Ako ima poveke, da ne se gubi vreme. Ajde, sega vaka neka odi..

                    VehiclesContainer.Instance.CheckForMissedPendingPhoneCalls();
                   
                }
                catch (Exception ex)
                {
                    log.Error("ZORAN:   Ova e greska vo WHILE na GpsListener Thread-ot (VAZNO!!!)", ex);
                }
            }          
        }


        public void UpdateDispechers()
        {
           
            List<Vehicle> lstVehicles = VehiclesContainer.Instance.getAllVehicles();

            foreach (Vehicle veh in lstVehicles)
            {
                if (veh.StateChanged)
                {
                    gpsHandler(veh.IdCompany, veh);
                    if (veh.DirtyStateChange)
                    {
                        if (veh.currentState.GetType() == typeof(Taxi.Communication.Server.StateMachine.StateIdle))
                        {
                            _queueIdVehicle.Enqueue(veh.IdVehicle);
                        }
                    }
                    //Otkako ke se prari promenetoto vozilo, oznaci deka Ne treba vo sledniot krug
                    VehiclesContainer.Instance.clearStateChanged(veh.IdVehicle);
                }
            }           
        }
        

        public void Stop()
        {
            this.m_work = false;
            CloseSockets();
        }


        // This is the call back function, which will be invoked when a client is connected
        public void OnClientConnect(IAsyncResult asyn)
        {

            log.Error("KONEKCIJA !!!!!! ");

            if (this.m_work == false)
            {
                return;
            }

            lock (_sync)
            {
                try
                {

                    Socket workerSocket = null;

                    // Here we complete/end the BeginAccept() asynchronous call by calling EndAccept()
                    // which returns the reference to a new Socket object
                    workerSocket = m_mainSocket.EndAccept(asyn);

                  
                    // Add the workerSocket reference to our ArrayList
                    m_workerSocketList.Add(workerSocket);

                    workerSocket.Send(AscEnc.GetBytes("OK"));

                    Console.WriteLine("Client " + " Connected:  " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm") + "\n");


                    // Let the worker Socket do the further processing for the just connected client
                    WaitForData(workerSocket, null);

                    // Since the main Socket is now free, it can go back and wait for other clients who are attempting to connect                    
                }

                catch (ObjectDisposedException oe)
                {
                    log.Error("ERROR ", oe);
                }

                catch (SocketException se)
                {
                    log.Error("ERROR ", se);
                }

                finally
                {
                    m_mainSocket.BeginAccept(new AsyncCallback(OnClientConnect), null);
                }
            }
        }

        // Start waiting for data from the client
        public void WaitForData(System.Net.Sockets.Socket soc, SocketPacket theSocPkt)
        {
            try
            {
                if (pfnWorkerCallBack == null)
                {
                    // Specify the call back function which is to be invoked when there is any write activity by the connected client
                    pfnWorkerCallBack = new AsyncCallback(OnDataReceived);
                }


                if (theSocPkt == null)
                {
                    theSocPkt = new SocketPacket(soc);
                }

                // Jove da vidi sto ovde tocno se slucuva...?

                Array.Clear(theSocPkt.dataBuffer, 0, theSocPkt.dataBuffer.Length);

                soc.BeginReceive(theSocPkt.dataBuffer, 0,
                    theSocPkt.dataBuffer.Length,
                    SocketFlags.None,
                    pfnWorkerCallBack,
                    theSocPkt);
            }
            catch (SocketException se)
            {
                log.Error("ERROR ", se);
            }
        }

        // This the call back function which will be invoked when the socket detects any client writing of data on the stream
        public void OnDataReceived(IAsyncResult asyn)
        {
            SocketPacket socketData = (SocketPacket)asyn.AsyncState;

            if (!socketData.CurrentSocket.Connected)
            {
                log.Warn("Zoran! Dobien e event OnDataReceived na socked koj e vo status NotConnected !!!!");
                return;
            }

            try
            {
                // Complete the BeginReceive() asynchronous call by EndReceive() method which will return the number of 
                // characters written to the stream by the client
                int bytesRead = socketData.CurrentSocket.EndReceive(asyn);

                Socket client = socketData.CurrentSocket;


                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.

                    Array.Resize(ref socketData.SocketMessageBuffer, socketData.SocketMessageBuffer.Length + bytesRead);

                    Array.Copy(socketData.dataBuffer, 0, socketData.SocketMessageBuffer, socketData.SocketMessageBuffer.Length - bytesRead, bytesRead);

                }
                else
                {
                    // ZORAN: Tuka vleguva koga ke se iskluci socketot bez odjava.
                    // Mozebi treba i formalno da se zatvori socketot? JOVE?

                    // WaitForData(socketData.m_currentSocket, socketData);
                    //log.Error("Zoran:   socket-ot se isklucil bez najava.... DA SE PROVERI OVAA SITUACIJA!!!");
                    return;
                }
                

                CEDeviceMessageParser parser = new CEDeviceMessageParser();
             
                List<DeviceMessage> msg = new List<DeviceMessage>();

                try
                {
                    msg = parser.ParseMessageHeader(socketData);                    
                }
                catch (Exception e)
                {
                    log.Error("ERROR ", e);
                    msg = null;
                }

                if (msg != null)
                {


                    foreach (DeviceMessage deviceMessage in msg)
                    {
                        string tmpString = deviceMessage.MessageIndicator + deviceMessage.Command;

                        //if (tmpString != "BB34" && tmpString != "BB35" && tmpString != "BB36")
                        if(deviceMessage.MessageIndicator != "BB")
                        {
                            long temp_IdVehicle;

                            temp_IdVehicle = VehiclesContainer.Instance.getVehicleIDForUnitSerial(deviceMessage.DeviceNumber);

                            if (temp_IdVehicle != -1)
                            {
                                lock (_sync)
                                {
                                    if (!m_VehicleSockets.ContainsKey(temp_IdVehicle))
                                    {
                                        m_VehicleSockets.Add(temp_IdVehicle, socketData.CurrentSocket);                                        

                                        VehicleConnections mVehicleConnection = new VehicleConnections();

                                        mVehicleConnection.IdVehicle = temp_IdVehicle;
                                        mVehicleConnection.ConnectionDateTime = System.DateTime.Now;
                                        mVehicleConnection.ConnectionType = 1;  //Nova konekcija

                                        DataRepository.VehicleConnectionsProvider.Insert(mVehicleConnection);
                                    }
                                    else
                                    {
                                        if (m_VehicleSockets[temp_IdVehicle] != socketData.CurrentSocket)
                                        {
                                            m_VehicleSockets[temp_IdVehicle].Close();
                                            m_workerSocketList.Remove(m_VehicleSockets[temp_IdVehicle]);
                                            m_VehicleSockets[temp_IdVehicle] = socketData.CurrentSocket;                                            

                                            VehicleConnections mVehicleConnection = new VehicleConnections();

                                            mVehicleConnection.IdVehicle = temp_IdVehicle;
                                            mVehicleConnection.ConnectionDateTime = System.DateTime.Now;
                                            mVehicleConnection.ConnectionType = 2;  //Butanje stara i otvaranje nova konekcija

                                            DataRepository.VehicleConnectionsProvider.Insert(mVehicleConnection);
                                        }
                                    }
                                }

                                CEParser msgParser = new CEParser();

                                EnrichManager enricher = new EnrichManager(this, this._mapUtils);

                                ParserResponseContainer parserResponce = null;
                                 
                                try
                                {
                                    parserResponce = msgParser.parseData(deviceMessage);
                                }
                                catch (Exception ex)
                                {
                                    parserResponce = null;
                                    log.Error(" Message " + AscEnc.GetString(deviceMessage.data), ex);
                                }

                                if (parserResponce != null)
                                {
                                    //stavi ja i porakta
                                    parserResponce.msg = deviceMessage;

                                    //if(parserResponce.msg.MessageIndicator != 
                                    if (enricher.doEnrichMessage(parserResponce) == -1)
                                        log.Error("error in enrich");

                                    Vehicle blVeh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(deviceMessage.DeviceNumber);


                                    IBL bl = BLFactory.createBL(blVeh.IdCompany);

                                    if (bl != null)
                                    {
                                        parserResponce.clearUnitToMsg();
                                        parserResponce = bl.processBL(parserResponce, blVeh.IdVehicle);
                                        if (parserResponce.tSendToUnitMsg != null)
                                        {
                                            if (this.SendMsgToVehicle(blVeh.IdVehicle, parserResponce.tSendToUnitMsg) == -1)
                                                log.Error("GRESKA vo prakanje poraka na vozilo: " + blVeh.IdVehicle.ToString() + "URED: " + deviceMessage.DeviceNumber);
                                        }
                                    }

                                    if (parserResponce.tLocation != null)
                                        m_locQueue.Enqueue(parserResponce.tLocation);
                                }
                                else
                                    log.Error("error in parse for message - " + deviceMessage.MessageIndicator + deviceMessage.Command + deviceMessage.DeviceNumber);
                            }

                            else
                            {
                                log.Warn("INFO : Ne postoi vozilo so DeviceNumber" + deviceMessage.DeviceNumber);
                            }
                        }
                        else
                        {
                            long tmpIdVehicle;

                            tmpIdVehicle = VehiclesContainer.Instance.getVehicleIDForUnitSerial(deviceMessage.DeviceNumber);

                            if (tmpIdVehicle != -1)
                            {
                                m_ReceivedSentConfirmations[tmpIdVehicle] = true;
                            }
                        }
                    }
                }
                WaitForData(socketData.CurrentSocket, socketData);

            }
            catch (ObjectDisposedException oe)
            {
                log.Debug ("ZORAN: VAZNO!!! Ova podole e ObjectDisposedException ERROR!!! Da se proveri!!!", oe);
                //log.Error("ERROR ", oe);
            }
            catch (SocketException se)
            {
                //log.Debug("ZORAN: VAZNO!!! Ova podole e SocketException ERROR!!! Da se proveri!!!");

                if (se.ErrorCode == 10054) // Error code for Connection reset by peer
                {                    
                    string msg = "Client " + " Disconnected" + "\n";
                    Console.WriteLine(msg);
                }
                else
                {
                    log.Error("ERROR ", se);
                }
            }
        }


        void SendPingMsg()
        {

            lock (_sync)
            {
                List<Socket> remove_Socets = new List<Socket>();

                try
                {
                    byte[] byData = System.Text.Encoding.ASCII.GetBytes("Z");

                    Socket workerSocket = null;

                    for (int i = 0; i < m_workerSocketList.Count; i++)
                    {
                        workerSocket = (Socket)m_workerSocketList[i];

                        if (workerSocket != null)
                        {
                            try
                            {
                                workerSocket.Send(byData);
                            }
                            catch (SocketException se)
                            {
                                log.Error("ZORAN: SendPingMsg --> ", se);

                                remove_Socets.Add(workerSocket);
                            }
                        }
                        else
                        {
                            remove_Socets.Add(workerSocket);
                        }
                    }
                }
                catch (SocketException se)
                {
                    log.Error("SendPingMsg ERROR ", se);
                }

                List<long> remove_VechSockets = new List<long>();

                foreach (Socket var in remove_Socets)
                {
                    try
                    {
                        var.Close();

                        m_workerSocketList.Remove(var);

                        if (m_VehicleSockets.ContainsValue(var))
                        {

                            foreach (KeyValuePair<long, Socket> varPair in m_VehicleSockets)
                            {
                                if (varPair.Value == var)
                                {
                                    remove_VechSockets.Add(varPair.Key);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("ZORAN: (SendPingMsg) --> ", ex);
                    }
                }

                foreach (long var in remove_VechSockets)
                {
                    m_VehicleSockets.Remove(var);

                    //ZORAN:    Ovde ja resetiram sostojbata na voziloto za da se vidi i na strana na dispecer
                    //          Id_User = -100 e "System"
                    VehiclesContainer.Instance.ForceStateUndefine(var, -100);
                    log.Debug("ZORAN: SendPingMsg, socket za vozilo ID_Vehicle: " + var.ToString() + " e INACTIVE i se trga!!!");
                }
            }
        }


        



        public void CloseSockets()
        {
            lock (_sync)
            {
                if (m_mainSocket != null)
                {
                    m_mainSocket.Close();
                }

                List<long> remove_VechSockets = new List<long>();

                List<Socket> remove_Socets = new List<Socket>();

                if (m_VehicleSockets.Count > 0)
                {
                    foreach (KeyValuePair<long, Socket> soc in m_VehicleSockets)
                    {
                        if (soc.Value != null)
                        {
                            //m_VehicleSockets.Remove(soc.Key);
                            remove_VechSockets.Add(soc.Key);
                            m_workerSocketList.Remove(soc.Value);
                        }
                    }
                }

                foreach (Socket workerSocket in m_workerSocketList)
                {
                    if (workerSocket != null)
                    {
                        workerSocket.Close();
                        remove_Socets.Add(workerSocket);

                        //m_workerSocketList.Remove(workerSocket);
                    }
                }

                foreach (long var in remove_VechSockets)
                {
                    m_VehicleSockets.Remove(var);
                }

                foreach (Socket var in remove_Socets)
                {
                    m_workerSocketList.Remove(var);
                }
            }
        }

       


        public long SendMsgToVehicle(long ID_Vechicle, byte[] msg)
        {
            lock (_sync)
            {
                if (msg == null)
                    return -1;

                if (msg.Length < 2)
                    return 0;

                if (!m_VehicleSockets.ContainsKey(ID_Vechicle))
                    return -1;


                Socket workerSocket = (Socket)m_VehicleSockets[ID_Vechicle];
              
                try
                {
                    //workerSocket.Send(msg, 0, msg.Length, SocketFlags.None);
                    int tmpInt = workerSocket.Send(msg, 0, msg.Length, SocketFlags.None);
                    ////Console.WriteLine("Potvrda od Send: " + tmpInt.ToString());
                }
                catch (ObjectDisposedException e)
                {
                    log.Error("ERROR ", e);
                    m_VehicleSockets.Remove(ID_Vechicle);
                    return -1;
                }
                catch (SocketException se)
                {
                    log.Error("ERROR ", se);
                    m_VehicleSockets.Remove(ID_Vechicle);
                    return -1;
                }
                catch (Exception e)
                {
                    log.Error("ERROR ", e);
                    m_VehicleSockets.Remove(ID_Vechicle);
                    return -1;
                }

                return 0;
            }
        }

    }

}
