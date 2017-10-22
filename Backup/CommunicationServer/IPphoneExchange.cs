using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using GlobSaldo.AVL.Entities;
using Taxi.Communication.Server.PhoneSwitch.NaseTaxiSwitch;

namespace Taxi.Communication.Server
{
    [ServiceContract()]
    public interface IIPphoneExchangeService
    {
        ////#region PhoneCalls

        ////[OperationContract]
        ////string setPhoneCalls(string pPhoneCalls);

        ////#endregion

        [OperationContract]
        [WebInvoke(Method = "PUT",
                 ResponseFormat = WebMessageFormat.Xml,
                 BodyStyle = WebMessageBodyStyle.Wrapped,
                 UriTemplate = "xml")]
        string setPhoneCallsXml(string id);


        [OperationContract]
        [WebInvoke(Method = "PUT",
                 ResponseFormat = WebMessageFormat.Json,
                 BodyStyle = WebMessageBodyStyle.Wrapped,
                 UriTemplate = "json")]
        string setPhoneCallsJson(string id);
      



    }

    ////[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    public class IPphoneExchangeService : IIPphoneExchangeService
    {
        public static readonly ILog log = log4net.LogManager.GetLogger("MyService");
        private Taxi.Communication.Server.PhoneSwitch.PhoneSwitchListener _tt;

        public IPphoneExchangeService(Taxi.Communication.Server.PhoneSwitch.PhoneSwitchListener tt)
        {
            this._tt = tt;
        }

        public string setPhoneCallsXml(string pPhoneCall)
        {
            //log.Debug(System.DateTime.Now.ToString() + ":  " + pPhoneCall);

            this._tt.OnIpDataReceived(pPhoneCall);

            // ZORAN:   Ova e dodadeno poradi toa sto ne mozat da generiraat NA poraka za titka
            //          Zatoa, kako posleden parametar stavaat 1, ako e titka
            ////          Tuka, na sekoj povik sto ima 1-ca, prvo go pustam povikot so RC, pa potoa vednas NA
            ////          SE PLASAM da ne go zabavam procesot so ova, no toa e toa vo momentov!!!

            try
            {
                char[] delimiterChars = { ',' };

                string[] IpPhoneExcangeValues = pPhoneCall.Split(delimiterChars);

                if (IpPhoneExcangeValues[IpPhoneExcangeValues.GetLength(0) - 1] == "1")
                {
                    pPhoneCall = pPhoneCall.Replace("RC", "NA");

                    //log.Debug(System.DateTime.Now.ToString() + ":  " + pPhoneCall + "--Generirano");
                    this._tt.OnIpDataReceived(pPhoneCall);
                }
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            

            return pPhoneCall;
        }


        public string setPhoneCallsJson(string pPhoneCall)
        {
            log.Debug(System.DateTime.Now.ToString() + ":  " + pPhoneCall);

            this._tt.OnIpDataReceived(pPhoneCall);

            // ZORAN:   Ova e dodadeno poradi toa sto ne mozat da generiraat NA poraka za titka
            //          Zatoa, kako posleden parametar stavaat 1, ako e titka
            ////          Tuka, na sekoj povik sto ima 1-ca, prvo go pustam povikot so RC, pa potoa vednas NA
            ////          SE PLASAM da ne go zabavam procesot so ova, no toa e toa vo momentov!!!

            ////try
            ////{
            ////    char[] delimiterChars = { ',' };

            ////    string[] IpPhoneExcangeValues = pPhoneCall.Split(delimiterChars);

            ////    if (IpPhoneExcangeValues[IpPhoneExcangeValues.GetLength(0) - 1] == "1")
            ////    {
            ////        pPhoneCall = pPhoneCall.Replace("RC", "NA");

            ////        //log.Debug(System.DateTime.Now.ToString() + ":  " + pPhoneCall + "--Generirano");
            ////        this._tt.OnIpDataReceived(pPhoneCall);
            ////    }
            ////}
            ////catch (Exception ex)
            ////{
            ////    log.Error(ex.Message);
            ////}



            return pPhoneCall;
        }


        private PhoneCalls parsePhoneCall(string ppPhoneCall)
        {
            return null;
        }
    }
}
