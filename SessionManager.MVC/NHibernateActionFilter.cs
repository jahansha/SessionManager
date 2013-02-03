using System.Web.Mvc;

namespace SessionManager.MVC
{
    public class NHibernateActionFilter : ActionFilterAttribute
    {   
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionController = filterContext.Controller as SessionController;

            if (sessionController == null)
            {
                return;
            }

            sessionController.Session = GetSessionManager().GetCurrentSession();
        }

        public override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (filterContext.IsChildAction)
            {
                return;
            }

            if (filterContext.Exception != null &&
                !filterContext.ExceptionHandled)
            {
                GetSessionManager().Rollback();
                return;                
            }
            
            var sessionController = filterContext.Controller as SessionController;

            if (sessionController == null)
            {
                return;
            }          

            if (sessionController.ViewData.ModelState.IsValid)
            {
                GetSessionManager().Commit();
            }
            else
            {
                GetSessionManager().Rollback();
            }
        }

        protected virtual ISessionManager GetSessionManager()
        {
            return DependencyResolver.Current.GetService<ISessionManager>();
        }
    }
}
