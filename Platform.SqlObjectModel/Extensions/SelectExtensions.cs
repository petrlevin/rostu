using System;
using System.Collections.Generic;
using Microsoft.Data.Schema.ScriptDom.Sql;
using System.Linq;

namespace Platform.SqlObjectModel.Extensions
{
    /// <summary>
    /// Расширения для объекта SelectStatement
    /// </summary>
    public static class SelectExtensions
	{
		/// <summary>
		/// Проверка валидности SelectStatement выражения
		/// </summary>
		/// <param name="selectStatement"></param>
		private static void _validateSelectStatement(SelectStatement selectStatement)
		{
			if (selectStatement == null)
				throw new ArgumentNullException("selectStatement", "Передан пустой selectStatement");
			if (selectStatement.QueryExpression == null)
				throw new ArgumentException("У переданного selectStatement пустой QueryExpression", "selectStatement");
		}

		#region Join
		/// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// Алиас присоединяемой таблице будет равен имени присоединяемой таблицы
        /// Алиас таблицы которая уже присутствует в запросе определяется как алиас первой встреченной таблицы указанной как левая <see cref="leftTableName"/>
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Наименование схемы</param>
        /// <param name="tableName">Наименование таблицы</param>
        /// <param name="leftTableName">Наименование таблицы, которая уже присутствует в запросе</param>
        /// <param name="leftFieldName">Поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldName">Поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement JoinWithTable(this SelectStatement selectStatement, QualifiedJoinType jointType,
                                           string schemaName, string tableName, string leftTableName,
                                           string leftFieldName, string thisFieldName)
        {
            return Join(selectStatement, jointType,
                                           schemaName, tableName, tableName, selectStatement.GetAliasOnTable(leftTableName),
                                           leftFieldName, thisFieldName);
        }

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// Алиас присоединяемой таблице будет равен имени присоединяемой таблицы
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Наименование схемы</param>
        /// <param name="tableName">Наименование таблицы</param>
        /// <param name="leftAliasName">Алиас таблицы, которая уже присутствует в запросе</param>
        /// <param name="leftFieldName">Поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldName">Поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Join(this SelectStatement selectStatement, QualifiedJoinType jointType,
                                           string schemaName, string tableName, string leftAliasName,
                                           string leftFieldName, string thisFieldName)
        {
            return Join(selectStatement, jointType,
                                           schemaName, tableName, tableName, leftAliasName,
                                           leftFieldName, thisFieldName);
        }

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Наименование схемы</param>
        /// <param name="tableName">Наименование таблицы</param>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <param name="leftAliasName">Алиас таблицы, которая уже присутствует в запросе</param>
        /// <param name="leftFieldName">Поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldName">Поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Join(this SelectStatement selectStatement, QualifiedJoinType jointType, string schemaName, string tableName, string aliasName, string leftAliasName, string leftFieldName, string thisFieldName)
        {
            _validateSelectStatement(selectStatement);

            string newAliasName = string.IsNullOrWhiteSpace(aliasName) ? selectStatement.NextAlias() : aliasName;
            selectStatement.QueryExpression.AddJoin(jointType, schemaName, tableName, newAliasName, leftAliasName, leftFieldName, thisFieldName);
            return selectStatement;
        }

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="joinType"></param>
        /// <param name="joinTableSource"></param>
        /// <param name="searchCondition"></param>
        /// <returns></returns>
        public static SelectStatement Join(this SelectStatement selectStatement, QualifiedJoinType joinType, TableSource joinTableSource, Expression searchCondition)
        {
            _validateSelectStatement(selectStatement);

			if (joinTableSource == null)
                throw new ArgumentNullException("joinTableSource", "Передан пустой joinTableSource");
            if (searchCondition == null)
                throw new ArgumentNullException("searchCondition", "Передан пустой searchCondition");

            selectStatement.QueryExpression.AddJoin(joinType, joinTableSource, searchCondition);

            return selectStatement;
        }
		#endregion

		#region Fields
		/// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="aliasName">Алиас таблицы, которой принадлежат добавляемые поля</param>
        /// <param name="fieldsName">Имена полей</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Fields(this SelectStatement selectStatement, string aliasName, List<string> fieldsName)
        {
            _validateSelectStatement(selectStatement);
            if (string.IsNullOrWhiteSpace(aliasName))
				throw new ArgumentNullException("aliasName", "Передан пустой aliasName");

            selectStatement.QueryExpression.AddFields(aliasName, fieldsName);
            return selectStatement;
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Fields(this SelectStatement selectStatement, List<Field> fields)
        {
            _validateSelectStatement(selectStatement);

            selectStatement.QueryExpression.AddFields(fields);
            return selectStatement;
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="field">Добавляемое поле</param>
        /// <returns></returns>
        public static SelectStatement AddField(this SelectStatement selectStatement, SelectColumn field)
        {
            return selectStatement.AddFields(new List<SelectColumn> {field});
        }

        /// <summary>
        /// Добавляет в запрос поля результирующего набора
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="fields">Добавляемые поля</param>
        /// <returns></returns>
        public static SelectStatement AddFields(this SelectStatement selectStatement, List<SelectColumn> fields)
        {
            _validateSelectStatement(selectStatement);

            selectStatement.QueryExpression.AddFields(fields);
            return selectStatement;
        }
		#endregion

		#region Where
		/// <summary>
        /// Добавляет в запрос выражение WHERE или происоединяет к существующему
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="joinType">Тип соединения с существующим выражением (только AND или OR)</param>
        /// <param name="searchCondition">Условие поиска</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Where(this SelectStatement selectStatement, BinaryExpressionType joinType, Expression searchCondition)
        {
            _validateSelectStatement(selectStatement);
            
			if (searchCondition == null)
                throw new ArgumentNullException("selectStatement", "Передан пустой searchCondition");
            if (joinType != BinaryExpressionType.Or && joinType != BinaryExpressionType.And)
                throw new ArgumentException("Допускается только AND или OR", "joinType");

            selectStatement.QueryExpression.AddWhere(joinType, searchCondition);
            return selectStatement;
        }
		#endregion


        public static void DeleteCtes(this StatementWithCommonTableExpressionsAndXmlNamespaces source)
        {
            if (source.WithCommonTableExpressionsAndXmlNamespaces == null)
                return;
            source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Clear();
        }

        public static void CopyCtes(this StatementWithCommonTableExpressionsAndXmlNamespaces source, StatementWithCommonTableExpressionsAndXmlNamespaces destination)
        {
            if (source.WithCommonTableExpressionsAndXmlNamespaces == null)
                return;
            if (destination.WithCommonTableExpressionsAndXmlNamespaces==null)
                destination.WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces();
            source.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Where(scte=>!destination.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Any(dcte=>dcte.ExpressionName.Value.Equals(scte.ExpressionName.Value))).ToList().ForEach(
                scte =>
                destination.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(scte));
        }

		/// <summary>
        /// Преобразует объект SelectStatement в объект Subquery
        /// </summary>
        /// <param name="selectStatement">Преобразуемый объект</param>
        /// <returns>Subquery</returns>
        public static Subquery ToSubquery(this SelectStatement selectStatement)
        {
            _validateSelectStatement(selectStatement);
            
			if (selectStatement.WithCommonTableExpressionsAndXmlNamespaces != null)
                throw new Exception("ToSubquery: Объект с CommonTableExpressions не может быть преобразован");

            if (selectStatement.QueryExpression is BinaryQueryExpression)
                return new Subquery
                {
                    QueryExpression = selectStatement.QueryExpression
                };


            var subquerySpecification = selectStatement.QueryExpression.Cast<SubquerySpecification>();
            if (selectStatement.ForClause is XmlForClause)

                subquerySpecification.XmlForClause = (XmlForClause)selectStatement.ForClause;


            if ((selectStatement.ForClause is XmlForClause) ||
                ((selectStatement.QueryExpression is QuerySpecification) &&
                        ((QuerySpecification)selectStatement.QueryExpression).TopRowFilter != null))
                subquerySpecification.OrderByClause = selectStatement.OrderByClause;

            return new Subquery
            {
                QueryExpression = subquerySpecification
            };
        }

        /// <summary>
        /// Преобразует объект SelectStatement в объект QueryDerivedTable
        /// </summary>
        /// <param name="selectStatement">Преобразуемый объект</param>
        /// <param name="aliasName">Присваеваимый алиас</param>
        /// <returns></returns>
        public static QueryDerivedTable ToQueryDerivedTable(this SelectStatement selectStatement, string aliasName = "")
        {
            _validateSelectStatement(selectStatement);

			QueryDerivedTable result = new QueryDerivedTable();
            result.Subquery = selectStatement.ToSubquery();
            result.Alias =
                (string.IsNullOrWhiteSpace(aliasName) ? selectStatement.NextAlias() : aliasName).ToIdentifier();
            return result;
        }

        /// <summary>
        /// Проеобразует SelectStatement в SelectStatement с конструкцией CommonTableExpressions
        /// </summary>
        /// <param name="selectStatement">Преобразуемый объект</param>
        /// <param name="name">Имя конструкции WITH</param>
        /// <returns></returns>
        public static SelectStatement ToSelectStatementWithCommonTableExpressions(this SelectStatement selectStatement, string name)
        {
            _validateSelectStatement(selectStatement);
            
			if ((selectStatement.QueryExpression as QuerySpecification) == null)
                throw new Exception("ToSelectStatementWithCommonTableExpressions: Не рализовано для " +
                                    selectStatement.QueryExpression.GetType().ToString());

            List<TSqlFragment> fields =
                (List<TSqlFragment>)(selectStatement.QueryExpression as QuerySpecification).SelectElements;
            SelectStatement result = new SelectStatement { WithCommonTableExpressionsAndXmlNamespaces = new WithCommonTableExpressionsAndXmlNamespaces() };
            result.WithCommonTableExpressionsAndXmlNamespaces.CommonTableExpressions.Add(
                selectStatement.ToSubquery().ToCommonTableExpression(name));
            QuerySpecification query = new QuerySpecification();
            foreach (TSqlFragment field in fields)
            {
                if (field is Column)
                {
                    query.SelectElements.Add(field);
                }
                else if (field is SelectColumn)
                {
                    query.SelectElements.Add(("a." + ((field as SelectColumn).ColumnName as Identifier).Value).ToColumn());
                }
            }
            query.FromClauses.Add(Helper.CreateSchemaObjectTableSource("", name, "a"));
            result.QueryExpression = query;
            return result;
        }

        /// <summary>
        /// Возвращает следующий алиас для запроса
        /// </summary>
        /// <param name="selectStatement">Обрабатываемое выражение</param>
        /// <returns></returns>
        public static string NextAlias(this SelectStatement selectStatement)
        {
            _validateSelectStatement(selectStatement);

            string result = string.Empty;
            if (selectStatement.QueryExpression is QuerySpecification)
            {
                foreach (TableSource tableSource in (selectStatement.QueryExpression as QuerySpecification).FromClauses)
                {
                    if (tableSource is SchemaObjectTableSource)
                    {
                        string tmp = (tableSource as SchemaObjectTableSource).GetAliasName();
                        if (String.Compare(tmp, result, StringComparison.OrdinalIgnoreCase) > 0)
                            result = tmp;
                    }
                    else if (tableSource is QueryDerivedTable)
                    {
                        string tmp = (tableSource as QueryDerivedTable).GetAliasName();
                        if (String.Compare(tmp, result, StringComparison.OrdinalIgnoreCase) > 0)
                            result = tmp;
                    }
                    else if (tableSource is QualifiedJoin)
                    {
                        string tmp = (tableSource as QualifiedJoin).GetMaxAlias("a");
                        if (String.Compare(tmp, result, StringComparison.OrdinalIgnoreCase) > 0)
                            result = tmp;
                    }
                    else
                    {
                        throw new Exception("NextAlias: Не реализовано для " + tableSource.GetType().ToString());
                    }
                }
            }
            else throw new Exception("NextAlias: не реализовано для " + selectStatement.QueryExpression.GetType().ToString());
            return result.GetNextAlias();
        }

        /// <summary>
        /// Возвращает алиас из выражения для первой встреченой укзанной таблицы
        /// </summary>
        /// <param name="selectStatement">Выражение</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <returns>string</returns>
        public static string GetAliasOnTable(this SelectStatement selectStatement, string tableName, bool includeSourceSubqueries = false)
        {
            _validateSelectStatement(selectStatement);

			if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("GetAlias: не указан tableName");


            var querySpecification = selectStatement.QueryExpression as QuerySpecification;
            if (querySpecification != null)
                return querySpecification.GetFirstAliasName(tableName, includeSourceSubqueries);

            throw new Exception("NextAlias: не реализовано для " + selectStatement.QueryExpression.GetType().ToString());


        }

        /// <summary>
        /// Добавляет в запрос конструкцию ORDER BY или дополняет существующую
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="expressions">Добавляемые поля или выражения</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement OrderBy(this SelectStatement selectStatement, List<ExpressionWithSortOrder> expressions)
        {
            if (selectStatement == null)
                throw new Exception("OrderBy: передан пустой selectStatement");
            if (selectStatement.OrderByClause == null)
            {
                OrderByClause orderByClause = new OrderByClause();
                foreach (ExpressionWithSortOrder field in expressions)
                {
                    orderByClause.OrderByElements.Add(field);
                }
                selectStatement.OrderByClause = orderByClause;
            }
            else
            {
                foreach (ExpressionWithSortOrder field in expressions)
                {
                    selectStatement.OrderByClause.OrderByElements.Add(field);
                }
            }
            return selectStatement;
        }

        /// <summary>
        /// Удаляет поле из выражения SELECT
        /// </summary>
        /// <param name="selectStatement">Обрабатываемое выражение типа SelectStatement</param>
        /// <param name="fieldName">Наименование удаляемого поля</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement RemoveField(this SelectStatement selectStatement, string fieldName)
        {
            TSqlFragment forDelete = null;

            foreach (TSqlFragment selectElement in (selectStatement.QueryExpression as QuerySpecification).SelectElements)
            {
                if ((selectElement as SelectColumn) != null
                    && (selectElement as SelectColumn).ColumnName != null
                    && ((selectElement as SelectColumn).ColumnName as Identifier).Value == fieldName
                    )
                    forDelete = selectElement;
				if(((selectElement as Column)!=null)
					&& (selectElement as Column).Identifiers.Last().Value==fieldName)
					forDelete = selectElement;
				
            }
            if (forDelete != null)
                (selectStatement.QueryExpression as QuerySpecification).SelectElements.Remove(forDelete);

            return selectStatement;



        }


        public static SelectStatement AddField(this SelectStatement selectStatement, Expression field)
        {
            selectStatement.QueryExpression.AddField(field);
            return selectStatement;
        }


        /// <summary>
        /// Возваращает выражения из поля конструкции выражения SELECT
        /// </summary>
        /// <remarks>
        /// Для [b].[Caption] AS [idOrganization_Caption], [a].[Date]
        /// при вызове с параметром idOrganization_Caption вернет [b].[Caption].
        /// при вызове с параметром [a].[Date] вернет [a].[Date].
        /// </remarks>
        /// <param name="selectStatement">Обрабатываемое выражение типа SelectStatement</param>
        /// <param name="fieldName">Наименование или алиас искомого поля</param>
        /// <returns></returns>
        public static Expression GetSelectColumn(this SelectStatement selectStatement, string fieldName)
        {
            return selectStatement.QueryExpression.GetSelectColumn(fieldName);
        }

        public static Expression GetSourceColumnForTable(this SelectStatement selectStatement, string tableName, string fieldName, bool useFirstSourceIfNotFound = true)
        {
            return selectStatement.QueryExpression.GetSourceColumnForTable(tableName, fieldName, useFirstSourceIfNotFound);
        }

        public static Expression GetSourceColumn(this SelectStatement selectStatement, string aliasName, string fieldName)
        {
            return selectStatement.QueryExpression.GetSourceColumn(aliasName, fieldName, selectStatement.WithCommonTableExpressionsAndXmlNamespaces);
        }


        public static SelectStatement AddForClause(this SelectStatement @select, string pathValue, bool withType = true)
        {
            var @for = new XmlForClause();
            @for.Options.Add(new XmlForClauseOption() { OptionKind = XmlForClauseOptions.Path, Value = pathValue.ToLiteral() });
            if (withType)
                @for.Options.Add(new XmlForClauseOption() { OptionKind = XmlForClauseOptions.Type });
            @select.ForClause = @for;
            return @select;
        }




    }



}
