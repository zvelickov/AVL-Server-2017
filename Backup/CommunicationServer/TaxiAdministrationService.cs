using System;
using System.Collections.Generic;
using System.Text;

////using JP.Data.Utils;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Data.Bases;
using GlobSaldo.AVL.Entities;

using Taxi.Communication.Server.Utils.Containers;

namespace Taxi.Communication.Server
{
    public class TaxiAdministrationService : AdministrationService, IAdministration
    {
        

        #region Users

        public TList<Users> getAllUsersByCompany(Company pIdCompany)
        {
            TList<Users> retVal = new TList<Users>();

            try
            {
                TList<Users> tmpRetVal = DataRepository.UsersProvider.GetByIdCompany(pIdCompany.IdCompany);


                if (tmpRetVal != null && tmpRetVal.Count > 0)
                    foreach (Users tmpUser in tmpRetVal)
                        if (tmpUser.IsDeleted != true)
                            retVal.Add(tmpUser);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllUsersByCompany(Company pIdCompany)" + ex.Message);
            }

            if (retVal != null && retVal.Count == 0)
                retVal = null;

            return retVal;
        }


        public long InsertUser(Users pUser)
        {
            long retVal = -1;

            try
            {
                DataRepository.UsersProvider.Insert(pUser);
                retVal = pUser.IdUser;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertUser" + ex.Message);
            }

            return retVal;
        }


        public long UpdateUser(Users pUser)
        {
            long retVal = -1;

            try
            {
                if(DataRepository.UsersProvider.Update(pUser))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateUser" + ex.Message);
            }

            return retVal;
        }


        public long DeleteUser(Users pUser)
        {
            long retVal = -1;

            try
            {
                pUser.IsDeleted = true;
                pUser.IsActive = false;

                if (DataRepository.UsersProvider.Update(pUser))
                    retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteUser" + ex.Message);
            }

            return retVal;
        }


#endregion

        #region Drivers

        public TList<Driver> getAllDriversByCompany(Company pCompany)
        {
            TList<Driver> retVal = new TList<Driver>();
            TList<Driver> tmpRetVal = null;

            try
            {
                tmpRetVal = DataRepository.DriverProvider.GetByIdCompany(pCompany.IdCompany);

                if (tmpRetVal != null)
                {
                    // ZORAN:   Ima problem sto IDRfCard ne e definirano kako foreign key.
                    //          Ova treba da e napraveno kako vo komentiraniot del

                    //Type[] tl = new Type[1];
                    //tl[0] = typeof(RfIdCards);

                    //DataRepository.DriverProvider.DeepLoad(tmpRetVal, true, DeepLoadType.IncludeChildren, tl);

                    // ZORAN:   Ovoj del treba da se trgne koga ke se sredi gornoto
                    // ___________________________________________________________

                    foreach (Driver drv in tmpRetVal)
                    {
                        if (drv.IdRfIdCard != null)
                        {
                            RfIdCards tmpRfIdCard = DataRepository.RfIdCardsProvider.GetByIdRfIdCard((long)drv.IdRfIdCard);

                            ////if (tmpRfIdCard != null)
                            ////    drv.RfIdCardDescription = tmpRfIdCard.Description.Trim();
                        }
                    }

                    // ___________________________________________________________

                    foreach (Driver tmpDriver in tmpRetVal)
                        if (tmpDriver.IsDeleted == false)
                            retVal.Add(tmpDriver);
                }
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllDriversByCompany(Company pCompany)" + ex.Message);
            }

            if (retVal.Count == 0)
                retVal = null;

            return retVal;
        }


        public TList<Driver> getAllDriversByCompanyWithoutRfIdCard (Company pCompany)
        {
            TList<Driver> retVal = new TList<Driver>();
            TList<Driver> tmpRetVal = null;

            try
            {
                tmpRetVal = DataRepository.DriverProvider.GetByIdCompany(pCompany.IdCompany);

                if (tmpRetVal != null)
                    foreach (Driver tmpDriver in tmpRetVal)
                        if ((tmpDriver.IdRfIdCard == null) && (tmpDriver.IsDeleted == false))
                            retVal.Add(tmpDriver);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllDriversByCompanyWithoutRfIdCard (Company pCompany)" + ex.Message);
            }

            if (retVal.Count == 0)
                retVal = null;

            return retVal;
        }


