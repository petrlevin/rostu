using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Factoring.Strategies;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.DbClr
{
    /// <summary>
    /// Фабрика для создания базовых объектов
    /// не использует кэш 
    /// не использует DJ
    /// </summary>
    public static class Objects
    {
        /// <summary>
        /// Получает объект из базы данных по его идентификатору 
        /// </summary>
        /// <typeparam name="TObject">тип объекта</typeparam>
        /// <param name="dbConnection">подключение к базе данных</param>
        /// <param name="id">идентификатор</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static public TObject ById<TObject>(int id) where TObject : Metadata
        {
            return  GetFactory().ById<TObject>(id);

        }

        /// <summary>
        /// Получает объект из базы данных по его идентификатору 
        /// </summary>
        /// <typeparam name="TObject">тип объекта</typeparam>
        /// <param name="dbConnection">подключение к базе данных</param>
        /// <param name="name">имя</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        static public TObject ByName<TObject>(string name) where TObject : Metadata
        {
            return GetFactory().ByName<TObject>(name);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        static public TObject Create<TObject>() where TObject : Metadata
        {
            return GetFactory().Create<TObject>();

        }

        static private  Factory GetFactory()
        {
            var result = new Factory();
            lock (_connections)
            {
                if (!_connections.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                    result.FactoryStrategy = new CloseConnection(new SqlConnection("context connection=true"));
                else
                    result.FactoryStrategy = new Default(_connections[Thread.CurrentThread.ManagedThreadId].Connection);
                return result;
            }
        }

        static Objects()
        {
            Metadata.GetObjects = ()=> GetFactory();
        }

        static private readonly Dictionary<Int32, ConnectionUsing> _connections = new Dictionary<int, ConnectionUsing>();

        public static IDisposable UseConnection(SqlConnection connection)
        {
            if (_connections.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                throw new InvalidOperationException("Соединение уже используется");
            return  new ConnectionUsing(connection);

        }

        class ConnectionUsing : IDisposable
        {
            private readonly SqlConnection _connection;

            public ConnectionUsing(SqlConnection connection)
            {
                _connection = connection;
                _connections.Add(Thread.CurrentThread.ManagedThreadId,this);
            }

            public SqlConnection Connection
            {
                get { return _connection; }
            }

            public void Dispose()
            {
                lock (_connections)
                {
                    if (!_connections.ContainsKey(Thread.CurrentThread.ManagedThreadId))
                        throw new InvalidOperationException("Соединение уже не используется");
                    if (_connections[Thread.CurrentThread.ManagedThreadId]!=this)
                        throw new InvalidOperationException("Используется другое соединение");

                    _connections.Remove(Thread.CurrentThread.ManagedThreadId);
                }
            }
        }


    }
}
