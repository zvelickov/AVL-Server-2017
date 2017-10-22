using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;

namespace Taxi.Communication.Server.StateMachine.PhoneCallStates
{
    public class WaitForAccept : PhoneCallState
    {
        public WaitForAccept(PhoneCalls call)
            : base(call,PHONE_STATES.WaitForAccept)
        {
            this.hasTimeOut = true;
            this.m_MyTimeOut = 25; //PAZI Treba da se cita od baza
        }

        private WaitForAccept()
            : base(null, PHONE_STATES.WaitForAccept)
        {
        }

        protected bool IsTimeOut()
        {
            if (m_MyTimeOut <= 0)
            {
                m_PhoneCall.IsDirty = true;
                m_PhoneCall.currentState = new CallRejected(m_PhoneCall);
                return true;
            }
            else
            {
                return false;
            }

            
        }

        public override long acceptPhoneCall()
        {
            m_PhoneCall.currentState = new CallAccepted(m_PhoneCall);
            return 0;
        }

        public override PhoneCallState Copy()
        {
            return new WaitForAccept();
        }

        public override long rejectPhoneCall()
        {
            m_PhoneCall.currentState = new CallRejected(m_PhoneCall);
            return 0;
        }

        public override long sendPhoneCallToVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }
    }
}
