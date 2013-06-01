using System.Data;
using NHibernate;
using NHibernate.Context;

namespace SessionManager
{
    public class SessionManager : ISessionManager
    {
        private readonly ISessionFactory _sessionFactory;
        
        public SessionManager(ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
        }

        public virtual void Commit()
        {
            var session = CurrentSessionContext.HasBind(_sessionFactory) ?
                _sessionFactory.GetCurrentSession() :
                null;
            
            Commit(session);
        }

        public virtual void Commit(ISession session)
        {      
            if (session == null)
            {
                return;
            }

            if (session.Transaction == null)
            {
                return;
            }

            if (!session.Transaction.IsActive)
            {
                return;
            }

            try
            {
                session.Transaction.Commit();
            }
            catch (HibernateException)
            {
                Rollback();

                throw;
            }
            finally
            {
                DisposeOfSession();
            }
        }

        public virtual void DisposeOfSession()
        {
            var session = CurrentSessionContext.HasBind(_sessionFactory) ?
                CurrentSessionContext.Unbind(_sessionFactory) :
                null;

            DisposeOfSession(session);
        }
 
        public virtual void DisposeOfSession(ISession session)
        {
            if (session == null)
            {
                return;
            }

            if (session.Transaction != null)
            {
                session.Transaction.Dispose();
            }

            session.Dispose();            
        }

        public virtual void Rollback()
        {
            var session = CurrentSessionContext.HasBind(_sessionFactory) ?
                _sessionFactory.GetCurrentSession() :
                null;

            Rollback(session);
        }

        public virtual void Rollback(ISession session)
        {
            if (session == null)
            {
                return;
            }

            if (session.Transaction == null)
            {
                return;
            }

            if (!session.Transaction.IsActive)
            {
                return;
            }

            session.Transaction.Rollback();

            DisposeOfSession(session);
        }

        public virtual ISession GetCurrentSession()
        {
            ISession session;

            if (CurrentSessionContext.HasBind(_sessionFactory))
            {
                session = _sessionFactory.GetCurrentSession();
            }
            else
            {
                session = OpenSession(IsolationLevel.Unspecified);
                CurrentSessionContext.Bind(session);
            }

            return session;
        }

        public virtual ISession OpenSession(IsolationLevel isolationLevel)
        {
            var session = _sessionFactory.OpenSession();

            session.BeginTransaction(isolationLevel);

            return session;
        }
    }
}