        public TList<Driver> getAllDriversByCompanyWithRfIdCard(Company pCompany)
        {
            TList<Driver> retVal = new TList<Driver>();
            TList<Driver> tmpRetVal = null;

            try
            {
                tmpRetVal = DataRepository.DriverProvider.GetByIdCompany(pCompany.IdCompany);


               
                if (tmpRetVal != null)
                {
                    // ZORAN:   Ima problem sto IDRfCard ne e definirano kako foreign key.
                    //          Ova treba da e napraveno kako vo komentiraniot del

                    //Type[] tl = new Type[1];
                    //tl[0] = typeof(RfIdCards);

                    //DataRepository.DriverProvider.DeepLoad(tmpRetVal, true, DeepLoadType.IncludeChildren, tl);

                    // ZORAN:   Ovoj del treba da se trgne koga ke se sredi gornoto
                    // ___________________________________________________________

                    foreach (Driver drv in tmpRetVal)
                    {
                        if (drv.IdRfIdCard != null)
                        {
                            RfIdCards tmpRfIdCard = DataRepository.RfIdCardsProvider.GetByIdRfIdCard((long)drv.IdRfIdCard);

                            ////if (tmpRfIdCard != null)
                            ////    drv.RfIdCardDescription = tmpRfIdCard.Description.Trim();
                        }
                    }

                    // ___________________________________________________________

                    foreach (Driver tmpDriver in tmpRetVal)
                        if ((tmpDriver.IdRfIdCard != null) && (tmpDriver.IsDeleted == false))
                            retVal.Add(tmpDriver);
                }
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllDriversByCompanyWithRfIdCard(Company pCompany)" + ex.Message);
            }

            if (retVal.Count == 0)
                retVal = null;

            return retVal;
        }



        public long InsertDriver(Driver pDriver)
        {
            long retVal = -1;

            try
            {
                DataRepository.DriverProvider.Insert(pDriver);
                retVal = pDriver.IdDriver;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertDriver(Driver pDriver)" + ex.Message);
            }

            return retVal;

        }


        public long UpdateDriver(Driver pDriver)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.DriverProvider.Update(pDriver))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateDriver(Driver pDriver)" + ex.Message);
            }

            return retVal;
        }


        public long DeleteDriver(Driver pDriver)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.DriverProvider.Delete(pDriver))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteDriver(Driver pDriver)" + ex.Message);
            }

            return retVal;

        }



        #endregion

        #region Clients


        public TList<Clients> getAllClientsByCompany(Company pIdCompany)
         {
             TList<Clients> retVal = new TList<Clients>();

            try
            {
                TList<Clients> tmpRetVal = DataRepository.ClientsProvider.GetByIdCompany(pIdCompany.IdCompany);

                if (tmpRetVal != null && tmpRetVal.Count > 0)
                    foreach (Clients tmpClient in tmpRetVal)
                        if (tmpClient.IsDeleted == false)
                            retVal.Add(tmpClient);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllClientsByCompany(Company pIdCompany)" + ex.Message);
            }

            if (retVal != null && retVal.Count == 0)
                retVal = null;

            return retVal;
        }


        public TList<Clients> getAllClientsByName(string Name, Company pIdCompany)
        {
            TList<Clients> retVal = null;

            StringBuilder strBuild = new StringBuilder();

            strBuild.Append("Name LIKE N'%");
            strBuild.Append(Name);
            strBuild.Append("%' AND ID_Company = ");
            strBuild.Append(pIdCompany.IdCompany.ToString());   

            try
            {
                int i;
                retVal = DataRepository.ClientsProvider.GetPaged(strBuild.ToString(), "Name", 0, 100000, out i);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllClientsByCompany(Company pIdCompany)" + ex.Message);
            }

            return retVal;
        }

        public long InsertClient(Clients pClient)
        {
            long retVal = -1;

            try
            {
                DataRepository.ClientsProvider.Insert(pClient);
                retVal = pClient.IdClient;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertClient" + ex.Message);
            }

            return retVal;
        }

        public long UpdateClient(Clients pClient)
         {
            long retVal = -1;

            try
            {
                if (DataRepository.ClientsProvider.Update(pClient))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateClient" + ex.Message);
            }

            return retVal;
        }

        public long DeleteClient(Clients pClient)
        {
            long retVal = -1;

            try
            {
                pClient.IsDeleted = true;

                DataRepository.ClientsProvider.Update(pClient);
                    
                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteClient" + ex.Message);
            }

            return retVal;
        }


