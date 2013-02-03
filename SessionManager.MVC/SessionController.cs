using NHibernate;
using System.Web;
using System.Web.Mvc;

namespace SessionManager.MVC
{
    public class SessionController : Controller
    {
        public HttpSessionStateBase HttpSession
        {
            get { return base.Session; }
        }

        public new ISession Session { get; set; }
    }
}
