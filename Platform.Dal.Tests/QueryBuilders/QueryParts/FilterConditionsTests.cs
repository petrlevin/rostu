using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Data.Schema.ScriptDom.Sql;
using NUnit.Framework;
using Platform.Common;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.SqlObjectModel.Extensions;
using Platform.Unity;

namespace Platform.Dal.Tests.QueryBuilders.QueryParts
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class FilterConditionsTests : DalTestsBase
	{
        [TestFixtureSetUp]
        public void SetUp()
        {
            IoC.InitWith(new DependencyResolverBase());
        }

		[Test]
		public void ToExpression_Sql()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = null, Sql = "select id from cmn.Reference" };
			Assert.AreEqual("[id] IN (SELECT id FROM cmn.Reference)", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Simple_InList()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.InList, Value = new List<int> {1,2}, Type = LogicOperator.Simple };
			Expression expression = filterConditions.ToExpression();
			Assert.AreEqual("[id] IN (1, 2)", expression.Render(options));
		}

		[Test]
		public void ToExpression_Simple_InList_Exception()
		{
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.Simple, Operator = ComparisionOperator.InList };
			Assert.Throws<Exception>(() => filterConditions.ToExpression());
		}

		[Test]
		public void ToExpression_Simple_Like()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "name", Operator = ComparisionOperator.Like, Value = "%строка%", Type = LogicOperator.Simple };
			Assert.AreEqual("[name] LIKE '%строка%'", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Simple_IsNull()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.IsNull, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] IS NULL", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Simple_IsNotNull()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.IsNotNull, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] IS NOT NULL", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Simple_Default_ToBinaryExpressionType()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] = 1", filterConditions.ToExpression().Render(options));
			filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.Less, Value = 1, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] < 1", filterConditions.ToExpression().Render(options));
			filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.LessOrEqual, Value = 1, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] <= 1", filterConditions.ToExpression().Render(options));
			filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.Greater, Value = 1, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] > 1", filterConditions.ToExpression().Render(options));
			filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.GreaterOrEqual, Value = 1, Type = LogicOperator.Simple };
			Assert.AreEqual("[id] >= 1", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Simple_Default_ToBinaryExpressionType_Exception()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = (ComparisionOperator)12, Value = 1, Type = LogicOperator.Simple };
			Assert.Throws<Exception>(() => filterConditions.ToExpression());
		}


		[Test]
		public void ToExpression_Simple_DefaultWithNot()
		{
			FilterConditions filterConditions = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple, Not = true};
			Assert.AreEqual("NOT [id] = 1", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Or()
		{
			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions2 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 2, Type = LogicOperator.Simple };
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.Or, Operands = new List<IFilterConditions> { inFilterConditions1, inFilterConditions2 } };
			Assert.AreEqual("[id] = 1 OR [id] = 2", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_Operands_Exception()
		{
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.Or };
			Assert.Throws<Exception>(() => filterConditions.ToExpression());

			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			filterConditions = new FilterConditions { Type = LogicOperator.Or, Operands = new List<IFilterConditions> {inFilterConditions1}};
			Assert.Throws<Exception>(() => filterConditions.ToExpression());
		}

		[Test]
		public void ToExpression_OrWithNot()
		{
			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions2 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 2, Type = LogicOperator.Simple };
			FilterConditions filterConditions = new FilterConditions { Not = true, Type = LogicOperator.Or, Operands = new List<IFilterConditions> { inFilterConditions1, inFilterConditions2 } };
			Assert.AreEqual("NOT ([id] = 1 OR [id] = 2)", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_And()
		{
			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions2 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 2, Type = LogicOperator.Simple };
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.And, Operands = new List<IFilterConditions> { inFilterConditions1, inFilterConditions2 } };
			Assert.AreEqual("[id] = 1 AND [id] = 2", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_And_Exception()
		{
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.And };
			Assert.Throws<Exception>(() => filterConditions.ToExpression());
		}

		[Test]
		public void ToExpression_AndWithNot()
		{
			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions2 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 2, Type = LogicOperator.Simple };
			FilterConditions filterConditions = new FilterConditions { Not = true, Type = LogicOperator.And, Operands = new List<IFilterConditions> { inFilterConditions1, inFilterConditions2 } };
			Assert.AreEqual("NOT ([id] = 1 AND [id] = 2)", filterConditions.ToExpression().Render(options));
		}

		[Test]
		public void ToExpression_ManyOperands()
		{
			FilterConditions inFilterConditions1 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 1, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions2 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 2, Type = LogicOperator.Simple };
			FilterConditions inFilterConditions3 = new FilterConditions { Field = "id", Operator = ComparisionOperator.Equal, Value = 3, Type = LogicOperator.Simple };
			FilterConditions filterConditions = new FilterConditions { Type = LogicOperator.Or, Operands = new List<IFilterConditions> { inFilterConditions1, inFilterConditions2, inFilterConditions3 } };
			Assert.AreEqual("[id] = 1 OR [id] = 2 OR [id] = 3", filterConditions.ToExpression().Render(options));
		}
	}
}
