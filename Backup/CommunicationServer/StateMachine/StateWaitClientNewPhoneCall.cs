using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateWaitClientNewPhoneCall : StateWaitClient
    {
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();
   
        public StateWaitClientNewPhoneCall(Vehicle vehicle)
            : base(vehicle)
        {
            m_SecInCurrentState = 0;

            bStateChanged = true;
        }

        public override long releaseVehicle()
        {
            if (m_myVehicle.NextPhoneCall == null)
            {
                m_myVehicle.CurrentPhoneCall = null;
                m_myVehicle.NextPhoneCall = null;
                m_myVehicle.currentState = new StateWaitRequest(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                return 0;
            }

            if ((m_myVehicle.NextPhoneCall != null) && (m_myVehicle.NextPhoneCall.StateName != PHONE_STATES.CallAccepted ))
            {
                m_myVehicle.CurrentPhoneCall = m_myVehicle.NextPhoneCall;
                m_myVehicle.NextPhoneCall = null;
                m_myVehicle.currentState = new StateWaitResponse(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return 0;
            }

            m_myVehicle.CurrentPhoneCall = null;
            m_myVehicle.NextPhoneCall = null;
            m_myVehicle.currentState = new StateIdle (this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
           
        }

        public override int releaseSecondCall()
        {
            this.m_myVehicle.NextPhoneCall = null;
            m_myVehicle.currentState = new StateWaitClient(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            this.m_myVehicle.StateChanged = true;
            return 0;
        }

        public override bool IsVehicleEligableForCall()
        {
                return false;
        }

        private StateWaitClientNewPhoneCall()
            : base()
        {
        }

        public override VehicleState Copy()
        {
            return new StateWaitClientNewPhoneCall();
        }
        public override int IDCurrentState()
        {
            return 60;
        }

        public override long reserveVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientNewPhoneCall", "reserveVehicle", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            if (m_myVehicle.NextPhoneCall == null)
            {

                //byte[] tAddressMessageByte = new byte[141];

                //byte[] tAddressMessageByteVoice;
                byte[] tAddressMessageByteLCD;

                clsMessageCreator messageCreator = new clsMessageCreator();

                // ZORAN: Sega praka samo poraka za LCD.... Da ne zaborvime za vo idnina...

                //tAddressMessageByteVoice = messageCreator.CreateAddressMessage(m_myVehicle, phoneCall);
                tAddressMessageByteLCD = messageCreator.CreateAddressMessageForLCD(m_myVehicle, phoneCall);


                //byte[] tAddressMessageVoicePlusLCD = new byte[tAddressMessageByteVoice.GetLength(0) + tAddressMessageByteLCD.GetLength(0)];
        

                //tAddressMessageByteVoice.CopyTo(tAddressMessageVoicePlusLCD, 0);
                //tAddressMessageByteLCD.CopyTo(tAddressMessageVoicePlusLCD, tAddressMessageByteVoice.GetLength(0));


                phoneCall.currentState = new Taxi.Communication.Server.StateMachine.PhoneCallStates.
                    WaitForAccept(phoneCall);

                m_myVehicle.NextPhoneCall = phoneCall;

                //return tAddressMessageVoicePlusLCD;
                return tAddressMessageByteLCD;
            }
            else
            {
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClientNewPhoneCall", "SendAddress", "");
                return null;
            }

        }


        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;
            //byte[] retValForName = null;

            retVal = base.UpdateSensorData(mySensorData);
            //Ne proveruvam dali ima kontakt, i da izvadi kluc, ne mu menuvan State
            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //Ako e vklucen -> BUSY
                m_myVehicle.currentState = new StateBusyNextPhoneCall(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
            }

            return retVal;
        }
    }
}
