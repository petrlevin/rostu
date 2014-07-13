using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.Multilink;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.FactoryPattern;
using Platform.Common.Exceptions;
using Platform.Utils.FactoryPattern.Interfaces;

namespace Platform.Dal
{
	/// <summary>
	/// Фабрика построителей SQL-запросов. 
	/// Для получения конкретного построителя вызовите соответствующий виду запроса метод фабрики:
	/// Select, Insert, Update, Delete. 
	/// </summary>
	public class QueryFactory
	{
		private const string ErrMsg = "Произведена попытка получить построитель запроса для незарегистрированного типа сущности.";

		private enum queryType
		{
			Select,
			Insert,
			Update,
			Delete
		}

	    protected IManagedFactory<EntityType, ISelectQueryBuilder> selectFactory;
	    protected IManagedFactory<EntityType, IInsertQueryBuilder> insertFactory;
	            
	    protected IManagedFactory<EntityType, IUpdateQueryBuilder> updateFactory;
	    protected IManagedFactory<EntityType, IDeleteQueryBuilder> deleteFactory;
	            

	    protected virtual void InitFactories()
	    {
	        selectFactory =
	            new Factory<EntityType, ISelectQueryBuilder>();
	        insertFactory =
	            new Factory<EntityType, IInsertQueryBuilder>();
	        updateFactory =
	            new Factory<EntityType, IUpdateQueryBuilder>();
	        deleteFactory =
	            new Factory<EntityType, IDeleteQueryBuilder>();
	    }

	    /// <summary>
		/// Тип целевой сущности - для которой конструируется запрос.
		/// От типа сущности зависит тип возвращаемого построителя запроса.
		/// </summary>
		public EntityType EntityType { get; set; }

		/// <remarks>
		/// Любой построитель работает с целевой таблицей/view
		/// </remarks>
		public IEntity Entity;

		public QueryFactory(EntityType entityType, IEntity entity)
		{
            InitFactories();
			EntityType = entityType;
			Entity = entity;
			registerClasses();
		}

		public QueryFactory(IEntity entity)
            : this(entity.EntityType, entity)
		{
		}

		/// <summary>
		/// Получает построитель на выборку
		/// </summary>
		/// <returns></returns>
		public ISelectQueryBuilder Select()
		{
			return (ISelectQueryBuilder)getBuilder(queryType.Select);
		}

		/// <summary>
		/// Получает построитель на вставку
		/// </summary>
		/// <returns></returns>
		public IInsertQueryBuilder Insert()
		{
			return (IInsertQueryBuilder)getBuilder(queryType.Insert);
		}

		/// <summary>
		/// Получает построитель на обновление
		/// </summary>
		/// <returns></returns>
		public IUpdateQueryBuilder Update()
		{
			return (IUpdateQueryBuilder)getBuilder(queryType.Update);
		}

		/// <summary>
		/// Получает построитель на удаление
		/// </summary>
		/// <returns></returns>
		public IDeleteQueryBuilder Delete()
		{
			return (IDeleteQueryBuilder)getBuilder(queryType.Delete);
		}

		#region Private Methods

		/// <summary>
		/// Возвращает построитель в разрезе двух измерений <see cref="queryType"/> и <see cref="EntityType"/>.
		/// </summary>
		/// <param name="querytype"></param>
		/// <returns></returns>
		private IQueryBuilder getBuilder(queryType querytype)
		{
			IQueryBuilder builder = null;
			try
			{
				switch (querytype)
				{
					case queryType.Select:
						builder = selectFactory.Create(EntityType);
						break;
					case queryType.Insert:
						builder = insertFactory.Create(EntityType);
						break;
					case queryType.Update:
						builder = updateFactory.Create(EntityType);
						break;
					case queryType.Delete:
						builder = deleteFactory.Create(EntityType);
						break;
				}
			}
			catch (ArgumentException)
			{
				throw new PlatformException(ErrMsg);
			}

			if (builder != null)
				builder.Entity= Entity;
			return builder;
		}


		/// <summary>
		/// Установка соответствия между типами сущности и классами-конструкторами SQL запросов.
		/// </summary>
		private void registerClasses()
		{
			/* 
			 * Для каждого типа сущности регистрируем соответствующий класс-конструктор SQL запроса. 
			 * Установка такого соответствия проделывается для каждого вида запроса: select, insert, update, delete.
			 * Т.о. при необходимости можно выстроить иерархия соответствующего вида запроса для любой сущности, однако это и не обязательно
			 * - для нескольких типов сущности можно зарегистрировать один класс запроса.
			 */

			// Select
			selectFactory.Add<SelectQueryBuilder>(EntityType.Enum);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Reference);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Tablepart);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Document);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Registry);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Tool);
			selectFactory.Add<SelectQueryBuilder>(EntityType.Report);
			selectFactory.Add<MultilinkSelectQueryBuilder>(EntityType.Multilink);

			// Insert
			insertFactory.Add<InsertQueryBuilder>(EntityType.Reference);
			//insertFactory.Add<InsertQuery>(EntityType.TablePart);

			// Update
			updateFactory.Add<UpdateQueryBuilder>(EntityType.Reference);
			//updateFactory.Add<UpdateQuery>(EntityType.TablePart);

			// Delete
			deleteFactory.Add<DeleteQueryBuilder>(EntityType.Reference);
			//deleteFactory.Add<DeleteQuery>(EntityType.TablePart);
		}

		#endregion

	}
}
