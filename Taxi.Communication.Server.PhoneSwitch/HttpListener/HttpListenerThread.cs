using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;


namespace Taxi.Communication.Server.PhoneSwitch.HttpListener
{
    class HttpListenerThread
    {
        bool exit = false;

        public void start()
        {

            System.Net.HttpListener listener = new System.Net.HttpListener();

            listener.Prefixes.Add("http://*:1004/");
            listener.Start();
            Console.WriteLine("Listening...");


            while(!exit)
            {
                HttpListenerContext ctx = listener.GetContext();
                new Thread(new Worker(ctx).ProcessRequest).Start();
            }     
        }


        class Worker
        {
            private HttpListenerContext context;

            public Worker(HttpListenerContext context)
            {
                this.context = context;
            }

            public void ProcessRequest()
            {
                string msg = context.Request.HttpMethod + " " + context.Request.Url;
                Console.WriteLine(msg);

                StringBuilder sb = new StringBuilder();
                //sb.Append("<html><body><h1>" + msg + "</h1>");

                //sb.Append("Pozdrav od Zoran");
                //sb.Append("<br>");
                //sb.Append(context.Request.Url.AbsolutePath);

                NameValueCollection queryStringCollection = context.Request.QueryString;

                //sb.Append("<br>");

                sb.Append(queryStringCollection["id"]);

                Console.WriteLine(queryStringCollection["id"]);


                //DumpRequest(context.Request, sb);
                //sb.Append("</body></html>");

                byte[] b = Encoding.UTF8.GetBytes(sb.ToString());
                context.Response.ContentLength64 = b.Length;
                context.Response.OutputStream.Write(b, 0, b.Length);
                context.Response.OutputStream.Close();
            }
        }
    }
}








    

