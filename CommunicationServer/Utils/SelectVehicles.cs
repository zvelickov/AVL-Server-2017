using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
using JP.Data.Utils;


namespace Taxi.Communication.Server.Utils
{
    public class SelectVehicles
    {
        // ZORAN:   Selekcija na kola prema XY koordinatite na na povikot, nezavisno kako e dojdeno do niv
        //          PARAMETRI:
        //                  -   pLongitudeX i  pLatitudeY se jasni
        //                  -   pIncludeBusy e dali da gi vkluci vo selekcijata i kolite so patnik!!!
        //                      AKO e narackata od mobilen ili od WEB, pBusy MORA da e FALSE
        //                      NEMA logika toj so mobilen da ceka nekoj da go ostavi patnikot, pa da dojde po nego!
        //                  -   pNumberOfVehiclesToReturn e MAX broj na vozila koi procedurata treba da gi vrati.
        //                      Za mobilni useri + WEB mora da vraka SAMO 1!
        // **********************************************************************************************************
        public TList<Vehicle> SelectVehiclesforXY(double pLongitudeX, double pLatitudeY, bool pIncludeBusy, int pNumberOfVehiclesToReturn)
        {
            TList<Vehicle> lstEligableVehicles = new TList<Vehicle>();
            
     
            //// ZORAN:   Prvo da gi zemam site regioni eligable za toj od kade e povikot
            ////
            //TList<GisSearchRegions> lstEligableGisSearchRegions = DataRepository.GisSearchRegionsProvider.GetByIDREGION(pGisRegion.IDRegion);

            //if (lstEligableGisSearchRegions != null && lstEligableGisSearchRegions.Count > 0)
            //{
            //    // ZORAN:   Sega gi selektiram site vozila koi se vo selektiranite regioni
            //    //          ZABELESKA:  Treba da se smeni funklcijata "GetAll()" so "GetByIDCompany(IDCompany)",
            //    //                      ama ne znam kako da ja dobijam Company!!!)

            //    TList<Vehicle> tmpVehicleGetAll = DataRepository.VehicleProvider.GetAll();

            //    TList<Vehicle> tmpVehicleInEligableRegions = new TList<Vehicle>();

            //    foreach(Vehicle veh in tmpVehicleGetAll)
            //    {
                    
            //        foreach(GisSearchRegions gsr in lstEligableGisSearchRegions)
            //        {
            //            if(veh.currentGPSData.IDRegion == gsr.IDALTERNATIVEREGION)
            //                tmpVehicleInEligableRegions.Add(veh);
            //        }
            //    }


            //    // ZORAN:   Sega proveruvam dali se eligable spored STATE-ot vo koj se naogaat
            //    //          Vo momentov, eligable spored state se:
            //    //              - 
            //    //Containers.VehiclesContainer.Instance.IsVehicleEligableForCall
            //}

            return lstEligableVehicles;

        }
    }
}
