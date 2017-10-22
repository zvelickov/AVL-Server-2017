using System;
using System.Collections.Generic;
using System.Text;
using GlobSaldo.AVL.Entities;

namespace Taxi.Communication.Server.Containers
{

    public sealed class SynchQueue<T>
    {
        //private volatile SynchQueue<T> instance;
        private object syncRoot = new Object();

        private Queue<T> _queLocations;

        public SynchQueue()
        {
           _queLocations = new Queue<T>();
        }
        /*
        public static SynchQueue<T> Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new LocationSynchQueue();
                            instance._queLocations = new Queue<Locations>();
                        }
                    }
                }

                return instance;
            }
        }*/

        public void Enqueue(T loc)
        {
            lock (syncRoot)
            {
                this._queLocations.Enqueue(loc);
            }
        }

        public T Dequeue()
        {
            lock (syncRoot)
            {
                return this._queLocations.Dequeue();
            }
        }

        public int Count()
        {
            lock (syncRoot)
            {
                return this._queLocations.Count;
            }
        }
    }
}
