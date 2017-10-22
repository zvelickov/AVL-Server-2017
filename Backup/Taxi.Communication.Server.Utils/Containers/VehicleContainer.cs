using System;
using System.Collections.Generic;
using System.Text;

// //using JP.Data.Utils;
using log4net;

using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;

using System.Configuration;

using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.Utils.Classes;
using Taxi.Communication.Server;

using System.Globalization;


namespace Taxi.Communication.Server.Utils.Containers
{
    //Singleton
    public sealed class VehiclesContainer
    {
        private static volatile VehiclesContainer instance;
        private static object syncRoot = new Object();

        private MapUtils _mapUtils;

        private Dictionary<long, TList<Vehicle>> gl_Vechicles;

        private Dictionary<long, TList<Vehicle>> lstOrdersPerVehicles;

        private Dictionary<long, Vehicle> _lstByID_Vehicle;

        private Dictionary<string, Vehicle> _lstByUnitSerial;

        private int NumberOfOrdersPerVehicle;

        public ILog log = LogManager.GetLogger("MyService");

        private VehiclesPerRegionsContainer _vehRegionsTO; //se azurira so setRegionTo i sendAddress
        private VehiclesPerRegionsContainer _vehRegionsCurrent; //se azurira so update

        private OngoingOrdersContainer _ongoingOrdersContainer;

        private DateTime dtDateTimeLastCheckedForPendingPhoneCalls;
        private DateTime dtDateTimeLastCheckedForMissedPendingPhoneCalls;

        #region Private
        private Vehicle getVehicle(long ID_Vehicle)
        {
            return null;
        }

        private Vehicle getVehicle(long ID_Vehicle, long ID_Company)
        {
            return null;
        }
        #endregion

        #region Singleton generation        

        private VehiclesContainer()
        {
            gl_Vechicles = new Dictionary<long, TList<Vehicle>>();
            _lstByID_Vehicle = new Dictionary<long, Vehicle>();
            _lstByUnitSerial = new Dictionary<string, Vehicle>();
            lstOrdersPerVehicles = new Dictionary<long, TList<Vehicle>>();
            _ongoingOrdersContainer = new OngoingOrdersContainer();

            NumberOfOrdersPerVehicle = int.Parse(ConfigurationManager.AppSettings["NumberOfOrdersPerVehicle"]);
        }       

       

        

