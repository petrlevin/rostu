using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic.Registry;
using Platform.Dal;
using Platform.Dal.Serialization;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;
using Platform.Utils;
using Select = Platform.SqlObjectModel.Select;

namespace Platform.BusinessLogic.Activity.Operations.Serialization
{
    /// <summary>
    /// Построитель запроса для восстановления документа из сохраненной XML копии
    /// </summary>
    public class RestoreBuilder
    {

        #region Public

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        public RestoreBuilder(IEntity entity)
        {
            _entity = entity;
        }

        /// <summary>

        /// </summary>
        /// <returns></returns>
        public RestoreCommands Build()
        {
            var result = new RestoreCommands(BuildMainBatch().Render());
            BuildInsertTablePartsBatch().ForEach(
                b => result.AddInsert(String.Format("[tp].[{0}]", b.Key), b.Value.Render())
                );
            return result;

        }

        #endregion


        #region Private

        private readonly IEntity _entity;

        private TSqlBatch BuildMainBatch()
        {
            var result = new TSqlBatch();
            result.Statements.Add(BuildStroredVariableDeclare());
            result.Statements.Add(BuildStoredVariableSet());
            result.Statements.Add(BuildUpdateThis());
            Fields.For(_entity, Options.Multilink).ForEach( ml=>
                result.Statements.Add(BuildDeleteMultilink(ml))
                );
            Fields.For(_entity, Options.TableParts).ForEach(tp =>
                    {
                        result.Statements.Add(BuildUpdateTablepart(tp));
                        result.Statements.Add(BuildDeleteTablepart(tp));
                    }
                );

            Fields.For(_entity, Options.Multilink).ForEach(
                ml => result.Statements.Add(BuildInsertMultilink(ml)));
            return result;

        }





        private List<KeyValuePair<String,TSqlBatch>> BuildInsertTablePartsBatch()
        {
            return BuildInsertBatch(BuildInsertTablepart,Options.TablePartsMasterFirst);
        }


        private List<KeyValuePair<String, TSqlBatch>> BuildInsertBatch(Func<IEntityField,InsertStatement> build , Options fieldSelectOptions  )
        {
            var result = new List<KeyValuePair<String, TSqlBatch>>();
            Fields.For(_entity, fieldSelectOptions).ForEach(field =>
            {
                var batch = new TSqlBatch();
                batch.Statements.Add(BuildStroredVariableDeclare());
                batch.Statements.Add(BuildStoredVariableSet());
                batch.Statements.Add(build(field));
                result.Add(new KeyValuePair<string, TSqlBatch>(field.EntityLink.Name, batch));
            });

            return result;
        }


        private InsertStatement BuildInsertMultilink(IEntityField mlField)
        {
            var multiLink = mlField.EntityLink;
            var rightField = MultilinkHelper.GetRightMultilinkField(multiLink, _entity.Id);
            var leftField = MultilinkHelper.GetLeftMultilinkField(multiLink, _entity.Id);

            var statement = new InsertStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("ml", multiLink.Name);
            statement.Target = target;
            var sourceQs = new QuerySpecification();
            statement.Columns.Add(rightField.Name.ToColumn());
            statement.Columns.Add(leftField.Name.ToColumn());
            sourceQs.SelectElements.Add(CreateValueFunctionCall(rightField.SqlType));
            sourceQs.SelectElements.Add(SerializationCommandFactory.GetThisParameter());
            sourceQs.FromClauses.Add(CreateVariableTableSource(String.Format("/root/{0}/{1}", mlField.Name,rightField.Name)));
            var inPredicate = new InPredicate()
            {
                Expression = CreateValueFunctionCall(rightField.SqlType),
                NotDefined = true,
                Subquery =
                    new Select(rightField.Name, "ml", multiLink.Name).GetQuery()
                                                          .Where(BinaryExpressionType.And,
                                                                 Helper.CreateBinaryExpression(
                                                                     leftField.Name.ToColumn(),
                                                                     SerializationCommandFactory
                                                                         .GetThisParameter(),
                                                                     BinaryExpressionType.Equals))
                                                          .ToSubquery()


            };
            sourceQs.AddWhere(inPredicate);
            statement.InsertSource = new SelectStatement()
            {
                QueryExpression = sourceQs
            };
            return statement;
       }


