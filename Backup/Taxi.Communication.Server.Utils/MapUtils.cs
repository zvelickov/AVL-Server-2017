using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net.Mime;
//using MapXLib;
using log4net;
using Taxi.Communication.Server.Utils.Classes;
using GlobSaldo.GISDB.Entities;
using GlobSaldo.GISDB.Data;
// using JP.Data.Utils;
using GlobSaldo.AVL.Entities;



namespace Taxi.Communication.Server.Utils
{
    public class MapUtils
    {
        //public static AxMapXLib.AxMap axMap1;
        //public MapXLib.Map axMap1;

        //public static StateMachine.frmMapX  frmGetGraphicalData;
        public bool ImaLeerRegioni = false;
        public bool ImaLeerStanici = false;
        public bool ImaLeerGeoLocations = false;
        public bool ImaLeerGeoFences = false;

        //private MapXLib.Layer tLayerRegioni;
        //private MapXLib.Layer tLayerStanici;
        //private MapXLib.Layer tLayerGeoLocations;
        //private MapXLib.Layer tLayerGeoFences;



        public MapUtils(string pathForTab)
        {
            //OVA E od Jove, so odvoeno od forma, ama nesaka
            initMap(); //MapX Funkciite se vo ovaa klasa 
            VcitajLeer(pathForTab); //MapX Funkciite se vo ovaa klasa
            //Console.WriteLine(GetIdRegionForLocation(21.0012, 41.590214));
        }

        public void initMap()
        {
            //Stara metoda, ne se koristi veke...           
        }


        public void VcitajLeer(string tFolderPodatoci)
        {
            //Stara metoda, ne se koristi veke...
        }


        public GlobSaldo.AVL.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> GetGisRegionForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> retValGisLib = null;
            GlobSaldo.AVL.Entities.VList<GlobSaldo.GISDB.Entities.Vregions> retVal = new GlobSaldo.AVL.Entities.VList<Vregions>();

            try
            {
                retValGisLib = DataRepository.VregionsProvider.uGetRegions((decimal)Longitude_X, (decimal)Latitude_Y, pID_Company);

            }
            catch (Exception ex)
            {
                //ServiceCallBack.log.Error("Greska vo procesiranje na GisREgionsDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retValGisLib = null;
            }

            if (retValGisLib != null)
            {
                foreach (Vregions tmpVgeo in retValGisLib)
                    retVal.Add(tmpVgeo);
            }
            else
            {
                retVal = null;
            }

            return retVal;
        }


        public GlobSaldo.AVL.Entities.VList<VgeoLocations> GetGeoLocationsForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.VgeoLocations> retValGisLib = null;
            GlobSaldo.AVL.Entities.VList<GlobSaldo.GISDB.Entities.VgeoLocations> retVal = new GlobSaldo.AVL.Entities.VList<VgeoLocations>();

            try
            {
                retValGisLib = DataRepository.VgeoLocationsProvider.uGetGeoLocations((decimal)Longitude_X, (decimal)Latitude_Y);

            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("Greska vo procesiranje na GetGeoLocationsForLocationDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retValGisLib = null;
            }

            if (retValGisLib != null)
            {
                foreach (VgeoLocations tmpVgeo in retValGisLib)
                    retVal.Add(tmpVgeo);
            }
            else
            {
                retVal = null;
            }

            return retVal;
        }


        public long GetIDGeoFenceForLocationDB(double Longitude_X, double Latitude_Y, long IdCompany)
        {

            return -1;

        }

        public GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vstations> GetStationsForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vstations> retVal = null;

            try
            {
                retVal = DataRepository.VstationsProvider.uGetStations((decimal)Longitude_X, (decimal)Latitude_Y, pID_Company);

            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("Greska vo procesiranje na GetStationsForLocationDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retVal = null;
            }

            return retVal;
        }

        public bool GetIsTaxiAtStationDB(double Longitude_X, double Latitude_Y)
        {

            GlobSaldo.GISDB.Entities.VList<GlobSaldo.GISDB.Entities.Vstations> retVal = GetStationsForLocationDB(Longitude_X, Latitude_Y, 1);

            if (retVal.Count == 0)
            {
                //Ako ne najde objekt, znaci ne e na stanica (ne se grizam za toa koja stanica, zasto prethodno procitav region
                return false;
            }
            else
            {
                // I da najde poveke (sto ne bi trebalo, zemi go prviot
                return true;
            }
        }

