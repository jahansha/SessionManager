using NHibernate;
using System;

namespace SessionManager
{   
    public interface ISessionManager
    {
        ISession GetCurrentSession();
        ISession GetCurrentSession(Action<ISession> sessionSetup);
        void Commit();
        void Commit(Action<ISession> afterCommit);
        void DisposeOfSession();
        void Rollback();        
    }
}
