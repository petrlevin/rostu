using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Decorators;
using Platform.Dal.QueryBuilders;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.Decorators
{
    [ExcludeFromCodeCoverage]
    [TestFixture]
    class AddHierarcyFilterTests
    {
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }

        /// <summary>
        /// Тест написан перед рефакторингом декоратора AddHierarcyFilter
        /// </summary>
        [Test]
        public void Test1()
        {
            #region data
            var srcSql = @"
            SELECT [a].[id],
                   [a].[idPublicLegalFormation],
                   [a].[idOrganization],
                   [a].[Caption],
                   [a].[idSBPType],
                   [a].[isFounder],
                   [a].[idParent],
                   [a].[idKVSR],
                   [a].[ValidityFrom],
                   [a].[ValidityTo],
                   [a].[idRoot],
                   [a].[idRefStatus],
                   [b].[Description] AS [idOrganization_Description],
                   [c].[Caption] AS [idParent_Description],
                   [d].[Description] AS [idKVSR_Description],
                   [e].[Caption] AS [idRoot_Description]
            FROM   [ref].[SBP] AS [a]
                   INNER JOIN
                   [ref].[Organization] AS [b]
                   ON [a].[idOrganization] = [b].[id]
                   LEFT OUTER JOIN
                   [ref].[SBP] AS [c]
                   ON [a].[idParent] = [c].[id]
                   LEFT OUTER JOIN
                   [ref].[KVSR] AS [d]
                   ON [a].[idKVSR] = [d].[id]
                   LEFT OUTER JOIN
                   [ref].[SBP] AS [e]
                   ON [a].[idRoot] = [e].[id]
            WHERE  ((([a].[idPublicLegalFormation] = 1))
                    AND ([a].[idSBPType] IN (3, 4, 5)))
                   AND [a].[idRefStatus] = 2";

            var expectedSql = @"
            WITH   [BeforeAddHierarcyFilter]
            AS     (SELECT [a].[id],
                           [a].[idPublicLegalFormation],
                           [a].[idOrganization],
                           [a].[Caption],
                           [a].[idSBPType],
                           [a].[isFounder],
                           [a].[idParent],
                           [a].[idKVSR],
                           [a].[ValidityFrom],
                           [a].[ValidityTo],
                           [a].[idRoot],
                           [a].[idRefStatus],
                           [b].[Description] AS [idOrganization_Description],
                           [c].[Caption] AS [idParent_Description],
                           [d].[Description] AS [idKVSR_Description],
                           [e].[Caption] AS [idRoot_Description],
                           1 AS [IsSelectable]
                    FROM   [ref].[SBP] AS [a]
                           INNER JOIN
                           [ref].[Organization] AS [b]
                           ON [a].[idOrganization] = [b].[id]
                           LEFT OUTER JOIN
                           [ref].[SBP] AS [c]
                           ON [a].[idParent] = [c].[id]
                           LEFT OUTER JOIN
                           [ref].[KVSR] AS [d]
                           ON [a].[idKVSR] = [d].[id]
                           LEFT OUTER JOIN
                           [ref].[SBP] AS [e]
                           ON [a].[idRoot] = [e].[id]
                    WHERE  ((([a].[idPublicLegalFormation] = 1))
                            AND ([a].[idSBPType] IN (3, 4, 5)))
                           AND [a].[idRefStatus] = 2),
                   [AddHierarcyFilter]
            AS     (SELECT [a].[id],
                           [a].[idPublicLegalFormation],
                           [a].[idOrganization],
                           [a].[Caption],
                           [a].[idSBPType],
                           [a].[isFounder],
                           [a].[idParent],
                           [a].[idKVSR],
                           [a].[ValidityFrom],
                           [a].[ValidityTo],
                           [a].[idRoot],
                           [a].[idRefStatus],
                           [a].[IsSelectable]
                    FROM   [BeforeAddHierarcyFilter] AS [a]
                    UNION ALL
                    SELECT [a].[id],
                           [a].[idPublicLegalFormation],
                           [a].[idOrganization],
                           [a].[Caption],
                           [a].[idSBPType],
                           [a].[isFounder],
                           [a].[idParent],
                           [a].[idKVSR],
                           [a].[ValidityFrom],
                           [a].[ValidityTo],
                           [a].[idRoot],
                           [a].[idRefStatus],
                           0 AS [IsSelectable]
                    FROM   [ref].[SBP] AS [a]
                           INNER JOIN
                           [AddHierarcyFilter] AS [b]
                           ON [a].[id] = [b].[idParent]),
                   [AfterAddHierarcyFilter]
            AS     (SELECT   [a].[id],
                             [a].[idPublicLegalFormation],
                             [a].[idOrganization],
                             [a].[Caption],
                             [a].[idSBPType],
                             [a].[isFounder],
                             [a].[idParent],
                             [a].[idKVSR],
                             [a].[ValidityFrom],
                             [a].[ValidityTo],
                             [a].[idRoot],
                             [a].[idRefStatus],
                             SUM([a].[IsSelectable]) AS [IsSelectable]
                    FROM     [AddHierarcyFilter] AS [a]
                    WHERE    [a].[idParent] IS NULL
                    GROUP BY [a].[id], [a].[idPublicLegalFormation], [a].[idOrganization], [a].[Caption], [a].[idSBPType], [a].[isFounder], [a].[idParent], [a].[idKVSR], [a].[ValidityFrom], [a].[ValidityTo], [a].[idRoot], [a].[idRefStatus])
            SELECT [a].[id],
                   [a].[idPublicLegalFormation],
                   [a].[idOrganization],
                   [a].[Caption],
                   [a].[idSBPType],
                   [a].[isFounder],
                   [a].[idParent],
                   [a].[idKVSR],
                   [a].[ValidityFrom],
                   [a].[ValidityTo],
                   [a].[idRoot],
                   [a].[idRefStatus],
                   [a].[IsSelectable],
                   (SELECT CAST (COUNT(1) AS BIT)
                    FROM   [ref].[SBP]
                    WHERE  [idParent] = [a].[id]) AS [isGroup]
            FROM   [AfterAddHierarcyFilter] AS [a]";
            #endregion

            SelectQueryBuilder builder = getInitialBuilder();

            string result = decorateAndRender(srcSql, builder);
            Assert.AreEqual(expectedSql.ToTSqlStatement().Render(), result);
        }

        private SelectQueryBuilder getInitialBuilder()
        {
            Entity entity = new Entity
            {
                EntityType = EntityType.Reference,
                Name = "SBP",
                Fields = new List<EntityField>
                    {
                        new EntityField { Name = "id" }, 
                        new EntityField { Name = "idPublicLegalFormation" }, 
                        new EntityField { Name = "idOrganization" }, 
                        new EntityField { Name = "Caption" }, 
                        new EntityField { Name = "idSBPType" }, 
                        new EntityField { Name = "isFounder" }, 
                        new EntityField { Name = "idParent" }, 
                        new EntityField { Name = "idKVSR" }, 
                        new EntityField { Name = "ValidityFrom" }, 
                        new EntityField { Name = "ValidityTo" }, 
                        new EntityField { Name = "idRoot" }, 
                        new EntityField { Name = "idActualItem" }, 
                        new EntityField { Name = "idRefStatus" }, 
                        new EntityField { Name = "tpSBP_Blank" }, 
                        new EntityField { Name = "tpSBP_PlanningPeriodsInDocumentsAUBU" }, 
                        new EntityField { Name = "tpBlankHistorys" }, 
                    }
            };
            return new SelectQueryBuilder(entity);
        }

        private string decorateAndRender(string source, SelectQueryBuilder builder)
        {
            AddHierarcyFilter decorator = new AddHierarcyFilter("idParent", null);
            var statement = decorator.Decorate(source.ToTSqlStatement(), builder);
            return statement.Render();
        }
    }
}
