using System;
using System.Web.Mvc;

namespace SessionManager.Mvc
{
    public class ControllerContextWrapper
    {
        public ControllerContext ControllerContext { get; set; }
        public Exception Exception { get; set; }
        public bool ExceptionHandled { get; set; }
    }
}
