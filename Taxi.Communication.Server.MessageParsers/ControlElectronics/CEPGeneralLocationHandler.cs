using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
//using JP.Data.Utils;
using System.Globalization;
using Taxi.Communication.Server.Utils;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics
{
    public class CEPGeneralLocationHandler
    {
        protected int SensorOnStation = 0;

        protected int bytesInMessageHeader = 9;

        protected readonly ASCIIEncoding enc = new ASCIIEncoding();

        protected GPSData parseGPSData( DeviceMessage msg)
        {
            GPSData gpsData = new GPSData();
            byte[] tmpByte;


            tmpByte = new byte[2];
            tmpByte[0] = msg.data[4 + bytesInMessageHeader];
            Int16 Utc_day = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[5 + bytesInMessageHeader];
            Int16 Utc_month = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[6 + bytesInMessageHeader];
            Int16 Utc_year = System.BitConverter.ToInt16(tmpByte, 0);
            Utc_year += 2000;

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[7 + bytesInMessageHeader];
            Int16 Utc_hour = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[8 + bytesInMessageHeader];
            Int16 Utc_minute = System.BitConverter.ToInt16(tmpByte, 0);

            tmpByte = new byte[2];
            tmpByte[0] = msg.data[9 + bytesInMessageHeader];
            Int16 Utc_second = System.BitConverter.ToInt16(tmpByte, 0);

            IFormatProvider culture = new CultureInfo("en-US", true);

            try
            {
                gpsData.UTC = DateTime.Parse(Utc_year + "/" + Utc_month + "/" + Utc_day + " " + Utc_hour + ":" + Utc_minute + ":" + Utc_second, culture, DateTimeStyles.NoCurrentDateDefault);
            }
            catch (Exception ex)
            {

                //log4net.LogManager.GetLogger("MyService").Error("gpsData.UTC...PROBLEM, msg.data.toString(): " + msg.data.ToString(), ex);
                //log4net.LogManager.GetLogger("MyService").Error("Utc_year: " + Utc_year.ToString());
                //log4net.LogManager.GetLogger("MyService").Error("Utc_month: " + Utc_month.ToString());
                //log4net.LogManager.GetLogger("MyService").Error("Utc_day: " + Utc_day.ToString());
                //log4net.LogManager.GetLogger("MyService").Error("Utc_hour: " + Utc_hour.ToString());
                //log4net.LogManager.GetLogger("MyService").Error("Utc_minute: " + Utc_minute.ToString());

                gpsData.UTC = DateTime.Parse("02/08/1903 00:00:00");
            }

            

            byte[] lat_degrees = new byte[2];

            lat_degrees[0] = msg.data[10 + bytesInMessageHeader];
            lat_degrees[1] = msg.data[11 + bytesInMessageHeader];

            byte[] lat_minutes = new byte[4];

            lat_minutes[0] = msg.data[12 + bytesInMessageHeader];
            lat_minutes[1] = msg.data[13 + bytesInMessageHeader];
            lat_minutes[2] = msg.data[14 + bytesInMessageHeader];
            lat_minutes[3] = msg.data[15 + bytesInMessageHeader];

            try
            {
                gpsData.Latutude_Y = System.BitConverter.ToInt16(lat_degrees, 0);
                gpsData.Latutude_Y = gpsData.Latutude_Y + System.BitConverter.ToSingle(lat_minutes, 0) / 60;


            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("gpsData.Latutude_Y...PROBLEM", ex);
                gpsData.Latutude_Y = 9999;
            }


            

            char lat_direction = enc.GetChars(msg.data, 16 + bytesInMessageHeader, 1)[0];

            gpsData.Latitude_Direction = lat_direction.ToString();

            

            byte[] lon_degrees = new byte[2];

            lon_degrees[0] = msg.data[17 + bytesInMessageHeader];
            lon_degrees[1] = msg.data[18 + bytesInMessageHeader];

            byte[] lon_minutes = new byte[4];

            lon_minutes[0] = msg.data[19 + bytesInMessageHeader];
            lon_minutes[1] = msg.data[20 + bytesInMessageHeader];
            lon_minutes[2] = msg.data[21 + bytesInMessageHeader];
            lon_minutes[3] = msg.data[22 + bytesInMessageHeader];

            try
            {
                gpsData.Longitude_X = System.BitConverter.ToInt16(lon_degrees, 0);
                gpsData.Longitude_X = gpsData.Longitude_X + System.BitConverter.ToSingle(lon_minutes, 0) / 60;
            }
            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("gpsData.Longitude_X...PROBLEM", ex);
                gpsData.Longitude_X = 9999;
            }

            // Zapisi na konzola
            

            char lon_direction = enc.GetChars(msg.data, 23 + bytesInMessageHeader, 1)[0];

            gpsData.Longitude_Direction = lon_direction.ToString();


            // Zapisi na konzola
            

            byte[] brzina = new byte[4];

            brzina[0] = msg.data[24 + bytesInMessageHeader];
            brzina[1] = msg.data[25 + bytesInMessageHeader];
            brzina[2] = msg.data[26 + bytesInMessageHeader];
            brzina[3] = msg.data[27 + bytesInMessageHeader];

            try
            {
                gpsData.Speed = System.BitConverter.ToSingle(brzina, 0);
                
                if(gpsData.Speed > 150)
                    gpsData.Speed = (Single)(0);
            }
            catch (Exception)
            {
                gpsData.Speed = (Single)(0);
            }

            // Zapisi na konzola
            

            byte[] Broj_na_sateliti = new byte[2];

            Broj_na_sateliti[0] = msg.data[28 + bytesInMessageHeader];
            Broj_na_sateliti[1] = msg.data[29 + bytesInMessageHeader];

            try
            {
                gpsData.NumerOfSatellites = System.BitConverter.ToInt16(Broj_na_sateliti, 0);
            }
            catch (Exception)
            {
                gpsData.NumerOfSatellites = 0;
            }


            // Zapisi na konzola
            


            byte[] HDOP = new byte[4];

            HDOP[0] = msg.data[30 + bytesInMessageHeader];
            HDOP[1] = msg.data[31 + bytesInMessageHeader];
            HDOP[2] = msg.data[32 + bytesInMessageHeader];
            HDOP[3] = msg.data[33 + bytesInMessageHeader];

            try
            {
                gpsData.HDOP = System.BitConverter.ToSingle(HDOP, 0);
            }
            catch (Exception)
            {
                gpsData.HDOP = (Single)(0);
            }

            // Zapisi na konzola
            


            byte[] Visina = new byte[2];

            Visina[0] = msg.data[34 + bytesInMessageHeader];
            Visina[1] = msg.data[35 + bytesInMessageHeader];

            try
            {
                gpsData.Altitude = System.BitConverter.ToInt16(Visina, 0);
            }
            catch (Exception)
            {
                gpsData.Altitude = 0;
            }


            // Zapisi na konzola
            


            byte[] BEARRING = new byte[4];

            BEARRING[0] = msg.data[36 + bytesInMessageHeader];
            BEARRING[1] = msg.data[37 + bytesInMessageHeader];
            BEARRING[2] = msg.data[38 + bytesInMessageHeader];
            BEARRING[3] = msg.data[39 + bytesInMessageHeader];

            try
            {
                gpsData.Bearing = System.BitConverter.ToSingle(BEARRING, 0);

                if (gpsData.Bearing > 360)
                    gpsData.Bearing = 999;
            }
            catch (Exception)
            {
                gpsData.Bearing = (Single)(0);
            }


            // Zapisi na konzola
            


            byte[] kmGPS = new byte[4];

            kmGPS[0] = msg.data[44 + bytesInMessageHeader];
            kmGPS[1] = msg.data[45 + bytesInMessageHeader];
            kmGPS[2] = msg.data[46 + bytesInMessageHeader];
            kmGPS[3] = msg.data[47 + bytesInMessageHeader];

            try
            {
                gpsData.KmGPS = System.BitConverter.ToSingle(kmGPS, 0);
            }
            catch (Exception)
            {
                gpsData.KmGPS = (Single)(0);
            }


            // Zapisi na konzola
            



            byte[] kmTaxi = new byte[4];

            kmTaxi[0] = msg.data[48 + bytesInMessageHeader];
            kmTaxi[1] = msg.data[49 + bytesInMessageHeader];
            kmTaxi[2] = msg.data[50 + bytesInMessageHeader];
            kmTaxi[3] = msg.data[51 + bytesInMessageHeader];

            try
            {
                gpsData.KmTaxi = System.BitConverter.ToSingle(kmTaxi, 0);
            }

            catch (Exception)
            {
                gpsData.KmTaxi = (Single)(0);
            }

            // Zapisi na konzola
            

            

            return gpsData;

        }

        protected SensorData parseSensorData(DeviceMessage msg)
        {
            SensorData sensData = new SensorData();

            //int tempAddr = 0;
            int tempByte = 0x00;
            int tempControl = 0x01;

            int binarni_podatoci_1;
            binarni_podatoci_1 = msg.data[40 + bytesInMessageHeader];

            tempByte = binarni_podatoci_1;

            sensData.Senzor_1 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_2 = tempByte & tempControl;

            tempByte = tempByte >> 1;

            sensData.Senzor_3 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_4 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_5 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_6 = tempByte & tempControl;
            


            int binarni_podatoci_2;
            binarni_podatoci_2 = msg.data[41 + bytesInMessageHeader];

            tempByte = binarni_podatoci_2;

            sensData.Senzor_7 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_8 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_9 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_10 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_11 = tempByte & tempControl;
            

            tempByte = tempByte >> 1;

            sensData.Senzor_12 = tempByte & tempControl;
            

            SensorOnStation = sensData.Senzor_12;


            //////Console.WriteLine("1: " + sensData.Senzor_1.ToString());
            //////Console.WriteLine("2: " + sensData.Senzor_2.ToString());
            //////Console.WriteLine("3: " + sensData.Senzor_3.ToString());
            //////Console.WriteLine("4: " + sensData.Senzor_4.ToString());
            //////Console.WriteLine("5: " + sensData.Senzor_5.ToString());
            //////Console.WriteLine("6: " + sensData.Senzor_6.ToString());
            //////Console.WriteLine("7: " + sensData.Senzor_7.ToString());
            //////Console.WriteLine("8: " + sensData.Senzor_8.ToString());
            //////Console.WriteLine("9: " + sensData.Senzor_9.ToString());
            //////Console.WriteLine("10: " + sensData.Senzor_10.ToString());
            //////Console.WriteLine("11: " + sensData.Senzor_11.ToString());
            //////Console.WriteLine("12: " + sensData.Senzor_12.ToString());



            byte[] analogen_podatok_1 = new byte[2];

            analogen_podatok_1[0] = msg.data[42 + bytesInMessageHeader];
            analogen_podatok_1[1] = msg.data[43 + bytesInMessageHeader];

            try
            {
                sensData.AnalogSenzor_1 = System.BitConverter.ToInt16(analogen_podatok_1, 0);
            }
            catch (Exception)
            {
                sensData.AnalogSenzor_1 = 0;
            }

            byte[] ByteVoRfIdCard = new byte[10];


            ByteVoRfIdCard[0] = msg.data[52 + bytesInMessageHeader];
            ByteVoRfIdCard[1] = msg.data[53 + bytesInMessageHeader];
            ByteVoRfIdCard[2] = msg.data[54 + bytesInMessageHeader];
            ByteVoRfIdCard[3] = msg.data[55 + bytesInMessageHeader];
            ByteVoRfIdCard[4] = msg.data[56 + bytesInMessageHeader];
            ByteVoRfIdCard[5] = msg.data[57 + bytesInMessageHeader];
            ByteVoRfIdCard[6] = msg.data[58 + bytesInMessageHeader];
            ByteVoRfIdCard[7] = msg.data[59 + bytesInMessageHeader];
            ByteVoRfIdCard[8] = msg.data[60 + bytesInMessageHeader];
            ByteVoRfIdCard[9] = msg.data[61 + bytesInMessageHeader];

            sensData.RfIdCard = enc.GetString(ByteVoRfIdCard);

            
            


            return sensData;

        }

    }
}
