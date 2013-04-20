using System.Web.Mvc;

namespace SessionManager.Mvc
{
    public enum CommitOnType
    {
        OnActionExecuted,
        OnResultExecuted
    }
    
    public class NHibernateActionFilter : ActionFilterAttribute
    {
        private bool _useSessionController = true;
        private CommitOnType _commitOn = CommitOnType.OnResultExecuted;

        public bool UseSessionController { get { return _useSessionController; } set { _useSessionController = value; } }
        public CommitOnType CommitOn { get { return _commitOn; } set { _commitOn = value; } }
        
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (UseSessionController)
            {
                InjectSessionIntoController(filterContext);
            }
        }

        public override void  OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (CommitOn != CommitOnType.OnActionExecuted)
            {
                return;
            }

            var controllerContextWapper = new ControllerContextWrapper
                {
                    ControllerContext = filterContext,
                    Exception = filterContext.Exception,
                    ExceptionHandled = filterContext.ExceptionHandled
                };

            Commit(controllerContextWapper);
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (CommitOn != CommitOnType.OnResultExecuted)
            {
                return;
            }

            var controllerContextWapper = new ControllerContextWrapper
                {
                    ControllerContext = filterContext,
                    Exception = filterContext.Exception,
                    ExceptionHandled = filterContext.ExceptionHandled
                };

            Commit(controllerContextWapper);
        }

        protected virtual void Commit(ControllerContextWrapper controllerContextWrapper)
        {
            if (controllerContextWrapper.ControllerContext.IsChildAction)
            {
                return;
            }

            if (controllerContextWrapper.Exception != null &&
                !controllerContextWrapper.ExceptionHandled)
            {
                GetSessionManager().Rollback();
                return;
            }

            var controller = controllerContextWrapper.ControllerContext.Controller;

            if (UseSessionController)
            {
                controller = controllerContextWrapper.ControllerContext.Controller as SessionController;

                if (controller == null)
                {
                    return;
                }
            }

            if (controller.ViewData.ModelState.IsValid)
            {
                GetSessionManager().Commit();
            }
            else
            {
                GetSessionManager().Rollback();
            }
        }

        protected virtual void InjectSessionIntoController(ActionExecutingContext filterContext)
        {
            var controller = filterContext.Controller as SessionController;

            if (controller == null)
            {
                return;
            }

            controller.Session = GetSessionManager().GetCurrentSession();
        }

        protected virtual ISessionManager GetSessionManager()
        {
            return DependencyResolver.Current.GetService<ISessionManager>();
        }
    }
}
