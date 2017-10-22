using System;
using System.Collections.Generic;
using System.Text;

namespace Taxi.Communication.Server.Utils.Parsers
{
    public class clsPenaltyCreator
    {
        public void Createpenalty(long IDPenalty, GlobSaldo.AVL.Entities.Vehicle myVehicle)
        {
            //Tuka treba za najaveniot vozac
            string ErrorMessage = "Penalty ID [" + IDPenalty.ToString() + "] for Vehicle " + myVehicle.DescriptionShort;
            //Console.WriteLine(ErrorMessage);
            //ServiceCallBack.log.Debug(ErrorMessage);

            GlobSaldo.AVL.Entities.Penalties tPenalties = new GlobSaldo.AVL.Entities.Penalties();

            tPenalties.IdVehicle = myVehicle.IdVehicle;

            if (myVehicle.CurrentPhoneCall != null)
            {
                tPenalties.IdPhoneCall = myVehicle.CurrentPhoneCall.IdPhoneCall;
            }
            else
            {
                tPenalties.IdPhoneCall = 0;
            }

            if (myVehicle.CurrentPhoneCall != null)
            {
                if (myVehicle.CurrentPhoneCall.oAddressFrom != null)
                {
                    tPenalties.IdAddressFrom = 0;
                    if (myVehicle.CurrentPhoneCall.oAddressFrom.oGisObjects != null)
                        tPenalties.IdAddressFrom = myVehicle.CurrentPhoneCall.oAddressFrom.oGisObjects.IdObject;

                    if (myVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets != null)
                    {
                        tPenalties.IdAddressFrom = Convert.ToInt32(myVehicle.CurrentPhoneCall.oAddressFrom.oGisStreets.IdStreet);
                        tPenalties.HouseNumberFrom = myVehicle.CurrentPhoneCall.oAddressFrom.HouseNumber.ToString();
                    }
                }
                else
                {
                    tPenalties.IdAddressFrom = 0;
                    tPenalties.HouseNumberFrom = "";
                }
            }
            else
            {
                tPenalties.IdAddressFrom = 0;
                tPenalties.HouseNumberFrom = "";
            }

            if (myVehicle.CurrentPhoneCall != null)
            {
                if (myVehicle.CurrentPhoneCall.oAddressTo != null)
                {
                    tPenalties.IdAddressTo = 0;
                                        
                    tPenalties.HouseNumberTo = myVehicle.CurrentPhoneCall.oAddressTo.HouseNumber.ToString();
                }
                {
                    tPenalties.IdAddressTo = 0;
                    tPenalties.HouseNumberTo = "";
                }
            }
            else
            {
                tPenalties.IdAddressTo = 0;
                tPenalties.HouseNumberTo = "";
            }

            if (myVehicle.DriverShiftInOut != null)
                tPenalties.IdDriver = myVehicle.DriverShiftInOut.IdDriver;
            else
                tPenalties.IdDriver = 0;
            
            tPenalties.IdUser = 1; //Kako da znam koj user zapisal (Na server sum i moze da pukaat tajmeri (Neka e 1, ke ignoriram
            tPenalties.IdReasonForPenalty = IDPenalty;
            tPenalties.SystemTime = DateTime.Now;
            try
            {
                GlobSaldo.AVL.Data.DataRepository.PenaltiesProvider.Insert(tPenalties);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Insert Penalties " + ex.Message);
                //ServiceCallBack.log.Debug("Error Insert Penalties " + ex.Message);
            }



        }
    }

}
