using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;

namespace Taxi.Communication.Server.StateMachine.PhoneCallStates
{
    public class CallRejected : PhoneCallState
    {
        public CallRejected(PhoneCalls call)
            : base(call, PHONE_STATES.CallRejected)
        {
            m_PhoneCall.IsDirty = true;
        }

        private CallRejected()
            : base(null, PHONE_STATES.CallRejected)
        {
        }

        public override bool isRejected()
        {
            return true;
        }

        public override PhoneCallState Copy()
        {
            return new CallRejected();
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
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }
    }
}
