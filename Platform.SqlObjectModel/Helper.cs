using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Schema.ScriptDom;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils;
using Platform.Utils.Extensions;
using BinaryExpression = Microsoft.Data.Schema.ScriptDom.Sql.BinaryExpression;
using Expression = Microsoft.Data.Schema.ScriptDom.Sql.Expression;
using UnaryExpression = Microsoft.Data.Schema.ScriptDom.Sql.UnaryExpression;

namespace Platform.SqlObjectModel
{
    /// <summary>
    /// Класс помошников для построения запросов с использованием Microsoft.Data.Schema.ScriptDom.Sql
    /// </summary>
    public class Helper
    {
        #region Column, SelectColumn
        /// <summary>
        /// Создает объект Column
        /// </summary>
        /// <param name="tableAlias">Алиас таблицы которой принадлежит поле</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns>Column</returns>
        public static Column CreateColumn(Identifier tableAlias, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException("fieldName", "Не указан fieldName");

            Column result = new Column();
            if (tableAlias != null)
                result.Identifiers.Add(tableAlias);

            result.Identifiers.Add(fieldName.ToIdentifier());
            return result;
        }

        /// <summary>
        /// Создает объект Column
        /// </summary>
        /// <param name="tableAliasName">Алиас таблицы которой принадлежит поле</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns>Column</returns>
        public static Column CreateColumn(string tableAliasName, string fieldName)
        {
            return CreateColumn(string.IsNullOrWhiteSpace(tableAliasName) ? null : tableAliasName.ToIdentifier(), fieldName);
        }

        /// <summary>
        /// Создает объект Column 
        /// </summary>
        /// <param name="tableAliasName">Алиас таблицы</param>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="fieldAliasName">Алиас поля</param>
        /// <returns></returns>
        public static SelectColumn CreateColumn(string tableAliasName, string fieldName, string fieldAliasName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException("fieldName", "Не передано 'Имя поля'");
            SelectColumn result = new SelectColumn
            {
                Expression = CreateColumn(tableAliasName, fieldName)
            };
            if (!string.IsNullOrWhiteSpace(fieldAliasName))
                result.ColumnName = fieldAliasName.ToIdentifier();
            return result;
        }

        /// <summary>
        /// Создает объект Column
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <param name="fieldAliasName">Алиас поля</param>
        /// <returns>Column</returns>
        public static SelectColumn CreateColumn(Expression expression, string fieldAliasName)
        {
            if (expression == null)
                throw new Exception("Не указано выражение");
            SelectColumn result = new SelectColumn
            {
                Expression = expression
            };
            if (!string.IsNullOrWhiteSpace(fieldAliasName))
                result.ColumnName = fieldAliasName.ToIdentifier();

            return result;
        }
        #endregion

        #region BinaryExpression
        /// <summary>
        /// Создание объекта Expression вида field1 (операция сравнения) field2
        /// </summary>
        /// <param name="firstAliasName">Алиас таблицы</param>
        /// <param name="firstFieldName">Имя поля</param>
        /// <param name="secondAliasName">Алиас таблицы</param>
        /// <param name="secondFieldName">Имя поля</param>
        /// <param name="binaryExpressionType">Тип сравнения</param>
        /// <returns>BinaryExpression</returns>
        public static BinaryExpression CreateBinaryExpression(string firstAliasName, string firstFieldName, string secondAliasName, string secondFieldName, BinaryExpressionType binaryExpressionType)
        {

            if (string.IsNullOrWhiteSpace(firstFieldName))
                throw new Exception("Не указан firstFieldName");
            if (string.IsNullOrWhiteSpace(secondFieldName))
                throw new Exception("Не указан secondFieldName");

            return new BinaryExpression
            {
                BinaryExpressionType = binaryExpressionType,
                FirstExpression = CreateColumn(firstAliasName, firstFieldName),
                SecondExpression = CreateColumn(secondAliasName, secondFieldName)
            };
        }


