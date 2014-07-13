using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Caching.Common;
using Platforms.Tests.Common;

namespace Platform.Caching.Test
{
    [TestFixture]
	[ExcludeFromCodeCoverage]
	public class CacheSqlDependeciedTests : SqlTests
    {

        [SetUp]
        public void SetUp()
        {
            var comm =connection.CreateCommand();
            comm.CommandText = "IF NOT EXISTS(SELECT 1 FROM sys.tables where name ='TESTS1') BEGIN CREATE TABLE [ref].[Tests1] ([id] TINYINT  NULL, [tstamp] TIMESTAMP NULL,[Name] NVARCHAR(50) NULL, [Caption] NVARCHAR(100) NULL, [Description] NVARCHAR(400) NULL) END ";
            comm.ExecuteNonQuery();

            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (1) ";
            comm.ExecuteNonQuery();

            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (2) ";
            comm.ExecuteNonQuery();

            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (3) ";
            comm.ExecuteNonQuery();

            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (3) ";
            comm.ExecuteNonQuery();


            comm.CommandText = "IF NOT EXISTS(SELECT 1 FROM sys.tables where name ='TESTS2') BEGIN CREATE TABLE [ref].[Tests2] ([id] TINYINT NULL, [tstamp] TIMESTAMP NULL,[Name] NVARCHAR(50) NULL, [Color] NVARCHAR(100) NULL, [Voice] NVARCHAR(400) NULL) END ";
            comm.ExecuteNonQuery();
            
            connection.Close();
            SqlDependency.Start(connectionString);
            Thread.Sleep(500);

        }

        [TearDown]
        public void TearDown()
        {
            SqlDependency.Stop(connectionString);
            var comm = connection.CreateCommand();
            comm.CommandText = "DROP TABLE [ref].[TESTS1]";
            comm.ExecuteNonQuery();
            comm.CommandText = "DROP TABLE [ref].[TESTS2]";
            comm.ExecuteNonQuery();

            connection.Close();


        }


        [Test]
        public void PutGetShouldBeSame()
        {
            
            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";
            Object cachedObject = new Object();
            cache.Put(comm,cachedObject,"somethingKey");
            
            Assert.AreEqual(cachedObject, cache.Get("somethingKey"));
            Thread.Sleep(200);
            Assert.AreEqual(cachedObject, cache.Get("somethingKey"));

        }

        [Test]
        public void PutGetShouldBeSameManyKeys()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";
            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, "somethingKey",78,76,true);

