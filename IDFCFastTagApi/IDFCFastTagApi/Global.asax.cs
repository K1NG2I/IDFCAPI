using System;
using System.Web;
using System.Web.Http;
using RSuite.Infrastructure.Core.Base;
using RSuite.Infrastructure.Core.Messaging;
using RSuite.Infrastructure.Core.Logger;
using RSuite.UserInterface.Web.Mvc;
using RSuite.UserInterface.Web.Mvc.AppCode.Common.Factory;
using StructureMap;
using IDFCFastTagApi.Services;

namespace IDFCFastTagApi
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            // Initialize full container
            MvcBootstrapper.Init();

            // IMPORTANT: register service immediately AFTER Init
            StructureMap.ObjectFactory.Container.Configure(cfg =>
            {
                cfg.For<IFastagService>().Use<FastagService>();
            });

            GlobalConfiguration.Configuration.DependencyResolver =
                new StructureMapDependencyResolver(
                    StructureMap.ObjectFactory.Container
                );
        }

        protected void Application_PostAuthorizeRequest()
        {
            if (HttpContext.Current != null)
            {
                HttpContext.Current.SetSessionStateBehavior(
                    System.Web.SessionState.SessionStateBehavior.Required);
            }
        }


        protected void Application_EndRequest(object sender, EventArgs e)
        {
            var unit = EntityFactory.GetInstance<IUnitOfWork>();
            var messageCarrier = EntityFactory.GetInstance<IMessageCarrier>();

            if (!messageCarrier.ContainsCritialError())
            {
                unit.Commit();
            }
            else
            {
                unit.RollBack();
            }

            unit.Dispose();
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var unit = EntityFactory.GetInstance<IUnitOfWork>();
            var logger = EntityFactory.GetInstance<IErrorLogger>();

            unit.RollBack();

            Exception ex = Server.GetLastError();
            logger.LogError(null, ex);
        }
    }
}