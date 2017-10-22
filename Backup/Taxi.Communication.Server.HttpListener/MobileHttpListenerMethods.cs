using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using System.Configuration;

using log4net;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.Bases;

using GlobSaldo.GISDB.Entities;

using Taxi.Communication.Server.Utils;

//using JP.Data.Utils;

using System.Collections.Specialized;

using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;
using System.Web.Script.Serialization;

using Taxi.Communication.Server.Utils.Containers;


namespace Taxi.Communication.Server.HttpListener
{
    public class MobileHttpListenerMethods
    {
        private static ILog log = LogManager.GetLogger("MyService");

        private long NumberOfGetLocationByIdOrder = 0;
        
        JavaScriptSerializer mSerializer = new JavaScriptSerializer();

        private ServiceCallBack _serviceCallBack = null;

        MobileHttpListenerUtils mMobileHttpListenerUtils = new MobileHttpListenerUtils(); 

        long tmpIdVehicle;

        public void setCallBack(ServiceCallBack pServiceCallBack)
        {
            _serviceCallBack = pServiceCallBack;
        }
       


        public long Register(HttpListenerContext pContext, string pEmail, string pPassword, string pName, string pLastName, string pPhoneNumber, string pImei)
        {
            long retVal = -1;

            try
            {
                if (pEmail.Length == 0 || pPassword.Length == 0 || pName.Length == 0 || pLastName.Length == 0 || pPhoneNumber.Length == 0 || pImei.Length == 0)
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Fali nekoj parametar");
                    SendErrorToMobile(pContext, mJsonResponse);
                    return retVal;
                }

                retVal = mMobileHttpListenerUtils.RegisterMobileUser(pEmail, pPassword, pName, pLastName, pPhoneNumber, pImei);

                if (retVal != -1)
                {
                    string mJsonResponse = GenerateJsonForResponse(retVal, "Успешно креиран корисник. Ке добиете код за активација на наведениот e-mail и СМС порака");

                    SendToMobile(pContext, mJsonResponse);

                    //Prvo e-mail
                    Thread myNewThread = new Thread(() => mMobileHttpListenerUtils.SendEmail(pEmail, retVal.ToString()));
                    myNewThread.Start();

                    //Potoa SMS
                    SmSsent mSmsSent = new SmSsent();

                    mSmsSent.PhoneNumber = pPhoneNumber;
                    mSmsSent.SmStext = retVal.ToString();

                    DataRepository.SmSsentProvider.Insert(mSmsSent);
                    
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Проблем со креирање на кориснички профил. Ве молиме обидете се повторно!");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Angel, proveri si gi vrednostite !!!");
                SendErrorToMobile(pContext, mJsonResponse);
            }

            return retVal;
        }


        public long Confirmation(HttpListenerContext pContext, string pImei, string pCode)
        {
            long retVal = -1;

            try
            {
                GlobSaldo.AVL.Entities.TList <MobileUser> lstRetVal = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (lstRetVal != null && lstRetVal.Count > 0)
                {
                    if (lstRetVal[0].Code == long.Parse(pCode))
                    {
                        lstRetVal[0].CodeDateTimeConfirmed = System.DateTime.Now;
                        DataRepository.MobileUserProvider.Update(lstRetVal[0]);

                        retVal = lstRetVal[0].IdMobileUser;
                    }
                }                                
            }
            catch (Exception ex)
            { 
                log.Error("Confirmation ERROR: ", ex);
            }

            string mJsonResponse;

            if(retVal == -1)
                mJsonResponse = GenerateJsonForResponse(retVal, "Ne neuspesna verifikacija");
            else
                mJsonResponse = GenerateJsonForResponse(retVal, "Uspesna verifikacija");

            SendToMobile(pContext, mJsonResponse);

            return retVal;
        }


        // Deaktivirawe po baranje na korisnik
        // Vo praksa i ne mora, bidejki aktivacijata e identicna na nov user, no neka ostanesega vaka.
        public long Deactivate(HttpListenerContext pContext, string pImei, string pId)
        {
            long retVal = -1;

            try
            {
                MobileUser tmpMobileUserForDeactivation = DataRepository.MobileUserProvider.GetByIdMobileUser(long.Parse(pId));

                if (tmpMobileUserForDeactivation != null )
                {
                    //Prvo da stavam tekovnata sostojba vo MobileUserHistory
                    mMobileHttpListenerUtils.InsertNewMobileUserHistory(tmpMobileUserForDeactivation, "Деактивирање по барање на корисник");

                    // Sega go trgam od MobileUser                   
                    DataRepository.MobileUserProvider.Delete(tmpMobileUserForDeactivation);
                    retVal = 0;
                }
            }
            catch (Exception ex)
            { }

            string mJsonResponse;

            if(retVal == -1)
                mJsonResponse = GenerateJsonForResponse(retVal, "Neuspesna deaktivacija");
            else
                mJsonResponse = GenerateJsonForResponse(retVal, "Uspesna deaktivacija");

            SendToMobile(pContext, mJsonResponse);

            //mMobileHttpListenerUtils.SendEmail(pEmail);

            return retVal;
        }


        // pConfirmOrder moze da gi ima slednite vrednosti:
        //      0 = default, nisto ne odgovoril klientot  (avtomatski se stava na nula, na nivo na baza)
        //      1 = OK, tocen klient
        //      2 = Baram da me pobara dispecer
        public void ConfirmDrive(HttpListenerContext pContext, string pImei, string pIdOrder, string pConfirmCode)
        {
            int pIntConfirmCode = int.Parse(pConfirmCode);

            string mJsonResponse = "";

            if (pIntConfirmCode == 1)
                mJsonResponse = GenerateJsonForResponse(0, "");

            if (pIntConfirmCode == 2)
            {
                string mRetVal = mMobileHttpListenerUtils.ConfirmRepeatedOrders(pImei, pIdOrder);

                mJsonResponse = GenerateJsonForResponse(1, mRetVal);
            }

            SendSucessToMobile (pContext, mJsonResponse);
        }


