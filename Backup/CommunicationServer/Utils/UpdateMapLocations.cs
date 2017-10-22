using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.Reflection;
using System.Threading;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Data.Bases;
////using JP.Data.Utils;
using Taxi.Communication.Server.Containers;
using Microsoft.SqlServer.Types;
using System.Data.SqlClient;

using System.Configuration;
using GlobSaldo.GISDB.Entities;


namespace Taxi.Communication.Server.Utils
{
    public class UpdateMapLocations
    {
        private bool m_work;
        private static ILog log;
        private MapUtils m_mapUtils;
        private SynchQueue<Locations> m_locQueue;
        private string connStr;
        private Dictionary<long, GlobSaldo.AVL.Entities.Vehicle> dictVehicles = new Dictionary<long, GlobSaldo.AVL.Entities.Vehicle>();


        private bool checkForUnprocessed = false;

        public UpdateMapLocations(MapUtils mapUtils, SynchQueue<Locations> locQueue)
        {
            connStr = ConfigurationManager.ConnectionStrings["netTiersConnectionStringGISDB"].ConnectionString;
            m_work = true;
            log = LogManager.GetLogger("MyService");
            log.Debug("TEST");
            m_mapUtils = mapUtils;
            m_locQueue = locQueue;
            GlobSaldo.AVL.Entities.TList <GlobSaldo.AVL.Entities.Vehicle> tVehicles = DataRepository.VehicleProvider.GetAll();
            foreach (GlobSaldo.AVL.Entities.Vehicle tLocVeh in tVehicles)
            {
                dictVehicles.Add(tLocVeh.IdVehicle, tLocVeh);
            }
            checkForUnprocessed = true;

        }

        

        public void getUnProcessedLocations()
        {
            GlobSaldo.AVL.Entities.TList<Locations> lstLoc = DataRepository.LocationsProvider.GetUnProcessedLocations();

            if (lstLoc == null)
            {
                checkForUnprocessed = false;
                return;
            }

            if (lstLoc.Count == 0)
            {
                checkForUnprocessed = false;
                return;
            }

            foreach (Locations loc in lstLoc)
            {
                m_locQueue.Enqueue(loc);
            }

        }

        public void start()
        {
            while (m_work)
            {
                if (checkForUnprocessed)
                {
                    //getUnProcessedLocations();
                }
                //log.Debug(DateTime.Now.ToLongTimeString() + " Pravam Update na lokacii");
                //Console.WriteLine("----  Update MapLocations  " + DateTime.Now.ToLongTimeString () );

                /*
                TList<GlobSaldo.AVL.Entities.Locations> lstLocations;
                LocationsParameterBuilder tLocBuilder = new LocationsParameterBuilder();
                tLocBuilder.AppendIsNull(GlobSaldo.AVL.Entities.LocationsColumn.IdGeoLocation);

                lstLocations = DataRepository.LocationsProvider.Find(tLocBuilder.GetParameters());
                 */
                //Console.WriteLine ("Vkupno " + lstLocations.Count.ToString () + " lokacii" );

                int queueLen = this.m_locQueue.Count();
                try
                {
                    while (queueLen > 0)
                    {

                        //log.Info("In Location Queue " + queueLen.ToString());
                        //Console.WriteLine("In Location Queue " + queueLen.ToString());
                        Locations loc = this.m_locQueue.Dequeue();
                        try
                        {

                            //loc = checkForGeoLocationGIS(loc);
                            loc = checkForGeoLocationDB(loc);
                            //loc = checkForStationGIS(loc);
                            loc = checkForStationDB(loc);

                            loc.IsProcessed = true;
                            DataRepository.LocationsProvider.Update(loc);
                        }
                        catch (Exception e)
                        {
                            log.Error("ERROR", e);
                            DataRepository.LocationsProvider.Delete(loc);
                            log.Info("Greska vo coordinati ID_Vehicle=" + loc.IdVehicle.ToString() + " X=" + loc.LongitudeX.ToString() + " Y=" + loc.LatitudeY.ToString());
                        }
                        queueLen = this.m_locQueue.Count();
                    }
                }
                catch (Exception e)
                {
                    log.Error("ERROR", e);
                }

                Thread.Sleep(25000);
            }
        }

