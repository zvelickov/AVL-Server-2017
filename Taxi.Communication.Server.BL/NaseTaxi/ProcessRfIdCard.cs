using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
//using JP.Data.Utils;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.BL.NaseTaxi
{
    public class ProcessRfIdCard : IBL
    {
        #region IBL Members

        public ParserResponseContainer processBL(ParserResponseContainer message, long IdVehicle)
        {
            try
            {
                if (message.tLocation == null)
                    return message;


                if (((message.tLocation.RfIdCard == null) || (message.tLocation.RfIdCard == "0000000000"))
                    && (VehiclesContainer.Instance.setRiminderForRfId(IdVehicle) == true))
                {
                    clsMessageCreator remMsg = new clsMessageCreator();
                    message.addToNewMsgToUnit(remMsg.CreateNewPopUpMessageForLCD(VehiclesContainer.Instance.getSingleVehicle(IdVehicle), "Stavete ja Klient kartickata povtorno", '4'));
                    return message;
                }

                if (message.tLocation.RfIdCard == null)
                    return message;

                if (message.tLocation.RfIdCard == "0000000000")
                    return message;

                Vehicle veh = VehiclesContainer.Instance.getSingleVehicle(IdVehicle);

                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.GetBySerialNumber(message.tLocation.RfIdCard);

                if ((cards == null) || (cards.Count == 0))
                {
                    clsMessageCreator tMessageCreator = new clsMessageCreator();
                    Vehicle tmpVeh = VehiclesContainer.Instance.getSingleVehicle(IdVehicle);

                    message.addToNewMsgToUnit(tMessageCreator.CreateNewPopUpMessageForLCD(tmpVeh, "Nevalidna karticka", '4'));

                    return message;
                }

                TList<Driver> drivers = DataRepository.DriverProvider.GetByIDRfIdCard(cards[0].IdRfIdCard);

                if ((drivers == null) || (drivers.Count == 0))
                {
                    if ((veh.CurrentRfIdCard != null) && (veh.CurrentRfIdCard.IdRfIdCard != cards[0].IdRfIdCard) && (VehiclesContainer.Instance.setRiminderForRfId(IdVehicle) == true))
                    {
                        clsMessageCreator remMsg = new clsMessageCreator();

                        Vehicle tmpVeh = VehiclesContainer.Instance.getSingleVehicle(IdVehicle);

                        message.addToNewMsgToUnit(remMsg.CreateNewPopUpMessageForLCD(tmpVeh, "Stavete ja Klient kartickata povtorno", '4'));
                        
                        return message;
                    }

                    return processLoyalty(message, cards[0], veh);
                }
                else
                {
                    return message;
                    //return processDriver(message, cards[0], veh);
                }
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("Loyalty", e);
            }
            return message;
        }

        private ParserResponseContainer processLoyalty(ParserResponseContainer message, RfIdCards card, Vehicle veh)
        {
            try
            {
                //TList<RfIdCards> card = DataRepository.RfIdCardsProvider.GetBySerialNumberRfIdCard(message.tLocation.RfIdCard);

                if (card == null)
                    return message;



                clsMessageCreator tMessageCreator = new clsMessageCreator();

                

                if ( (veh.CurrentRfIdCard == null) || (veh.CurrentRfIdCard.IdRfIdCard != card.IdRfIdCard))
                {
                    TList<RegisteredPassengers> selPass = DataRepository.RegisteredPassengersProvider.GetByIdRfIdCard(card.IdRfIdCard);
                    if ((selPass == null) || (selPass.Count == 0))
                    {
                        return message;
                    }

                    long succ = VehiclesContainer.Instance.setRfIdCardPassinger(veh.IdVehicle, card);

                    StringBuilder strBuild = new StringBuilder();

                    if (selPass[0].IdClient == null)
                    {
                        strBuild.Append("Promo KM+\r\n");
                    }

                    else
                    {
                        Clients tmpClient = DataRepository.ClientsProvider.GetByIdClient((long)selPass[0].IdClient);

                        if (tmpClient != null)
                            strBuild.Append(tmpClient.Name.Trim());
                    }

                    
                    strBuild.Append(selPass[0].LastName);
                    strBuild.Append(" ");
                    strBuild.Append(selPass[0].Name);
                    strBuild.Append("\r\n");
                    strBuild.Append(selPass[0].CurrentNumberOfReceipts);
                    strBuild.Append("/");
                    int hours = (int)selPass[0].CurrentDuration / 3600;
                    decimal rem = selPass[0].CurrentDuration % 3600;
                    int min = (int) rem / 60;
                    int sec = (int)rem % 60;
                    strBuild.Append(hours);
                    strBuild.Append(":");
                    strBuild.Append(min);
                    strBuild.Append(":");
                    strBuild.Append(sec);

                    strBuild.Append("/");
                    if (selPass[0].CurrentAmount >= 40)
                    {
                        strBuild.Append((int)(selPass[0].CurrentAmount - 40) / 25);
                    }
                    else
                    {
                        strBuild.Append("0");
                    }
                    strBuild.Append("\r\n");

                    if ((selPass[0].Invoice != null) && (selPass[0].Invoice == true))
                    {
                        strBuild.Append("Faktura");
                    }
                    else
                    {
                        strBuild.Append("Gotovina");
                    }

                    if (selPass[0].IsDeleted)
                    {
                        strBuild = new StringBuilder();
                        strBuild.Append("Nevalidna karticka!");
                    }

                    if (selPass[0].TemporaryDiscarded == true)
                    {
                        strBuild = new StringBuilder();
                        strBuild.Append("Izgubena karticka!");
                    }

                    if (strBuild.Length > 80)
                    {
                        strBuild.Remove(80, strBuild.Length - 80);
                    }

                    if (succ == 0 || succ == 1)
                    {
                            message.addToNewMsgToUnit(tMessageCreator.CreateNewPopUpMessageForLCD(veh, strBuild.ToString(), '4'));                                                    
                    }

                    if (succ == -1)
                    {
                            message.addToNewMsgToUnit(tMessageCreator.CreateNewPopUpMessageForLCD(veh, "Client-ot ne e prifaten!", '4'));
                    }
                }



            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(BLFactory.LOG_NAME).Error("Loyalty", e);
            }

            return message;
        }


        public static byte[] CheckForCheckInDriverShift(SensorData pSensorData, Vehicle m_myVehicle)
        {
            byte[] retVal = null;


            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();

            string m_retVal = RfIdCardProcessor.ProcessCheckInDriver(pSensorData.RfIdCard, m_myVehicle);

            if (m_retVal != "")
            {
                    retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');
            }

            return retVal;
        }

        

        #endregion
    }
}
