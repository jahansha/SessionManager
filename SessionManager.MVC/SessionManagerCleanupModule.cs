using NHibernate;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SessionManager.Mvc
{
    public class SessionManagerCleanupModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.EndRequest += ContextEndRequest;
            context.Error += ContextError;
        }

        public void Dispose()
        {
        }

        private void ContextEndRequest(object sender, EventArgs e)
        {
            foreach (var sessionManager in GetSessionManagers())
            {
                sessionManager.DisposeOfSession();
            }
        }  
      
        private void ContextError(object sender, EventArgs e)
        {
            var exception = HttpContext.Current.Server.GetLastError();

            if (!(exception is HibernateException))
            {
                return;
            }

            foreach (var sessionManager in GetSessionManagers())
            {
                sessionManager.Rollback();
            }
        }

        private IEnumerable<ISessionManager> GetSessionManagers()
        {
            return DependencyResolver.Current.GetServices<ISessionManager>() ?? new List<ISessionManager>();
        }
    }
}
