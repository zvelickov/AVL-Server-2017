using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ServiceModel;
//using JP.Data.Utils;
using log4net;
using Taxi.Communication.Server.ConnectionListeners;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Data.Bases;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.SqlClient;
using GlobSaldo.AVL.Entities.PartialClasses;
using Taxi.Communication.Server.Containers;
using Taxi.Communication.Server.Utils.Containers;
using Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch;

namespace Taxi.Communication.Server
{
    [ServiceContract()]
    public interface IAdministrationService
    {
       

        #region Users
        [OperationContract]
        TList<Users> getAllUsers(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<Users> getDataByUserNamePassword(string username, string password);
        #endregion

        #region CanceledPhoneCalls
        [OperationContract]
        TList<CanceledPhoneCalls> getAllCanceledPhoneCalls(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        long insertCanceledPhoneCall(CanceledPhoneCalls newCanceledPhoneCall);
        #endregion

        #region Penalties
        [OperationContract]
        TList<Penalties> getAllPenalties(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        long insertPenalties(Penalties newPenatly);
        #endregion

        #region Driver
        [OperationContract]
        Driver getDataByLastAssignedDriver(long ID_Vehicle);

        [OperationContract]
        Driver getDataLastDriverShift(long ID_Vehicle);

        #endregion

        #region Orders
        [OperationContract]
        TList<Orders> getAllOrders(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        long insertOrders(Orders newOrder);
        #endregion

        #region PhoneNumbersBlackList
        [OperationContract]
        TList<PhoneNumbersBlackList> getAllPhoneNumbersBlackList(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<PhoneNumbersBlackList> getDataByPhoneNumber(string phoneNumber);

        [OperationContract]
        long insertPhoneNumbersBlackList(PhoneNumbersBlackList newPhoneBlack);
        #endregion

        #region UserInOut
        [OperationContract]
        TList<UserInOut> getAllUserInOut(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        long UpdateQueryLogOutUser(DateTime dateOut, long ID_User);

        [OperationContract]
        long insertUserInOut(UserInOut newUserInOut);

        [OperationContract]
        long updateUserInOut(UserInOut pUserInOut);

        [OperationContract]
        UserInOut getUserInOutForCurrentExtension(Users pUser, long pExtension);

        #endregion

        #region PhoneNumber
        [OperationContract]
        TList<PhoneNumbers> getAllPhoneNumbers(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<PhoneNumbers> getDataByObjectName(string address);
        #endregion

        #region PhoneCalls

        [OperationContract]
        long updatePhoneCalls(PhoneCalls pPhoneCalls);

        #endregion


        [OperationContract]
        TList<Vehicle> getAllVehiclesByCompanies(string pIdCompanies);

        [OperationContract]
        TList<PhoneCalls> GetAllActivePhoneCalls();


        #region GisStreets

        [OperationContract]
        TList<GisStreets> getAllGisStreets(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<GisStreets> getGisStreetsByName(string name);

        [OperationContract]
        GisStreets GetGisStreetForIdStreet(string IDStreet);

        #endregion

        #region GisRegions
        [OperationContract]
        GisRegions getGisRegionForIdRegion(int IdRegion);

        [OperationContract]
        TList<GisRegions> getAllGisRegions(int startPage, int pageSize, out int maxPages);
        #endregion

        #region GisSearchRegions
        [OperationContract]
        TList<GisSearchRegions> getAllGisSearchRegions(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<GisSearchRegions> getGisSearchRegionsByIdRegion(long ID_Region, int typeAlternative);
        #endregion

        #region GisAddressModel

        [OperationContract]
        TList<GisAddressModel> getAllGisAddressModel(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreet(string ID_Street);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber);

        [OperationContract]
        TList<AddressModelDescription> getAddressModelDescriptionsForIDStreetHouseNumber(string ID_Street, int houseNumber);

        [OperationContract]
        TList<AddressModelDescription> getAddressModelDescriptionsByIDStreet(string ID_Street);

        [OperationContract]
        long saveAddressModelDescriptions(AddressModelDescription addressModelDescription);

        [OperationContract]
        long deleteAddressModelDescriptions(AddressModelDescription addressModelDescription);

        #endregion

        #region GisPhoneNumber
        [OperationContract]
        TList<GisPhoneNumbers> getAllGisPhoneNumbers(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        GisPhoneNumbers getGisPhoneNumbersByNumber(string phoneNumber);

        [OperationContract]
        long deleteGisPhoneNumbersForPhoneNumber(GisPhoneNumbers gisPhoneNumber);

        [OperationContract]
        long insertGisPhoneNumbers(GisPhoneNumbers newPhoneNumber);

        [OperationContract]
        TList<GisPhoneNumbers> getGisPhoneNumbersByIDObject(long IDObject);

        [OperationContract]
        TList<PhoneNumberDescription> getPhoneNumberDescriptionsByGisPhoneNumber(string phoneNumber);

        [OperationContract]
        long savePhoneNumberDescriptions(PhoneNumberDescription phoneNumberDescription);

        [OperationContract]
        long deletePhoneNumberDescriptions(PhoneNumberDescription phoneNumberDescription);


        #endregion

        #region GisObjects

        [OperationContract]
        TList<GisObjects> getAllGisObjectsByName(string name); //-------

        [OperationContract]
        TList<ObjectDescription> getObjectDescriptionsByGisObject(long IDObject); //-------

        [OperationContract]
        long saveObjectDescriptions(ObjectDescription objectDescription); //-------
        
        #endregion

        #region PhoneNumbersSurovi
        /*
        [OperationContract]
        long savePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi);

        [OperationContract]
        long deletePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi);

        [OperationContract]
        long updatePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi);

        [OperationContract]
        TList<PhoneNumbersSurovi> getAllPhoneNumbersSurovi(int pageStart, int pageLength, out int maxPages);

        [OperationContract]
        TList<PhoneNumbersSurovi> getPhoneNumbersSuroviByPhoneNumber(string phoneNumber);

        [OperationContract]
        TList<PhoneNumbersSurovi> getPhoneNumbersSuroviByAddress(string address);
         */
        #endregion

        #region  ClientPhoneNumbers
        [OperationContract]
        Clients searchVipClients(ClientPhoneNumbers clientPhoneNumber);
        #endregion
       
        #region Reservation

        [OperationContract]
        TList<Reservations> getReservationsForContactPhone(string phoneNumber);

        [OperationContract]
        TList<Reservations> getReservationsForPeriod(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        int deleteReservationList(TList<Reservations> objReservations);

        [OperationContract]
        int deleteReservationContactPhonePeriod(string phoneNumber, DateTime dateFrom, DateTime dateTo);

        #endregion

        #region FiskalReceipt

        [OperationContract]
        TList<FiskalReceipt> getFiskalReceiptForVehicle(long iDVehicle, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        TList<FiskalReceipt> getFiskalReceiptForPeriod(DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        TList<FiskalReceipt> getFiskalReceiptForDriver(long iDDriver, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        TList<FiskalReceipt> getFiskalReceiptForRfIdCard(string rfIdCard, DateTime dateFrom, DateTime dateTo);

        [OperationContract]
        long saveFiskalReceipt(FiskalReceipt objFiskalReceipt);

        [OperationContract]
        long deleteFiskalReceipt(FiskalReceipt objFiskalReceipt);

        #endregion

        #region RfIDCards

        [OperationContract]
        RegisteredPassengers getRegisteredPassingerForCard(string description);

        [OperationContract]
        Driver getDriverForCard(string description);

        #endregion

        #region SearchVechicles Ror reaon

        [OperationContract]
        TList<Vehicle> searchAllVechiclesByIDReaon(long ID_Region, int typeAlternative);

        [OperationContract]
        bool IsVehicleEligibleForCall(Vehicle pVehicle, PhoneCalls phoneCall);

        [OperationContract]
        TList<Vehicle> GetAllAvailableVehiclesForPhoneCall(PhoneCalls phoneCall);

        [OperationContract]
        TList<Vehicle> GetAllAvailableVehiclesForLocation(decimal? latitude, decimal? longitude, long id_Region);

        #endregion


        #region MobileReservations

        [OperationContract]
        TList<MobileReservations> getAllActiveMobileReservations(long IdUser);

        [OperationContract]
        long UpdateMobileReservation(long pIdReservation, long pIdUser);

        #endregion

    }




    public class AdministrationService : IAdministrationService
    {
        public static readonly ILog log = log4net.LogManager.GetLogger("MyService");

        #region IAdministrationService Members
        


        #region Locations

        public TList<Locations> getLocationsByVehicleAndPeriod(long pIdVehicle, DateTime pDateFrom, DateTime pDateTo)
        {
            TList<Locations> retVal = null;

            try
            {             
                retVal = DataRepository.LocationsProvider.GetLocationByIDVehicleForInterval (pIdVehicle, pDateFrom, pDateTo);                
            }
            catch (Exception e)
            {
                log.Error("Error vo getLocationsByVehicleAndPeriod", e);
            }

            return retVal;
            //return new TList<Locations>();
        }

        #endregion

        #region Users
        public TList<Users> getAllUsers(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.UsersProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<Users> getDataByUserNamePassword(string username, string password)
        {
            try
            {
                return DataRepository.UsersProvider.getByUserNamePassword(username, password);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        #endregion

        #region CanceledPhoneCalls
        public TList<CanceledPhoneCalls> getAllCanceledPhoneCalls(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.CanceledPhoneCallsProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public long insertCanceledPhoneCall(CanceledPhoneCalls newCanceledPhoneCall)
        {
            try
            {
                if (DataRepository.CanceledPhoneCallsProvider.Insert(newCanceledPhoneCall))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }
        #endregion

        #region Penatlties
        public TList<Penalties> getAllPenalties(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.PenaltiesProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public long insertPenalties(Penalties newPenatly)
        {
            try
            {
                if (DataRepository.PenaltiesProvider.Insert(newPenatly))
                    return newPenatly.IdPenalty;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }
        #endregion

        #region Driver
        public Driver getDataByLastAssignedDriver(long ID_Vehicle)
        {
            try
            {
                return null;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public Driver getDataLastDriverShift(long ID_Vehicle)
        {
            Driver retVal = null;


            try
            {
                TList<ShiftInOut> tShiftInOut = DataRepository.ShiftInOutProvider.GetByIDVehicleAndDateTimeNull (ID_Vehicle);
                if (tShiftInOut != null && tShiftInOut.Count > 0)
                {
                    ShiftInOut tShift = tShiftInOut[tShiftInOut.Count - 1];
                    retVal = DataRepository.DriverProvider.GetByIdDriver(tShift.IdDriver);
                }

            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }


            return retVal;
        }

        #endregion

        #region Orders
        public TList<Orders> getAllOrders(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.OrdersProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public long insertOrders(Orders newOrder)
        {
            try
            {
                if (DataRepository.OrdersProvider.Insert(newOrder))
                    return newOrder.IdOrder;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }
        #endregion

        #region PhoneNumbersBlackList
        public TList<PhoneNumbersBlackList> getAllPhoneNumbersBlackList(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.PhoneNumbersBlackListProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<PhoneNumbersBlackList> getDataByPhoneNumber(string phoneNumber)
        {
            try
            {
                return DataRepository.PhoneNumbersBlackListProvider.getByPhoneNumber(phoneNumber);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public long insertPhoneNumbersBlackList(PhoneNumbersBlackList newPhoneBlack)
        {
            try
            {
                if (DataRepository.PhoneNumbersBlackListProvider.Insert(newPhoneBlack))
                    return newPhoneBlack.IdBlackList;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }
        #endregion

        #region UserInOut
        public TList<UserInOut> getAllUserInOut(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.UserInOutProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("getAllUserInOut Error", e);
                return null;
            }
        }

        public long UpdateQueryLogOutUser(DateTime dateOut, long ID_User)
        {
            try
            {
                DataRepository.UserInOutProvider.UpdateQueryLogOutUser(dateOut, ID_User);
                return 0;
            }
            catch (Exception e)
            {
                log.Error("UpdateQueryLogOutUser Error", e);
                return -1;
            }
        }

        public long insertUserInOut(UserInOut newUserInOut)
        {
            try
            {
                DataRepository.UserInOutProvider.CloseAllOpenLoginsForExtension(newUserInOut.PhoneExtension);

                if (DataRepository.UserInOutProvider.Insert(newUserInOut))
                    return newUserInOut.IdUserLogInOut;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("insertUserInOut Error", e);
                return -1;
            }
        }



        public long updateUserInOut(UserInOut pUserInOut)
        {
            try
            {
                if (DataRepository.UserInOutProvider.Update(pUserInOut))
                    return pUserInOut.IdUserLogInOut;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("updateUserInOut Error", e);
                return -1;
            }
        }


        public UserInOut getUserInOutForCurrentExtension(Users pUser, long pExtension)
        {
            UserInOut retVal = null;
            TList<UserInOut> retValList = null;


            try
            {
                // ZORAN: trena storna za ovaa rabota, no moze i vaka (se povikuva samo pri logiranje na user!

                retValList = DataRepository.UserInOutProvider.GetByIdUser(pUser.IdUser);

                if (retValList != null && retValList.Count > 0)
                {
                    for (int i = 0; i < retValList.Count; i++)
                        if (retValList[i].DateTimeOut == null)
                            if (retVal == null)
                                retVal = retValList[i];
                            else
                                if (retVal.DateTimeIn.CompareTo(retValList[i].DateTimeIn) < 0)
                                    retVal = retValList[i];
                }
            }
            catch (Exception e)
            {
                log.Error("getUserInOutForCurrentExtension Error", e);
                return null;
            }

            return retVal;
        }
    


        #endregion

        #region PhoneNumbers
        public TList<PhoneNumbers> getAllPhoneNumbers(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.PhoneNumbersProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<PhoneNumbers> getDataByObjectName(string address)
        {
            try
            {
                return null;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }
        #endregion       

        #region PhoneCalls


        public long updatePhoneCalls(PhoneCalls pPhoneCalls)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.PhoneCallsProvider.Update(pPhoneCalls))
                    retVal = 1;

            }
            catch (Exception e)
            {
                log.Error("updatePhoneCalls Error", e);
                return -1;
            }

            return retVal;
        }



        #endregion

        #region GisObjecs

        public TList<GisObjects> getAllGisObjects(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.GisObjectsProvider.GetPaged(startPage, pageSize, out maxPages);           
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public GisObjects getGisObjectForIdObject(long ID_Objects)
        {
            try
            {
                return DataRepository.GisObjectsProvider.GetByIdObject(ID_Objects);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<GisObjects> getAllGisObjectsByName(string name)
        {
            try
            {
                return DataRepository.GisObjectsProvider.getByObjectName(name);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }


        public TList<GisObjectsCategory> getAllGisObjectsCategories()
        {
            try
            {
                return DataRepository.GisObjectsCategoryProvider.GetAll();
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return null;
            }

        }




        public TList<GisObjectsSubCategory> getAllGisObjectsSubCategories()
        {
            try
            {
                return DataRepository.GisObjectsSubCategoryProvider.GetAll();
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return null;
            }
        }


        public TList<GisObjectsSubCategory> getAllGisObjectsSubCategoriesPerCategory(GisObjectsCategory pGisObjectsCategory)
        {
            try
            {
                return DataRepository.GisObjectsSubCategoryProvider.GetByIdGisObjectsCategory (pGisObjectsCategory.IdGisObjectsCategory);
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return null;
            }
        }



        public TList<GisObjects> getAllGisObjectsPerSubCategories(GisObjectsSubCategory pGisObjectsSubCategory)
        {
            try
            {
                return DataRepository.GisObjectsProvider.GetByIdGisObjectsSubCategory  (pGisObjectsSubCategory.IdGisObjectsCategory);
            }
            catch (Exception ex)
            {
                log.Error("Error", ex);
                return null;
            }
        }



        
        public long insertGisObjects(GisObjects pGisObjects)
        {
            try
            {
                if (DataRepository.GisObjectsProvider.Insert(pGisObjects))
                    return pGisObjects.IdObject;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

      
        public long updateGisObjects(GisObjects pGisObjects)
        {
            try
            {
                if (DataRepository.GisObjectsProvider.Update(pGisObjects))
                    return pGisObjects.IdObject;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }


        public long deleteGisObjects(GisObjects pGisObjects)
        {
            try
            {
                if (DataRepository.GisObjectsProvider.Delete(pGisObjects))
                    return pGisObjects.IdObject;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }



        public long insertGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory)
        {
            try
            {
                if (DataRepository.GisObjectsSubCategoryProvider.Insert(pGisObjectsSubCategory))
                    return pGisObjectsSubCategory.IdGisObjectsSubCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }



        public long updateGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory)
        {
            try
            {
                if (DataRepository.GisObjectsSubCategoryProvider.Update(pGisObjectsSubCategory))
                    return pGisObjectsSubCategory.IdGisObjectsCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }



        public long deleteGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory)
        {
            try
            {
                if (DataRepository.GisObjectsSubCategoryProvider.Delete(pGisObjectsSubCategory))
                    return pGisObjectsSubCategory.IdGisObjectsCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }



        public long insertGisObjectsCategory(GisObjectsCategory pGisObjectsCategory)
        {
            try
            {
                if (DataRepository.GisObjectsCategoryProvider.Insert(pGisObjectsCategory))
                    return pGisObjectsCategory.IdGisObjectsCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }


        public long updateGisObjectsCategory(GisObjectsCategory pGisObjectsCategory)
        {
            try
            {
                if (DataRepository.GisObjectsCategoryProvider.Update(pGisObjectsCategory))
                    return pGisObjectsCategory.IdGisObjectsCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }


        public long deleteGisObjectsCategory(GisObjectsCategory pGisObjectsCategory)
        {
            try
            {
                if (DataRepository.GisObjectsCategoryProvider.Delete(pGisObjectsCategory))
                    return pGisObjectsCategory.IdGisObjectsCategory;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }




        public TList<ObjectDescription> getObjectDescriptionsByGisObject(long IDObject)
        {
            TList<ObjectDescription> retVal = null;
            try
            {
                retVal = DataRepository.ObjectDescriptionProvider.GetByIdObject(IDObject);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }

        public long saveObjectDescriptions(ObjectDescription objectDescription)
        {
            try
            {
                if (objectDescription.IdObjectDescription <= 0)
                {
                    if (DataRepository.ObjectDescriptionProvider.Insert(objectDescription))
                        return objectDescription.IdObjectDescription;
                    else
                        return -1;
                }
                else
                {
                    if (DataRepository.ObjectDescriptionProvider.Update(objectDescription))
                        return objectDescription.IdObjectDescription;
                    else
                        return -1;
                }
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }

        }

        public long deleteObjectDescriptions(ObjectDescription objectDescription)
        {
            try
            {
                if (DataRepository.ObjectDescriptionProvider.Delete(objectDescription))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

        #endregion

        #region GisStreets

        public TList<GisStreets> getAllGisStreets(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.GisStreetsProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public GisStreets GetGisStreetForIdStreet(string IDStreet)
        {
            GisStreets retVal = null;

            try
            {
                return DataRepository.GisStreetsProvider.GetByIdStreet(IDStreet);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return retVal;
            }
        }

        public TList<GisStreets> getGisStreetsByName(string name)
        {
            try
            {
                //return null;
                TList<GisStreets> retVal = null;

                //retVal = DataRepository.GisStreetsProvider.Find("StreetName = '" + name + "'");
                retVal = DataRepository.GisStreetsProvider.GetByName(name);
                return retVal;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        #endregion

        #region GisRegions



        public GisRegions getGisRegionForIdRegion(int IdRegion)
        {
            GisRegions retVal = null;

            try
            {
                return DataRepository.GisRegionsProvider.GetByIdRegion(IdRegion);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return retVal;
            }
        }

        public TList<GisRegions> getAllGisRegions(int startPage, int pageSize, out int maxPages)
        {
            TList<GisRegions> retVal = null;
            maxPages = -1;
            try
            {
                return DataRepository.GisRegionsProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return retVal;
            }
        }

        #endregion

        #region GisPhoneNumbers
        public TList<GisPhoneNumbers> getAllGisPhoneNumbers(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.GisPhoneNumbersProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public GisPhoneNumbers getGisPhoneNumbersByNumber(string phoneNumber)
        {
            GisPhoneNumbers retVal = null;

            try
            {
                retVal = DataRepository.GisPhoneNumbersProvider.GetByPhoneNumber(phoneNumber);

                if (retVal != null)
                {
                    Type[] tl = new Type[2];
                    tl[0] = typeof(GisStreets);
                    tl[1] = typeof(GisObjects);

                    DataRepository.GisPhoneNumbersProvider.DeepLoad(retVal, true, DeepLoadType.IncludeChildren, tl);
                }
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                retVal = null;
            }

            return retVal;
        }


        public long deleteGisPhoneNumbersForPhoneNumber(GisPhoneNumbers gisPhoneNumber)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.GisPhoneNumbersProvider.Delete(gisPhoneNumber))
                {
                    retVal = 0;
                }

            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }

            return retVal;

        }

        public long insertGisPhoneNumbers(GisPhoneNumbers newPhoneNumber)
        {
            long retVal = -1;

            try
            {
                if (DataRepository.GisPhoneNumbersProvider.Insert(newPhoneNumber))
                {
                    retVal = 0;
                }
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }

            return retVal;

        }

        public TList<GisPhoneNumbers> getGisPhoneNumbersByIDObject(long IDObject)
        {
            TList<GisPhoneNumbers> retVal = null;
            try
            {
                retVal = DataRepository.GisPhoneNumbersProvider.GetByIdObject(IDObject);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }

        public TList<PhoneNumberDescription> getPhoneNumberDescriptionsByGisPhoneNumber(string phoneNumber)
        {
            TList<PhoneNumberDescription> retVal = null;
            try
            {
                retVal = DataRepository.PhoneNumberDescriptionProvider.GetByPhoneNumber(phoneNumber);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }

        public long savePhoneNumberDescriptions(PhoneNumberDescription phoneNumberDescription)
        {
            try
            {
                if (phoneNumberDescription.IdPhoneNumberDescription <= 0)
                {
                    if (DataRepository.PhoneNumberDescriptionProvider.Insert(phoneNumberDescription))
                        return phoneNumberDescription.IdPhoneNumberDescription;
                    else
                        return -1;
                }
                else
                {
                    if (DataRepository.PhoneNumberDescriptionProvider.Update(phoneNumberDescription))
                        return phoneNumberDescription.IdPhoneNumberDescription;
                    else
                        return -1;
                }
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }

        }

        public long deletePhoneNumberDescriptions(PhoneNumberDescription phoneNumberDescription)
        {
            try
            {
                if (DataRepository.PhoneNumberDescriptionProvider.Delete(phoneNumberDescription))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

        #endregion

        #region GisSearchRegion
        public TList<GisSearchRegions> getAllGisSearchRegions(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.GisSearchRegionsProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<GisSearchRegions> getGisSearchRegionsByIdRegion(long ID_Region, int typeAlternative)
        {

            //maxPages = -1;
            try
            {
                return DataRepository.GisSearchRegionsProvider.GetByIdRegion(ID_Region);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }
        #endregion

        #region GisAddressModel

        public TList<GisAddressModel> getAllGisAddressModel(int startPage, int pageSize, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.GisAddressModelProvider.GetPaged(startPage, pageSize, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<GisAddressModel> getGisAddressModelForIDStreet(string ID_Street)
        {
            try
            {
                return DataRepository.GisAddressModelProvider.GetByIdStreet(ID_Street);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<GisAddressModel> getGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber)
        {
            try
            {
                //return null;
                TList<GisAddressModel> retVal = null;
                retVal = DataRepository.GisAddressModelProvider.GetIDStreetHouseNumber(ID_Street, houseNumber);
                return retVal;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<AddressModelDescription> getAddressModelDescriptionsForIDStreetHouseNumber(string ID_Street, int houseNumber)
        {
            TList<AddressModelDescription> retVal = null;
            try
            {
                retVal = DataRepository.AddressModelDescriptionProvider.GetIDStreetHouseNumber(ID_Street, houseNumber);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }

        public TList<AddressModelDescription> getAddressModelDescriptionsByIDStreet(string ID_Street)
        {
            TList<AddressModelDescription> retVal = null;
            try
            {
                retVal = DataRepository.AddressModelDescriptionProvider.GetByIdStreet(ID_Street);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
            }
            return retVal;
        }


        public long saveAddressModelDescriptions(AddressModelDescription addressModelDescription)
        {
            try
            {
                if (addressModelDescription.IdAddressModelDescription <= 0)
                {
                    if (DataRepository.AddressModelDescriptionProvider.Insert(addressModelDescription))
                        return addressModelDescription.IdAddressModelDescription;
                    else
                        return -1;
                }
                else
                {
                    if (DataRepository.AddressModelDescriptionProvider.Update(addressModelDescription))
                        return addressModelDescription.IdAddressModelDescription;
                    else
                        return -1;
                }
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }

        }

        public long deleteAddressModelDescriptions(AddressModelDescription addressModelDescription)
        {
            try
            {
                if (DataRepository.AddressModelDescriptionProvider.Delete(addressModelDescription))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }


        #endregion

        #region PhoneNumbersSurovi
        /*
        public long savePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi)
        {
            try
            {
                if (DataRepository.PhoneNumbersSuroviProvider.Insert(phoneNumbersSurovi))
                    return phoneNumbersSurovi.IDPhoneNumbersSurovi;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

        public long deletePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi)
        {
            try
            {
                if (DataRepository.PhoneNumbersSuroviProvider.Delete(phoneNumbersSurovi))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

        public long updatePhoneNumbersSurovi(PhoneNumbersSurovi phoneNumbersSurovi)
        {
            try
            {
                if (DataRepository.PhoneNumbersSuroviProvider.Update(phoneNumbersSurovi))
                    return 0;
                else
                    return -1;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return -1;
            }
        }

        public TList<PhoneNumbersSurovi> getAllPhoneNumbersSurovi(int pageStart, int pageLength, out int maxPages)
        {
            maxPages = -1;
            try
            {
                return DataRepository.PhoneNumbersSuroviProvider.GetPaged(pageStart, pageLength, out maxPages);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<PhoneNumbersSurovi> getPhoneNumbersSuroviByPhoneNumber(string phoneNumber)
        {
            try
            {
                return DataRepository.PhoneNumbersSuroviProvider.GetByTelefon(phoneNumber);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public TList<PhoneNumbersSurovi> getPhoneNumbersSuroviByAddress(string address)
        {
            try
            {
                return DataRepository.PhoneNumbersSuroviProvider.GetByAdresa(address);
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }
         */
        #endregion

        #region  ClientPhoneNumbers
        public Clients searchVipClients(ClientPhoneNumbers clientPhoneNumber)
        {
            try
            {
                ClientPhoneNumbersParameterBuilder filtBuild = new ClientPhoneNumbersParameterBuilder();
                //SqlFilterBuilder<ClientPhoneNumbersColumn> filtBuild = new SqlFilterBuilder<ClientPhoneNumbersColumn>(true);
                filtBuild.AppendEquals(ClientPhoneNumbersColumn.PhoneNumber, clientPhoneNumber.PhoneNumber);




                SqlFilterParameter param = new SqlFilterParameter(ClientPhoneNumbersColumn.PhoneNumber, clientPhoneNumber.PhoneNumber, 0);
                
                SqlFilterParameterCollection paramCol = new SqlFilterParameterCollection();
                paramCol.Add(param);



                TList<ClientPhoneNumbers> phoneNumbers =
                    DataRepository.ClientPhoneNumbersProvider.Find(filtBuild.GetParameters());
                if (phoneNumbers.Count > 0)
                    return DataRepository.ClientsProvider.GetByIdClient(phoneNumbers[0].IdClient);
                else
                    return null;
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }
        #endregion        

        #region Reservation

        public TList<Reservations> getReservationsForContactPhone(string phoneNumber)
        {
            return new TList<Reservations>();
        }

        public TList<Reservations> getReservationsForPeriod(DateTime dateFrom, DateTime dateTo)
        {
            return new TList<Reservations>();
        }

        public int deleteReservationList(TList<Reservations> objReservations)
        {
            return 0;
        }

        public int deleteReservationContactPhonePeriod(string phoneNumber, DateTime dateFrom, DateTime dateTo)
        {
            return 0;
        }

        #endregion

        #region FiskalReceipt

        public TList<FiskalReceipt> getFiskalReceiptForVehicle(long iDVehicle, DateTime dateFrom, DateTime dateTo)
        {
            return new TList<FiskalReceipt>();
        }

        public TList<FiskalReceipt> getFiskalReceiptForPeriod(DateTime dateFrom, DateTime dateTo)
        {
            return new TList<FiskalReceipt>();
        }

        public TList<FiskalReceipt> getFiskalReceiptForDriver(long iDDriver, DateTime dateFrom, DateTime dateTo)
        {
            return new TList<FiskalReceipt>();
        }

        public TList<FiskalReceipt> getFiskalReceiptForRfIdCard(string rfIdCard, DateTime dateFrom, DateTime dateTo)
        {
            return new TList<FiskalReceipt>();
        }

        public long saveFiskalReceipt(FiskalReceipt objFiskalReceipt)
        {
            return 0;
        }

        public long deleteFiskalReceipt(FiskalReceipt objFiskalReceipt)
        {
            return 0;
        }

        #endregion
       
        #region SearchVechicles Ror reaon

        public TList<Vehicle> searchAllVechiclesByIDReaon(long ID_Region, int typeAlternative)
        {

            TList<Vehicle> retVal = VehiclesContainer.Instance.searchAllVechiclesByIDReaon(ID_Region, typeAlternative);
            
            return retVal;
        }

        public bool IsVehicleEligibleForCall(Vehicle pVehicle, PhoneCalls phoneCall)
        {
            bool retVal = false;

            retVal = VehiclesContainer.Instance.IsVehicleEligableForCall(pVehicle.IdVehicle);

            return retVal;
        }

        public TList<Vehicle> GetAllAvailableVehiclesForPhoneCall(PhoneCalls phoneCall)
        {
            return null;
        }

        public TList<Vehicle> GetAllAvailableVehiclesForLocation(decimal? latitude, decimal? longitude,long id_Region)
        {
            return null;
        }


        #endregion

        public TList<Vehicle> getAllVehiclesByCompanies(string pIdCompanies)
        {
            TList<Vehicle> retVal = null;

            // ZORAN: gi dobivam ID_Company, kako string, kade sekoj ID_Company e oddelen so zapirka
            // -------------------------------------------------------------------------------------

            try
            {
                char[] delimiterChars = { ',' };

                string[] stringIdCompany = pIdCompanies.Split(delimiterChars);

                TList<Vehicle> retValPerCompany = null;

                foreach (string tmpString in stringIdCompany)
                {
                    retValPerCompany = VehiclesContainer.Instance.getAllForCompany(int.Parse(tmpString));                   

                    if (retValPerCompany != null && retValPerCompany.Count > 0)
                    {
                        if (retVal == null)
                            retVal = new TList<Vehicle>();

                        foreach (Vehicle tmpVehicle in retValPerCompany)
                            retVal.Add(tmpVehicle);
                    }
                }
            }
            catch (Exception e)
            {
                retVal = null;
                log.Error("ERROR ", e);
            }

            return retVal;
        }


        public TList<PhoneCalls> GetAllActivePhoneCalls()
        {
            TList<PhoneCalls> retVal = null;


            NaseTaxiSwitchExchangeListener tmpNaseTaxiSwitchExchangeListener = new NaseTaxiSwitchExchangeListener();

            retVal = tmpNaseTaxiSwitchExchangeListener.getAllActivePhoneCalls();


            return retVal;
        }


        #region RfIDCards


        public RegisteredPassengers getRegisteredPassingerForCard(string description)
        {
            try
            {
                RfIdCardsParameterBuilder pbuilder = new RfIdCardsParameterBuilder();
                pbuilder.AppendEquals(RfIdCardsColumn.Description, description);

                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.Find(pbuilder.GetParameters());
                if ((cards == null) || (cards.Count == 0))
                    return null;

                TList<RegisteredPassengers> pass = DataRepository.RegisteredPassengersProvider.GetByIdRfIdCard(cards[0].IdRfIdCard);

                if ((pass == null) || (pass.Count != 1))
                {
                    return null;
                }
                else
                {
                    Type[] tl = new Type[2];
                    tl[0] = typeof(Clients);
                    tl[1] = typeof(Company);

                    DataRepository.RegisteredPassengersProvider.DeepLoad(pass, true, DeepLoadType.IncludeChildren, tl);
                    return pass[0];
                }

                
            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        public Driver getDriverForCard(string description)
        {
            try
            {
                RfIdCardsParameterBuilder pbuilder = new RfIdCardsParameterBuilder();
                pbuilder.AppendEquals(RfIdCardsColumn.Description, description);

                TList<RfIdCards> cards = DataRepository.RfIdCardsProvider.Find(pbuilder.GetParameters());
                if ((cards == null) || (cards.Count == 0))
                    return null;

                TList<Driver> drivers = DataRepository.DriverProvider.GetByIDRfIdCard(cards[0].IdRfIdCard);

                if ((drivers == null) || (drivers.Count != 1))
                {
                    return null;
                }
                else
                {
                    Type[] tl = new Type[1];
                    tl[0] = typeof(Company);

                    DataRepository.DriverProvider.DeepLoad(drivers, true, DeepLoadType.IncludeChildren, tl);
                    return drivers[0];
                }


            }
            catch (Exception e)
            {
                log.Error("Error", e);
                return null;
            }
        }

        #endregion


        #region MobileReservations

        public TList<MobileReservations> getAllActiveMobileReservations(long IdUser)
        {
            TList<MobileReservations> retVal = null;

            try
            {
                retVal = DataRepository.MobileReservationsProvider.GetAllActiveMobileReservations();
            }
            catch (Exception ex)
            {
                log.Error("Greska vo getAllActiveMobileReservations", ex);
            }

            return retVal;
        }


        public long UpdateMobileReservation(long pIdReservation, long pIdUser)
        {
            long retVal = -1;

            try
            {
                DataRepository.MobileReservationsProvider.SetMobileReservationCompleted(pIdReservation, pIdUser);

                retVal = 1;
            }
            catch (Exception ex)
            {
                log.Error("Greska vo UpdateMobileReservation", ex);
            }

            return retVal;
        }

        #endregion

        #endregion


    }
}
