using FluentAssertions;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SessionManager.Tests
{
    public class SessionManagerTests
    {
        protected ISessionFactory SessionFactory;
        protected SessionManager SessionManager;
        protected Configuration Cfg;
        
        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            var mapper = new ModelMapper();

            mapper.AddMappings(
                Assembly.GetAssembly(typeof(BlogMapping))
                .GetExportedTypes()
                .Where(x => x.Namespace == typeof(BlogMapping).Namespace));

            var domainMapping = mapper.CompileMappingFor(
                Assembly.GetAssembly(typeof(Blog))
                .GetExportedTypes()
                .Where(x => x.Namespace == typeof(Blog).Namespace));
            
            Cfg = new Configuration();

            Cfg.DataBaseIntegration(db =>
            {
                db.Dialect<NHibernate.Dialect.SQLiteDialect>();
                db.Driver<NHibernate.Driver.SQLite20Driver>();                
                db.LogSqlInConsole = true; 
                db.LogFormattedSql = true;
                db.ConnectionStringName = "AppDb";
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                db.SchemaAction = SchemaAutoAction.Update;
                db.ConnectionReleaseMode = ConnectionReleaseMode.OnClose;
            });

            Cfg.AddMapping(domainMapping);

            SessionFactory = Cfg.CurrentSessionContext<NHibernate.Context.ThreadStaticSessionContext>().BuildSessionFactory();
            SessionManager = new SessionManager(SessionFactory);
        }

        [SetUp]
        public void Setup()
        {
            new SchemaExport(Cfg).Execute(true, true, false, SessionManager.GetCurrentSession().Connection, Console.Out);
        }

        [Test]
        public void Can_Get_Currnet_Session()
        {
            var session = SessionManager.GetCurrentSession();
            var session2 = SessionManager.GetCurrentSession();

            session.Should().NotBeNull();
            session.IsOpen.Should().BeTrue();
            session.IsConnected.Should().BeTrue();
            session.ShouldBeEquivalentTo(session2);
            session.Transaction.IsActive.Should().BeTrue();
        }

        [Test]
        public void Can_Dispose_Of_Session()
        {
            // arrange
            var session = SessionManager.GetCurrentSession();

            var blog = new Blog { Title = "This is a Blog!" };

            session.Save(blog);

            var transaction = session.Transaction;

            // act
            SessionManager.DisposeOfSession();

            // assert
            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            session.Transaction.IsActive.Should().BeFalse();
            transaction.WasCommitted.Should().BeFalse();
        }

        [Test]
        public void Can_Commit_Session()
        {
            var session = SessionManager.GetCurrentSession();

            var blog = new Blog { Title = "This is a Blog!" };

            session.Save(blog);

            var transaction = session.Transaction;

            SessionManager.Commit();

            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            transaction.IsActive.Should().BeFalse();
            transaction.WasCommitted.Should().BeTrue();      
        }

        [Test]
        public void Can_Rollback_Session()
        {
            var session = SessionManager.GetCurrentSession();

            SessionManager.Rollback();

            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            session.Transaction.IsActive.Should().BeFalse();
        }

        [TearDown]
        public void TearDown()
        {
            SessionManager.DisposeOfSession();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            SessionFactory.Dispose();
        }
    }

    public class Blog
    {
        public virtual int Id { get; set; }
        public virtual String Title { get; set; }
        public virtual List<Post> Posts { get; set; }
    }

    public class Post
    {
        public virtual int Id { get; set; }
        public virtual String Body { get; set; }
        public virtual Blog Blog { get; set; }
    }

    public class BlogMapping : ClassMapping<Blog>
    {
        public BlogMapping()
        {
            Id(x => x.Id, x => x.Generator(Generators.Identity));
            Property(x => x.Title);
            Bag(x => x.Posts, x => { }, x => x.OneToMany());
        }
    }

    public class PostMapping : ClassMapping<Post>
    {
        public PostMapping()
        {
            Id(x => x.Id, x => x.Generator(Generators.Identity));
            Property(x => x.Body);
            ManyToOne(x => x.Blog);
        }
    }
}