            Assert.AreEqual(cachedObject, cache.Get("somethingKey",78,76,true));
            Thread.Sleep(200);
            Assert.AreEqual(cachedObject, cache.Get("somethingKey",78,76,true));

        }


        [Test]
        public void PutSqlChangeInsertGetShouldBeNull()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, "somethingKey");
            comm = connection.CreateCommand();
            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (100) ";
            Assert.IsNotNull(cache.Get("somethingKey"));
            comm.ExecuteNonQuery();
            
            Thread.Sleep(150);

            Assert.IsNull(cache.Get("somethingKey"));

        }


        [Test]
        public void PutSqlChangeInsertGetShouldBeNullManyKeys()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, true,7,false,"somethingKey");
            comm = connection.CreateCommand();
            comm.CommandText = "INSERT INTO [ref].[Tests1] ([id]) VALUES (100) ";
            Assert.IsNotNull(cache.Get(true, 7, false, "somethingKey"));
            comm.ExecuteNonQuery();

            Thread.Sleep(150);

            Assert.IsNull(cache.Get(true, 7, false, "somethingKey"));

        }



        [Test]
        public void PutSqlChangeInsertOtherTableGetShoulBeSame()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, 5);
            comm = connection.CreateCommand();
            comm.CommandText = "INSERT INTO [ref].[Tests2] ([id]) VALUES (100) ";
            comm.ExecuteNonQuery();
            Thread.Sleep(150);

            Assert.AreSame(cachedObject,cache.Get(5));

        }


        [Test]
        public void PutSqlChangeUpdateGetShoulBeNull()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";
            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, "somethingKey");
            comm = connection.CreateCommand();
            

            comm.CommandText = "UPDATE [ref].[Tests1] SET [Name] = 'Howard' WHERE [id]=1 ";
            comm.ExecuteNonQuery();
            Thread.Sleep(150);

            Assert.IsNull(cache.Get(cachedObject, "somethingKey"));

        }


        [Test]
        public void PutSqlChangeUpdateOtherRowGetShoulBeSame()
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=1 ";
            Object cachedObject = new Object();
            cache.Put(comm, cachedObject, "somethingKey");
            comm = connection.CreateCommand();


            comm.CommandText = "UPDATE [ref].[Tests1] SET [Caption] = 'Bastard' WHERE [id]=2 ";
            comm.ExecuteNonQuery();

            Thread.Sleep(150);

            Assert.AreSame(cachedObject,cache.Get( "somethingKey"));

        }

        [Test]
        public void PutManySqlChangeShoulBeNullOnlyForOne()
            //Все депенденси разные по ид
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=1 ";
            
            cache.Put(comm, new Object(), "somethingKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=2 ";
            cache.Put(comm, new Object(), "otherKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=3 ";

            cache.Put(comm, new Object(), 67);

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=4 ";

            cache.Put(comm, new Object(), 670);

            comm = connection.CreateCommand();
            comm.CommandText = "UPDATE [ref].[Tests1] SET [Caption] = 'Bastard' WHERE [id]=2 ";
            comm.ExecuteNonQuery();

            Thread.Sleep(150);

            Assert.IsNotNull(cache.Get("somethingKey"));
            Assert.IsNull(cache.Get("otherKey"));
            Assert.IsNotNull(cache.Get(67));
            Assert.IsNotNull(cache.Get(670));



        }

        [Test]
        public void PutManySqlChangeShoulBeNullOnlyForSubset()
            // некоторпые депенденси одинковые
        {

            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            cache.Put(comm, new Object(), "somethingKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=1 ";
            cache.Put(comm, new Object(), "otherKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=2 ";

            cache.Put(comm, new Object(), 67);

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=3 ";

            cache.Put(comm, new Object(), 670);


            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=4 ";

            cache.Put(comm, new Object(), 670, true);


            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            cache.Put(comm, new Object(), "Dog");

            comm = connection.CreateCommand();
            comm.CommandText = "UPDATE [ref].[Tests1] SET [Caption] = 'Bastard' WHERE [id]=2 ";
            comm.ExecuteNonQuery();

            Thread.Sleep(150);

            Assert.IsNull(cache.Get("somethingKey"));
            Assert.IsNotNull(cache.Get("otherKey"));
            Assert.IsNull(cache.Get(67));
            Assert.IsNotNull(cache.Get(670));
            Assert.IsNotNull(cache.Get(670,true));



        }

        [Test]
        public void PutManySameNoChangesShouldbeSameLast()
        {
            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            var o1 = new Object();
            cache.Put(comm, o1 , "somethingKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=1 ";
            var o2 = new Object();
            cache.Put(comm, o2, "otherKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=2 ";
            var o3 = new Object();
            cache.Put(comm, o3, 67);

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=3 ";
            var o4 = new Object();
            cache.Put(comm, o4, 670);

            Assert.AreSame(o4,cache.Get(670));



            
        }

        [Test]
        public void PutManySameChangesOnFirstPutShoulbeSameLast()
        {
            ICache cache = new Cache();
            var comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] ";

            var o1 = new Object();
            cache.Put(comm, o1, "somethingKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=1 ";
            var o2 = new Object();
            cache.Put(comm, o2, "otherKey");

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=2 ";
            var o3 = new Object();
            cache.Put(comm, o3, 67);

            comm = connection.CreateCommand();
            comm.CommandText = "SELECT id,tstamp,Name,Caption FROM [ref].[Tests1] WHERE [id]=3 ";
            var o4 = new Object();
            cache.Put(comm, o4, 670);

            comm = connection.CreateCommand();
            comm.CommandText = "UPDATE [ref].[Tests1] SET [Caption] = 'Bastard' WHERE [id]=2 ";
            comm.ExecuteNonQuery();


            Assert.AreSame(o4, cache.Get(670));




        }






    }
}
