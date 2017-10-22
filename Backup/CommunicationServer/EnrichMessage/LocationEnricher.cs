using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.ConnectionListeners;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.Containers;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Utils;
using log4net;

using GlobSaldo.GISDB.Entities;
using Taxi.Communication.Server.Utils.Containers;
using Taxi.Communication.Server.Utils.Parsers;

using System.IO;

namespace Taxi.Communication.Server.EnrichMessage
{
    public class LocationEnricher : IEnrichMessageHandler
    {
        private GPSListener _gpsListener = null;
        private MapUtils _mapUtils;
        public ILog log;

       
        public LocationEnricher(GPSListener gpsListener, MapUtils mapUtils)
        {
            _gpsListener = gpsListener;
            _mapUtils = mapUtils;

            log = LogManager.GetLogger("MyService");
        }

        #region IEnrichMessageHandler Members

        public bool canHandle(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {
            if (message.tLocation != null)
                return true;
            else
                return false;
        }

        public long enrichMessage(Taxi.Communication.Server.Utils.Parsers.ParserResponseContainer message)
        {

            try
            {
                Vehicle veh = VehiclesContainer.Instance.getVehicleObjForUnitSerial(message.msg.DeviceNumber);

                message.tLocation.IdVehicle = veh.IdVehicle;
                message.tLocation.IdUnit = (long)veh.IdUnit;

                if (veh.DriverShiftInOut != null)
                {
                    message.tLocation.AnalogSensor = (int)veh.DriverShiftInOut.IdDriver; //PAZI!!! (NEMAM poim kade OVA se koristi)
                    message.tLocation.IdDriver = veh.DriverShiftInOut.IdDriver;
                }
                message.tLocation.LocalTime = CalculateLocalTime(message.tLocation.Utc, veh);

                // ZORAN:   Cesto znae da pukne glup datum (na primer godina 2080)
                //          Zatoa tuka, ako ima razlika so SystemTime > 10 min, go stavam tekovnoto vreme vo Local time
                //          DOPOLNITELNO, moze da padne bidejki moze da se primi Utc sto e 1000 god vo idnina, pa try/catch

                try
                {
                    TimeSpan ts = ((System.DateTime)message.tLocation.LocalTime - (System.DateTime)message.tLocation.SystemTime);

                    if (ts.TotalMinutes > 10)
                        message.tLocation.LocalTime = (System.DateTime)message.tLocation.SystemTime;
                }
                catch(Exception ex)
                {
                    message.tLocation.LocalTime = (System.DateTime)message.tLocation.SystemTime;
                }
                
                message.tLocation.IdVehicleState = (long)veh.currentState.IDCurrentState();
                message.tLocation.Station = veh.Station;

                // Prvo potvrda do uredot deka porakata e primena
                if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, message.msg.ReturnMessage) == -1)
                {
                    Console.WriteLine("GRESKA vo prakanje potvrda za poraka: " + message.msg.Command);
                    log.Error("GRESKA vo prakanje poraka za potvrda na priem na poraka AA08: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                }
                else
                {
                    //Console.WriteLine("Pratena potvrda za poraka: " + message.msg.Command);
                }

                //probuvam da otkrijam vo koj region e vozilo, i dali se naoga na stanica
                //Pazi tuka, vo koj region se naoga i dali e na stanica se cuva vo CurrentGPS Data na vozilo (a vreme od koga e na stanica na samo vozilo)
                try
                {
                    GlobSaldo.AVL.Entities.VList<Vregions> tmpDBregions = _mapUtils.GetGisRegionForLocationDB(message.tLocation.GpsData.Longitude_X, message.tLocation.GpsData.Latutude_Y, veh.IdCompany);


                    if (tmpDBregions != null && tmpDBregions.Count > 0)
                    {
                        message.tLocation.IdRegion = (long)tmpDBregions[0].IdRegion;
                        message.tLocation.RegionName = tmpDBregions[0].RegionName;

                        message.tLocation.GpsData.IdRegion = (long)tmpDBregions[0].IdRegion;
                        message.tLocation.GpsData.RegionName = tmpDBregions[0].RegionName;
                    }
                    else
                    {
                        message.tLocation.IdRegion = (long)-1;
                        message.tLocation.RegionName = "Непознат регион";

                        message.tLocation.GpsData.IdRegion = (long)-1;
                        message.tLocation.GpsData.RegionName = "Непознат регион";
                    }


                    GlobSaldo.AVL.Entities.VList<VgeoLocations> tmpDBlocations = _mapUtils.GetGeoLocationsForLocationDB(message.tLocation.GpsData.Longitude_X, message.tLocation.GpsData.Latutude_Y, veh.IdCompany);

                    if (tmpDBlocations != null && tmpDBlocations.Count > 0)
                    {
                        message.tLocation.GeoLocation = tmpDBlocations[0].GeoLocationName;
                        message.tLocation.IdLocation = (long)tmpDBlocations[0].IdGeoLocation;
                    } 
  
                    //ZORAN:    Na 05.02.2016 e dodadeno da mu go resetira vremeto na posledniot taksimetar ako premine od nulti vo prvi/vtori reon i obratno
                    //          Dopolnitelno, tuka se praka i poraka do voziloto
                    //          Ova samo ako e vo state Idle

                    if (veh.currentStateString == "StateIdle")
                    {
                        if (IsSearchRegionChanged(veh.currentGPSData.IdRegion, message.tLocation.GpsData.IdRegion))
                        {
                            veh.TaximetarLast = System.DateTime.Now;

                            clsMessageCreator mMessageCreator = new clsMessageCreator();

                            byte[] mPopUpMessage = mMessageCreator.CreateNewPopUpMessageForLCD(veh, "Preminavte od region " + veh.currentGPSData.IdRegion.ToString() + " vo  region " + message.tLocation.GpsData.IdRegion.ToString() + "! Vremeto vi se resetira!", '4');

                            if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, mPopUpMessage) == -1)
                            {                                
                                log.Error("GRESKA vo prakanje poraka za resetiranje na taksimetar " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                            }
                        }
                    }
                    
                }
                catch (Exception e)
                {
                    log.Error("VRegion Error location save", e);

                    System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(message.tLocation.GetType());
                    StringWriter textWriter = new StringWriter();

                    x.Serialize(textWriter, message.tLocation);

                    log.Debug(textWriter.ToString());   
                }

               

                if ((message.tLocation.LocalTime < message.tLocation.SystemTime.Value.AddMinutes(30)) || (1==1))
                {
                    // Treto: zapis vo baza 
                    try
                    {
                        DataRepository.LocationsProvider.Insert(message.tLocation);

                        if (message.tLocation.GpsData != null)
                        {
                            message.tLocation.GpsData.IdLocation = message.tLocation.IdLocation;

                            List<byte[]> retValBytes = VehiclesContainer.Instance.update(veh.IdVehicle, message.tLocation.GpsData, message.tLocation.SensData, message.tLocation.IdLocation);

                            foreach (byte[] retByte in retValBytes)
                            {
                                if (_gpsListener.SendMsgToVehicle(veh.IdVehicle, retByte) == -1)
                                    log.Error("GRESKA vo prakanje poraka na vozilo: " + veh.IdVehicle.ToString() + "URED: " + message.msg.DeviceNumber);
                            }                            
                        }
                    }
                    catch (Exception e)
                    {
                        log.Debug("SLEDNATA PORAKA E ZA VOZILO ID: " + veh.IdVehicle.ToString() + " ( " + veh.Plate + ")");
                        log.Error("Error location save", e);

                        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(message.tLocation.GetType());
                        StringWriter textWriter = new StringWriter();

                        x.Serialize(textWriter, message.tLocation);

                        log.Debug(textWriter.ToString());                                                    
                    }                   
                }               

                return 0;

            }
            catch (Exception ex)
            {
                log.Error("Error in LocationEnricher", ex);
                return -1;
            }

        }

