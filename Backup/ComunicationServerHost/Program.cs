using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Configuration;
////using JP.Data.Utils;
using log4net;
using System.Reflection;
using Taxi.Communication.Server.ConnectionListeners;
using System.Threading;
using Taxi.Communication.Server.StateMachine;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Containers;
using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.PhoneSwitch;
using System.Diagnostics;
using Taxi.Communication.Server.Host.Hosts;
using Taxi.Communication.Server.Utils.Containers;
using Taxi.Communication.Server.HttpListener;


namespace Taxi.Communication.Server.Host
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                GPSListener m_gpsListeners;

                MobileHttpListener m_MobileHttpListener;

                SmsListener m_SmsListener;

                PhoneSwitchListener m_phoneListener;

                ReservationProcessingThread m_reservation;
                UpdateMapLocations m_UpdateMapLocation;
                KeepAliveThread m_keepAliveThread;

                Thread phoneExchThread = null;
                Thread gpsThread = null;

                Thread MobileThread = null;
                Thread SmsThread = null;
             
                Thread reservationThread = null;
                Thread keepAliveThread = null;
                Thread UpdateMapLocationThread = null;

                bool startPhoneListener = true;
                bool startReservationProcessing = true;
                
                log4net.Config.XmlConfigurator.Configure();

                log4net.LogManager.GetLogger("MyService").Info("STARTING");
                log4net.LogManager.GetLogger("TimeLog").Info("STARTING");
              
                try
                {

                    if (!bool.Parse(ConfigurationManager.AppSettings["startPhoneListener"]))
                        startPhoneListener = false;
                }
                catch (FormatException fe)
                {
                    startPhoneListener = false;
                    log4net.LogManager.GetLogger("MyService").Error("PhoneListener", fe);
                }

                try
                {
                    if (!bool.Parse(ConfigurationManager.AppSettings["startReservationProcessing"]))
                        startReservationProcessing = false;
                }
                catch (FormatException fe)
                {
                    startReservationProcessing = false;
                    log4net.LogManager.GetLogger("MyService").Error("ReservationProcessing", fe);
                }

                Uri baseAddressCore = new Uri(ConfigurationManager.AppSettings["baseAddressCore"]);
                Uri baseAdminAddressCore = new Uri(ConfigurationManager.AppSettings["baseAdminAddressCore"]);                
                Uri baseTaxiAdminAddressCore = new Uri(ConfigurationManager.AppSettings["baseTaxiAdminAddressCore"]);
                Uri baseIpPhoneAddressCore = new Uri(ConfigurationManager.AppSettings["baseIpPhoneAddressCore"]);

                GeneralConstants.ShiftInOutSensor = ConfigurationManager.AppSettings["ShiftInOutSensor"];
                GeneralConstants.ShiftInOutSensorStateIn = Int16.Parse(ConfigurationManager.AppSettings["ShiftInOutSensorStateIn"]);


                
                //Uri baseWebConnectCore = new Uri(ConfigurationManager.AppSettings["baseWebConnectCore"]);
                




                Taxi.Communication.Server.Utils.ThreadsConfigurationHandler tHand =
                    ConfigurationManager.GetSection("ThreadsConfiguration")
                    as Taxi.Communication.Server.Utils.ThreadsConfigurationHandler;
               

                MapUtils mapUtils = new Taxi.Communication.Server.Utils.MapUtils(ConfigurationManager.AppSettings["patekaZaTab"]);

                //Initialize the VehiclesContainer
                VehiclesContainer.Instance.initialize(initVechicle(), mapUtils);
               
                //Close ALL open UserInOut
                DataRepository.UserInOutProvider.CloseAllOpenLogins();

                SynchQueue<Locations> locQueue = new SynchQueue<Locations>();
                SynchQueue<long> vehicleID_Queue = new SynchQueue<long>();
                SynchQueue<PendingPhoneCalls> queuePending = new SynchQueue<PendingPhoneCalls>();


                m_gpsListeners = new GPSListener(mapUtils, locQueue, vehicleID_Queue);

                m_MobileHttpListener = new MobileHttpListener();

                m_SmsListener = new SmsListener();

                
                string switchType = System.Configuration.ConfigurationManager.AppSettings["commListenerType"];
                string port = System.Configuration.ConfigurationManager.AppSettings["commPortId"];
                int baud = Int32.Parse(System.Configuration.ConfigurationManager.AppSettings["commPortBaud"]);
                m_phoneListener = Taxi.Communication.Server.PhoneSwitch.PhoneSwitchFacotry.CreateSwitchListener(switchType, port, baud);


                m_reservation = new ReservationProcessingThread(30000, 100);              
                m_UpdateMapLocation = new UpdateMapLocations(mapUtils, locQueue);
                m_keepAliveThread = new KeepAliveThread(1000 * 30); //5min sleep time

                ServiceCallBack servCallBack = new ServiceCallBack(m_gpsListeners, m_phoneListener);

                m_reservation.setCallBack(servCallBack);
                m_keepAliveThread.setCallBack(servCallBack);
                m_MobileHttpListener.setCallBack(servCallBack);

                



                #region Generate ServiceHosts
                
                //AHost webConnectServiceHost = null;
                //if (ConfigurationManager.AppSettings["StartWebConnect"].Equals("true", StringComparison.InvariantCultureIgnoreCase))
                //{
                //    webConnectServiceHost = 
                //        new WebConnectHost(log4net.LogManager.GetLogger("MyService"), new WebConnectService(servCallBack), baseWebConnectCore);
                //}

                // Instantiate new ServiceHost
                //ServiceHost CoreServiceHost = new ServiceHost(servCallBack, baseAddressCore);
                AHost CoreServiceHost = new CallBackHost(log4net.LogManager.GetLogger("MyService"), servCallBack, baseAddressCore);

                //ServiceHost adminServiceHost = new ServiceHost(typeof(AdministrationService), baseAdminAddressCore);
                AHost adminServiceHost = new AdministrationHost(log4net.LogManager.GetLogger("MyService"), baseAdminAddressCore);
                               

                //ServiceHost taxiAdminServiceHost = new ServiceHost(typeof(TaxiAdministrationService), baseTaxiAdminAddressCore);
                AHost taxiAdminServiceHost = new TaxiAdminHost(log4net.LogManager.GetLogger("MyService"),baseTaxiAdminAddressCore);

                // ZORAN: go staviv tuka, gospod znae dali taka treba!!!

                IPphoneExchangeService phoneExcheangeService = new IPphoneExchangeService(m_phoneListener);

                AHost IPphoneExchangeServiceHost = new IPphoneExchangeHost(log4net.LogManager.GetLogger("MyService"),phoneExcheangeService, baseIpPhoneAddressCore);
                
                #endregion

                Console.WriteLine("Starting listeners");
             
                if (startReservationProcessing)
                {
                    reservationThread = new Thread(new ThreadStart(m_reservation.start));
                    reservationThread.Start();
                    Console.WriteLine("Wait for reservation thread");
                    while (!reservationThread.IsAlive) ;
                }

                if (startPhoneListener)
                {
                    phoneExchThread = new Thread(new ThreadStart(m_phoneListener.Start));
                    phoneExchThread.Start();
                    Console.WriteLine("Start Done Waiting for Phone Exchange");
                    while (!phoneExchThread.IsAlive) ;
                }

                gpsThread = new Thread(new ThreadStart(m_gpsListeners.Start));
                gpsThread.Start();
                Console.WriteLine("Wait for GPS Listener thread");
                while (!gpsThread.IsAlive) ;
                
                MobileThread = new Thread(new ThreadStart(m_MobileHttpListener.start));
                MobileThread.Start();
                Console.WriteLine("Wait for MobileThread thread");
                while (!MobileThread.IsAlive) ;

                SmsThread = new Thread(new ThreadStart(m_SmsListener.start));
                SmsThread.Start();
                Console.WriteLine("Wait for SmsThread thread");
                while (!SmsThread.IsAlive) ;


                CoreServiceHost.Open();
                adminServiceHost.Open();
                taxiAdminServiceHost.Open();
              
                keepAliveThread = new Thread(new ThreadStart(m_keepAliveThread.start));
                keepAliveThread.Start();
                Console.WriteLine("Wait for keepAliveThread thread");
                while (!keepAliveThread.IsAlive)

                    Console.WriteLine("Taxi Service is Online.");
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Press any key to terminate service.");

                //          ------------------------------------------------------------
                //ZORAN:    Ova da mi prati sms ako Donka go resetira serverot
                //          ------------------------------------------------------------

                SmSsent mSmsSent = new SmSsent();

                mSmsSent.PhoneNumber = "+38970335464";
                mSmsSent.SmStext = "Serverot se restartira!!!";

                DataRepository.SmSsentProvider.Insert(mSmsSent);

                //          ------------------------------------------------------------

                Console.ReadKey();
                Console.WriteLine("-----------------------------------");
                Console.WriteLine("Terminating....");
                if (phoneExchThread != null)
                {
                    m_phoneListener.Stop();
                    phoneExchThread.Join();
                }
                Console.WriteLine("Phone Exchange DONE.");
                if (gpsThread != null)
                {
                    m_gpsListeners.Stop();
                    gpsThread.Join();
                }
                Console.WriteLine("GPSListener DONE.");
               
                if (reservationThread != null)
                {
                    m_reservation.stop();
                    reservationThread.Join();
                }
                Console.WriteLine("Reservation DONE.");
                if (UpdateMapLocationThread != null)
                {
                    m_UpdateMapLocation.Stop();
                    UpdateMapLocationThread.Join();
                }
                Console.WriteLine("UpdateMapLocation DONE.");

                if (keepAliveThread != null)
                {
                    m_keepAliveThread.stop();
                    keepAliveThread.Interrupt();
                    keepAliveThread.Join();
                }
                Console.WriteLine("KeepAlive DONE.");      

                adminServiceHost.Close();
                Console.WriteLine("AdminHost DONE.");
                CoreServiceHost.Close();
                Console.WriteLine("Core Host DONE.");
                taxiAdminServiceHost.Close();
                Console.WriteLine("Taxi Admin DONE.");


            }
            catch (Exception ex)
            {
                string SourceName = "CommunicationServerHost";
                if (!EventLog.SourceExists(SourceName))
                {
                    EventLog.CreateEventSource(SourceName, "Application");
                }

                EventLog eventLog = new EventLog();
                eventLog.Source = SourceName;
                string message = string.Format("Exception: {0} \n\nStack: {1}", ex.Message, ex.StackTrace);
                eventLog.WriteEntry(message, EventLogEntryType.Error);
            }

        }

        static Dictionary<long, TList<Vehicle>> initVechicle()
        {
            Dictionary<long, TList<Vehicle>> retVal = new Dictionary<long, TList<Vehicle>>();
            TList<Vehicle> lstVehicles;
            TList<Company> lstCompany;

            lstCompany = DataRepository.CompanyProvider.GetAll();
            //lstCompany = new TList<Company>();

            foreach (Company company in lstCompany)
            {
                lstVehicles = DataRepository.VehicleProvider.GetByIdCompany(company.IdCompany);
               

                // Zoran
                // Tuka gi trgam site vozila koi nemaat montiran ured, inace paga...           
                // ...........................................................................................

                TList<Vehicle> tmpLstVehicles = new TList<Vehicle>();

                foreach (Vehicle tmpVehicle in lstVehicles)
                    if (tmpVehicle.IdUnit != null)
                        tmpLstVehicles.Add(tmpVehicle);

                lstVehicles = tmpLstVehicles;

                // ...........................................................................................


                Console.WriteLine("");
                Console.WriteLine("Procitav " + lstVehicles.Count.ToString() + " vozila za " + company.Description);
                Console.WriteLine("");


                Type[] tl = new Type[3];
                tl[0] = typeof(Unit);
                tl[1] = typeof(Company);
                tl[2] = typeof(CompanyTimeZone);

                DataRepository.VehicleProvider.DeepLoad(lstVehicles, true, DeepLoadType.IncludeChildren, tl);

                foreach (Vehicle vehicle in lstVehicles)
                {                   
                    //Zemam koj e najaven Driver, ako ima zapis vo ShiftInOut kade DateOut = null  
                    //Porano imavmei Vehicle.ID_Driver i Vehicle.ID_ShiftInOut
                    //SEGA ima samo Vehicle.DriverShiftInOut

                    vehicle.DriverShiftInOut = null;

                    TList<GlobSaldo.AVL.Entities.ShiftInOut> tShiftInOutDrivers = GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.GetByIDVehicleAndDateTimeNull  (vehicle.IdVehicle);

                    if(tShiftInOutDrivers != null && tShiftInOutDrivers.Count > 0)
                    {
                        vehicle.DriverShiftInOut = tShiftInOutDrivers[0];
                    }

                    vehicle.currentState = new StateUndefined(vehicle);
                    UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();
                    tmpUpdateStateInOut.UpdateVehicleState(vehicle);

                    vehicle.previousState = new StateUndefined(vehicle);
                    vehicle.currentSensorData = new SensorData();
                    vehicle.currentGPSData = new GPSData();

                    vehicle.StateChanged = true;

                }

                retVal.Add(company.IdCompany, lstVehicles);
            }


            return retVal;
        }       


        static TList<PhoneCalls> initPhoneExchangeMessage()
        {
            return new TList<PhoneCalls>();
        }
    }
}
