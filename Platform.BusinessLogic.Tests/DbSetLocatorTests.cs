using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity;
using Platform.Caching;
using Platform.Caching.Common;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Rhino.Mocks;
using SomeBusiness;
using SomeBusiness.Reference;

namespace Platform.BusinessLogic.Tests
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class DbSetLocatorTests
    {

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {

            using (var c = new SomeTestContext())
            {
                c.Others.ToList();
            }
        }

        [Test]
        public void CorrectSetGot()
        {
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";

            using (DbContext c = new SomeTestContext())
            {
                ISimpleCache cache = new SimpleCache();
                var loc = new DbSetLocator(cache);
                var dbSet =loc.Set(c, entity);
                dbSet.Cast<Some>();

            }

        }


        [Test]
        public void CacheWasUsed()
        {
            var entity = MockRepository.GenerateStub<IEntity>();
            //entity.Stub(e => e.GenerateEntityClass).Return(true);
            entity.GenerateEntityClass = true;
            entity.GenerateEntityClass = true;
            entity.EntityType = EntityType.Reference;
            entity.Name = "Some";
            entity.Id= 100;

            ISimpleCache cache = MockRepository.GenerateMock<ISimpleCache>();
            cache.Stub(c => c.Get<Type>(typeof (SomeTestContext), 100)).Return(null);
            cache.Stub(c => c.Put(typeof(Some), typeof(SomeTestContext), 100));
            using (DbContext cont = new SomeTestContext())
            {

                var loc = new DbSetLocator(cache);
                var dbSet = loc.Set(cont, entity);





                cache.BackToRecord(BackToRecordOptions.All);
                cache.Replay();

                cache.Stub(c => c.Get<Type>(typeof(SomeTestContext), 100)).Return(typeof(Some));
                dbSet = loc.Set(cont, entity);
                dbSet.Cast<Some>();


            }

            cache.AssertWasCalled(c => c.Get<Type>(typeof(SomeTestContext), 100),op=>op.Repeat.Twice());
            cache.AssertWasCalled(c => c.Put(typeof(Some), typeof(SomeTestContext), 100),op=>op.Repeat.Once());



        }

    }
}
