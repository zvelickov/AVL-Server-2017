using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;
////using JP.Data.Utils;

namespace Taxi.Communication.Server.Utils
{
    public static class GeneralFunctions
    {
        public static readonly ILog log = log4net.LogManager.GetLogger("MyService");

        public static bool checkShiftInOutSensorForStateIn(SensorData sensorData)
        {
            switch (GeneralConstants.ShiftInOutSensor)
            {
                case "1":
                    if (sensorData.Senzor_1 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "2":
                    if (sensorData.Senzor_2 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "3":
                    if (sensorData.Senzor_3 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "4":
                    if (sensorData.Senzor_4 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "5":
                    if (sensorData.Senzor_5 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "6":
                    if (sensorData.Senzor_6 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "7":
                    if (sensorData.Senzor_7 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "8":
                    if (sensorData.Senzor_8 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "9":
                    if (sensorData.Senzor_9 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "10":
                    if (sensorData.Senzor_10 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "11":
                    if (sensorData.Senzor_11 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                case "12":
                    if (sensorData.Senzor_12 == GeneralConstants.ShiftInOutSensorStateIn)
                        return true;
                    else
                        return false;
                    break;
                default:
                    return false;
                    break;
            }


        }

        public static GisAddressModel findGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber)
        {

            TList<GisAddressModel> tAdresenModel = new TList<GisAddressModel>();

            GisAddressModel m_PronajdenaAdresa = null;

            try
            {


                tAdresenModel = GlobSaldo.AVL.Data.DataRepository.GisAddressModelProvider.GetIDStreetHouseNumber(ID_Street, houseNumber);
            }
            catch (Exception ex) { log.Error("ERROR", ex); }




            if (tAdresenModel.Count > 0)
            {
                m_PronajdenaAdresa = (tAdresenModel[0]);
                //m_SelectedLocationQuality = 1;
            }
            else
            {
                //Ako nema ke treba da baram najbliska
                TList<GisAddressModel> tAdresenModelFullStreet = new TList<GisAddressModel>();
                try
                {
                    //TUKA PREBARUVAM SPORED IME NA ULICA
                    tAdresenModelFullStreet = GlobSaldo.AVL.Data.DataRepository.GisAddressModelProvider.GetByIdStreet(ID_Street);
                }
                catch (Exception ex) { log.Error("ERROR", ex); }


                //Baram koja razlika na kukni broevi po apsolutna vrednost e najmala
                int tRazlikaSega = 999;
                if (tAdresenModelFullStreet.Count > 0)
                {
                    foreach (GisAddressModel tAdresModel in tAdresenModelFullStreet)
                    {
                        if (Math.Abs(houseNumber - tAdresModel.HouseNumber) < tRazlikaSega)
                        {
                            tRazlikaSega = Math.Abs(houseNumber - tAdresModel.HouseNumber);
                            m_PronajdenaAdresa = tAdresModel;
                            //m_SelectedLocationQuality = 2;
                        }
                    }
                }
                else
                {
                    GisStreets objGisStreet = GlobSaldo.AVL.Data.DataRepository.GisStreetsProvider.GetByIdStreet (ID_Street);
                    if (objGisStreet != null)
                    {
                        //Ako na ovaa adresa nema kukni broevi, odam na sredina na ulica so kuken broj 0
                        m_PronajdenaAdresa = new GisAddressModel();
                        m_PronajdenaAdresa.IdStreet = ID_Street;
                        m_PronajdenaAdresa.IdRegion = 0;
                        m_PronajdenaAdresa.LatitudeY = (double)(objGisStreet.MaxLatitudeY ?? 0);
                        m_PronajdenaAdresa.LongitudeX = (double)(objGisStreet.MaxLongitudeX ?? 0);
                        m_PronajdenaAdresa.HouseNumber = houseNumber;
                        //m_SelectedLocationQuality = 3;
                    }
                }
            }

            return m_PronajdenaAdresa;
        }

    }
}