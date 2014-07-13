using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Platform.BusinessLogic.AppServices;
using Platform.BusinessLogic.CaptionExpressions;
using Platform.BusinessLogic.Denormalizer.ModelTransformers;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.ReportingServices.PrintForms;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.Dal;
using Platform.Dal.Common.Interfaces;
using Platform.Dal.Common.Interfaces.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring.Extensions;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// Получение модели сущности (Поля, Наименования полей, Связанные поля для фильтрации и т.д.)
    /// </summary>
    public class ModelManager : ManagerBase
    {
        private readonly PrintFormsInfo _reportsInfo;

        /// <summary>
        /// Конструктор по-умолчанию 
        /// </summary>
        public ModelManager()
        {
            DbConnection = IoC.Resolve<SqlConnection>("DbConnection");
            _reportsInfo = IoC.Resolve<PrintFormsInfo>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEntity"></param>
        /// <param name="idOwnerEntity"></param>
        /// <returns></returns>
        public ModelAppResponse GetEntityModel(int idEntity, int? idOwnerEntity)
        {
            Entity entity = Objects.ById<Entity>(idEntity);
            Entity source = getEntityToOpen(entity, idOwnerEntity); // разное поведение при получении формы выбора для а) ссылочного поля б) мультиссылки
            ModelAppResponse result = this.GetModelByEntity(source);

            result.HierarchyFields = _getHierarchyFields(source);

            var tableParts = source.Fields.Where(a => (a.EntityFieldType == EntityFieldType.Tablepart ||
                                                      a.EntityFieldType == EntityFieldType.VirtualTablePart ||
                                                      a.EntityFieldType == EntityFieldType.Multilink) && a.IdEntityLink.HasValue).Select(a => a).ToList();

            result.Actions = IoC.Resolve<ListFormActionsInfo>().GetAction(entity.Name);
            if (tableParts.Count > 0)
            {
                var temp = new List<object>();
                var hasDenormalizedTableparts = false;
                foreach (var tp in tableParts)
                {
// ReSharper disable PossibleInvalidOperationException
                    entity = Objects.ById<Entity>( tp.IdEntityLink.Value );
// ReSharper restore PossibleInvalidOperationException
                    Entity realTp = getEntityToOpen(entity, source.Id);
                    ModelAppResponse tpModel = this.GetModelByEntity(realTp);
                    
                    temp.Add(new
                    {
                        id = tp.Id,
                        model = tpModel
                    });
                    hasDenormalizedTableparts = hasDenormalizedTableparts || tpModel.IsDenormalizedTablepart;
                    tpModel.HierarchyFields = _getHierarchyFields(realTp);
                }
                result.EntityId = source.Id;
                result.TableParts = temp;
                result.HasDenormalizedTableparts = hasDenormalizedTableparts;
            }
            return result;
        }

        /// <summary>
        /// Получает сущность, для которой запросили модель. В случае запроса модели для мультиссылки, возвращает модель "противоположной" сущности.
        /// </summary>
        /// <param name="entity">Сущность, модель которой запросили</param>
        /// <param name="idOwnerEntity">id сущности в которой находится поле, для которого пошел запрос</param>
        /// <returns></returns>
        private Entity getEntityToOpen(Entity entity, int? idOwnerEntity)
        {
            if (!idOwnerEntity.HasValue || entity.EntityType != EntityType.Multilink)
                return entity;

            // Следовательно EntityId - это id мультиссылки, которая находится в сущности OwnerId.
            // Получаем сущность, на которую ссылается мультиссылка.

            if (entity.Fields.Count(f => f.EntityFieldType == EntityFieldType.Link) != 2)
                throw new PlatformException(string.Format("Мультиссылка {0} не содержит двух обязательных ссылочных полей", entity.Name));

            IEntityField field = entity.Fields.Single(f => f.EntityFieldType == EntityFieldType.Link && f.IdEntityLink.HasValue && f.IdEntityLink != idOwnerEntity); // ToDo: желательно это перенести в какой-то ядровой метод
// ReSharper disable PossibleInvalidOperationException
            return Objects.ById<Entity>(field.IdEntityLink.Value);
// ReSharper restore PossibleInvalidOperationException
        }

        /// <summary>
        /// Получает модель сущности
        /// </summary>
        /// <param name="entity">Сущность, для которой требуется получить модель</param>
        /// <param name="withFilters">Включить информацию о фильтрах в результат</param>
        /// <returns>AppResponse.Result - список строк EntityField для указанной сущности</returns>
        private ModelAppResponse GetModelByEntity(Entity entity, bool withFilters = true)
        {
            ModelAnalyzer modelAnalyzer = new ModelAnalyzer(entity);

            Entity source = Objects.ById<Entity>(entity.Id);
            var caption = source.Fields.SingleOrDefault(a => a.IsCaption ?? false);
            var description = source.Fields.SingleOrDefault(a => a.IsDescription ?? false);

            var fields = this.GetModelDataSet(entity); // получили поля сущности
            calculateCaptionsExpressions(fields);

            var context = IoC.Resolve<DbContext>().Cast<DataContext>();
            Dictionary<int, string> importTemplates = context.TemplateImportXLS.Where(w => w.IdEntity == entity.Id && w.IdRefStatus == (byte)PrimaryEntities.DbEnums.RefStatus.Work).Select(s => new
                {
                    s.Id,
                    s.Caption
                }).ToDictionary(s => s.Id, s => s.Caption);

            var appResponse = new ModelAppResponse();
            appResponse.EntityName = source.Name;
            appResponse.CaptionField = caption == null ? "name" : caption.Name.ToLowerInvariant();
            appResponse.DescriptionField = description == null ? "" : description.Name.ToLowerInvariant();
            
            appResponse.Result = fields.Rows;
            if (withFilters)
            {
                AddFieldDependencies(source, appResponse.Result);
            }
            modelAnalyzer.RemoveChildTablefields(appResponse.Result);

            appResponse.Count = fields.Count;
            appResponse.ImportTemplates = importTemplates;
            appResponse.EntityId = entity.Id;
            appResponse.IsDenormalizedTablepart = modelAnalyzer.IsMasterTablepart;
            appResponse.PrintForms = _reportsInfo.GetPrintFormsFor(source.Name);
            return appResponse;
        }

        private void calculateCaptionsExpressions(GridResult result)
        {
            foreach (var row in result.Rows)
            {
                if (!row.ContainsKey("caption"))
                    continue;

                string caption = row["caption"].ToString();
                if (CaptionEvaluator.IsExpression(caption))
                    row["caption"] = CaptionEvaluator.CalculateCaptionExpression(caption);
            }
        }
       
        private List<string> _getHierarchyFields(Entity entity)
        {
            DataContext db = IoC.Resolve<DbContext>().Cast<DataContext>();
            EntitySetting entitySetting = db.EntitySetting.SingleOrDefault(a => a.IdEntity == entity.Id);
            List<IEntityField> listHierarchyFields = entity.Fields.Where(
                a => a.EntityFieldType == EntityFieldType.Link && a.IdEntityLink.HasValue && a.IdEntityLink.Value == a.IdEntity).
                ToList();
            List<string> result = new List<string>();
            if (entitySetting != null && entitySetting.AlwaysShowLinearly)
            {
                return result;
            }
            if (entitySetting != null && entitySetting.IdEntityField_Hierarchy.HasValue)
            {
                result.Add(listHierarchyFields.Single(a => a.Id == entitySetting.IdEntityField_Hierarchy.Value).Name);
                result.AddRange(listHierarchyFields.Where(a => a.Id != entitySetting.IdEntityField_Hierarchy.Value).Select(a => a.Name));
            }
            else
            {
                result.AddRange(listHierarchyFields.Select(a => a.Name));
            }
            return result;
        }

        private List<string> _getHierarchyFieldsOld(int idEntity, List<IDictionary<string, object>> fields)
        {
            var idEntityLink = "idEntityLink".ToLower();
            var name = "Name".ToLower();
            var result = new List<string>();

            foreach (var field in fields)
            {
                if (field[idEntityLink] != DBNull.Value && (int)field[idEntityLink] == idEntity)
                {
                    result.Add((string)field[name]);
                }
            }
            return result;
        }

        private void AddFieldDependencies(Entity source, List<IDictionary<string, object>> rows)
        {
            Dictionary<int, List<string>> fieldDependencies = GetFieldDependenciesByFilter(rows.Select(a => (int)a["id"]));
            fieldDependencies = getFieldDependenciesByCalculatedSqlStatement(source, fieldDependencies);

            
            if (fieldDependencies.Count==0)
				return;
	        foreach (KeyValuePair<int, List<string>> fieldDependency in fieldDependencies)
	        {
		        IDictionary<string, object> field = rows.First(a => (int) a["id"] == fieldDependency.Key);
				field.Add("dependencies", fieldDependency.Value);
	        }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="idEntityFields">Идентификаторы полей, для которых следует получить зависимости</param>
        /// <returns>Словарь вида: ключ - идентификатор поля, значение - список имен полей, от которых зависит данное</returns>
        private Dictionary<int, List<string>> GetFieldDependenciesByFilter(IEnumerable<int> idEntityFields)
        {
            var result = new Dictionary<int, List<string>>();
            Entity filterEntity = Objects.ByName<Entity>("Filter");
            var conditions = new FilterConditions
            {
                Type = LogicOperator.And,
                Operands = new List<IFilterConditions>
				{
					new FilterConditions
					{
						Field = "idEntityField",
						Operator = ComparisionOperator.InList,
						Value = string.Join(",",idEntityFields)
					},
					new FilterConditions
					{
						Type = LogicOperator.Or,
						Operands = new List<IFilterConditions>
						{
							new FilterConditions
							{
								Field = "idRightEntityField",
								Operator = ComparisionOperator.IsNotNull
							},
							new FilterConditions // в фигурных скобках содержатся имена полей
							{
								Field = "RightSqlExpression",
								Operator = ComparisionOperator.Like,
								Value = "%{%}%"
							}
						}
					}
				}
            };
            ISelectQueryBuilder builder = new IoCQueryFactory(filterEntity).Select();

            builder.Conditions = conditions;

            SqlCommand cmd = builder.GetSqlCommand(DbConnection);
            List<Filter> filters;
            using (SqlDataReader reader = cmd.ExecuteReaderLog())
            {
                filters = reader.AsEnumerable<Filter>().ToList();
				reader.Close();
            }
            foreach (var filter in filters)
            {
                if (filter.IdRightEntityField.HasValue)
                {
                    List<string> tmp;
                    if (result.TryGetValue(filter.IdEntityField, out tmp))
                    {
                        result[filter.IdEntityField].AddUnique(filter.RightEntityField.Name);
                    }
                    else
                    {
                        result.Add(filter.IdEntityField, new List<string> { filter.RightEntityField.Name });
                    }
                }
                else if (!string.IsNullOrWhiteSpace(filter.RightSqlExpression))
                {
                    var rx = new Regex(@"\{(\w*?)\}");
                    foreach (Match match in rx.Matches(filter.RightSqlExpression))
                    {
                        if (match.Groups.Count == 2 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                        {
                            List<string> tmp;
                            if (result.TryGetValue(filter.IdEntityField, out tmp))
                            {
                                result[filter.IdEntityField].AddUnique(match.Groups[1].Value);
                            }
                            else
                            {
                                result.Add(filter.IdEntityField, new List<string> { match.Groups[1].Value });
                            }
                        }
                    }
                }
            }

            return result;

        }

        private Dictionary<int, List<string>> getFieldDependenciesByCalculatedSqlStatement(Entity source, Dictionary<int, List<string>> fieldDependencies)
        {
            var fieldsWithSqlComputations =
                source.Fields.Where(f => f.IdFieldDefaultValueType == (int) FieldDefaultValueType.SqlComputed).ToList();

            foreach (var field in fieldsWithSqlComputations)
            {
                if ( String.IsNullOrEmpty(field.DefaultValue) )
                    continue;

                var rx = new Regex(@"@(?<param>[\w\.\[\]]+)");

                foreach (Match match in rx.Matches(field.DefaultValue))
                {
                    if (match.Groups.Count == 2 && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
                    {
                        List<string> tmp;
                        if (fieldDependencies.TryGetValue(field.Id, out tmp))
                        {
                            fieldDependencies[field.Id].AddUnique(match.Groups[1].Value);
                        }
                        else
                        {
                            fieldDependencies.Add(field.Id, new List<string> { match.Groups[1].Value });
                        }
                    }
                }
            }

            return fieldDependencies;
        }

        /// <summary>
        /// Получаем поля сущности
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private GridResult GetModelDataSet(Entity entity)
        {
            var entityField = Objects.ByName<Entity>("EntityField");

            var entityFieldBuilder = new IoCQueryFactory(entityField).Select();
            {
                entityFieldBuilder.Conditions = new FilterConditions
                    {
                                                        Field = "idEntity",
                                                        Value = entity.Id
                                                    };

                return GetDataSet(entityFieldBuilder);
            }
        }

    }
}
