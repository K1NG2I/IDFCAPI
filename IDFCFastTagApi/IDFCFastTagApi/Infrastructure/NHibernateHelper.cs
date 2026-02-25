using System;
using System.Configuration;
using System.Web;
using NHibernate;
using NHibernate.Cfg;

namespace IDFCFastTagApi.Infrastructure
{
    public static class NHibernateHelper
    {
        private static readonly object SyncRoot = new object();
        private static ISessionFactory _sessionFactory;

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_sessionFactory == null)
                {
                    lock (SyncRoot)
                    {
                        if (_sessionFactory == null)
                        {
                            _sessionFactory = BuildSessionFactory();
                        }
                    }
                }

                return _sessionFactory;
            }
        }

        private static ISessionFactory BuildSessionFactory()
        {
            var cfg = new NHibernate.Cfg.Configuration();

            string configPath;
            if (HttpContext.Current != null)
            {
                configPath = HttpContext.Current.Server.MapPath("~/App_Data/hibernate.cfg.xml");
            }
            else
            {
                configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "hibernate.cfg.xml");
            }

            cfg.Configure(configPath);

            var connStr = ConfigurationManager.ConnectionStrings["FastagDb"].ConnectionString;
            cfg.SetProperty("connection.connection_string", connStr);

            return cfg.BuildSessionFactory();
        }

        public static ISession OpenSession()
        {
            return SessionFactory.OpenSession();
        }
    }
}
