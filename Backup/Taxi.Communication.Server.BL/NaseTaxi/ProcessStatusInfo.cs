using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
using System.Globalization;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    class ProcessStatusInfo : IBL
    {       
        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message, long IdVehicle)
        {

            if (message.tBaranjeStatus == null)
                return message;


            try
            {

                switch (message.tBaranjeStatus.KodNaBaranje)
                {

                    case 1: // Baranje info za status na vozilo (svoe ili drugo)

                        Vehicle veh1 = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                        string retValStr1 = "";
                        byte[] retValByte1 = null;

                        retValStr1 = GenerateInfoForVehicle(veh1, message.tBaranjeStatus.TextZaBaranje);

                        clsMessageCreator tmpclsMessageCreator1 = new clsMessageCreator();
                        
                        retValByte1 = tmpclsMessageCreator1.CreateNewPopUpMessageForLCD(veh1, retValStr1, '4');

                        message.addToNewMsgToUnit(retValByte1);

                        break;

                    case 2:     // Prijava za Region (moze da ja dade samo vo status Busy)

                        Vehicle veh2 = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                        Console.WriteLine("Prijava za region: " + message.tBaranjeStatus.Vrednost.ToString() + ", za vozilo: " + veh2.Plate);

                        break;


                    case 3:     // Minuti za prodolzuvanje vreme (samo vo sostojbi Move2Client i WaitClient)

                        Vehicle veh3 = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                        Console.WriteLine("Prodolzuvanje na vreme (sec): " + message.tBaranjeStatus.Vrednost.ToString() + ", za vozilo: " + veh3.Plate);

                        break; 

                    case 4:

                        Vehicle veh4 = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                        string retValStr4 = "";
                        byte[] retValByte4 = null;

                        retValStr4 = GenerateRegionInfoForAllVehicles();

                        clsMessageCreator tmpclsMessageCreator4 = new clsMessageCreator();

                        retValByte4 = tmpclsMessageCreator4.CreateNewPopUpMessageForLCD(veh4, retValStr4, '4');

                        message.addToNewMsgToUnit(retValByte4);                       
                        
                        break;

                    case 5:     // Otkazuvanje na poracka

                        Vehicle veh5 = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                        Console.WriteLine("Otkazuvanje naracka za vozilo: " + veh5.Plate);

                        break; 
                    
                    default:
                        break; 
                }

            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("ProcessStatusInfo", e);
            }

            return message;
        }

        #endregion




        // ZORAN:   Ova e zamena za pogornata f-ja. Se napravi po integracija na 3-te kompanii
        //          private string GenerateRegionInfoForAllVehicles(long pIdCompany)
        private string GenerateRegionInfoForAllVehicles()
        {
            string retVal = "";

            TList<Vehicle> tmpListVehicles = null;

            tmpListVehicles = new TList<Vehicle>(VehiclesContainer.Instance.getAllVehicles());

            if (tmpListVehicles != null)
            {
                TList<GisRegions> lstGisRegions = null;

                //lstGisRegions = DataRepository.GisRegionsProvider.GetByIdCompany(pIdCompany);
                lstGisRegions = DataRepository.GisRegionsProvider.GetByIdCompany(1);

                if (lstGisRegions != null)
                {
                    List<int> lstNumberOfNaseTaxiVehiclesPerRegion = new List<int>();
                    List<int> lstNumberOfLotusVehiclesPerRegion = new List<int>();
                    List<int> lstNumberOfSonceVehiclesPerRegion = new List<int>();

                    foreach (GisRegions tmpGisRegion in lstGisRegions)
                    {
                        lstNumberOfNaseTaxiVehiclesPerRegion.Add(0);
                        lstNumberOfLotusVehiclesPerRegion.Add(0);
                        lstNumberOfSonceVehiclesPerRegion.Add(0);
                    }

                    for (int i = 0; i < lstGisRegions.Count - 1; i++)
                    {
                        foreach (Vehicle tmpVhc in tmpListVehicles)
                        {
                            if (tmpVhc.currentGPSData.IdRegion == lstGisRegions[i].IdRegion && tmpVhc.currentStateString == "StateIdle")
                            {
                                if (tmpVhc.IdCompany == 1)
                                    lstNumberOfNaseTaxiVehiclesPerRegion[i] = lstNumberOfNaseTaxiVehiclesPerRegion[i] + 1;

                                if (tmpVhc.IdCompany == 7)
                                    lstNumberOfLotusVehiclesPerRegion[i] = lstNumberOfLotusVehiclesPerRegion[i] + 1;

                                if (tmpVhc.IdCompany == 8)
                                    lstNumberOfSonceVehiclesPerRegion[i] = lstNumberOfSonceVehiclesPerRegion[i] + 1;
                            }
                        }
                    }


                    //ZORAN:    Sega go generiram stringot za prikazuvanje na LCD
                    //          

                    int n = 1;

                    for (int i = 0; i < lstGisRegions.Count - 1; i++)
                    {
                        retVal = retVal + lstGisRegions[i].IdRegion.ToString("0000") + "/" +
                                    lstNumberOfNaseTaxiVehiclesPerRegion[i].ToString("00") + "/" +
                                    lstNumberOfLotusVehiclesPerRegion[i].ToString("00") + "/" +
                                    lstNumberOfSonceVehiclesPerRegion[i].ToString("00");


                        if (n % 3 == 0)
                        {
                            retVal = retVal + System.Environment.NewLine;
                        }
                        else
                        {
                            retVal = retVal + "  ";
                        }

                        n++;

                    }
                }
            }

            return retVal;
        }




        private string GenerateInfoForVehicle(Vehicle pVehicle, string pTextZaBaranje)
        {

           
            //          Ako vozacot prati "AF0", da dobie info za svoeto vozilo
            //          Ako prati "AFXnekoj-broj", da go dobie info-to za toa vozilo i toa:
            //                  X = ""...za negovata kola
            //                  X = N...za kola od Nase
            //                  X = L...za Lotus
            //                  X = S...za Sonce

           string mInfoMessage = null;
            Vehicle mVehicleForInfo = null;

            pTextZaBaranje = pTextZaBaranje.Trim().ToUpper();

            if (pTextZaBaranje == "AF0")
            {
                mInfoMessage = CreateStatusInfoForVehicle(pVehicle, pVehicle);
            }

            if (pTextZaBaranje.Length > 3)
            {
                string mCompanyFirstLetter = pTextZaBaranje.Substring(2, 1).ToUpper();

                if (mCompanyFirstLetter == "N" || mCompanyFirstLetter == "L" || mCompanyFirstLetter == "S")
                {
                    mVehicleForInfo = FindVehicle(mCompanyFirstLetter, pTextZaBaranje.Substring(3));

                    if (mVehicleForInfo != null)
                    {
                        mInfoMessage = CreateStatusInfoForVehicle(mVehicleForInfo, pVehicle);
                    }
                }

                else
                {
                    mInfoMessage = "Gresna komanda!";
                }
            }

            return mInfoMessage;
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
                //log.Debug("NE NAJDOV VOZILO ZA: " + mCompanyFirstLetter + "---" + mVehicleNumber);
            }

            return retVal;
        }




        private string CreateStatusInfoForVehicle(Vehicle mVehicleForInfo, Vehicle mVehicleForSend)
        {
            String mInfoMessage = "";

            try
            {
                mInfoMessage = mInfoMessage + mVehicleForInfo.DescriptionShort + " / ";

                mInfoMessage = mInfoMessage + mVehicleForInfo.currentStateString + " / ";

                //Âðåìå íà ïîñëåäåí òàêñèìåòàð
                if (mVehicleForInfo.TaximetarLast != null && mVehicleForInfo.TaximetarLast != DateTime.Parse("01.01.0001 00:00:00"))
                {
                    TimeSpan interval = DateTime.Now.Subtract(mVehicleForInfo.TaximetarLast);

                    mInfoMessage = mInfoMessage + "Taksimetar:" + interval.Hours.ToString("0") + ":" + interval.Minutes.ToString("00") + ":" + interval.Seconds.ToString("00") + " / ";
                }

                // Tekoven region
                mInfoMessage = mInfoMessage + "Region:" + mVehicleForInfo.currentGPSData.RegionName;


            }
            catch (Exception ex)
            {
                //log.Error("CreateStatusInfoForVehicle", ex);
            }

            return mInfoMessage;

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

        ////        if (mInfoMessage != "")
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
    }
}


