using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.Utils.Classes;

namespace Taxi.Communication.Server.Utils.Parsers
{

    public class ConfirmationData
    {
       public string confirmationCommand;
       public bool status;

        public ConfirmationData()
        {
            this.status = false;
        }
    }

    public class ParserResponseContainer
    {

        public DeviceMessage msg = null;

        public IFiscalReceiptExtraData fiscalReceiptExtraData = null;

        public Locations tLocation;
        public FiskalReceipt tFiscal;
        public ReceivedShortMessage tShortMessage;
        public ReceivedRegionsTo tRegionsTo;
        public ReceivedFreeText tFreeText;
        public byte[] tSendToUnitMsg;
        public ConfirmationData tConfirmMsg;
        public clsBaranjeStatus tBaranjeStatus;
        public clsPotvrdaNaNaracka tPotvrdaNaNaracka;


        private ParserResponseContainer()
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public void clearUnitToMsg()
        {
            if (this.tSendToUnitMsg != null)
            {
                this.tSendToUnitMsg = null;
            }
        }

        public void addToNewMsgToUnit(byte[] msg)
        {
            if (this.tSendToUnitMsg == null)
            {
                this.tSendToUnitMsg = new byte[msg.Length];
                Array.Copy(msg, this.tSendToUnitMsg, msg.Length);
            }
            else
            {
                int initL = this.tSendToUnitMsg.Length;
                Array.Resize(ref this.tSendToUnitMsg, this.tSendToUnitMsg.Length + msg.Length);
                System.Buffer.BlockCopy(msg, 0, this.tSendToUnitMsg, initL, msg.Length);
            }
        }

        public static ParserResponseContainer merge(ParserResponseContainer first, ParserResponseContainer second)
        {
            if (first == null)
                return second;

            if (second == null)
                return first;

            ParserResponseContainer retVal = new ParserResponseContainer();

            retVal.tLocation = first.tLocation != null ? first.tLocation : second.tLocation;
            retVal.tFiscal = first.tFiscal != null ? first.tFiscal : second.tFiscal;
            retVal.tShortMessage = first.tShortMessage != null ? first.tShortMessage : second.tShortMessage;
            retVal.tRegionsTo = first.tRegionsTo != null ? first.tRegionsTo : second.tRegionsTo;
            retVal.tFreeText = first.tFreeText != null ? first.tFreeText : second.tFreeText;
            retVal.tSendToUnitMsg = first.tSendToUnitMsg != null ? first.tSendToUnitMsg : second.tSendToUnitMsg;
            retVal.tConfirmMsg = first.tConfirmMsg != null ? first.tConfirmMsg : second.tConfirmMsg;
            retVal.tBaranjeStatus = first.tBaranjeStatus != null ? first.tBaranjeStatus : second.tBaranjeStatus;
            retVal.tPotvrdaNaNaracka = first.tPotvrdaNaNaracka != null ? first.tPotvrdaNaNaracka : second.tPotvrdaNaNaracka;

            return retVal;
        }

        


        public ParserResponseContainer(Locations loc)
        {
            tLocation = loc;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(FiskalReceipt fiscal)
        {
            tLocation = null;
            tFiscal = fiscal;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(ReceivedShortMessage shortMsg)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = shortMsg;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(ReceivedRegionsTo regionTo)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = regionTo;
            tFreeText = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(ReceivedFreeText freeText)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = freeText;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(byte[] sendToUnitMsg)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = sendToUnitMsg;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(ConfirmationData confirmMsg)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = confirmMsg;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(clsBaranjeStatus pBaranjeStatus)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = pBaranjeStatus;
            tPotvrdaNaNaracka = null;
        }

        public ParserResponseContainer(clsPotvrdaNaNaracka pPotvrdaNaNaracka)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = null;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = pPotvrdaNaNaracka;
        }

        [Obsolete("Use New Version with ConfirmationData")]
        public ParserResponseContainer(int confirmMsg)
        {
            tLocation = null;
            tFiscal = null;
            tShortMessage = null;
            tRegionsTo = null;
            tFreeText = null;
            tSendToUnitMsg = null;
            tConfirmMsg = new ConfirmationData();
            tConfirmMsg.status = true;
            tBaranjeStatus = null;
            tPotvrdaNaNaracka = null;
        }

    }
}
