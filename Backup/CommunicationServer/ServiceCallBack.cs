using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.Configuration;
////using JP.Data.Utils;
using log4net;
using Taxi.Communication.Server.ConnectionListeners;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.Bases;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.Containers;
using Taxi.Communication.Server.PhoneSwitch;
using Taxi.Communication.Server.Utils.Parsers;
using Taxi.Communication.Server.Utils.Containers;

using System.Threading;


namespace Taxi.Communication.Server
{

    interface IMyContractCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnCallback(Vehicle vehicle);

        [OperationContract(IsOneWay = true)]
        void ClientKeepAlive();

        [OperationContract(IsOneWay = true)]
        void OnCallbackPhoneExchange(PhoneCalls phoneMessage);

        [OperationContract(IsOneWay = true)]
        void OnNewReservations(TList<Reservations> reservationLists);

       
    }
    [ServiceContract(CallbackContract = typeof(IMyContractCallback))]
    public interface IServiceCallBack
    {
        [OperationContract]
        void DoSomething();

        [OperationContract]
        long Connect(Users user);

        [OperationContract]
        void Disconnect(Users user);

        #region Vehicle
        [OperationContract]
        [ReferencePreservingDataContractFormat]
        TList<Vehicle> getAllVehiclesForCompany(int company);

        [OperationContract]
        [ReferencePreservingDataContractFormat]
        TList<Vehicle> getAllVehiclesForCompanies(string companies);

        [OperationContract]
        [ReferencePreservingDataContractFormat]
        List<long> getAllVehiclesIdForCompanies(string companies);

        [OperationContract]
        [ReferencePreservingDataContractFormat]
        Vehicle getVehiclesForIdVehicle(long pVehicle);


        [OperationContract]
        [ReferencePreservingDataContractFormat]
        TList<Vehicle> getAllChangedVehiclesForCompany(int company);

        [OperationContract]
        [ReferencePreservingDataContractFormat]
        TList<Vehicle> getAllVehiclesForCompanyForState(int company, int state);

        #endregion



        #region DispecherFunctions

        [OperationContract]
        long ReserveVehicle(long ID_Vehicle, long ID_User);
        [OperationContract]
        long ReleaseVehicle(long ID_Vehicle, long ID_User);
   

        [OperationContract]
        long SendAddress1(long ID_Vehicle, long ID_User, PhoneCalls phoneCall);
        [OperationContract]
        long SendPopUp(long ID_Vehicle, long ID_User, string strPopUp);

        [OperationContract]
        TList<SentPopUpMessage> GetSentPopUpMessagesForVehicleAndPeriod(long pID_Vehicle, int pMinutes);

        [OperationContract]
        long CancellOrderFromClient(long ID_Vehicle, long ID_User);
        [OperationContract]
        long ExtendWaitClientTime(long ID_Vehicle, long ID_User);
        [OperationContract]
        long ResetAlarm(long ID_Vehicle, long ID_User);
       
        [OperationContract]
        long vehicleToUndefinedState(long ID_Vehicle, long ID_User);

        [OperationContract]
        long releaseNextPhoneCall(long ID_Vehicle, long ID_User);
        
        [OperationContract]
        GisGeoLocation GetGeoLocation(float Longitude_X, float Latitude_Y);

        [OperationContract]
        long generatePhoneCall(string Ext, string PhoneNumber);

        [OperationContract]
        TList<Users> getDataByUserNamePassword(string username, string password);

        [OperationContract]
        TList<GisPhoneNumbers> getGisPhoneNumbersByIDObject(long IDObject);

        [OperationContract]
        TList<GisObjects> getAllGisObjectsByName(string name);

        [OperationContract]
        Driver getDriverForCard(string description);

        [OperationContract]
        TList<GisStreets> getGisStreetsByName(string name);

        [OperationContract]
        long UpdateQueryLogOutUser(DateTime dateOut, long ID_User);

        [OperationContract]
        GisRegions getGisRegionForIdRegion(int IdRegion);

        [OperationContract]
        RegisteredPassengers getRegisteredPassingerForCard(string description);

        [OperationContract]
        long insertUserInOut(UserInOut newUserInOut);

        [OperationContract]
        long updatePhoneCalls(PhoneCalls pPhoneCalls);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber);

        [OperationContract]
        TList<PhoneNumbersBlackList> getDataByPhoneNumber(string phoneNumber);

        [OperationContract]
        Clients searchVipClients(ClientPhoneNumbers clientPhoneNumber);

        [OperationContract]
        Driver getDataLastDriverShift(long ID_Vehicle);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreet(string ID_Street);

        [OperationContract]
        bool IsVehicleEligibleForCall(Vehicle pVehicle, PhoneCalls phoneCall);





        //RfIdCardPerClients
        #region RfIdCardPerClients

        [OperationContract]
        long saveRfIdCardPerClients(RfIdCardPerClients itemObj);

        [OperationContract]
        long deleteRfIdCardPerClients(RfIdCardPerClients itemObj);

        [OperationContract]
        long updateRfIdCardPerClients(RfIdCardPerClients itemObj);

        [OperationContract]
        TList<RfIdCardPerClients> getAllRfIdCardPerClients();

        [OperationContract]
        RfIdCardPerClients getRfIdCardPerClients(long Id_RfIdCardPerClients);

        [OperationContract]
        TList<RfIdCardPerClients> getAllRfIdCardPerClientssForIdRfIdCard(long IdRfIdCard, bool permanentlyDiscarded,
                                                                         bool takenBack);

        #endregion

        #endregion

        #region Reservation
        [OperationContract]
        long startProcessReservation(Reservations reservation, long ID_User);

        [OperationContract]
        long cancelReservation(Reservations reservation, long ID_User);

        [OperationContract]
        long makeReservation(long ID_User, DateTime dateFrom, DateTime dateTo, TimeSpan timeEarlyAlarm, List<DayOfWeek> daysOfTheWeek, Reservations res);

        [OperationContract]
        long increaseNumberOfRetries(Reservations reservation, long ID_User);

        #endregion


#region MobileReservation

        [OperationContract]
        TList<MobileReservations> getActiveMobileReservation(long ID_User);

        [OperationContract]
        long ConfirmReservation(MobileReservations pMobileReservations, long ID_User);
        

