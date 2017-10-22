using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;
using Taxi.Communication.Server.Containers;
using log4net;
using Taxi.Communication.Server.ConnectionListeners;
using Taxi.Communication.Server.StateMachine;
using Taxi.Communication.Server.Utils.Parsers;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.EnrichMessage
{
    class SendToUnitMessageEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        public ILog log;

        public SendToUnitMessageEnricher(GPSListener gpsListener)
        {
            _gpsListener = gpsListener;
            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tSendToUnitMsg != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            try
            {

                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                string retValStr = "";
                byte[] retValByte = null;

                retValStr = GenerateRegionInfoForAllVehicles();

                clsMessageCreator tmpclsMessageCreator = new clsMessageCreator();

                retValByte = tmpclsMessageCreator.CreateInfoForVehiclesPerRegion(veh, retValStr);

                if (retValByte != null)
                {
                    // Prvo potvrda do uredot deka porakata e primena
                    if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                        log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);


                    // Vtoro, prakanje na poraka za BrojNaVozilaPoRegion

                    if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, retValByte) == -1)
                        log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);

                    return 0;
                }
                else
                {
                    return -1;
                }


            }
            catch (Exception e)
            {
                log.Error("ERROR ", e);
                return -1;
            }

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
    }
}
