using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;

namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.ExtraData
{

    public class FiscalReceiptAccentExtraData : IFiscalReceiptExtraData
    {
        public string N_DRIVER;

        #region tarif T1

        private string m_T1_KM_PRICE;
        public string T1_KM_PRICE
        {
            set
            {
                m_T1_KM_PRICE = value;
            }
            get
            {
                return getPrice(m_T1_KM_PRICE);
            }
        }

        private string m_T1_MIN_PRICE;
        public string T1_MIN_PRICE
        {
            set
            {
                m_T1_MIN_PRICE = value;
            }
            get
            {
                return getPrice(m_T1_MIN_PRICE);
            }
        }

        public string T1_PAID_METERS;

        private string m_T1_PAID_METERS_PRICE;
        public string T1_PAID_METERS_PRICE
        {
            set
            {
                m_T1_PAID_METERS_PRICE = value;
            }
            get
            {
                return getPrice(m_T1_PAID_METERS_PRICE);
            }
        }

        public string T1_PAID_SECONDES;

        private string m_T1_PAID_SECONDES_PRICE;
        public string T1_PAID_SECONDES_PRICE
        {
            set
            {
                m_T1_PAID_SECONDES_PRICE = value;
            }
            get
            {
                return getPrice(m_T1_PAID_SECONDES_PRICE);
            }
        }

        #endregion

        #region tarif T2

        private string m_T2_KM_PRICE;
        public string T2_KM_PRICE
        {
            set
            {
                m_T2_KM_PRICE = value;
            }
            get
            {
                return getPrice(m_T2_KM_PRICE);
            }
        }

        private string m_T2_MIN_PRICE;
        public string T2_MIN_PRICE
        {
            set
            {
                m_T2_MIN_PRICE = value;
            }
            get
            {
                return getPrice(m_T2_MIN_PRICE);
            }
        }

        public string T2_PAID_METERS;

        private string m_T2_PAID_METERS_PRICE;
        public string T2_PAID_METERS_PRICE
        {
            set
            {
                m_T2_PAID_METERS_PRICE = value;
            }
            get
            {
                return getPrice(m_T2_PAID_METERS_PRICE);
            }
        }

        public string T2_PAID_SECONDES;

        private string m_T2_PAID_SECONDES_PRICE;
        public string T2_PAID_SECONDES_PRICE
        {
            set
            {
                m_T2_PAID_SECONDES_PRICE = value;
            }
            get
            {
                return getPrice(m_T2_PAID_SECONDES_PRICE);
            }
        }

        #endregion

        #region tarif T3

        private string m_T3_KM_PRICE;
        public string T3_KM_PRICE
        {
            set
            {
                m_T3_KM_PRICE = value;
            }
            get
            {
                return getPrice(m_T3_KM_PRICE);
            }
        }

        private string m_T3_MIN_PRICE;
        public string T3_MIN_PRICE
        {
            set
            {
                m_T3_MIN_PRICE = value;
            }
            get
            {
                return getPrice(m_T3_MIN_PRICE);
            }
        }

        public string T3_PAID_METERS;

        private string m_T3_PAID_METERS_PRICE;
        public string T3_PAID_METERS_PRICE
        {
            set
            {
                m_T3_PAID_METERS_PRICE = value;
            }
            get
            {
                return getPrice(m_T3_PAID_METERS_PRICE);
            }
        }

        public string T3_PAID_SECONDES;

        private string m_T3_PAID_SECONDES_PRICE;
        public string T3_PAID_SECONDES_PRICE
        {
            set
            {
                m_T3_PAID_SECONDES_PRICE = value;
            }
            get
            {
                return getPrice(m_T3_PAID_SECONDES_PRICE);
            }
        }

        #endregion

        #region START PRICE
        private string m_START_PRICE;
        public string START_PRICE
        {
            set
            {
                m_START_PRICE = value;
            }
            get
            {
                return getPrice(m_START_PRICE);
            }
        }
        #endregion

        #region EXTRA PRICES
        private string m_FIRST_EXTRA_PRICE;
        public string FIRST_EXTRA_PRICE
        {
            set
            {
                m_FIRST_EXTRA_PRICE = value;
            }
            get
            {
                return getPrice(m_FIRST_EXTRA_PRICE);
            }
        }

        private string m_SECOND_EXTRA_PRICE;
        public string SECOND_EXTRA_PRICE
        {
            set
            {
                m_SECOND_EXTRA_PRICE = value;
            }
            get
            {
                return getPrice(m_SECOND_EXTRA_PRICE);
            }
        }

        private string m_THIRD_EXTRA_PRICE;
        public string THIRD_EXTRA_PRICE
        {
            set
            {
                m_THIRD_EXTRA_PRICE = value;
            }
            get
            {
                return getPrice(m_THIRD_EXTRA_PRICE);
            }
        }
        #endregion

        #region DISCOUNT PRICES
        private string m_GLOBAL_DISCOUNT_PRICE;
        public string GLOBAL_DISCOUNT_PRICE
        {
            set
            {
                m_GLOBAL_DISCOUNT_PRICE = value;
            }
            get
            {
                return getPrice(m_GLOBAL_DISCOUNT_PRICE);
            }
        }

        private string m_RECEIPT_DISCOUNT_PRICE;
        public string RECEIPT_DISCOUNT_PRICE
        {
            set
            {
                m_RECEIPT_DISCOUNT_PRICE = value;
            }
            get
            {
                return getPrice(m_RECEIPT_DISCOUNT_PRICE);
            }
        }
        #endregion

        #region TOTAL PRICE
        private string m_TOTAL_PRICE;
        public string TOTAL_PRICE
        {
            set
            {
                m_TOTAL_PRICE = value;
            }
            get
            {
                return getPrice(m_TOTAL_PRICE);
            }
        }
        #endregion

        #region TAX PRICES
        private string m_TAXA_PRICE;
        public string TAXA_PRICE
        {
            set
            {
                m_TAXA_PRICE = value;
            }
            get
            {
                return getPrice(m_TAXA_PRICE);
            }
        }

        private string m_TAXB_PRICE;
        public string TAXB_PRICE
        {
            set
            {
                m_TAXB_PRICE = value;
            }
            get
            {
                return getPrice(m_TAXB_PRICE);
            }
        }
        #endregion

        public string DISTANCE_METERS;
        public string N_RECEIPT;

        private string getPrice(string value)
        {
            try
            {
                double retVal = (double)(((double.Parse(value)) / 100));
                return retVal.ToString();
            }
            catch (Exception )
            {
                return value;
            }
        }

    }
}

