using NHibernate;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SessionManager.Mvc
{
    public class SessionManagerCleanupModule : IHttpModule
    {
        private HttpApplication _httpApplication;

        public void Init(HttpApplication context)
        {
            _httpApplication = context;
            context.EndRequest += ContextEndRequest;
            context.Error += ContextError;
        }

        public void Dispose()
        {
            _httpApplication.EndRequest -= ContextEndRequest;
            _httpApplication.Error -= ContextError;
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
