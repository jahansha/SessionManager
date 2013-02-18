using NHibernate;
using NHibernate.Context;
using System;

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
            Commit(x => x.Dispose());
        }

        public virtual void Commit(Action<ISession> afterCommit)
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
            catch (HibernateException)
            {
                Rollback();

                DisposeOfSession();

                throw;
            }

            if (afterCommit != null)
            {
                afterCommit(session);
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

            if (session.Transaction != null)
            {
                session.Transaction.Dispose();
            }

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

            DisposeOfSession();
        }

        public virtual ISession GetCurrentSession()
        {
            return GetCurrentSession(x => {});
        }

        public virtual ISession GetCurrentSession(Action<ISession> sessionSetup)
        {
            ISession session = null;

            if (CurrentSessionContext.HasBind(sessionFactory))
            {
                session = sessionFactory.GetCurrentSession();
            }
            else
            {
                session = sessionFactory.OpenSession();
                CurrentSessionContext.Bind(session);
            }

            sessionSetup(session);

            if (session.Transaction == null || !session.Transaction.IsActive)
            {
                session.BeginTransaction();
            }

            return session;
        }
    }
}