        #endregion


        protected bool IsSearchRegionChanged(long pVehicleLastIdRegion, long pVehicleCurrentIdRegion)
        {
            bool retVal = false;
            bool found = false;

            if (pVehicleLastIdRegion == pVehicleCurrentIdRegion)
                return retVal;

            try
            {


                GlobSaldo.AVL.Entities.TList<GisSearchRegions> lstEligableGisSearchRegions = GlobSaldo.AVL.Data.DataRepository.GisSearchRegionsProvider.GetByIdRegion(pVehicleLastIdRegion);

                if (lstEligableGisSearchRegions != null && lstEligableGisSearchRegions.Count > 0)
                {
                    foreach (GisSearchRegions gsr in lstEligableGisSearchRegions)
                    {
                        if (gsr.IdAlternativeregion == pVehicleCurrentIdRegion)
                        {
                            if (gsr.TypeAlternative > 0)
                            {
                                retVal = true;
                                found = true;
                            }
                        }
                    }

                    //Tuka proveruvam da ne e nekoj reon sto ne e vo baza. I tuka treba da resetira i prati poraka
                    if (!found)
                        retVal = true;
                }
            }
            catch (Exception ex)
            {
                log.Error("IsSearchRegionChanged: ", ex);
            }

            return retVal;
        }

        protected DateTime CalculateLocalTime(DateTime pUtc, Vehicle pVehicle)
        {

            // Ova e ako ne moze da iskalkulira, pa da ima nesto. 
            // Pokasno ke stavam i vo log, ako ima potreba...

            DateTime retVal = DateTime.Parse("02/08/1903 00:00:00");

            double tmpHours = -9999;


            if (pVehicle.IdCompanySource != null)
                if (pVehicle.IdCompanySource.IdCompanyTimeZoneSource != null)
                    tmpHours = pVehicle.IdCompanySource.IdCompanyTimeZoneSource.Hours;

            if (tmpHours != -9999)
            {
                if (pUtc.IsDaylightSavingTime() == true)
                {
                    retVal = pUtc.AddHours(tmpHours + 1);
                }
                else
                {
                    retVal = pUtc.AddHours(tmpHours);
                }

            }
            else
            {
                //log.Error("Greska vo procesiranje na LocalTime (" + pUtc.ToString() + ") za vozilo: " + pVehicle.Plate);
            }

            return retVal;
        }
    }
}
