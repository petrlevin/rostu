using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;

namespace Platform.SqlObjectModel.Extensions
{
    /// <summary>
    /// Расширения для объектов QueryExpression, QuerySpecification
    /// </summary>
    public static class QueryExtensions
    {
        #region Join
        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="query">Запрос в который происходит добавление</param>
        /// <param name="joinType">Тип соединения</param>
        /// <param name="tableSource">Присоединяемая таблица</param>
        /// <param name="joinCondition">Условие соединения</param>
        public static void AddJoin(this QueryExpression query, QualifiedJoinType joinType, TableSource tableSource,
                                   Expression joinCondition)
        {
            if (query == null)
                throw new ArgumentNullException("query", "Передан пустой 'Запрос'");
            if (tableSource == null)
                throw new ArgumentNullException("tableSource", "Передана пустая 'Присоединяемая таблица'");
            if (joinCondition == null)
				throw new ArgumentNullException("joinCondition", "Передано пустое 'Условие соединения'");

            if (query is QuerySpecification)
            {
                (query as QuerySpecification)._addJoin(joinType, tableSource, joinCondition);
            }
            else throw new NotImplementedException("Не реализовано для: "+query.GetType());
        }



        /// <summary>
		/// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
		/// <param name="query">Запрос в который происходит добавление</param>
		/// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Схема левой таблицы</param>
        /// <param name="tableName">Имя левой таблицы</param>
        /// <param name="aliasName">Алиас правой таблицы</param>
		/// <param name="leftAliasName">Алиас левой таблицы</param>
        /// <param name="leftFieldName">Левое поля для соедиенения</param>
        /// <param name="thisFieldName">Парвой поле для соединения</param>
		public static void AddJoin(this QueryExpression query, QualifiedJoinType jointType,
                                   string schemaName, string tableName, string aliasName, string leftAliasName,
                                   string leftFieldName, string thisFieldName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName", "Передан пустой tableName");
            if (string.IsNullOrWhiteSpace(leftFieldName))
				throw new ArgumentNullException("leftFieldName", "Передан пустой leftFieldName");
            if (string.IsNullOrWhiteSpace(thisFieldName))
				throw new ArgumentNullException("thisFieldName", "Передан пустой thisFieldName");
            if (string.IsNullOrWhiteSpace(aliasName))
				throw new ArgumentNullException("aliasName", "Передан пустой aliasName");

            query.AddJoin(jointType,
                          Helper.CreateSchemaObjectTableSource(schemaName, tableName,
                                                               aliasName),
                          Helper.CreateBinaryExpression(leftAliasName, leftFieldName,
                                                        aliasName, thisFieldName,
                                                        BinaryExpressionType.Equals));
        }


        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="query">Запрос в который происходит добавление</param>
        /// <param name="joinType">Тип соединения</param>
        /// <param name="tableSource">Присоединяемая таблица</param>
        /// <param name="searchCondition">Условие соединения</param>
        private static void _addJoin(this QuerySpecification query, QualifiedJoinType joinType, TableSource tableSource,
                                    Expression searchCondition)
        {
            query.FromClauses[0] = query.FromClauses[0].Join(joinType, tableSource, searchCondition);
        }
        #endregion

        #region Fields
		/// <summary>
		/// Добавление выражения в запрос
		/// </summary>
		/// <param name="query">Запрос</param>
		/// <param name="expression">Выражение</param>
		public static void AddField(this QueryExpression query, Expression expression)
		{
			if (query == null)
				throw new ArgumentNullException("query", "Передан пустой query");
			if (expression == null)
				throw new ArgumentNullException("expression", "Передан пустой expression");
			if (query is QuerySpecification)
			{
				(query as QuerySpecification)._addFields(new List<Expression> {expression});
			}
			else throw new NotImplementedException("Не реализовано для "+query.GetType());
		}

		/// <summary>
		/// Добавление списка выражений в запрос
		/// </summary>
		/// <param name="query">Запрос</param>
		/// <param name="expressions">Список выражений</param>
		public static void AddFields(this QueryExpression query, List<Expression> expressions)
		{
			if (query == null)
				throw new ArgumentNullException("query", "Передан пустой query");
			if (expressions == null)
				throw new ArgumentNullException("expressions", "Передан пустой expressions");
			if (query is QuerySpecification)
			{
				(query as QuerySpecification)._addFields(expressions);
			}
			else throw new NotImplementedException("Не реализовано для " + query.GetType());
		}

