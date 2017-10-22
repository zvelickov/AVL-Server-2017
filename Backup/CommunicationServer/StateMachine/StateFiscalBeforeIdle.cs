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
    public class StateFiscalBeforeIdle : VehicleState
    {
        private long m_MyTimeOut = 0;

        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateFiscalBeforeIdle(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            m_MyTimeOut = 5;
            m_SecInCurrentState = 0;

            //  Se da zatvoram ako ima otvoren ORDER od Android
            TList<MobileOrders> lstMobileOrders = DataRepository.MobileOrdersProvider.GetByIdVehicleAndStatus((long)vehicle.IdVehicle, 2);

            if (lstMobileOrders != null && lstMobileOrders.Count > 0)
            {
                lstMobileOrders[0].IdMobileOrderStatus = 3;
                lstMobileOrders[0].Comment = "StateFiscalBeforeIdle (" + lstMobileOrders.Count.ToString() + ")";
                DataRepository.MobileOrdersProvider.Update(lstMobileOrders[0]);
            }

            bStateChanged = true;
        }

        private StateFiscalBeforeIdle()
            : base(null)
        {
        }

        public override long AlarmConfirmed()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long CancellRequestFromClient()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
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


        public override VehicleState Copy()
        {
            return new StateFiscalBeforeIdle();
        }

        public override long ExtendWaitClientTime()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override int IDCurrentState()
        {
            return 71;
        }

        public override bool IsVehicleEligableForCall()
        {
            return false;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long KeysReturned()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
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

        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            return null;
        }

        public override long VoiceConfirm()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long VoiceInitiate()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long VoiceReject()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }


        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();

            m_SecInCurrentState += 1;

            if ((m_myVehicle.CurrentPhoneCall == null) && (m_myVehicle.CurrentRfIdCard == null))
            {
                m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return retVal;
            }
            
            
            if (m_MyTimeOut <= 0)
            {
                //PAZI Ako poradi nekoja pricina, ne e otkazana rezervacija ili pratena adresa
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

        public override int releaseSecondCall()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }

        public override long releaseVehicle()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
        }


        public override long reserveVehicle()
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateFiscalBeforeIdle", "AlarmRejected", "");
            return -1;
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
            return -1;
        }

    }
}
