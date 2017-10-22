using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

using System.Configuration;

using log4net;
using GlobSaldo.AVL.Data;
using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data.Bases;
//using JP.Data.Utils;

using Taxi.Communication.Server;

using System.Collections.Specialized;

using System.ServiceModel.Web;
using System.Runtime.Serialization.Json;

using System.Globalization;
using System.Web;
using System.Web.UI;


using Taxi.Communication.Server.Utils.Containers;


namespace Taxi.Communication.Server.HttpListener
{
    public class MobileHttpListener
    {
        bool exit = false;

        private static ILog log;

        private MobileHttpListenerMethods mMobileHttpListenerMethods = new MobileHttpListenerMethods();        

        private ServiceCallBack _serviceCallBack = null;


        public void setCallBack(ServiceCallBack pServiceCallBack)
        {
            _serviceCallBack = pServiceCallBack;
            mMobileHttpListenerMethods.setCallBack(_serviceCallBack);           
        }

        
        public void start()
        {
            log = LogManager.GetLogger("MyService");

            try
            {               
                System.Net.HttpListener mListener = new System.Net.HttpListener();

               
                mListener.Prefixes.Add(ConfigurationManager.AppSettings["HttpListenerBaseAddress"]);
                mListener.Start();
                Console.WriteLine("Listenin Android ...");

                

                while (true)
                {
                    HttpListenerContext ctx = mListener.GetContext();

                    NameValueCollection queryStringCollection = ctx.Request.QueryString;

                    Console.WriteLine(queryStringCollection["id"]); 
                    

                    Thread ThreadSendToIpPhone = new Thread(delegate() { ProcessRequest(ctx); });

                    ThreadSendToIpPhone.Start();
                }               
            }
            catch (SocketException se)
            {
                log.Error(se.Message);
            }
        }



        public void ProcessRequest(HttpListenerContext pContext)
        {
            string msg = pContext.Request.HttpMethod + " " + pContext.Request.Url;

            string mSubAddress = "";

            CultureInfo culture = CultureInfo.CreateSpecificCulture("fr-FR");

            mSubAddress = GetSubAdress(pContext.Request.RawUrl);

            if (mSubAddress == "")
            {
                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo adresa");
                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);

                return;
            }

            Console.WriteLine(mSubAddress);           

            NameValueCollection queryStringCollection = HttpUtility.ParseQueryString(pContext.Request.Url.Query);

