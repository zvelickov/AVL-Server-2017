using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Net.Mime;
using MapXLib;
using log4net;
using Taxi.Communication.Server.Classes;
using GlobSaldo.GISDB.Entities;
using GlobSaldo.GISDB.Data;
using JP.Data.Utils;
using GlobSaldo.AVL.Entities;



namespace Taxi.Communication.Server.Utils
{
    public class MapUtils
    {
        //public static AxMapXLib.AxMap axMap1;
        public MapXLib.Map axMap1;

        //public static StateMachine.frmMapX  frmGetGraphicalData;
        public bool ImaLeerRegioni = false;
        public bool ImaLeerStanici = false;
        public bool ImaLeerGeoLocations = false;
        public bool ImaLeerGeoFences = false;

        private MapXLib.Layer tLayerRegioni;
        private MapXLib.Layer tLayerStanici;
        private MapXLib.Layer tLayerGeoLocations;
        private MapXLib.Layer tLayerGeoFences;



        public MapUtils()
        {
            //OVA E od Jove, so odvoeno od forma, ama nesaka
            initMap(); //MapX Funkciite se vo ovaa klasa 
            VcitajLeer(); //MapX Funkciite se vo ovaa klasa
            //Console.WriteLine(GetIDRegionForLocation(21.0012, 41.590214));
        }

