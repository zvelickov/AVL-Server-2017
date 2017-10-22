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
    class ReceivedShortMessageEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        public ILog log;

        public ReceivedShortMessageEnricher(GPSListener gpsListener)
        {
            _gpsListener = gpsListener;
            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tShortMessage != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            try
            {

                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                message.tShortMessage.IdVehicle = veh.IdVehicle;

                ShortMessages oTmpShortMessage = DataRepository.ShortMessagesProvider.GetByIdShortMessage((long)message.tShortMessage.IdShortMessage);

                if (oTmpShortMessage != null)
                {
                    message.tShortMessage.IdShortMessageSource = oTmpShortMessage;

                    // Prvo potvrda do uredot deka porakata e primena
                    if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                        log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);

                    DataRepository.ReceivedShortMessageProvider.Insert(message.tShortMessage);

                    // Sega, ako e poraka za vreme, prvo proveruvam dali ima aktivna naracka od Android vo tabelaMobileOrders
                    // Ako ima, ja insertiram porakata vo ReceivedMessagesFromVehicles
                    if (message.tShortMessage.IdShortMessage >= 1001 && message.tShortMessage.IdShortMessage <= 1030)
                    {
                        TList<MobileOrders> lst_MobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus(veh.IdVehicle, 2);

                        if (lst_MobileOrders != null && lst_MobileOrders.Count > 0)
                        {
                            ReceivedMessagesFromVehicles mReceivedMessagesFromVehicles = new ReceivedMessagesFromVehicles();

                            mReceivedMessagesFromVehicles.IdOrders = lst_MobileOrders[0].IdOrder;
                            mReceivedMessagesFromVehicles.Imei = lst_MobileOrders[0].Imei;
                            mReceivedMessagesFromVehicles.DateTimeReceived = System.DateTime.Now;
                            mReceivedMessagesFromVehicles.MessageText = oTmpShortMessage.MessageText;

                            DataRepository.ReceivedMessagesFromVehiclesProvider.Insert(mReceivedMessagesFromVehicles);
                        }
                    }

                    //          TODO (Pajo) da se "zakaci" porakata na soodvetnoto Vozilo, na property-to LastReceivedShortMessage
                    //          I da go prijavi voziloto za "promeneto", za da se preprati do dispecerite
                    //

                    VehiclesContainer.Instance.setLastMessage(veh.IdVehicle, message.tShortMessage);

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
    }
}
