using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Data;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateMoveToClient : VehicleState
    {
        private long m_MyTimeOut = 0;
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateMoveToClient(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            m_MyTimeOut = 600; //=20 min (se mnozi so 2000ms vo GpsListener) PAZI Treba da se cita od baza!
            m_SecInCurrentState = 0;

            bStateChanged = true;
            //  Se da zatvoram ako ima otvoren ORDER od Android
            //  Tuka NE ZATVARAM ORDER
        }

        public override bool IsVehicleEligableForCall()
        {
            return false;
        }

        protected StateMoveToClient()
            : base(null)
        {
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
            return new StateMoveToClient();
        }
        public override int IDCurrentState()
        {
            return 5;
        }

        public override int releaseSecondCall()
        {
            return 0;
        }

        public override long reserveVehicle()
        {
            if (m_myVehicle.NextPhoneCall == null)
            {
                m_myVehicle.currentState = new StateMoveToClientNewPhoneCall(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
                return 0;
            }
            else
            {
                //throw new Exception("The method or operation is not implemented.");
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "reserveVehicle", "");
                return -1;
            }

        }

        public override long releaseVehicle()
        {
            m_myVehicle.CurrentPhoneCall = null;
            m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long VoiceInitiate()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User,  PhoneCalls phoneCall)
        {

            
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateMoveToClient", "SendAddress Already Has NextPhoneCall", "");
                return null;
     

            //throw new Exception("The method or operation is not implemented.");
           
        }

        public override long CancellRequestFromClient()
        {
            //Klientot se otkazuva
            m_myVehicle.currentState = new StateIdle(m_myVehicle);
            tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
            m_myVehicle.StateChanged = true;
            return 0;
        }

        public override long ExtendWaitClientTime()
        {
            m_MyTimeOut = 600; //Pazi, treba da cita od Baza kolku treba da prodolzi
            return 0;
        }

        public override List<byte[]> decreaseTimeOut()
        {
            List<byte[]> retVal = new List<byte[]>();

            if (m_MyTimeOut <= 0)
            {
                //PAZI MoveToPoint, pominal TimeOut, a ne stignal vo Zona na klient
                clsPenaltyCreator PenaltyCreator = new clsPenaltyCreator();
                PenaltyCreator.Createpenalty(4, m_myVehicle);
                m_myVehicle.currentState = new StateIdle(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
            }
            //Namaluvam vreme
            m_MyTimeOut -= 1;

            if (this.m_myVehicle.NextPhoneCall != null)
            {
                this.m_myVehicle.NextPhoneCall.currentState.Tick();
                if (this.m_myVehicle.NextPhoneCall.currentState.IsTimeOut())
                {
                    this.m_myVehicle.NextPhoneCall = null;
                    m_myVehicle.currentState = new StateMoveToClient(this.m_myVehicle);
                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                    this.m_myVehicle.StateChanged = true;
                }
            }

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 20, true));

                mMessageCreator = null;
            }     

            return retVal;
        }

        public override byte[] UpdateGPSData(GPSData myGPSData)
        {
            m_myVehicle.currentGPSData = myGPSData;
            
            clsMessageCreator tMessageCreator = new clsMessageCreator();
            byte[] retValForStation = null;

            GPSData GpsDataTo = new GPSData ();
            GpsDataTo.Longitude_X = m_myVehicle.CurrentPhoneCall.oAddressFrom.LocationX;
            GpsDataTo.Latutude_Y = m_myVehicle.CurrentPhoneCall.oAddressFrom.LocationY;

            //PAZI Golemina na zona na klient, treba da cita od baza
            //Console.WriteLine("Dalecina do klient = " + CalculateDistance(m_myVehicle.currentGPSData, GpsDataTo).ToString()); 
            if (CalculateDistance (m_myVehicle.currentGPSData, GpsDataTo ) < 65)
            {                
                m_myVehicle.currentState = new StateWaitClient(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
            }

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


            //Proveruvam, ako ima RfIdCard na klient, da vrati popup so info
            string m_DriverOrClient = RfIdCardProcessor.ProcessClient(m_myVehicle.currentSensorData.RfIdCard);

            if (m_DriverOrClient != "")
            {
                retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_DriverOrClient, '4');
            }

            //Ne proveruvam dali ima kontakt, i da izvadi kluc, ne mu menuvan State
            //PROC03 Dali ima kontakt, Ako ne prefrli vo StatePause

            //////PROC07 Proveruvam Kopce Accept Call 
            ////// Moze da se sluci samo ako ima Pending Call
            ////if (m_myVehicle.currentSensorData.Senzor_4 == 1)
            ////{
            ////    if (m_myVehicle.NextPhoneCall != null)
            ////    {
            ////        m_myVehicle.NextPhoneCall.currentState.acceptPhoneCall();
            ////        //SetDirty bit
            ////        m_myVehicle.StateChanged = true;
            ////    }
            ////}

            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //Ako e vklucen -> BUSY
                //Kazna, Vo MoveToPoint Vklucil Taksimetar
                clsPenaltyCreator PenaltyCreator = new clsPenaltyCreator();
                PenaltyCreator.Createpenalty(5, m_myVehicle);
                m_myVehicle.currentState = new StateBusy(this.m_myVehicle);
                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);
                m_myVehicle.StateChanged = true;
            }

            //Ne Menuvam State

            return retVal ;
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

        protected double CalculateDistance(GPSData From, GPSData To)
        {
            //PAZI FORMAT NA KOORDINATI 
            //(Prima Stepeni DD.DDDDDDDD)
            //Latituda (42) = Y
            //Longituda (21) = X
            double RetVal = 0;


            ////Za vo radijani
            GPSData myFrom = new GPSData();
            GPSData myTo = new GPSData();
            myFrom.Longitude_X = From.Longitude_X / 180 * Math.PI;
            myFrom.Latutude_Y = From.Latutude_Y / 180 * Math.PI;
            myTo.Longitude_X = To.Longitude_X / 180 * Math.PI;
            myTo.Latutude_Y = To.Latutude_Y / 180 * Math.PI;


            //---------------------------------------------------------------------------------------------------------------
            //Ova e vtora verzija
            double dLat = myTo.Longitude_X - myFrom.Longitude_X;
            double dLon = myTo.Latutude_Y - myFrom.Latutude_Y;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) + Math.Cos(myFrom.Longitude_X) * Math.Cos(myTo.Longitude_X) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            RetVal = 6377397 * c;

            return RetVal;
        }
    }
}