#endregion


        #region PenaltyPerDriverPerPeriod

        public PenaltyPerDriverPerPeriod getPenaltyPerDriverPerPeriod(long pIdDriver)
        {
            PenaltyPerDriverPerPeriod retVal = null;
            TList<PenaltyPerDriverPerPeriod> tmpRetVal = null;

            try
            {
                tmpRetVal = DataRepository.PenaltyPerDriverPerPeriodProvider.GetByIdDriver (pIdDriver);

                if (tmpRetVal != null && tmpRetVal.Count > 0)
                {
                    foreach (PenaltyPerDriverPerPeriod tmpPpDpP in tmpRetVal)
                    {
                        if (retVal == null)
                            retVal = tmpPpDpP;
                        else
                            if (retVal.PenaltyDateTimeTo < tmpPpDpP.PenaltyDateTimeTo)
                                retVal = tmpPpDpP;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertPenaltyPerDriverPerPeriod: " + ex.Message);
            }

            return retVal;
        }


        public long InsertPenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriod)
        {
            long retVal = -1;

            try
            {
                DataRepository.PenaltyPerDriverPerPeriodProvider.Insert(pPenaltyPerDriverPerPeriod);
                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("InsertPenaltyPerDriverPerPeriod: " + ex.Message);
            }

            return retVal;
        }

        public long UpdatePenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriod)
        {
            long retVal = -1;

            try
            {
                DataRepository.PenaltyPerDriverPerPeriodProvider.Update(pPenaltyPerDriverPerPeriod);
                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("UpdatePenaltyPerDriverPerPeriod: " + ex.Message);
            }

            return retVal;
        }

        public long DeletePenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriod)
        {
            long retVal = -1;

            try
            {
                DataRepository.PenaltyPerDriverPerPeriodProvider.Delete(pPenaltyPerDriverPerPeriod);
                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("DeletePenaltyPerDriverPerPeriod: " + ex.Message);
            }
            
            return retVal;
        }

        #endregion


        #region FiskalReceipt

        public TList<FiskalReceipt> getAllFiskalReceiptByVehicleForPeriod(Vehicle vehicle, DateTime dateFrom, DateTime dateTo)
        {
            TList<FiskalReceipt> retVal = null;

            string _from = dateFrom.ToString("yyyy-MM-dd");
            string _to = dateTo.ToString("yyyy-MM-dd");

            StringBuilder strBuild = new StringBuilder();

            strBuild.Append(" ID_Vehicle = ");
            strBuild.Append(vehicle.IdVehicle.ToString());
            strBuild.Append(" AND SystemDate BETWEEN '");
            strBuild.Append(_from);
            strBuild.Append("' AND '");
            strBuild.Append(_to);
            strBuild.Append("'");           

            try
            {
                int i;
                retVal = DataRepository.FiskalReceiptProvider.GetPaged(strBuild.ToString(), "SystemDate", 0, 100000, out i);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllClientsByCompany(Company pIdCompany)" + ex.Message);
            }

            return retVal;
        }

        public long UpdateFiskalReceipt(FiskalReceipt pObject)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.FiskalReceiptProvider.Update(pObject))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateFiscal" + ex.Message);
            }

            return retVal;
        }

        #endregion

        #region Companies


        public TList<Company> getAllCompanies()
        {
            TList<Company> retVal = null;

            try
            {
                retVal = DataRepository.CompanyProvider.GetAll();
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllCompanies()" + ex.Message);
            }

            return retVal;
        }


        public Company GetCompanyByID(long pCompany)
        {
            Company retVal = null;

            try
            {
                retVal = DataRepository.CompanyProvider.GetByIdCompany(pCompany);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo GetCompanyByID()" + ex.Message);
            }

            return retVal;
        }


        public long InsertCompany(Company pCompany)
        {
            long retVal = -1;

            try
            {
                DataRepository.CompanyProvider.Insert(pCompany);
                retVal = pCompany.IdCompany;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertUser" + ex.Message);
            }

            return retVal;
        }


        public long UpdateCompany(Company pCompany)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.CompanyProvider.Update(pCompany))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateUser" + ex.Message);
            }

            return retVal;
        }


        public long DeleteCompany(Company pCompany)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.CompanyProvider.Delete(pCompany))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteUser" + ex.Message);
            }

            return retVal;
        }


        #endregion


        #region Vehicles


        public TList<Vehicle> getAllVehiclesByCompany(Company pIdCompany)
        {
            TList<Vehicle> retVal = null;
            

            try
            {
                retVal = DataRepository.VehicleProvider.GetByIdCompany(pIdCompany.IdCompany);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllUsersByCompany(Company pIdCompany)" + ex.Message);
            }

            return retVal;
        }



        


        public long InsertVehicle(Vehicle pVehicle)
        {
            long retVal = -1;

            try
            {
                DataRepository.VehicleProvider.Insert(pVehicle);
                retVal = pVehicle.IdCompany;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertUser" + ex.Message);
            }

            return retVal;
        }


        public long UpdateVehicle(Vehicle pVehicle)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.VehicleProvider.Update(pVehicle))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateUser" + ex.Message);
            }

            return retVal;
        }


        public long DeleteVehicle(Vehicle pVehicle)
        {
            long retVal = -1;

            try
            {
                if (pVehicle.IdUnit != null)
                {
                    pVehicle.IdUnit = null;

                    DataRepository.VehicleProvider.Update(pVehicle);
                }

                DataRepository.VehicleProvider.Delete(pVehicle);

                retVal = 1;
                
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteVehicle" + ex.Message);
            }

            return retVal;
        }


        
        public long AttachUnitOnVehicle(Unit pUnit, Vehicle pVehicle, string pComment)
        {
            long retVal = -1;

            try
            {
                UnitHistory tmpUnitHistory = new UnitHistory();

                tmpUnitHistory.IdUnit = pUnit.IdUnit;
                tmpUnitHistory.IdVehicle = pVehicle.IdVehicle;
                tmpUnitHistory.InstalationDate = System.DateTime.Now;
                tmpUnitHistory.Description = "Montaza: " + pComment;

                DataRepository.UnitHistoryProvider.Insert(tmpUnitHistory);



                pVehicle.IdUnit = pUnit.IdUnit;

                DataRepository.VehicleProvider.Update(pVehicle);

                retVal = 1;

            }
            catch (Exception ex)
            {
                log.Error("Greska vo AttachUnitOnVehicle(Unit pUnit, Vehicle pVehicle, string pComment)" + ex.Message);
            }

            return retVal;
        }





        public long DetachUnitFromVehicle(Unit pUnit, Vehicle pVehicle, string pComment)
        {
            long retVal = -1;
            TList<UnitHistory> tmpUnitHistory = null;

            try
            {
                pVehicle.IdUnit = null;

                DataRepository.VehicleProvider.Update(pVehicle);

                tmpUnitHistory = DataRepository.UnitHistoryProvider.GetByIdUnit(pUnit.IdUnit);

                if (tmpUnitHistory != null)
                {
                    foreach (UnitHistory tmpUH in tmpUnitHistory)
                        if ((tmpUH.IdVehicle == pVehicle.IdVehicle) && (tmpUH.UninstallationDate == null))
                        {
                            tmpUH.UninstallationDate = System.DateTime.Now;
                            tmpUH.Description += " Demontaza: " + pComment;

                            DataRepository.UnitHistoryProvider.Update(tmpUH);

                            retVal = 1;
                        }
                }
                else
                    retVal = -1;

            }
            catch (Exception ex)
            {
                log.Error("Greska vo DetachUnitFromVehicle(Unit pUnit, Vehicle pVehicle, string pComment)" + ex.Message);
            }

            return retVal;
        }


