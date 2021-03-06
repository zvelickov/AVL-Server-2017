/***********
 * Da se dodade:
 * Vehicle retVal = null;
 * retVal = VehiclesContainer.Instance.getSingleVehicle(ID_Vehicle);
 * 
 * tFiscalReceipt.IdVehicle = retVal.IdVehicle;
 * tFiscalReceipt.IdUnit = (long)retVal.IdUnit;
 * tFiscalReceipt.Driver = retVal.IdDriverShift.ToString();
 * 
 * ***************/

using System;
using System.Collections.Generic;
using System.Text;
//using JP.Data.Utils;
using Taxi.Communication.Server.Utils.Parsers;

using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.MessageParsers.ControlElectronics.ExtraData;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class FF13 : IGeneralMessageHandler
    {
        private readonly ASCIIEncoding enc = new ASCIIEncoding();

        private int bytesInMessageHeader = 11; //tuka gi trgam i cc=broj na byte-i vo poraka

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "FF13")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {
            try
            {
                // polesno e, da ne mu ja mnislam, da si napravam kopija od bufferot
                byte[] m_dataBuffer = new byte[msg.DataLength - bytesInMessageHeader];

                for (int n = 0; n < m_dataBuffer.Length; n++)
                {
                    m_dataBuffer[n] = (byte)msg.data[n + bytesInMessageHeader];
                }

                string m_BrojNaSmetka = parseFirstString(ref m_dataBuffer, 0);
                string m_Vozac = parseFirstString(ref m_dataBuffer, 0);
                string m_brojNaVozenje = parseFirstString(ref m_dataBuffer, 0);
                string m_Tarifa = parseFirstString(ref m_dataBuffer, 0);
                string m_Vremetraenje = parseFirstString(ref m_dataBuffer, 0);
                string m_Promet = parseFirstString(ref m_dataBuffer, 0);
                string m_Datum = parseFirstString(ref m_dataBuffer, 0);


                // Sega treba vo stringot da ima samo 10 byte-i za RfIdCard
                // Ako nema tocno 10 byte-i, stavam ?????, za da ne se sluci problem!!!

                string m_RfIdCard;

                if (m_dataBuffer.Length != 10)
                    m_RfIdCard = "??????????";
                else
                    m_RfIdCard = enc.GetString(m_dataBuffer);

                // Zoran: Tuka treba da se zapise vo baza !!!
                // Privremeno: ispisuva na ekran

                Console.WriteLine("Broj na smetka   : {0}", m_BrojNaSmetka);
                Console.WriteLine("Vozac            : {0}", m_Vozac);
                Console.WriteLine("Broj na vozenje  : {0}", m_brojNaVozenje);
                Console.WriteLine("Tarifa           : {0}", m_Tarifa);
                Console.WriteLine("Vremetraenje     : {0}", m_Vremetraenje);
                Console.WriteLine("Promet           : {0}", m_Promet);
                Console.WriteLine("Datum            : {0}", m_Datum);
                Console.WriteLine("Rf ID Card       : {0}", m_RfIdCard);


                GlobSaldo.AVL.Entities.FiskalReceipt tFiscalReceipt = new FiskalReceipt();

                tFiscalReceipt.IdUnit = 1;
                tFiscalReceipt.ReceiptNumber = m_BrojNaSmetka;
                tFiscalReceipt.DriveNumber = m_Vozac;
                tFiscalReceipt.ReceiptNumber = m_brojNaVozenje;
                tFiscalReceipt.Tariff = m_Tarifa;
                tFiscalReceipt.Duration = m_Vremetraenje;
                tFiscalReceipt.Money = m_Promet;
                tFiscalReceipt.SystemDate = DateTime.Now;
                tFiscalReceipt.ReceiptDate = m_Datum;
                tFiscalReceipt.RfIdCard = m_RfIdCard;
                
                /*
                if (retVal.currentGPSData != null)
                {
                    if (retVal.currentGPSData.IdLocation != 0)
                    {
                        tFiscalReceipt.IdLocation = retVal.currentGPSData.IdLocation;
                    }
                    else
                    {
                        tFiscalReceipt.IdLocation = 20000000;
                    }
                }
                else
                {
                    tFiscalReceipt.IdLocation = 20000000;
                }
                */
                ParserResponseContainer retVal = new ParserResponseContainer(tFiscalReceipt);
                retVal.fiscalReceiptExtraData = null;
                return retVal;
            }

            catch (Exception ex)
            {
                //log.Error("Greska vo obrabotka na fiskalna smetka", ex);
                return null;
            }



        }

        #endregion

        public string parseRfIdCard(ref byte[] p_string)
        {
            byte[] tmpBuffer = new byte[10];

            for (int i = 0; i < 10; i++)
                tmpBuffer[i] = p_string[i];

            string retVal = enc.GetString(tmpBuffer);

            //Sega da gi trgnam prvite 10 byte-i

            for (int n = 0; n < p_string.Length - 11; n++)
            {
                p_string[n] = p_string[n + 10];
            }

            Array.Resize(ref p_string, p_string.Length - 10);

            return retVal;
        }

        public string parseFirstString(ref byte[] p_string, int startIndex)
        {
            string tmpString;
            string retVal = "";
            int m_index;

            tmpString = enc.GetString(p_string);

            m_index = tmpString.IndexOf(Environment.NewLine);

            if (m_index <= 0)                   //-1 ako ne go najde, 0 ako e empty string-ot
                return retVal;


            retVal = enc.GetString(p_string, startIndex, m_index - startIndex);

            // Brisam do posle prvoto CrLf
            int n = 0;

            for (int j = m_index + 2; j <= p_string.Length - 1; j++, n++)
                p_string[j - m_index - 2] = p_string[j];

            // OK, sega go namaluvame bufferot za da ostane samo ostatokot od byte-ite (sega se veke siftirani na pocetokot
            // Od prethodnoto se gleda kolku treba da ostane byte-ti vo bafer-ot: n byte-i

            Array.Resize(ref p_string, n);

            return retVal;
        }

        public FiscalReceiptAccentExtraData parseFiskalnaSmetkaString(ref byte[] p_string)
        {
            string tmpString;
            FiscalReceiptAccentExtraData retVal = new FiscalReceiptAccentExtraData();

            tmpString = enc.GetString(p_string);


            //Sega, gi popolnuvam edno po edno pole vo strukturata, nezavino dali mi treba ili ne

            retVal.N_DRIVER = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_KM_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_MIN_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_PAID_METERS = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_PAID_METERS_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_PAID_SECONDES = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T1_PAID_SECONDES_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_KM_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_MIN_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_PAID_METERS = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_PAID_METERS_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_PAID_SECONDES = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T2_PAID_SECONDES_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_KM_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_MIN_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_PAID_METERS = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_PAID_METERS_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_PAID_SECONDES = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.T3_PAID_SECONDES_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.START_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.FIRST_EXTRA_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.SECOND_EXTRA_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.THIRD_EXTRA_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.GLOBAL_DISCOUNT_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.RECEIPT_DISCOUNT_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.TOTAL_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.TAXA_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.TAXB_PRICE = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.DISTANCE_METERS = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            retVal.N_RECEIPT = tmpString.Substring(0, tmpString.IndexOf(","));
            tmpString = tmpString.Substring(tmpString.IndexOf(",") + 1);

            return retVal;
        }


    }
}
