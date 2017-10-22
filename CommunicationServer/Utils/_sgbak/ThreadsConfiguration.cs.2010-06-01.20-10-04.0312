using System;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Configuration;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;


namespace Taxi.Communication.Server.Utils
{
    public class ThreadsConfigurationHandler : ConfigurationSection
    {
        public ThreadsConfigurationHandler()
        {
        }
        [System.Configuration.ConfigurationProperty("Threads")]
        public ThreadElementCollection  threadConfigurations
        {
            get
            {
                return (ThreadElementCollection)this["Threads"] ??
                 new ThreadElementCollection();
            }
        }

       
        
    }

    [ConfigurationCollection(typeof(ThreadElement))]
    public sealed class ThreadElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ThreadElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ThreadElement)element).ThreadName;
        }
        public  override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        protected override string ElementName
        {
            get
            {
                return "thread";
            }
        }

        public ThreadElement this[int index]
        {
            get
            {
                return base.BaseGet(index) as ThreadElement;
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                this.BaseAdd(index, value);
            }
        }
    }

    public class ThreadElement  : ConfigurationElement
    {
        //public static CompanyElement CurrentCompany;
        public ThreadElement()
        {
        }
        public ThreadElement(String threadName, int sleepName, bool paramIsEnabled)
        {
            ThreadName = threadName;
            SleepTime = sleepName;
            IsEnabled = paramIsEnabled;
        }

        [ConfigurationProperty("ThreadName", IsRequired = true)]
        public String ThreadName
        {
            get
            { return (String)this["ThreadName"]; }
            set
            { this["ThreadName"] = value; }
        }

        [ConfigurationProperty("Enabled", IsRequired = true)]
        public bool IsEnabled
        {
            get { return (bool)this["Enabled"]; }
            set { this["Enabled"] = value; }
        }
	

        [ConfigurationProperty("SleepTime", DefaultValue = 100, IsRequired = true)]
        public int SleepTime
        {
            get
            { return (int)this["SleepTime"]; }
            set
            { this["SleepTime"] = value; }
        }
    }
}

