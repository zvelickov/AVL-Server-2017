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
    public class StateIdle : VehicleState
    {
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateIdle(Vehicle vehicle)
            : base(vehicle)
        {         
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;

            vehicle.CurrentPhoneCall = null;
            vehicle.CurrentRfIdCard = null;
            //vehicle.oRegionTo = null;
            m_myVehicle.DirtyStateChange = true;

            m_SecInCurrentState = 0;
            
            //this.SensorForLogging = int.Parse(ConfigurationManager.AppSettings["SensorForLogging"]);

            //ZORAN:    Moram da mu resetiram vremeto na posleden taksimetar AKO prehodni sostojbi bile:
            //          - Pause         (imal izvadeno kluc, pa go vratil)
            //          - ShiftEnded    (se prijavil na pauza, pa se vratil)
            //          - Undefined     (sistemot se resetiral, pa sega se prijavuva. Ova ne e pravedno, no nema drugo)
            //          - DOPOLNITELNO:
            //                  StateBusy i StateFiscalBeforeIdle ke gi opfatam bidejki na toj nacin ke gi fatime i tie sto se odjavuvaat od taksimetar!!


            if (this.m_myVehicle.previousStateString == "StateShiftEnded" ||
                this.m_myVehicle.previousStateString == "StateUndefined" ||
                this.m_myVehicle.previousStateString == "StateBusy" ||
                this.m_myVehicle.previousStateString == "StateFiscalBeforeIdle")
            {
                this.m_myVehicle.TaximetarLast = System.DateTime.Now;
            }

            //  Se da zatvoram ako ima otvoren ORDER od Android
            TList<MobileOrders> lstMobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus((long)vehicle.IdVehicle, 2);

            if (lstMobileOrders != null && lstMobileOrders.Count > 0)
            {
                lstMobileOrders[0].IdMobileOrderStatus = 3;
                lstMobileOrders[0].Comment = "StateIdle (" + lstMobileOrders.Count.ToString() + ")";
                DataRepository.MobileOrdersProvider.Update(lstMobileOrders[0]);
            }

            bStateChanged = true;
        }

        private int SensorForLogging;

        public override int releaseSecondCall()
        {
            return 0;
        }

        public override bool setRiminderForRfId()
        {
            return false;
        }

        public override long acceptedFromClient()
        {
            return -1;
        }

        public override long setClientRfIdCard(RfIdCards ID_Driver)
        {
            return 0;
        }


        public override bool IsVehicleEligableForCall()
        {            
            return true;            
        }


        private StateIdle()
            : base(null)
        {
        }

        #region IVehicleState Members
        public override VehicleState Copy()
        {
            return new StateIdle();
        }
        public override int IDCurrentState()
        {
            return 2;
        }

        public override long reserveVehicle()
        {
            m_myVehicle.currentState = new StateWaitRequest(this.m_myVehicle);

            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long releaseVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "releaseVehicle", "");
            return -1;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "SendAddress", "");
            return null;
        }

        public override long CancellRequestFromClient()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "CancellRequestFromClient", "");
            return -1;
        }

        public override long ExtendWaitClientTime()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateIdle", "ExtendWaitClientTime", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();
            //errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "DecreaseTimeout", "");

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 0, false));

                mMessageCreator = null;
            }     
            return retVal;
        }


        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;


            //  ZORAN:      Proveruvam: 
            //                  1. Ako ima RfIdCard na klient, da vrati zeleno svetlo
            //                  2. Ne pravam nisto ako RfIdCard == "0000000000"
            //                  3. Vraka prazen byte[0] ako ne najde nesto...
            
            if (m_myVehicle.DriverShiftInOut == null)
            {
                m_myVehicle.currentState = new StateShiftEnded(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                return retVal;
            }


            //PROC03 Dali ima kontakt, Ako ne prefrli vo StatePause
            ////if (m_myVehicle.currentSensorData.Senzor_1 != 1 && m_myVehicle.Station == false)
            ////{
            ////    m_myVehicle.currentState = new StatePause(this.m_myVehicle);
            ////    m_myVehicle.StateChanged = true;
            ////    return retVal;
            ////}

            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //Ako e vklucen -> BUSY
                m_myVehicle.currentState = new StateBusy(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                return retVal;
            }


            //Dali e vklucen Senzor za Kraj na smena

            if (retVal == null)
            {
                if (m_myVehicle.currentSensorData.Senzor_6 == 1)
                {

                    //  ZORAN:      Proveruvam: 
                    //                  1. Ako ima RfIdCard na VOZAC, da go ODJAVI i da vrati zeleno svetlo
                    //                  2. Ne pravam nisto ako RfIdCard == "0000000000"
                    //                  3. Vraka prazen byte[0] ako ne najde nesto...
                    //                  

                    if (m_myVehicle.currentSensorData.RfIdCard != "0000000000")
                        retVal = CheckForCheckOutDriverShift(m_myVehicle.currentSensorData);

                    //if (retVal != null)
                    //{
                        //Sekako go prefrlam vo StateShiftEnded
                        m_myVehicle.currentState = new StateShiftEnded(this.m_myVehicle);

                        tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                        m_myVehicle.StateChanged = true;
                    //}
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
                    

                    if (retVal != null)
                    {
                        m_myVehicle.currentState = new StateIdle(this.m_myVehicle);

                        tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                        m_myVehicle.StateChanged = true;
                    }

                    //Tuka ne menuvam State
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
                        retValForStation = tMessageCreator.CreateStationStatus(m_myVehicle, 1);
                        m_myVehicle.OnStationFromDateTime = DateTime.Now;
                        m_myVehicle.PreviousStationState = true;
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


            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();

            string m_retVal = RfIdCardProcessor.ProcessCheckOutDriver(pSensorData.RfIdCard, m_myVehicle);

            if (m_retVal != "")
            {
                    retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');
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
            if (pPhoneCall != null)
            {
                if (pPhoneCall.MessageType == "MC")
                    m_myVehicle.currentState = new StateWaitClientConfirmation(this.m_myVehicle);
                else
                    m_myVehicle.currentState = new StateMoveToClient(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            }
            
            m_myVehicle.StateChanged = true;
            return 0;
        }

        #endregion
    }
}