#endregion


        #region Units


        public TList<Unit> getAllUnitsByCompany(Company pCompany)
        {
            TList<Unit> retVal = null;

            try
            {
                retVal = DataRepository.UnitProvider.GetByIdCompany(pCompany.IdCompany);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllUsersByCompany(Company pIdCompany)" + ex.Message);
            }

            return retVal;
        }


        public long InsertUnit(Unit pUnit)
        {
            long retVal = -1;

            try
            {
                DataRepository.UnitProvider.Insert(pUnit);
                retVal = pUnit.IdUnit;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertUser" + ex.Message);
            }

            return retVal;
        }


        public long UpdateUnit(Unit pUnit)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.UnitProvider.Update(pUnit))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateUser" + ex.Message);
            }

            return retVal;
        }


        public long DeleteUnit(Unit pUnit)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.UnitProvider.Delete(pUnit))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteUser" + ex.Message);
            }

            return retVal;
        }


        public TList<UnitHistory> GetUnitHistory(Unit pUnit)
        {
            TList<UnitHistory> retVal = null;

            try
            {
                retVal = DataRepository.UnitHistoryProvider.GetByIdUnit(pUnit.IdUnit);
            }

            catch (Exception ex)
            {
                log.Error("Greska vo GetUnitHistory(Unit pUnit)" + ex.Message);
            }

            return retVal;

        }


        #endregion


        #region RfIdCardPerClients

        public long saveRegisteredPassengers(RegisteredPassengers pRegisteredPassengers)
        {
            long retVal = -1;

            try
            {
                
                if (DataRepository.RegisteredPassengersProvider.Insert(pRegisteredPassengers ))
                {
                    RfIdCards tmpRfIdCard = DataRepository.RfIdCardsProvider.GetByIdRfIdCard((long)pRegisteredPassengers.IdRfIdCard);

                    if (tmpRfIdCard != null)
                    {
                        tmpRfIdCard.Busy = true;

                        DataRepository.RfIdCardsProvider.Update(tmpRfIdCard);

                        retVal = pRegisteredPassengers.IdRegisteredPassengers;
                    }
                }
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
            }

            return retVal;

        }

        public long deleteRegisteredPassengers(RegisteredPassengers pRegisteredPassengers)
        {
            long retVal = -1;

            try
            {
                // Ne briseme, samo stavame IsDeleted = true

                pRegisteredPassengers.IsDeleted = true;

                DataRepository.RegisteredPassengersProvider.Update(pRegisteredPassengers);


                //Sega da ja oznacam deka kartickata e slobodna

                RfIdCards tmpRfIdCard = DataRepository.RfIdCardsProvider.GetByIdRfIdCard((long)pRegisteredPassengers.IdRfIdCard);

                if (tmpRfIdCard != null)
                {
                    tmpRfIdCard.Busy = false;

                    DataRepository.RfIdCardsProvider.Update(tmpRfIdCard);

                    retVal = 0;
                }
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
            }

            return retVal;
        }

        public long updateRegisteredPassengers(RegisteredPassengers pRegisteredPassengers)
        {
            try
            {
                if (DataRepository.RegisteredPassengersProvider.Update(pRegisteredPassengers))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                return -1;
            }
        }

        public TList<RegisteredPassengers> getAllRegisteredPassengersPerClient(Clients pClient)
        {
            TList<RegisteredPassengers> tmpRegisteredPassengers;
            TList<RegisteredPassengers> retVal = new TList<RegisteredPassengers>();

            try
            {
                tmpRegisteredPassengers = DataRepository.RegisteredPassengersProvider.GetByIdClient(pClient.IdClient);

                Type[] tl = new Type[2];
                tl[0] = typeof(Clients);
                tl[1] = typeof(RfIdCards);

                DataRepository.RegisteredPassengersProvider.DeepLoad(tmpRegisteredPassengers, true, DeepLoadType.IncludeChildren, tl);

                if (tmpRegisteredPassengers != null)
                    foreach (RegisteredPassengers tmpRegisteredPassengersPerClient in tmpRegisteredPassengers)
                        if (tmpRegisteredPassengersPerClient.IsDeleted == false)
                            retVal.Add(tmpRegisteredPassengersPerClient);

                if (retVal.Count == 0)
                    retVal = null;
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }

        
        

        public TList<RegisteredPassengers> getAllRegisteredPassengersPerPhisicalEntity(Company pCompany)
        {
            TList<RegisteredPassengers> tmpRegisteredPassengers;
            TList<RegisteredPassengers> retVal = new TList<RegisteredPassengers>();

            try
            {
                tmpRegisteredPassengers = DataRepository.RegisteredPassengersProvider.GetByIdCompany(pCompany.IdCompany); 

                Type[] tl = new Type[1];
                tl[0] = typeof(RfIdCards);

                DataRepository.RegisteredPassengersProvider.DeepLoad(tmpRegisteredPassengers, true, DeepLoadType.IncludeChildren, tl);

                if (tmpRegisteredPassengers != null)
                    foreach (RegisteredPassengers tmpRegisteredPassengersPerClient in tmpRegisteredPassengers)
                        if (tmpRegisteredPassengersPerClient.IsDeleted == false && tmpRegisteredPassengersPerClient.IdClient == null)
                            retVal.Add(tmpRegisteredPassengersPerClient);

                if (retVal.Count == 0)
                    retVal = null;
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }


        public TList<RegisteredPassengers> getAllRegisteredPassengersPerPhisicalEntityFiltered(Company pCompany, string pFilter)
        {
            TList<RegisteredPassengers> tmpRegisteredPassengers;
            TList<RegisteredPassengers> retVal = new TList<RegisteredPassengers>();

            try
            {
                tmpRegisteredPassengers = DataRepository.RegisteredPassengersProvider.GetByIdCompany(pCompany.IdCompany); 

                ////RegisteredPassengersParameterBuilder pbuilder = new RegisteredPassengersParameterBuilder();

                ////pbuilder. AppendIn(RegisteredPassengersColumn.Name, pFilterString);

                ////tmpRegisteredPassengers  = DataRepository.RegisteredPassengersProvider.Find(pbuilder.GetParameters());

                //tmpRegisteredPassengers = DataRepository.RegisteredPassengersProvider.Find( (pFilterString);

                Type[] tl = new Type[1];
                tl[0] = typeof(RfIdCards);

                DataRepository.RegisteredPassengersProvider.DeepLoad(tmpRegisteredPassengers, true, DeepLoadType.IncludeChildren, tl);

                if (tmpRegisteredPassengers != null)
                    foreach (RegisteredPassengers tmpRegisteredPassengersPerPhisicalEntity in tmpRegisteredPassengers)
                        if (tmpRegisteredPassengersPerPhisicalEntity.IsDeleted == false
                                && tmpRegisteredPassengersPerPhisicalEntity.IdClient == null
                                && (tmpRegisteredPassengersPerPhisicalEntity.Name.ToUpper().Contains(pFilter.ToUpper()) || tmpRegisteredPassengersPerPhisicalEntity.LastName.ToUpper().Contains(pFilter.ToUpper()))
                                
                            )
                            retVal.Add(tmpRegisteredPassengersPerPhisicalEntity);

                if (retVal.Count == 0)
                    retVal = null;
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }


        public TList<RfIdCards> getAllRfIdCardPerCompany(Company pCompany)
        {

            TList<RfIdCards> tmpRfIdCards;
            TList<RfIdCards> retVal = new TList<RfIdCards>();

            try
            {
                tmpRfIdCards = DataRepository.RfIdCardsProvider.GetByIdCompany(pCompany.IdCompany);

                if (tmpRfIdCards != null)
                    foreach (RfIdCards tmpRfIdCard in tmpRfIdCards)
                        if (tmpRfIdCard.IsDeleted == false )
                            retVal.Add(tmpRfIdCard);

                if (retVal.Count == 0)
                    retVal = null;
                
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }


        public TList<RfIdCards> getAllFreeRfIdCardPerCompany(Company pCompany)
        {
            TList<RfIdCards> tmpRfIdCards;
            TList<RfIdCards> retVal = new TList<RfIdCards>();

            try
            {
                tmpRfIdCards = DataRepository.RfIdCardsProvider.GetByIdCompany(pCompany.IdCompany);

                if (tmpRfIdCards != null)
                    foreach (RfIdCards tmpRfIdCard in tmpRfIdCards)
                        if ((tmpRfIdCard.IsDeleted == false) && (tmpRfIdCard.Busy == false))
                            retVal.Add(tmpRfIdCard);

                if (retVal.Count == 0)
                    retVal = null;

            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }



        public TList<RfIdCards> getAllBusyRfIdCardPerCompany(Company pCompany)
        {
            TList<RfIdCards> tmpRfIdCards;
            TList<RfIdCards> retVal = new TList<RfIdCards>();

            try
            {
                tmpRfIdCards = DataRepository.RfIdCardsProvider.GetByIdCompany(pCompany.IdCompany);

                if (tmpRfIdCards != null)
                    foreach (RfIdCards tmpRfIdCard in tmpRfIdCards)
                        if ((tmpRfIdCard.IsDeleted == false) && (tmpRfIdCard.Busy == true))
                            retVal.Add(tmpRfIdCard);

                if (retVal.Count == 0)
                    retVal = null;

            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                retVal = null;
            }

            return retVal;
        }



        public RfIdCardPerClients getRfIdCardPerClients(long ID_RfIdCardPerClients)
        {
            try
            {
                RfIdCardPerClients retVal;

                retVal = DataRepository.RfIdCardPerClientsProvider.GetByIdRfIdCardPerClients(ID_RfIdCardPerClients);

                Type[] tl = new Type[2];
                tl[0] = typeof(Clients);
                tl[1] = typeof(RfIdCards);

                DataRepository.RfIdCardPerClientsProvider.DeepLoad(retVal, true, DeepLoadType.IncludeChildren, tl);

                return retVal;
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
                return null;
            }
        }


        public RfIdCards getRfIdCardByDriver(Driver pDriver)
        {
            RfIdCards retVal = null;

            try
            {
                if (pDriver.IdRfIdCard != null)
                    retVal = DataRepository.RfIdCardsProvider.GetByIdRfIdCard((long)pDriver.IdRfIdCard);
            }
            catch (Exception e)
            {
                log.Error("ERROR", e);
            }
            return retVal;

        }



        public long InsertRfIdCardHistory(RfIdCards pRfIdCard, string pComment)
        {
            long retVal = -1;

            RfIdCardHistory tmpRfIdCardHistory = new RfIdCardHistory();

            tmpRfIdCardHistory.IdRfIdCard = pRfIdCard.IdRfIdCard;
            tmpRfIdCardHistory.SerialNumber = pRfIdCard.SerialNumber;
            tmpRfIdCardHistory.Date = System.DateTime.Now;
            tmpRfIdCardHistory.Description = pComment;

            try
            {
                DataRepository.RfIdCardHistoryProvider.Insert(tmpRfIdCardHistory);
                retVal = tmpRfIdCardHistory.IdRfIdCardHistory ;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertRfIdCardHistory" + ex.Message);
            }

            return retVal;
        }


        public long InsertRfIdCard(RfIdCards pRfIdCard)
        {
            long retVal = -1;

            try
            {
                DataRepository.RfIdCardsProvider.Insert(pRfIdCard);
                retVal = pRfIdCard.IdRfIdCard;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo InsertRfIdCard" + ex.Message);
            }

            return retVal;

        }
       
        public long UpdateRfIdCard(RfIdCards pRfIdCard)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.RfIdCardsProvider.Update(pRfIdCard))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateRfIdCard" + ex.Message);
            }

            return retVal;
        }

        public long DeleteRfIdCard(RfIdCards pRfIdCard)
        {
            long retVal = -1;

            try
            {
                pRfIdCard.IsDeleted = true;

                DataRepository.RfIdCardsProvider.Update(pRfIdCard);

                //ZORAN:    Ova mi e naknadno staveno, da izbrise SE zivo!!!

                DataRepository.RfIdCardsProvider.DeleteEdnaKarticka(pRfIdCard.Description);

                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo DeleteRfIdCard" + ex.Message);
            }

            return retVal;

        }

        #endregion



        #region  ClientPhoneNumbers

        public long saveClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers)
        {
            long retVal = -1;

            try
            {
                DataRepository.ClientPhoneNumbersProvider.Insert(pClientPhoneNumbers);
                retVal = pClientPhoneNumbers.IdClientPhoneNumbers;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo saveClientPhoneNumbers" + ex.Message);
            }

            return retVal;
        }

        public long deleteClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.ClientPhoneNumbersProvider.Delete(pClientPhoneNumbers))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo deleteClientPhoneNumbers" + ex.Message);
            }

            return retVal;
        }

        public long updateClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.ClientPhoneNumbersProvider.Update(pClientPhoneNumbers))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo updateClientPhoneNumbers" + ex.Message);
            }

            return retVal;
        }

        public TList<ClientPhoneNumbers> getAllClientPhoneNumbers(Clients pClient)
        {
            TList<ClientPhoneNumbers> retVal = null;

            try
            {
                retVal = DataRepository.ClientPhoneNumbersProvider.GetByIdClient (pClient.IdClient);
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllClientPhoneNumbers" + ex.Message);
            }

            return retVal;

        }

        #endregion

        #region GisPhoneNumber

        public long updateGisPhoneNumbers(GisPhoneNumbers newPhoneNumber)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.GisPhoneNumbersProvider.Update(newPhoneNumber))
                    return 0;
                else
                    return 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo updateGisPhoneNumbers" + ex.Message);
            }

            return retVal;

        }

        #endregion
    }
}
