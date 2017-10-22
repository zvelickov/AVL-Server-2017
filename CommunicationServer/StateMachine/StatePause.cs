using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Data;

namespace Taxi.Communication.Server.StateMachine
{
    public class StatePause : VehicleState
    {
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StatePause(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            //this.m_myVehicle.oRegionTo = null;

            m_SecInCurrentState = 0;

            //  Se da zatvoram ako ima otvoren ORDER od Android
            TList<MobileOrders> lstMobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus((long)vehicle.IdVehicle, 2);

            if (lstMobileOrders != null && lstMobileOrders.Count > 0)
            {
                lstMobileOrders[0].IdMobileOrderStatus = 3;
                lstMobileOrders[0].Comment = "StatePause (" + lstMobileOrders.Count.ToString() + ")";
                DataRepository.MobileOrdersProvider.Update(lstMobileOrders[0]);
            }

            bStateChanged = true;
        }
        private StatePause()
            : base(null)
        {
        }

        public override bool IsVehicleEligableForCall()
        {
            return false;
        }

        public override int releaseSecondCall()
        {
            return 0;
        }

        public override bool setRiminderForRfId()
        {
            return false;
        }

        public override long setClientRfIdCard(RfIdCards ID_Driver)
        {
            return 0;
        }

        public override long acceptedFromClient()
        {
            return -1;
        }

        #region IVehicleState Members
        public override VehicleState Copy()
        {
            return new StatePause();
        }
        public override int IDCurrentState()
        {
            return 8;
        }

        public override long reserveVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "reserveVehicle", "");
            return -1;
        }

        public override long releaseVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "releaseVehicle", "");
            return -1;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            m_myVehicle.currentState = new StateKeySurrendered(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User,  PhoneCalls phoneCall)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "SendAddress", "");
            return null;
        }

        public override long CancellRequestFromClient()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "CancellRequestFromClient", "");
            return -1;
        }

        public override long ExtendWaitClientTime()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StatePause", "ExtendWaitClientTime", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();
            m_SecInCurrentState += 1;

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
            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();


            //PROC09 Dali ima kontakt, Se Vratil od Pauza, Premini vo Idle
            if (m_myVehicle.currentSensorData.Senzor_1 == 1)
            {
                m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return retVal;
            }


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


            return retVal;
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
                //Prakam zeleno svetlo da se odjavi
                //retVal = MessageCreator.CreateGreenLight(m_myVehicle);

                //retValForName = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');
                retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');

                //byte[] tmpRetVal = new byte[retVal.GetLength(0) + retValForName.GetLength(0)];

                //retVal.CopyTo(tmpRetVal, 0);
                //retValForName.CopyTo(tmpRetVal, retVal.GetLength(0));

                //retVal = tmpRetVal;
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