        public void initMap()
        {

            //axMap1 = new AxMapXLib.AxMap();
            axMap1 = new MapXLib.MapClass();
            //axMap1.GeoDictionary = "";
            //axMap1.BeginInit();
            //axMap1.Location = new System.Drawing.Point(2, 1);
            //axMap1.Name = "axMap1";
        }
        
        
        public void VcitajLeer()
        {
            
            string tFolderPodatoci = ConfigurationManager.AppSettings["patekaZaTab"];


            if (!System.IO.Directory.Exists(tFolderPodatoci))
            {

                ServiceCallBack.log.Debug(" **************** Ne postoi patekata za TAB file-ovite !!! **********************");
            }

            //string tFolderPodatoci = "c:\\projects\\globsaldo_repository\\Taxi\\Dispecher\\Podatoci\\";


            //Leer Regioni
            string tPatekaPodatoci = tFolderPodatoci + "Regions.tab";
            //vcituvam leer regioni
            if (!System.IO.File.Exists(tPatekaPodatoci))
            {
                //Nema fajl, 
                ServiceCallBack.log.Debug(" **************** Ne postoi TAB file za Regioni !!! **********************");
            }
            else
            {
                try
                {
                    tLayerRegioni = axMap1.Layers.Add(tPatekaPodatoci, 1);
                    axMap1.DataSets.Add(MapXLib.DatasetTypeConstants.miDataSetLayer, tLayerRegioni, tLayerRegioni.Name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ImaLeerRegioni = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }


            //Znae da zaebe rabota ako nema Geodictionary (pa go teram uste ednas da proba so regionite
            //Kako na nevidliv objekt da mu se kaze da ne bara Georegion?
            if (!ImaLeerRegioni && System.IO.File.Exists(tPatekaPodatoci))
            {
                try
                {
                    tLayerRegioni = axMap1.Layers.Add(tPatekaPodatoci, 1);
                    axMap1.DataSets.Add(MapXLib.DatasetTypeConstants.miDataSetLayer, tLayerRegioni, tLayerRegioni.Name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ImaLeerRegioni = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }




            //Leer Stanici
            tPatekaPodatoci = tFolderPodatoci + "Stations.tab";
            if (!System.IO.File.Exists(tPatekaPodatoci))
            {
                //Nema fajl, 
                ServiceCallBack.log.Debug(" **************** Ne postoi TAB file za Stations !!! **********************");
            }
            else
            {
                try
                {
                    tLayerStanici = axMap1.Layers.Add(tPatekaPodatoci, 1);
                    axMap1.DataSets.Add(MapXLib.DatasetTypeConstants.miDataSetLayer, tLayerStanici, tLayerStanici.Name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ImaLeerStanici = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }



            //Leer GeoLocations
            tPatekaPodatoci = tFolderPodatoci + "GeoLocations.tab";
            if (!System.IO.File.Exists(tPatekaPodatoci))
            {
                //Nema fajl, 
                ServiceCallBack.log.Debug(" **************** Ne postoi TAB file za GeoLocations !!! **********************");
            }
            else
            {
                try
                {
                    tLayerGeoLocations = axMap1.Layers.Add(tPatekaPodatoci, 1);
                    axMap1.DataSets.Add(MapXLib.DatasetTypeConstants.miDataSetLayer, tLayerGeoLocations, tLayerGeoLocations.Name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ImaLeerGeoLocations = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            //Leer GeoLocations
            tPatekaPodatoci = tFolderPodatoci + "GeoFences.TAB";
            if (!System.IO.File.Exists(tPatekaPodatoci))
            {
                //Nema fajl, 
                ServiceCallBack.log.Debug(" **************** Ne postoi TAB file za GeoFences !!! **********************");
            }
            else
            {
                try
                {
                    tLayerGeoFences = axMap1.Layers.Add(tPatekaPodatoci, 1);
                    axMap1.DataSets.Add(MapXLib.DatasetTypeConstants.miDataSetLayer, tLayerGeoFences, tLayerGeoFences.Name, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                    ImaLeerGeoFences = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }


        public VList<VRegions> GetGisRegionForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            VList<VRegions> retVal = null;

            try
            {
                retVal = DataRepository.VRegionsProvider.uGetRegions((decimal)Longitude_X, (decimal)Latitude_Y, pID_Company);

            }
            catch (Exception ex)
            {
                //ServiceCallBack.log.Error("Greska vo procesiranje na GisREgionsDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retVal = null;
            }

            return retVal;
        }

        public VList<VGeoLocations> GetGeoLocationsForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            VList<VGeoLocations> retVal = null;

            try
            {
                retVal = DataRepository.VGeoLocationsProvider.uGetGeoLocations((decimal)Longitude_X, (decimal)Latitude_Y);

            }
            catch (Exception ex)
            {
                ServiceCallBack.log.Error("Greska vo procesiranje na GetGeoLocationsForLocationDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retVal = null;
            }

            return retVal;
        }

        public long GetIDGeoFenceForLocationDB(double Longitude_X, double Latitude_Y, long IDCompany)
        {

            return -1;

        }

        public VList<VStations> GetStationsForLocationDB(double Longitude_X, double Latitude_Y, long pID_Company)
        {
            VList<VStations> retVal = null;

            try
            {
                retVal = DataRepository.VStationsProvider.uGetStations((decimal)Longitude_X, (decimal)Latitude_Y, pID_Company);

            }
            catch (Exception ex)
            {
                ServiceCallBack.log.Error("Greska vo procesiranje na GetStationsForLocationDB" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString(), ex);
                retVal = null;
            }

            return retVal;
        }

        public bool GetIsTaxiAtStationDB(double Longitude_X, double Latitude_Y)
        {

            VList<VStations> retVal = GetStationsForLocationDB(Longitude_X, Latitude_Y, 1);

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

        public long GetIDRegionForLocationGIS(double Longitude_X, double Latitude_Y)
        {
            if (!ImaLeerRegioni) return -1;

            MapXLib.Point myPoint = new MapXLib.Point();
            myPoint.Set(Longitude_X, Latitude_Y);

            MapXLib.Features myFs = tLayerRegioni.SearchAtPoint(myPoint, Type.Missing);

            if (myFs.Count == 0)
            {
                //Ako nema Region vo taa tocka vrati 0\
                return 0;
            }
            else
            {
                // I da najde poveke (sto ne bi trebalo, zemi go prviot
                MapXLib.Feature myF = myFs[1];
                tLayerRegioni.KeyField = "ID_Region";
                return Convert.ToInt16(myF.KeyValue);
            }
        }

        public clsGisRegion GetGisRegionForLocationGIS(double Longitude_X, double Latitude_Y)
        {
            clsGisRegion retVal = null;
            string tmpID_Region = "";

            if (!ImaLeerRegioni)
                return retVal;

            MapXLib.Point myPoint = new MapXLib.Point();
            myPoint.Set(Longitude_X, Latitude_Y);

            MapXLib.Features myFs = tLayerRegioni.SearchAtPoint(myPoint, Type.Missing);

            if (myFs.Count == 0)
            {
                //Ako nema Region vo taa tocka vrati 0\
                return retVal;
            }
            else
            {
                try
                {
                    retVal = new clsGisRegion();

                    // I da najde poveke (sto ne bi trebalo, zemi go prviot
                    MapXLib.Feature myF = myFs[1];

                    tLayerRegioni.KeyField = "ID_Region";
                    tmpID_Region = myF.KeyValue.Trim();

                    retVal.IDRegion = Convert.ToInt16(myF.KeyValue);

                    tLayerRegioni.KeyField = "RegionName";
                    retVal.GisRegionName = myF.KeyValue;
                    tLayerRegioni.KeyField = "ID_Region";

                }
                catch (Exception ex)
                {
                    ServiceCallBack.log.Error("Greska vo procesiranje na GisREgions" + " X=" + Longitude_X.ToString() + " Y =" + Latitude_Y.ToString() + " ID_Region =" + tmpID_Region + "----", ex);
                    retVal = null;
                }

                return retVal;

            }
        }

        public clsGeoLocation GetGeoLocationForLocationGIS(double Longitude_X, double Latitude_Y)
        {
            clsGeoLocation retVal = null;

            if (!ImaLeerGeoLocations)
                return retVal;

            MapXLib.Point myPoint = new MapXLib.Point();
            myPoint.Set(Longitude_X, Latitude_Y);

            MapXLib.Features myFs = tLayerGeoLocations.SearchAtPoint(myPoint, Type.Missing);

            if (myFs.Count == 0)
            {
                //Ako nema Region vo taa tocka vrati null
                return retVal;
            }
            else
            {
                try
                {
                    retVal = new clsGeoLocation();

                    // I da najde poveke (sto ne bi trebalo, zemi go prviot
                    MapXLib.Feature myF = myFs[1];

                    tLayerGeoLocations.KeyField = "IDGeoLocation";
                    retVal.IDGeoLocation = Convert.ToInt16(myF.KeyValue);

                    tLayerGeoLocations.KeyField = "OpisGeoLocation";
                    retVal.GeoLocationName = myF.KeyValue;
                }
                catch (Exception ex)
                {
                    ServiceCallBack.log.Error("Greska vo procesiranje na GeoLocation", ex);
                    retVal = null;
                }
            }

            return retVal;
        }

        public long GetIDGeoFenceForLocationGIS(double Longitude_X, double Latitude_Y, long IDCompany)
        {

            return -1;
            if (!ImaLeerGeoLocations) return -1;
            long retVal = 0;

            //Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[1].Name);
            //Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[2].Name);
            //Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[3].Name);
            //Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[4].Name);
            //Console.WriteLine("#" + tLayerGeoFences.DataSets[1].Fields[5].Name);


            MapXLib.Point myPoint = new MapXLib.Point();
            myPoint.Set(Longitude_X, Latitude_Y);

            MapXLib.Features myFs = tLayerGeoFences.SearchAtPoint(myPoint, Type.Missing);

            try
            {
                if (myFs.Count == 0)
                {
                    //Ako nema Region vo taa tocka vrati 0\
                    retVal = 0;
                }
                else
                {
                    for (int i = 1; i <= myFs.Count; i++)
                    {
                        MapXLib.Feature myF = myFs[i];
                        tLayerGeoFences.KeyField = "IDCompany";
                        if (Convert.ToInt16(myF.KeyValue) == IDCompany)
                        {
                            tLayerGeoFences.KeyField = "IDGeoFence";
                            retVal = Convert.ToInt16(myF.KeyValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                retVal = -1;
            }

            return retVal;
        }

        public bool GetIsTaxiAtStationGIS(double Longitude_X, double Latitude_Y)
        {

            // Treba da se vidi so Nase Taxi dali default da bide true ili false
            if (!ImaLeerStanici) return false;

            MapXLib.Point myPoint = new MapXLib.Point();
            myPoint.Set(Longitude_X, Latitude_Y);

            MapXLib.Features myFs = tLayerStanici.SearchAtPoint(myPoint, Type.Missing);

            if (myFs.Count == 0)
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
        

        
        public void GetIsTaxiAtStationSensorGIS(Vehicle pVehicle, double Longitude_X, double Latitude_Y, int SensorStation)
        {

            if (SensorStation == 1)
            {
                if (pVehicle.PreviousStationState == false && ( pVehicle.currentStateString == "StateIdle" || pVehicle.currentStateString == "StateUndefined" ))
                {
                    bool tmpIsTaxiAtStationGIS = GetIsTaxiAtStationGIS(Longitude_X, Latitude_Y);

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