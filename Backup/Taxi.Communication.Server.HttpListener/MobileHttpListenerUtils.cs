using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using log4net;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.Bases;

using System.Configuration;

using System.Threading;
using System.Net.Mail;
using Taxi.Communication.Server.Utils;


namespace Taxi.Communication.Server.HttpListener
{
    public class MobileHttpListenerUtils
    {
        private static ILog log = LogManager.GetLogger("MyService");

        public void SendEmail(string pEmail, string pBody)
        {
            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                
                mail.From = new MailAddress("moetotaksi@gmail.com");
                mail.To.Add(pEmail);
                mail.Subject = "Generiran kod...";
                mail.Body = pBody;
                

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("moetotaksi", "donka12345");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);                
            }
            catch (Exception ex)
            {
                log.Error("ZORAN: Greska vo prakanje mail!!!", ex);
            }
        }



        public void SendEmailForReservation(MobileReservations pMobileReservations, MobileUser pMobileUser, string pSubject, string pTypeOfMail)
        {
            string strSubDirAndFile = "";
            string strSubDirAndFileSMS = "";

            try
            {
                MailMessage mail = new MailMessage();
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                mail.From = new MailAddress("moetotaksi@gmail.com");

                mail.To.Add(pMobileUser.Email);

                //Prvo, da gi parsiram kompaniite od kae se bara kola
                //Tuka toa mi treba za da znam kade da pratam e-mail
                //Malku go pravam komplicirano, no za sekoj slucaj, da ne se napisat loso adresite, pa da pagja
                //Predviduva da moze da se pratat na poveke e-mail adresi, podeleleni so zapirka

                char[] delimiterChars = { ',' };
                string[] stringIdCompany = pMobileReservations.Companies.Split(delimiterChars);

                string strCompanies = "";   // Ova mi treba za zamena vo HTML file-ovite

                //Zemam spisok na e-mail adresi za Lotus
                string[] lstLotusEmailAddresses = {};
                string strLotusEmailAddresses = ConfigurationManager.AppSettings["LotusEmailAddresses"];

                if (strLotusEmailAddresses != null)
                {                   
                    string[] tmpLotusEmailAddresses = strLotusEmailAddresses.Split(delimiterChars);
                    Array.Resize(ref lstLotusEmailAddresses, tmpLotusEmailAddresses.Length);

                    for (int n = 0; n < tmpLotusEmailAddresses.Length; n++)
                    {
                        lstLotusEmailAddresses[n] = tmpLotusEmailAddresses[n];
                    }
                }                

                //Zemam spisok na e-mail adresi za NaseTaksi
                string[] lstNaseTaksiEmailAddresses = { };
                string strNaseTaksiEmailAddresses = ConfigurationManager.AppSettings["NaseTaksiEmailAddresses"];

                if (strNaseTaksiEmailAddresses != null)
                {
                    string[] tmpNaseTaksiEmailAddresses = strNaseTaksiEmailAddresses.Split(delimiterChars);
                    Array.Resize(ref lstNaseTaksiEmailAddresses, tmpNaseTaksiEmailAddresses.Length);

                    for (int n = 0; n < tmpNaseTaksiEmailAddresses.Length; n++)
                    {
                        lstNaseTaksiEmailAddresses[n] = tmpNaseTaksiEmailAddresses[n];
                    }
                }      


                foreach (string tmpIdCompany in stringIdCompany)
                {
                    log.Debug("pMobileReservations.Companies: -" + pMobileReservations.Companies + "-");
                    log.Debug("pMobileReservations.Imei     : " + pMobileReservations.Imei);
                   
                    if (tmpIdCompany != "")
                    {
                        if (int.Parse(tmpIdCompany.Trim()) == 1)   //NaseTaksi
                        {
                            if (strCompanies == "")
                                strCompanies = "Наше Такси";
                            else
                                strCompanies = strCompanies + ", Наше Такси";

                            if (lstNaseTaksiEmailAddresses != null && lstNaseTaksiEmailAddresses.Length > 0)
                                foreach (string tmpStrNT in lstNaseTaksiEmailAddresses)
                                    if (tmpStrNT != "")
                                        mail.Bcc.Add(tmpStrNT);
                        }

                        if (int.Parse(tmpIdCompany.Trim()) == 7)   //Lotus
                        {
                            if (strCompanies == "")
                                strCompanies = "Лотус";
                            else
                                strCompanies = strCompanies + ", Лотус";


                            if (lstLotusEmailAddresses != null && lstLotusEmailAddresses.Length > 0)
                                foreach (string tmpStrL in lstLotusEmailAddresses)
                                    if (tmpStrL != "")
                                        mail.Bcc.Add(tmpStrL);
                        }
                    }
                }                    


                mail.Subject = pSubject;


                switch (pTypeOfMail)
                {
                    case "NewReservation":
                        strSubDirAndFile = "\\MailTemplates\\NewReservation.htm";
                        strSubDirAndFileSMS = "\\MailTemplates\\NewReservationSMS.txt";
                        break;
                    case "UpdateReservation":
                        strSubDirAndFile = "\\MailTemplates\\UpdateReservation.htm";
                        strSubDirAndFileSMS = "\\MailTemplates\\UpdateReservationSMS.txt";
                        break;
                    case "CancelReservation":
                        strSubDirAndFile = "\\MailTemplates\\CancelReservation.htm";
                        strSubDirAndFileSMS = "\\MailTemplates\\CancelReservationSMS.txt";
                        break;
                    default:
                        break;
                }


                string appPath = AppDomain.CurrentDomain.BaseDirectory;
                
                // Prvo e-mail
                string strBody = System.IO.File.ReadAllText(appPath + strSubDirAndFile, Encoding.GetEncoding("Windows-1251"));              

                strBody = strBody.Replace("#PhoneNumber#", pMobileUser.PhoneNumber);
                strBody = strBody.Replace("#ID_MobileReservations#", pMobileReservations.IdMobileReservations.ToString());
                strBody = strBody.Replace("#StreetName#", pMobileReservations.StreetName);
                strBody = strBody.Replace("#StreetNumber#", pMobileReservations.StreetNumber.ToString());
                strBody = strBody.Replace("#PickupAdress#", pMobileReservations.PickupAdress);
                strBody = strBody.Replace("#То#", pMobileReservations.To);
                strBody = strBody.Replace("#Comment#", pMobileReservations.Comment);
                strBody = strBody.Replace("#ReservationPickUpTime#", pMobileReservations.ReservationPickUpTime.ToString("HH:mm  dd-MM-yyyy"));
                strBody = strBody.Replace("#Companies#", strCompanies);
                             
                mail.Body = strBody;
                mail.IsBodyHtml = true;              

                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential("moetotaksi", "donka12345");
                SmtpServer.EnableSsl = true;

                SmtpServer.Send(mail);

                //Potoa SMS
                string strSMS = System.IO.File.ReadAllText(appPath + strSubDirAndFileSMS, Encoding.GetEncoding("Windows-1251"));

                strSMS = strSMS.Replace("#IdMobileReservation#", pMobileReservations.IdMobileReservations.ToString());
                strSMS = strSMS.Replace("#email#", pMobileUser.Email);

                SmSsent mSmsSent = new SmSsent();

                mSmsSent.PhoneNumber = pMobileUser.PhoneNumber;
                mSmsSent.SmStext = strSMS;

                DataRepository.SmSsentProvider.Insert(mSmsSent);
            }
            catch (Exception ex)
            {
                log.Error("ZORAN: Greska vo prakanje mail!!!", ex);
            }
        }


        public long RegisterMobileUser(string pEmail, string pPassword, string pName, string pLastName, string pPhoneNumber, string pImei)
        {
            long retVal = -1;
            MobileUser mMobileUser = new MobileUser();

            // Prvo gledam vo koj tip na user spaga
            // Mozni se 4 slucai:
            //      1. Da nema takov mail + IMEI, znaci 100% nov
            //      2. Da ima postoecki mail + IMEI, znaci povtorna instalacija, ist user na ist ured 
            //      3. Da ima postoecki mail, nov IMEI, znaci drug ured, da se iskluci na stariot
            //      4. Da ima nov mail, postoeci IMEI, znaci nov user na postoecki ured

            // Prvo baram po IMEI ( taka ke gi proveram opcii 2 i 4)            

            TList<MobileUser> lstMobileUserSelected = DataRepository.MobileUserProvider.GetByIMEI(pImei);

            if (lstMobileUserSelected != null && lstMobileUserSelected.Count > 0)
            {
                mMobileUser = lstMobileUserSelected[0];

                if (lstMobileUserSelected[0].Email == pEmail)   // Opcija 2
                {
                    //Prvo da stavam tekovnata sostojba vo MobileUserHistory
                    InsertNewMobileUserHistory(mMobileUser, "Повторна регистрација на постоечки корисник(ist e-mail + ist IMEI");

                    // Sega da UPDATE-iram tekoven zapis, bidejki Email + IMEI se isti, da ne se smenilo drugo
                    // 
                    mMobileUser.Name = pName;
                    mMobileUser.Lastname = pLastName;                    
                    mMobileUser.Password = pPassword;
                    mMobileUser.PhoneNumber = pPhoneNumber;
                    mMobileUser.IsTemporaryDiscarded = false;
                    mMobileUser.Code = GenerateCode(pImei, mMobileUser.IdMobileUser);
                    mMobileUser.CodeDateTimeSent = System.DateTime.Now;
                    mMobileUser.CodeDateTimeConfirmed = null;

                    DataRepository.MobileUserProvider.Update(mMobileUser);

                    retVal = (long)mMobileUser.Code;
                }
                else                                            // Opcija 4
                {
                    //Prvo da stavam tekovnata sostojba vo MobileUserHistory
                    InsertNewMobileUserHistory(mMobileUser, "Повторна регистрација на постоечки корисник, ist IMEI, razlicen e-mail");

                    // Sega da UPDATE-iram tekoven zapis, bidejki IMEI-to e isto, da ne se smenilo drugo (sekako email)
                    // 
                    mMobileUser.Name = pName;
                    mMobileUser.Lastname = pLastName;
                    mMobileUser.Email = pEmail;
                    mMobileUser.Password = pPassword;
                    mMobileUser.PhoneNumber = pPhoneNumber;
                    mMobileUser.IsTemporaryDiscarded = false;
                    mMobileUser.Code = GenerateCode(pImei, mMobileUser.IdMobileUser);
                    mMobileUser.CodeDateTimeConfirmed = null;

                    DataRepository.MobileUserProvider.Update(mMobileUser);

                    retVal = (long)mMobileUser.Code;                        
                }
            }
            
            else    // Sega proveruvam za opcija 1 i 3
            {                 
                    lstMobileUserSelected = DataRepository.MobileUserProvider.GetByEmail(pEmail);

                    if (lstMobileUserSelected != null && lstMobileUserSelected.Count > 0)   // Opcija 3
                    {
                        //Prvo da stavam tekovnata sostojba vo MobileUserHistory
                        InsertNewMobileUserHistory(lstMobileUserSelected[0], "Регистрација со постоечки e-mail, на нов IMEI");

                        //Sega gobri[am stariot Mobile User (stariot telefon)
                        DataRepository.MobileUserProvider.Delete(lstMobileUserSelected[0]);

                        //Insertirm celosno nov MobileUser
                        retVal = InsertNewMobileUser(pEmail, pPassword, pName, pLastName, pPhoneNumber, pImei);
                    }
                    else                     // Opcija 1
	                {
                        retVal = InsertNewMobileUser(pEmail, pPassword, pName, pLastName, pPhoneNumber, pImei);
	                }                                            
            }                                    

            return retVal;
        }


        private long InsertNewMobileUser(string pEmail, string pPassword, string pName, string pLastName, string pPhoneNumber, string pImei)
        {
            long retVal = -1;
            MobileUser mMobileUser = new MobileUser();

            mMobileUser.Name = pName;
            mMobileUser.Lastname = pLastName;
            mMobileUser.Email = pEmail;
            mMobileUser.Password = pPassword;
            mMobileUser.PhoneNumber = pPhoneNumber;
            mMobileUser.Imei = pImei;
            mMobileUser.CodeDateTimeSent = System.DateTime.Now;
            mMobileUser.IsTemporaryDiscarded = false;

            DataRepository.MobileUserProvider.Insert(mMobileUser);

            TList<MobileUser> lstMobileUserSelected = DataRepository.MobileUserProvider.GetByIMEI(pImei);                       

            retVal = GenerateCode(pImei, lstMobileUserSelected[0].IdMobileUser);

            mMobileUser.Code = retVal;

            DataRepository.MobileUserProvider.Update(mMobileUser);

            return retVal;
        }


        public long InsertNewMobileUserHistory(MobileUser pMobileUser, string pComment)
        {
            long retVal = -1;

            try
            {
                MobileUserHistory mMobileUserHistory = new MobileUserHistory();

                mMobileUserHistory.Email = pMobileUser.Email;
                mMobileUserHistory.Name = pMobileUser.Name;
                mMobileUserHistory.Lastname = pMobileUser.Lastname;
                mMobileUserHistory.Password = pMobileUser.Password;
                mMobileUserHistory.PhoneNumber = pMobileUser.PhoneNumber;
                mMobileUserHistory.Imei = pMobileUser.Imei;
                mMobileUserHistory.IsTemporaryDiscarded = pMobileUser.IsTemporaryDiscarded;
                mMobileUserHistory.IsDeleted = pMobileUser.IsDeleted;
                mMobileUserHistory.Code = pMobileUser.Code;
                mMobileUserHistory.CodeDateTimeSent =  pMobileUser.CodeDateTimeSent;
                mMobileUserHistory.CodeDateTimeVerified = pMobileUser.CodeDateTimeConfirmed;
                mMobileUserHistory.DateTimeSendToHistory = System.DateTime.Now;
                mMobileUserHistory.Comment = pComment;

                DataRepository.MobileUserHistoryProvider.Insert(mMobileUserHistory);

                retVal = 1;
            }
            catch (Exception ex)
            {
               log.Error("InsertNewMobileUserHistory ERROR...", ex);
            }

            return retVal;
        }



        public string ConfirmRepeatedOrders(string pImei, string pIdOrder)
        {
            string retVal = "";

            RepeatedOrders mRepeatedOrders = new RepeatedOrders();

            try
            {
                TList<MobileUser> lstMobileUser = DataRepository.MobileUserProvider.GetByIMEI(pImei);

                if (lstMobileUser != null && lstMobileUser.Count > 0)
                {
                    mRepeatedOrders.IdOrders = long.Parse(pIdOrder);
                    mRepeatedOrders.Imei = pImei;
                    mRepeatedOrders.DateTimeRepeated = System.DateTime.Now;
                    mRepeatedOrders.Comment = "Возилото зело друг патник";

                    DataRepository.RepeatedOrdersProvider.Insert(mRepeatedOrders);
                }
            }
            catch (Exception ex)
            {

                log.Error("ANDROID: Greska vo zapis na ConfirmRepeatedOrders!!", ex);
            }

            return retVal;
        }

        public string GetCorrectStateString(Vehicle pVehicle)
        {
            string pString = pVehicle.currentStateString;
            string retVal = pString;

            switch (pString)
            {
                case "StateUndefined":
                    retVal = "Stop";
                    break;
                case "StateIdle":
                    retVal = "Stop";
                    break;
                case "StateAlarmConfirmed":
                    retVal = "Stop";
                    break;
                case "StateBusyNextPhoneCall":
                    retVal = "StateBusy";
                    break;
                case "StateFiscalBeforeIdle":
                    retVal = "Stop";
                    break;
                case "StateAlarm":
                    retVal = "Stop";
                    break;
                case "StateMoveToClient":
                    retVal = "StateMoveToClient";
                    break;
                case "StateMoveToClientNewPhoneCall":
                    retVal = "StateMoveToClient";
                    break;
                case "StatePause":
                    retVal = "Stop";
                    break;
                case "StateShiftEnded":
                    retVal = "Stop";
                    break;
                case "StateWaitClientNewPhoneCall":
                    retVal = "StateWaitClient";
                    break;
            }

            // Ova samo za Nase Taksi, akoima freekm uklucno...
            // DA SE VNIMAVA dali Marjan dobrogi resetira senzorive!!!
            if (pVehicle.IdCompany == 1)
            {
                if (pVehicle.currentSensorData.Senzor_9 == 1 || pVehicle.currentSensorData.Senzor_10 == 1)
                    retVal = "StateBusy";
            }
            return retVal;
        }


        private string GetCorrectStateStringForOverview(Vehicle pVehicle)
        {
            string pString = pVehicle.currentStateString;
            string retVal = pString;

            switch (pString)
            {
                case "StateUndefined":
                    retVal = "Nedefinirana";
                    break;
                case "StateIdle":
                    retVal = "Sloboden";
                    break;
                case "StateAlarmConfirmed":
                    retVal = "Alarm";
                    break;
                case "StateBusyNextPhoneCall":
                    retVal = "Zafaten";
                    break;
                case "StateFiscalBeforeIdle":
                    retVal = "PecatiSmetka";
                    break;
                case "StateAlarm":
                    retVal = "Alarm";
                    break;
                case "StateMoveToClient":
                    retVal = "Kon klient";
                    break;
                case "StateMoveToClientNewPhoneCall":
                    retVal = "Kon klient";
                    break;
                case "StatePause":
                    retVal = "Pauza";
                    break;
                case "StateShiftEnded":
                    retVal = "Kraj na smena";
                    break;
                case "StateWaitClient":
                    retVal = "Ceka klient";
                    break;
                case "StateWaitClientNewPhoneCall":
                    retVal = "Ceka klient";
                    break;
            }

            // Ova samo za Nase Taksi, akoima freekm uklucno...
            // DA SE VNIMAVA dali Marjan dobro gi resetira senzorive!!!
            if (pVehicle.IdCompany == 1)
            {
                if (pVehicle.currentSensorData.Senzor_9 == 1 || pVehicle.currentSensorData.Senzor_10 == 1)
                    retVal = "FreeKM";
            }
            return retVal;
        }


        private int GetCorrectStateColor(Vehicle pVehicle)
        {
            string pString = pVehicle.currentStateString;
            int retVal = 0;

            switch (pString)
            {
                case "StateUndefined":
                    retVal = 0;     //Crna
                    break;
                case "StateIdle":
                    retVal = 1;     //Zelena
                    break;
                case "StateAlarmConfirmed":
                    retVal = 0;     //Crna (ne moze da vleze tuka)
                    break;
                case "StateBusy":
                    retVal = 3;     //Crvena
                    break;
                case "StateBusyNextPhoneCall":
                    retVal = 3;     //Crvena
                    break;
                case "StateFiscalBeforeIdle":
                    retVal = 3;     //Crvena
                    break;
                case "StateAlarm":
                    retVal = 0;     //Crna (ne moze da vleze tuka)
                    break;
                case "StateMoveToClient":
                    retVal = 2;     //Zolta
                    break;
                case "StateMoveToClientNewPhoneCall":
                    retVal = 2;     //Zolta
                    break;
                case "StatePause":
                    retVal = 4;     //Plava
                    break;
                case "StateShiftEnded":
                    retVal = 5;     //Nekoja
                    break;
                case "StateWaitClientConfirmation":
                    retVal = 2;     //Zolta
                    break;                    
                case "StateWaitClient":
                    retVal = 2;     //Zolta
                    break;
                case "StateWaitClientNewPhoneCall":
                    retVal = 2;     //Zolta
                    break;
            }

            //  Ova samo za Nase Taksi i Sonce, ako ima freekm uklucno...
            //  Ne proveruvam za toa koja kompanija e, posto drugite se nuli!!!
            //  DA SE VNIMAVA dali Marjan dobro gi resetira senzorive!!!
            
            if (pVehicle.currentSensorData.Senzor_9 == 1 || pVehicle.currentSensorData.Senzor_10 == 1)
                    retVal = 6;     //Portokalova
            
            return retVal;
        }

        public string GetCorrectAddressString(string pStreetName, string pStreetNumber, string pPickupAddress, string pComment, string pDo)
        {
            string retVal = "";

            try
            {

                if ((pPickupAddress.Trim().Length + pStreetName.Trim().Length) > 35)      // So Ljupco dogovorivme da bide Adresa + PickupAddress <= 35
                {
                    if (pPickupAddress.Trim().Length > 35)
                    {
                        pPickupAddress = pPickupAddress.Substring(0, 34);
                        retVal = retVal + "";
                    }
                    else
                    {
                        retVal = retVal + pStreetName.Trim().Substring(0, 35 - pPickupAddress.Length);
                    }
                }
                else
                    retVal = retVal + pStreetName;


                if (pStreetNumber != "")
                    retVal = retVal + ", " + pStreetNumber.Trim();

                if (pPickupAddress != "")
                    retVal = retVal + ", " + pPickupAddress.Trim();

                if (pComment != "")
                    retVal = retVal + ", " + pComment.Trim();

                if (pDo != "")
                    retVal = retVal + ", Do: " + pDo.Trim();
            }
            catch (Exception ex)
            {
                log.Error("ZORAN: greska vo GetCorrectAddressString!!!", ex);
                log.Debug("pStreetName:    " + pStreetName);
                log.Debug("pStreetNumber:  " + pStreetNumber);
                log.Debug("pPickupAddress: " + pPickupAddress);
                log.Debug("pComment:       " + pComment);
                log.Debug("pDo:            " + pDo);
            }

            return retVal;
        }


        public JsonVehiclesStates FillVehicleState(Vehicle pVehicle)
        {
            JsonVehiclesStates retVal = new JsonVehiclesStates();

            try
            {
                retVal.IdVehicle = pVehicle.IdVehicle;
                retVal.Plate = pVehicle.Plate;
                retVal.DescriptionShort = pVehicle.DescriptionShort;
                retVal.LongitudeX = pVehicle.currentGPSData.Longitude_X;
                retVal.LatitudeY = pVehicle.currentGPSData.Latutude_Y;
                retVal.Bearing = pVehicle.currentGPSData.Bearing;
                retVal.Speed = pVehicle.currentGPSData.Speed.ToString("###");
                retVal.VehicleState = GetCorrectStateStringForOverview(pVehicle);
                retVal.VehicleStateColor = GetCorrectStateColor(pVehicle);

                TList<ShiftInOut> lstShiftInOut = DataRepository.ShiftInOutProvider.GetByIDVehicleAndDateTimeNull((long)pVehicle.IdVehicle);

                if (lstShiftInOut != null && lstShiftInOut.Count > 0)
                {
                    Driver mDriver = DataRepository.DriverProvider.GetByIdDriver(lstShiftInOut[0].IdDriver);

                    if (mDriver != null)
                    {
                        String tmpString = mDriver.Name.Trim() + " " + mDriver.LastName.Trim();
                        retVal.Driver = Taxi.Communication.Server.Utils.UnicodeStrings.UncodeToAscii(tmpString); 

                        if (lstShiftInOut[0].DateTimeIn != null && lstShiftInOut[0].DateTimeIn != DateTime.MinValue)
                        {
                            TimeSpan IntervalShift = DateTime.Now.Subtract(lstShiftInOut[0].DateTimeIn);
                            retVal.DriverStart = IntervalShift.TotalHours.ToString("0") + ":" + IntervalShift.Minutes.ToString("00") + ":" + IntervalShift.Seconds.ToString("00");
                        }
                        else
                        {
                            retVal.DriverStart = "00:00:00";
                        }
                    }
                }
                else
                {
                    retVal.Driver = "Nema logirano";
                    retVal.DriverStart = "00:00:00";
                }

                if (pVehicle.TaximetarLast != null && pVehicle.TaximetarLast != DateTime.MinValue)
                {
                    TimeSpan IntervalTaximeter = DateTime.Now.Subtract(pVehicle.TaximetarLast);
                    retVal.Taximeter = IntervalTaximeter.TotalHours.ToString("0") + ":" + IntervalTaximeter.Minutes.ToString("00") + ":" + IntervalTaximeter.Seconds.ToString("00");
                }
                else
                {
                    retVal.Taximeter = "00:00:00";
                }

                if (pVehicle.currentGPSData != null && pVehicle.currentGPSData.UTC != null)
                {
                    DateTime mDateTime = pVehicle.currentGPSData.UTC;

                    if (mDateTime.AddDays(100) > System.DateTime.Now)
                    {                      
                        TimeSpan IntervalLastLocation = DateTime.Now.Subtract(CalculateLocalTime(pVehicle.currentGPSData.UTC, pVehicle));
                        retVal.LastLocation = IntervalLastLocation.TotalHours.ToString("0") + ":" + IntervalLastLocation.Minutes.ToString("00") + ":" + IntervalLastLocation.Seconds.ToString("00");
                    }
                    else
                    {
                        retVal.LastLocation = "00:00:00";
                    }
                }
                else
                {
                    retVal.LastLocation = "00:00:00";
                }

                if (pVehicle.OnStationFromDateTime != DateTime.MinValue)
                {
                    TimeSpan IntervalLastStation = DateTime.Now.Subtract(pVehicle.OnStationFromDateTime);
                    retVal.Stanica = IntervalLastStation.TotalHours.ToString("0") + ":" + IntervalLastStation.Minutes.ToString("00") + ":" + IntervalLastStation.Seconds.ToString("00");
                }
                else
                {
                    retVal.Stanica = "NE";
                }
                
                if (pVehicle.currentGPSData != null && pVehicle.currentGPSData.RegionName != null)
                    retVal.Region = UnicodeStrings.UncodeToAscii(pVehicle.currentGPSData.RegionName.Trim());
                else
                    retVal.Region = ".";
                
                if (pVehicle.CurrentPhoneCall != null && 
                        pVehicle.CurrentPhoneCall.oAddressTo != null &&
                        pVehicle.CurrentPhoneCall.oAddressTo.oGisRegions != null &&
                        pVehicle.CurrentPhoneCall.oAddressTo.oGisRegions.RegionName != null &&
                        pVehicle.CurrentPhoneCall.oAddressTo.oGisRegions.RegionName.Length > 2)
                {
                    retVal.DoRegion = UnicodeStrings.UncodeToAscii(pVehicle.CurrentPhoneCall.oAddressTo.oGisRegions.RegionName);
                }
                else
                    retVal.DoRegion = "...";

                // Sega za AdresaDo
                
                retVal.Adresa1 = "...";

                if (pVehicle.CurrentPhoneCall != null && pVehicle.CurrentPhoneCall.oAddressFrom != null)
                {
                    if (pVehicle.CurrentPhoneCall.MessageType == "MC")   //Ova e od mobilen, pa se zema samo StreetName, kade e CELA adresa
                    {
                        if (pVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets != null)
                            retVal.Adresa1 = pVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets.StreetName;                      
                    }
                    else            //Ova znaci deka e od dispecer
                    {
                        if (pVehicle.CurrentPhoneCall.oAddressFrom.oGisObjects != null)
                        {
                            string tmpObjectName = pVehicle.CurrentPhoneCall.oAddressFrom.oGisObjects.ObjectName;
                            retVal.Adresa1 = UnicodeStrings.UncodeToAscii(tmpObjectName);
                        }
                        if (pVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets != null)
                        {
                            string tmoStreetName = pVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets.StreetName.Trim();
                            retVal.Adresa1 = UnicodeStrings.UncodeToAscii(tmoStreetName) + " " + pVehicle.CurrentPhoneCall.oAddressFrom.HouseNumber.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("FillVehicleState...", ex);
                retVal = null;
            }


            return retVal;
        }



        private long GenerateCode(string pImei, long pIdMobileUser)
        {
            long retVal = -1;

            try
            {
                // retVal = pIdMobileUser + long.Parse(pImei.Substring(pImei.Length - 4));
                Random rand = new Random();
                retVal = (long)rand.Next(1000, 9999);
            }
            catch (Exception ex)
            {
                log.Error("GenerateCode ERROR...", ex);
            }

            return retVal;
        }


        public List<long> getListOfCompanies(string pListOfCompanies)
        {
            List<long> retVal = new List<long>();

            char[] delimiterChars = { ',' };

            string[] stringIdCompany = pListOfCompanies.Split(delimiterChars);            

            if (stringIdCompany != null && stringIdCompany.GetLength(0) > 0)
            {
                foreach (string tmpStr in stringIdCompany)
                    if(tmpStr != "")
                        retVal.Add(long.Parse(tmpStr));
            }

            return retVal;
        }



        private DateTime CalculateLocalTime(DateTime pUTC, Vehicle pVehicle)
        {

            // Ova e ako ne moze da iskalkulira, pa da ima nesto. 
            // Pokasno ke stavam i vo log, ako ima potreba...

            DateTime retVal = DateTime.Parse("01/01/9999 00:00:00");

            double tmpHours = -9999;


            if (pVehicle.IdCompanySource != null)
                if (pVehicle.IdCompanySource.IdCompanyTimeZoneSource != null)
                    tmpHours = pVehicle.IdCompanySource.IdCompanyTimeZoneSource.Hours;

            if (tmpHours != -9999)
            {
                if (pUTC.IsDaylightSavingTime() == true)
                {
                    retVal = pUTC.AddHours(tmpHours + 1);
                }
                else
                {
                    retVal = pUTC.AddHours(tmpHours);
                }

            }

            return retVal;
        }
    }
}
