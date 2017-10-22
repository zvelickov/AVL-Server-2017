using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
using System.Globalization;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    public class FiscalLoyalty : IBL
    {

        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message, long IdVehicle)
        {

            if (message.tFiscal == null)
                return message;

            if (message.tFiscal.RfIdCard == null)
                return message;

            try
            {
                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.GetBySerialNumber(message.tFiscal.RfIdCard);
                if ((cards == null) || (cards.Count == 0))
                    return message;
                
                if (cards.Count > 1)
                {
                    log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Warn("More then one card for serial " + message.tFiscal.RfIdCard);
                }


                TList<RegisteredPassengers> passingers = DataRepository.RegisteredPassengersProvider.GetByIdRfIdCard(cards[0].IdRfIdCard);
                if ((passingers == null) || (passingers.Count == 0))
                    return message;

                if (passingers.Count > 1)
                {
                    log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Warn("More then one passingers for serial " + message.tFiscal.RfIdCard);
                }

                //parsing
                StringTokenizer sTok = new StringTokenizer(message.tFiscal.Duration, "'");
                decimal duration = 0;
                if (sTok.Count == 2)
                {
                    duration += Decimal.Parse(sTok[0]) * 60;
                    duration += Decimal.Parse(sTok[1].Substring(0,2));
                }
                IFormatProvider culture = new CultureInfo("mk-MK", true);

                passingers[0].CurrentNumberOfReceipts++;
                passingers[0].CurrentAmount += Decimal.Parse(message.tFiscal.Money, culture);
                passingers[0].CurrentDuration += duration;
                DataRepository.RegisteredPassengersProvider.Update(passingers[0]);
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("FiscalLoyalty", e);
            }

            return message;
        }

        #endregion
    }
}
