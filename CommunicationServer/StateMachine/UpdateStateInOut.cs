using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities.PartialClasses;

namespace Taxi.Communication.Server.StateMachine
{
    public class UpdateStateInOut
    {
        public bool UpdateVehicleState(Vehicle pVehicle)
        {
            bool retVal = false;

            try
            {
                if (pVehicle.previousState.IDCurrentState() != pVehicle.currentState.IDCurrentState())
                {
                    StateInOut tmpStateInOut = new StateInOut();

                    tmpStateInOut.CurrStateKmGps = (decimal)pVehicle.currentGPSData.KmGPS;
                    tmpStateInOut.CurrStateKmTaximeter = (decimal)pVehicle.currentGPSData.KmTaxi;
                    tmpStateInOut.CurrStateTime = System.DateTime.Now;
                    tmpStateInOut.DateTimeIn = tmpStateInOut.CurrStateTime;
                    tmpStateInOut.IdLocationIn = pVehicle.currentGPSData.IdLocation;
                    tmpStateInOut.IdRegionIn = pVehicle.currentGPSData.IdRegion;
                    tmpStateInOut.IdVehicle = pVehicle.IdVehicle;
                    tmpStateInOut.IdStateIn = (long)pVehicle.currentState.IDCurrentState();                    

                    if (pVehicle.DriverShiftInOut != null)
                    {
                        tmpStateInOut.IdDriver = pVehicle.DriverShiftInOut.IdDriver;
                    }

                    DataRepository.StateInOutProvider.Insert(tmpStateInOut);
                }

                retVal = true;
            }
            catch (Exception ex)
            {                
            }


            return retVal;

        }
    }
}
