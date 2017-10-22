using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.StateMachine
{
    class GeneralStateMachineClass
    {
        public byte[] CheckForCheckOutDriverShift(Vehicle veh)
        {
            byte[] retVal = null;
            
            ////byte[] retValForName = null;


            ////clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            ////clsMessageCreator MessageCreator = new clsMessageCreator();

            ////string m_retVal = RfIdCardProcessor.ProcessCheckOutDriver(veh.currentSensorData.RfIdCard, veh);

            ////if (m_retVal != "")
            ////{
            ////    //Prakam zeleno svetlo da se odjavi
            ////    retVal = MessageCreator.CreateGreenLight(veh);

            ////    retValForName = MessageCreator.CreatePopUpMessageForLCD(veh, m_retVal);

            ////    byte[] tmpRetVal = new byte[retVal.GetLength(0) + retValForName.GetLength(0)];

            ////    retVal.CopyTo(tmpRetVal, 0);
            ////    retValForName.CopyTo(tmpRetVal, retVal.GetLength(0));

            ////    retVal = tmpRetVal;
            ////}


            return retVal;
        }

        public byte[] CheckForCheckInDriverShift(Vehicle veh)
        {
            byte[] retVal = null;
            //////byte[] retValForName = null;


            //////clsRfIdCardProcessor RfIdCardProcessor = new clsRfIdCardProcessor();
            //////clsMessageCreator MessageCreator = new clsMessageCreator();

            //////string m_retVal = RfIdCardProcessor.ProcessCheckInDriver(veh.currentSensorData.RfIdCard, veh);

            //////if (m_retVal != "")
            //////{
            //////    //Prakam zeleno svetlo da se odjavi
            //////    retVal = MessageCreator.CreateGreenLight(veh);

            //////    retValForName = MessageCreator.CreatePopUpMessageForLCD(veh, m_retVal);

            //////    byte[] tmpRetVal = new byte[retVal.GetLength(0) + retValForName.GetLength(0)];

            //////    retVal.CopyTo(tmpRetVal, 0);
            //////    retValForName.CopyTo(tmpRetVal, retVal.GetLength(0));

            //////    retVal = tmpRetVal;
            //////}


            return retVal;
        }


        

    }
}
