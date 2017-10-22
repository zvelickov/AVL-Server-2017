using System;
using System.Collections.Generic;
using System.Text;
//using JP.Data.Utils;

using Taxi.Communication.Server.StateMachine;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Containers;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.ConnectionListeners
{
    public class FillVehicleData
    {
        private static readonly ASCIIEncoding enc = new ASCIIEncoding();
        /*
        public static Boolean FillSendVehicle(long ID_Vehicle, DeviceMessage msg)
        {

            List<byte[]> retVal = null;
            

            GPSData gpsData = new GPSData();
            SensorData sensData = new SensorData();

            switch (msg.Command)
            {
                case "08":
                    gpsData = parseGPSData(msg);
                    sensData = parseSensorData(msg);
                    break;
                case "02":
                    break;
                case "12":
                    gpsData = parseGPSData(msg);
                    sensData = parseSensorData(msg);
                    break;
                default:
                    gpsData = parseGPSData(msg);
                    sensData = parseSensorData(msg);
                    break;
            }

            retVal = VehiclesContainer.Instance.update(ID_Vehicle, gpsData, sensData, 0);

            if (retVal != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        */
        static GPSData parseGPSData(DeviceMessage msg)
        {
            GPSData gpsData = new GPSData();
            byte[] tmpByte;

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[0];
            Int16 Utc_hour = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[1];
            Int16 Utc_minute = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[2];
            Int16 Utc_second = System.BitConverter.ToInt16(tmpByte, 0);

            try
            {
                gpsData.UTC = DateTime.Parse(Utc_hour + ":" + Utc_minute + ":" + Utc_second);
            }
            catch (Exception e)
            {
                gpsData.UTC = DateTime.Parse("02/08/1903 00:00:00");
                //ServiceCallBack.log.Error("ERROR ", e);
            }

            Console.WriteLine("Utc                   : {0}", gpsData.UTC);

            byte[] lat_degrees = new byte[2];

            lat_degrees[0] = msg.data[3];
            lat_degrees[1] = msg.data[4];

            byte[] lat_minutes = new byte[4];

            lat_minutes[0] = msg.data[5];
            lat_minutes[1] = msg.data[6];
            lat_minutes[2] = msg.data[7];
            lat_minutes[3] = msg.data[8];

            try
            {
                gpsData.Latutude_Y = System.BitConverter.ToInt16(lat_degrees, 0);
                gpsData.Latutude_Y = gpsData.Latutude_Y + System.BitConverter.ToSingle(lat_minutes, 0)/60;

                
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }


            Console.WriteLine("Latitude Y            : {0}", gpsData.Latutude_Y);

            char lat_direction = enc.GetChars(msg.data, 9, 1)[0];

            gpsData.Latitude_Direction = lat_direction.ToString();

            Console.WriteLine("Direction Y           : {0}", gpsData.Latitude_Direction);

            byte[] lon_degrees = new byte[2];

            lon_degrees[0] = msg.data[10];
            lon_degrees[1] = msg.data[11];

            byte[] lon_minutes = new byte[4];

            lon_minutes[0] = msg.data[12];
            lon_minutes[1] = msg.data[13];
            lon_minutes[2] = msg.data[14];
            lon_minutes[3] = msg.data[15];

            try
            {
                gpsData.Longitude_X = System.BitConverter.ToInt16(lon_degrees, 0);
                gpsData.Longitude_X = gpsData.Longitude_X + System.BitConverter.ToSingle(lon_minutes, 0)/60;
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }

            // Zapisi na konzola
            Console.WriteLine("Longitude X           : {0}", gpsData.Longitude_X);

            char lon_direction = enc.GetChars(msg.data, 16, 1)[0];

            gpsData.Longitude_Direction = lon_direction.ToString();


            // Zapisi na konzola
            Console.WriteLine("Direction X           : {0}", gpsData.Longitude_Direction);

            byte[] brzina = new byte[4];

            brzina[0] = msg.data[17];
            brzina[1] = msg.data[18];
            brzina[2] = msg.data[19];
            brzina[3] = msg.data[20];

            //try
            //{
                gpsData.Speed = System.BitConverter.ToSingle(brzina, 0);
            //}
            //catch (Exception e)
            //{
            //    ServiceCallBack.log.Error("ERROR ", e);
            //}

            // Zapisi na konzola
            Console.WriteLine("Brzina                : {0}", gpsData.Speed );

            byte[] Broj_na_sateliti = new byte[2];

            Broj_na_sateliti[0] = msg.data[21];
            Broj_na_sateliti[1] = msg.data[22];

            try
            {
                gpsData.NumerOfSatellites = System.BitConverter.ToInt16(Broj_na_sateliti, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }


            // Zapisi na konzola
            Console.WriteLine("Sateliti              : {0}", gpsData.NumerOfSatellites );


            byte[] HDOP = new byte[4];

            HDOP[0] = msg.data[23];
            HDOP[1] = msg.data[24];
            HDOP[2] = msg.data[25];
            HDOP[3] = msg.data[26];

            try
            {
                gpsData.HDOP = System.BitConverter.ToSingle(HDOP, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }

            // Zapisi na konzola
            Console.WriteLine("HDOP                  : {0}", gpsData.HDOP);


            byte[] Visina = new byte[2];

            Visina[0] = msg.data[27];
            Visina[1] = msg.data[28];

            try
            {
                gpsData.Altitude = System.BitConverter.ToInt16(Visina, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }


            // Zapisi na konzola
            Console.WriteLine("Visina                : {0}", gpsData.Altitude);


            byte[] BEARRING = new byte[4];

            BEARRING[0] = msg.data[29];
            BEARRING[1] = msg.data[30];
            BEARRING[2] = msg.data[31];
            BEARRING[3] = msg.data[32];

            try
            {
                gpsData.Bearing = System.BitConverter.ToSingle(BEARRING, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }


            // Zapisi na konzola
            Console.WriteLine("Bearing               : {0}", gpsData.Bearing);


            byte[] kmGPS = new byte[4];

            kmGPS[0] = msg.data[37];
            kmGPS[1] = msg.data[38];
            kmGPS[2] = msg.data[39];
            kmGPS[3] = msg.data[40];

            try
            {
                gpsData.KmGPS = System.BitConverter.ToSingle(kmGPS, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }


            // Zapisi na konzola
            Console.WriteLine("kmGPS                 : {0}", gpsData.KmGPS );



            byte[] kmTaxi = new byte[4];

            kmTaxi[0] = msg.data[41];
            kmTaxi[1] = msg.data[42];
            kmTaxi[2] = msg.data[43];
            kmTaxi[3] = msg.data[44];

            try
            {
                gpsData.KmTaxi = System.BitConverter.ToSingle(kmTaxi, 0);
            }

            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }

            // Zapisi na konzola
            Console.WriteLine("kmTaxi                : {0}", gpsData.KmTaxi );

            Console.WriteLine("\n");

            return gpsData;

        }

        static SensorData parseSensorData(DeviceMessage msg)
        {
            SensorData sensData = new SensorData();

            //int tempAddr = 0;
            int tempByte = 0x00;
            int tempControl = 0x01;

            int binarni_podatoci_1;
            binarni_podatoci_1 = msg.data[33];

            tempByte = binarni_podatoci_1;
                    
            sensData.Senzor_1 = tempByte & tempControl;
            Console.WriteLine("Senzor1               : {0}", sensData.Senzor_1);

            tempByte = tempByte >> 1;

            sensData.Senzor_2 = tempByte & tempControl;
            Console.WriteLine("Senzor2               : {0}", sensData.Senzor_2);

            tempByte = tempByte >> 1;

            sensData.Senzor_3 = tempByte & tempControl;
            Console.WriteLine("Senzor3               : {0}", sensData.Senzor_3);

            tempByte = tempByte >> 1;

            sensData.Senzor_4 = tempByte & tempControl;
            Console.WriteLine("Senzor4               : {0}", sensData.Senzor_4);

            tempByte = tempByte >> 1;

            sensData.Senzor_5 = tempByte & tempControl;
            Console.WriteLine("Senzor5               : {0}", sensData.Senzor_5);

            tempByte = tempByte >> 1;

            sensData.Senzor_6 = tempByte & tempControl;
            Console.WriteLine("Senzor6               : {0}", sensData.Senzor_6);


            int binarni_podatoci_2;
            binarni_podatoci_2 = msg.data[34];

            tempByte = binarni_podatoci_2;

            sensData.Senzor_7 = tempByte & tempControl;
            Console.WriteLine("Senzor7               : {0}", sensData.Senzor_7);

            tempByte = tempByte >> 1;

            sensData.Senzor_8 = tempByte & tempControl;
            Console.WriteLine("Senzor8               : {0}", sensData.Senzor_8);

            tempByte = tempByte >> 1;

            sensData.Senzor_9 = tempByte & tempControl;
            Console.WriteLine("Senzor9               : {0}", sensData.Senzor_9);

            tempByte = tempByte >> 1;

            sensData.Senzor_10 = tempByte & tempControl;
            Console.WriteLine("Senzor10              : {0}", sensData.Senzor_10);

            tempByte = tempByte >> 1;

            sensData.Senzor_11 = tempByte & tempControl;
            Console.WriteLine("Senzor11              : {0}", sensData.Senzor_11);

            tempByte = tempByte >> 1;

            sensData.Senzor_12 = tempByte & tempControl;
            Console.WriteLine("Senzor12              : {0}", sensData.Senzor_12);
            Console.WriteLine("\n");

            byte[] analogen_podatok_1 = new byte[2];

            analogen_podatok_1[0] = msg.data[35];
            analogen_podatok_1[1] = msg.data[36];

            try
            {
                sensData.AnalogSenzor_1 = System.BitConverter.ToInt16(analogen_podatok_1, 0);
            }
            catch (Exception e)
            {
                ServiceCallBack.log.Error("ERROR ", e);
            }



            return sensData;

        }

    }
}