        public static VehiclesContainer Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                            instance = new VehiclesContainer();
                    }
                }

                return instance;
            }
        }
        #endregion

        public void initialize(Dictionary<long, TList<Vehicle>> newList, MapUtils mapUtils)
        {
            lock (syncRoot)
            {
                gl_Vechicles = newList;
                _lstByID_Vehicle = new Dictionary<long, Vehicle>();
                _lstByUnitSerial = new Dictionary<string, Vehicle>();
                _vehRegionsTO = new VehiclesPerRegionsContainer();
                _vehRegionsCurrent = new VehiclesPerRegionsContainer();
                _mapUtils = mapUtils;

                dtDateTimeLastCheckedForPendingPhoneCalls = System.DateTime.Now;
                dtDateTimeLastCheckedForMissedPendingPhoneCalls = System.DateTime.Now;

                foreach (KeyValuePair<long, TList<Vehicle>> pair in gl_Vechicles)
                {
                    foreach (Vehicle veh in pair.Value)
                    {
                        _lstByID_Vehicle.Add(veh.IdVehicle, veh);

                        _lstByUnitSerial.Add(veh.IdUnitSource.SerialNumber, veh);
                    }
                }
            }
        }       


        #region General Utilities

        public bool isPresent(long ID_Vehicle)
        {
            return _lstByID_Vehicle.ContainsKey(ID_Vehicle);
        }

        public long getVehicleIDForUnitSerial(string unitSerialNumber)
        {
            Vehicle veh = null;
            long retVal = -1;
            lock (syncRoot)
            {
                if (_lstByUnitSerial.ContainsKey(unitSerialNumber))
                {
                    veh = _lstByUnitSerial[unitSerialNumber];
                    retVal = veh.IdVehicle;
                }
                else
                    retVal = -1;
            }
            return retVal;
        }

        public Dictionary<Vehicle, List<byte[]>> decreaseTimeOutToAll()
        {
            List<byte[]> retValByte;
            Dictionary<Vehicle, List<byte[]>> retVal = new Dictionary<Vehicle, List<byte[]>>();

            foreach (Vehicle veh in _lstByID_Vehicle.Values)
            {
                retValByte = veh.currentState.decreaseTimeOut();

                if (retValByte != null && retValByte.Count > 0)
                {
                    retVal.Add(veh, retValByte);
                }
            }
            return retVal;
        }

        public List<Dictionary<long, byte[]>> decreaseTimeForOrders()
        {
            List<Dictionary<long, byte[]>> retVal = new List<Dictionary<long, byte[]>>();

            retVal = _ongoingOrdersContainer.decreaseTimeForOrders();            

            return retVal;
        }


        public void addNewOrderConfirmation(clsPotvrdaNaNaracka pPotvrdaNaNaracka)
        {
            _ongoingOrdersContainer.addNewOrderConfirmation(pPotvrdaNaNaracka);
        }

        public void clearStateChanged(long ID_Vehicle)
        {
            Vehicle veh;
            lock (syncRoot)
            {
                if (_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                {
                    veh = _lstByID_Vehicle[ID_Vehicle];
                    veh.StateChanged = false;
                    veh.DirtyStateChange = false;
                }
            }
        }

        public TList<Vehicle> getAllForCompany(long IdCompany)
        {
            TList<Vehicle> retVal = null;
            lock (syncRoot)
            {
                if (!gl_Vechicles.ContainsKey(IdCompany))
                    return null;

                retVal = new TList<Vehicle>(gl_Vechicles[IdCompany]);
            }
            return retVal;
        }

        public Vehicle getSingleVehicle(long ID_Vehicle, long ID_Company)
        {
            return this.getSingleVehicle(ID_Vehicle);
        }

        public long getVehicleIdRegion(long ID_Vehicle)
        {
            lock (syncRoot)
            {
                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return 0;
                if (_lstByID_Vehicle[ID_Vehicle].currentGPSData == null)
                    return 0;

                return _lstByID_Vehicle[ID_Vehicle].currentGPSData.IdRegion;

            }
        }

        public long getIdPhoneCall(long ID_Vehicle)
        {
            long retVal = -1;

            lock (syncRoot)
            {
                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return retVal;

                if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall == null)
                {
                    return retVal;
                }
                else
                {
                    return _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.IdPhoneCall;
                }

            }

        }


        public Vehicle getSingleVehicleZOKI(long ID_Vehicle)
        {
            Vehicle veh = null;

            lock (syncRoot)
            {

                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return null;
                return _lstByID_Vehicle[ID_Vehicle];
            }
        }


        public Vehicle getSingleVehicle(long ID_Vehicle)
        {
            Vehicle veh = null;
            lock (syncRoot)
            {

                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return null;

                veh = _lstByID_Vehicle[ID_Vehicle].Copy();
                veh.OnStationFromDateTime = _lstByID_Vehicle[ID_Vehicle].OnStationFromDateTime;
                veh.currentState = _lstByID_Vehicle[ID_Vehicle].currentState.Copy();
                veh.currentStateString = _lstByID_Vehicle[ID_Vehicle].currentStateString;
                veh.IdUnitSource = _lstByID_Vehicle[ID_Vehicle].IdUnitSource.Copy();
                veh.IdCompanySource = _lstByID_Vehicle[ID_Vehicle].IdCompanySource.Copy();

                if (_lstByID_Vehicle[ID_Vehicle].CurrentRfIdCard != null)
                {
                    veh.CurrentRfIdCard = _lstByID_Vehicle[ID_Vehicle].CurrentRfIdCard.Copy();
                }
                
                

                if (_lstByID_Vehicle[ID_Vehicle].NextPhoneCall != null)
                {
                    veh.NextPhoneCall = _lstByID_Vehicle[ID_Vehicle].NextPhoneCall.Copy();
                    veh.NextPhoneCall.currentState = _lstByID_Vehicle[ID_Vehicle].NextPhoneCall.currentState.Copy();
                }

                if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall != null)
                {
                    veh.CurrentPhoneCall = _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.Copy();

                    try
                    {
                        if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressTo != null)
                        {                            
                            veh.CurrentPhoneCall.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                            veh.CurrentPhoneCall.oAddressTo = _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressTo;

                            if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressTo.oGisRegions != null)
                            {
                                //ZORAN:    Ovde pagja, pa ako padne mu stavam prazen objekt za GisRegionsTo
                                //          Ne sum 100% siguren, no ova MISLAM deka ne se koristi nikade 
                                try
                                {                                   
                                    veh.CurrentPhoneCall.oAddressTo.oGisRegions =
                                        _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressTo.oGisRegions.Copy();
                                }
                                catch (Exception ex)
                                {
                                    //log.Error("ZORAN: padna na: _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressTo.oGisRegions.Copy()");
                                    //log.Error("Ne znam zosto paga, no mislam deka nikade nese koristi!!!!");

                                    veh.CurrentPhoneCall.oAddressTo.oGisRegions = null; // new GisRegions();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("ZORAN: ovde paga samo na nekoi vozila, ne mi e jasno zosto...Vozilo ID: " + veh.IdVehicle.ToString() + " (" + veh.Plate + ")");
                        log.Error(ex);
                    }

                    if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressFrom != null)
                    {
                        veh.CurrentPhoneCall.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                        veh.CurrentPhoneCall.oAddressFrom.LocationX =
                            _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressFrom.LocationX;

                        veh.CurrentPhoneCall.oAddressFrom.LocationY =
                            _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressFrom.LocationY;

                        try
                        {
                            if (_lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressFrom.oGisRegions != null)
                            {
                                veh.CurrentPhoneCall.oAddressFrom.oGisRegions =
                                    _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall.oAddressFrom.oGisRegions.Copy();
                            }
                        }
                        catch (Exception ex)
                        {
                            veh.CurrentPhoneCall.oAddressFrom.oGisRegions = null; // new GisRegions();
                        }
                    }

                    //veh.NextPhoneCall.currentState = _lstByID_Vehicle[ID_Vehicle].NextPhoneCall.currentState.Copy();
                }

                

                if (veh.currentGPSData == null)
                    veh.currentGPSData = new GPSData();

                veh.currentGPSData.IdLocation = _lstByID_Vehicle[ID_Vehicle].currentGPSData.IdLocation;

                //Zoran: Pajo da go proveri ovoj red, go kucam na princip copy/paste :)

                veh.IdCompanySource.IdCompanyTimeZoneSource = _lstByID_Vehicle[ID_Vehicle].IdCompanySource.IdCompanyTimeZoneSource.Copy();
                veh.OnStationFromDateTime = _lstByID_Vehicle[ID_Vehicle].OnStationFromDateTime;

                veh.IDLastPhoneCall = _lstByID_Vehicle[ID_Vehicle].IDLastPhoneCall;                                
                veh.SelectionIndex = _lstByID_Vehicle[ID_Vehicle].SelectionIndex;
                veh.DistanceToAddress = _lstByID_Vehicle[ID_Vehicle].DistanceToAddress;

                veh.TypeOfOrder = _lstByID_Vehicle[ID_Vehicle].TypeOfOrder;
                
                if(_lstByID_Vehicle[ID_Vehicle].lastPhoneCallAccepted != null)
                    veh.lastPhoneCallAccepted = _lstByID_Vehicle[ID_Vehicle].lastPhoneCallAccepted;
            }
            return veh;
        }

        public List<Vehicle> getAllVehicles()
        {
            lock (syncRoot)
            {
                return new List<Vehicle>(this._lstByID_Vehicle.Values);
            }
        }       

        #endregion

        public Vehicle getVehicleObjForUnitSerial(string unitSerialNumber)
        {
            Vehicle retVal = null;
            lock (syncRoot)
            {
                if (_lstByUnitSerial.ContainsKey(unitSerialNumber))
                {
                    retVal = _lstByUnitSerial[unitSerialNumber];
                }
            }



            return retVal;
        }



        #region Actions on states

        public long setLastMessage(long ID_Vehicle, ReceivedShortMessage shortMessage)
        {
            long retVal = -1;

            lock (syncRoot)
            {
                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                veh.oReceivedShortMessage = shortMessage;

                // ZORAN:   Ova go dodavam PRIVREMENO za da se update-ra property-to oRegionTo
                //          PRIVREMENO e zatoa sto kaj Marjan gi nema site reoni + reonite se so pogresni ID-ja
                //          Toa se pravi vo slednite slucai:
                //              1. Ako CurrentState = StateBusy
                //              2. Ako prethodno ne pritisnal za drug reon
                //              3. Ako ID_ShortMessage pripaga na nekoi od GisRegions
                //          Ovde se hard-codira, NE SMEE DA BIDE TAKA!!!!
                //
                //          VAZNO:  oREgionTo se RESETIRA i vo sekoj state, ako toa ne e StateBusy
                //                  bidejki, ednas koga ke stigne poraka, taa stoi se dodeka ne smeni status SatteBusy
                //                  Za sekoj slucaj, se resetira i tuka!

                /* Pajo commented. Promenetiot code e podolu!
                if (veh.currentStateString == "StateBusy")
                {
                    if (veh.oRegionTo == null)
                    {
                        GisRegions tmpGisRegions = GetGisRegionToFromReceivedShortMessage(shortMessage);

                        if (tmpGisRegions != null)
                            veh.oRegionTo = tmpGisRegions;
                    }
                }
                else
                    veh.oRegionTo = null;
                */

                if (veh.currentStateString == "StateBusy")
                {
                    if (veh.CurrentPhoneCall == null)
                    {
                        veh.CurrentPhoneCall = new PhoneCalls();
                        veh.CurrentPhoneCall.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                    }

                    if (veh.CurrentPhoneCall.oAddressTo == null)
                    {
                        veh.CurrentPhoneCall.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                    }                   
                }
                else
                {
                    if (veh.CurrentPhoneCall != null && veh.CurrentPhoneCall.oAddressTo != null)
                    {
                        veh.CurrentPhoneCall.oAddressTo.oGisRegions = null;
                    }
                }

                veh.StateChanged = true;
            }
            return retVal;
        }        



        // ZORAN:   Ova e procedura koja se povikuva posle primanje na poraka za RegiontTo (poraka 56)
        //          Porano imase improvizacija i se pravese vo procedurata SetLastMessage
        //          
        // *************************************************************************************************
        public long setRegionTo(long ID_Vehicle, long pGisRegion)
        {
            long retVal = -1;

            lock (syncRoot)
            {
                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];

                if ((veh.currentStateString == "StateBusy") ||
                    (veh.currentStateString == "StateMoveToClient") ||
                    (veh.currentStateString == "StateWaitClient")
                    )
                {
                    GisRegions tmpGisRegion = GlobSaldo.AVL.Data.DataRepository.GisRegionsProvider.GetByIdRegion(pGisRegion);

                    if (veh.CurrentPhoneCall == null)
                    {
                        veh.CurrentPhoneCall = new PhoneCalls();
                        veh.CurrentPhoneCall.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                    }

                    if (veh.CurrentPhoneCall.oAddressTo == null)
                    {
                        veh.CurrentPhoneCall.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                    }
                    veh.CurrentPhoneCall.oAddressTo.oGisRegions = tmpGisRegion;

                    if (tmpGisRegion != null)
                    {
                        _vehRegionsTO.setVehicleRegion(ID_Vehicle, tmpGisRegion.IdRegion);
                    }
                    else
                    {
                        _vehRegionsTO.setVehicleRegion(ID_Vehicle, -1);
                    }

                    veh.StateChanged = true;

                    retVal = 0;
                }
            }
            return retVal;
        }



        // ZORAN:   Ova e procedura koja se povikuva posle primanje na poraka za FreeText (poraka 71)        
        //          
        // *************************************************************************************************
        public long setReceivedFreeText(long ID_Vehicle, ReceivedFreeText pReceivedFreeText)
        {
            long retVal = -1;

            lock (syncRoot)
            {
                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];

                veh.oReceivedFreeText = pReceivedFreeText;

                veh.StateChanged = true;

                retVal = 0;

            }
            return retVal;
        }

        
        // ZORAN:   Ova e procedura koja se povikuva za UPDATE na ReceivedFreeText (UPDATE od strana na Dispecer)
        //          Ova e za da napravi update do site dispeceri. Se povikuva i od Confirm i od Cancel
        //          
        // *************************************************************************************************
        public long UpdateReceivedFreeText(long ID_Vehicle, ReceivedFreeText pReceivedFreeText)
        {
            long retVal = -1;

            lock (syncRoot)
            {
                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];

                veh.oReceivedFreeText = pReceivedFreeText;

                veh.StateChanged = true;

                retVal = 0;

            }
            return retVal;
        }


        

        public List<byte[]> update(long ID_Vehicle, GPSData gpsData, SensorData sensData, long IdLocation)
        {

            List<byte[]> retVal = null;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];

                //TODO ova e otkako ke se prosiri klasata State so ID_Location
                //veh.currentState.IdLocation = IdLocation;

                //_mapUtils.GetIsTaxiAtStationSensorGIS(veh, gpsData.Longitude_X, gpsData.Latutude_Y, sensData.Senzor_12);
                //_mapUtils.GetIsTaxiAtStationSensorDB(veh, gpsData.Longitude_X, gpsData.Latutude_Y, sensData.Senzor_12);

                retVal = veh.currentState.Update(gpsData, sensData);
                _vehRegionsCurrent.setVehicleRegion(ID_Vehicle, gpsData.IdRegion);
            }
            return retVal;

        }

        public List<byte[]> update(long ID_Vehicle, Locations loc)
        {

            List<byte[]> retVal = null;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];

                //TODO ova e otkako ke se prosiri klasata State so ID_Location
                //veh.currentState.IdLocation = IdLocation;

                veh.Station = (bool)loc.Station;

                retVal = veh.currentState.Update(loc.GpsData, loc.SensData);

            }
            return retVal;

        }

        public long reserveVehicle(long ID_Vehicle)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.reserveVehicle();

            }
            return retVal;
        }


        public long releaseVehicle(long ID_Vehicle)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.releaseVehicle();

            }
            return retVal;
        }

        public long releaseNextPhoneCall(long ID_Vehicle)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.releaseSecondCall();

            }
            return retVal;
        }

        public bool IsVehicleEligableForCall(long ID_Vehicle)
        {
            bool retVal = false;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.IsVehicleEligableForCall();

            }
            return retVal;
        }

        
        

        public long setRfIdCardPassinger(long ID_Vehicle, RfIdCards card)
        {
            long retVal = -1;
            lock (syncRoot)
            {
                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return -1;

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.setClientRfIdCard(card);
            }
            return retVal;
        }

        public bool setRiminderForRfId(long ID_Vehicle)
        {
            bool retVal = false;
            lock (syncRoot)
            {
                if (!_lstByID_Vehicle.ContainsKey(ID_Vehicle))
                    return false;

                retVal = _lstByID_Vehicle[ID_Vehicle].currentState.setRiminderForRfId();
            }
            return retVal;
        }

        
        public byte[] sendAddress(long ID_Vehicle, long ID_User, PhoneCalls phoneCall)
        {
            byte[] retVal = null;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.SendAddress(ID_User, phoneCall);

                if ((phoneCall.oAddressTo != null) && (phoneCall.oAddressTo.oGisRegions != null))
                {
                    _vehRegionsTO.setVehicleRegion(ID_Vehicle, phoneCall.oAddressTo.oGisRegions.IdRegion);
                }
                else
                {
                    _vehRegionsTO.setVehicleRegion(ID_Vehicle, -1);
                }

            }
            return retVal;
        }

        public long cancelRequestFromClient(long ID_Vehicle)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.CancellRequestFromClient();

            }
            return retVal;
        }

        public string getCurrentStateString(long ID_Vehicle)
        {
            string retVal = ""; ;
            if (!isPresent(ID_Vehicle))
                return retVal;

            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentStateString;

            }
            return retVal;
        }

        public long acceptedFromClient(long ID_Vehicle)
        {
            long retVal = -1;
            if (!isPresent(ID_Vehicle))
                return retVal;

            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.acceptedFromClient();

            }
            return retVal;
        }

        public long extendClientWatiTime(long ID_Vehicle)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.ExtendWaitClientTime();

            }
            return retVal;
        }


            
        public long ForceStateUndefine(long ID_Vehicle, long ID_User)
        {
            long retVal = -1;
            lock (syncRoot)
            {

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                retVal = veh.currentState.forceToUndefined(ID_User);

            }
            return retVal;
        }


        public long SetStateForSelectedVehicle(long ID_Vehicle, PhoneCalls pPhoneCall, int pMinuti, long ID_User)
        {
            long retVal = -1;
            lock (syncRoot)
            {                

                _lstByID_Vehicle[ID_Vehicle].lastPhoneCallAccepted = new PhoneCallAccepted();

                _lstByID_Vehicle[ID_Vehicle].lastPhoneCallAccepted.IdPhoneCall = pPhoneCall.IdPhoneCall;
                _lstByID_Vehicle[ID_Vehicle].lastPhoneCallAccepted.Minuti = pMinuti;

                _lstByID_Vehicle[ID_Vehicle].CurrentPhoneCall = pPhoneCall;

                Vehicle veh = _lstByID_Vehicle[ID_Vehicle];
                

                // ZORAN:       Tuka treba da se stavi vo StateWaitClientConfirmation ako e PhoneCall.MessageType == "MC"
                //              NO, za toase grizi metodata vo konkretnaat Satte
                // VNIMANIE:    Treba da promenam i vo WaitForDriverConfiramtion!!!!

                retVal = veh.currentState.updateStateForSelectedVehicle(pPhoneCall, pMinuti);
               
            }
            return retVal;
        }

        public TList<GisSearchRegions> getGisSearchRegionsByIdRegion(long ID_Region, int typeAlternative)
        {
            try
            {
                return GlobSaldo.AVL.Data.DataRepository.GisSearchRegionsProvider.GetByIdRegion (ID_Region);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public TList<Vehicle> searchAllVechiclesByIDReaon(long? ID_Region, int typeAlternative)
        {
            long idRegion = ID_Region ?? 0;

            TList<Vehicle> retVal = new TList<Vehicle>();

            TList<GisSearchRegions> lstGisSearchRegions = new TList<GisSearchRegions>();

            int tTypeRegion = 0;

            try
            {

                lstGisSearchRegions = getGisSearchRegionsByIdRegion(idRegion, tTypeRegion);

                if (lstGisSearchRegions != null && lstGisSearchRegions.Count != 0)
                {
                    foreach (GisSearchRegions gsr in lstGisSearchRegions)
                    {
                        foreach (Vehicle tVehicle in VehiclesContainer.Instance.getAllVehicles())
                        {
                            if ((tVehicle.currentGPSData.IdRegion == gsr.IdAlternativeregion))
                            {
                                tVehicle.RegionSearchType = gsr.TypeAlternative;
                                retVal.Add(tVehicle);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                return null;
            }

            return retVal;
        }

        #endregion


        #region VehicleSelectionForXY
       
      

        // ZORAN:   Selekcija na kola prema XY koordinatite na na povikot, ZA LISTA od ID-ja na KOMPANII nezavisno kako e dojdeno do niv
        //          PARAMETRI:
        //                  -   pLongitudeX i  pLatitudeY se jasni
        //                  -   pIncludeBusy e dali da gi vkluci vo selekcijata i kolite so patnik!!!
        //                      AKO e narackata od mobilen ili od WEB, pBusy MORA da e FALSE
        //                      NEMA logika toj so mobilen da ceka nekoj da go ostavi patnikot, pa da dojde po nego!
        //                  -   pNumberOfVehiclesToReturn e MAX broj na vozila koi procedurata treba da gi vrati.
        //                      Za mobilni useri + WEB mora da vraka SAMO 1!

        //ZORAN:    Ovaa metoda se koristi SAMO za MOBILNI uredi
        //          Ke se koristi se dodeka ne se stavi na android da bira primarni i sekundarni kompanii za selekcijs
        //          Tuka, dopolnitelno e staveno da se proveruva dali Vehicle.TypeOfOrder = 1
        // **********************************************************************************************************
        public TList<Vehicle> SelectVehiclesforXY(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, List<long> pIdCompanies)
        {

            TList<Vehicle> retVal = new TList<Vehicle>();

            TList<Vehicle> tmpVehiclesInEligableRegions = new TList<Vehicle>();


            // ZORAN:   Tuka stavam hard-coded ID_COmpany, ne znam kako da ja dobijam tuka!!!
            //          DOPOLNITELNO, funkcijata vo baza ne vodi smetka za ID_Company!!!
            //          Funkcijata vraka lista, a mi treba samo eden region. Go zemam prviot!!!

            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> retValRegions = null;
            GlobSaldo.GISDB.Entities.Vregions m_CurrentRegion = null;

            try
            {
                retValRegions = GlobSaldo.GISDB.Data.DataRepository.VregionsProvider.uGetRegions((decimal)pLongitudeX, (decimal)pLatitudeY, (long)1);

            }
            catch (Exception ex)
            {
                //ServiceCallBack.log.Error("Greska vo procesiranje na GisREgionsDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retValRegions = null;
            }

            if (retValRegions != null && retValRegions.Count > 0)
            {
                m_CurrentRegion = retValRegions[0];
            }
            else
            {
                return null;
            }


            // ZORAN:   Prvo da gi zemam site regioni eligable za toj od kade e povikot
            //          PLUS: dali, soglasno STATE-ot moze da primat naracka!


            TList<GisSearchRegions> lstEligableGisSearchRegions = GlobSaldo.AVL.Data.DataRepository.GisSearchRegionsProvider.GetByIdRegion ((long)m_CurrentRegion.IdRegion);

            if (lstEligableGisSearchRegions != null && lstEligableGisSearchRegions.Count > 0)
            {
                // ZORAN:   Sega gi selektiram site vozila koi se vo selektiranite regioni
                //          PLUS, soglasno STATE-ot dali se eligable
                //          ISTOVREMENO: Gi mnozam gsr.TypeAlternative so 100000 i gi stavam vo SelectionIndex (tuka mi e vo eden prolaz!!)

                List<Vehicle> tmplstVehiclesAll = VehiclesContainer.Instance.getAllVehicles();

                List<Vehicle> tmplstVehicles = new List<Vehicle>();

                // ZORAN:   tuka e edinstvenata razlika od prethodnata funkcija
                //          Gi zadrzuvam samo vozilata na kompaniiete cii ID-ja se vo listata
                // ZORAN:   Ovde dodadov i proverka na Vehicle.TypeOfOrder = 1 (selekcija na vozila po staro!)
                foreach (long tmpIdCompany in pIdCompanies)
                    foreach (Vehicle tmpVehicle in tmplstVehiclesAll)
                        if ((tmpVehicle.IdCompany == tmpIdCompany) && (tmpVehicle.TypeOfOrder == 1))
                            tmplstVehicles.Add(tmpVehicle);



                foreach (Vehicle veh in tmplstVehicles)
                {
                    foreach (GisSearchRegions gsr in lstEligableGisSearchRegions)
                    {
                        if (veh.CurrentPhoneCall != null && veh.CurrentPhoneCall.oAddressTo != null && veh.CurrentPhoneCall.oAddressTo.oGisRegions != null)
                        {
                            if (VehiclesContainer.Instance.IsVehicleEligableForCall(veh.IdVehicle) && ((veh.currentGPSData.IdRegion == gsr.IdAlternativeregion) || (veh.CurrentPhoneCall.oAddressTo.oGisRegions.IdRegion == gsr.IdAlternativeregion)))
                            {
                                veh.SelectionIndex = gsr.TypeAlternative * 100000;
                                tmpVehiclesInEligableRegions.Add(veh);
                            }
                        }
                        else
                        {
                            if (VehiclesContainer.Instance.IsVehicleEligableForCall(veh.IdVehicle) && veh.currentGPSData.IdRegion == gsr.IdAlternativeregion)
                            {
                                veh.SelectionIndex = gsr.TypeAlternative * 100000;
                                tmpVehiclesInEligableRegions.Add(veh);
                            }
                        }
                    }
                }
            }
            else
            {
                retVal = null;
            }

            // ZORAN:   Tuka gi imam site eligable vehicles, pa treba da napravam kalulacija i sortiranje
            //          Reonot (nulti, prvi, ... do MAX 9-ti, mi e prethodno iskalkulirano!

            //PRVO, da go iskalkulirame rastojanieto
            //_________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                double tmpDistance = Calc(pLatitudeY, pLongitudeX, tmpVevicle.currentGPSData.Latutude_Y, tmpVevicle.currentGPSData.Longitude_X);

                tmpVevicle.DistanceToAddress = (long)tmpDistance;

                tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 300 * (int)(tmpDistance / 200);
            }

            //VTORO:    Sega, da zememe predvid dali se dvizi ili ne
            //_________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                if (tmpVevicle.currentGPSData.Speed > 40)
                {
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 50000;
                }
            }


            //TRETO:    Sega, da zememe predvid koj koga isklucil taksimetar
            //          NAPOMENA:   Ako voopsto nema vkluceno taksimetar, ke mu se odzemaat 200 boda (=kako da isklucil pred 200 min) 
            //________________________________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                if (tmpVevicle.TaximetarLast == DateTime.MinValue)
                {
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex - 200;
                }
                else
                {
                    TimeSpan interval = DateTime.Now - tmpVevicle.TaximetarLast;
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex - (int)interval.TotalMinutes;
                }
            }

            //CETVRTO:    Ako voziloto e vo state BUSY, treba soodvetno da se odrabotat, soglasno pBusy parametarot
            //_________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                if (pIncludeBusy == true)
                {
                    if (tmpVevicle.currentStateString == "StateBusy" &&
                        tmpVevicle.CurrentPhoneCall.oAddressTo.oGisRegions != null &&
                        tmpVevicle.CurrentPhoneCall.oAddressTo.oGisRegions.IdRegion == (long)m_CurrentRegion.IdRegion)
                    {
                        tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 1000000;
                        retVal.Add(tmpVevicle);
                    }
                    else
                    {
                        if (tmpVevicle.currentStateString != "StateBusy")
                            retVal.Add(tmpVevicle);
                    }
                }

                else
                {
                    if (tmpVevicle.currentStateString != "StateBusy")
                        retVal.Add(tmpVevicle);
                }


            }

            if (retVal != null)
            {
                retVal.Sort("SelectionIndex");

                // I poslednoto, kolku zapisi da vrati...

                if (pNumberOfVehiclesToReturn >= retVal.Count)
                    return retVal;
                else
                {

                    TList<Vehicle> retVal2 = new TList<Vehicle>();

                    for (int i = 0; i < pNumberOfVehiclesToReturn; i++)
                    {
                        retVal2.Add(retVal[i]);
                    }

                    return retVal2;
                }

            }
            else
            {
                return retVal;
            }
        }


        // ZORAN:   Selekcija na kola prema XY koordinatite na na povikot, Prvo za Kompanijata od kade e povikot, do 2-ro nivo na reoni, a potoa za site drugi
        //                  -   pLongitudeX i  pLatitudeY se jasni
        //                  -   pIncludeBusy e dali da gi vkluci vo selekcijata i kolite so patnik!!!
        //                      AKO e narackata od mobilen ili od WEB, pBusy MORA da e FALSE
        //                      NEMA logika toj so mobilen da ceka nekoj da go ostavi patnikot, pa da dojde po nego!
        //                  -   pNumberOfVehiclesToReturn e MAX broj na vozila koi procedurata treba da gi vrati.
        //                      Za mobilni useri + WEB mora da vraka SAMO 1!
        // **********************************************************************************************************
        public TList<Vehicle> SelectVehiclesforXY(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pFirstCompany, List<long> pAlternativeCompanies, PhoneCalls pPhoneCall)
        {
            if (pFirstCompany == 0)
                return null;


            TList<Vehicle> retVal = new TList<Vehicle>();

            TList<Vehicle> tmpVehiclesInEligableRegions = new TList<Vehicle>();


            // ZORAN:   Tuka stavam hard-coded ID_COmpany, ne znam kako da ja dobijam tuka!!!
            //          DOPOLNITELNO, funkcijata vo baza ne vodi smetka za ID_Company!!!
            //          Funkcijata vraka lista, a mi treba samo eden region. Go zemam prviot!!!

            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> retValRegions = null;
            GlobSaldo.GISDB.Entities.Vregions m_CurrentRegion = null;

            try
            {
                retValRegions = GlobSaldo.GISDB.Data.DataRepository.VregionsProvider.uGetRegions((decimal)pLongitudeX, (decimal)pLatitudeY, (long)1);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                retValRegions = null;
            }

            if (retValRegions != null && retValRegions.Count > 0)
            {
                m_CurrentRegion = retValRegions[0];
            }
            else
            {
                return null;
            }


            // ZORAN:   Prvo da gi zemam site regioni eligable za toj od kade e povikot
            //          PLUS: dali, soglasno STATE-ot moze da primat naracka!            

            TList<GisSearchRegions> lstEligableGisSearchRegions = GlobSaldo.AVL.Data.DataRepository.GisSearchRegionsProvider.GetByIdRegion ((long)m_CurrentRegion.IdRegion);

            // ZORAN:   Sega imam regioni (proveruvam da ne e prazna listata, za sekoj slucaj)
            //          pa prvo proveruvam Count > 0
            //          Vo ova if, prvo proveruvam dali ima koli do  2-ro nivo od FirstCompany, pa ako nema zemam za vozila od listata na alternativni kompanii

            if (lstEligableGisSearchRegions != null && lstEligableGisSearchRegions.Count > 0)
            {
                // ZORAN:   Sega gi selektiram site vozila koi se vo selektiranite regioni
                //          PLUS, soglasno STATE-ot dali se eligable
                //          ISTOVREMENO: Gi mnozam gsr.TypeAlternative so 100000 i gi stavam vo SelectionIndex (tuka mi e vo eden prolaz!!)


                List<Vehicle> tmplstVehiclesAll = VehiclesContainer.Instance.getAllVehicles();

                List<Vehicle> tmplstVehicles = tmplstVehiclesAll;


                foreach (Vehicle veh in tmplstVehicles)
                {
                    foreach (GisSearchRegions gsr in lstEligableGisSearchRegions)
                    {
                        if (veh.CurrentPhoneCall != null && veh.CurrentPhoneCall.oAddressTo != null && veh.CurrentPhoneCall.oAddressTo.oGisRegions != null)
                        {
                            if (VehiclesContainer.Instance.IsVehicleEligableForCall(veh.IdVehicle) && ((veh.currentGPSData.IdRegion == gsr.IdAlternativeregion) || (veh.CurrentPhoneCall.oAddressTo.oGisRegions.IdRegion == gsr.IdAlternativeregion)))
                            {
                                if (!VehicleExistInTList(tmpVehiclesInEligableRegions, veh))
                                {
                                    veh.SelectionIndex = gsr.TypeAlternative * 100000;
                                    tmpVehiclesInEligableRegions.Add(veh);
                                }
                            }
                        }
                        else
                        {
                            if (VehiclesContainer.Instance.IsVehicleEligableForCall(veh.IdVehicle) && veh.currentGPSData.IdRegion == gsr.IdAlternativeregion)
                            {
                                if (!VehicleExistInTList(tmpVehiclesInEligableRegions, veh))
                                {
                                    veh.SelectionIndex = gsr.TypeAlternative * 100000;
                                    tmpVehiclesInEligableRegions.Add(veh);
                                }
                            }
                        }
                    }                    
                }                
            }
            else
            {
                retVal = null;
            }


            TList<Vehicle> tmpVehicles = new TList<Vehicle>();

            // ZORAN:   Prvo dali ima za First Company            
            //foreach (Vehicle tmpVehicle in tmpVehiclesInEligableRegions)
            //{
            //    if (tmpVehicle.IdCompany == pFirstCompany)
            //        tmpVehicles.Add(tmpVehicle);
            //}

            // ZORAN:   Selekcija za koja kompanija se odnesuva odese kako prethodno (se selektirase SAMO zaFirstCompany
            //          PO NOVO:
            //              - Ako e GroupCode == 291 ili 292, se bira samo za FirstCompany (ova e slucaj koga zvonelo nacentrala ili ima generiran povik so 01/02
            //              - Ako e GrouCode == 777, togas e Android i se selektira SAMO za AlternativeCompanies
            //              - Ako e GrouCode == 100, togas e ili rezervacija od Android ili generiran so 03 i se selektira SAMO za AlternativeCompanies
            //
            // VNIMANIE:    Tuka ne stavam tezinski faktor (tmpVehicle.SelectionIndex = tmpVehicle.SelectionIndex + 500000)
            //              na alternativni bidejki tie sega imaa identicen prioritet!!!!

            foreach (Vehicle tmpVehicle in tmpVehiclesInEligableRegions)
            {
              
                if (pPhoneCall.GroupCode.Trim() == "291" || pPhoneCall.GroupCode.Trim() == "292")
                {                    
                    if (tmpVehicle.IdCompany == pFirstCompany)                    
                        tmpVehicles.Add(tmpVehicle);                    
                }

                if (pPhoneCall.GroupCode == "777" || pPhoneCall.GroupCode == "100")
                {
                    foreach (long tmpLong in pAlternativeCompanies)
                        if (tmpVehicle.IdCompany == tmpLong)
                            tmpVehicles.Add(tmpVehicle);
                }
            }
         
            // ZORAN:   Da ne menuvam podole vo kod, pak si vrakam vo tmpVehiclesInEligableRegions

            tmpVehiclesInEligableRegions = tmpVehicles;


            // ZORAN:   Tuka gi imam site eligable vehicles, pa treba da napravam kalulacija i sortiranje
            //          Reonot (nulti, prvi, vtori), mi e prethodno iskalkulirano!

            //PRVO, da go iskalkulirame rastojanieto
            //_________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                double tmpDistance = Calc(pLatitudeY, pLongitudeX, tmpVevicle.currentGPSData.Latutude_Y, tmpVevicle.currentGPSData.Longitude_X);

                tmpVevicle.DistanceToAddress = (long)tmpDistance;

                //ZORAN:    Ovoj IF-ot e staven na 27.04.2016
                //          Idejata e ako e kolata vo maticen, nulti, reon, da ne se zema predvid rastojanieto
                if (tmpVevicle.SelectionIndex > 10000) //nulti e pod 100 000...
                {
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 300 * (int)(tmpDistance / 300);
                }
            }

            //VTORO:    Sega, da zememe predvid dali se dvizi ili ne
            //_________________________________________________________________________________________________


            //foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            //{
            //    if (tmpVevicle.currentGPSData.Speed > 20)
            //    {
            //        tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 50000;
            //    }
            //}


            //TRETO:    Sega, da zememe predvid koj koga isklucil taksimetar
            //          NAPOMENA:   Ako voopsto nema vkluceno taksimetar, ke mu se odzemaat 200 boda (=kako da isklucil pred 200 min) 
            //________________________________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                if (tmpVevicle.TaximetarLast == DateTime.MinValue)
                {
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex - 200;
                }
                else
                {
                    TimeSpan interval = DateTime.Now - tmpVevicle.TaximetarLast;
                    tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex - (int)interval.TotalMinutes;
                }
            }

            //CETVRTO:    Ako voziloto e vo state BUSY, treba soodvetno da se odrabotat, soglasno pBusy parametarot
            //_________________________________________________________________________________________________

            foreach (Vehicle tmpVevicle in tmpVehiclesInEligableRegions)
            {
                if (pIncludeBusy == true)
                {
                    if (tmpVevicle.currentStateString == "StateBusy" &&
                        tmpVevicle.CurrentPhoneCall != null &&
                        tmpVevicle.CurrentPhoneCall.oAddressTo != null &&
                        tmpVevicle.CurrentPhoneCall.oAddressTo.oGisRegions != null &&
                        tmpVevicle.CurrentPhoneCall.oAddressTo.oGisRegions.IdRegion == (long)m_CurrentRegion.IdRegion)
                    {
                        tmpVevicle.SelectionIndex = tmpVevicle.SelectionIndex + 1000000;
                        retVal.Add(tmpVevicle);
                    }
                    else
                    {
                        if (tmpVevicle.currentStateString != "StateBusy")
                            retVal.Add(tmpVevicle);
                    }
                }

                else
                {
                    if (tmpVevicle.currentStateString != "StateBusy")
                        retVal.Add(tmpVevicle);
                }

                

            }

            if (retVal != null)
            {
                retVal.Sort("SelectionIndex");

                //ZORAN:    Tuka malku gimnastika...
                //          Ako prvite N vozila se naogaat na rastojanie od prvoto < 50 metri i ako
                //          se naogaat vo razlicni krugovi (200, 400, 600 ... metri), za toe sto se vo "povisokite" krugovi,
                //          im se odzema po 300 boda (kolku za eden krug), pa prakticno, ako se blisku kolite, 
                //          a se vo razlicni krugovi, ke gi tretira kako da se vo ist krug...

                if (retVal.Count > 1)
                {
                    List<Vehicle> tmpVeh = new List<Vehicle>();

                    //log.Debug("PRED PREREDUVANJE...");

                    for (int n = 0; n <= retVal.Count - 1; n++)
                    {
                        long mMinutes = 0;

                        if (retVal[n].TaximetarLast == DateTime.MinValue)
                            mMinutes = 10000;
                        else
                        {
                            TimeSpan mInterval = DateTime.Now - retVal[n].TaximetarLast;
                            mMinutes = (long)mInterval.TotalMinutes;
                        }

                        //retVal[0].
                        //log.Debug(retVal[n].DescriptionShort + ": Distance:" + retVal[n].DistanceToAddress.ToString() + " / SelectionIndex:" + retVal[n].SelectionIndex + " / MinTaxiMetar:" + mMinutes.ToString() + " / Region:" + retVal[n].currentGPSData.RegionName + " / Speed:" + ((int)retVal[n].currentGPSData.Speed).ToString());
                    }

                    for (int n = 1; n <= retVal.Count-1; n++)
                    {
                        if (Calc(retVal[0].currentGPSData.Latutude_Y, retVal[0].currentGPSData.Longitude_X, retVal[n].currentGPSData.Latutude_Y, retVal[n].currentGPSData.Longitude_X) < 70)
                            tmpVeh.Add (retVal[n]);
                    }

                    

                    //if (tmpVeh.Count > 0)
                    //{
                    //    foreach (Vehicle VehTmp in tmpVeh)
                    //        if (((int)(VehTmp.DistanceToAddress / 200) - (int)(retVal[0].DistanceToAddress / 200)) == 1)
                    //            VehTmp.SelectionIndex = VehTmp.SelectionIndex - 300;

                        retVal.Sort("SelectionIndex");
                    //}                                     
                }


                // I poslednoto, kolku zapisi da vrati...

                if (pNumberOfVehiclesToReturn >= retVal.Count)
                    return retVal;
                else
                {

                    TList<Vehicle> retVal2 = new TList<Vehicle>();

                    for (int i = 0; i < pNumberOfVehiclesToReturn; i++)
                    {
                        retVal2.Add(retVal[i]);
                    }

                    return retVal2;
                }

            }
            else
            {
                return retVal;
            }
        }


        public List<Dictionary<long, byte[]>> CheckForPendingPhoneCalls()
        {
            List<Dictionary<long, byte[]>> retVal = null;
            Dictionary<long, byte[]> tmpRetVal = null;

            if (Math.Abs(System.DateTime.Now.Subtract(dtDateTimeLastCheckedForPendingPhoneCalls).TotalSeconds) > 60)
            {
                dtDateTimeLastCheckedForPendingPhoneCalls = System.DateTime.Now;

                TList<PendingPhoneCalls> lstPendingPhoneCalls = DataRepository.PendingPhoneCallsProvider.GetPending ();


                if (lstPendingPhoneCalls != null && lstPendingPhoneCalls.Count > 0)
                {

                    foreach (PendingPhoneCalls ppc in lstPendingPhoneCalls)
                    {

                        //Prvo, da ja ucitam celata adresa

                        PhoneCalls tmpPhoneCalls = fillPhoneCall(ppc);

                        if (tmpPhoneCalls != null)
                        {
                            tmpRetVal = null;
                            
                            tmpRetVal = VehiclesContainer.Instance.SelectVehiclesforXY((double)ppc.LongitudeX, (double)ppc.LatitudeY, ppc.IncludeBusy, (int)ppc.NumberOfVehiclesToReturn, (long)ppc.IdFirstCompany, getListOfCompanies(ppc.IdAlternativeCompanies), tmpPhoneCalls, (long)ppc.IdUser);

                            if (tmpRetVal != null && tmpRetVal.Count > 0)
                            {
                                if (retVal == null)
                                    retVal = new List<Dictionary<long, byte[]>>();

                                retVal.Add(tmpRetVal);
                            }
                        }

                        //Potoa da update-ram status vo baza.

                        ppc.NumberOfRetries = ppc.NumberOfRetries + 1;
                        ppc.DateTimeLastRetry = System.DateTime.Now;

                        DataRepository.PendingPhoneCallsProvider.Update(ppc);
                    }
                }
            }
            return retVal;
        }



        public void CheckForMissedPendingPhoneCalls()
        {     
            if (Math.Abs(System.DateTime.Now.Subtract(dtDateTimeLastCheckedForMissedPendingPhoneCalls).TotalSeconds) > 60)
            {
                dtDateTimeLastCheckedForMissedPendingPhoneCalls = System.DateTime.Now;

                TList<PendingPhoneCalls> lstMissedPendingPhoneCalls = DataRepository.PendingPhoneCallsProvider.GetMissed();

                if (lstMissedPendingPhoneCalls != null && lstMissedPendingPhoneCalls.Count > 0)
                {
                    SmSsent tmpSmsSent = new SmSsent();

                    foreach(PendingPhoneCalls ppc in lstMissedPendingPhoneCalls)
                    {                                            
                           tmpSmsSent = new SmSsent();

                           //tmpSmsSent.PhoneNumber = "+38978386281"; 
                           tmpSmsSent.PhoneNumber = ppc.ContactPhoneNumber;

                           tmpSmsSent.SmStext = "Se izvinuvame, no ne sme vo moznost da vi obezbedime vozilo. Vi blagodarime za razbiranjeto.";

                           DataRepository.SmSsentProvider.Insert(tmpSmsSent);

                       //Potoa da update-ram status vo baza.

                       ppc.ConfirmedToPassangerAsMissed = true;                       
                       DataRepository.PendingPhoneCallsProvider.Update(ppc);
                    }
                }
            }
            return;
        }



        private PhoneCalls fillPhoneCall(PendingPhoneCalls pPendingPhoneCalls)
        {

            PhoneCalls retVal = null;

            try
            {

                retVal = DataRepository.PhoneCallsProvider.GetByIdPhoneCall(pPendingPhoneCalls.IdPhoneCall);

                log.Debug ("fillPhoneCall, vrateni od GetByIdPhoneCall:     " + retVal.ToString());

                retVal.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();

                if (pPendingPhoneCalls.IdStreetFrom > 0)
                {
                    log.Debug("fillPhoneCall, IdStreetFrom:                  " + ((long)pPendingPhoneCalls.IdStreetFrom).ToString("0000") );

                    GisStreets tmpGisStreets = DataRepository.GisStreetsProvider.GetByIdStreet(((long)pPendingPhoneCalls.IdStreetFrom).ToString("0000"));

                    if (tmpGisStreets != null)
                    {
                        log.Debug("fillPhoneCall, GisStreetsProvider.GetByIdStreet:          " + tmpGisStreets.StreetName);
                        retVal.oAddressFrom.oGisStreets = tmpGisStreets;
                    }
                }
                else
                {
                    log.Debug("fillPhoneCall, IdStreetFrom:  NULL");
                }


                if (pPendingPhoneCalls.IdRegionFrom > 0)
                {
                    GisRegions tmpGisRegions = DataRepository.GisRegionsProvider.GetByIdRegion(pPendingPhoneCalls.IdRegionFrom);

                    if (tmpGisRegions != null)
                    {
                        log.Debug("fillPhoneCall, GisStreetsProvider.GetByIdRegion:          " + tmpGisRegions.RegionName);
                        retVal.oAddressFrom.oGisRegions = tmpGisRegions;
                    }
                }

                if (pPendingPhoneCalls.IdObjectFrom > 0)
                {
                    log.Debug("fillPhoneCall, IdObjectFrom:                  " + pPendingPhoneCalls.IdObjectFrom.ToString());

                    GisObjects tmpGisObjects = DataRepository.GisObjectsProvider.GetByIdObject((long)pPendingPhoneCalls.IdObjectFrom);

                    if (tmpGisObjects != null)
                        retVal.oAddressFrom.oGisObjects = tmpGisObjects;
                }
                else
                {
                    log.Debug("fillPhoneCall, IdObjectFrom:  NULL");
                }

                retVal.oAddressFrom.HouseNumber = int.Parse(pPendingPhoneCalls.HouseNumberFrom);
                retVal.oAddressFrom.LocationQuality = (int)pPendingPhoneCalls.LocationQuality;
                retVal.oAddressFrom.LocationX = (double)pPendingPhoneCalls.LongitudeX;
                retVal.oAddressFrom.LocationY = (double)pPendingPhoneCalls.LatitudeY;
                retVal.oAddressFrom.PickUpAddress = pPendingPhoneCalls.PickUpAddress;
                retVal.oAddressFrom.To = pPendingPhoneCalls.To;
                retVal.oAddressFrom.Comment = pPendingPhoneCalls.Comment;

                //Ova e NEPOTREBNO, no MISLAM deka tuka pagja, ako ne se stavi ova...

                retVal.oAddressTo = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();
                retVal.oAddressTo.oGisRegions = new GisRegions();
                              
            }
            catch (Exception ex)
            {
                log.Error("fillPhoneCall: ", ex);
                retVal = null;
            }
           

            return retVal;   
        }




        // ZORAN:   Ova e procedurata sto ke se koristi za selekcija na vozila po "novo" (licitacijana naracki)
        //          1. Prvo gi bira site koli po stariot kriterium (ne menuvam nisto, posto ne znam, da ne se vratime na "staro"
        //          2. Potoa proveruvam dali nekoja od kolite ima veke ispolneta kvota za broj  na naracki (NumberOfOrdersPerVehicle)
        //          3. Go popolnuvam "OngoingOrdersContainer" so noviot PhoneCall + SelectedVehicles
        //          4. Prakam naracka do site selektirani vozila ("kratkanajava")
        //          5. ...cekam odgovori od vozilata vo "OngoingOrdersContainer", preku decreaseTime
        //          *****************************************************************************************************************
        public  Dictionary<long, byte[]> SelectVehiclesforXY(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn, long pFirstCompany, List<long> pAlternativeCompanies, PhoneCalls pPhoneCall, long pUser)
        {                  

            Dictionary<long, byte[]> retVal = null;

            TList<Vehicle> vehiclesToBeRemoved = new TList<Vehicle>();

            try
            {                                              
                TList<Vehicle> retValSelectedVehicles = SelectVehiclesforXY(pLongitudeX, pLatitudeY, pIncludeBusy, pNumberOfVehiclesToReturn, pFirstCompany, pAlternativeCompanies, pPhoneCall);

                if (retValSelectedVehicles != null && retValSelectedVehicles.Count > 0)
                {
                    //Prvo sort po AvailabilityIndex
                    IComparer<Vehicle> comparer = new MyOrderingClass();
                    retValSelectedVehicles.Sort(comparer);
                   
                    //ZORAN:    Prvo proveruvam nekoe vozilo da ne ima ispolneta kvota za broj  na naracki..
                    //          Potoa gi trgam od selekcijata

                    //ZORAN:    Ova go staviv pokasno:
                    //          - Od selekcijata se trgaat i site vozila kade TypeOfOrder != 2
                    foreach (Vehicle tmpVeh in retValSelectedVehicles)
                    {                        
                        if ((tmpVeh.NumberOfOngoingOrders >= NumberOfOrdersPerVehicle) || (tmpVeh.TypeOfOrder != 2))
                        {
                            vehiclesToBeRemoved.Add(tmpVeh);
                        }
                    }

                    if (vehiclesToBeRemoved.Count > 0)
                        foreach (Vehicle tmpVehToBeRemoved in vehiclesToBeRemoved)
                            retValSelectedVehicles.Remove(tmpVehToBeRemoved);

                    if (retValSelectedVehicles.Count == 0)
                    {
                        InsertIntoDBNullSelectedByPhoneCall(pPhoneCall);
                        _ongoingOrdersContainer.InsertIntoDBSubcontractorPendingPhoneCalls(pPhoneCall);
                        return retVal;
                    }
                    
                    //ZORAN:    Sega da gi prijavam PhoneCall + selectedVehicle vo OngoingOrdersContainer
                    //          Funkcijata treba da mi vrati Dictionary<Vehicle, byte[]> za kratka naracka
                    //          koj sto treba da go vratam na CallBack procedurata (od kade se praka do site vozila)
                    retVal = _ongoingOrdersContainer.insertNewPhoneCall(pPhoneCall, retValSelectedVehicles);                    
                }
                else
                {
                    InsertIntoDBNullSelectedByPhoneCall(pPhoneCall);
                 
                    // ZORAN:   Bidejki nema vozila, insertiram za da se pratat kooperanti
                    //          Ovde samo insert-iram. Vo GPSListener se proveruuva dali vo baza ima neobraboteni povici za kooperanti

                    _ongoingOrdersContainer.InsertIntoDBSubcontractorPendingPhoneCalls (pPhoneCall);
                }
            }
            catch (Exception ex)
            {
                log.Error("ZORAN, selectVehiclesforXY:  ", ex);
            }

            return retVal;
        }



        private void InsertIntoDBNullSelectedByPhoneCall(PhoneCalls pPhoneCall)
        {
            try
            {
                VehiclesNullSelectedByPhoneCall mVehiclesNullSelectedByPhoneCall = new VehiclesNullSelectedByPhoneCall();

                mVehiclesNullSelectedByPhoneCall.IdPhoneCall = pPhoneCall.IdPhoneCall;
                mVehiclesNullSelectedByPhoneCall.PhoneCallRegion = pPhoneCall.oAddressFrom.oGisRegions.IdRegion;
                mVehiclesNullSelectedByPhoneCall.DateTimeForOrder = System.DateTime.Now;
                mVehiclesNullSelectedByPhoneCall.OrderText = _ongoingOrdersContainer.GetAddressFromPhoneCall(pPhoneCall);

                DataRepository.VehiclesNullSelectedByPhoneCallProvider.Insert(mVehiclesNullSelectedByPhoneCall);                
            }
            catch (Exception ex)
            {
                log.Error("InsertIntoDBNullSelectedByPhoneCall", ex);
            }

        }

        

        private void SendOrderForSelectedVehicles(TList<Vehicle> pSelectedVehicles, PhoneCalls pPhoneCall, long pUser)
        {
            //lstOrdersPerVehicles.Add(pPhoneCall.IdPhoneCall, pSelectedVehicles);

            //foreach (Vehicle tmpVehicle in pSelectedVehicles)
            //    SendOrderHandler(tmpVehicle.IdVehicle, pUser, pPhoneCall);
        }
               


        // ZORAN:   Selekcija na kooperant prema XY koordinatite na na povikot, 
        //                  -   Po pPhoneCall.GroupCode se znae za koja kompanija (NaseTaksi ili Lotus) e povikot:
        //                      291=NaseTaksi(ID_Company=1), 292=Lotus (ID_Company=7)
        //                  -   Vehicle.TypeOfOrder = 3 (kooperanti na NaseTaksi)
        //                  -   Vehicle.TypeOfOrder = 4 (----||---- na Lotus)
        //          Kriterium povrzan so plakjanje:
        //              - Prvo se proveruva dali za voziloto ima clenarina za konkretniot mesec
        //              - Vleguva vo selekcija na vozila ako ima clanarina
        //              - Ako nema, se proveruva dali ima seuste predplata za naracki (nezavisno dali ima mesecna predplata).
        //              - Vleguva vo selekcija ako ima predplata 
        //             Ako nema ni mesecna predplata ni za naracki, mu se prakja SMS deka ne e izbran zaradi nemanje sredstva
        // **********************************************************************************************************
        public TList<Vehicle> SelectSubContractorsForXYandPhoneCall(double pLongitudeX, double pLatitudeY, PhoneCalls pPhoneCall, long pUser)
        {

            TList<Vehicle> retVal = new TList<Vehicle>();
            TList<Vehicle> lstSubContractorsInEligableRegions = new TList<Vehicle>();

            try
            {
                //ZORAN:    Prvo daproveram dali ima soodvetni vozila od kooperanti

                List<Vehicle> lstVehiclesAll = VehiclesContainer.Instance.getAllVehicles();
                List<Vehicle> lstVehiclesToBeRemoved = new List<Vehicle>();                

                int mTypeOfOrderForSelection = (pPhoneCall.GroupCode.Trim() == "291") ? 3 : 4;

                foreach (Vehicle Veh in lstVehiclesAll)
                {
                    if (Veh.TypeOfOrder != mTypeOfOrderForSelection)
                        lstVehiclesToBeRemoved.Add(Veh);
                }

                if (lstVehiclesToBeRemoved.Count > 0)
                    foreach (Vehicle veh in lstVehiclesToBeRemoved)
                        lstVehiclesAll.Remove(veh);

                //ZORAN:    Do tuka vo lstVehiclesAll gi imam site vozila za koi treba da proveram dali se vo eligable regions

                if (lstVehiclesAll != null && lstVehiclesAll.Count > 0)
                {
                    GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> retValRegions = null;
                    GlobSaldo.GISDB.Entities.Vregions m_CurrentRegion = null;
                    TList<GisSearchRegions> lstEligableGisSearchRegions = null;

                    retValRegions = GlobSaldo.GISDB.Data.DataRepository.VregionsProvider.uGetRegions((decimal)pLongitudeX, (decimal)pLatitudeY, (long)1);

                    if (retValRegions != null && retValRegions.Count > 0)
                    {
                        m_CurrentRegion = retValRegions[0];

                        lstEligableGisSearchRegions = GlobSaldo.AVL.Data.DataRepository.GisSearchRegionsProvider.GetByIdRegion((long)m_CurrentRegion.IdRegion);

                        if (lstEligableGisSearchRegions != null && lstEligableGisSearchRegions.Count > 0)
                        {
                            // ZORAN:   Sega gi selektiram site vozila koi se vo selektiranite regioni
                            //          VAZNO: Ovde ne se vodi smetka za STATE-ot dali se eligableiline
                            //          ISTOVREMENO: Gi mnozam gsr.TypeAlternative so 100000 i gi stavam vo SelectionIndex (tuka mi e vo eden prolaz!!)

                            //ZORAN:    Tuka prvo iniciram retVal, za da mozam da stavam vo nea vozila
                            retVal = new TList<Vehicle>();

                            foreach (Vehicle veh in lstVehiclesAll)
                            {
                                foreach (GisSearchRegions gsr in lstEligableGisSearchRegions)
                                {

                                    if (veh.currentGPSData.IdRegion == gsr.IdAlternativeregion)
                                    {
                                        if (!VehicleExistInTList(retVal, veh))
                                        {
                                            veh.SelectionIndex = gsr.TypeAlternative * 100000;
                                            retVal.Add(veh);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                //ZORAN:    Sega proveruva dali selektiranite vozila imaat kredit za naracki

                lstVehiclesToBeRemoved.Clear();

                if (retVal != null && retVal.Count > 0)
                {                                        
                    TList<SubcontractorMonthlyFees> lstSubcontractorMonthlyFees = DataRepository.SubcontractorMonthlyFeesProvider.GetAllEligableForSelection();                    

                    bool found = false;

                    if (lstSubcontractorMonthlyFees != null && lstSubcontractorMonthlyFees.Count > 0)
                    {                        

                        foreach (Vehicle veh in retVal)
                        {
                            foreach (SubcontractorMonthlyFees scmf in lstSubcontractorMonthlyFees)
                                if (scmf.IdVehicle == veh.IdVehicle)
                                    found = true;

                            if (!found)
                            {
                                lstVehiclesToBeRemoved.Add(veh);
                                found = false;
                            }
                        }
                    }
                }

                if (lstVehiclesToBeRemoved.Count > 0)
                    foreach (Vehicle veh in lstVehiclesToBeRemoved)
                        retVal.Remove(veh);
            }
            catch (Exception ex)
            {
                log.Error("KOOPERANTI 7: Exception = ", ex);
                retVal = null;
            }
 
         
            if (retVal != null && retVal.Count > 0)
            {
                retVal.Sort("SelectionIndex");                              
            }
            else
            {
                retVal = null;
            }


            return retVal;
        }


        //ZORAN:    Ova e metoda za potvrda koj kooperant bil praten za konkreten povik
        //          Prethodno se evidentirani koi kooperanti bile selektirani za konkretniot povik
        //          Dopolnitelno, tuka mu se brisat i pari na kooperantot!!!
        //          Se izvrsuva od poseben thread..
        public void ProcessSubContractorForPhoneCall(long pIdVehicle, PhoneCalls pPhoneCall, long pIdUser)
        {
            try
            {
                // 1:   Da ja zatvoram otvorenata naracka
                log.Debug("PROCES, ProcessSubContractorForPhoneCall, 1");

                TList<SubcontractorPendingPhoneCalls> lstSubcontractorPendingPhoneCalls = DataRepository.SubcontractorPendingPhoneCallsProvider.GetByIdPhoneCall(pPhoneCall.IdPhoneCall);

                // ZORAN:   TREBA KOREKCIJA, moze da ima poveke SubcontractorPendingPhoneCalls za eden IdPhoneCall!
                //          Ovde gi zatvaram site!
                if (lstSubcontractorPendingPhoneCalls != null && lstSubcontractorPendingPhoneCalls.Count > 0)
                {
                    log.Debug("PROCES, ProcessSubContractorForPhoneCall, lstSubcontractorPendingPhoneCalls.COUNT = " + lstSubcontractorPendingPhoneCalls.Count.ToString());

                    foreach (SubcontractorPendingPhoneCalls ppc in lstSubcontractorPendingPhoneCalls)
                    {
                        ppc.IdVehicle = pIdVehicle;
                        ppc.IdUser = pIdUser;
                        ppc.DateTimeConfirmedByUser = System.DateTime.Now;

                        DataRepository.SubcontractorPendingPhoneCallsProvider.Update(ppc);
                    }
                }
                else
                {
                    log.Debug("ZORAN: Treba da se proveri zosto za IdPhoneCall: " + pPhoneCall.IdPhoneCall.ToString()
                                + " nema zapis vo SubcontractorPendingPhoneCalls !!!");
                }

                // 2:   Sega da minusiram pari/naracki od SubcontractorMonthlyFees (ako seuste ima vo predplatata
                log.Debug("PROCES, ProcessSubContractorForPhoneCall, 2");

                TList<SubcontractorMonthlyFees> lstSubcontractorMonthlyFees = DataRepository.SubcontractorMonthlyFeesProvider.GetByIdVehicle(pIdVehicle);

                //Pravo da vidam dali ima clanarina za tekovniot period + dali ima neiskoristeni naracki
                SubcontractorMonthlyFees mCurrentSubcontractorMonthlyFees = null;

                if (lstSubcontractorMonthlyFees != null && lstSubcontractorMonthlyFees.Count > 0)
                {                    

                    foreach (SubcontractorMonthlyFees smf in lstSubcontractorMonthlyFees)
                        if (smf.DateFrom <= System.DateTime.Now && smf.DateTo >= System.DateTime.Now && smf.IncludedFreeOrders - smf.SpentFreeOrders > 0)
                            mCurrentSubcontractorMonthlyFees = smf;
                }

                if (mCurrentSubcontractorMonthlyFees != null)
                {
                    log.Debug("PROCES, ProcessSubContractorForPhoneCall, lstSubcontractorMonthlyFees.COUNT = " + lstSubcontractorMonthlyFees.Count.ToString());

                    mCurrentSubcontractorMonthlyFees.SpentFreeOrders = mCurrentSubcontractorMonthlyFees.SpentFreeOrders + 1;
                    DataRepository.SubcontractorMonthlyFeesProvider.Update(mCurrentSubcontractorMonthlyFees);
                }

                else             // 3:   Sega, ako ne e minusirana narackata vo SubcontractorMonthlyFees, znaci treba da se minusira vo SubcontractorPrepaidFees            
                {
                    log.Debug("PROCES, ProcessSubContractorForPhoneCall, lstSubcontractorMonthlyFees.COUNT = NEMA ili NULL ");

                    SubcontractorPrepaidFees mCurrentSubcontractorPrepaidFees = null;

                    TList<SubcontractorPrepaidFees> lstSubcontractorPrepaidFees = DataRepository.SubcontractorPrepaidFeesProvider.GetByIdVehicle(pIdVehicle);

                    if (lstSubcontractorPrepaidFees != null && lstSubcontractorPrepaidFees.Count > 0)
                    {
                        foreach (SubcontractorPrepaidFees sppf in lstSubcontractorPrepaidFees)
                            if (sppf.IncludedFreeOrders - sppf.SpentFreeOrders > 0)
                                mCurrentSubcontractorPrepaidFees = sppf;
                    }

                    if (mCurrentSubcontractorPrepaidFees != null)
                    {
                        log.Debug("PROCES, ProcessSubContractorForPhoneCall, mCurrentSubcontractorPrepaidFees.COUNT = " + lstSubcontractorMonthlyFees.Count.ToString());

                        mCurrentSubcontractorPrepaidFees.SpentFreeOrders = mCurrentSubcontractorPrepaidFees.SpentFreeOrders + 1;
                        DataRepository.SubcontractorPrepaidFeesProvider.Update(mCurrentSubcontractorPrepaidFees);
                    }
                    else
                    {
                        log.Debug("PROCES, ProcessSubContractorForPhoneCall, mCurrentSubcontractorPrepaidFees.COUNT = NEMA ili NULL ");
                    }
                }

                // 4:   Prakam SMS na site GSM koi se prijaveni za voziloto

                TList<SubcontractorGsmNumbers> lstSubcontractorGsmNumbers = DataRepository.SubcontractorGsmNumbersProvider.GetByIdVehicle(pIdVehicle);

                if (lstSubcontractorGsmNumbers != null && lstSubcontractorGsmNumbers.Count > 0)
                {
                    log.Debug("PROCES, ProcessSubContractorForPhoneCall, lstSubcontractorGsmNumbers.COUNT = " + lstSubcontractorGsmNumbers.Count.ToString());

                    foreach (SubcontractorGsmNumbers sgn in lstSubcontractorGsmNumbers)
                    {
                        if (sgn.GsmNumber != null && sgn.GsmNumber.Trim() != "")
                        {
                            string tmpStr = "";
                            SmSsent mSmSsent = new SmSsent();

                            if (pPhoneCall.oAddressFrom.oGisStreets != null)
                            {
                                tmpStr += "Br: " + pPhoneCall.oAddressFrom.HouseNumber.ToString() + ", ";
                                tmpStr += pPhoneCall.oAddressFrom.oGisStreets.StreetName.Trim();
                            }

                            if (pPhoneCall.oAddressFrom.oGisObjects != null)
                                tmpStr += pPhoneCall.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

                            if (pPhoneCall.oAddressFrom.oGisRegions != null)
                                tmpStr += ", R: " + pPhoneCall.oAddressFrom.oGisRegions.IdRegion.ToString().Trim();

                            if (pPhoneCall.oAddressFrom.PickUpAddress != null && pPhoneCall.oAddressFrom.PickUpAddress.Trim() != "")
                                tmpStr += ", " + pPhoneCall.oAddressFrom.PickUpAddress.Trim();

                            if (pPhoneCall.oAddressFrom.To != null && pPhoneCall.oAddressFrom.To.Trim() != "")
                                tmpStr += ", Do: " + pPhoneCall.oAddressFrom.To.Trim();

                            if (pPhoneCall.oAddressFrom.Comment != null && pPhoneCall.oAddressFrom.Comment.Trim() != "")
                                tmpStr += ", K: " + pPhoneCall.oAddressFrom.Comment.Trim();

                            tmpStr = UnicodeStrings.UncodeToAscii(tmpStr);

                            if (tmpStr.Length > 160)
                                tmpStr = tmpStr.Substring(0, 159);

                            mSmSsent.PhoneNumber = sgn.GsmNumber;
                            mSmSsent.SmStext = tmpStr;
                            mSmSsent.Comment = "Kooperant, IdVehicle = " + pIdVehicle.ToString();

                            DataRepository.SmSsentProvider.Insert(mSmSsent);
                        }
                    }                   
                }
                else
                {
                    log.Debug("PROCES, ProcessSubContractorForPhoneCall, lstSubcontractorGsmNumbers.COUNT = NEMA ili NULL ");
                }

                // 5:   Kako posledno, da update-ram sostojbata na voziloto, za da moze da se sledi na koja adresa odi
                //      PROBELM: Prasanje e kako da se trgne adresata, posto vozilo na kooperant nemenuva state!!!!
                _lstByID_Vehicle[pIdVehicle].lastPhoneCallAccepted = new PhoneCallAccepted();

                _lstByID_Vehicle[pIdVehicle].lastPhoneCallAccepted.IdPhoneCall = pPhoneCall.IdPhoneCall;
                _lstByID_Vehicle[pIdVehicle].lastPhoneCallAccepted.Minuti = 10;

                _lstByID_Vehicle[pIdVehicle].CurrentPhoneCall = pPhoneCall;
                _lstByID_Vehicle[pIdVehicle].DirtyStateChange = true;
            }
            catch (Exception ex)
            {
                log.Error("ProcessSubContractorForPhoneCall: ", ex);
            }

            return;
        }



        // ZORAN:   Ova e procedura od Pajo
        //          Siguren sum deka ne treba tuka, no posle ke gledame, da ne ja mislam sega!!!        

        public static double Calc(double Lat1, double Long1, double Lat2, double Long2)
        {
            double dDistance = Double.MinValue;
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            // Distance.
            // const Double kEarthRadiusMiles = 3956.0;
            const Double kEarthRadiusKms = 6376.5;
            dDistance = kEarthRadiusKms * c;

            // ZORAN:   Ovde MISLAM deka vrakase vo km
            //          Gi prefrlam vo metri
            return dDistance * 1000;
        }


        private class MyOrderingClass : IComparer<Vehicle>
        {
            public int Compare(Vehicle x, Vehicle y)
            {
                int compareShortDescr = x.SelectionIndex.CompareTo(y.SelectionIndex);

                if (compareShortDescr == 0)
                {
                    return x.TaximetarLast.CompareTo(y.TaximetarLast);
                }
                return compareShortDescr;
            }
        }


        private bool VehicleExistInTList(TList<Vehicle> pVehicleList, Vehicle pVhicle)
        {
            bool retVal = false;

            foreach (Vehicle tmpV in pVehicleList)
            {
                if (tmpV.IdVehicle == pVhicle.IdVehicle)
                    retVal = true;
            }

            return retVal;
        }


        private List<long> getListOfCompanies(string pListOfCompanies)
        {
            List<long> retVal = new List<long>();

            char[] delimiterChars = { ',' };

            string[] stringIdCompany = pListOfCompanies.Split(delimiterChars);

            if (stringIdCompany != null && stringIdCompany.GetLength(0) > 0)
            {
                foreach (string tmpStr in stringIdCompany)
                    if (tmpStr != "")
                        retVal.Add(long.Parse(tmpStr));
            }

            return retVal;
        }

        #endregion
    }

}
