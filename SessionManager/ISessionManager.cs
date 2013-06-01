using System.Data;
using NHibernate;

namespace SessionManager
{   
    public interface ISessionManager
    {
        ISession GetCurrentSession();
        void Commit();
        void Commit(ISession session);
        void DisposeOfSession();
        void DisposeOfSession(ISession session);
        void Rollback();
        void Rollback(ISession session);
        ISession OpenSession(IsolationLevel isolationLevel);
    }
}
