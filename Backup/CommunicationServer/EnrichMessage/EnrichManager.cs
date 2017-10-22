using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using Taxi.Communication.Server.ConnectionListeners;
using Taxi.Communication.Server.Utils;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Containers;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.EnrichMessage
{
    public class EnrichManager
    {
        private List<IEnrichMessageHandler> _enrichWorkers;
        private GPSListener _gpsListener = null;

        public EnrichManager(GPSListener gpsListener, MapUtils mapUtils)
        {
            _gpsListener = gpsListener;

            _enrichWorkers = new List<IEnrichMessageHandler>();

            _enrichWorkers.Add(new LocationEnricher(gpsListener, mapUtils));
            _enrichWorkers.Add(new FiskalReceiptEnricher(gpsListener));
            //_enrichWorkers.Add(new ConfirmMessageEnricher());
            _enrichWorkers.Add(new ReceivedFreeTextEnricher(gpsListener));
            _enrichWorkers.Add(new ReceivedRegionsToEnricher(gpsListener));
            _enrichWorkers.Add(new ReceivedShortMessageEnricher(gpsListener));
            _enrichWorkers.Add(new SendToUnitMessageEnricher(gpsListener));
            //_enrichWorkers.Add(new GPIOEnricher(gpsListener));

        }

        public long doEnrichMessage(ParserResponseContainer message)
        {
            long retVal = 0;
            bool bPronajdenEnricher = false;

            foreach (IEnrichMessageHandler enricher in _enrichWorkers)
            {
                if (enricher.canHandle(message))
                {
                    retVal = enricher.enrichMessage(message);
                    bPronajdenEnricher = true;
                }
            }

            // ZORAN:   Bidejki prakanje potvrda za primena poraka e vo ENRICH-erot, 
            //          tuka ja prakam potvrdata za porakite za koi nema Enrich-er
            if (!bPronajdenEnricher)
            {
                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                {

                    Console.WriteLine("GRESKA vo prakanje potvrda za poraka: " + message.msg.Command);
                }
                else
                {
                    //Console.WriteLine("Pratena potvrda za poraka: " + message.msg.Command);
                }

               

            }
            return retVal;
        }
    }
}