            switch (pContext.Request.HttpMethod)
            {
                case "PUT":
                    break;
                case "POST":
                    break;
                case "GET":

                    switch (mSubAddress)
                    {
                      
                        case "Register":

                            Console.WriteLine(queryStringCollection["email"]);
                            Console.WriteLine(queryStringCollection["password"]);
                            Console.WriteLine(queryStringCollection["Name"]);
                            Console.WriteLine(queryStringCollection["Lastname"]);
                            Console.WriteLine(queryStringCollection["PhoneNumber"]);
                            Console.WriteLine(queryStringCollection["IMEI"]);


                            mMobileHttpListenerMethods.Register(
                                                                        pContext,
                                                                        queryStringCollection["email"],
                                                                        queryStringCollection["password"],
                                                                        queryStringCollection["Name"],
                                                                        queryStringCollection["Lastname"],
                                                                        queryStringCollection["PhoneNumber"],
                                                                        queryStringCollection["IMEI"]
                                                                      );

                            break;

                        case "Confirmation":

                            Console.WriteLine("IMEI: " + queryStringCollection["IMEI"]);
                            Console.WriteLine("Code: " + queryStringCollection["Code"]);


                            mMobileHttpListenerMethods.Confirmation(
                                                                        pContext,
                                                                        queryStringCollection["IMEI"],
                                                                        queryStringCollection["Code"]
                                                                      );

                            break;

                        case "Deactivate":

                            Console.WriteLine("IMEI: " + queryStringCollection["IMEI"]);
                            Console.WriteLine("Id:   " + queryStringCollection["Id"]);


                            mMobileHttpListenerMethods.Deactivate(
                                                                        pContext,
                                                                        queryStringCollection["IMEI"],
                                                                        queryStringCollection["Id"]
                                                                      );

                            break;

                        case "ConfirmDrive":

                            Console.WriteLine("IMEI:        " + queryStringCollection["IMEI"]);
                            Console.WriteLine("IdOrder:          " + queryStringCollection["IdOrder"]);
                            Console.WriteLine("ConfirmCode: " + queryStringCollection["ConfirmCode"]);


                            mMobileHttpListenerMethods.ConfirmDrive(
                                                                        pContext,
                                                                        queryStringCollection["IMEI"],
                                                                        queryStringCollection["IdOrder"],
                                                                        queryStringCollection["ConfirmCode"]
                                                                      );

                            break;

                        case "GetVehiclesStatesByIdCompany":
                            long mIdCompany;

                            if (long.TryParse(queryStringCollection["pIdCompany"], out mIdCompany))
                            {
                                mMobileHttpListenerMethods.GetVehiclesStatesByIdCompany(pContext, mIdCompany);
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na kompanija");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;

                        case "GetVehicleStatesByIdVehicle":
                            long mIdVehicle2;

                            if (long.TryParse(queryStringCollection["pIdVehicle"], out mIdVehicle2))
                            {
                                mMobileHttpListenerMethods.GetVehicleStatesByIdVehicle(pContext, mIdVehicle2);
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na kompanija");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;
                       

                        case "GetVehicleLocationByIdOrder":
                            long mIdOrder;
                            long mIdVehicle_;

                            if (long.TryParse(queryStringCollection["pIdOrder"], out mIdOrder))
                            {
                                if(long.TryParse(queryStringCollection["pIdVehicle"], out mIdVehicle_))
                                {
                                    Console.WriteLine("GetVehicleLocationByIdOrder-ID_Vehicle: " + queryStringCollection["pIdVehicle"]);
                                    Console.WriteLine("GetVehicleLocationByIdOrder-ID_Order  : " + queryStringCollection["pIdOrder"]);
                                    Console.WriteLine("GetVehicleLocationByIdOrder-EMEI      : " + queryStringCollection["pImei"]);

                                    mMobileHttpListenerMethods.GetVehicleLocationByIdOrder(pContext, mIdVehicle_, mIdOrder, queryStringCollection["pImei"]);
                                }
                                else
                                {
                                    string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na Vehicle");
                                    mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                                }
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na order");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;

                        case "Order":
                            double mLongitudeX;
                            double mLatitudeY;

                            Console.WriteLine(queryStringCollection["LongitudeX"]);
                            Console.WriteLine(queryStringCollection["LatitudeY"]);
                            Console.WriteLine(queryStringCollection["PhoneNumber"]);
                            Console.WriteLine(queryStringCollection["PhoneExtension"]);
                            Console.WriteLine(queryStringCollection["PickupAdress"]);                           
                            Console.WriteLine(queryStringCollection["Comment"]);
                            Console.WriteLine(queryStringCollection["Do"]);
                            Console.WriteLine(queryStringCollection["StreetName"]);
                            Console.WriteLine(queryStringCollection["StreetNumber"]);
                            Console.WriteLine(queryStringCollection["Companies"]);
                            Console.WriteLine(queryStringCollection["pImei"]);

                            

                            if (double.TryParse(queryStringCollection["LongitudeX"], System.Globalization.NumberStyles.Number, culture, out mLongitudeX) && double.TryParse(queryStringCollection["LatitudeY"], System.Globalization.NumberStyles.Number, culture, out mLatitudeY))

                            //if (double.TryParse(queryStringCollection["LongitudeX"], out mLongitudeX) && double.TryParse(queryStringCollection["LatitudeY"], out mLatitudeY))                            
                            {                               
                                mMobileHttpListenerMethods.MakeOrder(
                                                                       pContext,
                                                                       queryStringCollection["PhoneNumber"],
                                                                       queryStringCollection["PhoneExtension"],
                                                                       queryStringCollection["PickupAdress"],
                                                                       queryStringCollection["Comment"],
                                                                       queryStringCollection["Do"],
                                                                       mLatitudeY,
                                                                       mLongitudeX,
                                                                       queryStringCollection["StreetName"],
                                                                       queryStringCollection["StreetNumber"],
                                                                       queryStringCollection["Companies"],
                                                                       queryStringCollection["pImei"]
                                                                     );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na koordinati");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;

                        case "OrderConfirmation":
                            long mIdVehicleConfirmation;

                            int mConfirmationStatus;   //1=confirmed, 0=canceled

                            if (long.TryParse(queryStringCollection["pIdVehicle"], out mIdVehicleConfirmation) &&
                                int.TryParse(queryStringCollection["pConfirmationStatus"], out mConfirmationStatus))
                            {
                                Console.WriteLine("OrderConfirmation, pIdVehicle         : " + queryStringCollection["pIdVehicle"]);
                                Console.WriteLine("OrderConfirmation, pConfirmationStatus: " + queryStringCollection["pConfirmationStatus"]);
                                Console.WriteLine("OrderConfirmation, pConfirmationStatus: " + queryStringCollection["pImei"]);

                                mMobileHttpListenerMethods.OrderConfirmation(pContext, mIdVehicleConfirmation, mConfirmationStatus, queryStringCollection["pImei"]);
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo parametri");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;
                       

                        case "SentMessageToVehicleByIdOrder":

                            long m_IdOrder;

                            if (long.TryParse(queryStringCollection["pIdOrder"], out m_IdOrder))
                            {
                                mMobileHttpListenerMethods.SentMessageToVehicleByIdOrder(
                                                                                           pContext,
                                                                                           m_IdOrder,
                                                                                           -1,
                                                                                           queryStringCollection["pMessage"],
                                                                                           queryStringCollection["pImei"]
                                                                                           );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na Order");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;


                        case "SentMessageToVehicleByIdVehicle":

                            long m_IdVehicle;

                            if (long.TryParse(queryStringCollection["pIdVehicle"], out m_IdVehicle))
                            {
                                mMobileHttpListenerMethods.SentMessageToVehicleByIdVehicle(
                                                                                           pContext,
                                                                                           m_IdVehicle,
                                                                                           queryStringCollection["pMessage"]
                                                                                           );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na Vehicle");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;

                        case "SentRingMessageToVehicleByIdVehicle":

                            long m_IdVehicleForRing;
                            short m_NoSeconds;

                            if (long.TryParse(queryStringCollection["pIdVehicle"], out m_IdVehicleForRing) &&
                                    short.TryParse(queryStringCollection["pNoSeconds"], out m_NoSeconds))

                            {
                                mMobileHttpListenerMethods.SentRingMessageToVehicleByIdVehicle(
                                                                                           pContext,
                                                                                           m_IdVehicleForRing,
                                                                                           m_NoSeconds
                                                                                           );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na Vehicle");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }

                            break;   

                        case "GetTextInfo":

                            Console.WriteLine(queryStringCollection["pTextIndex"]);
                            Console.WriteLine(queryStringCollection["pIMEI"]);
                           
                            mMobileHttpListenerMethods.GetTextInfo(
                                                                    pContext,
                                                                    queryStringCollection["pTextIndex"],
                                                                    queryStringCollection["pIMEI"]
                                                                   );
                            
                            break;
                                               

                        case "ForceUpdateUnit":
                            
                    
                            mMobileHttpListenerMethods.ForceUpdateUnit (
                                                                    pContext,
                                                                   queryStringCollection["ured"],
                                                                   queryStringCollection["ip1"],
                                                                   queryStringCollection["ip2"],
                                                                   queryStringCollection["ip3"],
                                                                   queryStringCollection["ip4"],
                                                                   queryStringCollection["port"]
                                                                   );
                          
                            break;

// ********************************************************************************************************
// Od tuka nadole e prv UPDATE...

                        case "GetListOfCompanies":


                            mMobileHttpListenerMethods.GetListOfCompanies(
                                                                    pContext,
                                                                    queryStringCollection["pIMEI"]
                                                                   );

                            break;                       


                        case "GetReasonsForCancel":


                            mMobileHttpListenerMethods.GetReasonsForCancel(
                                                                    pContext,
                                                                    queryStringCollection["pIMEI"]
                                                                   );

                            break;

                        case "GetPaymentOptions":


                            mMobileHttpListenerMethods.GetPaymentOptions(
                                                                    pContext,
                                                                    queryStringCollection["pIMEI"]
                                                                   );

                            break;

                        case "CancelOrder":

                            long mIdVehicle;
                            long mIdOrderForCancel;
                            long mIdReasonForCancel;

                            if (long.TryParse(queryStringCollection["pIdVehicle"], out mIdVehicle) &&
                                long.TryParse(queryStringCollection["pIdOrder"], out mIdOrderForCancel) &&
                                long.TryParse(queryStringCollection["pIdReasonForCancel"], out mIdReasonForCancel))
                            {
                                //Console.WriteLine("CancelOrder, pIdVehicle        : " + queryStringCollection["pIdVehicle"]);
                                Console.WriteLine("CancelOrder, pIdOrder          : " + queryStringCollection["pIdOrder"]);
                                Console.WriteLine("CancelOrder, pIdReasonForCancel: " + queryStringCollection["pIdReasonForCancel"]);
                                Console.WriteLine("CancelOrder, pImei             : " + queryStringCollection["pImei"]);

                                mMobileHttpListenerMethods.CancelOrder(pContext, mIdOrderForCancel, mIdReasonForCancel, queryStringCollection["pImei"]);
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo parametri");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;


                        case "InsertServiceEvaluation":
                                                       
                            long mIdOrderForEvaluation;
                            int mEvaluationValue;

                            if (long.TryParse(queryStringCollection["pIdOrder"], out mIdOrderForEvaluation) &&
                                int.TryParse(queryStringCollection["pEvaluationValue"], out mEvaluationValue))
                            {

                                Console.WriteLine("CancelOrder, pIdOrder          : " + queryStringCollection["pIdOrder"]);
                                Console.WriteLine("CancelOrder, pIdReasonForCancel: " + queryStringCollection["pEvaluationValue"]);
                                Console.WriteLine("CancelOrder, pImei             : " + queryStringCollection["pImei"]);
                                Console.WriteLine("CancelOrder, pImei             : " + queryStringCollection["pComment"]);

                                mMobileHttpListenerMethods.InsertServiceEvaluation(pContext, mIdOrderForEvaluation, mEvaluationValue, queryStringCollection["pImei"], queryStringCollection["pComment"]);
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo parametri");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;


                        case "ReservationNew":
                            double mReservationLongitudeX;
                            double mReservationLatitudeY;
                          
                            if (double.TryParse(queryStringCollection["LongitudeX"], System.Globalization.NumberStyles.Number, culture, out mReservationLongitudeX) && double.TryParse(queryStringCollection["LatitudeY"], System.Globalization.NumberStyles.Number, culture, out mReservationLatitudeY))
                            
                            {
                                log.Debug("ZORAN, MOBILE, pravam rezervacija.....(sega sum vo MobileHttpListener)");

                                mMobileHttpListenerMethods.ReservationNew(
                                                                       pContext,
                                                                       queryStringCollection["PickupAdress"],
                                                                       queryStringCollection["Comment"],
                                                                       queryStringCollection["Do"],
                                                                       mReservationLongitudeX,
                                                                       mReservationLatitudeY,
                                                                       queryStringCollection["StreetName"],
                                                                       queryStringCollection["StreetNumber"],
                                                                       queryStringCollection["Companies"],
                                                                       queryStringCollection["DateTime"],
                                                                       queryStringCollection["pImei"]
                                                                     );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na koordinati");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;


                        case "ReservationUpdate":
                            double mReservationUpdateLongitudeX;
                            double mReservationUpdateLatitudeY;

                            if (double.TryParse(queryStringCollection["LongitudeX"], System.Globalization.NumberStyles.Number, culture, out mReservationUpdateLongitudeX) && double.TryParse(queryStringCollection["LatitudeY"], System.Globalization.NumberStyles.Number, culture, out mReservationUpdateLatitudeY))
                            {
                                log.Debug("ZORAN, MOBILE, pravam UPDATE.....(sega sum vo MobileHttpListener)");

                                mMobileHttpListenerMethods.ReservationUpdate(
                                                                       pContext,
                                                                       queryStringCollection["IdReservation"],
                                                                       queryStringCollection["PickupAdress"],
                                                                       queryStringCollection["Comment"],
                                                                       queryStringCollection["Do"],
                                                                       mReservationUpdateLongitudeX,
                                                                       mReservationUpdateLatitudeY,
                                                                       queryStringCollection["StreetName"],
                                                                       queryStringCollection["StreetNumber"],
                                                                       queryStringCollection["Companies"],
                                                                       queryStringCollection["DateTime"],
                                                                       queryStringCollection["pImei"]
                                                                     );
                            }
                            else
                            {
                                string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo ID na koordinati");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                            }
                            break;

                        case "ReservationCancel":                            

                                mMobileHttpListenerMethods.ReservationCancel(
                                                                       pContext,
                                                                       queryStringCollection["IdReservation"], 
                                                                       queryStringCollection["pImei"]
                                                                     );
                            break;


                        case "ReservationGetAllPending":

                            
                                mMobileHttpListenerMethods.ReservationGetAllPending(
                                                                       pContext,                                                                       
                                                                       queryStringCollection["pImei"]
                                                                     );
                            break;

                        default:
                            {
                                string mJsonResponse2 = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo povikuvanje na metoda");
                                mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse2);
                            }
                            break;
                    }
                    break;

                    default:
                        {
                            string mJsonResponse = mMobileHttpListenerMethods.GenerateJsonForResponse(-1, "Greska vo HTTP metoda");
                            mMobileHttpListenerMethods.SendErrorToMobile(pContext, mJsonResponse);
                        }
                        break;
            }
        }


                

        private string GetSubAdress(string pRawUrl)
        {
            string retVal = "";

            char[] mCharArea = new char[] {'/'};

            try
            {
                if (pRawUrl.IndexOfAny(mCharArea, 1) > 0)
                    retVal = pRawUrl.Substring(1, pRawUrl.IndexOfAny(mCharArea, 1)-1);
            }

            catch (Exception ex)
            {
                log.Error(ex.Message);

                retVal = "";
            }

            return retVal;
        }


        
        
    }
}
