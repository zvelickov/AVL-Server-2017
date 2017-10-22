using System;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Net.Mail;

namespace Taxi.Communication.Server.Utils.GeoFence
{
    public class MailNotification : INotificationMethod
    {
        #region INotificationMethod Members

        public void sendNotification()
        {
            string fromEmail = "avl@SARIS.local";//sending email from...
            string ToEmail = "pavleb@SARIS.local";//destination email
            string body = "Body From C#";
            string subject = "SUBJ";
            int i = 0;

            try
            {
                SmtpClient sMail = new SmtpClient("15.10.10.125");//exchange or smtp server goes here.
                sMail.DeliveryMethod = SmtpDeliveryMethod.Network;
                sMail.UseDefaultCredentials = false;
                sMail.DeliveryMethod = SmtpDeliveryMethod.Network;

                sMail.Credentials = new NetworkCredential("avl", "avl00");
                sMail.Send(fromEmail, ToEmail, subject, body);
            }
            catch (Exception)
            {
                i++;
            }
        }

        #endregion
    }
}
