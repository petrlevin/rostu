using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Schema.ScriptDom.Sql;
using Platform.Common.Exceptions;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.Decorators.Abstract;
using Platform.Log;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.SqlObjectModel;
using Platform.SqlObjectModel.Extensions;

namespace Platform.Dal.Decorators
{

    /// <summary>
    /// Декоратор добавляющий caption самой сущности
    /// </summary>
    public class AddThisCaption: SelectDecoratorBase
    {
        private readonly string _captionAlias;

        /// <summary>
        /// Алис заголовка
        /// </summary>
        public string CaptionAlias
        {
            get { return _captionAlias; }
        }
        
        private readonly OnAbsent _onAbsent;

        public AddThisCaption(string captionAlias="__caption")
            : this(OnAbsent.SelectId(), captionAlias)
        {
            
        }

        public AddThisCaption(OnAbsent onAbsent,string captionAlias="__caption")
        {
            _captionAlias = captionAlias;
            _onAbsent = onAbsent;
        }

        protected override TSqlStatement Decorate(SelectStatement source, IQueryBuilder queryBuilder)
        {
            
            var query = source.QueryExpression as QuerySpecification;
            if (query == null)
                throw new PlatformException(
                    String.Format(
                        "Переданный запрос ('{0}') не является спецификацией (\"QuerySpecification\").Применение декоратора невозможно.",
                        source.Render()));
            var selectQueryBuilder = queryBuilder as ISelectQueryBuilder;
            if (query == null)
                throw new PlatformException(
                    String.Format(
                        "Переданный поcтроитель  не является пострителем выборки  (\"ISelectQueryBuilder\").Применение декоратора невозможно."
                        ));


            GetStrategy(queryBuilder).Decorate(source,query,selectQueryBuilder,_captionAlias);

            return source;

        }

        private Strategy GetStrategy(IQueryBuilder queryBuilder)
        {
            var entity = queryBuilder.Entity;
            var field =  entity.RealFields.FirstOrDefault(ef => ef.IsCaption.HasValue&& ef.IsCaption.Value);
            
            if (field != null)
                return new Default(field);
            else
                return _onAbsent;
        }

        #region Стратегии

        public abstract class Strategy
        {
            protected ISelectQueryBuilder queryBuilder;
            public abstract void Decorate(SelectStatement source,QuerySpecification query, ISelectQueryBuilder queryBuilder, string captionAlias);

        }


        class Default: Strategy
        {
            private IEntityField _entityField;

            public Default(IEntityField entityField)
            {
                _entityField = entityField;
            }

            public override void Decorate(SelectStatement source,QuerySpecification query, ISelectQueryBuilder queryBuilder, string captionAlias)
            {
                
                var alias = query.GetFirstAliasName(queryBuilder.Entity.Name) ?? queryBuilder.AliasName;
                var fieldName = _entityField.Name;
                var sourceColumn = source.GetSourceColumn(alias, fieldName);
                if (sourceColumn!=null)
                    query.AddFields(new List<SelectColumn>(){Helper.CreateColumn(sourceColumn,
                                             captionAlias)},QueryExtensions.OnExists.Ignore);
                else
                {
                    var entity = queryBuilder.Entity;
                    query.AddJoin(QualifiedJoinType.Inner, entity.Schema,entity.Name,"thisCaption",alias,"id","id");
                    query.AddFields(new List<SelectColumn>(){Helper.CreateColumn("thisCaption", _entityField.Name,
                                             captionAlias)});

                }

            }
        }

        internal class Throw : OnAbsent
        {
            public override void Decorate(SelectStatement source,QuerySpecification query, ISelectQueryBuilder queryBuilder, string captionAlias)
            {
               throw  new PlatformException(String.Format("У сущности '{0}' нет поля которое может выступать как \"Caption\"",queryBuilder.Entity.Name));
            }
        }

        internal  class SelectId : OnAbsent
        {
            private string _prefix;

            public SelectId(string prefix)
            {
                _prefix = prefix;
            }

            internal string Prefix
            {
                get { return _prefix; }
            }

            public override void Decorate(SelectStatement source,QuerySpecification query, ISelectQueryBuilder queryBuilder, string captionAlias)
            {

                var alias = query.GetFirstAliasName(queryBuilder.Entity.Name);
                if (String.IsNullOrEmpty(alias))
                    alias = queryBuilder.AliasName;
                query.SelectElements.Add(
                    new SelectColumn
                        {
                            ColumnName = captionAlias.ToIdentifier(),
                            Expression = Helper.CreateBinaryExpression(
                                _prefix.ToLiteral(),
                                Helper.CreateCast((alias+".id").ToColumn(),
                                                  SqlDataTypeOption.VarChar,100),
                                BinaryExpressionType.Add
                                )
                        });


            }
        }


        #endregion
    }



    public abstract class OnAbsent : AddThisCaption.Strategy
    {

        static public OnAbsent ThrowOnAbsent()
        {
            return new AddThisCaption.Throw();
        }

        static public OnAbsent SelectId(string prefix = "Id: ")
        {
            return new AddThisCaption.SelectId(prefix);
        }

        
    }



}
