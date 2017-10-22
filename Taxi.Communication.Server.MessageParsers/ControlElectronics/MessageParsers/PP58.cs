using System;
using System.Collections.Generic;
using System.Text;
using Taxi.Communication.Server.Utils.Parsers;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.Utils.Classes;


namespace Taxi.Communication.Server.MessageParsers.ControlElectronics.MessageParsers
{
    class PP58 : IGeneralMessageHandler
    {
        protected readonly ASCIIEncoding enc = new ASCIIEncoding();

        protected int bytesInMessageHeader = 9;


        public bool canHandle(DeviceMessage msg)
        {
            if ((msg.MessageIndicator + msg.Command) == "PP58")
                return true;
            else
                return false;
        }

        public ParserResponseContainer ProcessMessage(DeviceMessage msg)
        {
            // Go trgam header-ot
            byte[] m_dataBuffer = new byte[msg.DataLength - bytesInMessageHeader];

            for (int n = 0; n < m_dataBuffer.Length; n++)
            {
                m_dataBuffer[n] = (byte)msg.data[n + bytesInMessageHeader];
            }

            clsPotvrdaNaNaracka tPotvrdaNaNaracka = new clsPotvrdaNaNaracka();

            tPotvrdaNaNaracka = parseData(m_dataBuffer);
            
            //ZORAN:    Tuka treba da napravam nova storna Vehicles.GetVehicleByUnitSerial, ama sega ke odi vaka..
            TList<Unit> lstUnits = DataRepository.UnitProvider.GetAll();

            foreach(Unit tmpU in lstUnits)
                if (tmpU.SerialNumber == msg.DeviceNumber)
                {
                    TList<Vehicle> tmpV = DataRepository.VehicleProvider.GetByIdUnit(tmpU.IdUnit);
                    tPotvrdaNaNaracka.IdVehicle = tmpV[0].IdVehicle;
                }
            

            ParserResponseContainer retVal = new ParserResponseContainer(tPotvrdaNaNaracka);

            return retVal;
        }



        private clsPotvrdaNaNaracka parseData(byte[] msg)
        {
            clsPotvrdaNaNaracka retVal = new clsPotvrdaNaNaracka();

            byte[] tmpByte = new byte[4];

            tmpByte[0] = msg[0];
            tmpByte[1] = msg[1];
            tmpByte[2] = msg[2];
            tmpByte[3] = msg[3];           

            long m_IdPhoneCall = System.BitConverter.ToInt32(tmpByte, 0);

            retVal.IdPhoneCall = m_IdPhoneCall;

            tmpByte = new byte[2];
            tmpByte[0] = msg[4];
            tmpByte[1] = msg[5];

            int m_Minuti = System.BitConverter.ToInt16(tmpByte, 0);

            retVal.Minuti = m_Minuti;

            return retVal;
        }
    }
}
