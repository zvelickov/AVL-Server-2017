using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Data;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateBusyNextPhoneCall : StateBusy
    {
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateBusyNextPhoneCall(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            m_SecInCurrentState = 0;

            //  Se da zatvoram ako ima otvoren ORDER od Android
            TList<MobileOrders> lstMobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus((long)vehicle.IdVehicle, 2);

            if (lstMobileOrders != null && lstMobileOrders.Count > 0)
            {
                lstMobileOrders[0].IdMobileOrderStatus = 3;
                lstMobileOrders[0].Comment = "StateBusyNextPhoneCall (" + lstMobileOrders.Count.ToString() + ")";
                DataRepository.MobileOrdersProvider.Update(lstMobileOrders[0]);
            }

            bStateChanged = true;
        }

        protected StateBusyNextPhoneCall()
            : base()
        {
        }

        public override long setClientRfIdCard(RfIdCards card)
        {
            m_myVehicle.CurrentRfIdCard = card;
            return 0;
        }

        public override int releaseSecondCall()
        {
            this.m_myVehicle.NextPhoneCall = null;
            m_myVehicle.currentState = new StateBusy(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            this.m_myVehicle.StateChanged = true;
            return 0;
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

            if ((m_myVehicle.NextPhoneCall != null) && (m_myVehicle.NextPhoneCall.StateName != PHONE_STATES.CallAccepted))
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
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;

        }

        public override bool IsVehicleEligableForCall()
        {
            
            return false;
            
        }

        public override long reserveVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusyNextPhoneCall", "reserveVehicle", "");
            return -1;
        }

        public override VehicleState Copy()
        {
            return new StateBusyNextPhoneCall();
        }
        public override int IDCurrentState()
        {
            return 70;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();
            //errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "DecreaseTimeout", "");

            if (this.m_myVehicle.NextPhoneCall != null)
            {
                this.m_myVehicle.NextPhoneCall.currentState.Tick();
                if (this.m_myVehicle.NextPhoneCall.currentState.IsTimeOut())
                {
                    this.m_myVehicle.NextPhoneCall = null;
                    m_myVehicle.currentState = new StateBusy(this.m_myVehicle);
                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                    this.m_myVehicle.StateChanged = true;
                }
            }

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 30, false));

                mMessageCreator = null;
            }

            return retVal;
        }


        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {

            if (m_myVehicle.NextPhoneCall == null)
            {

                //byte[] tAddressMessageByte = new byte[141];

                //byte[] tAddressMessageByteVoice;
                byte[] tAddressMessageByteLCD;

                clsMessageCreator messageCreator = new clsMessageCreator();

                tAddressMessageByteLCD = messageCreator.CreateAddressMessageForLCD(m_myVehicle, phoneCall);                       

                phoneCall.currentState = new Taxi.Communication.Server.StateMachine.PhoneCallStates.
                    WaitForAccept(phoneCall);

                m_myVehicle.NextPhoneCall = phoneCall;

                //return tAddressMessageVoicePlusLCD;
                return tAddressMessageByteLCD;
            }
            else
            {
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateBusyNextPhoneCall", "SendAddress", "");
                return null;
            }


        }

    }
}