        /*
        public Locations checkForGeoLocationGIS(Locations currLoc)
        {
            //DateTime tProcessStart = DateTime.Now;

            // currLoc.IdGeoLocation = m_mapUtils.GetIdGeoLocationForLocation((double)currLoc.LongitudeX, (double)currLoc.LatitudeY);

            clsGeoLocation currGeoLocation = m_mapUtils.GetGeoLocationForLocationGIS((double)currLoc.LongitudeX, (double)currLoc.LatitudeY);

            if (currGeoLocation != null)
            {
                currLoc.IdGeoLocation = (long)currGeoLocation.IdGeoLocation;
                currLoc.GeoLocation = currGeoLocation.GeoLocationName;
            }
            else
            {
                currLoc.IdGeoLocation = -1;
                currLoc.GeoLocation = "Непозната локација";
            }

            //    //PAZI, Tuka treba IDCOmpany za toa vozilo



            //currLoc.IDGeoFence = m_mapUtils.GetIDGeoFenceForLocation((double)currLoc.LongitudeX, (double)currLoc.LatitudeY, dictVehicles[currLoc.IdVehicle].IdCompany);
            //DataRepository.LocationsProvider.Update(currLoc);

            //TimeSpan tPominatoVreme = DateTime.Now.Subtract(tProcessStart);
            //Console.WriteLine("UPDATE MAP LOCATIONS Obraboteni " + iBroj.ToString() + " od vkupno " + lstLocations.Count.ToString() + " lokacii za " + tPominatoVreme.TotalSeconds.ToString("00.00") + " sekundi");

            return currLoc;
        }
        */


        public Locations checkForGeoLocationDB(Locations currLoc)
        {
            Vehicle veh = DataRepository.VehicleProvider.GetByIdVehicle(currLoc.IdVehicle);

            GlobSaldo.AVL.Entities.VList<VgeoLocations> currGeoLocation = m_mapUtils.GetGeoLocationsForLocationDB((double)currLoc.LongitudeX, (double)currLoc.LatitudeY, veh.IdCompany);

            if (currGeoLocation.Count != 0)
            {
                currLoc.IdGeoLocation = (long)currGeoLocation[0].IdGeoLocation;
                currLoc.GeoLocation = currGeoLocation[0].GeoLocationName;
            }
            else
            {
                currLoc.IdGeoLocation = -1;
                currLoc.GeoLocation = "Непозната локација";
            }

            return currLoc;
        }

        
        //public Locations checkForStationGIS1(Locations currLoc)
        //{
        //    bool retVal = false;

        //    retVal = m_mapUtils.GetIsTaxiAtStationGIS1((double)currLoc.LongitudeX, (double)currLoc.LatitudeY);

        //    if (retVal == true)
        //    {
        //        currLoc.Station = true;
        //    }
        //    else
        //    {
        //        currLoc.Station = false;
        //    }

        //    return currLoc;
        //}
        
        public Locations checkForStationDB(Locations currLoc)
        {
            bool retVal = false;

            retVal = m_mapUtils.GetIsTaxiAtStationDB((double)currLoc.LongitudeX, (double)currLoc.LatitudeY);

            if (retVal == true)
            {
                currLoc.Station = true;
            }
            else
            {
                currLoc.Station = false;
            }

            return currLoc;
        }       
       

        public void Stop()
        {
            this.m_work = false;
        }

        private List<long> selectGeoFences(Locations loc, long ID_Company)
        {
            //Microsoft.SqlServer.Types.SqlGeometry

            SqlConnection conn = new SqlConnection(this.connStr);
            conn.Open();

            SqlCommand cmd = new SqlCommand("getGeoFencesForCompanyLocation", conn);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;

            SqlParameter desc = new SqlParameter();
            desc.Direction = System.Data.ParameterDirection.Input;
            desc.ParameterName = "@ID_Company";
            desc.SqlDbType = System.Data.SqlDbType.BigInt;
            desc.Value = ID_Company;



            SqlGeographyBuilder gLocationBuilder = new SqlGeographyBuilder();
            gLocationBuilder.SetSrid(4326);
            //gLocationBuilder.SetSrid(0);
            gLocationBuilder.BeginGeography(OpenGisGeographyType.Point);

            //gLocationBuilder.BeginFigure(41.999370, 21.408241 );

            gLocationBuilder.BeginFigure((double)loc.LongitudeX, (double)loc.LatitudeY);
            //gLocationBuilder.BeginFigure(21.406267, 41.999435);


            gLocationBuilder.EndFigure();
            gLocationBuilder.EndGeography();

            // Create the geometry parameter  
            SqlParameter pLocation =
               new SqlParameter("@point", gLocationBuilder.ConstructedGeography);
            pLocation.UdtTypeName = "geography";

            cmd.Parameters.Add(pLocation);
            cmd.Parameters.Add(desc);


            SqlDataReader reader = cmd.ExecuteReader();

            List<long> retVal = new List<long>();
            while (reader.Read())
            {
                retVal.Add((long)reader.GetSqlInt64(0));
                //log.Info(((long)reader.GetSqlInt64(0)).ToString());
            }

            reader.Close();
            conn.Close();
            return retVal;
        }

    }
}
