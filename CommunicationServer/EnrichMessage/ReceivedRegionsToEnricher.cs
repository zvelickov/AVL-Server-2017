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

namespace Taxi.Communication.Server.EnrichMessage
{
    class ReceivedRegionsToEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        public ILog log;

        public ReceivedRegionsToEnricher(GPSListener gpsListener)
        {
            _gpsListener = gpsListener;
            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tRegionsTo != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            try
            {
                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                message.tRegionsTo.IdVehicle = veh.IdVehicle;

                // Vtoro:   Proverka dali za dadeniot ID postoi realno region.
                //          Ako ne postoi, isto kako nisto da ne pristignalo, samo se evidentira vo log

                GisRegions oGisRegions = DataRepository.GisRegionsProvider.GetByIdRegion(message.tRegionsTo.IdRegion);

                if (oGisRegions != null)
                {


                    // Prvo potvrda do uredot deka porakata e primena

                    if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                        log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);


                    // Vtoro: zapis vo baza

                    DataRepository.ReceivedRegionsToProvider.Insert(message.tRegionsTo);

                    // Treto:   Da go "zakacam" prijaveniot Region na Voziloto

                    long tmpRetVal = VehiclesContainer.Instance.setRegionTo(veh.IdVehicle, message.tRegionsTo.IdRegion);

                    if (tmpRetVal < 0)
                    {
                        log.Error("Pogresno pratena sifra za oRegionTo (poraka PP56): " + veh.IdVehicle.ToString() + message.tRegionsTo.IdRegion.ToString());
                    }


                    return 0;
                }
                else
                {
                    log.Error("Ne e pronajden GisRegion za konkretniot IDREgion (PP56) !!!");
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
    }
}