        //public long GetIdRegionForLocationGIS1(double Longitude_X, double Latitude_Y)
        //{
        //    //if (!ImaLeerRegioni) return -1;

        //    //MapXLib.Point myPoint = new MapXLib.Point();
        //    //myPoint.Set(Longitude_X, Latitude_Y);

        //    //MapXLib.Features myFs = tLayerRegioni.SearchAtPoint(myPoint, Type.Missing);

        //    //if (myFs.Count == 0)
        //    //{
        //    //    //Ako nema Region vo taa tocka vrati 0\
        //    //    return 0;
        //    //}
        //    //else
        //    //{
        //    //    // I da najde poveke (sto ne bi trebalo, zemi go prviot
        //    //    MapXLib.Feature myF = myFs[1];
        //    //    tLayerRegioni.KeyField = "ID_Region";
        //    //    return Convert.ToInt16(myF.KeyValue);
        //    //}
        //}

        //public clsGisRegion GetGisRegionForLocationGIS1(double Longitude_X, double Latitude_Y)
        //{
        //    //clsGisRegion retVal = null;
        //    //string tmpID_Region = "";

        //    //if (!ImaLeerRegioni)
        //    //    return retVal;

        //    //MapXLib.Point myPoint = new MapXLib.Point();
        //    //myPoint.Set(Longitude_X, Latitude_Y);

        //    //MapXLib.Features myFs = tLayerRegioni.SearchAtPoint(myPoint, Type.Missing);

        //    //if (myFs.Count == 0)
        //    //{
        //    //    //Ako nema Region vo taa tocka vrati 0\
        //    //    return retVal;
        //    //}
        //    //else
        //    //{
        //    //    try
        //    //    {
        //    //        retVal = new clsGisRegion();

        //    //        // I da najde poveke (sto ne bi trebalo, zemi go prviot
        //    //        MapXLib.Feature myF = myFs[1];

        //    //        tLayerRegioni.KeyField = "ID_Region";
        //    //        tmpID_Region = myF.KeyValue.Trim();

        //    //        retVal.IdRegion = Convert.ToInt16(myF.KeyValue);

        //    //        tLayerRegioni.KeyField = "RegionName";
        //    //        retVal.GisRegionName = myF.KeyValue;
        //    //        tLayerRegioni.KeyField = "ID_Region";

        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        log4net.LogManager.GetLogger("MyService").Error("Greska vo procesiranje na GisREgions" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString() + " ID_Region =" + tmpID_Region + "----", ex);
        //    //        retVal = null;
        //    //    }

        //    //    return retVal;

        //    //}
        //}

        //public clsGeoLocation GetGeoLocationForLocationGIS1(double Longitude_X, double Latitude_Y)
        //{
        //    //clsGeoLocation retVal = null;

        //    //if (!ImaLeerGeoLocations)
        //    //    return retVal;

        //    //MapXLib.Point myPoint = new MapXLib.Point();
        //    //myPoint.Set(Longitude_X, Latitude_Y);

        //    //MapXLib.Features myFs = tLayerGeoLocations.SearchAtPoint(myPoint, Type.Missing);

        //    //if (myFs.Count == 0)
        //    //{
        //    //    //Ako nema Region vo taa tocka vrati null
        //    //    return retVal;
        //    //}
        //    //else
        //    //{
        //    //    try
        //    //    {
        //    //        retVal = new clsGeoLocation();

        //    //        // I da najde poveke (sto ne bi trebalo, zemi go prviot
        //    //        MapXLib.Feature myF = myFs[1];

        //    //        tLayerGeoLocations.KeyField = "IdGeoLocation";
        //    //        retVal.IdGeoLocation = Convert.ToInt16(myF.KeyValue);

        //    //        tLayerGeoLocations.KeyField = "OpisGeoLocation";
        //    //        retVal.GeoLocationName = myF.KeyValue;
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        log4net.LogManager.GetLogger("MyService").Error("Greska vo procesiranje na GeoLocation", ex);
        //    //        retVal = null;
        //    //    }
        //    //}

