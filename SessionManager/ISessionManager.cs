using NHibernate;

namespace SessionManager
{
    public interface ISessionManager
    {
        ISession GetCurrentSession();
        void Commit();
        void DisposeOfSession();
        void Rollback();        
    }
}
