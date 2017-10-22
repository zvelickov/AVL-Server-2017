using System;
using System.Collections.Generic;
using System.Text;

using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Data.Bases;
//using JP.Data.Utils;

namespace Tests
{
    class Program
    {
        public static double Calc(double Lat1,
                  double Long1, double Lat2, double Long2)
        {
            /*
                The Haversine formula according to Dr. Math.
                http://mathforum.org/library/drmath/view/51879.html
                
                dlon = lon2 - lon1
                dlat = lat2 - lat1
                a = (sin(dlat/2))^2 + cos(lat1) * cos(lat2) * (sin(dlon/2))^2
                c = 2 * atan2(sqrt(a), sqrt(1-a)) 
                d = R * c
                
                Where
                    * dlon is the change in longitude
                    * dlat is the change in latitude
                    * c is the great circle distance in Radians.
                    * R is the radius of a spherical Earth.
                    * The locations of the two points in 
                        spherical coordinates (longitude and 
                        latitude) are lon1,lat1 and lon2, lat2.
            */
            double dDistance = Double.MinValue;
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;

            // Intermediate result a.
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2.0) +
                       Math.Cos(dLat1InRad) * Math.Cos(dLat2InRad) *
                       Math.Pow(Math.Sin(dLongitude / 2.0), 2.0);

            // Intermediate result c (great circle distance in Radians).
            double c = 2.0 * Math.Asin(Math.Sqrt(a));

            // Distance.
            // const Double kEarthRadiusMiles = 3956.0;
            const Double kEarthRadiusKms = 6376.5;
            dDistance = kEarthRadiusKms * c;

            return dDistance;
        }

        

        static void Main(string[] args)
        {
            DateTime prevDate = new DateTime(2011, 11, 8);
            DateTime endDate = new DateTime(2011, 11, 14);
            TList<Vehicle> vehs = DataRepository.VehicleProvider.GetAll();
            Dictionary<long, double> vehDist = new Dictionary<long, double>();
            double dist = 0;
            TimeSpan timeLim = new TimeSpan(0, 0, 30);
            foreach (Vehicle veh in vehs)
            {
                dist = 0;
                Console.WriteLine("Vehicle " + veh.IdVehicle.ToString());
                LocationsParameterBuilder locParam = new LocationsParameterBuilder();
                locParam.AppendGreaterThanOrEqual(LocationsColumn.LocalTime, prevDate.ToString("yyyy-MM-dd HH:mm:ss"));
                locParam.AppendLessThan(LocationsColumn.LocalTime, endDate.ToString("yyyy-MM-dd HH:mm:ss"));
                locParam.AppendEquals(LocationsColumn.IdVehicle, veh.IdVehicle.ToString());
                locParam.AppendEquals(LocationsColumn.IdVehicleState, "1");
                Console.WriteLine("Selecting locations");
                TList<Locations> lloc = DataRepository.LocationsProvider.Find(locParam.GetParameters());
                lloc.Sort("LocalTime ASC");
                Locations lastLoc = null;
                Console.WriteLine("Procesing locations");
                foreach (Locations loc in lloc)
                {
                    
                    if ( (lastLoc != null ) && (loc.LocalTime - lastLoc.LocalTime < timeLim) ) 
                    {
                        dist = dist + Program.Calc
                            (Decimal.ToDouble(loc.LatitudeY), Decimal.ToDouble(loc.LongitudeX), 
                            Decimal.ToDouble(lastLoc.LatitudeY), Decimal.ToDouble(lastLoc.LongitudeX));
                        
                    }
                    lastLoc = loc;
                }
                vehDist.Add(veh.IdVehicle, dist);
                
            }
            foreach (long key in vehDist.Keys)
            {
                Console.WriteLine(key.ToString() + " " + vehDist[key].ToString());
            }
            Console.ReadLine();
        }
    }

   
}
