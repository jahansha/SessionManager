using NHibernate;
using NHibernate.Cfg;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.Mapping.ByCode;
using System.Reflection;
using NHibernate.Tool.hbm2ddl;

namespace SessionManager.Tests
{
    public class SessionManagerTests
    {
        protected ISessionFactory sessionFactory;
        protected SessionManager sessionManager;
        protected Configuration cfg;
        
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
            
            cfg = new Configuration();

            cfg.DataBaseIntegration(db =>
            {
                db.Dialect<global::NHibernate.Dialect.SQLiteDialect>();
                db.Driver<global::NHibernate.Driver.SQLite20Driver>();                
                db.LogSqlInConsole = true; 
                db.LogFormattedSql = true;
                db.ConnectionStringName = "AppDb";
                db.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
                db.SchemaAction = SchemaAutoAction.Update;
               
            });

            cfg.AddMapping(domainMapping);

            sessionFactory = cfg.CurrentSessionContext<NHibernate.Context.ThreadStaticSessionContext>().BuildSessionFactory();
            sessionManager = new SessionManager(sessionFactory);
        }

        [SetUp]
        public void Setup()
        {
            new SchemaExport(cfg).Execute(true, true, false, sessionManager.GetCurrentSession().Connection, Console.Out);
        }

        [Test]
        public void Can_Get_Currnet_Session()
        {
            var session = sessionManager.GetCurrentSession();
            var session2 = sessionManager.GetCurrentSession();

            session.Should().NotBeNull();
            session.IsOpen.Should().BeTrue();
            session.IsConnected.Should().BeTrue();
            session.ShouldBeEquivalentTo(session2);
            session.Transaction.IsActive.Should().BeTrue();
        }

        [Test]
        public void Can_Dispose_Of_Session()
        {
            // arranage
            var session = sessionManager.GetCurrentSession();

            var blog = new Blog() { Title = "This is a Blog!" };

            session.Save(blog);

            var transaction = session.Transaction;

            // act
            sessionManager.DisposeOfSession();

            // assert
            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            session.Transaction.IsActive.Should().BeFalse();
            transaction.WasCommitted.Should().BeFalse();
        }

        [Test]
        public void Can_Commit_Session()
        {
            // arrange
            var session = sessionManager.GetCurrentSession();
            
            var blog = new Blog() { Title = "This is a Blog!" };
            
            session.Save(blog);

            var transaction = session.Transaction;

            // act     
            sessionManager.Commit(null);

            var wasCommitted = transaction.WasCommitted;
            transaction = null;

            session = sessionManager.GetCurrentSession();

            var blogs = session.QueryOver<Blog>().List();

            sessionManager.DisposeOfSession();

            // assert
            wasCommitted.Should().BeTrue();
            blogs.Count.Should().Be(1);

            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            session.Transaction.IsActive.Should().BeFalse();
        }

        [Test]
        public void Can_Rollback_Session()
        {
            var session = sessionManager.GetCurrentSession();

            sessionManager.Rollback();

            session.IsOpen.Should().BeFalse();
            session.IsConnected.Should().BeFalse();
            session.Transaction.IsActive.Should().BeFalse();
        }

        [Test]
        public void Can_Disconnect_On_Commit()
        {
            var session = sessionManager.GetCurrentSession();

            sessionManager.Commit(x => x.Disconnect());

            session.IsOpen.Should().BeTrue();
            session.IsConnected.Should().BeFalse();
        }

        [Test]
        public void Can_Reconnect_CurrentSession()
        {
            var session = sessionManager.GetCurrentSession();

            sessionManager.Commit(x => x.Disconnect());

            session = sessionManager.GetCurrentSession(x => x.Reconnect());

            session.IsConnected.Should().BeTrue();
            session.IsOpen.Should().BeTrue();
        }

        [TearDown]
        public void TearDown()
        {
            sessionManager.DisposeOfSession();
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            sessionFactory.Dispose();
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
            this.Id(x => x.Id, x => x.Generator(Generators.Identity));
            this.Property(x => x.Title);
            this.Bag(x => x.Posts, x => { }, x => x.OneToMany());
        }
    }

    public class PostMapping : ClassMapping<Post>
    {
        public PostMapping()
        {
            this.Id(x => x.Id, x => x.Generator(Generators.Identity));
            this.Property(x => x.Body);
            this.ManyToOne(x => x.Blog);
        }
    }
}
