using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;

namespace Platform.SqlObjectModel
{
    /// <summary>
    /// Класс реализующий построение рекурсивного запроса с использование конструкции WITH
    /// </summary>
    public class RecursiveSelect : BaseSelect
    {
        /// <summary>
        /// Поле для организации рекурсии
        /// </summary>
        private readonly string _reqursiveFieldName;

        /// <summary>
        /// Поле для организации рекурсии
        /// </summary>
        private readonly string _parentReqursiveFieldName;

        private readonly string _namePartWith;

		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="fieldsName">Споисок полей разделенных запятой</param>
		/// <param name="schemaName">Имя схемы</param>
		/// <param name="tableName">Имя таблицы</param>
		/// <param name="namePartWith">Имя WITH конструкции</param>
		/// <param name="aliasName">Алиас таблицы</param>
		/// <param name="reqursiveFieldName">Поле определяющее элемент</param>
		/// <param name="parentReqursiveFieldName">Поле ссылающееся на родителя</param>
        public RecursiveSelect(string fieldsName, string schemaName, string tableName, string namePartWith,
                               string aliasName, string reqursiveFieldName, string parentReqursiveFieldName) :
            this(String.IsNullOrWhiteSpace(fieldsName) ? new List<string>() : fieldsName.Split(',').Select(a=> a.Trim()).ToList()
             , schemaName, tableName, namePartWith,
                                aliasName, reqursiveFieldName, parentReqursiveFieldName)
        {

        }

        /// <summary>
        /// Конструктор с заполнением минимально необходимого набора данных для формирования рекурсивного запроса
        /// </summary>
        /// <param name="fieldsName"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="namePartWith"> </param>
        /// <param name="aliasName"></param>
        /// <param name="reqursiveFieldName"></param>
        /// <param name="parentReqursiveFieldName"> </param>
        public RecursiveSelect(IList<string> fieldsName, string schemaName, string tableName, string namePartWith, string aliasName, string reqursiveFieldName, string parentReqursiveFieldName)
        {
            if (fieldsName == null)
                fieldsName = new List<string>();
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentNullException("tableName", "RecursiveSelect: передан пустой tableName");
            if (string.IsNullOrWhiteSpace(namePartWith))
				throw new ArgumentNullException("namePartWith", "RecursiveSelect: передан пустой namePartWith");
            if (string.IsNullOrWhiteSpace(reqursiveFieldName))
                throw new ArgumentNullException("reqursiveFieldName", "RecursiveSelect: передан пустой reqursiveFieldName");
            if (string.IsNullOrWhiteSpace(parentReqursiveFieldName))
                throw new ArgumentNullException("parentReqursiveFieldName", "RecursiveSelect: передан пустой parentReqursiveFieldName");

            FieldsName = fieldsName;
            SchemaName = schemaName;
            TableName = tableName;
            AliasName = aliasName;
            _reqursiveFieldName = reqursiveFieldName;
            _parentReqursiveFieldName = parentReqursiveFieldName;
            _namePartWith = namePartWith;
        }

        /// <summary>
        /// Метод реализующий построение рекурсивного запроса с использование конструкции WITH
        /// </summary>
        /// <returns>SelectStatement</returns>
        public override SelectStatement GetQuery()
        {
            if (!FieldsName.Any())
				throw new ArgumentException("FieldsName не содержит элементов");
			SelectStatement resut = new SelectStatement { WithCommonTableExpressionsAndXmlNamespaces = _getWithClause() };

            List<TSqlFragment> fieldsCte = FieldsName.Select(s => Helper.CreateColumn(AliasName, s)).Cast<TSqlFragment>().ToList();

            resut.QueryExpression = Helper.CreateQuerySpecification(
                new TableSource[] { Helper.CreateSchemaObjectTableSource("", _namePartWith, AliasName) }, fieldsCte, null, null,
                null, null);


            return resut;
        }

