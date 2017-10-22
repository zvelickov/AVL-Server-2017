using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Data;

using System.Globalization;

namespace Taxi.Communication.Server.StateMachine
{
    public class StateWaitClient : VehicleState
    {
        private long m_MyTimeOut = 0;
        private long m_SecInCurrentState = 0;

        private bool bStateChanged = false;

        private UpdateStateInOut tmpUpdateStateInOut = new UpdateStateInOut();

        public StateWaitClient(Vehicle vehicle)
            : base(vehicle)
        {
            this.m_myVehicle.previousState = this.m_myVehicle.currentState;
            //this.m_myVehicle.oRegionTo = null;
            m_MyTimeOut = 450; //PAZI Treba da se cita od baza
            m_SecInCurrentState = 0;

            bStateChanged = true;
            //  Se da zatvoram ako ima otvoren ORDER od Android
            // Vo WaitClient NE ZATVARAM ORDER!!!

            //ZORAN: Tuka da pratam SMS
            SendSMSifMobilePhone();
        }

        protected StateWaitClient()
            : base(null)
        {
        }

        public override bool IsVehicleEligableForCall()
        {
                return false;
        }

        public override int releaseSecondCall()
        {
            return 0;
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
            return new StateWaitClient();
        }
        public override int IDCurrentState()
        {
            return 6;
        }

        public override long reserveVehicle()
        {
            if (m_myVehicle.NextPhoneCall == null)
            {
                m_myVehicle.currentState = new StateWaitClientNewPhoneCall(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
                return 0;
            }
            else
            {
                //throw new Exception("The method or operation is not implemented.");
                errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "reserveVehicle", "");
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
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "VoiceInitiate", "");
            return -1;
        }

