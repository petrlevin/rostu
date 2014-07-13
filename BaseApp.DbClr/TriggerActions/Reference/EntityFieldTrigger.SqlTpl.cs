using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseApp.DbClr.TriggerActions.Reference
{
    public partial class EntityFieldTrigger
    {
        /// <summary>
        /// Шаблоны SQL команд
        /// </summary>
        private static class SqlTpl
        {
            public const string RenameColumn = "EXECUTE sp_rename N'{0}.{1}.{2}', N'{3}', 'COLUMN'";
            public const string AlterColumnChangeType = "ALTER TABLE [{0}].[{1}] ALTER COLUMN [{2}] {3} {4}";
            public const string DropPk = "IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND name = N'PK_{1}') ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [PK_{1}]";

            /// <summary>
            /// Создание первичного ключа для поля id
            /// </summary>
            internal const string CreatePk =
                "ALTER TABLE [{0}].[{1}] WITH CHECK ADD CONSTRAINT [PK_{1}] PRIMARY KEY CLUSTERED ({2}) WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON);";


            /// <summary>
            /// Команда создания индекса
            /// </summary>
            public const string CreateIndex = "CREATE NONCLUSTERED INDEX [{0}] ON [{1}].[{2}] ([{0}])";

            /// <summary>
            /// Команда создания индекса
            /// </summary>
            public const string DropIndex = "IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[{0}].[{1}]') AND name = N'{2}') " +
                                            "DROP INDEX [{2}] ON [{0}].[{1}] WITH ( ONLINE = OFF )";


            /// <summary>
            /// Добавление столбца в таблицу без значения по умолчанию
            /// </summary>
            public const string CreateColumnId =
                "if not exists(select * from sys.columns where Name = N'{2}' and Object_ID = Object_ID(N'{0}.{1}')) ALTER TABLE [{0}].[{1}] ADD [{2}] {3} IDENTITY(1,1) {4}";

            /// <summary>
            /// Добавление столбца в таблицу без значения по умолчанию
            /// </summary>
            public const string CreateColumnWithoutDefault =
                "if not exists(select * from sys.columns where Name = N'{2}' and Object_ID = Object_ID(N'{0}.{1}')) ALTER TABLE [{0}].[{1}] ADD [{2}] {3} {4}";

            /// <summary>
            /// Добавление столбца в таблицу со значением по умолчанию
            /// </summary>
            public const string CreateColumnWithDefault = "if not exists(select * from sys.columns where Name = N'{2}' and Object_ID = Object_ID(N'{0}.{1}')) ALTER TABLE [{0}].[{1}] ADD [{2}] {3} {4} CONSTRAINT DEFAULT_{1}_{2} DEFAULT {5}";

            /// <summary>
            /// Добавление вычисляемого столбца в таблицу
            /// </summary>
            public const string CreateColumnComputed = "if not exists(select * from sys.columns where Name = N'{2}' and Object_ID = Object_ID(N'{0}.{1}')) ALTER TABLE [{0}].[{1}] ADD [{2}] AS {3} {4}";

            /// <summary>
            /// Создания Foreign Key в таблице для столбца
            /// </summary>
            ///"ALTER TABLE [{0}].[{1}] WITH CHECK ADD CONSTRAINT FK_{4}_{1}_{3} FOREIGN KEY ([{4}]) REFERENCES [{2}].[{3}] (id) ON UPDATE NO ACTION ON DELETE {5}";
            public const string CreateFk =
                "ALTER TABLE [{0}].[{1}] WITH CHECK ADD CONSTRAINT FK_{6} FOREIGN KEY ([{4}]) REFERENCES [{2}].[{3}] (id) ON UPDATE NO ACTION ON DELETE {5}";

            /// <summary>
            /// Удаление значения по умолчанию для столбца
            /// </summary>
            public const string DropDefault = "IF EXISTS (SELECT * FROM dbo.sysobjects WHERE name = 'DEFAULT_{1}_{2}' AND type = 'D') ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [DEFAULT_{1}_{2}]";

            /// <summary>
            /// Создание значения по умолчанию для столбца
            /// </summary>
            public const string CreateDefault = "ALTER TABLE [{0}].[{1}] ADD CONSTRAINT [DEFAULT_{1}_{2}] DEFAULT {3} FOR [{2}]";

            /// <summary>
            /// Удаление Foreign Key в таблице для столбца
            /// </summary>
            public const string DropFk = "IF EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[{0}].[FK_{2}]') AND parent_object_id = OBJECT_ID(N'[{0}].[{1}]')) ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [FK_{2}]";

            /// <summary>
            /// Удаление столбца в таблице
            /// </summary>
            public const string StrDropColumn = "IF EXISTS (SELECT * FROM sys.columns where name='{2}' and object_id=OBJECT_ID(N'[{0}].[{1}]')) ALTER TABLE [{0}].[{1}] DROP COLUMN [{2}]";

            /// <summary>
            /// Удалени строк из таблицы поддержки общих ссылок
            /// </summary>
            public const string DeleteGenericLinks = "DELETE FROM [dbo].[GenericLinks] WHERE [idReferencesEntityField]={0}";

        }
    }
}