        /// <summary>
        /// Создание объекта Expression вида field1 (операция сравнения) field2
        /// </summary>
        /// <param name="firstAlias">Алиас таблицы</param>
        /// <param name="firstFieldName">Имя поля</param>
        /// <param name="secondAlias">Алиас таблицы</param>
        /// <param name="secondFieldName">Имя поля</param>
        /// <param name="binaryExpressionType">Тип сравнения</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static BinaryExpression CreateBinaryExpression(Identifier firstAlias, string firstFieldName, Identifier secondAlias, string secondFieldName, BinaryExpressionType binaryExpressionType = BinaryExpressionType.Equals)
        {

            if (string.IsNullOrWhiteSpace(firstFieldName))
                throw new Exception("Не указан firstFieldName");
            if (string.IsNullOrWhiteSpace(secondFieldName))
                throw new Exception("Не указан secondFieldName");

            return new BinaryExpression
            {
                BinaryExpressionType = binaryExpressionType,
                FirstExpression = CreateColumn(firstAlias, firstFieldName),
                SecondExpression = CreateColumn(secondAlias, secondFieldName)
            };
        }

        /// <summary>
        /// Создание объекта Expression вида expression1 (операция сравнения) expression2
        /// </summary>
        /// <param name="firstExpression">Левое выражение</param>
        /// <param name="secondExpression">Правое выражение</param>
        /// <param name="binaryExpressionType">Тип сравнения</param>
        /// <returns></returns>
        public static BinaryExpression CreateBinaryExpression(Expression firstExpression, Expression secondExpression, BinaryExpressionType binaryExpressionType)
        {
            if (firstExpression == null)
                throw new Exception("Не указан firstExpression");
            if (secondExpression == null)
                throw new Exception("Не указан secondExpression");

            return new BinaryExpression
            {
                BinaryExpressionType = binaryExpressionType,
                FirstExpression = firstExpression,
                SecondExpression = secondExpression
            };
        }
        #endregion

        #region SchemaObjectName
        /// <summary>
        /// Создает объект SchemaObjectName
        /// </summary>
        /// <param name="schemaName">Имя схемы</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <returns>SchemaObjectName</returns>
        public static SchemaObjectName CreateSchemaObjectName(string schemaName, string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("CreateSchemaObjectName: Не указано 'Имя таблицы'");

            SchemaObjectName result = new SchemaObjectName();
            if (!string.IsNullOrWhiteSpace(schemaName))
                result.Identifiers.Add(schemaName.ToIdentifier());

            result.Identifiers.Add(tableName.ToIdentifier());
            return result;
        }

        /// <summary>
        /// Создает объект SchemaObjectName
        /// </summary>
        /// <param name="values">Значение идентификаторов</param>
        /// <returns>SchemaObjectName</returns>
        public static SchemaObjectName CreateSchemaObjectName(params string[] values)
        {
            if (values == null || values.Length == 0)
                throw new Exception("CreateSchemaObjectName: Не указан values");

            SchemaObjectName result = new SchemaObjectName();
            foreach (string value in values)
            {
                result.Identifiers.Add(value.ToIdentifier());
            }
            return result;
        }
        #endregion

        #region SchemaObjectTableSource
        /// <summary>
        /// Создает объект SchemaObjectTableSource
        /// </summary>
        /// <param name="schemaName">Имя схемы</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <returns>SchemaObjectTableSource</returns>
        public static SchemaObjectTableSource CreateSchemaObjectTableSource(string schemaName, string tableName, string aliasName = null)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("CreateSchemaObjectTableSource: Не указано 'Имя таблицы'");

            SchemaObjectTableSource result = new SchemaObjectTableSource
                {
                    SchemaObject = CreateSchemaObjectName(schemaName, tableName)
                };
            if (!string.IsNullOrWhiteSpace(aliasName))
                result.Alias = aliasName.ToIdentifier();


            return result;

        }
        #endregion

        #region QualifiedJoin
        /// <summary>
        /// Создает объект QualifiedJoin который является представлением конструкции [table1] тип соединения [table2] on условие соединения
        /// </summary>
        /// <param name="firstTable">Левая таблица</param>
        /// <param name="secondTable">Правая таблица</param>
        /// <param name="searchCondition">Условие соединения</param>
        /// <param name="jointType">Тип соединения</param>
        /// <returns>QualifiedJoin</returns>
        public static QualifiedJoin CreateQualifiedJoin(TableSource firstTable, TableSource secondTable, Expression searchCondition, QualifiedJoinType jointType)
        {
            if (firstTable == null || secondTable == null || searchCondition == null)
                throw new Exception("CreateQualifiedJoin: объект со значениеm NULL не может использоваться");
            return new QualifiedJoin
            {
                FirstTableSource = firstTable,
                SecondTableSource = secondTable,
                SearchCondition = searchCondition,
                QualifiedJoinType = jointType
            };
        }
        #endregion

