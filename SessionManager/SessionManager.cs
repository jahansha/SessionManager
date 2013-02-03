using NHibernate;
using NHibernate.Context;

namespace SessionManager
{
    public class SessionManager : ISessionManager
    {
        protected readonly ISessionFactory sessionFactory;
        
        public SessionManager(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        public virtual void Commit()
        {
            var session = GetCurrentSession();
            
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
            catch(HibernateException)
            {
                Rollback();

                DisposeOfSession();

                throw;
            }
        }

        public virtual void DisposeOfSession()
        {
            if (sessionFactory == null)
            {
                return;
            }

            var session = CurrentSessionContext.HasBind(sessionFactory) ? 
                sessionFactory.GetCurrentSession() :
                null;

            if (session == null)
            {
                return;
            }

            session = CurrentSessionContext.Unbind(sessionFactory);

            session.Dispose();            
        }

        public virtual void Rollback()
        {
            if (sessionFactory == null)
            {
                return;
            }

            var session = GetCurrentSession();

            if (session.Transaction == null)
            {
                return;
            }

            if (!session.Transaction.IsActive)
            {
                return;
            }

            session.Transaction.Rollback();
        }

        public virtual ISession GetCurrentSession()
        {
            if (CurrentSessionContext.HasBind(sessionFactory))
            {
                return sessionFactory.GetCurrentSession();
            }

            return OpenSession();
        }

        protected virtual ISession OpenSession()
        {
            var session = sessionFactory.OpenSession();

            session.BeginTransaction();

            CurrentSessionContext.Bind(session);

            return session;
        }
    }
}
