using System;
using System.Collections.Generic;
using System.Text;

using System.Configuration;
using log4net;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.Utils.Classes;
using Taxi.Communication.Server.Utils.Containers;
using Taxi.Communication.Server.Utils.Parsers;

using System.Collections.Specialized;

namespace Taxi.Communication.Server.Utils.Containers
{
    class OngoingOrdersContainer
    {
        private ILog log;
        
        private int SecondsToWaitOrderConfirmation;

        private Dictionary<long, TList<Vehicle>> _dictVehicleByPhoneCall;
        //private OrderedDictionary _dictVehicleByPhoneCall;

        private Dictionary<long, PhoneCalls> _dictPhoneCallsByIdPhoneCall;
        private Dictionary<long, long> _dictTimePerPhoneCall;
        private Dictionary<long, List<clsPotvrdaNaNaracka>> _dictOrdersConfirmations;
        private clsMessageCreator tMessageCreator = null;
 
        

        public OngoingOrdersContainer()
        {
            log = LogManager.GetLogger("MyService");            
            
            SecondsToWaitOrderConfirmation = int.Parse(ConfigurationManager.AppSettings["SecondsToWaitOrderConfirmation"]);

            _dictVehicleByPhoneCall = new Dictionary<long, TList<Vehicle>>();
            //_dictVehicleByPhoneCall = new OrderedDictionary();

            _dictPhoneCallsByIdPhoneCall = new Dictionary<long, PhoneCalls>();
            _dictTimePerPhoneCall = new Dictionary<long, long>();
            _dictOrdersConfirmations = new Dictionary<long, List<clsPotvrdaNaNaracka>>();
            tMessageCreator = new clsMessageCreator();
        }


        //Zoran:    Tuka se insertira nov PhoneCall + selektirani vozila za nego.
        //          1. Dokolku veke postoi toj PhoneCall vo _dictVehicleByPhoneCall ili vo _dictTimePerPhoneCall
        //             go brse! Toa mora da e greska, plus odi vo log file za da se proveri sto e makata!
        //          2. Go insertira PhoneCall + Listata na vozila vo _dictVehicleByPhoneCall
        //          3. Go insertira PhoneCall + vremeto koe treba da pomine dodeka da pukne time out (cita od App.Config)

        public Dictionary<long, byte[]> insertNewPhoneCall(PhoneCalls pPhoneCall, TList<Vehicle> pSelectedVehicles)
        {

            InsertIntoDBSelectedVehiclesByPhoneCall(pPhoneCall, pSelectedVehicles);

            Dictionary<long, byte[]> retVal = new Dictionary<long,byte[]>();

            cleanDictionariesForPhoneCall(pPhoneCall.IdPhoneCall);

            _dictVehicleByPhoneCall.Add(pPhoneCall.IdPhoneCall, pSelectedVehicles);
            //_dictVehicleByPhoneCall.Add(pPhoneCall.IdPhoneCall, pSelectedVehicles);

            _dictTimePerPhoneCall.Add(pPhoneCall.IdPhoneCall, SecondsToWaitOrderConfirmation);
            _dictPhoneCallsByIdPhoneCall.Add(pPhoneCall.IdPhoneCall, pPhoneCall);

            //ZORAN:    Tuka treba da generiram kratka poraka za site selektirani vozila...
            //          PLUS, da go zgolemam brojot na prateni naracki do vozilo

            byte[] tmpByte = null;

            foreach (Vehicle tmpVeh in pSelectedVehicles)
            {
                tmpByte = tMessageCreator.OrderCreate(tmpVeh, pPhoneCall);

                if (tmpByte != null)
                {
                    retVal.Add(tmpVeh.IdVehicle, tmpByte);
                    //.....Tuka da povikam metoda na vehcle container za increase na NumberOfOngoingOrders
                    
                }
            }

            if (retVal.Count == 0)
                retVal = null;

            return retVal;
        }




