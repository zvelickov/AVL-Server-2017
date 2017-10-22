using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using log4net;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public class clsRfIdCardProcessor
    {

        clsPenaltyCreator myPenaltyCreator = new clsPenaltyCreator();

        public string ProcessCheckOutDriver(string RfIdCard, GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {

            //Proveruvam dali mozam da go odjavam vozacot pravilno
            //Ako e istiot vozac koj e momentalno najaven go odjavuvam pravilno i vrakam True
            //Ako proba da se odjavi drug vozac, prethodniot go odjavuvam, ama vrakam False


            //PAZI, SEKOGAS VRAKA TRUE, ZA TESTIRANJE NA LOGIKA< KE GLEDAME DALI ZAPISUVA VO BAZA



            // Zoran: Kako da doaga ovde i koga RfIdCard = '0000000000'
            if (RfIdCard == "0000000000") return "";


            

            TList<GlobSaldo.AVL.Entities.RfIdCards> tRfIdCardList = GlobSaldo.AVL.Data.DataRepository.RfIdCardsProvider.GetBySerialNumberRfIdCard(RfIdCard);


            if (tRfIdCardList.Count == 0) return "Nevalidna karticka";
            
            GlobSaldo.AVL.Entities.RfIdCards tRfIdCard = tRfIdCardList[0];


            //Za koj vozac e
            TList<GlobSaldo.AVL.Entities.Driver> tDriverList = DataRepository.DriverProvider.GetByIDRfIdCard (tRfIdCard.IdRfIdCard);

            if (tDriverList.Count == 0) return "";

            GlobSaldo.AVL.Entities.Driver tDriver = tDriverList[0];


            //PAZI, Tuka zemam posleden zapis od tabela ShiftInOut
            //TODO Za IDVozilo site zapisi za ShiftInOut kade DateOut = null
            TList<GlobSaldo.AVL.Entities.ShiftInOut> tShiftInOutList = GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.GetByIdVehicle(myVehicle.IdVehicle);

            if (tShiftInOutList.Count == 0)
            {
                //Nema zapis, Ne bi smeelo, ke vratam Odjaven bidejki ne zna sto drugo!
                return ("000");
            }
            else
            {
                //Dali e najaven Toj sto saka da se odjavi, Zemam posleden, radi mozni nezapisuvanja na odjava
                GlobSaldo.AVL.Entities.ShiftInOut tShiftInOut = tShiftInOutList[tShiftInOutList.Count - 1];
                if (tDriver.IdDriver == tShiftInOut.IdDriver)
                {
                    //Regularna odjava
                    tShiftInOut.DateTimeOut = DateTime.Now;
                    tShiftInOut.RegularShiftOut = true;
                    try
                    {
                        GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Update(tShiftInOut);
                        //myPenaltyCreator.Createpenalty(10, myVehicle);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("ERROR Update ShiftInOutProvider " + ex.Message);
                        //ServiceCallBack.log.Error("ERROR ", ex);
                    }
                    myVehicle.DriverShiftInOut = null;
                }
                else
                {
                    //Losa odjava
                    tShiftInOut.DateTimeOut = DateTime.Now;

                    // ZORAN
                    tShiftInOut.RegularShiftOut = false;

                    //tShiftInOut.RegularShiftOut = true;

                    try
                    {
                        GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Update(tShiftInOut);
                        //myPenaltyCreator.Createpenalty(10, myVehicle);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("ERROR Update ShiftInOutProvider " + ex.Message);
                        //ServiceCallBack.log.Error("ERROR ", ex);
                    }
                }

                //Sega i vo vozilo zapisuvam Deka se odjavil
                myVehicle.DriverShiftInOut = null;
               
                return ("000"); //Sekako vrati za odjava
            }

        }

        public string ProcessCheckInDriver(string RfIdCard, GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {

            //Proveruvam dali mozam da go najavam vozacot
            //Ako go najavam vrakam True

            //PAZI, SEKOGAS VRAKA TRUE, ZA TESTIRANJE NA LOGIKA< KE GLEDAME DALI ZAPISUVA VO BAZA


            // Zoran: Kako da doaga ovde if koga RfIdCard = '0000000000'
            if (RfIdCard == "0000000000") return "";


            TList<GlobSaldo.AVL.Entities.RfIdCards> tRfIdCardList = GlobSaldo.AVL.Data.DataRepository.RfIdCardsProvider.GetBySerialNumberRfIdCard(RfIdCard);

            // ZORAN
            if (tRfIdCardList.Count == 0) return "";              // Nema takva kartica, nema sto da pravi...


            GlobSaldo.AVL.Entities.RfIdCards tRfIdCard = tRfIdCardList[0];

            //Za koj vozac e
            TList<GlobSaldo.AVL.Entities.Driver> tDriverList = GlobSaldo.AVL.Data.DataRepository.DriverProvider.GetByIDRfIdCard(tRfIdCard.IdRfIdCard);

            if (tDriverList.Count == 0) return "";                      //Nema vozac so takva kartica, nema sto da pravi ponatamu!

            GlobSaldo.AVL.Entities.Driver tDriver = tDriverList[0];     // Ovde e glupo, ne smee da ima poveke od eden vozas, no....

            //Prvo, Za sekoj slucaj proveruvam koj e najaven vo baza
             TList<GlobSaldo.AVL.Entities.ShiftInOut> tShiftInOutList = GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.GetByIdVehicle(myVehicle.IdVehicle);

            if (tShiftInOutList.Count == 0)
            {
                //Nema zapis, najavi go momentalniov
                GlobSaldo.AVL.Entities.ShiftInOut tShiftInOutNew = new GlobSaldo.AVL.Entities.ShiftInOut();
                tShiftInOutNew.IdDriver = tDriver.IdDriver;
                tShiftInOutNew.IdVehicle = myVehicle.IdVehicle;
                tShiftInOutNew.DateTimeIn = DateTime.Now;
                try
                {
                    GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Insert(tShiftInOutNew);
                    myPenaltyCreator.Createpenalty(9, myVehicle);
                }
                catch (Exception ex)
                {
                    Console.Write("ERROR Insert ShiftInOutProvider " + ex.Message);
                    //ServiceCallBack.log.Error("ERROR ", ex);
                }

                //I vo vozilo zapisuvam deka se najavil, i resetiram Taksimetar
                myVehicle.DriverShiftInOut = tShiftInOutNew;
                myVehicle.TaximetarLast = System.DateTime.Now; //DateTime.Parse("01.01.0001 00:00:00");
              
                return ("111" + tDriver.Name + " " + tDriver.LastName);
            }
            else
            {
                //I da ima poveke, go zemam posledniot
                GlobSaldo.AVL.Entities.ShiftInOut tShiftInOut = tShiftInOutList[tShiftInOutList.Count - 1];
                if (tShiftInOut.IdDriver == tDriver.IdDriver)
                {
                    if (tShiftInOut.DateTimeOut == null)
                    {
                        //Ako e najaven istiot sto sega saka da se najavi, vrati true, i kazi mu da ne si igra
                        return (tDriver.Name + " " + tDriver.LastName + " (Ne igraj so kartickata !)"); ;
                    }
                    else
                    {
                        //Istiot sto posleden ja vozel kolata saka da se najavi
                        GlobSaldo.AVL.Entities.ShiftInOut tShiftInOutNew = new GlobSaldo.AVL.Entities.ShiftInOut();
                        tShiftInOutNew.IdDriver = tDriver.IdDriver;
                        tShiftInOutNew.IdVehicle = myVehicle.IdVehicle;
                        tShiftInOutNew.DateTimeIn = DateTime.Now;
                        try
                        {
                            GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Insert(tShiftInOutNew);
                            //myPenaltyCreator.Createpenalty(9, myVehicle);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("ERROR Insert ShiftInOutProvider " + ex.Message);
                            //ServiceCallBack.log.Error("ERROR ", ex);

                        }
                        myVehicle.DriverShiftInOut = tShiftInOutNew;
                        myVehicle.TaximetarLast = System.DateTime.Now; //DateTime.Parse("01.01.0001 00:00:00"); //Reseriram taksimetar                        

                        return ("111" + tDriver.Name + " " + tDriver.LastName);
                    }
                }
                else
                {

                    //PAZI, ako prethodniot e odjaven korektno, netreba da se odjavuva
                    if (tShiftInOut.DateTimeOut == null)
                    {
                        //Ako e nekoj drug, odjavi go stariot (pisi kazna) i najavi go Toj koj sega dava RfIdCard. Vrati True
                        tShiftInOut.DateTimeOut = DateTime.Now;
                        tShiftInOut.RegularShiftOut = false;

                        try
                        {
                            GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Update(tShiftInOut);
                        }
                        catch (Exception ex)
                        {
                            Console.Write("ERROR Update ShiftInOutProvider " + ex.Message);
                        }
                    }

                    //Sega najava na nov
                    GlobSaldo.AVL.Entities.ShiftInOut tShiftInOutNew = new GlobSaldo.AVL.Entities.ShiftInOut();
                    tShiftInOutNew.IdDriver = tDriver.IdDriver;
                    tShiftInOutNew.IdVehicle = myVehicle.IdVehicle;
                    tShiftInOutNew.DateTimeIn = DateTime.Now;
                    try
                    {
                        GlobSaldo.AVL.Data.DataRepository.ShiftInOutProvider.Insert(tShiftInOutNew);
                        //myPenaltyCreator.Createpenalty(9, myVehicle);
                    }
                    catch (Exception ex)
                    {
                        Console.Write("ERROR Insert ShiftInOutProvider " + ex.Message);                        
                    }
                    myVehicle.DriverShiftInOut = tShiftInOutNew;
                    myVehicle.TaximetarLast = System.DateTime.Now; //DateTime.Parse("01.01.0001 00:00:00"); //Reseriram taksimetar
                   

                    return ("111" + tDriver.Name + " " + tDriver.LastName);
                }
            }

        }

        public string ProcessClient(string RfIdCard)
        {

            // Zoran: Kako da doaga ovde if koga RfIdCard = '0000000000'
            if (RfIdCard == "0000000000") return "";


            TList<GlobSaldo.AVL.Entities.RfIdCards> tRfIdCardList = GlobSaldo.AVL.Data.DataRepository.RfIdCardsProvider.GetBySerialNumberRfIdCard(RfIdCard);


            if (tRfIdCardList.Count == 0) return "Nevalidna karticka";


            GlobSaldo.AVL.Entities.RfIdCards tRfIdCard = tRfIdCardList[0];


            //Proveruvam dali za klienti ima takva karticka
            //Ovaa vraka lista na site zapisi, pa si gi proveruvam, treba edna na server
            TList<GlobSaldo.AVL.Entities.RfIdCardPerClients> tRfIdCardPerClientsList = GlobSaldo.AVL.Data.DataRepository.RfIdCardPerClientsProvider.GetByIdRfIdCard(tRfIdCard.IdRfIdCard);

            foreach (GlobSaldo.AVL.Entities.RfIdCardPerClients tRfIdCardPerClients in tRfIdCardPerClientsList)
            {
                if (tRfIdCardPerClients.DateFrom < DateTime.Now && tRfIdCardPerClients.DateTo > DateTime.Now)
                {
                    //OK, validen zapis. Sega go vrakam imeto na klientot
                    try
                    {
                        return GlobSaldo.AVL.Data.DataRepository.ClientsProvider.GetByIdClient(tRfIdCardPerClients.IdClient).Name;
                    }
                    catch (Exception)
                    {
                        return "Problem so procesiranje na karticka!";               // Ova ne e logicno (da ne go najde klientot, no posigurno e vaka!
                    }
                }
                else
                    return "Kartickata ne e validna !";
            }


            return "";

        }
    }
}
