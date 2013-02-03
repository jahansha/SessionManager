using NHibernate;
using NHibernate.Cfg;

namespace SessionManager
{
    public interface ISessionFactoryBuilder
    {
        Configuration ConfigureSessionFactory();
        ISessionFactory BuildSessionFactory();
    }
}
