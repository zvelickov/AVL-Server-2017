using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
using JP.Data.Utils;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    public class LoyaltyProgram : IBL
    {
        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message, Vehicle veh)
        {
            if (message.tLocation == null)
                return message;


            if (message.tLocation.RfIdCard == null)
                return message;


            try
            {
                TList<RfIdCards> card = DataRepository.RfIdCardsProvider.GetBySerialNumberRfIdCard(message.tLocation.RfIdCard);

                if (card == null)
                    return message;

                clsMessageCreator tMessageCreator = new clsMessageCreator();

                if (veh.CurrentRfIdCard == null)
                {
                    veh.CurrentRfIdCard = card[0];
                    TList<RegisteredPassengers> selPass = DataRepository.RegisteredPassengersProvider.GetByIDRfIdCard(card[0].IDRfIdCard);
                    if ((selPass == null) || (selPass.Count == 0))
                        return message;

                    message.addToNewMsgToUnit(tMessageCreator.CreatePopUpMessageForLCD(veh, "TEST"));
                }

                

            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("Loyalty", e);
            }

            return message;
        }

        #endregion
    }
}