        public override long VoiceConfirm()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "VoiceConfirm", "");
            return -1;
        }

        public override long VoiceReject()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "VoiceReject", "");
            return -1;
        }

        public override long AlarmConfirmed()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "AlarmConfirmed", "");
            return -1;
        }

        public override long AlarmRejected()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "AlarmRejected", "");
            return -1;
        }

        public override long AlarmReset()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "AlarmReset", "");
            return -1;
        }

        public override long KeysReturned()
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "KeysReturned", "");
            return -1;
        }

        public override long KeysAssigned(long ID_Driver)
        {
            //throw new Exception("The method or operation is not implemented.");
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "KeysAssigned", "");
            return -1;
        }

        public override byte[] SendAddress(long ID_User,  PhoneCalls phoneCall)
        {
            errStateMachine.LogStateMachineError(m_myVehicle.IdVehicle, "StateWaitClient", "SendAddress", "");
            return null;           
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

            m_SecInCurrentState += 1;

            if (m_MyTimeOut <= 0)
            {
                //PAZI WaitClient, pominal TimeOut, Ne Kacil Patnik
                clsPenaltyCreator PenaltyCreator = new clsPenaltyCreator();
                PenaltyCreator.Createpenalty(6, m_myVehicle);
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
                    //PAJO: Ova e tuka bidejki se povikuva i od kerka klasa
                    m_myVehicle.currentState = new StateWaitClient(this.m_myVehicle);

                    tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                    this.m_myVehicle.StateChanged = true;
                }
            }

            if ((bStateChanged) && (m_myVehicle.TypeOfOrder == 2))
            {
                clsMessageCreator mMessageCreator = new clsMessageCreator();

                bStateChanged = false;
                retVal.Add(mMessageCreator.VehicleState(this.m_myVehicle, 15, true));

                mMessageCreator = null;
            }     

            return retVal;
        }

        public override byte[] UpdateSensorData(SensorData mySensorData)
        {
            m_myVehicle.currentSensorData = mySensorData;
            byte[] retVal = null;

            clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            clsMessageCreator MessageCreator = new clsMessageCreator();


            //Proveruvam, ako ima RfIdCard na klient, da vrati zeleno svetlo
            string m_DriverOrClient = RfIdCardProcessor.ProcessClient(m_myVehicle.currentSensorData.RfIdCard);
           
            if (m_DriverOrClient != "")
            {
                    retVal = MessageCreator.CreateNewPopUpMessageForLCD(m_myVehicle, m_DriverOrClient, '4');
            }


            //Ne proveruvam dali ima kontakt, i da izvadi kluc, ne mu menuvan State
            //PROC06 Proveruvam Senzor Taximeter
            if (m_myVehicle.currentSensorData.Senzor_8 == 0)
            {
                //Ako e vklucen -> BUSY
                m_myVehicle.currentState = new StateBusy(this.m_myVehicle);

                tmpUpdateStateInOut.UpdateVehicleState(m_myVehicle);

                m_myVehicle.StateChanged = true;
            }

            return retVal;
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


        private void SendSMSifMobilePhone()
        {
            try
            {
                if(this.m_myVehicle.CurrentPhoneCall != null) 
                        //&& m_myVehicle.CurrentPhoneCall.MessageType != "MC"
                        //&& m_myVehicle.CurrentPhoneCall.MessageType != "GC")

                    if(isThisMobile(this.m_myVehicle.CurrentPhoneCall.PhoneNumber))
                    {
                        ////ZORAN:    Za pocetok samo za Donka i Ljupco
                        //if (this.m_myVehicle.CurrentPhoneCall.PhoneNumber.Contains("368207") || this.m_myVehicle.CurrentPhoneCall.PhoneNumber.Contains("386280"))
                        //{
                            SmSsent mSmsSent = new SmSsent();
                            mSmsSent.PhoneNumber = FormatNumberForSMS(this.m_myVehicle.CurrentPhoneCall.PhoneNumber);
                            mSmsSent.SmStext = "Pocituvani, za plakjanje vo gotovo dobivate popust. Vozilo " + this.m_myVehicle.DescriptionLong + " pristignuva za 1 min: " + "http://maps.google.com/?q=" + m_myVehicle.currentGPSData.Latutude_Y.ToString(CultureInfo.CreateSpecificCulture("en-GB")) + "," + m_myVehicle.currentGPSData.Longitude_X.ToString(CultureInfo.CreateSpecificCulture("en-GB"));                          

                            DataRepository.SmSsentProvider.Insert(mSmsSent);
                        //}
                    }
                    
            }
            catch (Exception ex)
            {
                
            }
                
        }


        private bool isThisMobile(string pPhoneNumber)
        {
            bool retVal = false;
            string mPhoneNumber = pPhoneNumber;

            if (mPhoneNumber.Substring(0, 1) == "+")
                mPhoneNumber = mPhoneNumber.Substring(1, mPhoneNumber.Length - 1);

            if (mPhoneNumber.Substring(0, 3) == "389")
            {
                mPhoneNumber = mPhoneNumber.Substring(3, mPhoneNumber.Length - 3);
                mPhoneNumber = "0" + mPhoneNumber;
            }

            switch(mPhoneNumber.Substring(0,3))                
            {
                case "070":
                case "071":
                case "072":
                case "075":
                case "076":
                case "077":
                case "078":
                    retVal = true;
                    break;
                default:
                    retVal = false;
                    break;
            } 
           	   	
            return retVal;
        }


        //ZORAN:    Mislam, ne sum siguren, deka treba da bide vo format +389XXXYYY
        //          Duri i da ne e, ke go napravam taka, da ne go mislam
        //          Nekogas stiga so +, nekogas ne
        //          Nekogasima 389, nekogas ne
        private string FormatNumberForSMS(string pPhoneNumber)
        {
            string retVal = pPhoneNumber;

            try
            {
                if (retVal.Substring(0, 1) == "+")
                    retVal = retVal.Substring(1, retVal.Length - 1);

                if (retVal.Substring(0, 3) == "389")
                {
                    retVal = retVal.Substring(3, retVal.Length - 3);
                    retVal = "0" + retVal;
                }

                retVal = "+389" + retVal.Substring(1, retVal.Length - 1);
            }
            catch (Exception ex)
            {
            }

            return retVal;
        }
    }
}