        private InsertStatement BuildInsertTablepart(IEntityField tablePartField)
        {

            var tablePart = tablePartField.EntityLink;
            var statement = new InsertStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("tp", tablePart.Name);
            statement.Target = target;
            var sourceQs = new QuerySpecification();
            Fields.For(tablePart).ForEach(f =>
                                              {
                                                  statement.Columns.Add(f.Name.ToColumn());
                                                  sourceQs.SelectElements.Add(CreateValueFunctionCall(f.SqlType, f.Name));
                                              });

            sourceQs.FromClauses.Add(CreateVariableTableSource(String.Format("/root/{0}/item", tablePartField.Name)));

            var inPredicate = new InPredicate()
                                  {
                                      Expression = CreateValueFunctionCall("INT", "id"),
                                      NotDefined = true,
                                      Subquery =
                                          new Select("id", "tp", tablePart.Name).GetQuery()
                                                                                .Where(BinaryExpressionType.And,
                                                                                       Helper.CreateBinaryExpression(
                                                                                           tablePartField.OwnerField()
                                                                                                         .Name.ToColumn(),
                                                                                           SerializationCommandFactory
                                                                                               .GetThisParameter(),
                                                                                           BinaryExpressionType.Equals))
                                                                                .ToSubquery()


                                  };
            sourceQs.AddWhere(inPredicate);
            statement.InsertSource = new SelectStatement()
                                         {
                                             QueryExpression = sourceQs
                                         };
            return statement;







        }

        private DeleteStatement BuildDeleteMultilink(IEntityField mlField)
        {
            var multiLink = mlField.EntityLink;
            var rightField = MultilinkHelper.GetRightMultilinkField(multiLink, _entity.Id);
            var leftField = MultilinkHelper.GetLeftMultilinkField(multiLink, _entity.Id);
            var statement = new DeleteStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("ml", multiLink.Name);
            statement.Target = target;
            Expression where =
                Helper.CreateBinaryExpression(
                    Helper.CreateFunctionCall("exist",
                                              String.Format("/root/{0}/{1}[text()  = sql:column(\"{1}\")]",
                                                            mlField.Name,rightField.Name).ToLiteral(), null, "@stored"), 0.ToLiteral(),
                    BinaryExpressionType.Equals);
            where = where.AddExpression(
                Helper.CreateBinaryExpression(leftField.Name.ToColumn(),
                                                                  SerializationCommandFactory.GetThisParameter(),
                                                                  BinaryExpressionType.Equals), BinaryExpressionType.And);
            statement.Where(where);
            return statement;
        }


        private DeleteStatement BuildDeleteTablepart(IEntityField tablePartField)
        {
            var tablePart = tablePartField.EntityLink;

            var statement = new DeleteStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("tp", tablePart.Name);
            statement.Target = target;


            Expression where =
                Helper.CreateBinaryExpression(
                    Helper.CreateFunctionCall("exist",
                                              String.Format("/root/{0}/item/id[text()  = sql:column(\"id\")]",
                                                            tablePartField.Name).ToLiteral(), null, "@stored"), 0.ToLiteral(),
                    BinaryExpressionType.Equals);

            where = where.AddExpression(
                Helper.CreateBinaryExpression(tablePartField.OwnerField().Name.ToColumn(),
                                                                  SerializationCommandFactory.GetThisParameter(),
                                                                  BinaryExpressionType.Equals), BinaryExpressionType.And);

            statement.Where(where);
            return statement;

        }



        private UpdateStatement BuildUpdateTablepart(IEntityField tablePartField)
        {
            var tablePart = tablePartField.EntityLink;

            var statement = new UpdateStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName("tp");
            statement.Target = target;


            statement.FromClauses.Add(BuildTablePartJoin(tablePartField));

            BuildSetClauses(statement, tablePart, "tp");

            //where
            var fields = Fields.For(tablePart, Options.WithoutId);
            Expression where = null;
            fields.ForEach(f => AddTablePartWhere(f, ref @where));
            where =
                where.AddExpression(Helper.CreateBinaryExpression(Helper.CreateColumn("tp", tablePartField.OwnerField().Name),
                                                                  SerializationCommandFactory.GetThisParameter(),
                                                                  BinaryExpressionType.Equals), BinaryExpressionType.And);

            statement.Where(where);
            return statement;

        }

        private QualifiedJoin BuildTablePartJoin(IEntityField tablePartField)
        {
            
            var searchCondition = Helper.CreateBinaryExpression(
                "tp.id".ToColumn(),
                CreateValueFunctionCall("INT", "id"),
                BinaryExpressionType.Equals
                );
            
            var join = Helper.CreateQualifiedJoin(
                Helper.CreateSchemaObjectTableSource("tp", tablePartField.EntityLink.Name, "tp"),
                CreateVariableTableSource(String.Format("/root/{0}/item", tablePartField.Name)),
                searchCondition,
                QualifiedJoinType.Inner
                );
            return @join;
        }