        /// <summary>
        /// Создание объекта QuerySpecification
        /// </summary>
        /// <param name="tableSources">Список для конструкции FROM</param>
        /// <param name="selectElements">Поля результирующего набора</param>
        /// <param name="whereClause">конструкиця WHERE</param>
        /// <param name="groupByClause">конструкиця GROUP BY</param>
        /// <param name="havingClause">конструкиця HAVING</param>
        /// <param name="topRowFilter">конструкиця TOP</param>
        /// <param name="uniqueRowFilter">конструкция DISTINCT</param>
        /// <returns>QuerySpecification</returns>
        public static QuerySpecification CreateQuerySpecification(IList<TableSource> tableSources, IList<TSqlFragment> selectElements, WhereClause whereClause, GroupByClause groupByClause, HavingClause havingClause, TopRowFilter topRowFilter, UniqueRowFilter uniqueRowFilter = UniqueRowFilter.NotSpecified)
        {
            if (selectElements == null)
                throw new Exception("CreateQuerySpecification: Параметр selectElements обязательный");

            QuerySpecification result = new QuerySpecification();
            if (tableSources != null)
            {
                foreach (TableSource tableSource in tableSources)
                {
                    result.FromClauses.Add(tableSource);
                }
            }
            foreach (TSqlFragment selectElement in selectElements)
            {
                result.SelectElements.Add(selectElement);
            }
            if (whereClause != null)
                result.WhereClause = whereClause;
            if (groupByClause != null)
                result.GroupByClause = groupByClause;
            if (havingClause != null)
                result.HavingClause = havingClause;
            if (topRowFilter != null)
                result.TopRowFilter = topRowFilter;
            result.UniqueRowFilter = uniqueRowFilter;
            return result;
        }

        /// <summary>
        /// Создание объекта QuerySpecification
        /// </summary>
        /// <param name="schemaName">Имя схемы</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="tableAlias">Алиас таблицы</param>
        /// <param name="fields">Поля результирующего набора</param>
        /// <returns>QuerySpecification</returns>
        public static QuerySpecification CreateQuerySpecification(string schemaName, string tableName, string tableAlias,
                                                                  string fields)
        {
            return CreateQuerySpecification(schemaName, tableName, tableAlias, fields.Split(',').Select(a => a.Trim()).ToList());
        }




        /// <summary>
        /// Создание объекта QuerySpecification
        /// </summary>
        /// <param name="schemaName">Имя схемы</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <param name="tableAlias">Алиас таблицы</param>
        /// <param name="fields">Поля результирующего набора</param>
        /// <returns>QuerySpecification</returns>
        public static QuerySpecification CreateQuerySpecification(string schemaName, string tableName, string tableAlias, List<string> fields)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new Exception("CreateQuerySpecification: не указан tableName");
            if (fields == null || fields.Count == 0)
                throw new Exception("CreateQuerySpecification: не указан fields");