        /// <summary>
		/// Возвращает конструкцию WITH с рекурсивным запросом
        /// </summary>
        /// <returns></returns>
        private WithCommonTableExpressionsAndXmlNamespaces _getWithClause()
        {
            CommonTableExpression commonTable = GetCommonTable();

            WithCommonTableExpressionsAndXmlNamespaces withCommonTableExpressions = new WithCommonTableExpressionsAndXmlNamespaces();
            withCommonTableExpressions.CommonTableExpressions.Add(commonTable);
            return withCommonTableExpressions;
        }

        public CommonTableExpression GetCommonTable(bool withFields, SelectStatement parentSelect, bool withParent = true)
        {
            return GetCommonTable(withFields, firstQuery =>
                                                  {
                                                      if (!withParent)
                                                          firstQuery.AddWhere(BinaryExpressionType.And,
                                                                              Helper.CreateInPredicate(Helper.CreateColumn("a",
                                                                                                       _parentReqursiveFieldName),
                                                                                                       parentSelect
                                                                                                           .ToSubquery()));
                                                      else
                                                          firstQuery.AddWhere(BinaryExpressionType.And,
                                                                              Helper.CreateInPredicate(Helper.CreateColumn("a", "id"),
                                                                                                       parentSelect
                                                                                                           .ToSubquery()));


                                                  }
                );
            
        }

        public CommonTableExpression GetCommonTable(bool withFields = true, Literal parent = null,
                                                    bool withParent = true)
        {
            return GetCommonTable(withFields, firstQuery =>
                                                  {
                                                      if (parent == null)
                                                          firstQuery.AddWhere(BinaryExpressionType.And,
                                                                              Helper.CreateCheckFieldIsNull("a",
                                                                                                            _parentReqursiveFieldName));
                                                      else if (!withParent)
                                                          firstQuery.AddWhere(BinaryExpressionType.And,
                                                                              Helper.CreateBinaryExpression(
                                                                                  ("a." + _parentReqursiveFieldName)
                                                                                      .ToColumn(), parent,
                                                                                  BinaryExpressionType.Equals));
                                                      else
                                                          firstQuery.AddWhere(BinaryExpressionType.And,
                                                                              Helper.CreateBinaryExpression(
                                                                                  "a.id".ToColumn(), parent,
                                                                                  BinaryExpressionType.Equals));

                                                  }
                );
        }

        private CommonTableExpression GetCommonTable(bool withFields, Action<QuerySpecification> addWhereAction)
        {
            SchemaObjectTableSource firstTableSource = Helper.CreateSchemaObjectTableSource(SchemaName, TableName, "a");

            List<TSqlFragment> internalFields = new List<TSqlFragment>(FieldsName.Select(a => Helper.CreateColumn("a", a)));
            if (FieldsName.All(a => a != _reqursiveFieldName))
                internalFields.Add(Helper.CreateColumn("a", _reqursiveFieldName));
            if (FieldsName.All(a => a != _parentReqursiveFieldName))
                internalFields.Add(Helper.CreateColumn("a", _parentReqursiveFieldName));

            QuerySpecification firstQuery = Helper.CreateQuerySpecification(firstTableSource, internalFields);

            addWhereAction(firstQuery);

            QuerySpecification reqursiveQuery = Helper.CreateQuerySpecification(firstTableSource, internalFields);
            reqursiveQuery.AddJoin(QualifiedJoinType.Inner, Helper.CreateSchemaObjectTableSource("", _namePartWith, "b"),
                                   Helper.CreateBinaryExpression("a", _parentReqursiveFieldName, "b", "id",
                                                                 BinaryExpressionType.Equals));

            Subquery subquery = firstQuery.Union(reqursiveQuery, true).ToSubquery();
            return withFields ? subquery.ToCommonTableExpression(_namePartWith, FieldsName.ToList()) : subquery.ToCommonTableExpression(_namePartWith);
        }
    }
}
