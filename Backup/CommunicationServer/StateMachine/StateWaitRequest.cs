using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using System.Configuration;
using Taxi.Communication.Server.Utils.Parsers;


namespace Taxi.Communication.Server.StateMachine
{
    public class StateWaitRequest : VehicleState
    {

        private long m_MyTimeOut = 0;
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateWaitRequest(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            //this.m_myVehicle.oRegionTo = null;
            m_MyTimeOut = 30; //PAZI Treba da se cita od baza, Tuka e staveno samo za nekoja sigurnost da ne se zaboravi
            m_SecInCurrentState = 0;

            //this.SensorForLogging = int.Parse(ConfigurationManager.AppSettings["SensorForLogging"]);

            bStateChanged = true;
        }


        public int SensorForLogging;

        private StateWaitRequest()
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
            return new StateWaitRequest();
        }
        public override int IDCurrentState()
        {
            return 3;
        }

        public override long reserveVehicle()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "reserveVehicle", "");
            return -1;
        }

        public override long releaseVehicle()
        {
            //Ako nekoj rezerviral vozilo, a se otkaze, mora da se vrati vo StateIdle
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);

            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "KeysAssigned", "");
            return -1;
        }


        public override byte[] SendAddress(long ID_User, PhoneCalls phoneCall)
        {
            byte[] tAddressMessageByteLCD;

            clsMessageCreator messageCreator = new clsMessageCreator();

            tAddressMessageByteLCD = messageCreator.CreateAddressMessageForLCD(m_myVehicle, phoneCall);
           
            m_myVehicle.currentState = new StateWaitResponse(this.m_myVehicle);

            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

            m_myVehicle.CurrentPhoneCall = phoneCall;
            m_myVehicle.StateChanged = true;

            return tAddressMessageByteLCD;
        }


        public override long CancellRequestFromClient()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "CancellRequestFromClient", "");
            return -1;
        }

        public override long ExtendWaitClientTime()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitRequest", "ExtendWaitClientTime", "");
            return -1;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();
            if (m_MyTimeOut <= 0)
            {
                //PAZI Ako poradi nekoja pricina, ne e otkazana rezervacija ili pratena adresa
                m_myVehicle.currentState = new StateIdle(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
            }

            m_SecInCurrentState += 1;

            //Namaluvam vreme
            m_MyTimeOut -= 1;

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 3, false));

                mMessageCreator = null;
            }     

            return retVal;
        }

        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;


            string m_DriverOrClient = "";

            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();



            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //Ako e vklucen -> BUSY
                m_myVehicle.currentState = new StateBusy(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                return retVal;
            }



            //Proveruvam, ako ima RfIdCard na klient, da vrati zeleno svetlo
            m_DriverOrClient = RfIdCardProcessor.ProcessClient(m_myVehicle.currentSensorData.RfIdCard);

            if (m_DriverOrClient != "")
            {
                retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_DriverOrClient, '4');
            }


            //Dali e vklucen Senzor za Kraj na smena

            if (m_myVehicle.currentSensorData.Senzor_6 == 1)
            {

                //  ZORAN:      Proveruvam: 
                //                  1. Ako ima RfIdCard na VOZAC, da go ODJAVI i da vrati zeleno svetlo
                //                  2. Ne pravam nisto ako RfIdCard == "0000000000"
                //                  3. Vraka prazen byte[0] ako ne najde nesto...
                //                  

                if (m_myVehicle.currentSensorData.RfIdCard != "0000000000")
                    retVal = CheckForCheckOutDriverShift(m_myVehicle.currentSensorData);

                if (retVal != null)
                {
                    //Sekako go prefrlam vo StateShiftEnded
                    m_myVehicle.currentState = new StateShiftEnded(this.m_myVehicle);

                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                    m_myVehicle.StateChanged = true;
                }
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
                    retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_retVal, '4');
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
