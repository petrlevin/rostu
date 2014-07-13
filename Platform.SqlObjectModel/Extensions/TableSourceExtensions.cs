using System;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
    /// <summary>
    /// Расширения для объектов TableSource
    /// </summary>
    public static class TableSourceExtensions
    {
        /// <summary>
        /// Создает выражение [схема].[текущая таблица] [алиас1] JOIN(тип соединения) [схема].[присоединяемая таблица] [алиас2] ON (выражение соединения)
        /// </summary>
        /// <param name="thisTableSource">Текущая таблица</param>
        /// <param name="joinType">Тип соединения</param>
        /// <param name="tableSource">Присоединяемая таблица</param>
        /// <param name="searchCondition">Условие соединения</param>
        /// <returns>QualifiedJoin</returns>
        public static QualifiedJoin Join(this TableSource thisTableSource, QualifiedJoinType joinType, TableSource tableSource, Expression searchCondition)
        {
            if (thisTableSource == null)
                throw new Exception("Join: передан пустой thisTableSource");
            if (tableSource == null)
                throw new Exception("Join: передан пустой tableSource");
            if (searchCondition == null)
                throw new Exception("Join: передан пустой searchCondition");

            return Helper.CreateQualifiedJoin(thisTableSource, tableSource, searchCondition, joinType);
        }

        /// <summary>
        /// Возвращает Алиас
        /// </summary>
        /// <param name="tableSource">Обрабатываемый TableSource</param>
        /// <returns>string</returns>
        public static string GetAliasName(this TableSource tableSource)
        {
            if (tableSource == null)
                throw new Exception("GetAlias: передан пустой tableSource");

            string result = null;
            if (tableSource is TableSourceWithAlias)
                result = (tableSource as TableSourceWithAlias).GetAliasName();
            else
                throw new Exception("GetAlias: не реализовано для - " + tableSource.GetType());

            return result;
        }





        /// <summary>
        /// Возвращает Алиас
        /// </summary>
        /// <param name="tableSource">Обрабатываемый TableSource</param>
        /// <returns>string</returns>
        private static string GetAliasName(this TableSourceWithAlias tableSource)
        {
            return tableSource.Alias == null ? "" : tableSource.Alias.Value;
        }


        /// <summary>
        /// Возвращает Алиас
        /// </summary>
        /// <param name="join">Обрабатываемый QualifiedJoin</param>
        /// <param name="currentMax">Строка с которой присходит сравнение</param>
        /// <returns>string</returns>
        public static string GetMaxAlias(this QualifiedJoin join, string currentMax)
        {
            if (join == null)
                throw new Exception("GetMaxAlias: передан пустой tableSource");
            if (join.FirstTableSource == null)
                throw new Exception("GetMaxAlias: у переданного tableSource пустой FirstTableSource");
            if (join.SecondTableSource == null)
                throw new Exception("GetMaxAlias: у переданного tableSource пустой SecondTableSource");

            string result = currentMax;
            string first = "";
            if ((join.FirstTableSource is SchemaObjectTableSource) || (join.FirstTableSource is QueryDerivedTable))
                first = join.FirstTableSource.GetAliasName();
            if (join.FirstTableSource is QualifiedJoin)
                first = (join.FirstTableSource as QualifiedJoin).GetMaxAlias(result);
            if (first.IsValidString() && String.Compare(first, currentMax, StringComparison.OrdinalIgnoreCase) > 0)
                result = first;
            if (join.SecondTableSource is TableSourceWithAlias)
            {
                string second = (join.SecondTableSource as TableSourceWithAlias).GetAliasName();
                if (second.IsValidString() && String.Compare(second, currentMax, StringComparison.OrdinalIgnoreCase) > 0)
                    result = second;
            }
            return result;
        }

        public static bool HasTable(this QueryDerivedTable derivedTable, string tableName)
        {

            var querySpecification = derivedTable.Subquery.QueryExpression as QuerySpecification;
            if (querySpecification == null)
                return false;
            
            foreach (var tableSource in querySpecification.FromClauses)
            {
                if (tableSource.HasTable(tableName))
                    return true;
            }
            return false;


        }

        public static bool HasTable(this SchemaObjectTableSource schemaObjectTableSource, string tableName)
        {
            return String.Compare(schemaObjectTableSource.SchemaObject.BaseIdentifier.Value, tableName,
                           StringComparison.OrdinalIgnoreCase) == 0
            ;
        }

        public static bool HasTable(this QualifiedJoin qualifiedJoin, string tableName)
        {
            return qualifiedJoin.FirstTableSource.HasTable(tableName) ||
                   qualifiedJoin.SecondTableSource.HasTable(tableName);
        }



        public static bool HasTable(this TableSource tableSource, string tableName)
        {
            var schemaObjectTableSource = tableSource as SchemaObjectTableSource;
            if (schemaObjectTableSource != null)
                return schemaObjectTableSource.HasTable(tableName);

            var derivedTableChild = tableSource as QueryDerivedTable;
            if (derivedTableChild != null)
                return derivedTableChild.HasTable(tableName);

            var qualifiedJoin = tableSource as QualifiedJoin;
            if (qualifiedJoin != null)
                return qualifiedJoin.HasTable(tableName);

            return false;

        }


        public static string GetAliasNameOnTable(this QualifiedJoin join, string tableName,
                                                 bool includeSourceSubqueries = false)
        {
            var result = GetAliasOnTable(join, tableName, includeSourceSubqueries);
            return result != null ? result.Value : null;
        }


        public static TableSourceWithAliasAndColumns GetTableSourceWithColumns(this QualifiedJoin join, string aliasName)
        {
            return GetTableSource<TableSourceWithAliasAndColumns>(join, aliasName);
        }

        public static TableSourceWithAlias GetTableSource(this QualifiedJoin join, string aliasName)
        {
            return GetTableSource<TableSourceWithAlias>(join, aliasName);
        }

        public static TTableSourceWithAlias GetTableSource<TTableSourceWithAlias>(this QualifiedJoin join, string aliasName) where TTableSourceWithAlias : TableSourceWithAlias
        {
            var ftsWa = join.FirstTableSource as TTableSourceWithAlias;
            if ((ftsWa!=null) &&
                (ftsWa.GetAliasName().ToLower() == aliasName.ToLower()))
                return ftsWa;
            var ftsQj = join.FirstTableSource as QualifiedJoin;
            if (ftsQj != null)
            {
                var result = ftsQj.GetTableSource<TTableSourceWithAlias>(aliasName);
                if (result != null)
                    return result;
            }
            var stsWa = join.SecondTableSource as TTableSourceWithAlias;
            if ((stsWa != null) &&
                (stsWa.GetAliasName().ToLower() == aliasName.ToLower()))
                return stsWa;
            var stsQj = join.SecondTableSource as QualifiedJoin;
            if (stsQj != null)
            {
                var result = stsQj.GetTableSource<TTableSourceWithAlias>(aliasName);
                if (result != null)
                    return result;
            }
            return null;



        }
        

        /// <summary>
        /// Возвращает Алиас
        /// </summary>
        /// <param name="join">Выражение</param>
        /// <param name="tableName">Имя таблицы</param>
        /// <returns>string</returns>
        public static Identifier GetAliasOnTable(this QualifiedJoin join, string tableName ,bool includeSourceSubqueries=false )
        {
            Identifier result = null;
            if (join.FirstTableSource is SchemaObjectTableSource)
            {
                if (join.FirstTableSource.HasTable(tableName))
                {
                    result = ((SchemaObjectTableSource)join.FirstTableSource).Alias;
                }
            }
            else if (join.FirstTableSource is QualifiedJoin)
            {
                result = (join.FirstTableSource as QualifiedJoin).GetAliasOnTable(tableName, includeSourceSubqueries);
            }
            else if ((join.FirstTableSource is QueryDerivedTable) && (includeSourceSubqueries) && (join.FirstTableSource.HasTable(tableName)))
            {
                result = ((QueryDerivedTable)join.FirstTableSource).Alias;
            }
            if (result==null)
            {
                if (join.SecondTableSource is SchemaObjectTableSource)
                {
                    if (join.SecondTableSource.HasTable(
                            tableName))
                    {
                        result = (join.SecondTableSource as SchemaObjectTableSource).Alias;
                    }

                }
                else if ((join.SecondTableSource is QueryDerivedTable) && (includeSourceSubqueries) && (join.SecondTableSource.HasTable(tableName)))
                {
                    result = ((QueryDerivedTable)join.SecondTableSource).Alias;
                }

                /*законетил, помоему такая ситуация не возможна
                else if (join.SecondTableSource is QualifiedJoin)
                {
                    result = (join.SecondTableSource as QualifiedJoin).GetAliasOnTable(tableName);
                }*/
            }
            return result;
        }


        /// <summary>
        /// Преобразование объекта SchemaObjectName к SchemaObjectDataModificationTarget
        /// </summary>
        /// <param name="schemaObjectName">Преобразуемый объект</param>
        /// <returns></returns>
        public static SchemaObjectDataModificationTarget ToSchemaObjectDataModificationTarget(this SchemaObjectName schemaObjectName)
        {
            return new SchemaObjectDataModificationTarget { SchemaObject = schemaObjectName };
        }
    }
}