        //  GetVehiclesStatesByIdCompany 
        //  ----------------------------------------------------------------------------------------------
        public void GetVehiclesStatesByIdCompany(HttpListenerContext pContext, long pIdCompany)
        {
            try
            {
                GlobSaldo.AVL.Entities.TList<Vehicle> lstVehicles = VehiclesContainer.Instance.getAllForCompany(pIdCompany);

                if (lstVehicles != null && lstVehicles.Count > 0)
                {
                    List<JsonVehiclesStates> lstJsonVehiclesStates = new List<JsonVehiclesStates>();

                    JsonVehiclesStates mJsonVehicleStates = null;

                    foreach (Vehicle veh in lstVehicles)
                    {
                        mJsonVehicleStates = mMobileHttpListenerUtils.FillVehicleState(veh);

                        if (mJsonVehicleStates != null)
                            lstJsonVehiclesStates.Add(mJsonVehicleStates);
                    }

                    string output = mSerializer.Serialize(lstJsonVehiclesStates);

                    SendToMobile(pContext, output);
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema vozila za ovaa kompanija");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                log.Error("GetVehiclesStatesByIdCompany", ex);
            }
        }


        //  GetVehicleStatesByIdVehicle
        //  ----------------------------------------------------------------------------------------------
        public void GetVehicleStatesByIdVehicle(HttpListenerContext pContext, long pIdVehicle)
        {

            try
            {
                Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(pIdVehicle);

                if (mVehicle != null)
                {

                    JsonVehiclesStates mJsonVehicleStates = mMobileHttpListenerUtils.FillVehicleState(mVehicle);

                    if (mJsonVehicleStates != null)
                    {
                        string output = mSerializer.Serialize(mJsonVehicleStates);

                        SendToMobile(pContext, output);
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Problem so podatocite za vozilo so baraniot ID");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema vozilo so baraniot ID");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                log.Error("GetVehicleStatesByIdVehicle", ex);
            }
        }


        //  GetVehicleLocationByIdOrder
        //  --------------------------------------------------------------------------------------------
        public void GetVehicleLocationByIdOrder(HttpListenerContext pContext, long pId_Vehicle, long pIdOrder, string pImei)
        {
            try
            {
                //NumberOfGetLocationByIdOrder++;

                GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mImei != null && mImei.Count > 0)
                {
                    GlobSaldo.AVL.Entities.TList<OrdersCanceledByDrivers> lstOrdersCanceledByDrivers = DataRepository.OrdersCanceledByDriversProvider.GetByIdOrder(pIdOrder);

                    //if (lstOrdersCanceledByDrivers == null)
                    //{

                        Orders mOrder = DataRepository.OrdersProvider.GetByIdOrder(pIdOrder);

                        if (mOrder != null)
                        {
                            Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI((long)mOrder.IdVehicle);

                            if (mVehicle != null)
                            {
                                JsonVehicleLocation mJsonVehicleLocation = new JsonVehicleLocation();

                                mJsonVehicleLocation.IdVehicle = mVehicle.IdVehicle;
                                mJsonVehicleLocation.LongitudeX = mVehicle.currentGPSData.Longitude_X;
                                mJsonVehicleLocation.LatitudeY = mVehicle.currentGPSData.Latutude_Y;
                                mJsonVehicleLocation.Bearing = mVehicle.currentGPSData.Bearing;

                                if (mVehicle.IdCompany == 1)
                                {
                                    if (mVehicle.currentSensorData.Senzor_9 == 1 || mVehicle.currentSensorData.Senzor_10 == 1)
                                        mJsonVehicleLocation.FreeKm = true;
                                    else
                                        mJsonVehicleLocation.FreeKm = false;
                                }

                                mJsonVehicleLocation.VehicleState = mMobileHttpListenerUtils.GetCorrectStateString(mVehicle);

                                // Sega da vidam da ne ima poraka od vozac za ovoj Order

                                mJsonVehicleLocation.MessageFromDriver = "";

                                GlobSaldo.AVL.Entities.TList<ReceivedMessagesFromVehicles> lstReceivedMessagesFromVehicles = DataRepository.ReceivedMessagesFromVehiclesProvider.GetByIdOrders(pIdOrder);

                                if (lstReceivedMessagesFromVehicles != null && lstReceivedMessagesFromVehicles.Count > 0)
                                {
                                    ReceivedMessagesFromVehicles mReceivedMessageFromVehicle = null;

                                    foreach (ReceivedMessagesFromVehicles tmpRm in lstReceivedMessagesFromVehicles)
                                        if (tmpRm.DateTimeRead == null)
                                        {
                                            mReceivedMessageFromVehicle = tmpRm;

                                            tmpRm.Imei = pImei;
                                            tmpRm.DateTimeRead = System.DateTime.Now;

                                            DataRepository.ReceivedMessagesFromVehiclesProvider.Update(tmpRm);
                                        }

                                    if (mReceivedMessageFromVehicle != null)
                                    {
                                        mJsonVehicleLocation.MessageFromDriver = mReceivedMessageFromVehicle.MessageText;
                                    }
                                }

                                string output = mSerializer.Serialize(mJsonVehicleLocation);
                                SendToMobile(pContext, output);
                            }
                            else
                            {
                                string mJsonResponse = GenerateJsonForResponse(-1, "Nema takvo vozilo");
                                SendErrorToMobile(pContext, mJsonResponse);
                            }
                        }
                        else
                        {
                            string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov order");
                            SendErrorToMobile(pContext, mJsonResponse);
                        }
                    //}
                    //else
                    //{
                    //    string mJsonResponse = GenerateJsonForResponse(-10, "Ве известуваме дека нарачаното возило не е во можност да пристигне до Вашата локација. Се извинуваме за непријатностите");
                    //    SendErrorToMobile(pContext, mJsonResponse);
                    //}
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Interna greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);

                log.Error("GetVehicleLocationByIdOrder----", ex);
            }
        }


        public long MakeOrder(HttpListenerContext pContext, string strPhoneNumber, string strPhoneExtension, string pPickupAdress, string pComment, string pDo,
            double pLatitude, double pLongitude, string pStreetName, string strStreetNumber, string pListOfCompanies, string pImei)
        {
            long retVal = -1;
            
            //ZORAN: Kontrola da ne proleta nekoj NULL (sredeno e na Android, ama just in case)

            if (pPickupAdress == null)
                pPickupAdress = "";

            if (pComment == null)
                pComment = "";

            if (pDo == null)
                pDo = "";

            if (pStreetName == null)
                pStreetName = "";

            if (strStreetNumber == "")
                strStreetNumber = "0";

            string PickupAdress = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pPickupAdress);
            string strComment = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pComment);
            string strDo = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pDo);
            string strStreetName = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pStreetName);
           
            try
            {

                GlobSaldo.AVL.Entities.TList<BlackList> mBlackList = DataRepository.BlackListProvider.GetByEmailOrImeiOrPhoneNumber("", pImei, "");

                //if (mBlackList == null)
                //{

                    GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                    if (mImei != null && mImei.Count > 0)
                    {

                        List<long> mListOfCompanies = mMobileHttpListenerUtils.getListOfCompanies (pListOfCompanies);

                        if(mListOfCompanies.Count == 0)                            
                        {
                            log.Error("Greska vo parametri za kompanii: " + pListOfCompanies);
                            string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo parametri za kompanii");
                            SendErrorToMobile(pContext, mJsonResponse);
                            return retVal;
                        }

                        //ZORAN:    Prvo generiram PhoneCall
                        //          -------------------------------------
                        PhoneCalls retValPhoneCalls = MobileGeneratePhoneCall(strPhoneNumber, strPhoneExtension, PickupAdress, strComment, pLatitude, pLongitude, strStreetName, strStreetNumber, strDo);                        

                        if (retValPhoneCalls == null)
                        {
                            log.Error("Problem so generiranje na PhoneCall !!!");
                            string mJsonResponse = GenerateJsonForResponse(-1, "Interen problem so PhoneCall, ne moze da se generira naracka");
                            SendErrorToMobile(pContext, mJsonResponse);
                            return retVal;
                        }

                        //ZORAN:    Vtoro, ako imam PhoneCall, pravam order
                        //          ---------------------------------------                                        
                        long retValSelectedVehicles = _serviceCallBack.SelectVehiclesforXYforFirstAndAlternativeCompaniesAndPhoneCall
                                                            (
                                                              retValPhoneCalls.oAddressFrom.LocationX
                                                            , retValPhoneCalls.oAddressFrom.LocationY
                                                            , false
                                                            , 10
                                                            , 1
                                                            , mListOfCompanies
                                                            , retValPhoneCalls
                                                            , -1
                                                            );

                        if (retValSelectedVehicles > 0)
                        {                            
                            //ZORAN:    Ovde otvaram novThread koj ke ceka voziloto da smeni status
                            //          Ceka dodeka e vo StateWaitResponse, odnosno se dodeka ne dojde vo StateWaitClientConfirmation
                            //          Site drugi state-ovi se nedozvoleni i za niv treba da vrati ERROR                                 

                            Thread ThreadWaitForDriverConfiramtion = new Thread(() => WaitForDriverConfiramtion(pContext, retValPhoneCalls.IdPhoneCall));
                            ThreadWaitForDriverConfiramtion.Start();
                        }
                        else
                        {                           
                            OrdersWithNoCars mOrdersWithNoCars = new OrdersWithNoCars();

                            mOrdersWithNoCars.Imei = pImei;
                            mOrdersWithNoCars.DateTime = System.DateTime.Now;
                            mOrdersWithNoCars.Comment = strPhoneNumber + ", " + strPhoneExtension
                                                            + ", " + pPickupAdress + ", " + pComment + ", "
                                                            + pDo + ", " + pStreetName + ", " + strStreetNumber;

                            DataRepository.OrdersWithNoCarsProvider.Insert(mOrdersWithNoCars);

                            string mJsonResponse = GenerateJsonForResponse(-1, "Нема слободни возила, обидете се повторно! ");
                            Thread ThreadInformCustomer = new Thread(() => SendToMobileWithDelay(pContext, mJsonResponse, 5));
                            ThreadInformCustomer.Start();
                        }
                    }

                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                //}
                //else
                //{
                //    string mJsonResponse = GenerateJsonForResponse(-2, "Вашиот налог е привремено блокиран. За деблокирање испратете е-маил.");
                //    SendErrorToMobile(pContext, mJsonResponse);
                //}

            }
            
            catch (Exception ex)
            {
                log.Error("Greska vo parametri za order", ex);
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo parametri za order");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        

            return retVal;

        }
        


        
        public void SentMessageToVehicleByIdOrder(HttpListenerContext pContext, long pIdOrder, long pIdUser, string pMessage, string pImei)
        {
            try
            {

                GlobSaldo.AVL.Entities.TList<MobileUser> lstMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (lstMobileUser != null && lstMobileUser.Count > 0)
                {
                    Orders mOrder = DataRepository.OrdersProvider.GetByIdOrder(pIdOrder);

                    if(mOrder != null)
                    {
                        Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI((long)mOrder.IdVehicle);

                        if (mVehicle.currentStateString == "StateMoveToClient" ||
                                mVehicle.currentStateString == "StateMoveToClientNewPhoneCall" ||
                                mVehicle.currentStateString == "StateWaitClient" ||
                                mVehicle.currentStateString == "StateWaitClientNewPhoneCall")
                        {

                            long retVal = _serviceCallBack.SendPopUp((long)mOrder.IdVehicle, pIdUser, pMessage);

                            if (retVal == 0)
                            {
                                // Da insertiram prvo vo baza
                                SentMessagesToVehicles mSentMessagesToVehicles = new SentMessagesToVehicles();

                                mSentMessagesToVehicles.IdOrders = pIdOrder;
                                mSentMessagesToVehicles.Imei = pImei;
                                mSentMessagesToVehicles.MessageText = pMessage;
                                mSentMessagesToVehicles.DateTimeSent = System.DateTime.Now;

                                DataRepository.SentMessagesToVehiclesProvider.Insert(mSentMessagesToVehicles);

                                string mJsonResponse = GenerateJsonForResponse(0, "Pratena poraka do vozilo");
                                SendSucessToMobile(pContext, mJsonResponse);
                            }
                            else
                            {
                                string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo prakanje poraka do vozilo");
                                SendErrorToMobile(pContext, mJsonResponse);
                            }
                        }
                        else
                        {
                            string mJsonResponse = GenerateJsonForResponse(-1, "Ne moze da se prati zaradi State");
                            SendErrorToMobile(pContext, mJsonResponse);
                        }
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov Order");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Interna greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }



        public void SentMessageToVehicleByIdVehicle(HttpListenerContext pContext, long pIdVehicle, string pMessage)
        {
            try
            {
                Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(pIdVehicle);

                if (mVehicle != null)
                {
                    long retVal = _serviceCallBack.SendPopUp (pIdVehicle, -1, pMessage);

                    if (retVal == 0)
                    {
                        // Da insertiram prvo vo baza
                        MobileMonitoringSentMessagesToVehicles mMobileMonitoringSentMessagesToVehicles = new MobileMonitoringSentMessagesToVehicles();

                        mMobileMonitoringSentMessagesToVehicles.IdVehicle = pIdVehicle;
                        mMobileMonitoringSentMessagesToVehicles.MessageText = pMessage;
                        mMobileMonitoringSentMessagesToVehicles.DateTimeSent = System.DateTime.Now;

                        DataRepository.MobileMonitoringSentMessagesToVehiclesProvider.Insert(mMobileMonitoringSentMessagesToVehicles);

                        string mJsonResponse = GenerateJsonForResponse(0, "Pratena poraka do vozilo");
                        SendSucessToMobile(pContext, mJsonResponse);
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo prakanje poraka do vozilo");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takvo vozilo");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Interna greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }


        //ZORAN: Samo za test
        public void SentRingMessageToVehicleByIdVehicle(HttpListenerContext pContext, long pIdVehicle, short pNoSeconds)
        {
            try
            {
                Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(pIdVehicle);

                if (mVehicle != null)
                {
                    long retVal = _serviceCallBack.SendRingMessageByIdVehicle(pIdVehicle,pNoSeconds);

                    if (retVal == 0)
                    {
                        
                        string mJsonResponse = GenerateJsonForResponse(0, "Pratena poraka do vozilo");
                        SendSucessToMobile(pContext, mJsonResponse);
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo prakanje poraka do vozilo");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takvo vozilo");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Interna greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }



        private void WaitForDriverConfiramtion(HttpListenerContext pContext, long pIdPhoneCall)
        {
            bool bExit = false;
            int mSecondsToWaitForVehicle = 20; //Sega se ceka na vozilo 12-15 sec, pa tuka neka se 20...

            List<Vehicle> mVehiclesList;
            Vehicle mSelectedVehicle = null;

            while (!bExit && mSecondsToWaitForVehicle > 0)
            {
                mVehiclesList = VehiclesContainer.Instance.getAllVehicles();

                foreach (Vehicle tmpVeh in mVehiclesList)
                {
                    if (tmpVeh.currentStateString == "StateWaitClientConfirmation" && tmpVeh.CurrentPhoneCall.IdPhoneCall == pIdPhoneCall)
                    {
                        bExit = true;
                        mSelectedVehicle = tmpVeh;
                    }
                }
                if (!bExit)
                {
                    Thread.Sleep(1000);
                    mSecondsToWaitForVehicle--;
                }
            }


            if (mSelectedVehicle != null)
            {

                JsonOrderResponse mJsonOrderResponse = new JsonOrderResponse();

                mJsonOrderResponse.IdVehicle = mSelectedVehicle.IdVehicle;
                mJsonOrderResponse.LongitudeX = mSelectedVehicle.currentGPSData.Longitude_X;
                mJsonOrderResponse.LatitudeY = mSelectedVehicle.currentGPSData.Latutude_Y;
                mJsonOrderResponse.Bearing = mSelectedVehicle.currentGPSData.Bearing;
                mJsonOrderResponse.DescriptionShort = mSelectedVehicle.DescriptionShort;
                mJsonOrderResponse.Plate = mSelectedVehicle.Plate;
                mJsonOrderResponse.IdOrder = 0;
                mJsonOrderResponse.TimeToPickUp = mSelectedVehicle.lastPhoneCallAccepted.Minuti;

                string output = mSerializer.Serialize(mJsonOrderResponse);
                SendToMobile(pContext, output);
            }
            else
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Vozacot ne potvrdi naracka");
                SendErrorToMobile(pContext, mJsonResponse);
            }

        }


        // ZORAN: Tuka treba da dojde i ID_Order!!! (moze i nameto ID_Vehicle)

        public void OrderConfirmation(HttpListenerContext pContext, long pIdVehicle, int OrderConfirmation, string pImei)
        {
            long retVal = -1;

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mImei != null && mImei.Count > 0)
                {
                    Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(pIdVehicle);

                    if (mVehicle != null)
                    {                        
                        if (OrderConfirmation == 0)
                        {
                            retVal = VehiclesContainer.Instance.cancelRequestFromClient(pIdVehicle);

                            _serviceCallBack.SendPopUp(pIdVehicle, -1, "Android: Klientot ja otkaza narackata");
                         

                            // SEGA da zapisam vo baza deka e cancel-irano

                            GlobSaldo.AVL.Entities.TList<MobileUser> mMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                            if (mMobileUser != null && mMobileUser.Count > 0)
                            {
                                CanceledOrders mCanceledOrders = new CanceledOrders();

                                mCanceledOrders.DateTimeCanceled = System.DateTime.Now;
                                mCanceledOrders.Email = mMobileUser[0].Email;
                                mCanceledOrders.PhoneNumber = mMobileUser[0].PhoneNumber;
                                mCanceledOrders.Imei = pImei;

                                DataRepository.CanceledOrdersProvider.Insert(mCanceledOrders);
                            }
                            else
                            {
                                log.Debug("PROBLEM, dobiena poraka od neaktiven korisnik, IMEI: " + pImei);
                            }

                            string mJsonResponse = GenerateJsonForResponse(0, "Uspesno otkazana naracka");
                            SendSucessToMobile(pContext, mJsonResponse);
                            return;
                        }
                        else
                        {
                            retVal = VehiclesContainer.Instance.acceptedFromClient(pIdVehicle);
                            //log.Debug("Potvrdena naracka za vozilo: " + mVehicle.DescriptionShort);
                        }


                        if (retVal == 0)
                        {
                            long retValInsert = InsertAcceptedOrder(mVehicle, pImei);

                            //log.Debug("INSERT na ORDER za naracka za vozilo: " + mVehicle.DescriptionShort + "ID_Order=" + retValInsert.ToString());

                            if (retValInsert != -1)
                            {
                                JsonOrderResponse mJsonOrderResponse = new JsonOrderResponse();

                                mJsonOrderResponse.IdVehicle = mVehicle.IdVehicle;
                                mJsonOrderResponse.LongitudeX = mVehicle.currentGPSData.Longitude_X;
                                mJsonOrderResponse.LatitudeY = mVehicle.currentGPSData.Latutude_Y;
                                mJsonOrderResponse.Bearing = mVehicle.currentGPSData.Bearing;
                                mJsonOrderResponse.DescriptionShort = mVehicle.DescriptionShort;
                                mJsonOrderResponse.Plate = mVehicle.Plate;
                                mJsonOrderResponse.IdOrder = retValInsert;

                                string output = mSerializer.Serialize(mJsonOrderResponse);
                                SendToMobile(pContext, output);
                            }
                            else
                            {
                                string mJsonResponse = GenerateJsonForResponse(-1, "");
                                SendErrorToMobile(pContext, mJsonResponse);
                            }
                        }
                        else
                        {
                            string mJsonResponse = GenerateJsonForResponse(-1, "");
                            SendErrorToMobile(pContext, mJsonResponse);
                        }
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }

            catch (Exception ex)
            {
                log.Error("Greska vo OrderConfirmation!!!", ex);
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo OrderConfirmation");
                SendErrorToMobile(pContext, mJsonResponse);       
            }
            
        }

        private long InsertAcceptedOrder(Vehicle pVehicle, string pImei)
        {            

            Orders tNewOrder = new Orders();

            try
            {

                tNewOrder.IdVehicle = pVehicle.IdVehicle;
                tNewOrder.IdPhoneCall = pVehicle.CurrentPhoneCall.IdPhoneCall;
                tNewOrder.IdDriver = pVehicle.DriverShiftInOut.IdDriver;
                tNewOrder.IdUser = -1;
                tNewOrder.IdRegionFrom = 0;
                tNewOrder.AddressMessage = pVehicle.CurrentPhoneCall.oAddressFrom.Comment;


                tNewOrder.LocationQuality = pVehicle.CurrentPhoneCall.oAddressFrom.LocationQuality;
                tNewOrder.CoordinateQuality = pVehicle.CurrentPhoneCall.oAddressFrom.LocationQuality;
                tNewOrder.LongitudeX = pVehicle.CurrentPhoneCall.oAddressFrom.LocationX;
                tNewOrder.LongitudeX = pVehicle.CurrentPhoneCall.oAddressFrom.LocationY;
                tNewOrder.SystemTime = System.DateTime.Now;
                tNewOrder.IdOrdersSource = 3;   //Android = 3                             

            }
            catch (Exception ex)
            {
                tNewOrder = null;
                log.Error("ANDROID: Greska vo zapis na Order!!", ex);
            }

            if (tNewOrder == null)
                return -1;

            if (DataRepository.OrdersProvider.Insert(tNewOrder))
            {
                MobileOrders mMobileOrders = new MobileOrders();

                mMobileOrders.IdOrder = tNewOrder.IdOrder;
                mMobileOrders.IdVehicle = pVehicle.IdVehicle;
                mMobileOrders.Imei = pImei;
                mMobileOrders.OrderDateTime = System.DateTime.Now;
                mMobileOrders.IdMobileOrderStatus = 2;

                DataRepository.MobileOrdersProvider.Insert(mMobileOrders);

                return tNewOrder.IdOrder;
            }
            else
                return -1;
           
        }

        public void GetTextInfo(HttpListenerContext pContext, string pTextIndex, string pImei)
        {
                JsonResponse mJsonResponse = new JsonResponse();

                MobileInfos mMobileInfos = DataRepository.MobileInfosProvider.GetByIdMobileInfo((long)1);

                if (mMobileInfos != null)
                {
                    mJsonResponse.ResponseCode = long.Parse(pTextIndex);
                    mJsonResponse.ResponseDescription = mMobileInfos.MobileInfosText;

                    string output = mSerializer.Serialize(mJsonResponse);
                    SendToMobile(pContext, output);
                }
                else
                {
                    mJsonResponse.ResponseCode = long.Parse(pTextIndex);
                    mJsonResponse.ResponseDescription = "Prava i obvrski....";

                    string output = mSerializer.Serialize(mJsonResponse);
                    SendToMobile(pContext, output);
                }
        }



        public void ForceUpdateUnit(HttpListenerContext pContext, string pUred, string pIp1, string pIp2, string pIp3, string pIp4, string pPort)
        {
            try
            {
                long mIdVehicle = VehiclesContainer.Instance.getVehicleIDForUnitSerial(pUred);

                if (mIdVehicle > 0)
                {
                    Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI(mIdVehicle);

                    if (mVehicle != null)
                    {
                        char tmpCharArea = (char)int.Parse(pIp1);

                        _serviceCallBack.ForceUpdateUnit(mVehicle.IdVehicle, pUred, (char)int.Parse(pIp1), (char)int.Parse(pIp2), (char)int.Parse(pIp3), (char)int.Parse(pIp4), int.Parse(pPort));

                        string mJsonResponse = GenerateJsonForResponse(-1, "Pratena ForceUpdateUnit do vozilo so seriski broj na ured: " + pUred);
                        SendSucessToMobile(pContext, mJsonResponse);
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema takvo vozilo");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takvo vozilo");
                    SendErrorToMobile(pContext, mJsonResponse);
                }

            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Gresni podatoci ili interna greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }


        // ZORAN:   Funkcija za generiranje na CALL od mobilen
        //          Se povikuva od Android. Na ovoj nacin i ovie CALL-ovi se evidentiraat vo bazata
        //          Dispecerite ne go gledaat, nema potreba
        // 
        //          Ne se stava vo listata na tekovni povici activePhoneCalls, nema logika
        // ***************************************************************************************************

        private PhoneCalls MobileGeneratePhoneCall(string strPhoneNumber, string strPhoneExtension, string PickupAdress, string strComment,
            double pLatitude, double pLongitude, string strStreetName, string strStreetNumber, string strDo)
        {                       

            PhoneCalls phoneMessage = new PhoneCalls();

            phoneMessage.oAddressFrom = new GlobSaldo.AVL.Entities.PartialClasses.clsAddress();

            phoneMessage.PhoneNumber = strPhoneNumber;
            phoneMessage.Extension = strPhoneExtension;     
            phoneMessage.MessageType = "MC";                //Demek, od Mobile Call...
            phoneMessage.LineIn = 0;
            phoneMessage.CallDuration = 0;
            phoneMessage.RingDuration = 0;
            phoneMessage.RinglDateTime = System.DateTime.Now;
            phoneMessage.GroupCode = "777";
            phoneMessage.IdUserInOut = 0;

            phoneMessage.oAddressFrom.LocationX = pLongitude;
            phoneMessage.oAddressFrom.LocationY = pLatitude;
            phoneMessage.oAddressFrom.LocationQuality = 1;

            phoneMessage.oAddressFrom.oGisStreets = new GlobSaldo.AVL.Entities.GisStreets();
            phoneMessage.oAddressFrom.oGisStreets.IdStreet = "0000";
            //phoneMessage.oAddressFrom.oGisStreets.StreetName = "";   // strStreetName;

            

            int mHouseNumber;

            int.TryParse(strStreetNumber, out mHouseNumber);

            phoneMessage.oAddressFrom.HouseNumber = mHouseNumber;      

            // ZORAN: Sega, vo phoneMessage.oAddressFrom.oGisStreets.StreetName ke stavam se:
            //          - ulica + broj
            //          - Pickup Lokacija
            //          - Zabeleska
            //          - Do kade
            // Vo phoneMessage.oAddressFrom.Comment ke stavam prazen string....
             
            phoneMessage.oAddressFrom.Comment = "";

            string tmpStrAddress = mMobileHttpListenerUtils.GetCorrectAddressString(strStreetName, mHouseNumber.ToString(), PickupAdress, strComment, strDo);

            log.Debug("MobileGeneratePhoneCall: GetCorrectAddressString: " + tmpStrAddress);
           
            phoneMessage.oAddressFrom.oGisStreets.StreetName = tmpStrAddress;

            // Sega da stavam i vo koj region se naogja povikot,ke mi trebaza ponatamu...

            MapUtils mMapUtils = new MapUtils("");

            GlobSaldo.AVL.Entities.VList<Vregions> tmpDBregions = mMapUtils.GetGisRegionForLocationDB(pLongitude, pLatitude, 1);

            if (tmpDBregions != null && tmpDBregions.Count > 0)
            {
                GisRegions mGisRegions = DataRepository.GisRegionsProvider.GetByIdRegion((long)tmpDBregions[0].IdRegion);

                if (mGisRegions != null)
                    phoneMessage.oAddressFrom.oGisRegions = mGisRegions;                    
                else
                    phoneMessage.oAddressFrom.oGisRegions = new GisRegions();
            }


            try
            {
                DataRepository.PhoneCallsProvider.Insert(phoneMessage);
            }
            catch (Exception ex)
            {
                log.Error("ZORAN (problem vo obrabotka na MC): Greska so upis na Mobile CALL vo baza", ex);
                return null;                
            }           

            return phoneMessage;
        }



        // ZORAN:   Funkcija za selekcija i rezerviranje na vozilo
        //          Se povikuva posle generiran PhoneCalls. 
        //          Odi po regularna procedura, pa i Dispecerite mu go gledaat statusot
        //         
        // ***************************************************************************************************
        public Vehicle searchAndReserveVehicle(PhoneCalls pPhoneCall, List<long> pListOfCompanies)
        {
            Vehicle retVal = null;   
                 
            try
            {
                GlobSaldo.AVL.Entities.TList<Vehicle> lstVeh = VehiclesContainer.Instance.SelectVehiclesforXY(pPhoneCall.oAddressFrom.LocationX, pPhoneCall.oAddressFrom.LocationY, false, 1, pListOfCompanies);

                if (lstVeh != null && lstVeh.Count > 0)
                {
                    long retVal2 = _serviceCallBack.ReserveVehicle(lstVeh[0].IdVehicle, -1);

                    if (retVal2 >= 0)
                    {
                        long retVal3 = _serviceCallBack.SendAddress1(lstVeh[0].IdVehicle, -1, pPhoneCall);

                        if (retVal3 >= 0)
                        {
                            retVal = lstVeh[0];
                        }                        
                    }                   
                }             
            }

            catch (Exception ex)
            {
                log.Error("Greska vo searchAndReserveVehicle", ex);
            }
           

            return retVal;
        }



        public void GetListOfCompanies(HttpListenerContext pContext, string pImei)
        {
            try
            {

                GlobSaldo.AVL.Entities.TList<MobileUser> lstMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);
                

                if (lstMobileUser != null && lstMobileUser.Count > 0)
                {

                    GlobSaldo.AVL.Entities.TList<Company> lstCompanies = DataRepository.CompanyProvider.GetAll();

                    if (lstCompanies != null && lstCompanies.Count > 0)
                    {

                        List<JsonCompany> lstJsonCompany = new List<JsonCompany>();
                        JsonCompany mJsonCompany = null;

                        foreach (Company tmpComp in lstCompanies)
                        {
                            if (!tmpComp.IsDeleted)
                            {
                                mJsonCompany = new JsonCompany();

                                mJsonCompany.IdCompany = tmpComp.IdCompany;
                                mJsonCompany.CompanyName = tmpComp.Description;

                                lstJsonCompany.Add(mJsonCompany);
                            }
                        }

                        if(lstJsonCompany.Count > 0)
                        {
                            string output = mSerializer.Serialize(lstJsonCompany);

                            SendToMobile(pContext, output);
                        }
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }                    
                }
                
                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                    SendErrorToMobile(pContext, mJsonResponse);
                }

            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }



        public void GetReasonsForCancel(HttpListenerContext pContext, string pImei)
        {
            try
            {

                GlobSaldo.AVL.Entities.TList<MobileUser> lstMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);


                if (lstMobileUser != null && lstMobileUser.Count > 0)
                {

                    GlobSaldo.AVL.Entities.TList<ReasonsForUserCancelation> lstReasonsForUserCancelation = DataRepository.ReasonsForUserCancelationProvider.GetAll();                    

                    if (lstReasonsForUserCancelation != null && lstReasonsForUserCancelation.Count > 0)
                    {
                        List<JsonReasonsForUserCancelation> lstJsonReasonsForUserCancelation = new List<JsonReasonsForUserCancelation>();
                        JsonReasonsForUserCancelation mJsonReasonsForUserCancelation = null;

                        foreach (ReasonsForUserCancelation tmpRfC in lstReasonsForUserCancelation)
                        {
                            mJsonReasonsForUserCancelation = new JsonReasonsForUserCancelation();

                            mJsonReasonsForUserCancelation.IdReasonsForUserCancelation = tmpRfC.IdReasonsForUserCancelation;
                            mJsonReasonsForUserCancelation.ReasonsForUserCancelationText = tmpRfC.ReasonsForUserCancelationText;

                            lstJsonReasonsForUserCancelation.Add(mJsonReasonsForUserCancelation);
                        }

                        if (lstJsonReasonsForUserCancelation.Count > 0)
                        {
                            string output = mSerializer.Serialize(lstJsonReasonsForUserCancelation);

                            SendToMobile(pContext, output);
                        }
                    }
                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }

                else
                {
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                    SendErrorToMobile(pContext, mJsonResponse);
                }

            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }



        public void GetPaymentOptions(HttpListenerContext pContext, string pImei)
        {
            try
            {

                GlobSaldo.AVL.Entities.TList<BlackList> lstBlackList = DataRepository.BlackListProvider.GetByEmailOrImeiOrPhoneNumber("", pImei, "");

                //if (lstBlackList == null)
                //{
                    GlobSaldo.AVL.Entities.TList<MobileUser> lstMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                    if (lstMobileUser != null && lstMobileUser.Count > 0)
                    {

                        GlobSaldo.AVL.Entities.TList<PaymentOptions> lstPaymentOptions = DataRepository.PaymentOptionsProvider.GetAll();

                        if (lstPaymentOptions != null && lstPaymentOptions.Count > 0)
                        {
                            List<JsonPaymentOptions> lstJsonPaymentOptions = new List<JsonPaymentOptions>();
                            JsonPaymentOptions mJsonPaymentOptions = null;

                            foreach (PaymentOptions tmpPO in lstPaymentOptions)
                            {
                                mJsonPaymentOptions = new JsonPaymentOptions();

                                mJsonPaymentOptions.IdPaymentOption = tmpPO.IdPaymentOption;
                                mJsonPaymentOptions.PaymentOptionText = tmpPO.PaymentOptionName;

                                lstJsonPaymentOptions.Add(mJsonPaymentOptions);
                            }

                            if (lstJsonPaymentOptions.Count > 0)
                            {
                                string output = mSerializer.Serialize(lstJsonPaymentOptions);

                                SendToMobile(pContext, output);
                            }
                        }
                        else
                        {
                            string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                            SendErrorToMobile(pContext, mJsonResponse);
                        }
                    }

                    else
                    {
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema podatoci!");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                //}
                //else
                //{
                //    string mJsonResponse = GenerateJsonForResponse(-2, "Имате забрана за користење на услугата!");
                //    SendErrorToMobile(pContext, mJsonResponse);
                //}

            }
            catch (Exception ex)
            {
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska na servisot");
                SendErrorToMobile(pContext, mJsonResponse);
            }
        }


        public void CancelOrder(HttpListenerContext pContext, long pIdOrderForCancel, long pIdReasonForCancel, string pImei)
        {
            long retVal = -1;

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mImei != null && mImei.Count > 0)
                {
                    Orders mOrder = DataRepository.OrdersProvider.GetByIdOrder(pIdOrderForCancel);

                    if (mOrder != null)
                    {
                        Vehicle mVehicle = VehiclesContainer.Instance.getSingleVehicleZOKI((long)mOrder.IdVehicle);

                        if (mVehicle != null)
                        {
                            retVal = VehiclesContainer.Instance.cancelRequestFromClient(mVehicle.IdVehicle);

                            _serviceCallBack.SendPopUp(mVehicle.IdVehicle, -1, "Android: Klientot ja otkaza narackata");


                            // SEGA da zapisam vo baza deka e cancel-irano

                            GlobSaldo.AVL.Entities.TList<MobileUser> mMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);
                            GlobSaldo.AVL.Entities.ReasonsForUserCancelation mReasonsForUserCancelation = DataRepository.ReasonsForUserCancelationProvider.GetByIdReasonsForUserCancelation(pIdReasonForCancel);

                            if (mMobileUser != null && mMobileUser.Count > 0)
                            {
                                CanceledOrders mCanceledOrders = new CanceledOrders();

                                mCanceledOrders.IdOrders = pIdOrderForCancel;
                                mCanceledOrders.DateTimeCanceled = System.DateTime.Now;
                                mCanceledOrders.Email = mMobileUser[0].Email;
                                mCanceledOrders.PhoneNumber = mMobileUser[0].PhoneNumber;
                                mCanceledOrders.Imei = pImei;
                                mCanceledOrders.Comment = "Zakasneto otkazuvanje: " + mReasonsForUserCancelation.ReasonsForUserCancelationText;

                                DataRepository.CanceledOrdersProvider.Insert(mCanceledOrders);
                            }
                            else
                            {
                                log.Debug("PROBLEM, dobiena poraka od neaktiven korisnik, IMEI: " + pImei);
                            }

                            string mJsonResponse = GenerateJsonForResponse(0, "Uspesno otkazana naracka");
                            SendSucessToMobile(pContext, mJsonResponse);
                            return;


                        }
                        else
                        {
                            log.Debug("ANDROID: Pobarano e OrderCancel za nepostoecko vozilo");
                            string mJsonResponse = GenerateJsonForResponse(-1, "");
                            SendErrorToMobile(pContext, mJsonResponse);
                        }
                    }
                    else
                    {
                        log.Debug("Android: Pobarano e OrderCancel od nepostoecki IMEI");
                        string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }
                else
                {
                    log.Debug("Android: Pobarano e OrderCancel za nepostoecki IdOrder");
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov IdOrder");
                    SendErrorToMobile(pContext, mJsonResponse);
                }

            }

            catch (Exception ex)
            {
                log.Error("Greska vo CancelOrder!!!", ex);
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo CancelOrder");
                SendErrorToMobile(pContext, mJsonResponse);       
            }
            
        }


        public void InsertServiceEvaluation(HttpListenerContext pContext, long pIdOrderForCancel, int pEvaluationValue, string pImei, string pComment)            
        {           

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mImei != null && mImei.Count > 0)
                {
                    GlobSaldo.AVL.Entities.ServiceEvaluation mServiceEvaluation = new ServiceEvaluation();

                    mServiceEvaluation.IdOrder = pIdOrderForCancel;
                    mServiceEvaluation.EvaluationValue = pEvaluationValue;
                    mServiceEvaluation.Imei = pImei;
                    mServiceEvaluation.DateTimeReceived = System.DateTime.Now;
                    mServiceEvaluation.Comment = pComment;

                    DataRepository.ServiceEvaluationProvider.Insert(mServiceEvaluation);

                    string mJsonResponse = GenerateJsonForResponse(0, "Uspesno insertirana evaluacija");
                    SendSucessToMobile(pContext, mJsonResponse);
                    return;
                }
                else
                {
                    log.Debug("Android: Pobarano e InsertServiceEvaluation od nepostoecki IMEI");
                    string mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }

            catch (Exception ex)
            {
                log.Error("Greska vo InsertServiceEvaluation!!!", ex);
                string mJsonResponse = GenerateJsonForResponse(-1, "Greska vo InsertServiceEvaluation");
                SendErrorToMobile(pContext, mJsonResponse);       
            }
            
        }




        public long ReservationNew(HttpListenerContext pContext, string pPickupAdress, string pComment, string pDo,
            double pLatitudeY, double pLongitudeX, string pStreetName, string pStreetNumber, string pListOfCompanies, string pDateTime, string pImei)
        {
            long retVal = -1;
            string mJsonResponse;

            //ZORAN: Kontrola da ne proleta nekoj NULL (sredeno e na Android, ama just in case)

            if (pPickupAdress == null)
                pPickupAdress = "";

            if (pComment == null)
                pComment = "";

            if (pDo == null)
                pDo = "";

            if (pStreetName == null)
                pStreetName = "";

            if (pStreetNumber == "")
                pStreetNumber = "0";

            string PickupAdress = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pPickupAdress);
            string strComment = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pComment);
            string strDo = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pDo);
            string strStreetName = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pStreetName);

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mMobileUser != null && mMobileUser.Count > 0)
                {
                    try
                    {
                        MobileReservations mMobileReservations = new MobileReservations();

                        mMobileReservations.StreetName = pStreetName;
                        mMobileReservations.StreetNumber = int.Parse(pStreetNumber);
                        mMobileReservations.PickupAdress = pPickupAdress;
                        mMobileReservations.To = pDo;
                        mMobileReservations.Comment = pComment;
                        mMobileReservations.Companies = pListOfCompanies;
                        mMobileReservations.Longitudex = pLongitudeX;
                        mMobileReservations.Latitudey = pLatitudeY;
                        mMobileReservations.ReservationSystemTime = System.DateTime.Now;
                        mMobileReservations.ReservationPickUpTime = DateTime.ParseExact(pDateTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                        mMobileReservations.Imei = pImei;
                        mMobileReservations.IdMobileReservationStatus = 1;

                        DataRepository.MobileReservationsProvider.Insert(mMobileReservations);

                        mJsonResponse = GenerateJsonForResponse(mMobileReservations.IdMobileReservations, "Успешно запишано во база!");
                        SendSucessToMobile(pContext, mJsonResponse);

                        //Sega e-mail (+ SMS, no toa e vo procedurata: mMobileHttpListenerUtils.SendEmailForReservation)
                        Thread myNewThread = new Thread(() => mMobileHttpListenerUtils.SendEmailForReservation(mMobileReservations, mMobileUser[0], "Потврда за резервација", "NewReservation"));
                        myNewThread.Start();

                        ////Potoa SMS
                        //SmSsent mSmsSent = new SmSsent();

                        //mSmsSent.PhoneNumber = mMobileUser[0].PhoneNumber ;
                        //mSmsSent.SmStext = "Vasa rezervacija: " + mMobileReservations.IdMobileReservations.ToString();

                        //DataRepository.SmSsentProvider.Insert(mSmsSent);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Greska vo INSERT za ReservationNew", ex);
                        mJsonResponse = GenerateJsonForResponse(-1, "Greska vo INSERT za ReservationNew");
                        SendErrorToMobile(pContext, mJsonResponse);
                    }
                }

                else
                {
                    mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }               
            }

            catch (Exception ex)
            {
                log.Error("Greska vo parametri za ReservationNew", ex);
                mJsonResponse = GenerateJsonForResponse(-1, "Greska vo parametri za ReservationNew");
                SendErrorToMobile(pContext, mJsonResponse);
            }


            return retVal;

        }




        public long ReservationUpdate(HttpListenerContext pContext, string pIdMobileReservation, string pPickupAdress, string pComment, string pDo,
            double pLatitudeY, double pLongitudeX, string pStreetName, string pStreetNumber, string pListOfCompanies, string pDateTime, string pImei)
        {
            long retVal = -1;
            string mJsonResponse;

            //ZORAN: Kontrola da ne proleta nekoj NULL (sredeno e na Android, ama just in case)

            if (pPickupAdress == null)
                pPickupAdress = "";

            if (pComment == null)
                pComment = "";

            if (pDo == null)
                pDo = "";

            if (pStreetName == null)
                pStreetName = "";

            if (pStreetNumber == "")
                pStreetNumber = "0";

            string PickupAdress = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pPickupAdress);
            string strComment = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pComment);
            string strDo = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pDo);
            string strStreetName = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(pStreetName);

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mMobileUser != null && mMobileUser.Count > 0)
                {                    
                    MobileReservations mMobileReservations = new MobileReservations();

                    mMobileReservations.IdMobileReservations = long.Parse(pIdMobileReservation);
                    mMobileReservations.StreetName = pStreetName;
                    mMobileReservations.StreetNumber = int.Parse(pStreetNumber);
                    mMobileReservations.PickupAdress = pPickupAdress;
                    mMobileReservations.To = pDo;
                    mMobileReservations.Comment = pComment;
                    mMobileReservations.Companies = pListOfCompanies;
                    mMobileReservations.Longitudex = pLongitudeX;
                    mMobileReservations.Latitudey = pLatitudeY;
                    mMobileReservations.ReservationSystemTime = System.DateTime.Now;
                    mMobileReservations.ReservationPickUpTime = DateTime.ParseExact(pDateTime, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                    mMobileReservations.Imei = pImei;
                    mMobileReservations.IdMobileReservationStatus = 2;

                    DataRepository.MobileReservationsProvider.Update(mMobileReservations);

                    mJsonResponse = GenerateJsonForResponse(mMobileReservations.IdMobileReservations, "Успешно запишано во база!");
                    SendSucessToMobile(pContext, mJsonResponse);


                    //Sega e-mail (+ SMS, no toa e vo procedurata: mMobileHttpListenerUtils.SendEmailForReservation)
                    Thread myNewThread = new Thread(() => mMobileHttpListenerUtils.SendEmailForReservation(mMobileReservations, mMobileUser[0], "Корекција на резервација", "UpdateReservation"));
                    myNewThread.Start();

                    //////Potoa SMS
                    ////SmSsent mSmsSent = new SmSsent();

                    ////mSmsSent.PhoneNumber = mMobileUser[0].PhoneNumber;
                    ////mSmsSent.SmStext = "Korekcija na rezervacija: " + mMobileReservations.IdMobileReservations.ToString();

                    ////DataRepository.SmSsentProvider.Insert(mSmsSent);
                }

                else
                {
                    mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }               
            }

            catch (Exception ex)
            {
                log.Error("Greska vo parametri za ReservationNew", ex);
                mJsonResponse = GenerateJsonForResponse(-1, "Greska vo parametri za ReservationNew");
                SendErrorToMobile(pContext, mJsonResponse);
            }


            return retVal;
        }


        public long ReservationCancel(HttpListenerContext pContext, string pIdMobileReservation, string pImei)
        {
            long retVal = -1;
            string mJsonResponse;
           
            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mMobileUser != null && mMobileUser.Count > 0)
                {                                          
                    long mIdMobileReservations = long.Parse(pIdMobileReservation);

                    MobileReservations mMobileReservations = DataRepository.MobileReservationsProvider.GetByIdMobileReservations(mIdMobileReservations);
                  
                    mMobileReservations.IdMobileReservationStatus = 3;

                    DataRepository.MobileReservationsProvider.Update(mMobileReservations);

                    mJsonResponse = GenerateJsonForResponse(mMobileReservations.IdMobileReservations, "Успешно запишано во база!");
                    SendSucessToMobile(pContext, mJsonResponse);


                    //Sega e-mail (+ SMS, no toa e vo procedurata: mMobileHttpListenerUtils.SendEmailForReservation)
                    Thread myNewThread = new Thread(() => mMobileHttpListenerUtils.SendEmailForReservation(mMobileReservations, mMobileUser[0], "Откажување на резервација", "CancelReservation"));
                    myNewThread.Start();

                    //////Potoa SMS
                    ////SmSsent mSmsSent = new SmSsent();

                    ////mSmsSent.PhoneNumber = mMobileUser[0].PhoneNumber;
                    ////mSmsSent.SmStext = "Otkazana rezervacija: " + mMobileReservations.IdMobileReservations.ToString();

                    ////DataRepository.SmSsentProvider.Insert(mSmsSent);
                }

                else
                {
                    mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }

            catch (Exception ex)
            {
                log.Error("Greska vo UPDATE za Reservation", ex);
                mJsonResponse = GenerateJsonForResponse(-1, "Greska vo UPDATE za Reservation");
                SendErrorToMobile(pContext, mJsonResponse);
            }

            return retVal;
        }


        public long ReservationGetAllPending(HttpListenerContext pContext, string pImei)
        {
            long retVal = -1;
            string mJsonResponse;

            try
            {
                GlobSaldo.AVL.Entities.TList<MobileUser> mImei = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (mImei != null && mImei.Count > 0)
                {
                   
                    GlobSaldo.AVL.Entities.TList<MobileReservations> lstMobileReservations = DataRepository.MobileReservationsProvider.GetActiveByIMEI(pImei);

                    if (lstMobileReservations != null && lstMobileReservations.Count > 0)
                    {                        
                        List<JsonReservation> lstListJsonReservation = new List<JsonReservation>();

                        JsonReservation mJsonReservation = null;

                        foreach (MobileReservations tmpReservation in lstMobileReservations)
                        {
                            mJsonReservation = new JsonReservation();

                            mJsonReservation.IdReservation = tmpReservation.IdMobileReservations;
                            mJsonReservation.StreetName = tmpReservation.StreetName;
                            mJsonReservation.StreetNumber = (int)tmpReservation.StreetNumber;
                            mJsonReservation.PickupAdress = tmpReservation.PickupAdress;
                            mJsonReservation.To = tmpReservation.To;
                            mJsonReservation.Comment = tmpReservation.Comment;
                            mJsonReservation.Companies = tmpReservation.Companies;
                            mJsonReservation.LongitudeX= tmpReservation.Longitudex;
                            mJsonReservation.LatitudeY = tmpReservation.Latitudey;
                            mJsonReservation.ReservationPickUpTime = tmpReservation.ReservationPickUpTime.ToString("yyyy-MM-dd HH:mm");                                                          
                            
                            lstListJsonReservation.Add(mJsonReservation);
                        }

                        // ZORAN:   Tuka ne proveruvam dali ima objekti vo listata ili ne
                        //          NA strana na android ako e prazna lista, se brise i lokalnata baza!!!
                                                
                        string output = mSerializer.Serialize(lstListJsonReservation);
                        SendToMobile(pContext, output);                                               

                    }
                    else
                    {
                        mJsonResponse = GenerateJsonForResponse(0, "Нема активни резервации за овој корисник!");
                        SendSucessToMobile(pContext, mJsonResponse);
                    }
                }

                else
                {
                    mJsonResponse = GenerateJsonForResponse(-1, "Nema takov MobielUser");
                    SendErrorToMobile(pContext, mJsonResponse);
                }
            }

            catch (Exception ex)
            {
                log.Error("Greska vo UPDATE za Reservation", ex);
                mJsonResponse = GenerateJsonForResponse(-1, "Greska vo UPDATE za Reservation");
                SendErrorToMobile(pContext, mJsonResponse);
            }

            return retVal;
        }


        private void SendToMobile(HttpListenerContext pContext, string pString)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pString);


            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            pContext.Response.ContentLength64 = b.Length;
            pContext.Response.KeepAlive = false;
            pContext.Response.OutputStream.Write(b, 0, b.Length);
            pContext.Response.OutputStream.Close();
        }


        private void SendToMobileWithDelay(HttpListenerContext pContext, string pString, int pDelay)
        {
            int mDelay = pDelay;

            while (mDelay > 0)
            {
                Thread.Sleep(1000);

                mDelay--;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(pString);


            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            pContext.Response.ContentLength64 = b.Length;
            pContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            pContext.Response.KeepAlive = false;
            pContext.Response.OutputStream.Write(b, 0, b.Length);
            pContext.Response.OutputStream.Close();
        }

        public void SendErrorToMobile(HttpListenerContext pContext, string JsonString)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonString);


            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            pContext.Response.ContentLength64 = b.Length;
            pContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            pContext.Response.KeepAlive = false;
            pContext.Response.OutputStream.Write(b, 0, b.Length);
            pContext.Response.OutputStream.Close();
        }


        public void SendSucessToMobile(HttpListenerContext pContext, string JsonString)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(JsonString);


            byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
            pContext.Response.ContentLength64 = b.Length;
            pContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
            pContext.Response.KeepAlive = false;
            pContext.Response.OutputStream.Write(b, 0, b.Length);
            pContext.Response.OutputStream.Close();
        }

       


        public string GenerateJsonForResponse(long pResponseCode, string pResponseDescription)
        {
            string retVal = "";

            try
            {
                JsonResponse mJsonResponse = new JsonResponse();

                mJsonResponse.ResponseCode = pResponseCode;
                mJsonResponse.ResponseDescription = pResponseDescription;

                retVal = mSerializer.Serialize(mJsonResponse);
            }
            catch (Exception ex)
            {
            }

            return retVal;
        }

    }
}
