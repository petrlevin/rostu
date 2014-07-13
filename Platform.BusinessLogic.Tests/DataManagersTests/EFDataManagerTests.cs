using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NUnit.Framework;
using Platform.BusinessLogic.Activity;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Activity.Operations;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityFramework;
using Platform.BusinessLogic.Exceptions;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Interfaces;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Unity;
using Platform.Utils.Collections;
using Platforms.Tests.Common;
using Rhino.Mocks;
using Sbor.Document;
using Sbor.Reference;
using SomeBusiness;
using SomeBusiness.Reference;
using Platform.Common;


namespace Platform.BusinessLogic.Tests.DataManagersTests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class EFDataManagerTests : SqlTestBase
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            DbContextInitializer.TraceEnabled = false;
            SqlDependency.Start(connectionString);
        }


        public static IEnumerable ContextFabrica
        {
            get
            {
                yield return new TestCaseData(new Func<DbContext>(() => new SomeTestContext()), new Action(() => _IoC(typeof(SomeTestContext)))).SetName("Тестовый контекст");
                yield return new TestCaseData(new Func<DbContext>(() => new BussinessTestContext()), new Action(() => _IoC(typeof(BussinessTestContext)))).SetName("Бизнес контекст");
            }
        }


        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            using (var c = new SomeTestContext())
            {
                if (c.Database.Exists())
                {
                    c.Database.Delete();
                }
                c.Database.Create();
            }

            using (var c = new BussinessTestContext())
            {
                if (c.Database.Exists())
                {
                    c.Database.Delete();
                }
                c.Database.Create();
            }
        }


        private static void _IoC(Type contextType)
        {
            Platform.Common.IoC.Dispose();
            UnityContainer uc = new UnityContainer();

            uc.RegisterInstance(typeof(DbContext), Activator.CreateInstance(contextType));
            Platform.PrimaryEntities.Factoring.DependencyInjection.RegisterIn(uc, false, false);
            uc.RegisterType(typeof(object), typeof(TestNumerator), "DeafaultValues");
            Platform.Common.IoC.InitWith(new DependencyResolverBase(uc));
        }


        [TearDown]
        public void TearDown()
        {
            using (var c = new SomeTestContext())
            {
                c.Others.ToList().ForEach(e => c.Others.Remove(e));
                c.Somes.ToList().ForEach(e => c.Somes.Remove(e));
                c.SaveChanges();
            }
            using (var c = new BussinessTestContext())
            {
                c.Others.ToList().ForEach(e => c.Others.Remove(e));
                c.Somes.ToList().ForEach(e => c.Somes.Remove(e));
                c.SaveChanges();
            }
            SqlDependency.Stop(connectionString);
        }


        [Test]
        public void ToolExp()
        {
            IoC.Dispose();
            UnityContainer uc = new UnityContainer();

            uc.RegisterInstance(typeof(ICache), "Cache", new Cache());
            uc.RegisterInstance(typeof(DbContext), new Sbor.DataContext("PlatformDBConnectionString"));
            Platform.PrimaryEntities.Factoring.DependencyInjection.RegisterIn(uc, false, false, connectionString);
            uc.RegisterType(typeof(object), typeof(TestNumerator), "DeafaultValues");
            IoC.InitWith(new DependencyResolverBase(uc));

            using (var cont = new Sbor.DataContext("PlatformDBConnectionString"))
            {
                var a = cont.DocumentsOfSED.FirstOrDefault();
                //Assert.AreSame(a, cont.DocumentsOfSED.Where(d => d.Number == "hhg6788"));
                PlatformException ex = new ToolStatusException("hhhgh", a);
                var s = ex.ToString();
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void NewEntityAdded_LocatingTypeMocked(Func<DbContext> contextFabrica, Action init)
        {
            init();
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            using (var c = contextFabrica())
            {
                var entityManager = MockRepository.GeneratePartialMock<EntityManagerMock>(entity);
                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase(),entityManager);
                entityManager.Stub(m => m.DoGetDbSet(c)).IgnoreArguments().Return(null).WhenCalled(mi =>
                    {
                        mi.ReturnValue = (DbSet)((ITestContext)mi.Arguments[0]).Somes;
                    });
                var values = new IgnoreCaseDictionary<object>() { { "Name", "Petr" } };
                manager.CreateEntry(values);
            }
            using (var c = contextFabrica())
            {
                Assert.IsNotNull(((ITestContext)c).Somes.FirstOrDefault());
                Assert.AreEqual("Petr", ((ITestContext)c).Somes.First().Name);
            }
        }


        [Test]
        [TestCaseSource("ContextFabrica")]
        public void GetDefBase(Func<DbContext> contextFabrica, Action init)
        {
            init();
            IEntity entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id = 1;
            IEntityField ef = MockRepository.GenerateStub<IEntityField>();
			ef.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Application;
            ef.Name = "ActiveColor";
            ef.DefaultValue = "{Color}";
            entity.Fields = new List<IEntityField>()
	            {
		            ef
	            };
            using (var c = contextFabrica())
            {

                

                var manager = new EFDataManager((SqlConnection)c.Database.Connection, entity);

                var dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("ActiveColor"));
                Assert.AreEqual("Green", dict["ActiveColor"]);
            }
        }


        [Test]
        [TestCaseSource("ContextFabrica")]
        public void GetDef(Func<DbContext> contextFabrica, Action init)
        {
            init();
            using (var c = contextFabrica())
            {
                c.Database.Connection.Open();
                IEntity entity = MockRepository.GenerateStub<IEntity>();
                //entity.Stub(e => e.GenerateEntityClass).Return(true);
                entity.GenerateEntityClass = true;

                entity.EntityType = EntityType.Reference;
                entity.Name = "Other";
                entity.Id = 2;
                IEntityField ef1 = MockRepository.GenerateStub<IEntityField>();
				ef1.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Application;
                ef1.Name = "Color";
                ef1.DefaultValue = "{Color}";

                IEntityField ef2 = MockRepository.GenerateStub<IEntityField>();
				ef2.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Application;
                ef2.Name = "Name";
                ef2.DefaultValue = "{Color}";


                entity.Fields = new List<IEntityField>()
                    {
                        ef1,ef2
                    };

                var manager = new EFDataManager((SqlConnection)c.Database.Connection, entity);

                var dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("Color"));
                Assert.That(dict.ContainsKey("Name"));
                Assert.That(dict.ContainsKey("Counter"));
                Assert.AreEqual("Green", dict["Color"]);
                Assert.AreEqual("Ivan", dict["Name"]);
                Assert.AreEqual(1, dict["Counter"]);

                IEntityField ef3 = MockRepository.GenerateStub<IEntityField>();
				ef3.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Application;
                ef3.Name = "Counter";
                ef3.DefaultValue = "{Number}";

                entity.Fields = new List<IEntityField>()
                    {
                        ef1,ef2 ,ef3
                    };

                //var manager = new EFDataManager((SqlConnection)c.Database.Connection, entity);

                dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("Color"));
                Assert.That(dict.ContainsKey("Name"));
                Assert.That(dict.ContainsKey("Counter"));
                Assert.AreEqual("Green", dict["Color"]);
                Assert.AreEqual("Ivan", dict["Name"]);
                Assert.AreEqual(6, dict["Counter"]);

                ef3.DefaultValue = "{Number100}";

                dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("Color"));
                Assert.That(dict.ContainsKey("Name"));
                Assert.That(dict.ContainsKey("Counter"));
                Assert.AreEqual("Green", dict["Color"]);
                Assert.AreEqual("Ivan", dict["Name"]);
                Assert.AreEqual(100, dict["Counter"]);

				ef3.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Sql;
                ef3.DefaultValue = "51";

                dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("Color"));
                Assert.That(dict.ContainsKey("Name"));
                Assert.That(dict.ContainsKey("Counter"));
                Assert.AreEqual("Green", dict["Color"]);
                Assert.AreEqual("Ivan", dict["Name"]);
                Assert.AreEqual(52, dict["Counter"]);

				ef3.IdFieldDefaultValueType = (byte)FieldDefaultValueType.Application;
                ef3.DefaultValue = "{Number}+200";

                ef1.DefaultValue = "'Extra' +'{Color}'";

                dict = manager.GetDefaults();
                Assert.That(dict.ContainsKey("Color"));
                Assert.That(dict.ContainsKey("Name"));
                Assert.That(dict.ContainsKey("Counter"));
                Assert.AreEqual("ExtraGreen", dict["Color"]);
                Assert.AreEqual("Ivan", dict["Name"]);
                Assert.AreEqual(206, dict["Counter"]);
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void NewEntityAdded(Func<DbContext> contextFabrica, Action init)
        {
            init();

            IEntity entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id = 1;

            using (var c = contextFabrica())
            {
                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase());
                var values = new IgnoreCaseDictionary<object>() { { "Name", "Petr" } };
                manager.CreateEntry(values);
            }
            using (var c = contextFabrica())
            {
                Assert.IsNotNull(((ITestContext)c).Somes.FirstOrDefault());
                Assert.AreEqual("Petr", ((ITestContext)c).Somes.First().Name);
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void EntityUpdated_LocatingTypeMocked(Func<DbContext> contextFabrica, Action init)
        {
            init();
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            int id;

            using (var c = contextFabrica())
            {
                ((ITestContext)c).Somes.Add(new Some() { Name = "Simon" });
                var some = ((ITestContext)c).Somes.Add(new Some() { Name = "Jane" });
                c.SaveChanges();

                id = some.Id;


                var entityManager = MockRepository.GeneratePartialMock<EntityManagerMock>(entity);

                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase(), entityManager);
                entityManager.Stub(m => m.DoGetDbSet(c)).IgnoreArguments().Return(null).WhenCalled(mi =>
                {
                    mi.ReturnValue = (DbSet)
                        ((ITestContext)
                         mi.Arguments[0]).Somes;
                });
                var values = new Dictionary<string, object>() { { "Name", "Petr" } };
                manager.UpdateEntry(id, values);
            }
            using (var c = contextFabrica())
            {
                Assert.AreEqual("Petr", ((ITestContext)c).Somes.Find(id).Name);
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void EntityUpdated(Func<DbContext> contextFabrica, Action init)
        {
            init();
            IEntity entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id = 1;

            int id;

            using (var c = contextFabrica())
            {
                ((ITestContext)c).Somes.Add(new Some() { Name = "Simon" });
                var some = ((ITestContext)c).Somes.Add(new Some() { Name = "Jane" });
                c.SaveChanges();
                id = some.Id;

                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase());
                var values = new Dictionary<string, object>() { { "Name", "Petr" } };
                manager.UpdateEntry(id, values);
            }
            using (var c = contextFabrica())
            {
                Assert.AreEqual("Petr", ((ITestContext)c).Somes.Find(id).Name);
            }
        }


        [Test]
        [TestCaseSource("ContextFabrica")]
        public void ControlPassesUpdatedBeforeSaved(Func<DbContext> contextFabrica, Action init)
        {
            init();

            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Other";
            entity.Id = 2;

            int kentId;
            int lmId;

            using (var c = contextFabrica())
            {
                var kent = ((ITestContext)c).Others.Add(new Other() { Name = "Kent" });
                var lm = ((ITestContext)c).Others.Add(new Other() { Name = "LM" });
                c.SaveChanges();
                kentId = kent.Id;
                lmId = lm.Id;

                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase());
                var values = new Dictionary<string, object>() { { "Name", "Malboro" } };
                manager.UpdateEntry(kentId, values);
            }
            using (var c = contextFabrica())
            {
                Assert.AreEqual(1, ((ITestContext)c).Others.Find(kentId).Counter);
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void EntityDeleted_LocatingTypeMocked(Func<DbContext> contextFabrica, Action init)
        {
            init();
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            int id;

            using (var c = contextFabrica())
            {
                ((ITestContext)c).Somes.Add(new Some() { Name = "Simon" });
                var some = ((ITestContext)c).Somes.Add(new Some() { Name = "Jane" });
                c.SaveChanges();
                id = some.Id;

                var entityManager = MockRepository.GeneratePartialMock<EntityManagerMock>(entity);
                var manager = new EFDataManagerMock((SqlConnection)c.Database.Connection, entity, new ControlDispatcherBase(), entityManager);
                entityManager.Stub(m => m.DoGetDbSet(c)).IgnoreArguments().Return(null).WhenCalled(mi =>
                {
                    mi.ReturnValue = (DbSet)
                        ((ITestContext)
                         mi.Arguments[0]).Somes;
                });

                manager.DeleteItem(new int[] { id });
            }
            using (var c = contextFabrica())
            {
                Assert.IsNull(((ITestContext)c).Somes.Find(id));
            }
        }


        [Test]
        [Ignore]
        public void OperationExecuted()
        {
            _IoC(typeof(BussinessTestContext));
            var entity = new Entity();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            entity.EntityType = EntityType.Reference;
            entity.Name = "Other";
            entity.Id = 2;

            int id;

            using (var c = new BussinessTestContext())
            {

                c.Others.Add(new Other() { Name = "Simon" });
                var other = c.Others.Add(new Other() { Name = "Jane" });

                var op = new Operation() { Name = "Clean" };
                c.Operation.Add(op);
                var eop = new EntityOperation()
                    {
                        Operation = op,
                        Entity = entity
                    };
                c.EntityOperation.Add(eop);
                c.SaveChanges();

                id = other.Id;
                DbContext usedContext;

                var manager = new ToolsDataManager((SqlConnection)c.Database.Connection, entity);
                manager.ExecuteOperation(id, eop.Id);
            }

            using (var c = new BussinessTestContext())
            {
                Assert.AreEqual(1, ((ITestContext)c).Others.Find(id).Counter);
            }
        }


        [Test]
        [TestCaseSource("ContextFabrica")]
        [Repeat(1)]
        public void OperationExecuted_OperationDispatcherMocked(Func<DbContext> contextFabrica, Action init)
        {
            init();

            var entity = new Entity();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;

            entity.EntityType = EntityType.Reference;
            entity.Name = "Other";
            entity.Id = 2;

            int id;

            using (var c = contextFabrica())
            {
                ((ITestContext)c).Others.Add(new Other() { Name = "Simon" });
                var other = ((ITestContext)c).Others.Add(new Other() { Name = "Jane" });
                c.SaveChanges();

                id = other.Id;
            }
            using (var c = contextFabrica())
            {
                var t = c.Configuration.AutoDetectChangesEnabled;
                var disp = MockRepository.GeneratePartialMock<OperationDispatcherMock>();

                var manager = new ToolsDataManagerMock((SqlConnection)c.Database.Connection, entity);
                disp.Stub(d => d.DoGetOperation(0)).IgnoreArguments()
                       .Return(new EntityOperation()
                                   {
                                       Id = 100,
                                       Operation = new Operation() { Id = 400, Name = "Clean" }
                                   });

                manager.ExecuteOperation(id, 100);
            }

            using (var c = contextFabrica())
            {
                Assert.AreEqual(1, ((ITestContext)c).Others.Find(id).Counter);
            }
        }


        [Test]
        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void EntityDeleted(Func<DbContext> contextFabrica, Action init)
        {
            init();

            //var entity = Objects.Create<Entity>();
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id = 1;

            int id;
            using (var c = contextFabrica())
            {
                ((ITestContext)c).Somes.Add(new Some() { Name = "Simon" });
                var some = ((ITestContext)c).Somes.Add(new Some() { Name = "Jane" });
                c.SaveChanges();
                id = some.Id;
                var manager = new EFDataManager((SqlConnection)c.Database.Connection, entity);

                manager.DeleteItem(new int[] { id });
            }
            using (var c = contextFabrica())
            {
                Assert.IsNull(((ITestContext)c).Somes.Find(id));
            }
        }


        [Test]
        //        [Repeat(2)]
        [TestCaseSource("ContextFabrica")]
        public void EntitiesDeleted(Func<DbContext> contextFabrica, Action init)
        {
            init();

            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id = 1;

            Some simon;
            Some jane;
            Some paul;

            using (var c = contextFabrica())
            {
                simon = ((ITestContext)c).Somes.Add(new Some() { Name = "Simon" });
                jane = ((ITestContext)c).Somes.Add(new Some() { Name = "Jane" });
                paul = ((ITestContext)c).Somes.Add(new Some() { Name = "Paul" });
                c.SaveChanges();

                var manager = new EFDataManager((SqlConnection)c.Database.Connection, entity);

                manager.DeleteItem(new int[] { simon.Id, jane.Id });
            }
            using (var c = contextFabrica())
            {
                Assert.IsNull(((ITestContext)c).Somes.Find(simon.Id));
                Assert.IsNull(((ITestContext)c).Somes.Find(jane.Id));
                Assert.IsNotNull(((ITestContext)c).Somes.Find(paul.Id));
            }
        }



        public class EFDataManagerMock : EFDataManager
        {
            public EFDataManagerMock(SqlConnection dbConnection, IEntity source, IControlDispatcher controlDispatcher, EntityManager entityManager = null) : base(dbConnection, source,entityManager)
            {
            }

            protected override void ValidateRequiredField(Dictionary<String, object> values,
                                                         bool onlyExistedInSource = false)
            {
                
            }
        }

        public class ToolsDataManagerMock:ToolsDataManager
        {
            public ToolsDataManagerMock(SqlConnection dbConnection, IEntity source) : base(dbConnection, source)
            {
            }

            protected override void ValidateRequiredField(Dictionary<String, object> values,
                                                         bool onlyExistedInSource = false)
            {

            }

        }

        public abstract class EntityManagerMock : EntityManager
        {

            public EntityManagerMock(IEntity source)
                : base(source)
            {

            }

            protected override DbSet GetDbSet(DbContext dbContext)
            {
                return DoGetDbSet(dbContext);
            }

            public abstract DbSet DoGetDbSet(DbContext dbContext);
        }


        abstract public class OperationDispatcherMock : OperationDispatcherBase
        {
            protected OperationDispatcherMock(DbContext dbContext)
                : base(dbContext)
            {
            }

            protected OperationDispatcherMock()
                : base(null)
            {
            }

            protected override EntityOperation GetOperation(int entityOperationId)
            {
                return DoGetOperation(entityOperationId);
            }

            protected override void BlockDocument(EntityTypes.ToolEntity document)
            {

            }

            protected override void CheckBeginState(EntityTypes.ToolEntity document, EntityOperation entityOperation)
            {

            }

            protected override void CheckFinalStatus(EntityTypes.ToolEntity document, EntityOperation entityOperation)
            {

            }

            public abstract EntityOperation DoGetOperation(int entityOperationId);
        }


        public class TestNumerator
        {
            public string Color()
            {
                return "Green";
            }

            public int Number
            {
                get { return 5; }
            }

            public int Number100
            {
                get { return 100; }
            }
        }
    }
}
