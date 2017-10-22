/***********
 * Da se dodade:
 * Vehicle retVal = null;
 * retVal = VehiclesContainer.Instance.getSingleVehicle(ID_Vehicle);
 * 
 * tLocation.IdVehicle = retVal.IdVehicle;
 * tLocation.IdUnit = (long)retVal.IdUnit;
 * tLocation.AnalogSensor = (int)retVal.IdDriverShift; //PAZI (OVA treba da odi vo kolona vozac)
 * tLocation.IdDriver = (int)retVal.IdDriverShift;
 * tLocation.LocalTime = CalculateLocalTime(gpsData.UTC, retVal);
 * tLocation.IdVehicleState = (long)retVal.currentState.IDCurrentState();
 * tLocation.Station = retVal.Station;
 * tLocation.IdRegion =
 * message.tLocation.RegionName =
 * 
 * ***************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Taxi.Communication.Server.Utils;
using Taxi.Communication.Server.Utils.Parsers;

//using JP.Data.Utils;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    internal class FF08 : CEPGeneralLocationHandler, IGeneralMessageHandler
    {

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "FF08")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {
            GPSData gpsData = new GPSData();
            SensorData sensData = new SensorData();


            //// Broj na stari podatoci 

            //// OVA E ZEMENO OD MESSAGE PARSER. TREBA DA E TUKA, NE TAMU !!! 
            //// *****************************************************
            //for (i = 9; i < 13; i++)
            //{
            //    ByteVoBrojNaStariPodatoci[i - 9] = (byte)dataBuffer[i];
            //}
            //

            try
            {
                sensData = parseSensorData(msg);
                gpsData = parseGPSData(msg);
            }

            catch (Exception)
            {
                gpsData = null;
                sensData = null;
            }


            try
            {

                //retVal e vozilo za koe e primena poraka

                if (gpsData != null && sensData != null)
                {

                    //Zapisuvam SAMO vo Locations

                    GlobSaldo.AVL.Entities.Locations tLocation = new Locations();
                    tLocation.Utc = gpsData.UTC;
                    tLocation.LatitudeY = (System.Decimal)gpsData.Latutude_Y;
                    //PAZI, RadiLosi podatoci

                    tLocation.Ns = "O"; //Zoran.    Ova e glupo, no vo sprotivno ke treba da se dostavi edno pole vo baza (i onaka ne ni treba ovaa informacija
                    //          Ovde stavam bukva O za slucaj da e stara poraka. Za regularna poraka ke stavam R (vo AA08)
                    //          Soodvetno SystemTime ke bide gpsData.UTC + 1/2 casa (ke se proveri dali e zimsko/letno i taka ke se sredi

                    //tLocation.Ns  = gpsData.Latitude_Direction ;
                    tLocation.LongitudeX = (System.Decimal)gpsData.Longitude_X;
                    //PAZI, RadiLosi podatoci
                    tLocation.Ew = "E";
                    //tLocation.Ew  = gpsData.Longitude_Direction ;
                    tLocation.Speed = (System.Decimal)gpsData.Speed;
                    tLocation.Altitude = gpsData.Altitude;
                    tLocation.Bearing = (System.Decimal)gpsData.Bearing;
                    tLocation.NumberOfSatellites = gpsData.NumerOfSatellites;
                    tLocation.KmTaxiMeter = (System.Decimal)gpsData.KmTaxi;
                    tLocation.KmGps = (System.Decimal)gpsData.KmGPS;
                    tLocation.Hdop = (System.Decimal)gpsData.HDOP;
                    tLocation.NumberOfOldMessages = gpsData.NumerOfSatellites;
                    tLocation.Senzor1 = sensData.Senzor_1;
                    tLocation.Senzor2 = sensData.Senzor_2;
                    tLocation.Senzor3 = sensData.Senzor_3;
                    tLocation.Senzor4 = sensData.Senzor_4;
                    tLocation.Senzor5 = sensData.Senzor_5;
                    tLocation.Senzor6 = sensData.Senzor_6;
                    tLocation.Senzor7 = sensData.Senzor_7;
                    tLocation.Senzor8 = sensData.Senzor_8;
                    tLocation.Senzor9 = sensData.Senzor_9;
                    tLocation.Senzor10 = sensData.Senzor_10;
                    tLocation.Senzor11 = sensData.Senzor_11;
                    tLocation.Senzor12 = sensData.Senzor_12;
                    //tLocation.AnalogSensor = (int)retVal.IdDriverShift; //PAZI (OVA treba da odi vo kolona vozac)
                    //tLocation.IdDriver = (int)retVal.IdDriverShift; 
                    tLocation.RfIdCard = sensData.RfIdCard;


                    tLocation.SystemTime = DateTime.Now;



                    tLocation.IdRegion = gpsData.IdRegion;
                    tLocation.RegionName = gpsData.RegionName;


                    //Pajo Added partial class for Location
                    tLocation.GpsData = gpsData;
                    tLocation.SensData = sensData;




                    // ZORAN:   Ovaa kontrola ja stavam bidejki se pojavuvaat lokacii so glupi koordinati, pa tie ni pravat zabuna kaj dispecerite!
                    //          Gi trgam site koi ne se 18 < X < 30
                    //          i                       38 < Y < 50
                    //          i zapisuvam vo log!

                    //if (((gpsData.Longitude_X > 18 && gpsData.Longitude_X < 30) && (gpsData.Latutude_Y > 38 && gpsData.Latutude_Y < 50)))
                    ParserResponseContainer retVal = new ParserResponseContainer(tLocation);
                    return retVal;
                    //else
                    //{
                    //    ServiceCallBack.log.Error("Greska vo coordinati ID_Vehicle=" + tLocation.IdVehicle.ToString() + " X=" + gpsData.Longitude_X.ToString() + " Y=" + gpsData.Latutude_Y.ToString());
                    //    return null;
                    //}
                }

                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("Greska vo obrabotka  FF08 za Vozilo", ex);
                return null;
            }



            
            //else
            //{
            //    return null;
            //}

        }

        #endregion


    }
}
