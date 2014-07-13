using System;
using System.Linq.Expressions;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils;

namespace Platform.SqlObjectModel.Helpers
{
    /// <summary>
    /// Построитель объединений в запросах
    /// </summary>
    /// <typeparam name="TLeftPropertyNameTarget"></typeparam>
    /// <typeparam name="TThisPropertyNameTarget"></typeparam>
    public static class Join<TLeftPropertyNameTarget, TThisPropertyNameTarget>
    {

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// Алиас присоединяемой таблице будет равен имени присоединяемой таблицы
        /// Алиас таблицы которая уже присутствует в запросе определяется как алиас первой встреченной таблицы указанной как левая
        /// <see cref="leftTableName"/>
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Наименование схемы</param>
        /// <param name="tableName">Наименование таблицы</param>
        /// <param name="leftTableName">Наименование таблицы, которая уже присутствует в запросе</param>
        /// <param name="leftFieldNameLambda">Выражение определяющие поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldNameLambda">Выражение определяющие поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement WithTable<TLeftProperty, TThisProperty>(
            SelectStatement selectStatement, QualifiedJoinType jointType,
            string schemaName, string tableName, string leftTableName,
            Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {
            return selectStatement.JoinWithTable(jointType, schemaName, tableName, leftTableName,
                                                 Reflection.Property(leftFieldNameLambda).Name,
                                                 Reflection.Property(thisFieldNameLambda).Name);
        }


        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// Имя присоединямой таблицы определяется из <see cref="TThisPropertyNameTarget"/>
        /// Алиас присоединяемой таблице будет равен имени присоединяемой таблицы
        /// Алиас таблицы которая уже присутствует в запросе определяется как алиас первой встреченной таблицы указанной как левая
        /// <see cref="TLeftPropertyNameTarget"/>
        /// </summary>
        /// <param name="selectStatement">Выражение, в которое присходит добавление</param>
        /// <param name="jointType">Тип соединения</param>
        /// <param name="schemaName">Наименование схемы</param>
        /// <param name="leftFieldNameLambda">Выражение определяющие поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldNameLambda">Выражение определяющие поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement WithTable<TLeftProperty, TThisProperty>(
            SelectStatement selectStatement, QualifiedJoinType jointType,
            string schemaName, 
            Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {
            return WithTable(selectStatement,jointType, schemaName, typeof(TThisPropertyNameTarget).Name, typeof(TLeftPropertyNameTarget).Name,
                             leftFieldNameLambda,
                             thisFieldNameLambda);
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
        /// <param name="leftFieldNameLambda">Выражение определяющие поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldNameLambda">Выражение определяющие поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Add<TLeftProperty, TThisProperty>(SelectStatement selectStatement, QualifiedJoinType jointType,
                                                                        string schemaName, string tableName, string leftAliasName,
                                                                        Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {
            return Add(selectStatement, jointType,
                       schemaName, tableName, tableName, leftAliasName,
                       leftFieldNameLambda, thisFieldNameLambda);
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
        /// <param name="leftFieldNameLambda">Выражение определяющие поле таблицы, которая уже присутствует в запросе</param>
        /// <param name="thisFieldNameLambda">Выражение определяющие поле присоединяемой таблицы</param>
        /// <returns>SelectStatement</returns>
        public static SelectStatement Add<TLeftProperty, TThisProperty>(SelectStatement selectStatement,
                                                                        QualifiedJoinType jointType,
                                                                        string schemaName, string tableName,
                                                                        string aliasName, string leftAliasName,
                                                                        Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda,
                                                                        Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {

            return selectStatement.Join(jointType, schemaName, tableName, aliasName,leftAliasName,
                                        Reflection.Property(leftFieldNameLambda).Name,
                                        Reflection.Property(thisFieldNameLambda).Name);
        }

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="query"></param>
        /// <param name="jointType"></param>
        /// <param name="schemaName"></param>
        /// <param name="tableName"></param>
        /// <param name="aliasName"></param>
        /// <param name="leftAliasName"></param>
        /// <param name="leftFieldNameLambda"></param>
        /// <param name="thisFieldNameLambda"></param>
        /// <typeparam name="TLeftProperty"></typeparam>
        /// <typeparam name="TThisProperty"></typeparam>
        /// <returns></returns>
        public static QuerySpecification Add<TLeftProperty, TThisProperty>(
            QuerySpecification query, QualifiedJoinType jointType,
            string schemaName, string tableName,string aliasName,string leftAliasName,
            Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {
            query.AddJoin(jointType, schemaName, tableName, aliasName, leftAliasName,
                          Reflection.Property(leftFieldNameLambda).Name,
                          Reflection.Property(thisFieldNameLambda).Name
                );
            return query;
        }

        /// <summary>
        /// Добавляет в запрос выражение JOIN(тип соединения) [схема].[таблица] [алиас] ON [алиас].[поле]=[алиас].[поле]
        /// </summary>
        /// <param name="query"></param>
        /// <param name="jointType"></param>
        /// <param name="schemaName"></param>
        /// <param name="leftFieldNameLambda"></param>
        /// <param name="thisFieldNameLambda"></param>
        /// <typeparam name="TLeftProperty"></typeparam>
        /// <typeparam name="TThisProperty"></typeparam>
        /// <returns></returns>
        public static QuerySpecification Add<TLeftProperty, TThisProperty>(
            QuerySpecification query, QualifiedJoinType jointType,
            string schemaName, 
            Expression<Func<TLeftPropertyNameTarget, TLeftProperty>> leftFieldNameLambda, Expression<Func<TThisPropertyNameTarget, TThisProperty>> thisFieldNameLambda)
        {
            query.AddJoin(jointType, schemaName, typeof(TThisPropertyNameTarget).Name, typeof(TThisPropertyNameTarget).Name, query.GetFirstAliasName(typeof(TLeftPropertyNameTarget).Name),
                          Reflection.Property(leftFieldNameLambda).Name,
                          Reflection.Property(thisFieldNameLambda).Name
                );
            return query;
        }
    }
}