#endregion


        #region ShortMessage
        [OperationContract]
        long confirmShortMessage(ReceivedShortMessage shortMessage, long ID_User);

        [OperationContract]
        long cancelShortMessage(ReceivedShortMessage shortMessage, long ID_User);

        [OperationContract]
        TList<ReceivedShortMessage> GetReceivedShortMessageForVehicleAndPeriod(long pID_Vehicle, int pMinutes);
        #endregion


        #region ReceivedFreeText
        [OperationContract]
        long confirmReceivedFreeText(ReceivedFreeText pReceivedFreeText, long ID_User);

        [OperationContract]
        long cancelReceivedFreeText(ReceivedFreeText pReceivedFreeText, long ID_User);

        [OperationContract]
        TList<ReceivedFreeText> GetReceivedFreeTextForVehicleAndPeriod(long pID_Vehicle, int pMinutes);

        #endregion

        #region VehiclesSelection

        [OperationContract]
        TList<Vehicle> SelectVehiclesforXYforCompanies(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, List<long> pIdCompanies);       

        [OperationContract]
        long SelectVehiclesforXYforFirstAndAlternativeCompaniesAndPhoneCall(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pIdFirstCompany, List<long> pIdAlternativeCompanies, PhoneCalls pPhoneCall, long pUser);


        //ZORAN:    Ova e metoda za selekcija na kooperanti
        //          PRAVILO:
        //                  - Ako e povikot na Nase Taksi, a nema koli, prvo se nudi na LOTUS, pa potoa kooperanti
        //                  - Ako e povikot na LOTUS, a nema koli, se odi direktno na kooperanti
        [OperationContract]
        TList<Vehicle> SelectSubContractorsForXYandPhoneCall(double pLongitudeX, double pLatitudeY, PhoneCalls pPhoneCall, long pUser);


          //ZORAN:    Ova e metoda za potvrda koj kooperant bil praten za konkreten povik
        //          Prethodno se evidentirani koi kooperanti bile selektirani za konkretniot povik
        //          Dopolnitelno, tuka mu se brisat i pari na kooperantot!!!
        [OperationContract]
        long ConfirmSubContractorForPhoneCall(long pIdVehicle, PhoneCalls pIdPhoneCall, long pUser);

        //ZORAN:    Ovae nova metoda. 
        //          Od dispecher se povikuva ili prethodnata ili ovaa, vo zavisnost dali klientot saka da ceka odgovorili da mu pratat na SMS         
        [OperationContract]
        long InsertPendingPhoneCalls(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pIdFirstCompany, List<long> pIdAlternativeCompanies, PhoneCalls pPhoneCall, string pContactPhoneNumber, long pUser);


        #endregion

    }



    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceCallBack : IServiceCallBack, ICallbacksForPhoneSwitch, ICallbacksForGPSmessageRecived, ICallbacksReservationProcessing, ICallbacksKeepAlive
    {
        public static readonly ILog log = log4net.LogManager.GetLogger("MyService");

        // ZORAN:   Ova go napraviv vo dogovor so Pajo, posle vklucuvanjeto na Lotus i Sonce
        //private static Dictionary<long, List<IMyContractCallback>> m_Callbacks = new Dictionary<long, List<IMyContractCallback>>();

        private static List<IMyContractCallback> m_Callbacks = new List<IMyContractCallback>();

        private static bool disconnectFlag = false;

        private static bool connectFlag = false;

        private GPSListener _gpsListener = null;
        private PhoneSwitchListener _phoneListener = null;

        private static object locker = new object();

        private static bool _debugMode = false;

        public event PhoneCallRecived OnPhoneCallRecived;

        public event SendOrderToVehicle OnOrderReceived;

        private ReservationProcessing reservationHeandler;

        public ServiceCallBack(GPSListener gpsListener, PhoneSwitchListener phoneListener)
        {
            this._gpsListener = gpsListener;
            this._gpsListener.setCallBack(this);
            this._phoneListener = phoneListener;
            this._phoneListener.setCallBack(this);


            

            try
            {

                if (!bool.Parse(ConfigurationManager.AppSettings["DebugMode"]))
                    _debugMode = false;
                else
                    _debugMode = true;
            }
            catch (FormatException ex)
            {
                _debugMode = false;
                LogManager.GetLogger("MyService").Error("_debugMode", ex);
            }

            OnPhoneCallRecived += new PhoneCallRecived(ServiceCallBack_OnPhoneCallRecived);

            //OnOrderReceived += new SendOrderToVehicle(

            reservationHeandler = CallBackReservationProcessing;
                        
        }

        public void ServiceCallBack_OnPhoneCallRecived(object sender, PhoneCalls e)
        {
            CallClientsForPhoneExchangeMessage(e);
        }
      

        public void CallForPhoneCallRecived(object sender, PhoneCalls e)
        {
            CallClientsForPhoneExchangeMessage(e);
        }

        public void CallBackGPSMessageRecived(long ID_Company, Vehicle vehicle)
        {
            CallClientsForVehicle(ID_Company, vehicle);
        }        


        public void CallBackReservationProcessing(long ID_Company, TList<Reservations> reservations)
        {
            CallClientForReservations(ID_Company, reservations);
        }


        public void CallBackKeepAlive()
        {
            CallClientKeepAlive();
        }

       

        //ZORAN:    Ova e stara funkcija, ne znam tocno kako se upotrebuva..
        public void CallBackForOrderRecived(long ID_Vehicle, long ID_User, PhoneCalls pPhoneCall)
        {
            SendAddress1(ID_Vehicle, ID_User, pPhoneCall);
        }

        #region RegisterAndCallBack


        public long Connect(Users user)
        {
            connectFlag = true;

            LogManager.GetLogger("TimeLog").Info("Se logira User: " + user.Name + " " + user.Lastname);
            LogManager.GetLogger("TimeLog").Info("Broj na CallBacks: " + m_Callbacks.Count.ToString());
            

            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            ////LogManager.GetLogger("TimeLog").Info("STARTING - Connect " + tmpCalculateSaveTimeStart);
            long retVal = -1;
            try
            {

                TList<Users> lstUsers = null;

                lstUsers = DataRepository.UsersProvider.CheckUser(user.Username, user.Password);

                if (lstUsers.Count > 1 || lstUsers.Count == 0)
                {
                    connectFlag = false;
                    retVal = -1;
                }
                else
                {

                    IMyContractCallback callback = OperationContext.Current.
                                                       GetCallbackChannel<IMyContractCallback>();


                    lock (locker)
                    {    
                        //if (!m_Callbacks.ContainsKey(lstUsers[0].IdCompany))
                        //{
                        //    m_Callbacks.Add(lstUsers[0].IdCompany, new List<IMyContractCallback>());
                        //}

                        //if (m_Callbacks[lstUsers[0].IdCompany].Contains(callback) == false)
                        if (m_Callbacks.Contains(callback) == false)

                        {
                            //m_Callbacks[lstUsers[0].IdCompany].Add(callback);
                            m_Callbacks.Add(callback);
                            Console.WriteLine("Íà¼àâåí êîðèñíèê " + lstUsers[0].Lastname + " " + lstUsers[0].Name);

                            //////AdministrationService.saveLogInfo(SourceApplicationList.AVL_Server, "Íà¼àâåí êîðèñíèê " + lstUsers[0].Lastname + " " + lstUsers[0].Name);
                        }
                    }

                   
                    retVal = 0;
                }

            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
                retVal = -2;
            }

            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - Connect ");

            connectFlag = false;
            return retVal;
        }

        public void Disconnect(Users user)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - Disconnect " + tmpCalculateSaveTimeStart);

            disconnectFlag = true;
            try
            {               

                IMyContractCallback callback = OperationContext.Current.GetCallbackChannel<IMyContractCallback>();
                if (disconnectFlag)
                {
                   
                    lock (locker)
                    {
                        //if (!m_Callbacks.ContainsKey(lstUsers[0].IdCompany))
                        //{
                        //    disconnectFlag = false;
                        //}
                        //else if (m_Callbacks[lstUsers[0].IdCompany].Contains(callback) == true)

                        if (m_Callbacks.Contains(callback) == true)
                        {
                            m_Callbacks.Remove(callback);
                            disconnectFlag = false;
                        }
                        else
                        {
                            disconnectFlag = false;
                            //throw new InvalidOperationException("Cannot find callback");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - Disconnect ");

            return;
        }

        public void CallClientsForPhoneExchangeMessage(PhoneCalls phoneMessage)
        {
            if (disconnectFlag || connectFlag)
                return;

            //if (!m_Callbacks.ContainsKey(ID_Company))
            //    return;


            //m_Callbacks.ForEach(invoke);

            List<IMyContractCallback> remove_CallBacks = new List<IMyContractCallback>();

            try
            {
                lock (locker)
                {
                    //foreach (KeyValuePair<long, List<IMyContractCallback>> varPair in m_Callbacks)
                    //{
                    //    // Tuka da se implemetira do koja kompanija ke se prakja PhoneCallBack
                    //    //if (varPair.Key == 1){
                    //    foreach (IMyContractCallback var in varPair.Value)
                    foreach (IMyContractCallback var in m_Callbacks)
                        {
                            try
                            {
                                var.OnCallbackPhoneExchange(phoneMessage);
                            }
                            catch (ObjectDisposedException e)
                            {
                                log.Error("ERROR ", e);
                                //m_Callbacks.Remove(var);
                                remove_CallBacks.Add(var);
                            }
                            catch (Exception e)
                            {
                                log.Error("ERROR ", e);
                                //m_Callbacks.Remove(var);
                                remove_CallBacks.Add(var);
                            }
                        }

                        //foreach (IMyContractCallback var in remove_CallBacks)
                        //{
                        //    if (varPair.Value.IndexOf(var) > 0)
                        //        varPair.Value.Remove(var);
                        //}

                        foreach (IMyContractCallback var in remove_CallBacks)
                        {
                            if (m_Callbacks.IndexOf(var) > 0)
                                m_Callbacks.Remove (var);
                        }


                        remove_CallBacks.Clear();
                    }
            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
            }

            //////DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            //////TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            //////if (_debugMode)
            //////    LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - CallClientsForPhoneExchangeMessage ");
        }

        public void CallClientsForVehicle(long ID_Company, Vehicle vehicle)
        {

            if (disconnectFlag || connectFlag)
                return;

            List<IMyContractCallback> remove_CallBacks = new List<IMyContractCallback>();

            try
            {
                lock (locker)
                {                    
                    foreach (IMyContractCallback var in m_Callbacks)
                    {
                        try
                        {
                            var.OnCallback(vehicle);                          
                        }
                        catch (ObjectDisposedException e)
                        {
                            log.Error("CallClientsForVehicle -ERROR --> ", e);

                            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(vehicle.GetType());
                            //System.IO.StringWriter textWriter = new System.IO.StringWriter();

                            //x.Serialize(textWriter, vehicle);
                            //log.Error(textWriter.ToString()); 

                            remove_CallBacks.Add(var);
                        }
                        catch (System.ServiceModel.CommunicationObjectAbortedException e)
                        {
                            log.Error("WARNING: Removed object",e);

                            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(vehicle.GetType());
                            //System.IO.StringWriter textWriter = new System.IO.StringWriter();

                            //x.Serialize(textWriter, vehicle);
                            //log.Error(textWriter.ToString()); 

                            remove_CallBacks.Add(var);
                        }
                        catch (Exception e)
                        {
                            log.Error("ERROR ", e);

                            //System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(vehicle.GetType());
                            //System.IO.StringWriter textWriter = new System.IO.StringWriter();

                            //x.Serialize(textWriter, vehicle);
                            //log.Error(textWriter.ToString()); 

                            remove_CallBacks.Add(var);
                        }
                    }

                    foreach (IMyContractCallback var in remove_CallBacks)
                    {                        
                        m_Callbacks.Remove(var);
                    }
                }

            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
            }                       
        }

        public void CallClientForReservations(long ID_Company, TList<Reservations> reservations)
        {

            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - CallClientForReservations " + tmpCalculateSaveTimeStart);

            if (disconnectFlag || connectFlag)
                return;

            //if (!m_Callbacks.ContainsKey(ID_Company))
            //    return;


            //m_Callbacks.ForEach(invoke);

            List<IMyContractCallback> remove_CallBacks = new List<IMyContractCallback>();

            if (reservations == null)
                return;

            if (reservations.Count == 0)
                return;

            try
            {
                lock (locker)
                {
                    //foreach (IMyContractCallback var in m_Callbacks[ID_Company])
                    foreach (IMyContractCallback var in m_Callbacks)
                    {
                        try
                        {
                            var.OnNewReservations(reservations);
                            //log.Debug("CALL BACK");
                        }
                        catch (ObjectDisposedException e)
                        {
                            log.Error("ERROR ", e);
                            //m_Callbacks.Remove(var);
                            remove_CallBacks.Add(var);
                        }
                        catch (System.ServiceModel.CommunicationObjectAbortedException e)
                        {
                            log.Error("WARNING: Removed object",e);
                            remove_CallBacks.Add(var);
                        }
                        catch (Exception e)
                        {
                            log.Error("ERROR ", e);
                            //m_Callbacks.Remove(var);
                            remove_CallBacks.Add(var);
                        }
                    }

                    foreach (IMyContractCallback var in remove_CallBacks)
                    {
                        m_Callbacks.Remove(var);
                    }
                }

            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - CallClientForReservations ");
        }

        public static void CallClientKeepAlive()
        {

            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - CallClientForReservations " + tmpCalculateSaveTimeStart);

            if (disconnectFlag || connectFlag)
                return;


            //m_Callbacks.ForEach(invoke);

            List<IMyContractCallback> remove_CallBacks = new List<IMyContractCallback>();


            try
            {
                //foreach (long key in m_Callbacks.Keys)
                //{
                    //foreach (IMyContractCallback var in m_Callbacks[key])
                    foreach (IMyContractCallback var in m_Callbacks)
                    {
                        try
                        {
                            var.ClientKeepAlive();
                        }
                        catch (ObjectDisposedException e)
                        {
                            log.Error("ERROR ", e);
                        }
                        catch (Exception e)
                        {
                            log.Error("ERROR ", e);
                        }
                    }
                //}

            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - CallClientForReservations ");
        }
        

        public void DoSomething()
        {
        }

        #endregion


        #region Vehicle

        public TList<Vehicle> getAllVehiclesForCompanyForState(int company, int state)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getAllVehiclesForCompanyForState " + tmpCalculateSaveTimeStart);

            TList<Vehicle> retVal = new TList<Vehicle>(); //Samo tie koi se vo konkreten state

            try
            {
                TList<Vehicle> retValAllVehicles = VehiclesContainer.Instance.getAllForCompany(company); //Site vozila za taa komapnija

                foreach (Vehicle tVeh in retValAllVehicles)
                {
                    if (tVeh.currentState.IDCurrentState() == state) retVal.Add(tVeh);
                }
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getAllVehiclesForCompanyForState ");

            return retVal;
        }


        public TList<Vehicle> getAllChangedVehiclesForCompany(int company)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getAllChangedVehiclesForCompany " + tmpCalculateSaveTimeStart);
            TList<Vehicle> retVal = null;
            try
            {
                retVal = VehiclesContainer.Instance.getAllForCompany(company);
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getAllChangedVehiclesForCompany ");

            return retVal;
        }


        public TList<Vehicle> getAllVehiclesForCompany(int company)
        {            
            TList<Vehicle> retVal = null;

            try
            {
                retVal = VehiclesContainer.Instance.getAllForCompany(company);               
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }          

            return retVal;
        }


        public TList<Vehicle> getAllVehiclesForCompanies(string companies)
        {

        // ZORAN: gi dobivam ID_Company, kako string, kade sekoj ID_Company e oddelen so zapirka
        // -------------------------------------------------------------------------------------

            TList<Vehicle> retVal = null;

            try
            {

                char[] delimiterChars = {','};

                string[] stringIdCompany = companies.Split(delimiterChars);
                
                TList<Vehicle> retValPerCompany = null;
               

                foreach (string tmpString in stringIdCompany)
                {
                    retValPerCompany = VehiclesContainer.Instance.getAllForCompany(int.Parse(tmpString));                   

                    if (retValPerCompany != null && retValPerCompany.Count > 0)
                    {
                        if (retVal == null)
                            retVal = new TList<Vehicle>();

                        foreach (Vehicle tmpVehicle in retValPerCompany)
                            retVal.Add(tmpVehicle);
                    }
                }
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }            

            return retVal;
        }


        public List<long> getAllVehiclesIdForCompanies(string companies)
        {

            // ZORAN: gi dobivam ID_Company, kako string, kade sekoj ID_Company e oddelen so zapirka
            // -------------------------------------------------------------------------------------

            List<long> retVal = null;

            try
            {
                char[] delimiterChars = { ',' };

                string[] stringIdCompany = companies.Split(delimiterChars);

                TList<Vehicle> retValPerCompany = null;

                foreach (string tmpString in stringIdCompany)
                {
                    retValPerCompany = VehiclesContainer.Instance.getAllForCompany(int.Parse(tmpString));                    

                    if (retValPerCompany != null && retValPerCompany.Count > 0)
                    {
                        if (retVal == null)
                            retVal = new List<long>();

                        foreach (Vehicle tmpVehicle in retValPerCompany)
                            retVal.Add(tmpVehicle.IdVehicle);
                    }
                }
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }

            return retVal;
        }


        public Vehicle getVehiclesForIdVehicle(long pVehicle)
        {
            Vehicle retVal = null;

            try
            {
                retVal = VehiclesContainer.Instance.getSingleVehicleZOKI(pVehicle);
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }
           
            return retVal;
        }


        private Vehicle getSingleVehicle(long ID_Vehicle, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getSingleVehicle " + tmpCalculateSaveTimeStart);

            Vehicle retVal = null;

            try
            {
                Users usr = DataRepository.UsersProvider.GetByIdUser(ID_User);

                retVal = VehiclesContainer.Instance.getSingleVehicle(ID_Vehicle, usr.IdCompany);
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getSingleVehicle ");

            return retVal;
        }

        #endregion





        #region Reservation


            public long startProcessReservation(Reservations reservation, long ID_User)
            {
                long retVal = -1;

                //DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
                //LogManager.GetLogger("TimeLog").Info("STARTING - startProcessReservation " + tmpCalculateSaveTimeStart);

                Reservations tCurrRes = DataRepository.ReservationsProvider.GetByIdReservation(reservation.IdReservation);

                if (tCurrRes.IdMessageStatus != (long)MessageStatusList.Inserted)
                {
                    return tCurrRes.IdMessageStatus;
                }

                try
                {
                    tCurrRes.IdReservation = reservation.IdReservation;
                    tCurrRes.IdUser = ID_User;
                    tCurrRes.DateTimeUserTake = DateTime.Now;
                    tCurrRes.IdMessageStatus = (long)MessageStatusList.Confirmed;    // ZORAN:   Potvrdena e 

                    if (DataRepository.ReservationsProvider.Update(tCurrRes))
                        retVal = 0;
                    else
                        retVal = -1;
                }
                catch (Exception e)
                {
                    log.Error("Error", e);
                    retVal = -2;
                }


                if (_debugMode)
                {
                    //DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
                    //TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

                    //LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - startProcessReservation ");
                }
                return retVal;
            }


            public long cancelReservation(Reservations reservation, long ID_User)
            {

                DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
                //LogManager.GetLogger("TimeLog").Info("STARTING - cancelReservation " + tmpCalculateSaveTimeStart);

                long retVal = -1;

                try
                {
                    reservation.IdUser = ID_User;
                    reservation.DateTimeUserTake = DateTime.Now;
                    reservation.IdMessageStatus = (long)MessageStatusList.Dropped;

                    if (DataRepository.ReservationsProvider.Update(reservation))
                        retVal = 0;
                    else
                        retVal = -1;
                }
                catch (Exception e)
                {
                    log.Error("Error", e);
                    retVal = -2;
                }
                DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
                TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

                if (_debugMode)
                    LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - cancelReservation ");

                return retVal;
            }


            public long makeReservation(long ID_User, DateTime dateFrom, DateTime dateTo, TimeSpan timeEarlyAlarm, List<DayOfWeek> daysOfTheWeek, Reservations res)
            {

                DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
                //LogManager.GetLogger("TimeLog").Info("STARTING - makeReservation " + tmpCalculateSaveTimeStart);
                long retVal = -1;
                try
                {
                    DateTime currDate = dateFrom;
                    Reservations newRes = null;
                    log.Debug(currDate.ToLongDateString());
                    while (currDate <= dateTo)
                    {
                        log.Debug(currDate.ToLongDateString());
                        if (daysOfTheWeek.Contains(currDate.DayOfWeek))
                        {
                            newRes = res.Copy();
                            newRes.IdMessageStatus = (long)MessageStatusList.Inserted;
                            newRes.DateReservation = currDate;
                            newRes.DateTimeLastAlarm = currDate.Add(-timeEarlyAlarm);
                            newRes.TimeReservation = DateTime.Now;
                            log.Debug("Insert Reservation");
                            if (!DataRepository.ReservationsProvider.Insert(newRes))
                            {
                                retVal = -2;
                            }

                        }
                        currDate = currDate.AddHours(24);
                    }
                    retVal = 0;
                }
                catch (Exception e)
                {
                    log.Error("Error", e);
                    retVal = -1;
                }
                DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
                TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

                if (_debugMode)
                    LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - makeReservation ");

                return retVal;
            }


            public long increaseNumberOfRetries(Reservations reservation, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - cancelReservation " + tmpCalculateSaveTimeStart);

            long retVal = -1;

            Reservations tCurrRes = DataRepository.ReservationsProvider.GetByIdReservation(reservation.IdReservation);

            try
            {
                tCurrRes.IdReservation = reservation.IdReservation;


                tCurrRes.NumberOfRetries++;

                if (DataRepository.ReservationsProvider.Update(tCurrRes))
                    retVal = 0;
                else
                    retVal = -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }

            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - cancelReservation ");

            return retVal;
        }


        #endregion




#region MobileReservation


        public TList<MobileReservations> getActiveMobileReservation(long ID_User)
        {
            TList<MobileReservations> retVal = null;


            return retVal;

        }


        public long ConfirmReservation(MobileReservations pMobileReservations, long ID_User)
        {
            long retVal = -1;


            return retVal;
        }

#endregion


        #region ShortMessage
        public long confirmShortMessage(ReceivedShortMessage shortMessage, long ID_User)
        {

            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - confirmShortMessage " + tmpCalculateSaveTimeStart);
            long retVal = -1;
            try
            {
                shortMessage.IdUser = ID_User;
                shortMessage.ConfirmedByUser = DateTime.Now;
                if (!DataRepository.ReceivedShortMessageProvider.Update(shortMessage))
                    retVal = -1;
                VehiclesContainer.Instance.setLastMessage(shortMessage.IdVehicle, shortMessage);

                // ********
                // Tuka,    po baranje na NaseTaksi, se prepraka i poraka deka ke se razgleda porakata
                //          Nisto ne se zapisuva vo baza, zatoa i ne se koristi originalnata metoda SendPopUp()

                clsMessageCreator tMessageCreator = new clsMessageCreator();

                byte[] retValByte;

                Vehicle tmpVeh = VehiclesContainer.Instance.getSingleVehicle(shortMessage.IdVehicle);

                retValByte = tMessageCreator.CreateNewPopUpMessageForLCD(tmpVeh, "Dispecer: OK", '0');

                // Samo ako ne e pratena porakata zapisuvam vo log, da ima traga..
                if (_gpsListener.SendMsgToVehicle(shortMessage.IdVehicle, retValByte) != -1)
                    //log.Error("Neuspesno prakanje na poraka do Vozilo ID: " + shortMessage.IdVehicle.ToString() + "confirmShortMessage");

                // ********


                retVal = 0;

            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -1;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - confirmShortMessage ");

            return retVal;

        }

        public long cancelShortMessage(ReceivedShortMessage shortMessage, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - cancelShortMessage " + tmpCalculateSaveTimeStart);
            long retVal = -1;
            try
            {
                shortMessage.IdUser = ID_User;
                shortMessage.CanceledByUser = DateTime.Now;
                if (!DataRepository.ReceivedShortMessageProvider.Update(shortMessage))
                    retVal = -1;
                VehiclesContainer.Instance.setLastMessage(shortMessage.IdVehicle, shortMessage);
                retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -1;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - cancelShortMessage ");

            return retVal;
        }



        public TList<ReceivedShortMessage> GetReceivedShortMessageForVehicleAndPeriod(long pID_Vehicle, int pMinutes)
        {
            TList<ReceivedShortMessage> retVal = null;
            DateTime mPeriod = System.DateTime.Now.AddMinutes(-1 * pMinutes);

            ReceivedShortMessageParameterBuilder SearchReceivedShortMessage = new ReceivedShortMessageParameterBuilder();


            SearchReceivedShortMessage.AppendEquals(ReceivedShortMessageColumn.IdVehicle, pID_Vehicle.ToString());
            SearchReceivedShortMessage.AppendGreaterThan(ReceivedShortMessageColumn.DateTimeReceived, mPeriod.ToString());

            try
            {
                retVal = DataRepository.ReceivedShortMessageProvider.Find(SearchReceivedShortMessage.GetParameters());
            }
            catch (Exception ex)
            {
                log.Error("Greska vo GetReceivedFreeTextForVehicleAndPeriod", ex);
                retVal = null;
            }

            return retVal;

        }

        #endregion



        #region ReceivedFreeText


        public long confirmReceivedFreeText(ReceivedFreeText pReceivedFreeText, long ID_User)
        {

            long retVal = -1;

            try
            {
                pReceivedFreeText.IdUser = ID_User;
                pReceivedFreeText.ConfirmedByUser = DateTime.Now;


                if (!DataRepository.ReceivedFreeTextProvider.Update(pReceivedFreeText))
                    retVal = -1;

                VehiclesContainer.Instance.UpdateReceivedFreeText(pReceivedFreeText.IdVehicle, pReceivedFreeText);

                // ********
                // Tuka,    po baranje na NaseTaksi, se prepraka i poraka deka ke se razgleda porakata
                //          Nisto ne se zapisuva vo baza, zatoa i ne se koristi originalnata metoda SendPopUp()

                clsMessageCreator tMessageCreator = new clsMessageCreator();

                byte[] retValByte;

                Vehicle tmpVeh = VehiclesContainer.Instance.getSingleVehicle(pReceivedFreeText.IdVehicle);

                retValByte = tMessageCreator.CreateNewPopUpMessageForLCD(tmpVeh, "Dispecer: OK", '0');

                // Samo ako ne e pratena porakata zapisuvam vo log, da ima traga..
                if (_gpsListener.SendMsgToVehicle(pReceivedFreeText.IdVehicle, retValByte) != -1)
                    //log.Error("Neuspesno prakanje na poraka do Vozilo ID: " + pReceivedFreeText.IdVehicle.ToString() + "confirmShortMessage");

                // ********


                retVal = 0;

            }
            catch (Exception e)
            {
                log.Error("confirmReceivedFreeText ERROR", e);
                retVal = -1;
            }


            return retVal;
        }

        public long cancelReceivedFreeText(ReceivedFreeText pReceivedFreeText, long ID_User)
        {

            long retVal = -1;
            try
            {
                pReceivedFreeText.IdUser = ID_User;
                pReceivedFreeText.CanceledByUser = DateTime.Now;

                if (!DataRepository.ReceivedFreeTextProvider.Update(pReceivedFreeText))
                    retVal = -1;

                VehiclesContainer.Instance.UpdateReceivedFreeText(pReceivedFreeText.IdVehicle, pReceivedFreeText);

                retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("cancelReceivedFreeText ERROR:", e);
                retVal = -1;
            }

            return retVal;
        }

        public TList<ReceivedFreeText> GetReceivedFreeTextForVehicleAndPeriod(long pID_Vehicle, int pMinutes)
        {
            TList<ReceivedFreeText> retVal = null;
            DateTime mPeriod = System.DateTime.Now.AddMinutes(-1 * pMinutes);

            ReceivedFreeTextParameterBuilder SearchReceivedFreeText = new ReceivedFreeTextParameterBuilder();


            SearchReceivedFreeText.AppendEquals(ReceivedFreeTextColumn.IdVehicle, pID_Vehicle.ToString());
            SearchReceivedFreeText.AppendGreaterThan(ReceivedFreeTextColumn.DateTimeReceived, mPeriod.ToString());

            try
            {
                retVal = DataRepository.ReceivedFreeTextProvider.Find(SearchReceivedFreeText.GetParameters());
            }
            catch (Exception ex)
            {
                log.Error("Greska vo GetReceivedFreeTextForVehicleAndPeriod", ex);
                retVal = null;
            }

            return retVal;

        }


        #endregion



        #region DispecherFunctions


        public long ReserveVehicle(long ID_Vehicle, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - ReserveVehicle " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                //log.Debug(">>>>>>>>>>>>>> RESERVE Vehicle PRED metoda, za ID_User = " + ID_User.ToString());

                retVal = VehiclesContainer.Instance.reserveVehicle(ID_Vehicle);

                //log.Debug(">>>>>>>>>>>>>> RESERVE Vehicle POSLE metoda, so vrateno retVal = " + retVal.ToString());

                //retVal = 0;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - ReserveVehicle ");

            return retVal;
        }


        public long ReleaseVehicle(long ID_Vehicle, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - ReleaseVehicle " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                retVal = VehiclesContainer.Instance.releaseVehicle(ID_Vehicle);
                //retVal = 0;

            }
            catch (Exception ex)
            {
                log.Info("This exeption happen in ReleaseVehicle funtion because of ID_Vehicle = " + ID_Vehicle + " send by ID_User = " + ID_User);
                log.Error("Error", ex);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - ReleaseVehicle ");

            return retVal;
        }                                  
                      

        public long SendAddress1(long ID_Vehicle, long ID_User, PhoneCalls phoneCall)
        {
            //DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - SendAddress1 " + tmpCalculateSaveTimeStart);
            long retVal = 0;

            try
            {
                byte[] retValByte;

                //log.Debug("--------------- PRED prakanje adresa (vo try), ID_Vehicle=" + ID_Vehicle.ToString());

                retValByte = VehiclesContainer.Instance.sendAddress(ID_Vehicle, ID_User, phoneCall);
                if (retValByte == null)
                {
                    retVal = -1;
                }

                if (retValByte != null)
                {
                    if (_gpsListener.SendMsgToVehicle(ID_Vehicle, retValByte) == -1)
                    {
                        retVal = -1;
                    }
                }
                //log.Debug("--------------- POSLE prakanje adresa (vo try), ID_Vehicle=" + ID_Vehicle.ToString());
                
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                retVal = -2;
            }
            //DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            //TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            //if (_debugMode)
            //    LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - SendAddress1 ");

            return retVal;
        }


        public long SendPopUp(long ID_Vehicle, long ID_User, string strPopUp)
        {
            //DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - SendPopUp " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                clsMessageCreator tMessageCreator = new clsMessageCreator();

                byte[] retValByte;               

                Vehicle tmpVeh = VehiclesContainer.Instance.getSingleVehicle(ID_Vehicle);

                retValByte = tMessageCreator.CreateNewPopUpMessageForLCD(tmpVeh, strPopUp, '0');

                if (_gpsListener.SendMsgToVehicle(ID_Vehicle, retValByte) == -1)
                {
                    retVal = -1;
                }
                else
                    retVal = 0;

                SentPopUpMessage popUp = new SentPopUpMessage();
                popUp.IdVehicle = ID_Vehicle;
                popUp.MessageText = strPopUp;
                popUp.MessageDateTime = DateTime.Now;
                popUp.DateTimeSubmettedByUser = DateTime.Now;
                popUp.DateTimeSentToGps = DateTime.Now;
                popUp.IdUser = ID_User;

                DataRepository.SentPopUpMessageProvider.Insert(popUp);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            //DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            //TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            //if (_debugMode)
            //    LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - SendPopUp ");

            return retVal;
        }


 

        public long SendNewPopUp(long ID_Vehicle, long ID_User)
        {           
            long retVal = -1;

            try
            {
                clsMessageCreator tMessageCreator = new clsMessageCreator();

                byte[] retValByte;

                retValByte = tMessageCreator.CreateNewPopUpMessageForLCD(VehiclesContainer.Instance.getSingleVehicle(ID_Vehicle), "TEST so 15 char", '4');

                if (_gpsListener.SendMsgToVehicle(ID_Vehicle, retValByte) == -1)
                {
                    retVal = -1;
                }
                else
                    retVal = 0;               
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }          
            return retVal;
        }


        //ZORAN:    Samo za testiranje so Marjan, treba da se 
        public long SendRingMessageByIdVehicle(long pID_Vehicle, short pNoSeconds)
        {
            long retVal = -1;

            try
            {
                clsMessageCreator tMessageCreator = new clsMessageCreator();

                byte[] retValByte;

                retValByte = tMessageCreator.CreateBeep(VehiclesContainer.Instance.getSingleVehicle(pID_Vehicle), pNoSeconds);

                if (_gpsListener.SendMsgToVehicle(pID_Vehicle, retValByte) == -1)
                {
                    retVal = -1;
                }
                else
                    retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }          

            return retVal;
        }

        public TList<SentPopUpMessage> GetSentPopUpMessagesForVehicleAndPeriod(long pID_Vehicle, int pMinutes)
        {
            TList<SentPopUpMessage> retVal = null;
            DateTime mPeriod = System.DateTime.Now.AddMinutes(-1 * pMinutes);

            SentPopUpMessageParameterBuilder SearchSentPopUpMessage = new SentPopUpMessageParameterBuilder();


            SearchSentPopUpMessage.AppendEquals(SentPopUpMessageColumn.IdVehicle, pID_Vehicle.ToString());
            SearchSentPopUpMessage.AppendGreaterThan(SentPopUpMessageColumn.MessageDateTime, mPeriod.ToString());

            try
            {
                retVal = DataRepository.SentPopUpMessageProvider.Find(SearchSentPopUpMessage.GetParameters());
            }
            catch (Exception ex)
            {
                log.Error("Greska vo GetSentPopUpMessagesForVehicleAndPeriod", ex);
                retVal = null;
            }

            return retVal;

        }


        public long CancellOrderFromClient(long ID_Vehicle, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - CancellOrderFromClient " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                retVal = VehiclesContainer.Instance.cancelRequestFromClient(ID_Vehicle);
                retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - CancellOrderFromClient ");

            return retVal;
        }


        public long ExtendWaitClientTime(long ID_Vehicle, long ID_User)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - ExtendWaitClientTime " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                retVal = VehiclesContainer.Instance.extendClientWatiTime(ID_Vehicle);
                retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - ExtendWaitClientTime ");

            return retVal;
        }


        public long ResetAlarm(long ID_Vehicle, long ID_User)
        {
            long retVal = -1;
            try
            {
                log.Error("Error call empty function for ResetAlaram ID_Vehicle = " + ID_Vehicle + " ; ID_User = " + ID_User);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            return retVal;
        }


        public GisGeoLocation GetGeoLocation(float Longitude_X, float Latitude_Y)
        {
            GisGeoLocation retVal = null;
            try
            {
                retVal = null;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = null;
            }
            return retVal;
        }
        

        public long generatePhoneCall(string Ext, string PhoneNumber)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - generatePhoneCall " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                _phoneListener.GeneratePhoneCall(PhoneNumber, Ext);
                retVal = 0;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - generatePhoneCall ");

            return retVal;
        }


        public TList<Users> getDataByUserNamePassword(string username, string password)
        {
            try
            {
                return DataRepository.UsersProvider.getByUserNamePassword(username, password);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public TList<GisPhoneNumbers> getGisPhoneNumbersByIDObject(long IDObject)
        {
            TList<GisPhoneNumbers> retVal = null;
            try
            {
                retVal = DataRepository.GisPhoneNumbersProvider.GetByIdObject(IDObject);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }


        public TList<GisObjects> getAllGisObjectsByName(string name)
        {
            try
            {
                return DataRepository.GisObjectsProvider.getByObjectName(name);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public Driver getDriverForCard(string description)
        {
            try
            {
                RfIdCardsParameterBuilder pbuilder = new RfIdCardsParameterBuilder();
                pbuilder.AppendEquals(RfIdCardsColumn.Description, description);

                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.Find(pbuilder.GetParameters());
                if ((cards == null) || (cards.Count == 0))
                    return null;

                TList<Driver> drivers = DataRepository.DriverProvider.GetByIDRfIdCard(cards[0].IdRfIdCard);

                if ((drivers == null) || (drivers.Count != 1))
                {
                    return null;
                }
                else
                {
                    Type[] tl = new Type[1];
                    tl[0] = typeof(Company);

                    DataRepository.DriverProvider.DeepLoad(drivers, true, DeepLoadType.IncludeChildren, tl);
                    return drivers[0];
                }


            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public TList<GisStreets> getGisStreetsByName(string name)
        {
            try
            {
                //return null;
                TList<GisStreets> retVal = null;

                //retVal = DataRepository.GisStreetsProvider.Find("StreetName = '" + name + "'");
                retVal = DataRepository.GisStreetsProvider.GetByName(name);
                return retVal;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public long UpdateQueryLogOutUser(DateTime dateOut, long ID_User)
        {
            try
            {
                DataRepository.UserInOutProvider.UpdateQueryLogOutUser(dateOut, ID_User);
                return 0;
            }
            catch (Exception e)
            {
                log.Error("UpdateQueryLogOutUser Error", e);
                return -1;
            }
        }


        public GisRegions getGisRegionForIdRegion(int IdRegion)
        {
            GisRegions retVal = null;

            try
            {
                return DataRepository.GisRegionsProvider.GetByIdRegion(IdRegion);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return retVal;
            }
        }


        public RegisteredPassengers getRegisteredPassingerForCard(string description)
        {
            try
            {
                RfIdCardsParameterBuilder pbuilder = new RfIdCardsParameterBuilder();
                pbuilder.AppendEquals(RfIdCardsColumn.Description, description);

                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.Find(pbuilder.GetParameters());
                if ((cards == null) || (cards.Count == 0))
                    return null;

                TList<RegisteredPassengers> pass = DataRepository.RegisteredPassengersProvider.GetByIdRfIdCard(cards[0].IdRfIdCard);

                if ((pass == null) || (pass.Count != 1))
                {
                    return null;
                }
                else
                {
                    Type[] tl = new Type[2];
                    tl[0] = typeof(Clients);
                    tl[1] = typeof(Company);

                    DataRepository.RegisteredPassengersProvider.DeepLoad(pass, true, DeepLoadType.IncludeChildren, tl);
                    return pass[0];
                }


            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public long insertUserInOut(UserInOut newUserInOut)
        {
            try
            {
                DataRepository.UserInOutProvider.CloseAllOpenLoginsForExtension(newUserInOut.PhoneExtension);

                if (DataRepository.UserInOutProvider.Insert(newUserInOut))
                    return newUserInOut.IdUserLogInOut;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("insertUserInOut Error", e);
                return -1;
            }
        }


        public long updatePhoneCalls(PhoneCalls pPhoneCalls)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.PhoneCallsProvider.Update(pPhoneCalls))
                    retVal = 1;

            }
            catch (Exception e)
            {
                log.Error("updatePhoneCalls Error", e);
                return -1;
            }

            return retVal;
        }


        public TList<GisAddressModel> getGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber)
        {
            try
            {
                //return null;
                TList<GisAddressModel> retVal = null;
                retVal = DataRepository.GisAddressModelProvider.GetIDStreetHouseNumber(ID_Street, houseNumber);
                return retVal;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public TList<PhoneNumbersBlackList> getDataByPhoneNumber(string phoneNumber)
        {
            try
            {
                return DataRepository.PhoneNumbersBlackListProvider.getByPhoneNumber(phoneNumber);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public Clients searchVipClients(ClientPhoneNumbers clientPhoneNumber)
        {
            try
            {
                ClientPhoneNumbersParameterBuilder filtBuild = new ClientPhoneNumbersParameterBuilder();
                //SqlFilterBuilder<ClientPhoneNumbersColumn> filtBuild = new SqlFilterBuilder<ClientPhoneNumbersColumn>(true);
                filtBuild.AppendEquals(ClientPhoneNumbersColumn.PhoneNumber, clientPhoneNumber.PhoneNumber);




                SqlFilterParameter param = new SqlFilterParameter(ClientPhoneNumbersColumn.PhoneNumber, clientPhoneNumber.PhoneNumber, 0);

                SqlFilterParameterCollection paramCol = new SqlFilterParameterCollection();
                paramCol.Add(param);



                TList<ClientPhoneNumbers> phoneNumbers =
                    DataRepository.ClientPhoneNumbersProvider.Find(filtBuild.GetParameters());
                if (phoneNumbers.Count > 0)
                    return DataRepository.ClientsProvider.GetByIdClient(phoneNumbers[0].IdClient);
                else
                    return null;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public Driver getDataLastDriverShift(long ID_Vehicle)
        {
            Driver retVal = null;


            try
            {
                TList<ShiftInOut> tShiftInOut = DataRepository.ShiftInOutProvider.GetByIDVehicleAndDateTimeNull(ID_Vehicle);
                if (tShiftInOut != null && tShiftInOut.Count > 0)
                {
                    ShiftInOut tShift = tShiftInOut[tShiftInOut.Count - 1];
                    retVal = DataRepository.DriverProvider.GetByIdDriver(tShift.IdDriver);
                }

            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }


            return retVal;
        }


        public TList<GisAddressModel> getGisAddressModelForIDStreet(string ID_Street)
        {
            try
            {
                return DataRepository.GisAddressModelProvider.GetByIdStreet(ID_Street);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public bool IsVehicleEligibleForCall(Vehicle pVehicle, PhoneCalls phoneCall)
        {
            bool retVal = false;

            retVal = VehiclesContainer.Instance.IsVehicleEligableForCall(pVehicle.IdVehicle);

            return retVal;
        }





        #region RfIdCardPerClients

        public long saveRfIdCardPerClients(RfIdCardPerClients itemObj)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - saveRfIdCardPerClients " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                if (DataRepository.RfIdCardPerClientsProvider.Insert(itemObj))
                    retVal = itemObj.IdRfIdCardPerClients;
                else
                    retVal = -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - saveRfIdCardPerClients ");

            return retVal;
        }

        public long deleteRfIdCardPerClients(RfIdCardPerClients itemObj)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - deleteRfIdCardPerClients " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                if (DataRepository.RfIdCardPerClientsProvider.Delete(itemObj))
                    retVal = 0;
                else
                    retVal = -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - deleteRfIdCardPerClients ");

            return retVal;
        }

        public long updateRfIdCardPerClients(RfIdCardPerClients itemObj)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - updateRfIdCardPerClients " + tmpCalculateSaveTimeStart);
            long retVal = -1;

            try
            {
                if (DataRepository.RfIdCardPerClientsProvider.Update(itemObj))
                    retVal = 0;
                else
                    retVal = -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = -2;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - updateRfIdCardPerClients ");

            return retVal;
        }

        public TList<RfIdCardPerClients> getAllRfIdCardPerClients()
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getAllRfIdCardPerClients " + tmpCalculateSaveTimeStart);
            TList<RfIdCardPerClients> retVal;

            try
            {

                retVal = DataRepository.RfIdCardPerClientsProvider.GetAll();

                Type[] tl = new Type[2];
                tl[0] = typeof(Clients);
                tl[1] = typeof(RfIdCards);

                DataRepository.RfIdCardPerClientsProvider.DeepLoad(retVal, true, DeepLoadType.IncludeChildren, tl);

            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getAllRfIdCardPerClients ");

            return retVal;
        }

        public RfIdCardPerClients getRfIdCardPerClients(long ID_RfIdCardPerClients)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getRfIdCardPerClients " + tmpCalculateSaveTimeStart);
            RfIdCardPerClients retVal;

            try
            {

                retVal = DataRepository.RfIdCardPerClientsProvider.GetByIdRfIdCardPerClients(ID_RfIdCardPerClients);

                Type[] tl = new Type[2];
                tl[0] = typeof(Clients);
                tl[1] = typeof(RfIdCards);

                DataRepository.RfIdCardPerClientsProvider.DeepLoad(retVal, true, DeepLoadType.IncludeChildren, tl);

            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getRfIdCardPerClients ");

            return retVal;
        }

        public TList<RfIdCardPerClients> getAllRfIdCardPerClientssForIdRfIdCard(long IdRfIdCard, bool permanentlyDiscarded, bool takenBack)
        {
            DateTime tmpCalculateSaveTimeStart = System.DateTime.Now;
            //LogManager.GetLogger("TimeLog").Info("STARTING - getAllRfIdCardPerClientssForIdRfIdCard " + tmpCalculateSaveTimeStart);
            TList<RfIdCardPerClients> retVal;

            try
            {

                retVal =
                    DataRepository.RfIdCardPerClientsProvider.GetAllRfIdCardPerClient(IdRfIdCard, permanentlyDiscarded,
                                                                                      takenBack);
                Type[] tl = new Type[2];
                tl[0] = typeof(Clients);
                tl[1] = typeof(RfIdCards);

                DataRepository.RfIdCardPerClientsProvider.DeepLoad(retVal, true, DeepLoadType.IncludeChildren, tl);

            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }
            DateTime tmpCalculateSaveTimeEnd = System.DateTime.Now;
            TimeSpan tmpCalculatedTime = tmpCalculateSaveTimeEnd.Subtract(tmpCalculateSaveTimeStart);

            if (_debugMode)
                LogManager.GetLogger("TimeLog").Info(tmpCalculatedTime.TotalMilliseconds.ToString() + (char)9 + (char)9 + "END - getAllRfIdCardPerClientssForIdRfIdCard ");

            return retVal;

        }


        #endregion

        #endregion

        public long releaseNextPhoneCall(long ID_Vehicle, long ID_User)
        {
            return VehiclesContainer.Instance.releaseNextPhoneCall(ID_Vehicle);
        }

        public long vehicleToUndefinedState(long ID_Vehicle, long ID_User)
        {
            long retVal = -1;

            if (long.Parse(ConfigurationManager.AppSettings["IdUserForSetToUndefined"]) == ID_User)
                retVal = VehiclesContainer.Instance.ForceStateUndefine(ID_Vehicle, ID_User);

            return retVal;
        }      


        public TList<Vehicle> SelectVehiclesforXYforCompanies(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, List<long> pIdCompanies)
        {
            return VehiclesContainer.Instance.SelectVehiclesforXY(pLongitudeX, pLatitudeY, pIncludeBusy, pNumberOfVehiclesToReturn, pIdCompanies);
        }
       

        public long SelectVehiclesforXYforFirstAndAlternativeCompaniesAndPhoneCall(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pIdFirstCompany, List<long> pIdAlternativeCompanies, PhoneCalls pPhoneCall, long pUser)
        {
            long retVal = -1;

            //ZORAN:    Ovaa metoda:
            //              1. Selektira koli za dadeniot PhoneCall
            //              2. Gi stava PhoneCall + site selektiraniKoli vo Dictionary
            //              3. Generira Kratka Najava i ja vraka kako Dictionary
            //          Od ovde se praka samo kratka najava, a se drugo odi preku GPSListener (DecreaseTimeForAll)    
            Dictionary<long, byte[]> tmpRetVal = VehiclesContainer.Instance.SelectVehiclesforXY(pLongitudeX, pLatitudeY, pIncludeBusy, pNumberOfVehiclesToReturn, pIdFirstCompany, pIdAlternativeCompanies, pPhoneCall, pUser);


            if (tmpRetVal != null && tmpRetVal.Count > 0)
            {
                foreach (KeyValuePair<long, byte[]> kvp in tmpRetVal)
                {
                    _gpsListener.SendMsgToVehicle(kvp.Key, kvp.Value);
                }
                retVal = tmpRetVal.Count;
            }
            return retVal;
        }


        //ZORAN:    Ova e metoda za selekcija na kooperanti
        //          PRAVILO:
        //                  - Ako e povikot na Nase Taksi, a nema koli, prvo se nudi na LOTUS, pa potoa kooperanti
        //                  - Ako e povikot na LOTUS, a nema koli, se odi direktno na kooperanti
        public TList<Vehicle> SelectSubContractorsForXYandPhoneCall(double pLongitudeX, double pLatitudeY, PhoneCalls pPhoneCall, long pUser)
        {
            TList<Vehicle> retVal = null;

            try
            {
                retVal = VehiclesContainer.Instance.SelectSubContractorsForXYandPhoneCall(pLongitudeX, pLatitudeY, pPhoneCall, pUser);
                
                //ZORAN:    Prvo da pratam da im zvoni
                //          Potoa da si zapisam vo baza koi koli bile selektirani
                if (retVal != null && retVal.Count > 0)
                {
                    log.Debug("SelectSubContractorsForXYandPhoneCall,selektirani: " + retVal.Count.ToString() + " za telefon: " + pPhoneCall.PhoneNumber);

                    byte[] retValMessage = null;

                    clsMessageCreator tMessageCreator = new clsMessageCreator();

                    foreach(Vehicle veh in retVal)
                    {
                        retValMessage = tMessageCreator.CreateBeep(veh, 5);

                        _gpsListener.SendMsgToVehicle(veh.IdVehicle, retValMessage);
                    }

                    TList<SubcontractorPendingPhoneCalls> lstSubcontractorPendingPhoneCalls = DataRepository.SubcontractorPendingPhoneCallsProvider.GetByIdPhoneCall(pPhoneCall.IdPhoneCall);

                    if (lstSubcontractorPendingPhoneCalls != null && lstSubcontractorPendingPhoneCalls.Count > 0)
                    {
                        // ZORAN:   TREBA KOREKCIJA!!!
                        //          Moze da ima POVEKE zapisi za ist IdPhoneCall, pa treba da se koregira koj da go zema!!!!
                        SubcontractorPendingPhoneCalls mSubcontractorPendingPhoneCalls = lstSubcontractorPendingPhoneCalls[0];

                        string strListOfSelectedVehicles = "";

                        foreach (Vehicle tmpVeh in retVal)
                        {
                            strListOfSelectedVehicles += tmpVeh.DescriptionShort;
                        }

                        mSubcontractorPendingPhoneCalls.SelectedVehicles = strListOfSelectedVehicles;

                        DataRepository.SubcontractorPendingPhoneCallsProvider.Update(lstSubcontractorPendingPhoneCalls);
                    }
                    else
                    {
                        log.Debug("ZORAN: Treba da se proveri zosto za IdPhoneCall: " + pPhoneCall.IdPhoneCall.ToString()
                                    + " nema zapis vo SubcontractorPendingPhoneCalls !!!");
                    }
                }
                else
                    log.Debug("SelectSubContractorsForXYandPhoneCall, NEMA selektirani koli za telefon: " + pPhoneCall.PhoneNumber);
            }
            catch (Exception ex)
            {
                log.Error("SelectSubContractorsForXYandPhoneCall: ", ex);
            }

            return retVal;
        }



        //ZORAN:    Ova e metoda za potvrda koj kooperant bil praten za konkreten povik
        //          Prethodno se evidentirani koi kooperanti bile selektirani za konkretniot povik
        //          Dopolnitelno, tuka mu se brisat i pari na kooperantot!!!
        //          Ovde samo se otvara nov Thread...se e vo VehicleContainer
        public long ConfirmSubContractorForPhoneCall(long pIdVehicle, PhoneCalls pPhoneCall, long pIdUser)
        {
            long retVal = -1;

            log.Debug("ConfirmSubContractorForPhoneCall: PRED Thread");

            Thread ThreadInformCustomer = new Thread(() => VehiclesContainer.Instance.ProcessSubContractorForPhoneCall(pIdVehicle, pPhoneCall, pIdUser));
            ThreadInformCustomer.Start();

            log.Debug("ConfirmSubContractorForPhoneCall: PRED Thread");

            retVal = 0;

            return retVal;
        }


        //ZORAN:    Ovaa metoda se koristi ako klientot se soglasi sistemot avtomatski da mu prati info na SMS
        //          Ovaa metoda samo INSERT-ira zapis vo tabela PendingPhoneCalls
        //          Nisto drugo, sekcijata na vozila si odi avtomatski, a potvrda odi na SMS
        public long InsertPendingPhoneCalls(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pIdFirstCompany, List<long> pIdAlternativeCompanies, PhoneCalls pPhoneCall, string pContactPhoneNumber, long pUser)
        {
            long retVal = -1;

            PendingPhoneCalls mPendingPhoneCalls = new PendingPhoneCalls();

            try
            {
                mPendingPhoneCalls.IdPhoneCall = pPhoneCall.IdPhoneCall;
                mPendingPhoneCalls.ContactPhoneNumber = pContactPhoneNumber;
                mPendingPhoneCalls.IdRegionFrom = pPhoneCall.oAddressFrom.oGisRegions.IdRegion;
                mPendingPhoneCalls.HouseNumberFrom = pPhoneCall.oAddressFrom.HouseNumber.ToString();

                if(pPhoneCall.oAddressFrom.oGisStreets != null)
                    mPendingPhoneCalls.IdStreetFrom = long.Parse(pPhoneCall.oAddressFrom.oGisStreets.IdStreet);

                if(pPhoneCall.oAddressFrom.oGisObjects != null)
                    mPendingPhoneCalls.IdObjectFrom = pPhoneCall.oAddressFrom.oGisObjects.IdObject;

			    mPendingPhoneCalls.LocationQuality = pPhoneCall.oAddressFrom.LocationQuality;

                if(pPhoneCall.oAddressTo != null && pPhoneCall.oAddressTo.oGisRegions != null)
                    mPendingPhoneCalls.IdRegionTo = pPhoneCall.oAddressTo.oGisRegions.IdRegion;

                mPendingPhoneCalls.LongitudeX = pLongitudeX;
                mPendingPhoneCalls.LatitudeY = pLatitudeY;
                mPendingPhoneCalls.PickUpAddress = pPhoneCall.oAddressFrom.PickUpAddress;
                mPendingPhoneCalls.To = pPhoneCall.oAddressFrom.To;
                mPendingPhoneCalls.Comment = pPhoneCall.oAddressFrom.Comment;
                mPendingPhoneCalls.IdUser = pUser;
                mPendingPhoneCalls.NumberOfVehiclesToReturn = pNumberOfVehiclesToReturn;
                mPendingPhoneCalls.IncludeBusy = pIncludeBusy;
                mPendingPhoneCalls.IdFirstCompany = pIdFirstCompany;

                mPendingPhoneCalls.IdAlternativeCompanies = "";

                string tmpString = "";

                if (pIdAlternativeCompanies != null && pIdAlternativeCompanies.Count > 0)
                    foreach (long mLongId in pIdAlternativeCompanies)
                    {
                        if (tmpString.Length != 0)
                            tmpString = tmpString + ",";
                        tmpString = tmpString + mLongId.ToString();
                    }

                mPendingPhoneCalls.IdAlternativeCompanies = tmpString;
                
                mPendingPhoneCalls.NumberOfRetries = 0;
                mPendingPhoneCalls.DateTimeSubmitted = System.DateTime.Now;
                mPendingPhoneCalls.IsCompleted = false;
                mPendingPhoneCalls.ConfirmedToPassangerAsMissed = false;
                

                DataRepository.PendingPhoneCallsProvider.Insert(mPendingPhoneCalls);

                retVal = 1;
                
            }
            catch (Exception ex)
            {
                log.Error("InsertPendingPhoneCalls: ", ex);
            }


            return retVal;
        }

        public void ForceUpdateUnit(long pIdVehicle, string pUnit, char pIp1, char pIp2, char pIp3, char pIp4, int pPort)
        {
            byte[] retVal = null;

            clsMessageCreator tMessageCreator = new clsMessageCreator();

            retVal = tMessageCreator.CreateForceUpdateUnit(pUnit, pIp1, pIp2, pIp3, pIp4, pPort);

            _gpsListener.SendMsgToVehicle(pIdVehicle, retVal);
        }
       
    }
}
