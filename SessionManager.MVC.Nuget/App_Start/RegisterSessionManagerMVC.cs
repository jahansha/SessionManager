using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using System.Web.Mvc;

[assembly: WebActivator.PreApplicationStartMethod(typeof(SessionManager.MVC.Nuget.App_Start.RegisterSessionManagerMvc), "PreStart")]

namespace SessionManager.MVC.Nuget.App_Start
{
    public static class RegisterSessionManagerMvc
    {
        public static void PreStart()
        {
            GlobalFilters.Filters.Add(new NHibernateActionFilter());
            DynamicModuleUtility.RegisterModule(typeof(SessionManagerCleanupModule));
        }
    }
}