        private static void AddTablePartWhere(IEntityField f, ref Expression @where)
        {
            Expression curExpr = Helper.CreateCheckFieldIsNull("tp", f.Name);
            curExpr = curExpr.AddExpression(
                Helper.CreateBinaryExpression(
                    Helper.CreateFunctionCall("exist", f.Name.ToLiteral(), null, "This"), 1.ToLiteral(), BinaryExpressionType.Equals),
                BinaryExpressionType.And);
            curExpr = curExpr.ToParenthesisExpression();
            @where = @where.AddExpression(curExpr, BinaryExpressionType.Or);


            curExpr = Helper.CreateCheckFieldIsNotNull("tp", f.Name);
            curExpr = curExpr.AddExpression(
                Helper.CreateBinaryExpression(
                    Helper.CreateFunctionCall("exist", String.Format("{0}[text()=sql:column(\"tp.{0}\")]", f.Name).ToLiteral(), null,
                                              "This"), 0.ToLiteral(), BinaryExpressionType.Equals),
                BinaryExpressionType.And);
            curExpr = curExpr.ToParenthesisExpression();
            @where = @where.AddExpression(curExpr, BinaryExpressionType.Or);

        }

        private FunctionCall CreateValueFunctionCall( string fielsSqlType,string fieldName =null )
        {
            var callTarget = new IdentifiersCallTarget();
            callTarget.Identifiers.Add("This".ToIdentifierWithoutQuote());
            var newValue = new FunctionCall()
            {
                CallTarget = callTarget,
                FunctionName = "value".ToIdentifierWithoutQuote(),
            };
            if (fieldName!=null)
                newValue.Parameters.Add(String.Format("({0})[1]", fieldName).ToLiteral());
            else
                newValue.Parameters.Add("text()[1]".ToLiteral());
            newValue.Parameters.Add(fielsSqlType.ToLiteral());
            return newValue;

        }

        private UpdateStatement BuildUpdateThis()
        {
            var updateStatement = new UpdateStatement();
            var target = new SchemaObjectDataModificationTarget();
            target.SchemaObject = Helper.CreateSchemaObjectName(_entity.Schema, _entity.Name);
            updateStatement.Target = target;

            var tableSource = CreateVariableTableSource(@"/root");
            updateStatement.FromClauses.Add(tableSource);

            BuildSetClauses(updateStatement, _entity);

            updateStatement.WhereClause = new WhereClause()
                                              {

                                                  SearchCondition =
                                                      Helper.CreateBinaryExpression("id".ToColumn(),
                                                                                    SerializationCommandFactory.GetThisParameter(),
                                                                                    BinaryExpressionType.Equals)

                                              };

            return updateStatement;
        }

        private void BuildSetClauses(UpdateStatement updateStatement, IEntity entity, string tableAlias = null)
        {
            var fields = Fields.For(entity, Options.WithoutId);

            fields.ForEach(
                f =>
                {
                    var newValue = CreateValueFunctionCall(f.SqlType, f.Name);

                    var setClause = new AssignmentSetClause()
                                        {
                                            AssignmentKind = AssignmentKind.Equals,
                                            Column = tableAlias == null ? f.Name.ToColumn() : String.Format("{0}.{1}", tableAlias, f.Name).ToColumn(),
                                            NewValue = newValue
                                        };
                    updateStatement.SetClauses.Add(setClause);
                }
                );
        }

        private static VariableTableSource CreateVariableTableSource(string parameter)
        {
            var functionCall = new FunctionCall()
                                   {
                                       FunctionName = "nodes".ToIdentifierWithoutQuote(),
                                   };
            functionCall.Parameters.Add(parameter.ToLiteral());

            var tableSource = new VariableTableSource()
                                  {
                                      Name = Stored,
                                      FunctionCall = functionCall,
                                      Alias = "T".ToIdentifierWithoutQuote()
                                  };

            tableSource.Columns.Add("This".ToIdentifierWithoutQuote());
            return tableSource;
        }


        private static Literal Stored
        {
            get { return "@stored".ToLiteral(LiteralType.Variable); }
        }

        private DeclareVariableStatement BuildStroredVariableDeclare()
        {
            var declareStatement = new DeclareVariableStatement();
            var declare = new DeclareVariableElement()
                              {
                                  DataType = new XmlDataType(),
                                  VariableName = Stored
                              };
            declareStatement.Declarations.Add(declare);
            return declareStatement;
        }

        private SelectStatement BuildStoredVariableSet()
        {
            var setStatement = new SelectStatement();
            var qs = new QuerySpecification();
            qs.SelectElements.Add(
                new SelectSetVariable()
                    {
                        VariableName = Stored,
                        Expression = Columns.Data
                    });
            qs.FromClauses.Add(Helper.CreateSchemaObjectTableSource("reg", typeof(SerializedEntityItem).Name));
            qs.AddWhere(Helper.CreateBinaryExpression(Columns.IdTool, SerializationCommandFactory.GetThisParameter(), BinaryExpressionType.Equals));
            qs.AddWhere(Helper.CreateBinaryExpression(Columns.IdToolEntity, _entity.Id.ToLiteral(), BinaryExpressionType.Equals));
            setStatement.QueryExpression = qs;
            return setStatement;

        }

        #endregion


    }
}