		/// <summary>
		/// Добавление списка выражений в запрос
		/// </summary>
		/// <param name="query">Запрос</param>
		/// <param name="expressions">Список выражений</param>
		private static void _addFields(this QuerySpecification query, IEnumerable<Expression> expressions)
		{
			foreach (Expression expression in expressions)
			{
				query.SelectElements.Add(expression);
			}
		}
	    
		/// <summary>
	    /// Добавляет в запрос поля результирующего набора
	    /// </summary>
	    /// <param name="query">Выражение, в которое присходит добавление</param>
	    /// <param name="aliasName">Алиас таблицы, которой принадлежат добавляемые поля</param>
	    /// <param name="fieldsName">Имена полей</param>
	    /// <param name="onExists"> </param>
	    /// <returns>SelectStatement</returns>
	    public static void AddFields(this QueryExpression query, string aliasName, string fieldsName, OnExists onExists = OnExists.Add)
	    {
		    AddFields(query, aliasName, fieldsName.Split(',').Select(a => a.Trim()), onExists);
	    }


	    /// <summary>
	    /// Добавляет в запрос поля результирующего набора
	    /// </summary>
	    /// <param name="query">Выражение, в которое присходит добавление</param>
	    /// <param name="aliasName">Алиас таблицы, которой принадлежат добавляемые поля</param>
	    /// <param name="fieldsName">Имена полей</param>
	    /// <param name="onExists"> </param>
	    /// <returns>SelectStatement</returns>
	    public static void AddFields(this QueryExpression query, string aliasName, IEnumerable<string> fieldsName ,OnExists onExists = OnExists.Add)
        {
            if (query == null)
                throw new Exception("AddFields: передан пустой query");
            if (string.IsNullOrWhiteSpace(aliasName))
                throw new Exception("AddFields: передан пустой aliasName");
            if (fieldsName == null)
                throw new Exception("AddFields: передан пустой fieldsName");

            if (query is QuerySpecification)
            {
                (query as QuerySpecification)._addFields(aliasName, fieldsName, onExists);
            }
            else throw new Exception("не реализовано");
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="aliasName">Алиас таблицы, которой принадлежат добавляемые поля</param>
        /// <param name="fieldsName">Имена полей</param>
        /// <returns>SelectStatement</returns>
        private static void _addFields(this QuerySpecification query, string aliasName, IEnumerable<string> fieldsName, OnExists onExists = OnExists.Add)
        {
            foreach (string fieldName in fieldsName)
            {
                if (HandleOnExists(query, onExists, fieldName)) continue;
                query.SelectElements.Add(Helper.CreateColumn(aliasName, fieldName));
            }
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        public static void AddFields(this QueryExpression query, List<Field> fields)
        {
            if (query == null)
                throw new Exception("AddFields: передан пустой query");
            if (fields == null)
                throw new Exception("AddFields: передан пустой fields");

            if (query is QuerySpecification)
            {
                (query as QuerySpecification)._addFields(fields);
            }
            else throw new Exception("не реализовано");
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        public static void AddFields(this QueryExpression query, List<SelectColumn> fields, OnExists onExists = OnExists.Add)
        {
            if (query == null)
                throw new Exception("AddFields: передан пустой query");
            if (fields == null)
                throw new Exception("AddFields: передан пустой fields");

            if (query is QuerySpecification)
            {
                (query as QuerySpecification).AddFields(fields, onExists);
            }
            else throw new Exception("не реализовано");
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        private static void _addFields(this QuerySpecification query, IEnumerable<Field> fields)
        {
            foreach (Field field in fields)
            {
                query.SelectElements.Add(Helper.CreateColumn(field.Experssion, field.Alias));
            }
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        private static void AddFields(this QuerySpecification query, List<SelectColumn> fields, OnExists onExists = OnExists.Add)
        {
            foreach (SelectColumn field in fields)
            {
                if (HandleOnExists(query, onExists, ((Identifier) field.ColumnName)==null ? "" : ((Identifier) field.ColumnName).Value)) continue;

                query.SelectElements.Add(field);
            }
        }

        private static bool HandleOnExists(QuerySpecification query, OnExists onExists, string field)
        {
            if (onExists != OnExists.Add)
            {
                var existed = query.GetSelectColumn(field) ??
                              query.SelectElements.OfType<Column>()
                                   .FirstOrDefault(
                                       c =>
                                       c.Identifiers[c.Identifiers.Count - 1].Value.ToLower() ==
                                       field.ToLower());
                if (existed != null)
                    if (onExists == OnExists.Ignore)
                        return true;
                    else
                        throw new ArgumentException(
                            String.Format("Колонка {0} уже присутствует в выборке {1}", field,
                            query.Render()));
            }
            return false;
        }

        public enum OnExists
        {
            Throw = 0,
            Ignore = 1,
            Add = 2

        }

        #endregion

        #region Where

        /// <summary>
        /// Добавляет в запрос WHERE условие или объеденяется с существующим
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="joinType">Условие соединения с существующим условием</param>
        /// <param name="searchCondition">Условие поиска</param>
        public static QueryExpression AddWhere(this QueryExpression query, BinaryExpressionType joinType,
                                    Expression searchCondition)
        {
            if (query == null)
                throw new Exception("AddWhere: передан пустой query");
            if (searchCondition == null)
                throw new Exception("AddWhere: передан пустой searchCondition");

            if (query is QuerySpecification)
            {
                (query as QuerySpecification).AddWhere(joinType, searchCondition);
            }
            else throw new Exception("не реализовано");
            return query;
        }


        /// <summary>
        /// Добавляет в запрос WHERE условие или объеденяется с существующим используя "И" (And)
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="searchCondition">Условие поиска</param>
        public static  QueryExpression  AddWhere(this QueryExpression query, Expression searchCondition)
        {
           return AddWhere(query, BinaryExpressionType.And, searchCondition);
        }


        /// <summary>
        /// Создает объект Column
        /// </summary>
        /// <param name="tableName">Имя таблицы которой принадлежит поле</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns>Column</returns>
        public static Column CreateColumn(this QuerySpecification query, string tableName, string fieldName)
        {
            return Helper.CreateColumn(query.GetFirstAliasName(tableName), fieldName);
        }





        public static Identifier GetFirstAlias(this QuerySpecification querySpecification, string tableName,
                                               bool includeSourceSubqueries = false)
        {
            Identifier result = null;

            foreach (TableSource tableSource in querySpecification.FromClauses)
            {
                if (tableSource is TableSourceWithAlias)
                {
                    if ((!includeSourceSubqueries) && (tableSource is QueryDerivedTable))
                        continue;
                    if (tableSource.HasTable(tableName))
                    {
                        result = (tableSource as TableSourceWithAlias).Alias;
                        break;
                    }
                }
                else if (tableSource is QualifiedJoin)
                {
                    result = (tableSource as QualifiedJoin).GetAliasOnTable(tableName, includeSourceSubqueries);
                    //break;
                }
                else
                {
                    throw new PlatformException("GetFirstAlias: не реализовано для " + tableSource.GetType().ToString());
                }
            }

            return result;

        }


        public static string GetFirstAliasName(this QuerySpecification querySpecification, string tableName,
                                               bool includeSourceSubqueries = false)
        {
            var result = querySpecification.GetFirstAlias(tableName, includeSourceSubqueries);
            if (result != null)
                return result.Value;
            return null;
        }



        /// <summary>
        /// Добавляет в запрос WHERE условие или объеденяется с существующим
        /// </summary>
        /// <param name="query">Выражение, в которое присходит добавление</param>
        /// <param name="joinType">Условие соединения с существующим условием</param>
        /// <param name="searchCondition">Условие поиска</param>
        private static void AddWhere(this QuerySpecification query, BinaryExpressionType joinType,
                                     Expression searchCondition)
        {
            if (query.WhereClause == null)
                query.WhereClause = new WhereClause { SearchCondition = searchCondition };
            else
                query.WhereClause.SearchCondition = query.WhereClause.SearchCondition != null
                                                        ? new BinaryExpression
                                                              {
                                                                  BinaryExpressionType = joinType,
                                                                  FirstExpression = query.WhereClause.SearchCondition,
                                                                  SecondExpression = searchCondition
                                                              }
                                                        : searchCondition;
        }

        #endregion

        #region Union

        /// <summary>
        /// Объеденяет текущий и переданный конструктор конструкцией UNION
        /// </summary>
        /// <param name="query">Текущий запрос</param>
        /// <param name="unionQuery">второй запрос</param>
        /// <param name="useAll">Использовать предложение ALL в конструкции UNION</param>
        /// <returns>BinaryQueryExpression</returns>
        public static BinaryQueryExpression Union(this QueryExpression query, QueryExpression unionQuery,
                                                  bool useAll = false, bool withParenthesis =false)
        {
            if (query == null)
                throw new Exception("Union: передан пустой query");
            if (unionQuery == null)
                throw new Exception("Union: передан пустой unionQuery");

            return new BinaryQueryExpression
                       {
                           FirstQueryExpression = withParenthesis ? query.ToParenthesis() : query,
                           SecondQueryExpression = withParenthesis ? unionQuery.ToParenthesis() : unionQuery,
                           BinaryQueryExpressionType = BinaryQueryExpressionType.Union,
                           All = useAll
                       };
        }

        public static QueryExpression UnionWith(this QueryExpression query, QueryExpression unionQuery,
                                                  bool useAll = false)
        {
            if (unionQuery == null)
                throw new ArgumentException("Union: передан пустой unionQuery");
            if (query == null)
                return unionQuery;

            return new BinaryQueryExpression
            {
                FirstQueryExpression = query,
                SecondQueryExpression = unionQuery,
                BinaryQueryExpressionType = BinaryQueryExpressionType.Union,
                All = useAll
            };
        }


        #endregion

        /// <summary>
        /// Преобразование объекта QueryExpression в объект Subquery
        /// </summary>
        /// <param name="query">Преобразовываемый объект</param>
        /// <returns>Subquery</returns>
        public static Subquery ToSubquery(this QueryExpression query)
        {
            if (query == null)
                throw new Exception("ToSubquery: передан пустой query");

			if (query is BinaryQueryExpression)
				return (query as BinaryQueryExpression).ToSubquery();
			else if (query is QuerySpecification)
				return (query as QuerySpecification).ToSubquery();
			else throw new Exception("не реализовано");
        }


        public static TQueryExpression Cast<TQueryExpression>(this QueryExpression query)
            where TQueryExpression : QueryExpression ,new()
        {
            var result = new TQueryExpression();
            result.FirstTokenIndex = query.FirstTokenIndex;
            result.LastTokenIndex = query.LastTokenIndex;
            result.ScriptTokenStream = query.ScriptTokenStream;
            var qsResult = result as QuerySpecification;
            if (qsResult!=null)
            {
                var qsSource = query as QuerySpecification;
                if (qsSource != null)
                {
                    
                    qsResult.GroupByClause = qsSource.GroupByClause;
                    qsResult.HavingClause = qsSource.HavingClause;
                    qsResult.Into = qsSource.Into;
                    foreach (var selectElement in qsSource.SelectElements)
                    {
                        qsResult.SelectElements.Add(selectElement);
                    }
                    foreach (var fromClause in qsSource.FromClauses)
                    {
                        qsResult.FromClauses.Add(fromClause);
                    }

                    qsResult.TopRowFilter = qsSource.TopRowFilter;
                    qsResult.UniqueRowFilter = qsSource.UniqueRowFilter;
                    qsResult.WhereClause = qsSource.WhereClause;
                }
            }
            var sqsResult = result as SubquerySpecification;
            if (sqsResult != null)
            {
                var sqsSource = query as SubquerySpecification;
                if (sqsSource != null)
                {
                    sqsResult.ForBrowse = sqsSource.ForBrowse;
                    sqsResult.OrderByClause = sqsSource.OrderByClause;
                    sqsResult.XmlForClause = sqsSource.XmlForClause;

                }
            }
            return result;
        }

        /// <summary>
        /// Преобразование объекта BinaryQueryExpression в объект Subquery
        /// </summary>
        /// <param name="query">Преобразовываемый объект</param>
        /// <returns>Subquery</returns>
        private static Subquery ToSubquery(this BinaryQueryExpression query)
        {
            return new Subquery
                       {
                           QueryExpression = query
                       };
        }

        /// <summary>
        /// Преобразование объекта QuerySpecification в объект Subquery
        /// </summary>
        /// <param name="query">Преобразовываемый объект</param>
        /// <returns>Subquery</returns>
        private static Subquery ToSubquery(this QuerySpecification query)
        {
            return new Subquery
                       {
                           QueryExpression = query,
                       };
        }

        /// <summary>
        /// Преобразование объекта Subquery в объект CommonTableExpression
        /// </summary>
        /// <param name="subquery">Преобразовываемый объект</param>
        /// <param name="name">Имя WITH конструкции</param>
        /// <returns>CommonTableExpression</returns>
        public static CommonTableExpression ToCommonTableExpression(this Subquery subquery, string name)
        {
            if (subquery == null)
                throw new Exception("ToCommonTableExpression: передан пустой query");
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("ToCommonTableExpression: передан пустой name");

            return new CommonTableExpression
                       {
                           Subquery = subquery,
                           ExpressionName = name.ToIdentifier()
                       };
        }

        /// <summary>
        /// Преобразование объекта Subquery в объект CommonTableExpression
        /// </summary>
        /// <param name="subquery">Преобразовываемый объект</param>
        /// <param name="name">Имя WITH конструкции</param>
        /// <param name="fieldsName">Имена полей</param>
        /// <returns>CommonTableExpression</returns>
        public static CommonTableExpression ToCommonTableExpression(this Subquery subquery, string name,
                                                                    List<string> fieldsName)
        {
            if (subquery == null)
                throw new Exception("ToCommonTableExpression: передан пустой query");
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("ToCommonTableExpression: передан пустой name");
            if (fieldsName == null)
                throw new Exception("ToCommonTableExpression: передан пустой fieldsName");

            CommonTableExpression result = new CommonTableExpression
                                               {
                                                   Subquery = subquery,
                                                   ExpressionName = name.ToIdentifier()
                                               };
            foreach (Identifier identifier in fieldsName.Select(s => s.ToIdentifier()))
            {
                result.Columns.Add(identifier);
            }
            return result;
        }

        /// <summary>
        /// Возваращает выражения из поля конструкции выражения SELECT
        /// </summary>
        /// <param name="query">Обрабатываемое выражение типа QueryExpression</param>
        /// <param name="fieldName">Наименование или алиас искомого поля</param>
        /// <returns></returns>
        public static Expression GetSelectColumn(this QueryExpression query, string fieldName)
        {
            if (query == null)
                throw new Exception("Передан пустой query");
            Expression result = null;
            if (query is QuerySpecification)
            {
                result = (query as QuerySpecification).GetSelectColumn(fieldName);
            }
            else if (query is BinaryQueryExpression)
            {
                result =  ((BinaryQueryExpression) query).FirstQueryExpression.GetSelectColumn(fieldName);
                //throw new Exception("Не реализовано для " + query.GetType());
            }
            return result;
        }

        public static Identifier GetSourceAlias(this QuerySpecification query, string tableName, bool orFirst = true)
        {
            var alias = query.GetFirstAlias(tableName, true);
            if (alias != null)
                return alias;
            if (orFirst)
            {
                var table = query.FromClauses.OfType<TableSourceWithAlias>().FirstOrDefault();
                if (table != null)
                    return table.Alias;
            }
            return null;

        }

        public static Expression GetSourceColumnForTable(this QuerySpecification query, string tableName, string fieldName, bool useFirstSourceIfNotFound = true)
        {

            var alias = query.GetSourceAlias(tableName, useFirstSourceIfNotFound);
            if (alias != null)
                return Helper.CreateColumn(alias, fieldName);
            else
                return null;
        }


        public static Expression GetSourceColumnForTable(this QueryExpression query, string tableName, string fieldName, bool useFirstSourceIfNotFound = true)
        {
            var querySpecification = query as QuerySpecification;
            if (querySpecification != null)
                return querySpecification.GetSourceColumnForTable(tableName, fieldName, useFirstSourceIfNotFound);


            throw new Exception("Не реализовано для " + query.GetType());


        }

        internal static Expression GetSourceColumn(this QueryExpression query, string aliasName, string fieldName ,WithCommonTableExpressionsAndXmlNamespaces withCommonTable)
        {
            var querySpecification = query as QuerySpecification;
            if (querySpecification != null)
                return querySpecification.GetSourceColumn(aliasName, fieldName, withCommonTable);


            throw new Exception("Не реализовано для " + query.GetType());


        }



        public static Expression FindColumn(this WithCommonTableExpressionsAndXmlNamespaces withCommonTables, string cteName, string fieldName)
        {
            var withCommonTable = withCommonTables.GetCommonTable(cteName);
            if (withCommonTable == null)
                return null;
            if (withCommonTable.Columns.Count != 0)
                return withCommonTable.Columns.Any(i => i.Value.ToLower() == fieldName)
                           ? Helper.CreateColumn(cteName, fieldName)
                           : null;
            return withCommonTable.Subquery.QueryExpression.GetSelectColumn(fieldName);


        }

        public static CommonTableExpression GetCommonTable(this WithCommonTableExpressionsAndXmlNamespaces withCommonTables,
                                           string cteName)
        {
            if (withCommonTables == null)
                return null;
            return  withCommonTables.CommonTableExpressions.FirstOrDefault(cte => cte.ExpressionName.Value.ToLower() == cteName.ToLower());
       }


        private static Expression GetSourceColumn(this QuerySpecification query, string aliasName, string fieldName, WithCommonTableExpressionsAndXmlNamespaces withCommonTable)
        {

            foreach (var tableSource in query.FromClauses)
            {
                TableSourceWithAliasAndColumns tsWa;
                var tsQj = tableSource as QualifiedJoin;
                if (tsQj != null)
                    tsWa = tsQj.GetTableSourceWithColumns(aliasName);
                else
                    tsWa = tableSource as TableSourceWithAliasAndColumns;

                if (tsWa != null)
                    if (tsWa.Alias.Value.ToLower() == aliasName)
                    {
                        var aoTs = tsWa as SchemaObjectTableSource;
                        bool found=false;
                        if (aoTs != null)
                            if (((aoTs.SchemaObject.SchemaIdentifier == null) ||
                                 (String.IsNullOrWhiteSpace(aoTs.SchemaObject.SchemaIdentifier.Value))) &&
                                (null != withCommonTable.GetCommonTable(aoTs.SchemaObject.BaseIdentifier.Value)))
                                found = withCommonTable.FindColumn(aoTs.SchemaObject.BaseIdentifier.Value, fieldName) !=
                                        null;
                            else
                                found = true;
                        if (found)
                            return Helper.CreateColumn(aliasName, fieldName); ;
                        if  (tsWa.Columns.Any(i => i.Value.ToLower() == fieldName.ToLower()))
                            return Helper.CreateColumn(aliasName, fieldName);
                        var wdt = tsWa as QueryDerivedTable;
                        if (wdt != null)
                            found = wdt.Subquery.QueryExpression.GetSelectColumn(fieldName) != null;
                        if (found)
                            return Helper.CreateColumn(aliasName, fieldName);
                    }

            }
            return null;
        }

        /// <summary>
        /// Возваращает выражения из поля конструкции выражения SELECT
        /// </summary>
        /// <param name="query">Обрабатываемое выражение типа QuerySpecification</param>
        /// <param name="fieldName">Наименование или алиас искомого поля</param>
        /// <returns></returns>
        private static Expression GetSelectColumn(this QuerySpecification query, string fieldName)
        {
            Expression result = null;
            foreach (TSqlFragment selectElement in query.SelectElements)
            {
                SelectColumn column = (selectElement as SelectColumn);
                if (column != null && column.ColumnName != null && (column.ColumnName as Identifier) != null && (column.ColumnName as Identifier).Value.Equals(fieldName, StringComparison.OrdinalIgnoreCase))
                {
                    result = column.Expression;
                    break;
                }
            }
            if (result != null)
                return result;
            return query.SelectElements.OfType<Column>()
                 .FirstOrDefault(
                     c => c.Identifiers[c.Identifiers.Count - 1].Value.ToLower() == fieldName.ToLower());
        }

		/// <summary>
		/// Возвращает массив имен отбираемых полей
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		public static List<string> GetSelectedColumns(this QuerySpecification query)
		{
			var result = new List<string>();
			foreach (TSqlFragment selectElement in query.SelectElements)
			{
				if (selectElement is Column)
				{
					var name = ((Column) selectElement).Identifiers.Select(id => id.Value).Last();
						//.Aggregate((a, b) => string.Format("{0}.{1}", a, b));
					result.Add(name);
				}
				else if (selectElement is SelectColumn)
				{
					result.Add(((Identifier)((SelectColumn)selectElement).ColumnName).Value);
				}
			}
			return result;
		}


        public static QueryParenthesis ToParenthesis(this QueryExpression query
            )
        {
            return new QueryParenthesis() {QueryExpression = query};
        }

        public static SelectStatement ToSelectStatement(this QueryExpression query
            )
        {
            return new SelectStatement() { QueryExpression = query };
        }


    }
}