        //    //return retVal;
        //}

        //public long GetIDGeoFenceForLocationGIS1(double Longitude_X, double Latitude_Y, long IdCompany)
        //{

        //    ////return -1;
        //    ////if (!ImaLeerGeoLocations) return -1;
        //    ////long retVal = 0;

        //    //////Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[1].Name);
        //    //////Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[2].Name);
        //    //////Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[3].Name);
        //    //////Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[4].Name);
        //    //////Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[5].Name);


        //    ////MapXLib.Point myPoint = new MapXLib.Point();
        //    ////myPoint.Set(Longitude_X, Latitude_Y);

        //    ////MapXLib.Features myFs = tLayerGeoFences.SearchAtPoint(myPoint, Type.Missing);

        //    ////try
        //    ////{
        //    ////    if (myFs.Count == 0)
        //    ////    {
        //    ////        //Ako nema Region vo taa tocka vrati 0\
        //    ////        retVal = 0;
        //    ////    }
        //    ////    else
        //    ////    {
        //    ////        for (int i = 1; i <= myFs.Count; i++)
        //    ////        {
        //    ////            MapXLib.Feature myF = myFs[i];
        //    ////            tLayerGeoFences.KeyField = "IdCompany";
        //    ////            if (Convert.ToInt16(myF.KeyValue) == IdCompany)
        //    ////            {
        //    ////                tLayerGeoFences.KeyField = "IDGeoFence";
        //    ////                retVal = Convert.ToInt16(myF.KeyValue);
        //    ////            }
        //    ////        }
        //    ////    }
        //    ////}
        //    ////catch (Exception ex)
        //    ////{
        //    ////    Console.WriteLine(ex.Message);
        //    ////    retVal = -1;
        //    ////}

        //    ////return retVal;
        //}

        //public bool GetIsTaxiAtStationGIS1(double Longitude_X, double Latitude_Y)
        //{

        //    ////// Treba da se vidi so Nase Taxi dali default da bide true ili false
        //    ////if (!ImaLeerStanici) return false;

        //    ////MapXLib.Point myPoint = new MapXLib.Point();
        //    ////myPoint.Set(Longitude_X, Latitude_Y);

        //    ////MapXLib.Features myFs = tLayerStanici.SearchAtPoint(myPoint, Type.Missing);

        //    ////if (myFs.Count == 0)
        //    ////{
        //    ////    //Ako ne najde objekt, znaci ne e na stanica (ne se grizam za toa koja stanica, zasto prethodno procitav region
        //    ////    return false;
        //    ////}
        //    ////else
        //    ////{
        //    ////    // I da najde poveke (sto ne bi trebalo, zemi go prviot
        //    ////    return true;
        //    ////}
        //}



        //public void GetIsTaxiAtStationSensorGIS1(Vehicle pVehicle, double Longitude_X, double Latitude_Y, int SensorStation)
        //{

        //    ////if (SensorStation == 1)
        //    ////{
        //    ////    if (pVehicle.PreviousStationState == false && (pVehicle.currentStateString == "StateIdle" || pVehicle.currentStateString == "StateUndefined"))
        //    ////    {
        //    ////        bool tmpIsTaxiAtStationGIS = GetIsTaxiAtStationGIS1(Longitude_X, Latitude_Y);

        //    ////        if (tmpIsTaxiAtStationGIS == true)
        //    ////        {
        //    ////            pVehicle.Station = true;
        //    ////        }
        //    ////    }

        //    ////}
        //    ////else
        //    ////{
        //    ////    pVehicle.Station = false;
        //    ////}

        //}


        public void GetIsTaxiAtStationSensorDB(Vehicle pVehicle, double Longitude_X, double Latitude_Y, int SensorStation)
        {

            if (SensorStation == 1)
            {
                if (pVehicle.PreviousStationState == false && (pVehicle.currentStateString == "StateIdle" || pVehicle.currentStateString == "StateUndefined"))
                {
                    bool tmpIsTaxiAtStationGIS = GetIsTaxiAtStationDB(Longitude_X, Latitude_Y);

                    if (tmpIsTaxiAtStationGIS == true)
                    {
                        pVehicle.Station = true;
                    }
                }

            }
            else
            {
                pVehicle.Station = false;
            }

        }


    }

}
