using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using GlobSaldo.AVL.Entities;
using GlobSaldo.AVL.Data;
//using JP.Data.Utils;

using log4net;
using System.Reflection;

namespace Taxi.Communication.Server.Utils
{
    public class KeepAliveThread
    {
        private bool m_work;
        private int m_sleepTime;

        private ILog log;

        private KeepAlive delegateHeandler;

        ICallbacksKeepAlive _callBack;

        public void setCallBack(ICallbacksKeepAlive callBack)
        {
            _callBack = callBack;
        }


        public KeepAliveThread(int sleepTime)
        {
            m_work = true;
            m_sleepTime = sleepTime;
            log = LogManager.GetLogger("MyService");
            log.Info("Starting KeepAlive Thread with Sleep time=" + m_sleepTime.ToString());

        }

        public void stop()
        {
            this.m_work = false;
        }

        public void start()
        {
            delegateHeandler = _callBack.CallBackKeepAlive;

            while (m_work)
            {
                try
                {
                    try
                    {
                        delegateHeandler();
                    }
                    catch (Exception e)
                    {
                        log.Error("Exception in Reservation processing", e);
                    }


                    Thread.Sleep(m_sleepTime);
                }
                catch (Exception e)
                {
                    log.Fatal("Error in KeepAlive thread thread", e);
                    m_work = false;
                }
            }
            log.Info("KeepAlive thread EXCITING");
        }
    }
}
