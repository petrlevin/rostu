using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{
	/// <summary>
	/// Добавляет в инструкцию INSERT клаузу OUTPUT со значением поля id.
	/// </summary>
	public class AddIdentityToOutput : TSqlStatementDecorator
	{
		private IInsertQueryBuilder _builder;

	    /// <summary>
	    /// Добавляет в инструкцию INSERT клаузу OUTPUT со значением поля id.
	    /// </summary>
	    public AddIdentityToOutput()
		{
		}

		#region Implementation of ITSqlStatementDecorator

		/// <summary>
		/// Применить декоратор
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		/// <param name="queryBuilder">Построитель</param>
		/// <returns>TSqlStatement</returns>
		protected override TSqlStatement DoDecorate(TSqlStatement source, IQueryBuilder queryBuilder)
		{
			_builder = (queryBuilder as IInsertQueryBuilder);
			if (_builder == null)
				return source;

			bool condition = _builder.ReturnIdentityValue
			                 && _builder.Entity.EntityType != EntityType.Enum
			                 && _builder.Entity.Fields.Any(f => f.Name.Equals("id", StringComparison.OrdinalIgnoreCase));
			if (!condition)
				return source;

			TSqlStatement result = processDecoration(source);
			//this.Log(result, queryBuilder);
			return result;
		}

		/// <summary>
		/// Метод, непосредственно добавляющий клаузу OUTPUT
		/// </summary>
		/// <param name="source">Расширяемое выражение</param>
		private TSqlStatement processDecoration(TSqlStatement source)
		{
			InsertStatement result = (source as InsertStatement);
			if (result == null)
				return source;

			SchemaObjectName schemaObjectName = new SchemaObjectName();
			schemaObjectName.Identifiers.Add("@tmp".ToIdentifierWithoutQuote());
			OutputClause output = new OutputClause();
			output.FirstSelectColumns.Add(Helper.CreateColumn("inserted", "id", ""));
			output.IntoTable = schemaObjectName;
			result.OutputClause = output;
			return result;
		}

		#endregion
	}
}
