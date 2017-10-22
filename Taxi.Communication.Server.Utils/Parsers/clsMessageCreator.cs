using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Entities.PartialClasses;
using GlobSaldo.AVL.Data;
using Taxi.Communication.Server.Utils;

using System.Configuration;
using log4net;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public class clsMessageCreator
    {
        private ILog log = LogManager.GetLogger("MyService");
        
        public byte[] CreateAddressMessageForLCD(GlobSaldo.AVL.Entities.Vehicle myVehicle, PhoneCalls phoneCall)
        {
            byte[] tAddressMessageByte = new byte[112];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '33'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("33");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];



            // *************************************************************************************
            // Sledat podatoci, gi cita od primena adresa
            // *************************************************************************************


            // *************************************************************************************
            //Reden broj na zvuk za presmetuvanje na adresa za zvono
            // Fiksna lokacija e: 10000 + 500 + 14
            byte[] ByteVoZvono = new byte[2];
            ByteVoZvono = System.BitConverter.GetBytes((int)10514);
            tAddressMessageByte[9] = ByteVoZvono[0];
            tAddressMessageByte[10] = ByteVoZvono[1];


            // Reden broj na ulica (Doaga od Dispetcher)
            // Adresa 1 zapocnuva na prvite 512 posle header-ot (10x512)
            // Headerot ne se broi, pa toa e "lokacija 1". 
            // *************************************************************************************

            byte[] ByteVoAdresaNaUlica = new byte[2];
            int tIdSound = 0;

            if (phoneCall.oAddressFrom.oGisObjects != null) tIdSound = (int)phoneCall.oAddressFrom.oGisObjects.IdSound;
            if (phoneCall.oAddressFrom.oGisStreets != null) tIdSound = (int)phoneCall.oAddressFrom.oGisStreets.IdSound;

            ByteVoAdresaNaUlica = System.BitConverter.GetBytes(tIdSound);
            tAddressMessageByte[11] = ByteVoAdresaNaUlica[0];
            tAddressMessageByte[12] = ByteVoAdresaNaUlica[1];


            // Kuken Broj (Doaga od Dispetcher)
            // Broj 1 zapocnuva na prvite 512 posle header-ot (10x512) + brojot na ulici (10000)
            // Headerot ne se broi, pa "broj 1" = 10000+1, "Broj 2" = 10000+2, . 
            // ***************************************************************************************
            byte[] ByteVoAdresaNaBroj = new byte[2];
         
            tAddressMessageByte[13] = 1; // ByteVoAdresaNaBroj[0];
            tAddressMessageByte[14] = 0; // ByteVoAdresaNaBroj[1];


            // Fiksno, Oznacuva deka se praka nova adresa = 1
            // *************************************************************************************
            byte[] ByteVoNaracka = new byte[2];
            ByteVoNaracka = System.BitConverter.GetBytes((int)1);
            tAddressMessageByte[15] = ByteVoNaracka[0];
            tAddressMessageByte[16] = ByteVoNaracka[1];


            // Broj na reon = addressFrom.IdRegionSound
            // *************************************************************************************
            byte[] ByteVoReon = new byte[2];

            int tIdRegionSound = 0;

            if (phoneCall.oAddressFrom.oGisRegions != null) tIdRegionSound = (int)phoneCall.oAddressFrom.oGisRegions.IdSound;

            ByteVoReon = System.BitConverter.GetBytes(tIdRegionSound);
            tAddressMessageByte[17] = ByteVoReon[0];
            tAddressMessageByte[18] = ByteVoReon[1];

            // From latitude Y
            // *************************************************************************************
            byte[] ByteVoDegLatY = new byte[2];
            byte[] ByteVoMinLatY = new byte[4];
            int latYdeg = (int)Math.Truncate(phoneCall.oAddressFrom.LocationY);

            ByteVoDegLatY = System.BitConverter.GetBytes(latYdeg);
            tAddressMessageByte[19] = ByteVoDegLatY[0];
            tAddressMessageByte[20] = ByteVoDegLatY[1];

            //Console.Write("Y: " + phoneCall.oAddressFrom.LocationY.ToString());

            ByteVoMinLatY = System.BitConverter.GetBytes((float)((phoneCall.oAddressFrom.LocationY - latYdeg) * 60));
            tAddressMessageByte[21] = ByteVoMinLatY[0];
            tAddressMessageByte[22] = ByteVoMinLatY[1];
            tAddressMessageByte[23] = ByteVoMinLatY[2];
            tAddressMessageByte[24] = ByteVoMinLatY[3];

            // From longitude X
            // *************************************************************************************
            byte[] ByteVoDegLonX = new byte[2];
            byte[] ByteVoMinLonX = new byte[4];
            int lonXdeg = (int)Math.Truncate(phoneCall.oAddressFrom.LocationX);

            //Console.Write("X: " + phoneCall.oAddressFrom.LocationX.ToString());

            ByteVoDegLonX = System.BitConverter.GetBytes(lonXdeg);
            tAddressMessageByte[25] = ByteVoDegLonX[0];
            tAddressMessageByte[26] = ByteVoDegLonX[1];

            ByteVoMinLonX = System.BitConverter.GetBytes((float)((phoneCall.oAddressFrom.LocationX - lonXdeg) * 60));
            tAddressMessageByte[27] = ByteVoMinLonX[0];
            tAddressMessageByte[28] = ByteVoMinLonX[1];
            tAddressMessageByte[29] = ByteVoMinLonX[2];
            tAddressMessageByte[30] = ByteVoMinLonX[3];

            byte[] ByteVoLocationQuality = new byte[1];

            ByteVoLocationQuality =  System.BitConverter.GetBytes((char) phoneCall.oAddressFrom.LocationQuality);
            tAddressMessageByte[31] = ByteVoLocationQuality[0];

            //Adresa text form
            // Tuka pakuvam: Ime na reon, ime na ulica ili objekt, kuken broj i komentar
            // *************************************************************************************

            
            
            string tmpStr = "";

            string tmpInitials = GetInitialsForDispecher(phoneCall);

            tmpStr += tmpInitials + "-";

            if (phoneCall.oAddressFrom.oGisRegions != null)
                tmpStr += "REON: " + phoneCall.oAddressFrom.oGisRegions.IdRegion.ToString().Trim() + " // ";

            if (phoneCall.oAddressFrom.oGisStreets != null)
            {
                tmpStr += phoneCall.oAddressFrom.oGisStreets.StreetName.Trim() + " ";

                if (phoneCall.oAddressFrom.oGisStreets.StreetName.Substring(0,2) != "A:")
                    tmpStr += phoneCall.oAddressFrom.HouseNumber + " ";
            }

            if (phoneCall.oAddressFrom.oGisObjects != null)
                tmpStr += phoneCall.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

            if (phoneCall.oAddressFrom.Comment != null)
                tmpStr += phoneCall.oAddressFrom.Comment.Trim() + " ";

            // ZORAN:   ova e koregirano na 12.03.2015
            //          VAKA izgleda deka im kratam 10 char!!!
            if (tmpStr.Length > 70)
                tmpStr = tmpStr.Substring(0, 69);

            string fromAddres = UnicodeStrings.UncodeToAscii(tmpStr); //Vo praksa, treba da se 80 char, no zaradi ch, ...

            // ZORAN:   ova e dodadeno na 12.03.2015
            //          VAKA kese pratat site 80 chars!!!
            if (fromAddres.Length > 80)
                fromAddres = fromAddres.Substring(0, 79);

            byte[] ByteVoFromAddres = new byte[80];

            ByteVoFromAddres = ASCIIenc.GetBytes(fromAddres);
            
            ByteVoFromAddres.CopyTo(tAddressMessageByte, 32);
        
            return addChkSum(tAddressMessageByte);
        }




        //ZORAN:    Najava kratka
        //          Ima nekoja i pogore, no tie se za test.
        // *********************************************************************************************
        public byte[] OrderCreate (GlobSaldo.AVL.Entities.Vehicle myVehicle, PhoneCalls pPhoneCall)
        {          
            byte[] tAddressMessageByte = new byte[74];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '34'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("34");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];

            //ZORAN:    Tuka da proveram dali ima tekovna kazna, za konkretniot Driver
            //          Ako e pod kazna
            //              - Vo ID_PhoneCall mu stavam zgolemeno za 1000, za da ne moze da licitira
            //              - Vo tekstot na porakata mu stava poraka deka e pod kazna do togas i togas
            TList<PenaltyPerDriverPerPeriod> mPenaltyPerDriverPerPeriod = DataRepository.PenaltyPerDriverPerPeriodProvider.GetActivePenaltyByIdDriver(myVehicle.DriverShiftInOut.IdDriver);

            byte[] ByteVoIdPhoneCall = new byte[4];

            if (mPenaltyPerDriverPerPeriod != null && mPenaltyPerDriverPerPeriod.Count > 0)
            {
                ByteVoIdPhoneCall = System.BitConverter.GetBytes(pPhoneCall.IdPhoneCall+1000);
            }
            else
            {
                ByteVoIdPhoneCall = System.BitConverter.GetBytes(pPhoneCall.IdPhoneCall);
            }

            tAddressMessageByte[9] = ByteVoIdPhoneCall[0];
            tAddressMessageByte[10] = ByteVoIdPhoneCall[1];
            tAddressMessageByte[11] = ByteVoIdPhoneCall[2];
            tAddressMessageByte[12] = ByteVoIdPhoneCall[3];

            

            // Izvor na poraka 0=Dispecer, 3=Android, 4=sistemska
            // *************************************************************************************
            byte[] ByteVoIzvorNaPoraka = new byte[1];

            char mIzvor = '4';

            if (pPhoneCall.MessageType == "MC")
                mIzvor = '3';
            else
                mIzvor = '0';

            ByteVoIzvorNaPoraka = System.BitConverter.GetBytes(mIzvor);
            tAddressMessageByte[13] = ByteVoIzvorNaPoraka[0];

            // Adresa: Kratka Najava (odi vo ASCII)
            // Tuka pakuvam: 
            //      Ako e Dispecher:
            //          - Naziv na ulica (bez broj)
            //          - Od reon
            //          - Do kade, ako postoi
            //      Ako e Android, ....treba da vidam...
            // *************************************************************************************
            
            byte[] ByteVoFromAddres = new byte[60];
            string tmpStr = "";

            //Tuka stavam, za pocetok na narackata:
            //      A,  ako e android
            //      D1, ako e dispecer, a povikot e od NT (pPhoneCall.GroupCode = 291)
            //      D2, ako e dispecer, a povikot e od Lotus (pPhoneCall.GroupCode = 292)
            //      -,  ako eod dispecer, a nema poznat pPhoneCall.GroupCode
            // AKO vozacot e pod kazna, mu se praka info do koga e kaznet!!!
           

            if (mPenaltyPerDriverPerPeriod != null && mPenaltyPerDriverPerPeriod.Count > 0)
            {
                tmpStr = "Vie ste pod kazna do: " + mPenaltyPerDriverPerPeriod[0].PenaltyDateTimeTo.ToString("HH:mm dd-MM-yyyy");
            }
            else
            {
                if (pPhoneCall.MessageType == "MC")
                    tmpStr = "A, ";
                else
                {
                    switch (pPhoneCall.GroupCode)
                    {
                        case "291":
                            tmpStr = "D1, ";
                            break;
                        case "292":
                            tmpStr = "D2, ";
                            break;
                        default:
                            tmpStr = "";
                            break;
                    }
                }


                if (pPhoneCall.MessageType == "MC")          //Tuka e malku glupo, no mora sega vaka, se e staveno vo StreetName
                {
                    if (pPhoneCall.oAddressFrom.oGisStreets != null)
                    {
                        //log.Debug("OrderCreate: " + pPhoneCall.oAddressFrom.oGisStreets.StreetName);
                        tmpStr += pPhoneCall.oAddressFrom.oGisStreets.StreetName + " ";
                    }                  
                }
                else
                {

                    if (pPhoneCall.oAddressFrom.oGisStreets != null)
                    {
                        tmpStr += "Br: " + pPhoneCall.oAddressFrom.HouseNumber.ToString() + ", ";
                        tmpStr += pPhoneCall.oAddressFrom.oGisStreets.StreetName.Trim() + " ";
                    }

                    if (pPhoneCall.oAddressFrom.oGisObjects != null)
                        tmpStr += pPhoneCall.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

                    tmpStr += System.Environment.NewLine;

                    if (pPhoneCall.oAddressFrom.oGisRegions != null)
                        tmpStr += "REON: " + pPhoneCall.oAddressFrom.oGisRegions.IdRegion.ToString().Trim() ;


                    if (pPhoneCall.GroupCode == "292")
                    {
                        if (pPhoneCall.oAddressFrom.PickUpAddress != null && pPhoneCall.oAddressFrom.PickUpAddress.Trim() != "")
                            tmpStr += ", " + pPhoneCall.oAddressFrom.PickUpAddress.Trim();

                        if (pPhoneCall.oAddressFrom.To != null && pPhoneCall.oAddressFrom.To.Trim() != "")
                            tmpStr += ", Do:" + pPhoneCall.oAddressFrom.To.Trim();

                        if (pPhoneCall.oAddressFrom.Comment != null && pPhoneCall.oAddressFrom.Comment.Trim() != "")
                            tmpStr += ", Kom:" + pPhoneCall.oAddressFrom.Comment.Trim();
                    }
                    else
                    {
                        //Ova e staveno po baranje na Donka, na 12.11.2016
                        //Prakticno, i na NT im se praka cela adresa ako se >= vtori reon

                        if (myVehicle.SelectionIndex >= 20000)
                        {
                            if (pPhoneCall.oAddressFrom.PickUpAddress != null && pPhoneCall.oAddressFrom.PickUpAddress.Trim() != "")
                                tmpStr += ", " + pPhoneCall.oAddressFrom.PickUpAddress.Trim();

                            if (pPhoneCall.oAddressFrom.To != null && pPhoneCall.oAddressFrom.To.Trim() != "")
                                tmpStr += ", Do:" + pPhoneCall.oAddressFrom.To.Trim();

                            if (pPhoneCall.oAddressFrom.Comment != null && pPhoneCall.oAddressFrom.Comment.Trim() != "")
                                tmpStr += ", Kom:" + pPhoneCall.oAddressFrom.Comment.Trim();
                        }
                    }
                }
            }

            string fromAddres = UnicodeStrings.UncodeToAscii(tmpStr);

            if (fromAddres.Length > 60)                             //Vo praksa, treba da se 60 char
                fromAddres = fromAddres.Substring(0, 59);

            ByteVoFromAddres = ASCIIenc.GetBytes(fromAddres);

            ByteVoFromAddres.CopyTo(tAddressMessageByte, 14);                      

            return addChkSum(tAddressMessageByte);
        }



        //ZORAN:    Najava kratka - OTKAZUVANJE
        //          Ima nekoja i pogore, no tie se za test.
        // *********************************************************************************************
        public byte[] OrderCreateCancel(GlobSaldo.AVL.Entities.Vehicle myVehicle, long pIdPhoneCall, string pInfo)
        {
            log.Debug(myVehicle.IdVehicle.ToString() + "/" + pIdPhoneCall.ToString() + "/" + pInfo);

            byte[] tAddressMessageByte = new byte[73];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '35'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("35");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];


            byte[] ByteVoIdPhoneCall = new byte[4];

            ByteVoIdPhoneCall = System.BitConverter.GetBytes(pIdPhoneCall);

            tAddressMessageByte[9] = ByteVoIdPhoneCall[0];
            tAddressMessageByte[10] = ByteVoIdPhoneCall[1];
            tAddressMessageByte[11] = ByteVoIdPhoneCall[2];
            tAddressMessageByte[12] = ByteVoIdPhoneCall[3];

            // Poraka (odi vo ASCII)
            // ********************

            string fromAddres = UnicodeStrings.UncodeToAscii(pInfo);

            if (fromAddres.Length > 60)                 //Vo praksa, treba da se 60 char
                fromAddres = fromAddres.Substring(0, 59);

            byte[] ByteVoFromAddres = new byte[60];

            ByteVoFromAddres = ASCIIenc.GetBytes(fromAddres);

            ByteVoFromAddres.CopyTo(tAddressMessageByte, 13);

            return addChkSum(tAddressMessageByte);                
        }




        //ZORAN:    Najava dolga - POTVRDA
        //          Ima nekoja i pogore, no tie se za test.
        // *********************************************************************************************

        public byte[] OrderCreateConfirm(GlobSaldo.AVL.Entities.Vehicle myVehicle, PhoneCalls pPhoneCall)
        {
            byte[] tAddressMessageByte = new byte[207];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '36'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("36");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];


            byte[] ByteVoIdPhoneCall = new byte[4];

            ByteVoIdPhoneCall = System.BitConverter.GetBytes(pPhoneCall.IdPhoneCall);

            tAddressMessageByte[9] = ByteVoIdPhoneCall[0];
            tAddressMessageByte[10] = ByteVoIdPhoneCall[1];
            tAddressMessageByte[11] = ByteVoIdPhoneCall[2];
            tAddressMessageByte[12] = ByteVoIdPhoneCall[3];


            // From latitude Y
            // *************************************************************************************
            byte[] ByteVoDegLatY = new byte[2];
            byte[] ByteVoMinLatY = new byte[4];
            int latYdeg = (int)Math.Truncate(pPhoneCall.oAddressFrom.LocationY);
            ByteVoDegLatY = System.BitConverter.GetBytes(latYdeg);
            tAddressMessageByte[13] = ByteVoDegLatY[0];
            tAddressMessageByte[14] = ByteVoDegLatY[1];


            ByteVoMinLatY = System.BitConverter.GetBytes((float)((pPhoneCall.oAddressFrom.LocationY - latYdeg) * 60));
            tAddressMessageByte[15] = ByteVoMinLatY[0];
            tAddressMessageByte[16] = ByteVoMinLatY[1];
            tAddressMessageByte[17] = ByteVoMinLatY[2];
            tAddressMessageByte[18] = ByteVoMinLatY[3];

            // From longitude X
            // *************************************************************************************

            byte[] ByteVoDegLonX = new byte[2];
            byte[] ByteVoMinLonX = new byte[4];
            int lonXdeg = (int)Math.Truncate(pPhoneCall.oAddressFrom.LocationX);
            ByteVoDegLonX = System.BitConverter.GetBytes(lonXdeg);
            tAddressMessageByte[19] = ByteVoDegLonX[0];
            tAddressMessageByte[20] = ByteVoDegLonX[1];


            ByteVoMinLonX = System.BitConverter.GetBytes((float)((pPhoneCall.oAddressFrom.LocationX - lonXdeg) * 60));
            tAddressMessageByte[21] = ByteVoMinLonX[0];
            tAddressMessageByte[22] = ByteVoMinLonX[1];
            tAddressMessageByte[23] = ByteVoMinLonX[2];
            tAddressMessageByte[24] = ByteVoMinLonX[3];

            byte[] ByteVoLocationQuality = new byte[1];

            ByteVoLocationQuality = System.BitConverter.GetBytes('1');
            tAddressMessageByte[25] = ByteVoLocationQuality[0];

            // Izvor na poraka 0=Dispecer, 3=Android, 4=sistemska
            // *************************************************************************************
            byte[] ByteVoIzvorNaPoraka = new byte[1];

            char mIzvor = '4';

            if (pPhoneCall.MessageType == "MC")
                mIzvor = '3';
            else
                mIzvor = '0';

            ByteVoIzvorNaPoraka = System.BitConverter.GetBytes(mIzvor);
            tAddressMessageByte[26] = ByteVoIzvorNaPoraka[0];


            // Poraka (odi vo ASCII)
            // *************************************************************************************
            byte[] ByteVoFromAddres = new byte[180];
           
            string tmpStr = "";

            if (pPhoneCall.MessageType == "MC")
            {
                if (pPhoneCall.oAddressFrom.oGisStreets != null)
                {                    
                    tmpStr += pPhoneCall.oAddressFrom.oGisStreets.StreetName + " ";
                } 
            }
            else
            {                
                if (pPhoneCall.oAddressFrom.oGisStreets != null)
                {
                    tmpStr += "Br: " + pPhoneCall.oAddressFrom.HouseNumber.ToString() + ", ";
                    tmpStr += pPhoneCall.oAddressFrom.oGisStreets.StreetName.Trim() + " ";                   
                }

                if (pPhoneCall.oAddressFrom.oGisObjects != null)
                    tmpStr += pPhoneCall.oAddressFrom.oGisObjects.ObjectName.Trim() + " ";

                tmpStr += System.Environment.NewLine;

                if (pPhoneCall.oAddressFrom.oGisRegions != null)
                    tmpStr += "Reon: " + pPhoneCall.oAddressFrom.oGisRegions.IdRegion.ToString().Trim() + " ";

                tmpStr += System.Environment.NewLine;

                if (pPhoneCall.oAddressFrom.PickUpAddress != null && pPhoneCall.oAddressFrom.PickUpAddress.Trim() != "")
                    tmpStr += ", " + pPhoneCall.oAddressFrom.PickUpAddress.Trim();

                if (pPhoneCall.oAddressFrom.To != null && pPhoneCall.oAddressFrom.To.Trim() != "")
                    tmpStr += ", Do:" + pPhoneCall.oAddressFrom.To.Trim();

                if (pPhoneCall.oAddressFrom.Comment != null && pPhoneCall.oAddressFrom.Comment.Trim() != "")
                    tmpStr += ", Komentar:" + pPhoneCall.oAddressFrom.Comment.Trim();  
            }

            string fromAddres = UnicodeStrings.UncodeToAscii(tmpStr);

            if (fromAddres.Length > 180)                             //Vo praksa, treba da se 180 char
                fromAddres = fromAddres.Substring(0, 179);

            ByteVoFromAddres = ASCIIenc.GetBytes(fromAddres);

            ByteVoFromAddres.CopyTo(tAddressMessageByte, 27);

            return addChkSum(tAddressMessageByte);
        }







        public byte[] VehicleState(GlobSaldo.AVL.Entities.Vehicle myVehicle, int pVremeVoSostojba, bool pProdolzuvanjeNaVreme)
        {
            byte[] tAddressMessageByte = new byte[33];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '40'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("40");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];


            // Sledat podatoci
            // Prvo koja sostojba



            byte[] ByteVoFromAddres = new byte[20];

            string tmpString;

            if(myVehicle.currentStateString.Length > 20)
                 tmpString = myVehicle.currentStateString.Substring(0,20);
            else
                tmpString = myVehicle.currentStateString;

            ByteVoFromAddres = ASCIIenc.GetBytes(tmpString);

            ByteVoFromAddres.CopyTo(tAddressMessageByte, 9);

  
            //Potoa kolku min moze da bi vo taa sostojba. Za neograniceno se stava 0


            //int tmpInt = 20;

            byte[] ByteVoVremeVoSostojba = new byte[2];

            ByteVoVremeVoSostojba = System.BitConverter.GetBytes(pVremeVoSostojba);
            tAddressMessageByte[29] = ByteVoVremeVoSostojba[0];
            tAddressMessageByte[30] = ByteVoVremeVoSostojba[1];


            byte[] ByteVoProdolzuvanjeNaVreme = new byte[1];

            if(pProdolzuvanjeNaVreme)
                ByteVoProdolzuvanjeNaVreme = System.BitConverter.GetBytes('1');
            else
                ByteVoProdolzuvanjeNaVreme = System.BitConverter.GetBytes('0');

            tAddressMessageByte[31] = ByteVoProdolzuvanjeNaVreme[0];


            byte[] ByteVoSostojba = new byte[2];

            ByteVoSostojba = System.BitConverter.GetBytes(myVehicle.currentState.IDCurrentState());

            tAddressMessageByte[32] = ByteVoSostojba[0];

           

            return addChkSum(tAddressMessageByte);
        }


        private string GetInitialsForDispecher(PhoneCalls pPhoneCall)
        {
            string retVal = "";

            try
            {
                UserInOut tmpUserInOut = GlobSaldo.AVL.Data.DataRepository.UserInOutProvider.GetByIdUserLogInOut ((long)pPhoneCall.IdUserInOut);

                if (tmpUserInOut != null)
                {
                    Users tmpUser = GlobSaldo.AVL.Data.DataRepository.UsersProvider.GetByIdUser((long)tmpUserInOut.IdUser);

                    if (tmpUser != null)
                    {
                        retVal = tmpUser.Name.Substring(0, 1).ToUpper() + tmpUser.Lastname.Substring(0, 1).ToUpper();
                    }
                }
            }
            catch (Exception ex)
            {
                
            }

            return retVal;

        }
       



        public byte[] CreateNewPopUpMessageForLCD(GlobSaldo.AVL.Entities.Vehicle myVehicle, string strPopUp, char pIzvorNaPoraka)
        {
            string fromAddres = UnicodeStrings.UncodeToAscii(strPopUp);

            byte[] tAddressMessageByte = new byte[14 + fromAddres.Length];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '45'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("45");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1];

            //Adresa text form
            // *************************************************************************************

           


            byte[] ByteVoMessage = new byte[3];
            int intDolzinaNaPoraka = fromAddres.Length + 2;
            string strBrojNaBajti = intDolzinaNaPoraka.ToString("000");

            ByteVoMessage = ASCIIenc.GetBytes(strBrojNaBajti);

            tAddressMessageByte[9] = ByteVoMessage[0];
            tAddressMessageByte[10] = ByteVoMessage[1];
            tAddressMessageByte[11] = ByteVoMessage[2];            


            // Fiksno, Oznacuva deka se praka nova adresa = 1
            // *************************************************************************************
            byte[] ByteVoNaracka = new byte[1];
            ByteVoNaracka = System.BitConverter.GetBytes((char)1);
            tAddressMessageByte[12] = ByteVoNaracka[0];

            
            // Poraka (odi vo ASCII)
            // *************************************************************************************
            byte[] ByteVoFromAddres = new byte[fromAddres.Length];

            ByteVoFromAddres = ASCIIenc.GetBytes(fromAddres);

            ByteVoFromAddres.CopyTo(tAddressMessageByte, 13);


            // Izvor na poraka 90=Dispecer, 3=Android, 4=sistemska
            // *************************************************************************************
            byte[] ByteVoIzvorNaPoraka = new byte[1];

            char mIzvor = pIzvorNaPoraka;


            ByteVoIzvorNaPoraka = System.BitConverter.GetBytes(mIzvor);
            tAddressMessageByte[13 + fromAddres.Length] = ByteVoIzvorNaPoraka[0];



            return addChkSum(tAddressMessageByte);
        }
       
              



        // Ova e message za Forsiranje avtomatski update na ured:
        // ****************************************************************************************
        public byte[] CreateForceUpdateUnit(string pUnitSerialNumber, char iP1, char iP2, char iP3, char iP4, int pPort)
        {
            byte[] retValByte = new byte[15];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];



            ByteVoBrojNaUred = ASCIIenc.GetBytes(pUnitSerialNumber);

            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '87'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("87");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];

            // PRVO IP adressa

            byte[] ByteVoIP1 = new byte[1];
            ByteVoIP1 = System.BitConverter.GetBytes(iP1);
            retValByte[9] = ByteVoIP1[0];


            byte[] ByteVoIP2 = new byte[1];
            ByteVoIP2 = System.BitConverter.GetBytes(iP2);
            retValByte[10] = ByteVoIP2[0];


            byte[] ByteVoIP3 = new byte[1];
            ByteVoIP3 = System.BitConverter.GetBytes(iP3);
            retValByte[11] = ByteVoIP3[0];


            byte[] ByteVoIP4 = new byte[1];
            ByteVoIP4 = System.BitConverter.GetBytes(iP4);
            retValByte[12] = ByteVoIP4[0];

            // Sega porta
            byte[] ByteVoPorta = new byte[2];
            ByteVoPorta = System.BitConverter.GetBytes(pPort);
            retValByte[13] = ByteVoPorta[0];
            retValByte[14] = ByteVoPorta[1];

            //CheckSum
            // *************************************************************************************
            return addChkSum(retValByte);
        }



        // Ova e message za Prakanje Info vo vrska so prestoj na stanicno mesto
        //            0= Izlegol od stanica (se odjavil, uklucil taksimer)
        //            1= Vlegol na stanica (se prijavil. Treba da vidme dali DEFINITIVNO ke tretirame prostor)
        //            2 = Restart na vreme (nemam poim koga ke se koristi, neka go ima)
        //            3 = Prestani da prikazuvas vreme na stanica na LCD (neka si broi vo pozadina)
        //            4 = Prodolzi da prikazuvas vreme na stanica na LCD (ova e slucaj za otkazana naracka…taka sakaat, iako ne e ok!)
        //           10 = Resetiraj status na stanica
        // ****************************************************************************************
        public byte[] CreateStationStatus(Vehicle pVehicle, int pStationStatus)
        {
            byte[] retValByte = new byte[10];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];

            ByteVoBrojNaUred = ASCIIenc.GetBytes(pVehicle.IdUnitSource.SerialNumber);

            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '88'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("88");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];

            byte[] ByteVoStationStatus = new byte[1];
            ByteVoStationStatus = System.BitConverter.GetBytes((char)pStationStatus);
            retValByte[9] = ByteVoStationStatus[0];

            //CheckSum
            // *************************************************************************************
            return addChkSum(retValByte);
        }


        //public byte[] CreateGreenLight(GlobSaldo.AVL.Entities.Vehicle myVehicle)
        //{
        //    byte[] retValByte = new byte[17];

        //    //AAxxxxx0401050101SS
        //    ASCIIEncoding ASCIIenc = new ASCIIEncoding();
        //    //Zaglavie 'AA'
        //    // *************************************************************************************
        //    retValByte[0] = (byte)'A';
        //    retValByte[1] = (byte)'A';

        //    //Broj na ured '12345'
        //    // *************************************************************************************
        //    byte[] ByteVoBrojNaUred = new byte[5];
        //    ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
        //    retValByte[2] = ByteVoBrojNaUred[0];
        //    retValByte[3] = ByteVoBrojNaUred[1];
        //    retValByte[4] = ByteVoBrojNaUred[2];
        //    retValByte[5] = ByteVoBrojNaUred[3];
        //    retValByte[6] = ByteVoBrojNaUred[4];


        //    //Broj na komanda '04'
        //    // *************************************************************************************
        //    byte[] ByteVoKomanda = new byte[2];
        //    ByteVoKomanda = ASCIIenc.GetBytes("04");
        //    retValByte[7] = ByteVoKomanda[0];
        //    retValByte[8] = ByteVoKomanda[1];


        //    //Adresa na Prv Podatok '0107'
        //    // *************************************************************************************
        //    byte[] ByteVoAdresaNaPodatok = new byte[4];
        //    ByteVoAdresaNaPodatok = ASCIIenc.GetBytes("0107");
        //    retValByte[9] = ByteVoAdresaNaPodatok[0];
        //    retValByte[10] = ByteVoAdresaNaPodatok[1];
        //    retValByte[11] = ByteVoAdresaNaPodatok[2];
        //    retValByte[12] = ByteVoAdresaNaPodatok[3];

        //    //Broj na podatoci '01'
        //    // *************************************************************************************
        //    byte[] ByteVoBrojNaPodatoci = new byte[2];
        //    ByteVoBrojNaPodatoci = ASCIIenc.GetBytes("01");
        //    retValByte[13] = ByteVoBrojNaPodatoci[0];
        //    retValByte[14] = ByteVoBrojNaPodatoci[1];

        //    //Podatok da zapali zeleno svetlo 1
        //    // *************************************************************************************
        //    byte[] ByteVoIntPodatok = new byte[2];
        //    ByteVoIntPodatok = System.BitConverter.GetBytes((int)1);
        //    retValByte[15] = ByteVoIntPodatok[0];
        //    retValByte[16] = ByteVoIntPodatok[1];

        //    //CheckSum 
        //    // *************************************************************************************

        //    return addChkSum(retValByte);
        //}



        // Ova e message za setiranje na:
        // APN, za GPRS konekcija
        // Toa e byte array od chars koj zavrsuva so #
        // ****************************************************************************************
        public byte[] SendAPN(GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {

            byte[] retValByte = new byte[24];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '05' (za char)
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("05");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];

            //Adresa na Prv Podatok '0040'
            // *************************************************************************************
            byte[] ByteVoAdresaNaPodatok = new byte[4];
            ByteVoAdresaNaPodatok = ASCIIenc.GetBytes("0040");
            retValByte[9] = ByteVoAdresaNaPodatok[0];
            retValByte[10] = ByteVoAdresaNaPodatok[1];
            retValByte[11] = ByteVoAdresaNaPodatok[2];
            retValByte[12] = ByteVoAdresaNaPodatok[3];

            //Broj na podatoci '09'
            // *************************************************************************************
            byte[] ByteVoBrojNaPodatoci = new byte[2];
            ByteVoBrojNaPodatoci = ASCIIenc.GetBytes("09");
            retValByte[13] = ByteVoBrojNaPodatoci[0];
            retValByte[14] = ByteVoBrojNaPodatoci[1];

            // *************************************************************************************
            //  Sledat podatoci, ... Stavam fiksno: "internet#"
            //  Stavam 20 char, da se popolni celiot prostor. Da se preslusam so Marjan!
            // *************************************************************************************

            retValByte[15] = (byte)'i';
            retValByte[16] = (byte)'n';
            retValByte[17] = (byte)'t';
            retValByte[18] = (byte)'e';
            retValByte[19] = (byte)'r';
            retValByte[20] = (byte)'n';
            retValByte[21] = (byte)'e';
            retValByte[22] = (byte)'t';
            retValByte[23] = (byte)'#';




            //CheckSum
            // *************************************************************************************
            return addChkSum(retValByte);

        }


        // Ova e message za setiranje na:
        // User name, za GPRS konekcija
        // Toa e byte array od chars koj zavrsuva so #
        // ****************************************************************************************
        public byte[] SendUser(GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {

            byte[] retValByte = new byte[24];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '05' (za char)
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("05");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];

            //Adresa na Prv Podatok '0060'
            // *************************************************************************************
            byte[] ByteVoAdresaNaPodatok = new byte[4];
            ByteVoAdresaNaPodatok = ASCIIenc.GetBytes("0060");
            retValByte[9] = ByteVoAdresaNaPodatok[0];
            retValByte[10] = ByteVoAdresaNaPodatok[1];
            retValByte[11] = ByteVoAdresaNaPodatok[2];
            retValByte[12] = ByteVoAdresaNaPodatok[3];

            //Broj na podatoci '09'
            // *************************************************************************************
            byte[] ByteVoBrojNaPodatoci = new byte[2];
            ByteVoBrojNaPodatoci = ASCIIenc.GetBytes("09");
            retValByte[13] = ByteVoBrojNaPodatoci[0];
            retValByte[14] = ByteVoBrojNaPodatoci[1];

            // *************************************************************************************
            //  Sledat podatoci, ... Stavam fiksno: "internet#"
            //  Stavam 20 char, da se popolni celiot prostor. Da se preslusam so Marjan!
            // *************************************************************************************

            retValByte[15] = (byte)'i';
            retValByte[16] = (byte)'n';
            retValByte[17] = (byte)'t';
            retValByte[18] = (byte)'e';
            retValByte[19] = (byte)'r';
            retValByte[20] = (byte)'n';
            retValByte[21] = (byte)'e';
            retValByte[22] = (byte)'t';
            retValByte[23] = (byte)'#';




            //CheckSum
            // *************************************************************************************
            return addChkSum(retValByte);

        }


        // Ova e message za setiranje na:
        // User password, za GPRS konekcija
        // Toa e byte array od chars koj zavrsuva so #
        // ****************************************************************************************
        public byte[] SendPassword(GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {

            byte[] retValByte = new byte[23];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '05' (za char)
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("05");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];

            //Adresa na Prv Podatok '0080'
            // *************************************************************************************
            byte[] ByteVoAdresaNaPodatok = new byte[4];
            ByteVoAdresaNaPodatok = ASCIIenc.GetBytes("0080");
            retValByte[9] = ByteVoAdresaNaPodatok[0];
            retValByte[10] = ByteVoAdresaNaPodatok[1];
            retValByte[11] = ByteVoAdresaNaPodatok[2];
            retValByte[12] = ByteVoAdresaNaPodatok[3];

            //Broj na podatoci '08'
            // *************************************************************************************
            byte[] ByteVoBrojNaPodatoci = new byte[2];
            ByteVoBrojNaPodatoci = ASCIIenc.GetBytes("08");
            retValByte[13] = ByteVoBrojNaPodatoci[0];
            retValByte[14] = ByteVoBrojNaPodatoci[1];

            // *************************************************************************************
            //  Sledat podatoci, ... Stavam fiksno: "mobimak#"
            // *************************************************************************************

            retValByte[15] = (byte)'m';
            retValByte[16] = (byte)'o';
            retValByte[17] = (byte)'b';
            retValByte[18] = (byte)'i';
            retValByte[19] = (byte)'m';
            retValByte[20] = (byte)'a';
            retValByte[21] = (byte)'k';
            retValByte[22] = (byte)'#';




            //CheckSum
            // *************************************************************************************
            return addChkSum(retValByte);

        }



        public byte[] CreateInfoForVehiclesPerRegion(GlobSaldo.AVL.Entities.Vehicle myVehicle, string strPopUp)
        {
            byte[] tAddressMessageByte = new byte[11 + strPopUp.Length];

            ASCIIEncoding ASCIIenc = new ASCIIEncoding();

            //Zaglavie 'AA'
            // *************************************************************************************
            tAddressMessageByte[0] = (byte)'A';
            tAddressMessageByte[1] = (byte)'A';


            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            tAddressMessageByte[2] = ByteVoBrojNaUred[0];
            tAddressMessageByte[3] = ByteVoBrojNaUred[1];
            tAddressMessageByte[4] = ByteVoBrojNaUred[2];
            tAddressMessageByte[5] = ByteVoBrojNaUred[3];
            tAddressMessageByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '72'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("72");
            tAddressMessageByte[7] = ByteVoKomanda[0];
            tAddressMessageByte[8] = ByteVoKomanda[1]; ;

            
            //Broj bytes vo stringot
            // *************************************************************************************
            byte[] ByteVoString = new byte[2];

            ByteVoString = System.BitConverter.GetBytes(strPopUp.Length);
            tAddressMessageByte[9] = ByteVoString[0];
            tAddressMessageByte[10] = ByteVoString[1];


            //string...
            // *************************************************************************************

            byte[] ByteVoStrPopUp = new byte[strPopUp.Length];

            ByteVoStrPopUp = ASCIIenc.GetBytes(strPopUp);
            ByteVoStrPopUp.CopyTo(tAddressMessageByte, 11);

            return addChkSum(tAddressMessageByte);
        }



        public byte[] CreateBeep(GlobSaldo.AVL.Entities.Vehicle myVehicle, Int16 pSeconds)
        {
            byte[] retValByte = new byte[11];

         
            ASCIIEncoding ASCIIenc = new ASCIIEncoding();
            //Zaglavie 'AA'
            // *************************************************************************************
            retValByte[0] = (byte)'A';
            retValByte[1] = (byte)'A';

            //Broj na ured '12345'
            // *************************************************************************************
            byte[] ByteVoBrojNaUred = new byte[5];
            ByteVoBrojNaUred = ASCIIenc.GetBytes(myVehicle.IdUnitSource.SerialNumber);
            retValByte[2] = ByteVoBrojNaUred[0];
            retValByte[3] = ByteVoBrojNaUred[1];
            retValByte[4] = ByteVoBrojNaUred[2];
            retValByte[5] = ByteVoBrojNaUred[3];
            retValByte[6] = ByteVoBrojNaUred[4];


            //Broj na komanda '50'
            // *************************************************************************************
            byte[] ByteVoKomanda = new byte[2];
            ByteVoKomanda = ASCIIenc.GetBytes("50");
            retValByte[7] = ByteVoKomanda[0];
            retValByte[8] = ByteVoKomanda[1];


            //Podatok kolku sec da bipka uredot
            // *************************************************************************************
            byte[] ByteVoIntPodatok = new byte[2];
            ByteVoIntPodatok = System.BitConverter.GetBytes(pSeconds);
            retValByte[9] = ByteVoIntPodatok[0];
            retValByte[10] = ByteVoIntPodatok[1];

            //CheckSum 
            // *************************************************************************************

            return addChkSum(retValByte);
        }




        public byte[] addChkSum(byte[] m_message)
        {
            //byte[] retVal = new byte[m_message.Length + 2];
            byte[] retVal = new byte[m_message.Length + 7];

            for (int i = 0; i < m_message.Length; i++)
            {
                retVal[i] = m_message[i];
            }

            byte tmpByte = (byte)0;


            foreach (byte item in m_message)
            {
                tmpByte = (byte)(tmpByte ^ item);
            }

            //retVal[retVal.Length - 1] = (byte)((tmpByte & 0x0f) | 0x30);
            //retVal[retVal.Length - 2] = (byte)(((tmpByte & 0xf0) >> 4) | 0x30);

            retVal[retVal.Length - 6] = (byte)((tmpByte & 0x0f) | 0x30);
            retVal[retVal.Length - 7] = (byte)(((tmpByte & 0xf0) >> 4) | 0x30);

            for (int i = 5; i > 0; i--)
            {
                retVal[retVal.Length - i] = (byte)'z';
            }

            return retVal;
        }
    }
}
