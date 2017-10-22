using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;
using Taxi.Communication.Server.Containers;
using log4net;
using Taxi.Communication.Server.ConnectionListeners;
using Taxi.Communication.Server.Utils.Containers;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.EnrichMessage
{
    class ReceivedFreeTextEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        public ILog log;

        public ReceivedFreeTextEnricher(GPSListener gpsListener)
        {
            _gpsListener = gpsListener;
            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tFreeText != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            try
            {
                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                message.tFreeText.IdVehicle = veh.IdVehicle;

                // Prvo potvrda do uredot deka porakata e primena

                if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                    log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);

                // Vtoro: Zapis vo baza

                DataRepository.ReceivedFreeTextProvider.Insert(message.tFreeText);

                // ZORAN:   OVA ke odi vo ProcessStatusInfo

                // Treto: Update na vozilo (zakacuvanje na ReceivedFreeText na voziloto i prakanje na Dispecer!)
                // ZORAN:   PROMENA na 04.02.2014
                //          Ako vozacot prati "AF0", da dobie info za svoeto vozilo
                //          Ako prati "AFXnekoj-broj", da go dobie info-to za toa vozilo i toa:
                //                  X = ""...za negovata kola
                //                  X = N...za kola od Nase
                //                  X = L...za Lotus
                //                  X = S...za Sonce

                // ZORAN:   Promena na 07.03.2014
                //          
                //          Ako e nekoja od ovie poraki, ne se prepraka na dispecer!!!

                byte[] mInfoMessage = null;
                Vehicle mVehicleForInfo = null;

                if (message.tFreeText.MessageText.Length <= 1)
                    return -1;

                switch (message.tFreeText.MessageText.Substring(0, 2).ToUpper())
                {
                    case "AF":
                        if (message.tFreeText.MessageText.Length == 2 || message.tFreeText.MessageText.Length == 3) // Znaci bara za sebe!!!
                        {
                            mInfoMessage = CreateStatusInfoForVehicle(veh, veh);
                        }

                        else
                        {
                            if (message.tFreeText.MessageText.Length > 3)
                            {
                                string mCompanyFirstLetter = message.tFreeText.MessageText.Substring(2, 1).ToUpper();

                                if (mCompanyFirstLetter == "N" || mCompanyFirstLetter == "L" || mCompanyFirstLetter == "S")
                                {

                                    mVehicleForInfo = FindVehicle(mCompanyFirstLetter, message.tFreeText.MessageText.Substring(3));

                                    if (mVehicleForInfo != null)
                                    {
                                        mInfoMessage = CreateStatusInfoForVehicle(mVehicleForInfo, veh);
                                    }
                                    else
                                    {
                                        log.Debug("Nema vrateno vozilo od FindVehicle");
                                    }
                                }

                                else
                                {
                                    mInfoMessage = System.Text.Encoding.ASCII.GetBytes("Gresna komanda!");
                                }
                            }
                        }

                        if (mInfoMessage != null)
                        {
                            if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, mInfoMessage) == -1)
                                log.Error("GRESKA vo prakanje info za sostojba na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                        }
                        break;

                    ////case "AM":

                    ////    try
                    ////    {
                    ////        if (message.tFreeText.MessageText.Length > 3)
                    ////        {
                    ////            long mDistance;

                    ////            if (long.TryParse(message.tFreeText.MessageText.Substring(2, message.tFreeText.MessageText.Length - 2), out mDistance))
                    ////            {
                    ////                if (mDistance >= 10 && mDistance <= 300)
                    ////                    mInfoMessage = CreateDistanceInfoForVehicle(veh, mDistance);
                    ////            }
                    ////        }
                    ////    }
                    ////    catch (Exception ex)
                    ////    {
                    ////        log.Error("Error ReceivedFreeText AM", ex);
                    ////    }

                    ////    if (mInfoMessage != null)
                    ////    {
                    ////        if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, mInfoMessage) == -1)
                    ////            log.Error("GRESKA vo prakanje info za sostojba na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                    ////    }
                    ////    break;

                    default:            // Znaci, ne baraat vozacite info, pa porakata se praka do dispecerite!
                        try             // NO! Ako ima aktiven order od Android, togas odi i na Android i na dispecer 
                        {
                            // Prvo proveruvam dali ima aktivna naracka od Android vo tabelaMobileOrders
                            // Ako ima, ja insertiram porakata vo ReceivedMessagesFromVehicles
                            TList<MobileOrders> lst_MobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus(veh.IdVehicle, 2);

                            if (lst_MobileOrders != null && lst_MobileOrders.Count > 0)
                            {
                                ReceivedMessagesFromVehicles mReceivedMessagesFromVehicles = new ReceivedMessagesFromVehicles();

                                mReceivedMessagesFromVehicles.IdOrders = lst_MobileOrders[0].IdOrder;
                                mReceivedMessagesFromVehicles.Imei = lst_MobileOrders[0].Imei;
                                mReceivedMessagesFromVehicles.DateTimeReceived = System.DateTime.Now;
                                mReceivedMessagesFromVehicles.MessageText = message.tFreeText.MessageText;

                                DataRepository.ReceivedMessagesFromVehiclesProvider.Insert(mReceivedMessagesFromVehicles);

                                //Sega dodavam edno "A_" pred porakata, za Dispecerot da znae deka e za android

                                message.tFreeText.MessageText = "A_" + message.tFreeText.MessageText;
                            }

                            //Sega go pustam i na dispecer
                            long RetValue = VehiclesContainer.Instance.setReceivedFreeText(veh.IdVehicle, message.tFreeText);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Error ReceivedFreeText insert", ex);
                        }
                        break;

                }

                return 0;                
            }

            catch (Exception e)
            {
                log.Error("ERROR ", e);
                return -1;
            }

        }




        Vehicle FindVehicle(string mCompanyFirstLetter, string mVehicleNumber)
        {
            Vehicle retVal = null;
            long mIdCompany = 0;

            switch (mCompanyFirstLetter)
            {
                case "N":
                    mIdCompany = 1;
                    break;

                case "L":
                    mIdCompany = 7;
                    break;

                case "S":
                    mIdCompany = 8;
                    break;

                default:
                    break;
            }

            TList<Vehicle> mlstVehicle = VehiclesContainer.Instance.getAllForCompany(mIdCompany);


            if (mlstVehicle != null && mlstVehicle.Count > 0)
            {
                foreach (Vehicle tmpV in mlstVehicle)
                {
                    //log.Debug("-" + tmpV.DescriptionLong.Trim() + "-" + mVehicleNumber.Trim() + "-");

                    if (tmpV.DescriptionLong.Trim() == mVehicleNumber.Trim())
                        retVal = tmpV;
                }
            }

            else
            {
                log.Debug("NE NAJDOV VOZILO ZA: " + mCompanyFirstLetter + "---" + mVehicleNumber);
            }

            return retVal;
        }




        byte[] CreateStatusInfoForVehicle(Vehicle mVehicleForInfo, Vehicle mVehicleForSend)
        {
            String mInfoMessage = "";
            byte[] retVal = null;

            try
            {
                mInfoMessage = mInfoMessage + mVehicleForInfo.DescriptionShort + " / ";

                mInfoMessage = mInfoMessage + mVehicleForInfo.currentStateString + " / ";

                //Време на последен таксиметар
                if (mVehicleForInfo.TaximetarLast != null && mVehicleForInfo.TaximetarLast != DateTime.Parse("01.01.0001 00:00:00"))
                {
                    TimeSpan interval = DateTime.Now.Subtract(mVehicleForInfo.TaximetarLast);

                    mInfoMessage = mInfoMessage + "Таксиметар:" + interval.Hours.ToString("0") + ":" + interval.Minutes.ToString("00") + ":" + interval.Seconds.ToString("00") + " / ";
                }

                // Tekoven region
                mInfoMessage = mInfoMessage + "Регион:" + mVehicleForInfo.currentGPSData.RegionName;


            }
            catch (Exception ex)
            {
                log.Error("CreateStatusInfoForVehicle", ex);
            }

            if (mInfoMessage != "")
            {
                //log.Debug("mInfoMessage : " + mInfoMessage);

                clsMessageCreator mMessageCreator = new clsMessageCreator();
                retVal = mMessageCreator.CreateNewPopUpMessageForLCD(mVehicleForSend, mInfoMessage, '4');

            }
            else
            {
                log.Debug("mInfoMessage = praznoo");
            }

            return retVal;

        }





        ////byte[] CreateDistanceInfoForVehicle(Vehicle mVehicleForDistance, long mDistance)
        ////{
        ////    String mInfoMessage = "";
        ////    byte[] retVal = null;

        ////    try
        ////    {
        ////        TList<Vehicle> tmpVehiclesList = new TList<Vehicle>(VehiclesContainer.Instance.getAllVehicles());

        ////        foreach (Vehicle tmpVehicle in tmpVehiclesList)
        ////        {
        ////            if (tmpVehicle.IdVehicle != mVehicleForDistance.IdVehicle)
        ////            {
        ////                tmpVehicle.DistanceToAddress = (long)(VehiclesContainer.Calc(mVehicleForDistance.currentGPSData.Latutude_Y, mVehicleForDistance.currentGPSData.Longitude_X, tmpVehicle.currentGPSData.Latutude_Y, tmpVehicle.currentGPSData.Longitude_X));
        ////            }
        ////        }
                
        ////        tmpVehiclesList.Sort("DistanceToAddress");

        ////        foreach (Vehicle tmpVehicle in tmpVehiclesList)
        ////            if (tmpVehicle.IdVehicle != mVehicleForDistance.IdVehicle && tmpVehicle.DistanceToAddress <= mDistance)
        ////                mInfoMessage = mInfoMessage + tmpVehicle.DescriptionShort + "(" + tmpVehicle.DistanceToAddress.ToString() + "), ";

        ////        clsMessageCreator mMessageCreator = new clsMessageCreator();

        ////        if(mInfoMessage != "")              
        ////            retVal = mMessageCreator.CreatePopUpMessageForLCD(mVehicleForDistance, mInfoMessage);
        ////        else
        ////            retVal = mMessageCreator.CreatePopUpMessageForLCD(mVehicleForDistance, "Nema vozila vo radius od " + mDistance.ToString() + " metri!");

        ////    }
        ////    catch (Exception ex)
        ////    {
        ////        log.Error("Greska vo CreateDistanceInfoForVehicle!!! ", ex);
        ////    }
           
        ////    return retVal;

        ////}

        #endregion
    }
}
