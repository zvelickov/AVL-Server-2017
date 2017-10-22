using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateWaitClientConfirmation : VehicleState
    {
        private long m_MyTimeOut = 0;
        private long m_SecInCurrentState = 0;
        
        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateWaitClientConfirmation(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            //this.m_myVehicle.oRegionTo = null;
            m_MyTimeOut = 8; //PAZI Treba da se cita od baza
            m_SecInCurrentState = 0;

            bStateChanged = true;
        }

        private StateWaitClientConfirmation()
            : base(null)
        {
        }

        public override long AlarmConfirmed()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "AlarmReject", "");
            return -1;
        }

        public override bool setRiminderForRfId()
        {
            return false;
        }

        public override long setClientRfIdCard(RfIdCards ID_Driver)
        {
            return 0;
        }

        public override long AlarmReset()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "AlarmReset", "");
            return -1;
        }

        public override long acceptedFromClient()
        {
            m_myVehicle.currentState = new StateMoveToClient(m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long CancellRequestFromClient()
        {
            m_myVehicle.currentState = new StateIdle(m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override VehicleState Copy()
        {
            return new StateWaitClientConfirmation();
        }

        public override long ExtendWaitClientTime()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "Extend wait client time", "");
            return -1;
        }

        public override int IDCurrentState()
        {
            return 41;
        }

        public override bool IsVehicleEligableForCall()
        {
            return false;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "KeyAssigned", "");
            return -1;
        }

        public override long KeysReturned()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "KeysReturned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "SendAddress", "");
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


        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;

            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();


            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //PAZI Kazna, Pratena adresa, Vklucil taksimetar
                clsPenaltyCreator PenaltyCreator = new clsPenaltyCreator();
                PenaltyCreator.Createpenalty(3, m_myVehicle);
                m_myVehicle.currentState = new StateBusy(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return retVal;
            }

           

            return null;
        }

        public override long VoiceConfirm()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceInitiate()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceReject()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "VoiceReject", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();

            byte[] tAddressMessageByteVoice;
            byte[] tAddressMessageByteLCD;

            m_SecInCurrentState += 1;

            Console.WriteLine("Timer: " + m_MyTimeOut.ToString()) ;

            if (m_MyTimeOut <= 0)
            {
                
                clsMessageCreator tMessageCreator = new clsMessageCreator();

                retVal.Add (tMessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, "Otkazano od klient!", '4'));
                             
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

        public override int releaseSecondCall()
        {
            return 0;
        }

        public override long releaseVehicle()
        {
            m_myVehicle.CurrentPhoneCall = null;
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long reserveVehicle()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientConfirmation", "VoiceReject", "");
            return -1;
        }

        public override long setDriverShift(long ID_Driver)
        {
            //ZORAN: Ova sega ne mi treba, no neka ostane kako traga..
            //this.m_myVehicle.IdDriverShift = ID_Driver;
            return 0;
        }
    }
}
