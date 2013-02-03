using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(SessionManager.MVC.Nuget.App_Start.RegisterSessionManagerMVC), "PreStart")]

namespace SessionManager.MVC.Nuget.App_Start
{
    public static class RegisterSessionManagerMVC
    {
        public static void PreStart()
        {
            GlobalFilters.Filters.Add(new SessionManager.MVC.NHibernateActionFilter());
            DynamicModuleUtility.RegisterModule(typeof(SessionManager.MVC.SessionManagerCleanupModule));
        }
    }
}
