using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities.PartialClasses;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.StateMachine.PhoneCallStates
{
	public class StateNewCall : PhoneCallState
	{

        public StateNewCall(PhoneCalls call)
            : base(call,PHONE_STATES.StateNewCall)
        {
           
        }

        private StateNewCall()
            : base(null,PHONE_STATES.StateNewCall)
        {
        }

        public override PhoneCallState Copy()
        {
            return new StateNewCall();
        }

        public override long acceptPhoneCall()
        {
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }

        public override long rejectPhoneCall()
        {
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }

        public override long sendPhoneCallToVehicle()
        {
            m_PhoneCall.currentState = new WaitForAccept(m_PhoneCall);
            return 0;
        }
    }
}
