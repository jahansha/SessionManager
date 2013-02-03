using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace SessionManager.MVC
{
    public class SessionManagerCleanupModule : IHttpModule
    {
        private HttpApplication httpApplication;

        public void Dispose()
        {
            httpApplication.EndRequest -= ContextEndRequest;
        }

        public void Init(HttpApplication context)
        {
            httpApplication = context;
            context.EndRequest += ContextEndRequest;
        }

        private void ContextEndRequest(object sender, EventArgs e)
        {
            foreach (var sessionManager in GetSessionManagers())
            {
                sessionManager.DisposeOfSession();
            }            
        }        

        private IEnumerable<ISessionManager> GetSessionManagers()
        {
            var sessionManagers = DependencyResolver.Current.GetServices<ISessionManager>();

            return sessionManagers ?? new List<ISessionManager>();
        }
    }
}
