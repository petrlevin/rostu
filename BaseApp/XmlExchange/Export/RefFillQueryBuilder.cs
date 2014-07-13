using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.BusinessLogic;
using Platform.Common;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace BaseApp.XmlExchange.Export
{
    /// <summary>
    /// 
    /// </summary>
    public class RefFillQueryBuilder
    {

        private readonly InsertStatement _source;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dbContext"></param>
        public RefFillQueryBuilder(InsertStatement source, DbContext dbContext = null)
        {
            _source = source;
        }

        public IEnumerable<TSqlStatement> Build()
        {

            
            var @continue = new DeclareVariableStatement();
            @continue.Declarations.Add( new DeclareVariableElement()
                                            {
                                                DataType = SqlDataTypeOption.Int.ToSqlDataType() ,
                                                VariableName = "@ROWCOUNT".ToLiteral(LiteralType.Variable),
                                                InitialValue = 1.ToLiteral() 

                                            });
            var statementList = new StatementList();
            statementList.Statements.Add(_source);
            statementList.Statements.Add(new SetVariableStatement()
                                             {
                                                 AssignmentKind = AssignmentKind.Equals,
                                                 VariableName = "@ROWCOUNT".ToLiteral(LiteralType.Variable),
                                                 Expression = "@@ROWCOUNT".ToLiteral(LiteralType.Variable)

                                             });


            var body = new BeginEndBlockStatement()
                                              {
                                                  StatementList = statementList 
                                                  
                                              };
            var cycle = new WhileStatement()
                                       {
                                           Predicate =
                                               Helper.CreateBinaryExpression("@ROWCOUNT".ToLiteral(LiteralType.Variable), 0.ToLiteral(),
                                                                             BinaryExpressionType.GreaterThan),
                                           Statement = body 


                                       };
            var result = new List<TSqlStatement>();
            result.Add(@continue);
            result.Add(@cycle);
            return result;

        }

    }
}
