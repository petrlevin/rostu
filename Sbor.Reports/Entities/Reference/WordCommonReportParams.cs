using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BaseApp;
using BaseApp.Reference;
using BaseApp.SystemDimensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.Common;
using Platform.Utils.Common;

namespace Sbor.Reports.Reference
{
    partial class WordCommonReportParams
    {
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before )]
        [ControlInitial(ExcludeFromSetup = true)]
        public void AutoUpdate(DataContext context)
        {
            if (!String.IsNullOrEmpty(SqlQuery))
                return;

            var ids = SysDimensionHelper.SysDimensionTypeIds;

            if (!IdOutputEntity.HasValue || !ids.Contains(IdOutputEntity.Value))
                Controls.Throw("Поле SQL-запрос может быть пустым, только если в качестве сущности входного параметра выбрано одно из системных измерений");
        }

        private DataContext _dc;
        [JsonIgnoreForException]
        public DataContext Context
        {
            get { return _dc ?? (_dc = IoC.Resolve<DbContext>().Cast<DataContext>()); }
        }

        public string GetValue(Dictionary<string, string> additionalParams)
        {
            if (String.IsNullOrEmpty(SqlQuery))
            {
                try
                {
                    var sysDimension = IoC.Resolve<SysDimensionsState>("CurentDimensions");

                    var outputSysDimension =
                        typeof (SysDimensionsState).GetProperty(OutputEntity.Name).GetValue(sysDimension);
                    var outputEntityType = SysDimensionHelper.GetTypeForSysDimension(OutputEntity.Name);

                    var valueFieldName = IdOutputEntityField.HasValue
                                             ? OutputEntityField.Name
                                             : OutputEntity.CaptionField.Name;

                    var value = outputEntityType.GetProperty(valueFieldName).GetValue(outputSysDimension);
                    return value.ToString();
                }
                catch
                {
                    return String.Format("Ошибка вычисления значения маркера: не удалось получить значение из системных измерений");
                }
            }

            var query = InputParams.Any() ? GetQuery(additionalParams) : SqlQuery;

            string result;
            try
            {
                var cmd = Context.GetCommand();
                    cmd.CommandText = query;
                    var temp = (object) cmd.ExecuteScalar();
                result = temp.ToString();
            }catch(Exception ex)
            {
                return String.Format("Ошибка вычисления значения маркера: для маркера {0} указан некорректный Sql-запрос", Caption);
            }
            
            if (OutputEntity != null)
            {
                try
                {
                    var element =
                        Context.Database.SqlQuery<string>(String.Format("Select {0} From [{1}].[{2}] Where id = {3}",
                                                                        IdOutputEntityField.HasValue
                                                                            ? OutputEntityField.Name
                                                                            : OutputEntity.CaptionField.Name
                                                                        , OutputEntity.Schema
                                                                        , OutputEntity.Name
                                                                        , result)).ToList().FirstOrDefault();
                    if (String.IsNullOrEmpty(element))
                        return
                            String.Format(
                                "Ошибка вычисления значения маркера: sql-запрос для маркера {0} вернул значения {1}. Не найден элемент сущности {2}.",
                                Caption, result, OutputEntity.Caption);
                    return element;
                }
                catch (Exception ex)
                {
                    return
                        String.Format(
                            "Ошибка вычисления значения маркера: sql-запрос для маркера {0} вернул значения {1}. При нахождении элемента сущности {2} произошла ошибка. {3}",
                            Caption, result, OutputEntity.Caption, ex.Message);
                }


            }

            return result;
        }

        private string GetQuery(Dictionary<string, string> additionalParams)
        {
            var query = new StringBuilder();
            query.Append(DeclareInputParams());

            foreach (var param in InputParams)
                query.Append(String.Format("Set @{0} = '{1}'{2}", param.Caption
                                                            , additionalParams.ContainsKey(param.Caption) ? additionalParams[param.Caption] : param.DefaultValue ?? String.Empty
                                                            , Environment.NewLine ));
            
            query.Append(SqlQuery);

            return query.ToString();
        }

        private string DeclareInputParams()
        {
            return String.Format("Declare {0};{1}", String.Join(", ", InputParams.Select(p => "@" + p.Caption +" nvarchar(128) ").ToList()), Environment.NewLine);
        }
    }
}
