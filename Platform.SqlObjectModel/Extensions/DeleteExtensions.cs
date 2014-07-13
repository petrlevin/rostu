using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;

namespace Platform.SqlObjectModel.Extensions
{
    /// <summary>
    /// Расширения для объекта DeleteStatement
    /// </summary>
    public static class DeleteExtensions
    {
        /// <summary>
        /// Добавляет в запрос выражение WHERE или происоединяет к существующему Тип соединения AND
        /// </summary>
        /// <param name="deleteStatement">Выражение, в которое присходит добавление</param>
        /// <param name="searchCondition">Условие поиска</param>
        /// <returns>DeleteStatement</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, Expression searchCondition)
        {
            return Where(deleteStatement,BinaryExpressionType.And, searchCondition);
        }

        /// <summary>
        /// Добавляет в запрос выражение WHERE или происоединяет к существующему
        /// </summary>
        /// <param name="deleteStatement">Выражение, в которое присходит добавление</param>
        /// <param name="joinType">Тип соединения с существующим выражением (только AND или OR)</param>
        /// <param name="searchCondition">Условие поиска</param>
        /// <returns>DeleteStatement</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, BinaryExpressionType joinType, Expression searchCondition)
        {
            if (deleteStatement == null)
                throw new ArgumentNullException("deleteStatement", "Передан пустой deleteStatement");
            if (searchCondition == null)
				throw new ArgumentNullException("searchCondition", "Передан пустой searchCondition");
            if (joinType != BinaryExpressionType.Or && joinType != BinaryExpressionType.And)
                throw new ArgumentException("Допускается только AND или OR", "joinType");

            if (deleteStatement.WhereClause == null || deleteStatement.WhereClause.SearchCondition == null)
                deleteStatement.WhereClause = new WhereClause { SearchCondition = searchCondition };
            else
                deleteStatement.WhereClause.SearchCondition = new BinaryExpression
                    {
                        BinaryExpressionType = joinType,
                        FirstExpression =
                            deleteStatement.WhereClause.
                                SearchCondition,
                        SecondExpression = searchCondition
                    };

            return deleteStatement;
        }
    }
}
