using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.ConnectionListeners;
using log4net;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Containers;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.MessageParsers.ControlElectronics.ExtraData;
//using JP.Data.Utils;
using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server.EnrichMessage
{
    class FiskalReceiptEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        public ILog log;

        public FiskalReceiptEnricher(GPSListener gpsListener)
        {
            _gpsListener = gpsListener;
            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tFiscal != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            try
            {

                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);
                long IdPhoneCall = VehiclesContainer.Instance.getIdPhoneCall(veh.IdVehicle);

                //TList<FiskalniExtraData> tFiskalniExtraData = new TList<FiskalniExtraData>();

                message.tFiscal.IdVehicle = veh.IdVehicle;
                message.tFiscal.IdUnit = (long)veh.IdUnit;

                if(veh.DriverShiftInOut != null)
                    message.tFiscal.Driver = veh.DriverShiftInOut.IdDriver.ToString();

                if (veh.CurrentRfIdCard != null)
                {
                    message.tFiscal.RfIdCard = veh.CurrentRfIdCard.SerialNumber;
                }

                if (IdPhoneCall != -1)
                {
                    TList<Orders> lstOrders = DataRepository.OrdersProvider.GetByIdPhoneCall(IdPhoneCall);
                    if ((lstOrders != null) && (lstOrders.Count == 1))
                    {
                        message.tFiscal.IdOrder = lstOrders[0].IdOrder;
                    }
                }


                if ( (veh.currentGPSData != null) && (veh.currentGPSData.IdLocation > 0))
                {
                    message.tFiscal.IdLocation = veh.currentGPSData.IdLocation;
                }
                else
                {
                    message.tFiscal.IdLocation = null;
                    //    log.Debug(" Vehicle.currentGPSData.IdLocation e 0 (nula) za vozilo " + veh.IdVehicle.ToString()); 
                }
                // Prvo potvrda do uredot deka porakata e primena

                if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                    log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                /*

                if (message.tFiscal.IDFiskalniType == 1)
                {
                }
                else if (message.tFiscal.IDFiskalniType == 2)
                {
                    FiskalniExtraData frExtraData = null;

                    FiscalReceiptAccentExtraData tFiscalReceiptAccentExtraData = (FiscalReceiptAccentExtraData)message.fiscalReceiptExtraData;

                    if (message.tFiscal.Tariff == "T1")
                    {
                        frExtraData =  new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_KM_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "KM_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);
                        
                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_MIN_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "MIN_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_PAID_METERS;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_PAID_METERS_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_PAID_SECONDES;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T1_PAID_SECONDES_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                    }
                    else if (message.tFiscal.Tariff == "T2")
                    {
                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_KM_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "KM_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_MIN_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "MIN_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_PAID_METERS;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_PAID_METERS_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_PAID_SECONDES;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T2_PAID_SECONDES_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);
                    }
                    else if (message.tFiscal.Tariff == "T3")
                    {
                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_KM_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "KM_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_MIN_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "MIN_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_PAID_METERS;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_PAID_METERS_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_METERS_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_PAID_SECONDES;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);

                        frExtraData = new FiskalniExtraData();
                        frExtraData.Value = tFiscalReceiptAccentExtraData.T3_PAID_SECONDES_PRICE;
                        frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "PAID_SECONDES_PRICE")[0].IDFiskalniCustomFileds;
                        tFiskalniExtraData.Add(frExtraData);
                    }

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.START_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "START_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.FIRST_EXTRA_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "FIRST_EXTRA_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.SECOND_EXTRA_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "SECOND_EXTRA_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.THIRD_EXTRA_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "THIRD_EXTRA_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.GLOBAL_DISCOUNT_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "GLOBAL_DISCOUNT_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.RECEIPT_DISCOUNT_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "RECEIPT_DISCOUNT_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.TAXA_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "TAXA_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.TAXB_PRICE;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "TAXB_PRICE")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                    frExtraData = new FiskalniExtraData();
                    frExtraData.Value = tFiscalReceiptAccentExtraData.DISTANCE_METERS;
                    frExtraData.IDFiskalniCustomFileds = DataRepository.FiskalniCustomFiledsProvider.GetByName(message.tFiscal.IDFiskalniType, "DISTANCE_METERS")[0].IDFiskalniCustomFileds;
                    tFiskalniExtraData.Add(frExtraData);

                }

                */

                // Vtoro: Zapis vo baza
                DataRepository.FiskalReceiptProvider.Insert(message.tFiscal);
                /*
                foreach (FiskalniExtraData frExtraData in tFiskalniExtraData)
                {
                    frExtraData.IDFiskalnReceipt = message.tFiscal.IDFiskalnReceipt;
                    DataRepository.FiskalniExtraDataProvider.Insert(frExtraData);
                }
                */
                return 0;
            }
            catch (Exception ex)
            {
                log.Error("Error FiskalReceipt ", ex);
                return -1;
            }
        }

        #endregion
    }
}