        //ZORAN:    Ovde se namaluva vremeto za sekoj tekovon PhoneCall
        //          Ako za nekoj PhoneCall se stigne do time out, togas toj povik se zatvara i se prakaat soodvetni poraki do vozilata
        //          Pri toa:
        //              - Se vraka Dictionary<Vehice, byte[]>
        //              - Za prvo-selektranoto vozilo se praka potvrda za naracka
        //              - Za ostanatite info deka drugo vozilo e selektirano
        //              - Vo eden povik moze da se pratat poraki za poveke PhoneCalls (site na koi im isteknalo vremeto)
        public List<Dictionary<long, byte[]>> decreaseTimeForOrders()
        {
            List<Dictionary<long, byte[]>> retVal = new List<Dictionary<long, byte[]>>();
            List<long> PhoneCallsWithTimeOut = new List<long>();

            //ZORAN:    Prvo, da ja namalam za 1 sekoja vrednost
            //          No, istovremeno popolnuval lista na PhoneCalls kade im puknal TimeOut
            List<KeyValuePair<long, long>> tmpList = new List<KeyValuePair<long, long>>(_dictTimePerPhoneCall);

            foreach (KeyValuePair<long, long> kvp in tmpList)
            {                
                _dictTimePerPhoneCall[kvp.Key] = _dictTimePerPhoneCall[kvp.Key] - 1;

                if(_dictTimePerPhoneCall[kvp.Key] == 0)
                    PhoneCallsWithTimeOut.Add(kvp.Key);
            }

            //ZORAN:    Vtoro, da gi odrabotam site PhoneCalls za koi pominalo vremeto
            //          Prvo go zemam toj PhoneCall, zaedno so vozilata i go trgam od Dictionary
            //          Proveruvam dali za niv ima odgovori od soferi
            //          Ako nema, NEKAKO treba da pratam info do DISPECER ili ANDROID, NE ZNAM KAKO VO MOMENTOV!
            //          Potoa, go trgam selektiranoto vozilo od site Dictionary, nema sto da ceka poveke
            //          Potoa:
            //              1. Generiram poraka za selektranoto vozilo
            //              2. Generiram identicna poraka za site drugi vozila za toj PhoneCall
            //              3. Vrakam gotovi poraki na GPSListener, kako dictionary, za da preprati na vozilata
            //NAPOMENA: Ova se pravi odednas za SITE PhoneCall za koi im e puknat TimeOut
                        
            if (PhoneCallsWithTimeOut != null && PhoneCallsWithTimeOut.Count > 0)
            {
                Dictionary<long, byte[]> tmpRetVal = new Dictionary<long, byte[]>(); 

                foreach (long tmpIdPhoneCall in PhoneCallsWithTimeOut)
                {
                    //log.Debug("Prateno na ProcessPhoneCall za IdPhoneCall: " + tmpIdPhoneCall.ToString());

                    tmpRetVal = ProcessPhoneCall(tmpIdPhoneCall);
                    
                    try
                    {
                        if (tmpRetVal != null && tmpRetVal.Count > 0)
                        {
                            retVal.Add(tmpRetVal);                           
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }                   
                }               
            }
            
            return retVal;
        }



        private Dictionary<long, byte[]> ProcessPhoneCall(long pIdPhoneCall)
        {            
            Dictionary<long, byte[]> retVal = new Dictionary<long, byte[]>();

            TList<Vehicle> lstSeletedVehicles = null;
            TList<Vehicle> lstSelectedVehiclesStoNeLicitirale = new TList<Vehicle>();
            List<clsPotvrdaNaNaracka> lstOrderConfirmations = null;
            PhoneCalls mPhoneCalls = null;

            try
            {                
                if (_dictVehicleByPhoneCall.ContainsKey(pIdPhoneCall)
                            && _dictOrdersConfirmations.ContainsKey(pIdPhoneCall)
                            && _dictPhoneCallsByIdPhoneCall.ContainsKey(pIdPhoneCall))
                {
                    
                    //Prvo gi zemam vrednostite od Dictionaries
                    lstSeletedVehicles = _dictVehicleByPhoneCall[pIdPhoneCall];
                    lstOrderConfirmations = _dictOrdersConfirmations[pIdPhoneCall];
                    mPhoneCalls = _dictPhoneCallsByIdPhoneCall[pIdPhoneCall];

                    //Potoa gi brisam tie zapisi od Dictionaries
                    _dictVehicleByPhoneCall.Remove(pIdPhoneCall);
                    _dictOrdersConfirmations.Remove(pIdPhoneCall);
                    _dictPhoneCallsByIdPhoneCall.Remove(pIdPhoneCall);
                    _dictTimePerPhoneCall.Remove(pIdPhoneCall);

                    //Sega proveruvam da ne se raboti za prazni listi (ne moze da e null)
                    if (lstSeletedVehicles.Count > 0 && lstOrderConfirmations.Count > 0)
                    {
                        TList<Vehicle> lstVehiclesToBeRemoved = new TList<Vehicle>();
                        bool found = false;

                        //sega da gi trgnam vozilata od kade nema odgovor za narackata
                        foreach (Vehicle veh in lstSeletedVehicles)
                        {
                            foreach (clsPotvrdaNaNaracka pnn in lstOrderConfirmations)
                                if (veh.IdVehicle == pnn.IdVehicle)
                                    found = true;

                            if (!found)
                            {
                                lstSelectedVehiclesStoNeLicitirale.Add(veh);
                            }

                            found = false;
                        }

                        if (lstSelectedVehiclesStoNeLicitirale.Count > 0)
                            foreach (Vehicle veh in lstSelectedVehiclesStoNeLicitirale)
                                lstSeletedVehicles.Remove(veh);

                        //Sega vo lstSeletedVehicles gi imam samo vozilata od koi imam odgovor (mozno e Count=0!!!)
                        //Prvoto e toa sto ke odi, a na site drugi im se otkazuva
                        //TREBA DA SE NAPRAVI:
                        //  1. Da se proveri sostojbata na prvo-selektiranoto, da ne smenilo STATE vo meguvreme!
                        //  2. Ako smenilo se odi na sledno, pa na sledno....
                        //  3. Se trga prvo-selektiranoto od site dictionaries, nema vise sto da ceka!

                        Vehicle prvoSelektiranoVehicle = null;
                        int BrojNaSelektiraniVozila = lstSeletedVehicles.Count;


                        while (BrojNaSelektiraniVozila > 0 && prvoSelektiranoVehicle == null)
                        {
                            prvoSelektiranoVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(lstSeletedVehicles[0].IdVehicle);

                            //log.Debug("PRVICNO prvo-selektirano vozilo, za ID_PhoneCall: " + pIdPhoneCall.ToString() + " e: " + prvoSelektiranoVehicle.Plate);

                            if (prvoSelektiranoVehicle.currentStateString != "StateIdle")
                            {
                                lstSeletedVehicles.Remove(prvoSelektiranoVehicle);
                                prvoSelektiranoVehicle = null;
                                BrojNaSelektiraniVozila--;
                            }
                        }

                        if (prvoSelektiranoVehicle != null)
                        {
                            //log.Debug("DEFINITIVNO prvo-selektirano vozilo, za ID_PhoneCall: " + pIdPhoneCall.ToString() + " e: " + prvoSelektiranoVehicle.Plate);

                            cleanDictionariesForSelectedVehicle(prvoSelektiranoVehicle);

                            lstSeletedVehicles.Remove(prvoSelektiranoVehicle);                            

                            //ZORAN:    Prvo vo listata vrakam naracka za prvo selektiranoto

                            retVal.Add(prvoSelektiranoVehicle.IdVehicle, tMessageCreator.OrderCreateConfirm(prvoSelektiranoVehicle, mPhoneCalls));

                            //ZORAN:    Sega, prakam SEKAKO SMS, ako brojot e mobilen
                            //         
                            SendSMSforSelectedVehicleAndPhoneCall(mPhoneCalls, prvoSelektiranoVehicle, GetTimeForOrderConfirmation(prvoSelektiranoVehicle.IdVehicle, lstOrderConfirmations));

                            //ZORAN:    Potoa, proveruvam da ne bil toa pendingPhoneCall, pa da si azuriramsoodvetno
                            UpdatePendingPhoneCall(pIdPhoneCall);                           

                            //ZORAN:    Sega za ID_User ke stavam -1 (Android), iako ne se tretira ovaa informacija za ponatamu
                            //          Treba ova da se smeni...

                            clsPotvrdaNaNaracka mPotvrdaNaNaracka = new clsPotvrdaNaNaracka();
                            
                            foreach (clsPotvrdaNaNaracka pTmp in lstOrderConfirmations)
                                if (pTmp.IdVehicle == prvoSelektiranoVehicle.IdVehicle)
                                    mPotvrdaNaNaracka = pTmp;

                            long intRetVal = VehiclesContainer.Instance.SetStateForSelectedVehicle (prvoSelektiranoVehicle.IdVehicle, mPhoneCalls, mPotvrdaNaNaracka.Minuti, -1);

                            InsertOrder(mPhoneCalls, prvoSelektiranoVehicle, lstSeletedVehicles);

                            double DistanceForFirstSelected = CalculateDistance(prvoSelektiranoVehicle.currentGPSData.Longitude_X, prvoSelektiranoVehicle.currentGPSData.Latutude_Y, mPhoneCalls.oAddressFrom.LocationX, mPhoneCalls.oAddressFrom.LocationY);

                            //ZORAN:     Sega, dokolku ima drugi za otkazuvanje, im se praka niv info za otkazuvanje

                            if (lstSeletedVehicles != null && lstSeletedVehicles.Count > 0)
                            {
                                foreach (Vehicle veh in lstSeletedVehicles)
                                {
                                    //string tmp = "(" + mPhoneCalls.IdPhoneCall.ToString() + ")" + "Prateno: " + prvoSelektiranoVehicle.DescriptionLong + "(" + DistanceForFirstSelected.ToString("#####m") + ")";                                  

                                    string tmp = "Prateno: " + prvoSelektiranoVehicle.DescriptionLong + " (" + prvoSelektiranoVehicle.SelectionIndex.ToString() + " / " + veh.SelectionIndex.ToString() + ")";

                                    retVal.Add(veh.IdVehicle, tMessageCreator.OrderCreateCancel(veh, mPhoneCalls.IdPhoneCall, tmp)); 

                                    lstSelectedVehiclesStoNeLicitirale.Remove(veh);
                                }
                            }

                            if (lstSelectedVehiclesStoNeLicitirale != null && lstSelectedVehiclesStoNeLicitirale.Count > 0)
                            {
                                foreach (Vehicle veh in lstSelectedVehiclesStoNeLicitirale)
                                {
                                    retVal.Add(veh.IdVehicle, tMessageCreator.OrderCreateCancel(veh, mPhoneCalls.IdPhoneCall, "(" + mPhoneCalls.IdPhoneCall.ToString() + ")" + "Prateno e vozilo: " + prvoSelektiranoVehicle.Plate));
                                }
                            }

                        }
                        else
                        {
                            //ZORAN:    Prvo stavam deka treba da se bara vozilo od SubContractors, pa potoa gi informiram site deka nema prateno vozilo
                            InsertIntoDBSubcontractorPendingPhoneCalls(mPhoneCalls);

                            foreach (Vehicle veh in lstSelectedVehiclesStoNeLicitirale)
                            {
                                retVal.Add(veh.IdVehicle, tMessageCreator.OrderCreateCancel(veh, mPhoneCalls.IdPhoneCall, "Ne e prateno nitu edno vozilo..."));
                            }
                        }
                    }
                    else
                    {
                        //ZORAN:    Prvo stavam deka treba da se bara vozilo od SubContractors, pa potoa gi informiram site deka nema prateno vozilo
                        InsertIntoDBSubcontractorPendingPhoneCalls(mPhoneCalls);

                        foreach (Vehicle veh in lstSelectedVehiclesStoNeLicitirale)
                        {
                            retVal.Add(veh.IdVehicle, tMessageCreator.OrderCreateCancel(veh, mPhoneCalls.IdPhoneCall, "(" + mPhoneCalls.IdPhoneCall.ToString() + ")" + "Ne e prateno nitu edno vozilo..."));
                        }
                    }
                }
                else
                {                                       
                    if (_dictVehicleByPhoneCall.ContainsKey(pIdPhoneCall))
                    //if (_dictVehicleByPhoneCall.Contains(pIdPhoneCall))
                    {
                        lstSelectedVehiclesStoNeLicitirale = (TList<Vehicle>)_dictVehicleByPhoneCall[pIdPhoneCall];
                        _dictVehicleByPhoneCall.Remove(pIdPhoneCall);
                    }

                    if (_dictOrdersConfirmations.ContainsKey(pIdPhoneCall))
                    {
                        _dictOrdersConfirmations.Remove(pIdPhoneCall);
                    }

                    if (_dictPhoneCallsByIdPhoneCall.ContainsKey(pIdPhoneCall))
                    {
                        InsertIntoDBSubcontractorPendingPhoneCalls(_dictPhoneCallsByIdPhoneCall[pIdPhoneCall] );
                        _dictPhoneCallsByIdPhoneCall.Remove(pIdPhoneCall);
                    }

                    foreach (Vehicle veh in lstSelectedVehiclesStoNeLicitirale)
                    {
                        //log.Debug(veh.IdVehicle.ToString() +  " / " + pIdPhoneCall.ToString());

                        retVal.Add(veh.IdVehicle, tMessageCreator.OrderCreateCancel(veh, pIdPhoneCall, "(" + pIdPhoneCall.ToString() + ")" + "Ne e prateno nitu edno vozilo..."));
                    }                    
                }
            }
            catch (Exception ex)
            {
                if(mPhoneCalls != null)
                    InsertIntoDBSubcontractorPendingPhoneCalls(mPhoneCalls);

                retVal = null;
                log.Error(ex);
            }

            return retVal;
        }

      

        public void addNewOrderConfirmation(clsPotvrdaNaNaracka pPotvrdaNaNaracka)
        {
            //ZORAN:    Prvo proveruvam dali postoi aktiven PhoneCall (vo _dictVehicleByPhoneCall)
            //          bidejki mozebi e zakasnet odgovor, pa go ignoriram (ova ne e dobro, no ke se slucuva...MArjan cisti na strana na ured)           
            
            bool mZakasnetaNaracka = true;           

            foreach (KeyValuePair<long, TList<Vehicle>> kvp in _dictVehicleByPhoneCall)           
            {
                if (kvp.Key == pPotvrdaNaNaracka.IdPhoneCall)
                {
                    mZakasnetaNaracka = false;

                    if (_dictOrdersConfirmations.ContainsKey(pPotvrdaNaNaracka.IdPhoneCall))
                    {
                        _dictOrdersConfirmations[pPotvrdaNaNaracka.IdPhoneCall].Add(pPotvrdaNaNaracka);
                    }
                    else
                    {
                        _dictOrdersConfirmations.Add(pPotvrdaNaNaracka.IdPhoneCall, new List<clsPotvrdaNaNaracka>());
                        _dictOrdersConfirmations[pPotvrdaNaNaracka.IdPhoneCall].Add(pPotvrdaNaNaracka);
                    }
                }
            }

            InsertIntoDBOrderConfirmationsByPhoneCall(pPotvrdaNaNaracka, mZakasnetaNaracka);
        }


        private void cleanDictionariesForPhoneCall(long pIdPhoneCall)
        {

            if (_dictVehicleByPhoneCall.ContainsKey(pIdPhoneCall))    // Ova ne treba da se sluci!!!
            //if (_dictVehicleByPhoneCall.Contains(pIdPhoneCall))    // Ova ne treba da se sluci!!!
            {
                log.Error("Vo insertNewPhoneCall veke postoel IdPhoneCall (_dictVehicleByPhoneCall): " + pIdPhoneCall.ToString());
                _dictVehicleByPhoneCall.Remove(pIdPhoneCall);
            }

            if (_dictTimePerPhoneCall.ContainsKey(pIdPhoneCall))    // Ova ne treba da se sluci!!!
            {
                log.Error("Vo insertNewPhoneCall veke postoel IdPhoneCall (_dictTimePerPhoneCall): " + pIdPhoneCall.ToString());
                _dictTimePerPhoneCall.Remove(pIdPhoneCall);
            }
         
            if (_dictPhoneCallsByIdPhoneCall.ContainsKey(pIdPhoneCall))    // Ova ne treba da se sluci!!!
            {
                log.Error("Vo insertNewPhoneCall veke postoel IdPhoneCall (_dictPhoneCallsByIdPhoneCall): " + pIdPhoneCall.ToString());
                _dictPhoneCallsByIdPhoneCall.Remove(pIdPhoneCall);
            }

            if (_dictOrdersConfirmations.ContainsKey(pIdPhoneCall))    // Ova ne treba da se sluci!!!
            {
                log.Error("Vo insertNewPhoneCall veke postoel IdPhoneCall (_dictOrdersConfirmations): " + pIdPhoneCall.ToString());
                _dictOrdersConfirmations.Remove(pIdPhoneCall);
            }            
        }

        private void cleanDictionariesForSelectedVehicle(Vehicle pVehicle)
        {                     

            Vehicle tmpVehicle = null;
            TList<Vehicle> tmpLstVeh = null;

            foreach (KeyValuePair<long, TList<Vehicle>> kvp in _dictVehicleByPhoneCall)
            {
                tmpVehicle = null;

                try
                {                    

                    tmpLstVeh = (TList<Vehicle>)_dictVehicleByPhoneCall[kvp.Key];

                    foreach (Vehicle PnN in tmpLstVeh)
                        if (PnN.IdVehicle == pVehicle.IdVehicle)
                            tmpVehicle = PnN;

                    if (tmpVehicle != null)
                    {
                        //log.Debug("Trgnato vozilo od _dictVehicleByPhoneCall: " + tmpVehicle.Plate);
                        _dictVehicleByPhoneCall[kvp.Key].Remove(tmpVehicle);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("cleanDictionariesForSelectedVehicle, greska za _dictVehicleByPhoneCall", ex);
                }
            }                        

             

            clsPotvrdaNaNaracka tmpPnN = null;
            List<clsPotvrdaNaNaracka> tmpLstPnN = null;

            foreach (KeyValuePair<long, List<clsPotvrdaNaNaracka>> kvp in _dictOrdersConfirmations)
            {
                tmpPnN = null;

                try
                {
                    tmpLstPnN = _dictOrdersConfirmations[kvp.Key];

                    foreach (clsPotvrdaNaNaracka PnN in tmpLstPnN)
                        if (PnN.IdVehicle == pVehicle.IdVehicle)
                            tmpPnN = PnN;

                    if (tmpPnN != null)
                    {
                        //log.Debug("Trgnata potvrda od _dictOrdersConfirmations za vozilo: " + pVehicle.Plate);
                        _dictOrdersConfirmations[kvp.Key].Remove(tmpPnN);
                    }
                }
                catch (Exception ex)
                {
                    log.Error("_dictOrdersConfirmations, greska za _dictVehicleByPhoneCall", ex);
                }
            }               
        }


        private void InsertOrder(PhoneCalls pPhoneCalls, Vehicle pVehicle, TList<Vehicle> pSeletedVehicles)
        {

            Orders mOrders = new Orders();

            try
            {
                mOrders.IdVehicle = pVehicle.IdVehicle;
                mOrders.IdPhoneCall = pPhoneCalls.IdPhoneCall;

                if (pPhoneCalls.oAddressFrom != null)
                {
                   
                    mOrders.LongitudeX = pPhoneCalls.oAddressFrom.LocationX;
                    mOrders.LatitudeY = pPhoneCalls.oAddressFrom.LocationY;
                    mOrders.HouseNumberFrom = pPhoneCalls.oAddressFrom.HouseNumber.ToString();    //Ova, najcesto = 0 (duri i vo slucaj na Object)    

                    if (pPhoneCalls.oAddressFrom.oGisRegions != null)
                        mOrders.IdRegionFrom = (long)pPhoneCalls.oAddressFrom.oGisRegions.IdRegion;

                    if (pPhoneCalls.oAddressFrom.oGisStreets != null)
                        mOrders.IdAddressFrom = long.Parse(pPhoneCalls.oAddressFrom.oGisStreets.IdStreet);                                     

                    if (pPhoneCalls.oAddressFrom.oGisObjects != null)
                    {
                        mOrders.IdObjectFrom = pPhoneCalls.oAddressFrom.oGisObjects.IdObject;
                    }


                    if (pPhoneCalls.IdUserInOut != 0)
                    {
                        UserInOut tmpUserInOut = GlobSaldo.AVL.Data.DataRepository.UserInOutProvider.GetByIdUserLogInOut((long)pPhoneCalls.IdUserInOut);

                        if (tmpUserInOut != null)
                        {
                            mOrders.IdUser = tmpUserInOut.IdUser;
                        }
                    }
                    else
                        mOrders.IdUser = -1;


                    string tmpStr = "";

                    if (pPhoneCalls.oAddressFrom.oGisRegions != null)
                        tmpStr += "REON: " + pPhoneCalls.oAddressFrom.oGisRegions.IdRegion.ToString().Trim() + " // ";

                    if (pPhoneCalls.oAddressFrom.oGisStreets != null)
                    {
                        tmpStr += pPhoneCalls.oAddressFrom.oGisStreets.StreetName.Trim() + " ";

                        if (pPhoneCalls.oAddressFrom.oGisStreets.StreetName.Substring(0, 2) != "A:")
                            tmpStr += pPhoneCalls.oAddressFrom.HouseNumber + " ";
                    }

                    if (pPhoneCalls.oAddressFrom.oGisObjects != null)
                        tmpStr += pPhoneCalls.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

                    mOrders.AddressMessage = tmpStr;
                    mOrders.CommentAddressTo = pPhoneCalls.oAddressFrom.Comment.Trim();


                    if (pSeletedVehicles != null && pSeletedVehicles.Count > 0)
                    {
                        tmpStr = "Индекс: " + pVehicle.SelectionIndex.ToString() + "|  ";

                        foreach (Vehicle tmpVeh in pSeletedVehicles)
                        {
                            tmpStr += tmpVeh.Plate + "  |" + tmpVeh.SelectionIndex.ToString() + "| " + " R:" + tmpVeh.currentGPSData.IdRegion.ToString() + ", "; 
                        }

                        mOrders.CommentGeneral = tmpStr;
                    }

                    mOrders.SystemTime = System.DateTime.Now;

                    DataRepository.OrdersProvider.Insert(mOrders);
                }
            }            
            
            catch (Exception ex)
            {
                log.Error("InsertOrder: ", ex);
            }
        }



        private void InsertIntoDBSelectedVehiclesByPhoneCall(PhoneCalls pPhoneCall, TList<Vehicle> pSelectedVehicles)
        {
            try
            {
                VehiclesSelectedByPhoneCall mVehiclesSelectedByPhoneCall;
                string txtSelectionIndex = "      /";

                //ZORAN:    Prvo da gi stavam site SElectionIndex vo eden string
                //          Potoa ke go zakacam na OrderText za da mi se pojavi vo izvestajot
                foreach (Vehicle mVeh in pSelectedVehicles)
                {
                    txtSelectionIndex = txtSelectionIndex + mVeh.DescriptionShort + "-" + mVeh.SelectionIndex.ToString() + "/";
                }

                foreach (Vehicle mVeh in pSelectedVehicles)
                {
                    mVehiclesSelectedByPhoneCall = new VehiclesSelectedByPhoneCall();

                    mVehiclesSelectedByPhoneCall.IdVehicle = mVeh.IdVehicle;
                    mVehiclesSelectedByPhoneCall.IdLocation = mVeh.currentGPSData.IdLocation;
                    mVehiclesSelectedByPhoneCall.IdPhoneCall = pPhoneCall.IdPhoneCall;
                    mVehiclesSelectedByPhoneCall.VehicleRegion = mVeh.currentGPSData.IdRegion;
                    mVehiclesSelectedByPhoneCall.PhoneCallRegion = pPhoneCall.oAddressFrom.oGisRegions.IdRegion;
                    mVehiclesSelectedByPhoneCall.DateTimeShortOrderSent = System.DateTime.Now;
                    mVehiclesSelectedByPhoneCall.OrderText = GetAddressFromPhoneCall(pPhoneCall) + "  " + txtSelectionIndex;

                    DataRepository.VehiclesSelectedByPhoneCallProvider.Insert(mVehiclesSelectedByPhoneCall);
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertIntoDBSelectedVehiclesByPhoneCall", ex);
            }

        }



        private void InsertIntoDBOrderConfirmationsByPhoneCall(clsPotvrdaNaNaracka pPotvrdaNaNaracka, bool pZakasnetaPotvrda)
        {
            try
            {
                VehiclesOrderConfirmationsByPhoneCall mVehiclesOrderConfirmationsByPhoneCall = new VehiclesOrderConfirmationsByPhoneCall();


                mVehiclesOrderConfirmationsByPhoneCall.IdVehicle = pPotvrdaNaNaracka.IdVehicle;
                mVehiclesOrderConfirmationsByPhoneCall.IdPhoneCall = pPotvrdaNaNaracka.IdPhoneCall;
                mVehiclesOrderConfirmationsByPhoneCall.Minutes = pPotvrdaNaNaracka.Minuti;
                mVehiclesOrderConfirmationsByPhoneCall.DateTimeConfirmationReceived = System.DateTime.Now;
                mVehiclesOrderConfirmationsByPhoneCall.ZakasnetaPotvrda = pZakasnetaPotvrda;

                long mLocation = 0;

                //ZORAN:    Ovde treba da se zeme tekovnata sostoja od VEhicle Container, no ne znam kako da go napravam toa
                //          Zatoa ja zemam vrednost za Vehicle sto veke e vo _dictVehicleByPhoneCall (moze da e zastarena)
                foreach (KeyValuePair<long, TList<Vehicle>> kvp in _dictVehicleByPhoneCall)               
                {
                    if (kvp.Key == pPotvrdaNaNaracka.IdPhoneCall)
                    {
                        TList<Vehicle> tmplLstVehicles = kvp.Value;

                        foreach (Vehicle tmp in tmplLstVehicles)
                        {
                            if (tmp.IdVehicle == pPotvrdaNaNaracka.IdVehicle)
                                mLocation = tmp.currentGPSData.IdLocation;
                        }
                    }
                }

                mVehiclesOrderConfirmationsByPhoneCall.IdLocation = mLocation;                

                DataRepository.VehiclesOrderConfirmationsByPhoneCallProvider.Insert(mVehiclesOrderConfirmationsByPhoneCall);
            }
            catch (Exception ex)
            {
                log.Error("InsertIntoDBOrderConfirmationsByPhoneCall", ex);
            }
           
        }


        public string GetAddressFromPhoneCall(PhoneCalls pPhoneCall)
        {
                                        
            string tmpStrAddress = "";

            try
            {
                if (pPhoneCall.MessageType == "MC")
                    tmpStrAddress = "A, ";
                else
                {
                    switch (pPhoneCall.GroupCode)
                    {
                        case "291":
                            tmpStrAddress = "D1, ";
                            break;
                        case "292":
                            tmpStrAddress = "D2, ";
                            break;
                        default:
                            tmpStrAddress = "";
                            break;
                    }
                }


                if (pPhoneCall.MessageType == "MC")
                {
                    if (pPhoneCall.oAddressFrom.oGisStreets != null)
                                           
                        tmpStrAddress += pPhoneCall.oAddressFrom.oGisStreets.StreetName + " ";
                }
                else
                {

                    if (pPhoneCall.oAddressFrom.oGisStreets != null)
                    {
                        tmpStrAddress += "Br: " + pPhoneCall.oAddressFrom.HouseNumber.ToString() + ", ";
                        tmpStrAddress += pPhoneCall.oAddressFrom.oGisStreets.StreetName.Trim() + " ";
                    }

                    if (pPhoneCall.oAddressFrom.oGisObjects != null)
                        tmpStrAddress += pPhoneCall.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

                    tmpStrAddress += System.Environment.NewLine;

                    if (pPhoneCall.oAddressFrom.oGisRegions != null)
                        tmpStrAddress += "REON: " + pPhoneCall.oAddressFrom.oGisRegions.IdRegion.ToString().Trim();
                                        
                    if(pPhoneCall.oAddressFrom.To != null && pPhoneCall.oAddressFrom.To.Trim() != "")
                        tmpStrAddress += ", Do: " + pPhoneCall.oAddressFrom.To.Trim();

                    if (pPhoneCall.oAddressFrom.PickUpAddress != null && pPhoneCall.oAddressFrom.PickUpAddress.Trim() != "")
                        tmpStrAddress += ", Mesto: " + pPhoneCall.oAddressFrom.PickUpAddress.Trim();

                    if (pPhoneCall.oAddressFrom.Comment != null)
                        tmpStrAddress += ", " + pPhoneCall.oAddressFrom.Comment.Trim();

                }
            }
            catch (Exception ex)
            {
                log.Error("GetAddressFromPhoneCall: ", ex);
                tmpStrAddress = "---";
            }

            return tmpStrAddress;
        }


        private void SendSMSforSelectedVehicleAndPhoneCall(PhoneCalls pPhoneCalls, Vehicle pPrvoSelektiranoVehicle, int pMinuti)
        {   

            if (pPhoneCalls.MessageType != "MC" && isThisMobile(pPhoneCalls.PhoneNumber.Trim()))
              
                {
                    string SMStext = "Za ";

                    try
                    {
                        
                        if (pPhoneCalls.oAddressFrom != null)
                        {
                            if (pPhoneCalls.oAddressFrom.oGisStreets != null)
                            {
                                SMStext = SMStext + " " + pPhoneCalls.oAddressFrom.oGisStreets.StreetName;
                                SMStext = SMStext + " br: " + pPhoneCalls.oAddressFrom.HouseNumber.ToString ();
                            }
                            
                            if (pPhoneCalls.oAddressFrom.oGisObjects != null)
                            {
                                SMStext = SMStext + " " + pPhoneCalls.oAddressFrom.oGisObjects.ObjectName;
                            }

                            if (pPhoneCalls.oAddressFrom.PickUpAddress != null && pPhoneCalls.oAddressFrom.PickUpAddress.Trim() != "")
                                SMStext = SMStext + ", " + pPhoneCalls.oAddressFrom.PickUpAddress;


                            if (pPhoneCalls.oAddressFrom.To != null && pPhoneCalls.oAddressFrom.To.Trim() != "")
                                SMStext = SMStext + ", do " + pPhoneCalls.oAddressFrom.To;

                            if (pPhoneCalls.oAddressFrom.Comment != null && pPhoneCalls.oAddressFrom.Comment.Trim() != "")
                                SMStext = SMStext + ", " + pPhoneCalls.oAddressFrom.Comment;

                            //Sega se vo latinica
                            SMStext = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(SMStext);

                            //A, potoa i SMS poraka
                            SmSsent tmpSmsSent = new SmSsent();

                            //tmpSmsSent.PhoneNumber = "+38978386281";
                            tmpSmsSent.PhoneNumber = FormatNumberForSMS(pPhoneCalls.PhoneNumber);

                            SMStext = SMStext + " prateno vi e vozilo " + pPrvoSelektiranoVehicle.DescriptionLong + " za " + pMinuti.ToString() + " min.";
                            SMStext = SMStext + ": " + "http://maps.google.com/?q=" + pPrvoSelektiranoVehicle.currentGPSData.Latutude_Y.ToString().Replace(",", ".") + "," + pPrvoSelektiranoVehicle.currentGPSData.Longitude_X.ToString().Replace(",", ".");

                            tmpSmsSent.SmStext = SMStext;

                            DataRepository.SmSsentProvider.Insert(tmpSmsSent);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }                
        }


        private int GetTimeForOrderConfirmation(long pIdVehicle, List<clsPotvrdaNaNaracka> plstOrderConfirmations)
        {
            int retVal = 0;

            if (plstOrderConfirmations != null && plstOrderConfirmations.Count > 0)
                foreach (clsPotvrdaNaNaracka pn in plstOrderConfirmations)
                    if (pn.IdVehicle == pIdVehicle)
                        retVal = pn.Minuti;            

            return retVal;
        }


        private void UpdatePendingPhoneCall(long pIdPhoneCalls)
        {

            try
            {
                TList<PendingPhoneCalls> tmpPendingPhoneCalls = DataRepository.PendingPhoneCallsProvider.GetByIdPhoneCall(pIdPhoneCalls);

                if (tmpPendingPhoneCalls != null && tmpPendingPhoneCalls.Count > 0)
                {                    
                    tmpPendingPhoneCalls[0].IsCompleted = true;
                    DataRepository.PendingPhoneCallsProvider.Update(tmpPendingPhoneCalls[0]);                  
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }



        public long InsertIntoDBSubcontractorPendingPhoneCalls(PhoneCalls pPhoneCall)
        {

            long retVal = -1;

            SubcontractorPendingPhoneCalls mSubcontractorPendingPhoneCalls = new SubcontractorPendingPhoneCalls();

            if (pPhoneCall != null)
            {
                try
                {                    
                    mSubcontractorPendingPhoneCalls.IdPhoneCall = pPhoneCall.IdPhoneCall;                    

                    mSubcontractorPendingPhoneCalls.ContactPhoneNumber = pPhoneCall.PhoneNumber;

                    mSubcontractorPendingPhoneCalls.IdRegionFrom = pPhoneCall.oAddressFrom.oGisRegions.IdRegion;

                    if (pPhoneCall.oAddressFrom.oGisStreets != null)
                        mSubcontractorPendingPhoneCalls.IdStreetFrom = long.Parse(pPhoneCall.oAddressFrom.oGisStreets.IdStreet);

                    mSubcontractorPendingPhoneCalls.HouseNumberFrom = pPhoneCall.oAddressFrom.HouseNumber.ToString();

                    if (pPhoneCall.oAddressFrom.oGisObjects != null)
                        mSubcontractorPendingPhoneCalls.IdObjectFrom = pPhoneCall.oAddressFrom.oGisObjects.IdObject;

                    mSubcontractorPendingPhoneCalls.LocationQuality = pPhoneCall.oAddressFrom.LocationQuality;

                    if (pPhoneCall.oAddressTo != null && pPhoneCall.oAddressTo.oGisRegions != null)
                        mSubcontractorPendingPhoneCalls.IdRegionTo = pPhoneCall.oAddressTo.oGisRegions.IdRegion;

                    mSubcontractorPendingPhoneCalls.LongitudeX = pPhoneCall.oAddressFrom.LocationX;
                    mSubcontractorPendingPhoneCalls.LatitudeY = pPhoneCall.oAddressFrom.LocationY;

                    mSubcontractorPendingPhoneCalls.PickUpAddress = pPhoneCall.oAddressFrom.PickUpAddress;
                    mSubcontractorPendingPhoneCalls.To = pPhoneCall.oAddressFrom.To;
                    mSubcontractorPendingPhoneCalls.Comment = pPhoneCall.oAddressFrom.Comment;

                    // mSubcontractorPendingPhoneCalls.IdUser = ...                 // ZORAN: Sega e NULL, no ova ke se popolni so ID_User koj go pratil voziloto na konkretnata adresa

                    mSubcontractorPendingPhoneCalls.NumberOfVehiclesToReturn = 4;   // ZORAN: Stavam fiksno 4, posto taa informacija e izgubena prethodno (treba da baram vo baza, nema logika)
                    mSubcontractorPendingPhoneCalls.IncludeBusy = false;
                    mSubcontractorPendingPhoneCalls.IdFirstCompany = 3;             // ZORAN: OVA MORA DA SE PROMENI SO KONKRETNO ID NA KOMPANIJA, inace ke pagja!!!!

                    mSubcontractorPendingPhoneCalls.IdAlternativeCompanies = "";

                    //string tmpString = "";

                    //if (pIdAlternativeCompanies != null && pIdAlternativeCompanies.Count > 0)
                    //    foreach (long mLongId in pIdAlternativeCompanies)
                    //    {
                    //        if (tmpString.Length != 0)
                    //            tmpString = tmpString + ",";
                    //        tmpString = tmpString + mLongId.ToString();
                    //    }

                    // mSubcontractorPendingPhoneCalls.IdAlternativeCompanies = tmpString;

                    mSubcontractorPendingPhoneCalls.NumberOfRetries = 1;
                    mSubcontractorPendingPhoneCalls.DateTimeSubmitted = System.DateTime.Now;
                    mSubcontractorPendingPhoneCalls.IsCompleted = false;
                    mSubcontractorPendingPhoneCalls.ConfirmedToPassangerAsMissed = false;

                    DataRepository.SubcontractorPendingPhoneCallsProvider.Insert(mSubcontractorPendingPhoneCalls);

                    retVal = 1;

                }
                catch (Exception ex)
                {
                    log.Error("InsertPendingPhoneCalls 1: ", ex);
                }
            }
            else
            {
                log.Error("NULL pPhoneCall paarmeter in InsertPendingPhoneCalls 2 !!!");
            }

            return retVal;

        }


        private bool isThisMobile(string pPhoneNumber)
        {
            bool retVal = false;
            string mPhoneNumber = pPhoneNumber;

            if (mPhoneNumber.Substring(0, 2) == "01" || mPhoneNumber.Substring(0, 2) == "02")
                mPhoneNumber = mPhoneNumber.Substring(2, mPhoneNumber.Length - 2);

            if (mPhoneNumber.Substring(0, 1) == "+")
                mPhoneNumber = mPhoneNumber.Substring(1, mPhoneNumber.Length - 1);

            if (mPhoneNumber.Substring(0, 3) == "389")
            {
                mPhoneNumber = mPhoneNumber.Substring(3, mPhoneNumber.Length - 3);
                mPhoneNumber = "0" + mPhoneNumber;
            }

            switch (mPhoneNumber.Substring(0, 3))
            {
                case "070":
                case "071":
                case "072":
                case "075":
                case "076":
                case "077":
                case "078":
                    retVal = true;
                    break;
                default:
                    retVal = false;
                    break;
            }

            return retVal;
        }



        //ZORAN:    Mislam, ne sum siguren, deka treba da bide vo format +389XXXYYY
        //          Duri i da ne e, ke go napravam taka, da ne go mislam
        //          Nekogas stiga so +, nekogas ne
        //          Nekogasima 389, nekogas ne
        private string FormatNumberForSMS(string pPhoneNumber)
        {
            string retVal = pPhoneNumber;

            try
            {
                if (retVal.Substring(0, 1) == "+")
                    retVal = retVal.Substring(1, retVal.Length - 1);

                if (retVal.Substring(0, 3) == "389")
                {
                    retVal = retVal.Substring(3, retVal.Length - 3);
                    retVal = "0" + retVal;
                }

                retVal = "+389" + retVal.Substring(1, retVal.Length - 1);
            }
            catch (Exception ex)
            {
            }

            return retVal;
        }



        private double CalculateDistance(double LongitudeX_From, double LatitudeY_From, double LongitudeX_To, double LatitudeY_To)
        {
            //PAZI FORMAT NA KOORDINATI 
            //(Prima Stepeni DD.DDDDDDDD)
            //Latituda (42) = Y
            //Longituda (21) = X
            double RetVal = 0;


            ////Za vo radijani
            GPSData myFrom = new GPSData();
            GPSData myTo = new GPSData();
            myFrom.Longitude_X = LongitudeX_From / 180 * Math.PI;
            myFrom.Latutude_Y = LatitudeY_From / 180 * Math.PI;
            myTo.Longitude_X = LongitudeX_To / 180 * Math.PI;
            myTo.Latutude_Y = LatitudeY_To / 180 * Math.PI;


            //---------------------------------------------------------------------------------------------------------------
            //Ova e vtora verzija
            double dLat = myTo.Longitude_X - myFrom.Longitude_X;
            double dLon = myTo.Latutude_Y - myFrom.Latutude_Y;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(myFrom.Longitude_X) * Math.Cos(myTo.Longitude_X) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            RetVal = 6377397 * c;

            return RetVal;
        }
    }
}