            QuerySpecification result = new QuerySpecification();
            result.FromClauses.Add(CreateSchemaObjectTableSource(schemaName, tableName, tableAlias));
            foreach (string field in fields)
            {
                result.SelectElements.Add(CreateColumn(tableAlias, field));
            }
            return result;
        }



        public static QuerySpecification CreateQuerySpecification(TableSource tableSource, string fields)
        {
            return CreateQuerySpecification(tableSource, fields.Split(',').ToList());
        }

        /// <summary>
        /// Создание объекта QuerySpecification
        /// </summary>
        /// <param name="tableSource">Таблица используемая в запросе</param>
        /// <param name="fields">Поля результирующего набора</param>
        /// <returns>QuerySpecification</returns>
        public static QuerySpecification CreateQuerySpecification(TableSource tableSource, List<string> fields)
        {
            if (tableSource == null)
                throw new Exception("CreateQuerySpecification: не указан tableSources");
            if (fields == null || fields.Count == 0)
                throw new Exception("CreateQuerySpecification: не указан fields");

            string tableAlias = tableSource.GetAliasName();
            QuerySpecification result = new QuerySpecification();
            result.FromClauses.Add(tableSource);
            foreach (string field in fields)
            {
                result.SelectElements.Add(CreateColumn(tableAlias, field));
            }
            return result;
        }

        public static QuerySpecification CreateQuerySpecification(TableSource tableSource, params Object[] fields)
        {
            return CreateQuerySpecification(tableSource, fields.Select(f =>
                                                                           {
                                                                               if (f is TSqlFragment)
                                                                                   return (TSqlFragment)f;
                                                                               var s = f as string;
                                                                               if (s != null)
                                                                                   return (s.Count(c => c == '.') == 2) ? (TSqlFragment)s.ToSelectColumn(false) : (TSqlFragment)s.ToColumn();
                                                                               return (TSqlFragment)s.ToLiteral();
                                                                           }
                                                             ).ToList());

        }

        /// <summary>
        /// Создание объекта QuerySpecification
        /// </summary>
        /// <param name="tableSource">Таблица используемая в запросе</param>
        /// <param name="fields">Поля результирующего набора</param>
        /// <returns>QuerySpecification</returns>
        public static QuerySpecification CreateQuerySpecification(TableSource tableSource, List<TSqlFragment> fields)
        {
            if (fields == null || fields.Count == 0)
                throw new Exception("CreateQuerySpecification: не указан fields");

            if (tableSource != null)
                return CreateQuerySpecification(new List<TableSource> { tableSource }, fields, null, null, null, null);
            else
                return CreateQuerySpecification(null, fields, null, null, null, null);
        }

        public static QuerySpecification CreateQuerySpecification(TableSource tableSource, TSqlFragment field)
        {
            return CreateQuerySpecification(tableSource, new List<TSqlFragment>() { field });
        }

        public static QuerySpecification CreateQuerySpecification(TSqlFragment field)
        {
            return CreateQuerySpecification(null, field);
        }



        #region UnaryExpression
        /// <summary>
        /// Создание объекта Expression вида field IS NULL
        /// </summary>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <returns>UnaryExpression</returns>
        public static UnaryExpression CreateCheckFieldIsNull(string aliasName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException("fieldName", "Не указан fieldName");

            return _createUnaryExpression(CreateColumn(aliasName, fieldName), UnaryExpressionType.IsNull); ;
        }

        /// <summary>
        /// Создание объекта UnaryExpression вида expression IS NULL
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <returns>UnaryExpression</returns>
        public static UnaryExpression CreateCheckIsNull(Expression expression, bool isNull = true)
        {
            return _createUnaryExpression(expression, isNull ? UnaryExpressionType.IsNull : UnaryExpressionType.IsNotNull);
        }

        /// <summary>
        /// Создание объекта UnaryExpression вида expression IS NULL
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <returns>UnaryExpression</returns>
        public static UnaryExpression CreateCheckIsNotNull(Expression expression)
        {
            return _createUnaryExpression(expression, UnaryExpressionType.IsNotNull);
        }

        /// <summary>
        /// Создание объекта UnaryExpression
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <param name="unaryExpressionType">Тип</param>
        /// <returns></returns>
        private static UnaryExpression _createUnaryExpression(Expression expression, UnaryExpressionType unaryExpressionType)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Передан пустой expression");
            return new UnaryExpression { UnaryExpressionType = unaryExpressionType, Expression = expression };
        }

        /// <summary>
        /// Создание объекта UnaryExpression
        /// </summary>
        /// <remarks>Выражение вида field IS NOT NULL</remarks>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <param name="fieldName">Имя поля</param>
        /// <returns>UnaryExpression</returns>
        public static UnaryExpression CreateCheckFieldIsNotNull(string aliasName, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException("fieldName", "Не указан fieldName");
            return _createUnaryExpression(CreateColumn(aliasName, fieldName), UnaryExpressionType.IsNotNull);
        }
        #endregion

        /// <summary>
        /// Создание объекта CreateBinaryQueryExpression
        /// </summary>
        /// <param name="firstQueryExpression">Первый запрос</param>
        /// <param name="secondQueryExpression">Второй запрос</param>
        /// <param name="binaryQueryExpressionType">Тип объединения</param>
        /// <param name="useAll">Использовать ALL</param>
        /// <returns>BinaryQueryExpression</returns>
        public static BinaryQueryExpression CreateBinaryQueryExpression(QueryExpression firstQueryExpression, QueryExpression secondQueryExpression, BinaryQueryExpressionType binaryQueryExpressionType, bool useAll = false)
        {
            if (firstQueryExpression == null)
                throw new Exception("CreateBinaryQueryExpression: firstQueryExpression == NULL");
            if (secondQueryExpression == null)
                throw new Exception("CreateBinaryQueryExpression: secondQueryExpression == NULL");

            return new BinaryQueryExpression
                {
                    BinaryQueryExpressionType = binaryQueryExpressionType,
                    FirstQueryExpression = firstQueryExpression,
                    SecondQueryExpression = secondQueryExpression,
                    All = useAll
                };
        }

        #region InPredicate
        /// <summary>
        /// Создание объекта InPredicate
        /// </summary>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Значения в предложении IN</param>
        /// <param name="literalType">Тип значений</param>
        /// <param name="useNot">Добавления конструкции NOT в выражение</param>
        /// <returns>InPredicate</returns>
        public static InPredicate CreateInPredicate(string aliasName, string fieldName, List<string> values, LiteralType literalType, bool useNot = false)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
                throw new ArgumentNullException("fieldName", "Не указано 'Имя поля'");
            if (values == null || values.Count == 0)
                throw new ArgumentNullException("values", "CreateInPredicate: Не указаны 'Значения в предложении IN'");

            InPredicate result = new InPredicate { Expression = CreateColumn(aliasName, fieldName), NotDefined = useNot };
            foreach (string value in values)
            {
                result.Values.Add(value.ToLiteral(literalType));
            }
            return result;
        }

        /// <summary>
        /// Создание объекта InPredicate
        /// </summary>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Значения в предложении IN которые будут подставлены без обработки</param>
        /// <param name="useNot">Добавления конструкции NOT в выражение</param>
        /// <returns>InPredicate</returns>
        public static InPredicate CreateInPredicate(string aliasName, string fieldName, string values, bool useNot = false)
        {
            return CreateInPredicate(CreateColumn(aliasName, fieldName), values, useNot);
        }

        /// <summary>
        /// Создание объекта InPredicate
        /// </summary>
        /// <param name="aliasName">Алиас таблицы</param>
        /// <param name="fieldName">Имя поля</param>
        /// <param name="values">Значения в предложении IN</param>
        /// <param name="useNot">Добавления конструкции NOT в выражение</param>
        /// <returns>InPredicate</returns>
        public static InPredicate CreateInPredicate(string aliasName, string fieldName, IEnumerable values, bool useNot = false)
        {
            return CreateInPredicate(CreateColumn(aliasName, fieldName), values, useNot);
        }

        /// <summary>
        /// Создание объекта InPredicate
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <param name="subquery">Запрос</param>
        /// <param name="useNot">Добавление конструкции NOT в выражение</param>
        /// <returns>InPredicate</returns>
        public static InPredicate CreateInPredicate(Expression expression, Subquery subquery, bool useNot = false)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Не указано 'Выражение'");
            if (subquery == null || subquery.QueryExpression == null)
                throw new ArgumentNullException("subquery", "Не указан 'Запрос'");

            return new InPredicate { Expression = expression, NotDefined = useNot, Subquery = subquery };
        }

        public static InPredicate CreateInPredicate(Expression expression, string values, bool useNot = false)
        {
            IEnumerable<Expression> expressions = new List<Expression> {values.ToLiteral(LiteralType.Variable)}.AsEnumerable();
            return CreateInPredicate(expression, expressions, useNot);
        }

        public static InPredicate CreateInPredicate(Expression expression, IEnumerable values, bool useNot = false)
        {
            if (values == null || !values.GetEnumerator().MoveNext())
                throw new ArgumentNullException("values", "Не указано 'Значения в предложении IN'");

            IEnumerable<Expression> expressions = values
                .Cast<Object>()
                .Select(v => v is Expression ? (Expression) v : (Expression) v.ToLiteral());

            return CreateInPredicate(expression, expressions, useNot);
        }

        public static InPredicate CreateInPredicate(Expression expression, IEnumerable<Expression> values, bool useNot = false)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Не указано 'Выражение'");

            var result = new InPredicate { Expression = expression, NotDefined = useNot };
            values.ToList().ForEach(v => result.Values.Add(v));
            return result;
        }

        #endregion

        #region LikePredicate
        /// <summary>
        /// Создание объекта LikePredicate
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <param name="likeExpression">Выражения для поиска</param>
        /// <param name="useNot">Добавление конструкции NOT в выражение</param>
        /// <returns>LikePredicate</returns>
        public static LikePredicate CreateLikePredicate(Expression expression, string likeExpression, bool useNot = false)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Не указано 'Выражение'");
            if (string.IsNullOrWhiteSpace(likeExpression))
                throw new ArgumentNullException("likeExpression", "Не указано 'Выражения для поиска'");

            return new LikePredicate
                {
                    FirstExpression = expression,
                    NotDefined = useNot,
                    SecondExpression = likeExpression.ToLiteral(LiteralType.AsciiStringLiteral),
                    EscapeExpression = "\\".ToLiteral()
                };
        }
        #endregion

        /// <summary>
        /// Создание выражения CAST(expression AS toType(length))
        /// </summary>
        /// <param name="expression">Выражение</param>
        /// <param name="toType">Тип к которому происходит приведение</param>
        /// <param name="length">Длинна(опционально)</param>
        /// <returns></returns>
        public static CastCall CreateCast(Expression expression, SqlDataTypeOption toType, int? length = null)
        {
            if (expression == null)
                throw new ArgumentNullException("expression", "Не указано приводимое выражение");
            SchemaObjectName schemaObjectName = new SchemaObjectName();
            schemaObjectName.Identifiers.Add(new Identifier { Value = toType.ToString(), QuoteType = QuoteType.NotQuoted });

            SqlDataType sqlDataType = new SqlDataType { Name = schemaObjectName, SqlDataTypeOption = toType };
            if (length.HasValue)
                sqlDataType.Parameters.Add(length.Value.ToLiteral());

            return new CastCall { DataType = sqlDataType, Parameter = expression };
        }

        /// <summary>
        /// Создание колонки с функцией ROW_NUMBER() OVER (ORDER BY...)
        /// </summary>
        /// <param name="orderByClause">Конструкция ORDER BY для OVER</param>
        /// <param name="fieldAliasName">Алиас поля</param>
        /// <returns></returns>
        public static SelectColumn CreateRowNumber(OrderByClause orderByClause, string fieldAliasName = "")
        {
            if (orderByClause == null)
                throw new Exception("CreateRowNumber: Не указано orderByClause");
            if (orderByClause.OrderByElements.Count == 0)
                throw new Exception("CreateRowNumber: Не указаны поля в orderByClause");

            SelectColumn result = new SelectColumn();
            FunctionCall function = new FunctionCall
                {
                    FunctionName = "ROW_NUMBER".ToIdentifierWithoutQuote(),
                    OverClause = new OverClause { OrderByClause = orderByClause },
                };
            result.Expression = function;
            if (!string.IsNullOrWhiteSpace(fieldAliasName))
                result.ColumnName = fieldAliasName.ToIdentifier();

            return result;
        }

        #region OverClause
        /// <summary>
        /// Создание кострукции OVER (ORDER BY orderByClause PARTITION BY partitions)
        /// </summary>
        /// <param name="orderByClause"></param>
        /// <param name="partitions"></param>
        /// <returns></returns>
        public static OverClause CreateOverClause(OrderByClause orderByClause, List<Expression> partitions)
        {
            OverClause result = new OverClause();
            if (orderByClause != null)
                result.OrderByClause = orderByClause;
            if (partitions != null)
            {
                foreach (Expression partition in partitions)
                {
                    result.Partitions.Add(partition);
                }
            }
            return result;
        }
        #endregion

        #region FunctionCall
        /// <summary>
        /// Создание обекта, представлюящего собой функцию
        /// </summary>
        /// <param name="functionName">Имя функции</param>
        /// <param name="parameter">Список параметров</param>
        /// <param name="overClause">конструкция OVER()</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static FunctionCall CreateFunctionCall(string functionName, Expression parameter, OverClause overClause = null, string target = null)
        {
            return CreateFunctionCall(functionName, new List<Expression>() { parameter }, overClause, target);
        }

        public static FunctionCall CreateFunctionCall(string functionName,
                                                        string target, params Object[] parameters)
        {


            return CreateFunctionCall(functionName, parameters.Select(
                o => (o is Expression) ? (Expression)o : (o is string) ? (((string)o).Contains('.') ? (Expression)((string)o).ToColumn() : o.ToLiteral()) : o.ToLiteral()
                                     ).ToList(), null, target);
        }

        /// <summary>
        /// Создание обекта, представлюящего собой функцию
        /// </summary>
        /// <param name="functionName">Имя функции</param>
        /// <param name="parameters">Список параметров</param>
        /// <param name="overClause">конструкция OVER()</param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static FunctionCall CreateFunctionCall(string functionName, List<Expression> parameters, OverClause overClause = null, string target = null)
        {
            if (string.IsNullOrWhiteSpace(functionName))
                throw new ArgumentNullException("functionName", "Не указано имя функции");

            FunctionCall result = new FunctionCall
                {
                    FunctionName = functionName.ToIdentifierWithoutQuote()

                };
            if (target != null)
            {
                var callTarget = new IdentifiersCallTarget();
                callTarget.Identifiers.Add(target.ToIdentifierWithoutQuote());
                result.CallTarget = callTarget;
            }

            foreach (Expression expression in parameters)
            {
                result.Parameters.Add(expression);
            }
            if (overClause != null)
                result.OverClause = overClause;
            return result;
        }
        #endregion


        /// <summary>
        /// Создание конструкции fieldName BETWEEN firstValue AND secondValue
        /// </summary>
        /// <param name="field">Поле</param>
        /// <param name="firstValue">Левая граница</param>
        /// <param name="secondValue">Правая граница</param>
        /// <returns></returns>
        public static TernaryExpression CreateBetween(Column field, object firstValue, object secondValue)
        {
            if (field == null || field.Identifiers.Count == 0)
                throw new ArgumentNullException("field", "Не указано field");
            if (firstValue == null)
                throw new ArgumentNullException("firstValue", "Не указано firstValue");
            if (secondValue == null)
                throw new ArgumentNullException("secondValue", "CreateBetween: Не указано secondValue");

            TernaryExpression result = new TernaryExpression
                {
                    FirstExpression = field,
                    SecondExpression = firstValue.ToLiteral(),
                    ThirdExpression = secondValue.ToLiteral(),
                    TernaryExpressionType = TernaryExpressionType.Between
                };
            return result;
        }

        /// <summary>
        /// Создание CASE выражения 
        /// </summary>
        /// <param name="inputExpression">Выражение</param>
        /// <param name="whenClauses">Условие when</param>
        /// <param name="elseExpression">Выражение Else</param>
        /// <returns></returns>
        public static CaseExpression CreateCaseExpression(Expression inputExpression, List<WhenClause> whenClauses, Expression elseExpression)
        {
            CaseExpression result = new CaseExpression();
            foreach (WhenClause clause in whenClauses)
            {
                result.WhenClauses.Add(clause);
            }
            if (elseExpression != null)
                result.ElseExpression = elseExpression;
            if (inputExpression != null)
                result.InputExpression = inputExpression;

            return result;
        }


        public static CaseExpression CreateCaseExpression(Expression inputExpression,
                                                          Expression elseExpression, params WhenClause[] whenClauses)
        {
            return CreateCaseExpression(inputExpression, whenClauses.Where(wc=>wc!=null).ToList(), elseExpression);
        }


        public static CaseExpression CreateCaseExpression(Expression inputExpression,
                                                          Expression elseExpression, WhenClause whenClause)
        {
            return CreateCaseExpression(inputExpression, new List<WhenClause>() { whenClause }, elseExpression);
        }

        #region WhenClause
        /// <summary>
        /// Создание WHEN выражения
        /// </summary>
        /// <param name="when">Условие</param>
        /// <param name="then">Выражение при истине условия</param>
        /// <returns></returns>
        public static WhenClause CreateWhenClause(Expression when, Expression then)
        {
            if (when == null)
                throw new ArgumentNullException("when", "Не передано 'Условие'");
            if (then == null)
                throw new ArgumentNullException("then", "Не передано 'Выражение при истине условия'");
            return new WhenClause { WhenExpression = when, ThenExpression = then };
        }
        #endregion

        #region ExistsPredicate
        /// <summary>
        /// Создание предиката EXISTS
        /// </summary>
        /// <param name="selectStatement">Выражение предиката</param>
        /// <returns></returns>
        public static ExistsPredicate CreateExistsPredicate(SelectStatement selectStatement)
        {
            return new ExistsPredicate { Subquery = selectStatement.ToSubquery() };
        }
        #endregion

        public static TSqlBatch ParseToBatch(string sql)
        {
            var parser = new TSql100Parser(false);
            IList<ParseError> errors;
            IScriptFragment sqlFragment = parser.Parse(new StringReader(sql),
                                                       out errors);
            if (errors.Count > 0)
            {
                throw new SqlParseException(
                    String.Format("Ошибка синтаксичечкого разбора sql: '{0}' <br/> {1}",
                                  sql, errors.ToString(",", pe => pe.Message)))
                {
                    Errors = errors
                };
            }
            if (((TSqlScript)sqlFragment).Batches.Count == 1)
                return ((TSqlScript)sqlFragment).Batches[0];
            else
                throw new InvalidOperationException(
                    String.Format("sql содержит более одного батча: '{0}' <br/>",
                                  sql));

        }

        public static TStatement Parse<TStatement>(string sql) where TStatement : TSqlStatement
        {

            var batch = ParseToBatch(sql);
            if (batch.Statements.Count == 1)
            {
                var result = batch.Statements[0] as TStatement;
                if (result == null)
                    throw new InvalidOperationException(
                        String.Format("sql не является  {1}: '{0}' <br/>",
                                      sql, typeof(TStatement).Name));
                return result;


            }
            throw new InvalidOperationException(
                String.Format("sql содержит более одного statement: '{0}' <br/>",
                              sql));

        }

    }


    public static class Helper<TLeftPropertyNameTarget, TRightPropertyNameTarget>
    {
        /// <summary>
        /// Создание объекта Expression вида field1 (операция сравнения) field2
        /// </summary>
        /// <param name="rightFieldNameLambda"></param>
        /// <param name="binaryExpressionType">Тип сравнения</param>
        /// <param name="leftFieldNameLambda"></param>
        /// <returns>BinaryExpression</returns>
        public static BinaryExpression CreateBinaryExpression<TLeftProperty, TRightProperty>(Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda,
                                                                                            Expression<Func<TRightPropertyNameTarget, TRightProperty>> rightFieldNameLambda,
                                                                                            BinaryExpressionType binaryExpressionType)
        {
            return Helper.CreateBinaryExpression(typeof(TLeftPropertyNameTarget).Name,
                                                 Reflection.Property(leftFieldNameLambda).Name,
                                                 typeof(TRightPropertyNameTarget).Name,
                                                 Reflection.Property(rightFieldNameLambda).Name, binaryExpressionType);

        }


        /// <summary>
        /// Создание объекта Expression вида field1 (операция сравнения) field2
        /// </summary>
        /// <param name="leftFieldNameLambda"></param>
        /// <param name="rightLiteral"></param>
        /// <param name="binaryExpressionType">Тип сравнения</param>
        /// <returns>BinaryExpression</returns>
        public static BinaryExpression CreateBinaryExpression<TLeftProperty>(Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Literal rightLiteral, BinaryExpressionType binaryExpressionType)
        {
            return Helper.CreateBinaryExpression(Helper.CreateColumn(typeof(TLeftPropertyNameTarget).Name,
                                                 Reflection.Property(leftFieldNameLambda).Name),
                                                 rightLiteral, binaryExpressionType);

        }

    }
}
