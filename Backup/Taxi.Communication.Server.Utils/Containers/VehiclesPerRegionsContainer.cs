using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Containers
{
    public class VehiclesPerRegionsContainer
    {
        private Dictionary<long, List<long>> _dictVehicleByRegion;
        private Dictionary<long, long> _vehicleReg;

        public VehiclesPerRegionsContainer()
        {
            _dictVehicleByRegion = new Dictionary<long, List<long>>();
            _vehicleReg = new Dictionary<long, long>();
        }

        public void setVehicleRegion(long ID_Vehicle, long ID_Region)
        {
            long currIdRegion = -1;
            if (_vehicleReg.ContainsKey(ID_Vehicle))
            {
                currIdRegion = _vehicleReg[ID_Vehicle];
                _vehicleReg[ID_Vehicle] = ID_Region;
            }
            else
            {
                _vehicleReg.Add(ID_Vehicle, ID_Region);
            }

            //Remove from the previous region
            if (_dictVehicleByRegion.ContainsKey(currIdRegion))
            {
                _dictVehicleByRegion[currIdRegion].Remove(ID_Vehicle);
            }

            //Add vehicle to new Region
            if (_dictVehicleByRegion.ContainsKey(ID_Region))
            {
                _dictVehicleByRegion[ID_Region].Add(ID_Vehicle);
            }
            else
            {
                _dictVehicleByRegion.Add(ID_Region, new List<long>());
                _dictVehicleByRegion[ID_Region].Add(ID_Vehicle);
            }



        }

        public List<long> getVehiclesForRegion(long ID_Region)
        {

            if (_dictVehicleByRegion.ContainsKey(ID_Region))
            {
                return _dictVehicleByRegion[ID_Region];
            }
            else
            {
                return new List<long>();
            }


        }
    }

}
