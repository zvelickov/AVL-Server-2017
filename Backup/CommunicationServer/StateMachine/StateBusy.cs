using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using System.Configuration;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Data;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateBusy : VehicleState
    {
        private bool sendRemind = true;

        private long m_SecInCurrentState = 0;

        private int mSecondsForMarketingPopUp = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateBusy(Vehicle vehicle)
            : base(vehicle)
        {

            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            
            this.m_myVehicle.IDLastPhoneCall = 0;

            m_SecInCurrentState = 0;

            try
            {
                mSecondsForMarketingPopUp = int.Parse(ConfigurationManager.AppSettings["SecondsForMarketingPopUp"]);
            }
            catch (Exception ex)
            {
                mSecondsForMarketingPopUp = 0;
            } 

            //this.SensorForLogging = int.Parse(ConfigurationManager.AppSettings["SensorForLogging"]);

            //  Se da zatvoram ako ima otvoren ORDER od Android
            TList<MobileOrders> lstMobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus((long)vehicle.IdVehicle, 2);

            if (lstMobileOrders != null && lstMobileOrders.Count > 0)
            {
                lstMobileOrders[0].IdMobileOrderStatus = 3;
                lstMobileOrders[0].Comment = "Busy (" + lstMobileOrders.Count.ToString() + ")";
                DataRepository.MobileOrdersProvider.Update(lstMobileOrders[0]);
            }

            this.bStateChanged = true;
        }

       

        public int SensorForLogging;

        public override int releaseSecondCall()
        {
            return -1;
        }

        public override long acceptedFromClient()
        {
            return -1;
        }

        public override bool IsVehicleEligableForCall()
        {
            if (m_myVehicle.NextPhoneCall == null && m_myVehicle.CurrentPhoneCall != null && m_myVehicle.CurrentPhoneCall.oAddressTo != null && m_myVehicle.CurrentPhoneCall.oAddressTo.oGisRegions != null)
            {               
                return true;                                  
            }
            else
            {
                return false;
            }
        }

        protected StateBusy()
            : base(null)
        {
        }
        #region IVehicleState Members
        public override VehicleState Copy()
        {
            return new StateBusy();
        }
        public override int IDCurrentState()
        {
            return 7;
        }

        public override long reserveVehicle()
        {

            if (m_myVehicle.NextPhoneCall == null)
            {
                m_myVehicle.currentState = new StateBusyNextPhoneCall(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return 0;
            }
            else
            {
                //throw new Exception("The method or operation is not implemented.");
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "reserveVehicle", "");
                return -1;
            }
        }

        public override long releaseVehicle()
        {
            m_myVehicle.CurrentPhoneCall = null;
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "VoiceInitiate", "");
            return -1;
        }

        public override long setClientRfIdCard(RfIdCards ID_Driver)
        {
            return 0;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {

            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "SendAddress", "");
            return null;           
        }

        public override long CancellRequestFromClient()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "CancellRequestFromClient", "");
            return -1;
        }

        public override long ExtendWaitClientTime()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusy", "ExtendWaitClientTime", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();

            m_SecInCurrentState += 1;



            //ZORAN:    Treto:  da pratam pop-up poraka do voziloto za popusti, i toa vo slednite slucai:
            //                  - Na 5 sec posle uklucuvanje na taximetar
            //                  - Na sekoi 2 min koga e vo StateBusy i koga ima kluc vo brava (Sensor1 = 1)
            //                  - Do 10-ta min, potoa ne e bitno                
            //                  - Ovde ke ima maka vo situacii kade sto se odjavuva od taksimetar, no ... vo idnina..
            //     

            //if (mSecondsForMarketingPopUp == 0)
            //    return retVal;

            //if (this.m_myVehicle.IdCompany == 8)
            //    return retVal;

            //if (this.m_myVehicle.IdCompany == 1)
            //    return retVal;

            //if (this.m_myVehicle.IdCompany == 7)
            //    return retVal;

            

            //if ((m_SecInCurrentState < 600 && (m_SecInCurrentState % mSecondsForMarketingPopUp == 0)) || (m_SecInCurrentState == 5))
            //{
                
            //        string mMessageToVehicle;

            //        if (this.m_myVehicle.IdCompany == 1)
            //            mMessageToVehicle = "Za sekoe gotovinsko plakjanje Vi sleduva 20 denari popust!";
            //        else
            //            mMessageToVehicle = "Za sekoe gotovinsko plakjane Vi sleduva popust!";
                    
            //        retVal.Add (mMessageCreator.CreatePopUpMessageForLCD(this.m_myVehicle, mMessageToVehicle));
            //}

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 30, false));
                
                mMessageCreator = null;
            }

           

            return retVal;
        }


        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;

            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();


            //PROC08 Proveruvam Senzor Taximeter, Ako Isklucil -> Idle
            if (m_myVehicle.currentSensorData.Senzor_8 == 1)
            {
                // ZORAN (29.06.2011):  Ako ima CurrentPhoneCall, ID_to na PhoneCall go stavam vo m_myVehicle.IDLastPhoneCall
                //                      Ova mi treba za da ja 'vrzam" pokasno fiskalnata smetka

                if (m_myVehicle.CurrentPhoneCall != null)
                {
                    m_myVehicle.IDLastPhoneCall = m_myVehicle.CurrentPhoneCall.IdPhoneCall;
                }

                if ((m_myVehicle.NextPhoneCall != null))
                {
                    m_myVehicle.CurrentPhoneCall = m_myVehicle.NextPhoneCall;
                    m_myVehicle.NextPhoneCall = null;
                    m_myVehicle.currentState = new StateMoveToClient(m_myVehicle);
                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                    m_myVehicle.StateChanged = true;
                }
                else
                {
                    //Ako e Isklucen -> Idle

                    //m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
                    m_myVehicle.currentState = new StateFiscalBeforeIdle(this.m_myVehicle);
                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                    m_myVehicle.StateChanged = true;
                    //PAZI, Tuka stavam Koga isklucil Taksimetar
                    m_myVehicle.TaximetarLast = DateTime.Now;
                }
            }


            
            if (retVal == null)
            {

                //Dali e vklucen Senzor za Kraj na smena (radi odjava na vozac)

                if (m_myVehicle.currentSensorData.Senzor_6 == 1)
                {

                    //  ZORAN:      Proveruvam: 
                    //                  1. Ako ima RfIdCard na VOZAC, da go ODJAVI i da vrati zeleno svetlo
                    //                  2. Ne pravam nisto ako RfIdCard == "0000000000"
                    //                  3. Vraka prazen byte[0] ako ne najde nesto...
                    //                  

                    if (m_myVehicle.currentSensorData.RfIdCard != "0000000000")
                        retVal = CheckForCheckOutDriverShift(m_myVehicle.currentSensorData);
                    /*
                    if (retVal != null)
                    {
                        //Sekako go prefrlam vo StateShiftEnded
                        m_myVehicle.currentState = new StateShiftEnded(this.m_myVehicle);
                        m_myVehicle.StateChanged = true;
                    }
                    */ 
                     
                    return retVal;
                }
                else
                {
                    //  ZORAN:      Proveruvam: 
                    //                  1. Ako ima RfIdCard na VOZAC, da go ODJAVI i da vrati zeleno svetlo
                    //                  2. Ne pravam nisto ako RfIdCard == "0000000000"
                    //                  3. Vraka prazen byte[0] ako ne najde nesto...
                    //                  

                    if (m_myVehicle.currentSensorData.RfIdCard != "0000000000")
                        retVal = Taxi.Communication.Server.BL.NaseTaxi.ProcessRfIdCard.CheckForCheckInDriverShift(m_myVehicle.currentSensorData, m_myVehicle);
                    /*
                    if (retVal != null)
                    {
                        m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
                        m_myVehicle.StateChanged = true;
                    }
                    */
                    //Tuka ne menuvam State

                    // ZORAN:   Ako e logiran vozacot 
                    return retVal;
                }
            }
            else
            {
                return retVal;
            }
            
        }

        public override byte[] UpdateGPSData(GPSData myGPSData)
        {
            m_myVehicle.currentGPSData = myGPSData;

            clsMessageCreator tMessageCreator = new clsMessageCreator();
            byte[] retValForStation = null;

            if (m_myVehicle.currentGPSData != null)
            {
                if (m_myVehicle.PreviousStationState != m_myVehicle.Station)
                {

                    if (m_myVehicle.PreviousStationState == false)
                    {
                        //retValForStation = tMessageCreator.CreateStationStatus(m_myVehicle, 1);
                        //m_myVehicle.OnStationFromDateTime = DateTime.Now;
                        //m_myVehicle.currentGPSData.PreviousStationState = true;
                    }
                    else
                    {
                        retValForStation = tMessageCreator.CreateStationStatus(m_myVehicle, 0);
                        m_myVehicle.OnStationFromDateTime = DateTime.MinValue;
                        m_myVehicle.PreviousStationState = false;
                    }
                }

            }

            return retValForStation;
        }

        private byte[] CheckForCheckOutDriverShift(SensorData pSensorData)
        {
            byte[] retVal = null;
            byte[] retValForName = null;


            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();

            string m_retVal = RfIdCardProcessor.ProcessCheckOutDriver(pSensorData.RfIdCard, m_myVehicle);

            if (m_retVal != "")
            {

                retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');

            }


            return retVal;
        }

        public override bool setRiminderForRfId()
        {
            bool retVal = false;

            if (this.m_myVehicle.CurrentRfIdCard == null)
            {
                this.sendRemind = false;
                retVal = false;
            }



            if (this.sendRemind)
            {
                this.m_myVehicle.CurrentRfIdCard = null;
                this.sendRemind = false;
                retVal = true;
            }
            return retVal;
        }

        public override long setDriverShift(long ID_Driver)
        {
            //ZORAN: Ova sega ne mi treba, no neka ostane kako traga..
            //this.m_myVehicle.IdDriverShift = ID_Driver;
            return 0;
        }
        public override long forceToUndefined(long ID_user)
        {
            m_myVehicle.currentState = new StateUndefined(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long updateStateForSelectedVehicle(PhoneCalls pPhoneCall, int pMinuti)
        {
            return 0;
        }

        #endregion
    }
}
