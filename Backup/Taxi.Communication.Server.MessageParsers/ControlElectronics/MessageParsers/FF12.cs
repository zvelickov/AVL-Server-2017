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
    class FF12 : IGeneralMessageHandler
    {
        private readonly ASCIIEncoding enc = new ASCIIEncoding();

        private int bytesInMessageHeader = 11; //tuka gi trgam i cc=broj na byte-i vo poraka

        #region IGeneralMessageHandler Members

        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "FF12")
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

                //  Prvo da ja dobijam RF_Kartickata, ako postoi (inaku se 10 nuli)
                //  Funkcijata gi obrabotuva prvite 10 byte-i i gi trga od m_dataBuffer

                string m_RfIdCard = parseRfIdCard(ref m_dataBuffer);


                // Sega ja imam samo cistata poraka od fiskalen smetac (se i sesto, odvoeno so zapirki)

                FiscalReceiptAccentExtraData tmpFiskalnaSmetka = parseFiskalnaSmetkaString(ref m_dataBuffer);



                GlobSaldo.AVL.Entities.FiskalReceipt tFiscalReceipt = new FiskalReceipt();

                tFiscalReceipt.IdUnit = 1;
                tFiscalReceipt.ReceiptNumber = tmpFiskalnaSmetka.N_RECEIPT;
                tFiscalReceipt.DriveNumber = tmpFiskalnaSmetka.N_DRIVER;

                // ZORAN:   Treba da se vidi kako da se odredi tarifata i vremetraenjeto
                //          Sega ke stavam "?", da da strci

                tFiscalReceipt.Tariff = "?";
                tFiscalReceipt.Duration = "?";


                tFiscalReceipt.Money = tmpFiskalnaSmetka.TOTAL_PRICE;


                tFiscalReceipt.SystemDate = DateTime.Now;
                tFiscalReceipt.ReceiptDate = DateTime.Now.ToString();
                tFiscalReceipt.RfIdCard = m_RfIdCard;




                ParserResponseContainer retVal = new ParserResponseContainer(tFiscalReceipt);
                retVal.fiscalReceiptExtraData = tmpFiskalnaSmetka;
                return retVal;
            }

            catch (Exception ex)
            {
                log4net.LogManager.GetLogger("MyService").Error("Greska vo obrabotka  FF12 za Vozilo", ex);
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
