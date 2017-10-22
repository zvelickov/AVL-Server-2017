using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateWaitResponse : VehicleState
    {

        private long m_MyTimeOut = 0;
        private long m_SecInCurrentState = 0;
        
        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateWaitResponse(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            //this.m_myVehicle.oRegionTo = null;
            m_MyTimeOut = 7; //PAZI Treba da se cita od baza
            m_SecInCurrentState = 0;

            bStateChanged = true;
        }

        private  StateWaitResponse()
            : base(null)
        {
        }

        public override int releaseSecondCall()
        {
            return 0;
        }

        public override bool IsVehicleEligableForCall()
        {
            return false;
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
            return new StateWaitResponse();
        }

        public override int IDCurrentState()
        {
            return 4;
        }

        public override long reserveVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "reserveVehicle", "");
            return -1;
        }

        public override long releaseVehicle()
        {
            //Ako nekoj rezerviral vozilo, a se otkaze, mora da se vrati vo StateIdle
            //PAZI, Tuka treba i da se vrati zvuk, se otkazuva povik
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);

            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "SendAddress", "");
            return null;
        }

        public override long CancellRequestFromClient()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "CancellRequestFromClient", "");
            return -1;
        }

        public override long ExtendWaitClientTime()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitResponse", "ExtendWaitClientTime", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();

            ////byte[] tAddressMessageByteVoice;
            ////byte[] tAddressMessageByteLCD;

            m_SecInCurrentState += 1;

            if (m_MyTimeOut <= 0)
            {

                m_myVehicle.currentState = new StateIdle(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;

            }
            //Namaluvam vreme
            m_MyTimeOut -= 1;

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 2, false));

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
           

            //PROC07 Proveruvam Kopce Accept Call
            if (m_myVehicle.currentSensorData.Senzor_4 == 1)
            {
                if (m_myVehicle.CurrentPhoneCall.MessageType.Equals("MC"))
                {
                    m_myVehicle.currentState = new StateWaitClientConfirmation(this.m_myVehicle);
                }
                else
                {
                    m_myVehicle.currentState = new StateMoveToClient(this.m_myVehicle);
                }

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                retVal = MessageCreator.CreateStationStatus(this.m_myVehicle, 3);

                return retVal;
            }

            return null;
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

        #endregion
        
    }
}
