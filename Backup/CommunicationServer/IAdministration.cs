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
using GlobSaldo.AVL.Entities.PartialClasses;

namespace Taxi.Communication.Server
{
    [ServiceContract()]
    public interface IAdministration
    {
       
        #region Locations
        [OperationContract]
        TList<Locations> getLocationsByVehicleAndPeriod(long pIdVehicle, DateTime pDateFrom, DateTime pDateTo);

       
        #endregion


        #region Users
        [OperationContract]
        TList<Users> getAllUsers(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<Users> getAllUsersByCompany(Company pIdCompany);

        [OperationContract]
        TList<Users> getDataByUserNamePassword(string username, string password);

        [OperationContract]
        long InsertUser(Users pUser);

        [OperationContract]
        long UpdateUser(Users pUser);

        [OperationContract]
        long DeleteUser(Users pUser);

        #endregion

        #region FiskalReceipt

        [OperationContract]
        TList<FiskalReceipt> getAllFiskalReceiptByVehicleForPeriod(Vehicle vehicle, DateTime dateFrom,DateTime dateTo);

        [OperationContract]
        long UpdateFiskalReceipt(FiskalReceipt pObject);

        #endregion

        #region Clients

        [OperationContract]
        TList<Clients> getAllClientsByCompany(Company pIdCompany);

        [OperationContract]
        TList<Clients> getAllClientsByName(string Name, Company pIdCompany);

        [OperationContract]
        long InsertClient(Clients pClient);

        [OperationContract]
        long UpdateClient(Clients pClient);

        [OperationContract]
        long DeleteClient(Clients pClient);

        #endregion


        #region PenaltyPerDriverPerPeriod

        [OperationContract]
        PenaltyPerDriverPerPeriod getPenaltyPerDriverPerPeriod(long pIdDriver);

        [OperationContract]
        long InsertPenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriod);

        [OperationContract]
        long UpdatePenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriodt);

        [OperationContract]
        long DeletePenaltyPerDriverPerPeriod(PenaltyPerDriverPerPeriod pPenaltyPerDriverPerPeriod);

        #endregion

        #region Drivers
        
        [OperationContract]
        TList<Driver> getAllDriversByCompany(Company pCompany);

        [OperationContract]
        TList<Driver> getAllDriversByCompanyWithoutRfIdCard(Company pCompany);

        [OperationContract]
        TList<Driver> getAllDriversByCompanyWithRfIdCard(Company pCompany);

        [OperationContract]
        long InsertDriver(Driver pDriver);

        [OperationContract]
        long UpdateDriver(Driver pDriver);

        [OperationContract]
        long DeleteDriver(Driver pDriver);


        #endregion

        #region Companies

        [OperationContract]
        TList<Company> getAllCompanies();


        [OperationContract]
        Company GetCompanyByID(long pCompany);

        [OperationContract]
        long InsertCompany (Company pCompany);

        [OperationContract]
        long UpdateCompany(Company pCompany);

        [OperationContract]
        long DeleteCompany(Company pCompany);

        #endregion



        #region GisObjects

        [OperationContract]
        TList<GisObjects> getAllGisObjects(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        GisObjects getGisObjectForIdObject(long ID_Objects);



        [OperationContract]
        long deleteObjectDescriptions(ObjectDescription objectDescription);

        [OperationContract]
        TList<GisObjectsCategory> getAllGisObjectsCategories();


        [OperationContract]
        TList<GisObjectsSubCategory> getAllGisObjectsSubCategories();

        [OperationContract]
        TList<GisObjectsSubCategory> getAllGisObjectsSubCategoriesPerCategory(GisObjectsCategory pGisObjectsCategory);

        [OperationContract]
        TList<GisObjects> getAllGisObjectsPerSubCategories(GisObjectsSubCategory pGisObjectsSubCategory);

        [OperationContract]
        long insertGisObjects(GisObjects pGisObjects);

        [OperationContract]
        long updateGisObjects(GisObjects pGisObjects);

        [OperationContract]
        long deleteGisObjects(GisObjects pGisObjects);


        [OperationContract]
        long insertGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory);

