using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;

namespace Taxi.Communication.Server.StateMachine.PhoneCallStates
{
    public class CallAccepted : PhoneCallState
    {
        public CallAccepted(PhoneCalls call)
            : base(call,PHONE_STATES.CallAccepted)
        {
            m_PhoneCall.IsDirty = true;
        }

        private CallAccepted()
            : base(null, PHONE_STATES.CallAccepted)
        {
        }

        public override PhoneCallState Copy()
        {
            return new CallAccepted();
        }

        public override bool isAccepted()
        {
            return true;
        }

        public override long acceptPhoneCall()
        {
            //throw new Exception("The method or operation is not implemented.");
            return 0;
        }

        public override long rejectPhoneCall()
        {
            return 0;
        }

        public override long sendPhoneCallToVehicle()
        {
            return 0;
        }
    }
}
