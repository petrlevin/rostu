using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Configuration;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Microsoft.Practices.Unity;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Dal.Interfaces;
using Platform.Dal.Requirements;
using Platform.Log;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils.Orderable;

namespace Platform.Dal.QueryBuilders
{
	/// <summary>
	/// Базовый класс для выражений SELECT, INSERT, UPDATE, DELETE
	/// </summary>
	public abstract class QueryBuilder: IQueryBuilder
	{

        static QueryBuilder()
        {
            TSqlStatementDecorator.Decorated+=(decorator, source, querybuilder) =>
            decorator.Log(source);
        }

		protected List<TSqlStatementDecorator> BeforePrivateDecorators;

		protected List<TSqlStatementDecorator> AfterPrivateDecorators;

		protected QueryBuilder(IEntity entity)
		{
			BeforePrivateDecorators = new List<TSqlStatementDecorator>();
			AfterPrivateDecorators = new List<TSqlStatementDecorator>();
			QueryDecorators = new List<TSqlStatementDecorator>();
			QueryValidators = new List<ITSqlStatementValidator>();
		    Entity = entity;
            InitPrivateDecorators();
		}

		private List<TSqlStatementDecorator> _allDecorators;
		/// <summary>
		/// Все декораторы (публичные и приватные), расположенные в нужном порядке применения
		/// </summary>
		protected List<TSqlStatementDecorator> AllDecorators
		{
			get
			{
				if (_allDecorators == null)
				{
					_allDecorators = new List<TSqlStatementDecorator>(BeforePrivateDecorators);
					_allDecorators.AddRange(QueryDecorators);
					_allDecorators.AddRange(AfterPrivateDecorators);
					_allDecorators = setDecoratorsOrder(_allDecorators);
				}
				return _allDecorators;
			}
		}

	    private List<TSqlStatementDecorator> setDecoratorsOrder(List<TSqlStatementDecorator> allDecorators)
	    {
	        return new Orderer().SetOrder(allDecorators);
	    }

	    protected QueryBuilder() :this(null)
	    {
	    }

	    #region Implementation of IQuery

		/// <summary>
		/// Иденификатор построителя для логирования
		/// </summary>
		[Dependency("Decorators")]
		public List<TSqlStatementDecorator> QueryDecorators { get; set; }
		
		public List<ITSqlStatementValidator> QueryValidators { get; set; }

		/// <summary>
		/// Сущность, для которой создается данный запрос. Т.о. объект запроса может получить доступ к свойствам и полям сущности.
		/// </summary>
		/// <remarks>
		/// Любой запрос обязательно отностится к конкретной сущности. 
		/// Если хочется создать произвольный запрос - создайте Точку данных и привяжите запрос к ней.
		/// </remarks>
		public IEntity Entity { get; set; }

		/// <summary>
		/// Метод возвращающий выражение в виде объектной модели из пространства Microsoft.Data.Schema.ScriptDom.Sql
		/// Чтобы получить тект запроса, вызовите метод Render() у возвращенного объекта.
		/// </summary>
		/// <returns></returns>
		public abstract TSqlStatement GetTSqlStatement();

		/// <summary>
		/// Получить sql-команду, готовую для выполнения.
		/// Выполняет GetTSqlStatement и к полученному выражения применяет приватные декораторы.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public virtual SqlCommand GetSqlCommand(SqlConnection connection)
		{
		    return GetSqlCommand(connection, AllDecorators);
		}

		public virtual SqlCommand GetSqlCommand(SqlConnection connection, List<TSqlStatementDecorator> decorators)
		{
			_notifyRequirements(decorators);

			TSqlStatement stm = _applyDecorators(GetTSqlStatement(), decorators);

			foreach (var validator in QueryValidators)
			{
				validator.Validate(stm);
			}

			return new SqlCommandFactory(stm.Render(), connection).CreateCommand();
		}

	    #endregion

		#region Private Methods

		protected abstract void InitPrivateDecorators();

		/// <summary>
		/// Применить приватные декораторы, зарегистрированные для конкретного построителя
		/// </summary>
		/// <param name="sourceStmt">Расширяемое выражение</param>
		/// <param name="decorators">Список декораторов</param>
		/// <returns></returns>
		private TSqlStatement _applyDecorators(TSqlStatement sourceStmt, IEnumerable<TSqlStatementDecorator> decorators)
		{
            this.Log(decorators);
			TSqlStatement stmt = sourceStmt;
		    var decs = decorators.ToList();
            for (int i = 0; i < decs.Count; i++)
            {
                var decorator = decs[i];                
                var observable = decorator as IObservableDecorator;
			    if (observable != null)
			    {
			        SubscribeDecorator(i, decs, observable, decorator);
			    }
	            stmt = decorator.Decorate(stmt, this);
	            
            }
            this.LogEnd();
			return stmt;
		}

	    private static void SubscribeDecorator(int i, List<TSqlStatementDecorator> decs, IObservableDecorator observable, TSqlStatementDecorator decorator)
	    {
	        EventDatas eventDatas = null;
	        for (int j = i + 1; j < decs.Count; j++)
	        {
	            var listener = decs[j] as IDecoratorListener;
	            if (listener != null)
	            {
	                observable.Decorated += e =>
	                {
	                    if (eventDatas == null)
	                        eventDatas = e.ToEventDatas();
	                    listener.OnDecorated(decorator, eventDatas);
	                };
	            }
	        }
	    }

	    private void _notifyRequirements(List<TSqlStatementDecorator> decorators)
        {
            var processDecorators = new List<TSqlStatementDecorator>(decorators);
			processDecorators.Reverse();
            IEnumerable<IRequirement> requirements = new List<IRequirement>();
			foreach (var decorator in processDecorators)
            {
                var acceptRequirements = decorator as IAcceptRequirements;
                if (acceptRequirements != null)
                    requirements = acceptRequirements.Accept(requirements);
                var hasRequirements = decorator as IHasRequirements;
                if (hasRequirements != null)
                {
                     var req = new List<IRequirement>(requirements);
                     req.AddRange(hasRequirements.GetRequirements());
                     requirements = req;

                }
            }
        }

	    #endregion
	}
}