        [OperationContract]
        long updateGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory);

        [OperationContract]
        long deleteGisObjectsSubCategory(GisObjectsSubCategory pGisObjectsSubCategory);


        [OperationContract]
        long insertGisObjectsCategory(GisObjectsCategory pGisObjectsCategory);

        [OperationContract]
        long updateGisObjectsCategory(GisObjectsCategory pGisObjectsCategory);

        [OperationContract]
        long deleteGisObjectsCategory(GisObjectsCategory pGisObjectsCategory);



        #endregion


        #region Vehicles

        [OperationContract]
        TList<Vehicle> getAllVehiclesByCompany(Company pIdCompany);



        [OperationContract]
        long InsertVehicle(Vehicle pVehicle);

        [OperationContract]
        long UpdateVehicle(Vehicle pVehicle);

        [OperationContract]
        long DeleteVehicle(Vehicle pVehicle);

        [OperationContract]
        long AttachUnitOnVehicle(Unit pUnit, Vehicle pVehicle, string pComment);

        [OperationContract]
        long DetachUnitFromVehicle(Unit pUnit, Vehicle pVehicle, string pComment);       

        #endregion


        #region Units

        [OperationContract]
        TList<Unit> getAllUnitsByCompany(Company pCompany);

        [OperationContract]
        long InsertUnit(Unit pUnit);

        [OperationContract]
        long UpdateUnit(Unit pUnit);

        [OperationContract]
        long DeleteUnit(Unit pUnit);

        [OperationContract]
        TList<UnitHistory> GetUnitHistory(Unit pUnit);

        #endregion


        #region RfIdCards

        //[OperationContract]
        //long saveRfIdCardPerClients(RfIdCardPerClients pRfIdCardPerClients);

        //[OperationContract]
        //long deleteRfIdCardPerClients(RfIdCardPerClients pRfIdCardPerClients);

        //[OperationContract]
        //long updateRfIdCardPerClients(RfIdCardPerClients pRfIdCardPerClients);

        //[OperationContract]
        //TList<RfIdCardPerClients> getAllRfIdCardPerClient(Clients pClient);

        [OperationContract]
        long saveRegisteredPassengers(RegisteredPassengers pRegisteredPassengers);

        [OperationContract]
        long deleteRegisteredPassengers(RegisteredPassengers pRegisteredPassengers);

        [OperationContract]
        long updateRegisteredPassengers(RegisteredPassengers pRegisteredPassengers);

        [OperationContract]
        TList<RegisteredPassengers> getAllRegisteredPassengersPerClient(Clients pClient);


        [OperationContract]
        TList<RegisteredPassengers> getAllRegisteredPassengersPerPhisicalEntity(Company pCompany);

        [OperationContract]
        TList<RegisteredPassengers> getAllRegisteredPassengersPerPhisicalEntityFiltered(Company pCompany, string pFilterString);

        [OperationContract]
        TList<RfIdCards> getAllRfIdCardPerCompany(Company pCompany);

        [OperationContract]
        TList<RfIdCards> getAllFreeRfIdCardPerCompany(Company pCompany);

        [OperationContract]
        TList<RfIdCards> getAllBusyRfIdCardPerCompany(Company pCompany);

        [OperationContract]
        RfIdCardPerClients getRfIdCardPerClients(long Id_RfIdCardPerClients);

        [OperationContract]
        RfIdCards getRfIdCardByDriver(Driver pDriver);

        [OperationContract]
        long InsertRfIdCardHistory(RfIdCards pRfIdCard, string pComment);

        [OperationContract]
        long InsertRfIdCard(RfIdCards pRfIdCard);

        [OperationContract]
        long UpdateRfIdCard(RfIdCards pRfIdCard);

        [OperationContract]
        long DeleteRfIdCard(RfIdCards pRfIdCard);


#endregion


        #region UserInOut
        [OperationContract]
        TList<UserInOut> getAllUserInOut(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        long UpdateQueryLogOutUser(DateTime dateOut, long ID_User);

        [OperationContract]
        long insertUserInOut(UserInOut newUserInOut);
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
        long updateGisPhoneNumbers(GisPhoneNumbers newPhoneNumber);

        [OperationContract]
        TList<GisPhoneNumbers> getGisPhoneNumbersByIDObject(long IDObject);
        #endregion


        #region GisAddressModel
        [OperationContract]
        TList<GisAddressModel> getAllGisAddressModel(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreet(string ID_Street);

        [OperationContract]
        TList<GisAddressModel> getGisAddressModelForIDStreetHouseNumber(string ID_Street, int houseNumber);
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


        #region GisStreets

        [OperationContract]
        TList<GisStreets> getAllGisStreets(int startPage, int pageSize, out int maxPages);

        [OperationContract]
        TList<GisStreets> getGisStreetsByName(string name);

        [OperationContract]
        GisStreets GetGisStreetForIdStreet(string IDStreet);

        #endregion


        


        #region  ClientPhoneNumbers

        [OperationContract]
        long saveClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers);

        [OperationContract]
        long deleteClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers);

        [OperationContract]
        long updateClientPhoneNumbers(ClientPhoneNumbers pClientPhoneNumbers);

        [OperationContract]
        TList<ClientPhoneNumbers> getAllClientPhoneNumbers(Clients pClient);

        [OperationContract]
        Clients searchVipClients(ClientPhoneNumbers clientPhoneNumber);




        #endregion
    }
}
